using System;
using System.Data;
using Oracle.DataAccess.Client;

namespace SYU_DBP
{
    public class CourseRepository
    {
        private readonly DBClass _db;
        public CourseRepository(DBClass db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        // 전체 수강 목록
        public DataTable GetEnrolledCourses()
        {
            const string sql = @"
                SELECT 
                    E.student_id,
                    S.name AS student_name,
                    E.course_number,
                    C.course_code,
                    C.course_name,
                    E.semester,
                    E.credits,
                    C.credit AS course_credit
                  FROM EnrolledCourse E
                  JOIN Student S ON E.student_id = S.student_id
                  JOIN Course C ON E.course_number = C.course_number
                 ORDER BY E.student_id, E.course_number, E.semester";
            return _db.GetDataTable(sql);
        }

        // 특정 학생 수강 목록
        public DataTable GetEnrolledCoursesByStudent(string studentId)
        {
            const string sql = @"
                SELECT 
                    E.student_id,
                    S.name AS student_name,
                    E.course_number,
                    C.course_code,
                    C.course_name,
                    E.semester,
                    E.credits,
                    C.credit AS course_credit
                  FROM EnrolledCourse E
                  JOIN Student S ON E.student_id = S.student_id
                  JOIN Course C ON E.course_number = C.course_number
                 WHERE E.student_id = :sid
                 ORDER BY E.course_number, E.semester";
            return _db.GetDataTable(sql, new OracleParameter("sid", studentId));
        }

        // 수강 추가
        public bool InsertEnrollment(string studentId, string courseNumber, string semester, int credits)
        {
            const string sql = "INSERT INTO EnrolledCourse(student_id, course_number, semester, credits) VALUES(:sid, :cno, :sem, :cr)";
            var affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("sid", studentId),
                new OracleParameter("cno", courseNumber),
                new OracleParameter("sem", semester),
                new OracleParameter("cr", credits));
            return affected > 0;
        }

        public bool UpdateEnrollmentSemester(string studentId, string courseNumber, string oldSemester, string newSemester)
        {
            const string sql = @"
                UPDATE EnrolledCourse 
                   SET semester = :newSem 
                 WHERE student_id = :sid 
                   AND course_number = :cno 
                   AND semester = :oldSem";

            // DBClass의 ExecuteNonQuery가 BindByName을 지원하지 않을 수 있으므로
            // 직접 커맨드를 생성하여 처리하거나 파라미터 순서를 완벽하게 맞춰야 합니다.
            // 여기서는 안전하게 직접 커맨드를 여는 방식을 권장합니다.

            _db.Open();
            try
            {
                using (var cmd = _db.Connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.BindByName = true; // [중요] 파라미터 이름으로 매핑하도록 강제 설정

                    cmd.Parameters.Add(new OracleParameter("newSem", newSemester));
                    cmd.Parameters.Add(new OracleParameter("sid", studentId));
                    cmd.Parameters.Add(new OracleParameter("cno", courseNumber));
                    cmd.Parameters.Add(new OracleParameter("oldSem", oldSemester));

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
            catch (OracleException ex)
            {
                // ORA-00001: 무결성 제약 조건 위반 (이미 해당 학기에 수강 내역이 존재할 경우)
                if (ex.Number == 1) return false;
                throw;
            }
        }

        // 부분 수정 (과목명/학점)
        public bool UpdateEnrollmentPartial(string studentId, string courseNumber, string currentSemester,
                                            string newCourseName = null, string newSemester = null, int? newCredits = null)
        {
            if (newCourseName == null && newSemester == null && newCredits == null) return false;

            _db.Open();
            using (var tx = _db.Connection.BeginTransaction())
            {
                try
                {
                    int totalAffected = 0;

                    if (newCourseName != null)
                    {
                        using (var cmd = _db.Connection.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.BindByName = true; // [핵심]
                            cmd.CommandText = "UPDATE Course SET course_name = :name WHERE course_number = :cno";
                            cmd.Parameters.Add(new OracleParameter("name", newCourseName));
                            cmd.Parameters.Add(new OracleParameter("cno", courseNumber));
                            totalAffected += cmd.ExecuteNonQuery();
                        }
                    }

                    bool semesterChanged = false;
                    if (newSemester != null && newSemester != currentSemester)
                    {
                        using (var cmd = _db.Connection.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.BindByName = true; // [핵심]
                            cmd.CommandText = @"UPDATE EnrolledCourse 
                                                   SET semester = :newSem 
                                                 WHERE student_id = :sid 
                                                   AND course_number = :cno 
                                                   AND semester = :oldSem";

                            cmd.Parameters.Add(new OracleParameter("newSem", newSemester));
                            cmd.Parameters.Add(new OracleParameter("sid", studentId));
                            cmd.Parameters.Add(new OracleParameter("cno", courseNumber));
                            cmd.Parameters.Add(new OracleParameter("oldSem", currentSemester));

                            int result = cmd.ExecuteNonQuery();
                            if (result > 0)
                            {
                                semesterChanged = true;
                                totalAffected += result;
                            }
                        }
                    }

                    if (newCredits.HasValue)
                    {
                        using (var cmd = _db.Connection.CreateCommand())
                        {
                            cmd.Transaction = tx;
                            cmd.BindByName = true; // [핵심]
                            cmd.CommandText = @"UPDATE EnrolledCourse 
                                                   SET credits = :cr 
                                                 WHERE student_id = :sid 
                                                   AND course_number = :cno 
                                                   AND semester = :sem"; // 변경된 학기를 기준으로 찾아야 함

                            cmd.Parameters.Add(new OracleParameter("cr", newCredits.Value));
                            cmd.Parameters.Add(new OracleParameter("sid", studentId));
                            cmd.Parameters.Add(new OracleParameter("cno", courseNumber));

                            // [중요 로직] 
                            // 학기가 변경 성공했다면 -> newSemester를 기준으로 검색
                            // 학기가 변경 안됐거나(null) 실패했다면 -> currentSemester 기준으로 검색
                            string targetSemester = semesterChanged ? newSemester : currentSemester;
                            cmd.Parameters.Add(new OracleParameter("sem", targetSemester));

                            totalAffected += cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                    return totalAffected > 0;
                }
                catch (Exception)
                {
                    tx.Rollback();
                    throw; // 에러를 상위로 던져서 메시지 확인 필요 (ORA-00001 등)
                }
            }
        }

        // 과목코드만 수정
        public bool UpdateCourseCode(string courseNumber, string newCourseCode)
        {
            const string sql = "UPDATE Course SET course_code = :code WHERE course_number = :cno";
            var affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("code", newCourseCode),
                new OracleParameter("cno", courseNumber));
            return affected > 0;
        }

        // 수강 삭제
        public bool DeleteEnrollment(string studentId, string courseNumber, string semester)
        {
            const string sql = "DELETE FROM EnrolledCourse WHERE student_id = :sid AND course_number = :cno AND semester = :sem";
            var affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("sid", studentId),
                new OracleParameter("cno", courseNumber),
                new OracleParameter("sem", semester));
            return affected > 0;
        }
    }
}
