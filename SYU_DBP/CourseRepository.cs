using System;
using System.Data;
using Oracle.DataAccess.Client;

namespace SYU_DBP
{
    public class CourseRepository
    {
        private readonly DBClass _db;
        public CourseRepository(DBClass db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        private bool ExistsDepartment(string departmentCode)
        {
            if (string.IsNullOrWhiteSpace(departmentCode)) return true; // nullable 허용
            const string sql = "SELECT COUNT(*) FROM Department WHERE department_code = :code";
            var cnt = Convert.ToInt32(_db.ExecuteScalar(sql, new OracleParameter("code", departmentCode)) ?? 0);
            return cnt > 0;
        }
        private bool ExistsProfessor(string professorId)
        {
            if (string.IsNullOrWhiteSpace(professorId)) return true; // nullable 허용
            const string sql = "SELECT COUNT(*) FROM Professor WHERE professor_id = :pid";
            var cnt = Convert.ToInt32(_db.ExecuteScalar(sql, new OracleParameter("pid", professorId)) ?? 0);
            return cnt > 0;
        }

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

            _db.Open();
            try
            {
                using (var cmd = _db.Connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.BindByName = true;

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
                if (ex.Number == 1) return false; // ORA-00001
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
                            cmd.BindByName = true;
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
                            cmd.BindByName = true;
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
                            cmd.BindByName = true;
                            cmd.CommandText = @"UPDATE EnrolledCourse 
                                                   SET credits = :cr 
                                                 WHERE student_id = :sid 
                                                   AND course_number = :cno 
                                                   AND semester = :sem";

                            cmd.Parameters.Add(new OracleParameter("cr", newCredits.Value));
                            cmd.Parameters.Add(new OracleParameter("sid", studentId));
                            cmd.Parameters.Add(new OracleParameter("cno", courseNumber));
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
                    throw;
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

        // ======== 과목정보 CRUD ========
        public DataTable GetCourses()
        {
            const string sql = @"SELECT course_number, course_code, course_name, credit, course_type,
                                         department_code, grade_level, professor_id, general_area
                                   FROM Course ORDER BY course_number";
            return _db.GetDataTable(sql);
        }

        // 과목코드로 과목 목록 필터 조회
        public DataTable GetCoursesByCode(string courseCode)
        {
            const string sql = @"SELECT course_number, course_code, course_name, credit, course_type,
                                         department_code, grade_level, professor_id, general_area
                                   FROM Course WHERE course_code = :code ORDER BY course_number";
            return _db.GetDataTable(sql, new OracleParameter("code", courseCode));
        }

        public bool InsertCourse(string courseNumber, string courseCode, string courseName, int? credit,
                                 string courseType = null, string departmentCode = null, int? gradeLevel = null,
                                 string professorId = null, string generalArea = null)
        {
            if (!ExistsDepartment(departmentCode)) return false;
            if (!ExistsProfessor(professorId)) return false;

            const string sql = @"INSERT INTO Course(course_number, course_code, course_name, credit, course_type,
                                                    department_code, grade_level, professor_id, general_area)
                                 VALUES(:num, :code, :name, :credit, :ctype, :dept, :grade, :prof, :garea)";
            int affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("num", courseNumber),
                new OracleParameter("code", (object)courseCode ?? DBNull.Value),
                new OracleParameter("name", (object)courseName ?? DBNull.Value),
                new OracleParameter("credit", (object)credit ?? DBNull.Value),
                new OracleParameter("ctype", (object)courseType ?? DBNull.Value),
                new OracleParameter("dept", (object)departmentCode ?? DBNull.Value),
                new OracleParameter("grade", (object)gradeLevel ?? DBNull.Value),
                new OracleParameter("prof", (object)professorId ?? DBNull.Value),
                new OracleParameter("garea", (object)generalArea ?? DBNull.Value));
            return affected > 0;
        }

        public bool UpdateCourse(string courseNumber, string courseCode = null, string courseName = null, int? credit = null,
                                 string courseType = null, string departmentCode = null, int? gradeLevel = null,
                                 string professorId = null, string generalArea = null)
        {
            if (departmentCode != null && !ExistsDepartment(departmentCode)) return false;
            if (professorId != null && !ExistsProfessor(professorId)) return false;

            var parts = new System.Collections.Generic.List<string>();
            var prms = new System.Collections.Generic.List<OracleParameter>();
            if (courseCode != null) { parts.Add("course_code = :code"); prms.Add(new OracleParameter("code", courseCode)); }
            if (courseName != null) { parts.Add("course_name = :name"); prms.Add(new OracleParameter("name", courseName)); }
            if (credit.HasValue) { parts.Add("credit = :credit"); prms.Add(new OracleParameter("credit", credit.Value)); }
            if (courseType != null) { parts.Add("course_type = :ctype"); prms.Add(new OracleParameter("ctype", courseType)); }
            if (departmentCode != null) { parts.Add("department_code = :dept"); prms.Add(new OracleParameter("dept", departmentCode)); }
            if (gradeLevel.HasValue) { parts.Add("grade_level = :grade"); prms.Add(new OracleParameter("grade", gradeLevel.Value)); }
            if (professorId != null) { parts.Add("professor_id = :prof"); prms.Add(new OracleParameter("prof", professorId)); }
            if (generalArea != null) { parts.Add("general_area = :garea"); prms.Add(new OracleParameter("garea", generalArea)); }
            if (parts.Count == 0) return false;

            string sql = "UPDATE Course SET " + string.Join(", ", parts) + " WHERE course_number = :num";
            prms.Add(new OracleParameter("num", courseNumber));
            int affected = _db.ExecuteNonQuery(sql, prms.ToArray());
            return affected > 0;
        }

        public bool DeleteCourse(string courseNumber)
        {
            _db.Open();
            using (var tx = _db.Connection.BeginTransaction())
            {
                try
                {
                    using (var delChild = _db.Connection.CreateCommand())
                    {
                        delChild.Transaction = tx;
                        delChild.CommandText = "DELETE FROM EnrolledCourse WHERE course_number = :num";
                        delChild.Parameters.Add(new OracleParameter("num", courseNumber));
                        delChild.ExecuteNonQuery();
                    }

                    using (var delCourse = _db.Connection.CreateCommand())
                    {
                        delCourse.Transaction = tx;
                        delCourse.CommandText = "DELETE FROM Course WHERE course_number = :num";
                        delCourse.Parameters.Add(new OracleParameter("num", courseNumber));
                        int affected = delCourse.ExecuteNonQuery();
                        tx.Commit();
                        return affected > 0;
                    }
                }
                catch (OracleException ex)
                {
                    try { tx.Rollback(); } catch { }
                    if (ex.Number == 2292)
                    {
                        throw new InvalidOperationException("해당 과목을 참조하는 데이터가 있어 삭제할 수 없습니다.");
                    }
                    throw;
                }
            }
        }
    }
}
