using System;
using System.Data;
using Oracle.DataAccess.Client;

namespace SYU_DBP
{
    public class Grade_ScoreReposittory
    {
        private readonly DBClass _db;
        public Grade_ScoreReposittory(DBClass db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        // 성적 목록 조회 (폼의 리스트 컬럼 순서에 맞는 컬럼 명을 반환)
        public DataTable GetGrades()
        {
            const string sql = @"
                SELECT 
                    G.student_id,
                    C.course_name,
                    G.credits,
                    G.course_number,
                    G.grade_point,
                    G.semester
                  FROM Grade G
                  LEFT JOIN Course C ON C.course_number = G.course_number
                 ORDER BY G.student_id, G.semester, G.course_number";
            return _db.GetDataTable(sql);
        }

        // 학번으로 성적 목록 조회
        public DataTable GetGradesByStudent(string studentId)
        {
            const string sql = @"
                SELECT 
                    G.student_id,
                    C.course_name,
                    G.credits,
                    G.course_number,
                    G.grade_point,
                    G.semester
                  FROM Grade G
                  LEFT JOIN Course C ON C.course_number = G.course_number
                 WHERE G.student_id = :sid
                 ORDER BY G.semester, G.course_number";
            return _db.GetDataTable(sql, new OracleParameter("sid", studentId));
        }

        // 성적 추가
        public bool InsertGrade(string studentId, string semester, string courseNumber, int credits, decimal gradePoint, string academicWarning = null)
        {
            const string sql = @"INSERT INTO Grade(student_id, semester, course_number, credits, grade_point, academic_warning)
                                 VALUES(:sid, :sem, :cno, :cr, :gp, :aw)";
            int affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("sid", studentId),
                new OracleParameter("sem", semester),
                new OracleParameter("cno", courseNumber),
                new OracleParameter("cr", credits),
                new OracleParameter("gp", gradePoint),
                new OracleParameter("aw", (object)academicWarning ?? DBNull.Value));
            return affected > 0;
        }

        // 성적 수정 (선택 필드만 수정)
        public bool UpdateGrade(string studentId, string semester, string courseNumber, int? credits = null, decimal? gradePoint = null, string academicWarning = null)
        {
            var parts = new System.Collections.Generic.List<string>();
            var prms = new System.Collections.Generic.List<OracleParameter>();
            if (credits.HasValue) { parts.Add("credits = :cr"); prms.Add(new OracleParameter("cr", credits.Value)); }
            if (gradePoint.HasValue) { parts.Add("grade_point = :gp"); prms.Add(new OracleParameter("gp", gradePoint.Value)); }
            if (academicWarning != null) { parts.Add("academic_warning = :aw"); prms.Add(new OracleParameter("aw", academicWarning)); }
            if (parts.Count == 0) return false;

            string sql = "UPDATE Grade SET " + string.Join(", ", parts) + " WHERE student_id = :sid AND semester = :sem AND course_number = :cno";
            prms.Add(new OracleParameter("sid", studentId));
            prms.Add(new OracleParameter("sem", semester));
            prms.Add(new OracleParameter("cno", courseNumber));
            int affected = _db.ExecuteNonQuery(sql, prms.ToArray());
            return affected > 0;
        }

        // 성적 삭제
        public bool DeleteGrade(string studentId, string semester, string courseNumber)
        {
            const string sql = "DELETE FROM Grade WHERE student_id = :sid AND semester = :sem AND course_number = :cno";
            int affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("sid", studentId),
                new OracleParameter("sem", semester),
                new OracleParameter("cno", courseNumber));
            return affected > 0;
        }
    }
}
