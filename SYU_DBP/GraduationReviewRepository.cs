using Oracle.DataAccess.Client;
using System;
using System.Data;

namespace SYU_DBP
{
    public class GraduationReviewRepository
    {
        private readonly DBClass _db;
        public GraduationReviewRepository(DBClass db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        // 목록 조회 (테이블 컬럼 순서에 맞게 반환)
        public DataTable GetAll()
        {
            const string sql = @"SELECT review_id, student_id, review_schedule, review_result, reviewer,
                                        course_number, semester, admission_year, department_code
                                   FROM GraduationReview ORDER BY review_id";
            return _db.GetDataTable(sql);
        }

        public DataRow GetById(string reviewId)
        {
            const string sql = @"SELECT review_id, student_id, review_schedule, review_result, reviewer,
                                        course_number, semester, admission_year, department_code
                                   FROM GraduationReview WHERE review_id = :rid";
            var dt = _db.GetDataTable(sql, new OracleParameter("rid", reviewId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public bool Insert(string reviewId, string studentId, DateTime? reviewSchedule,
                           string reviewResult, string reviewer, string courseNumber,
                           string semester, int? admissionYear, string departmentCode)
        {
            const string sql = @"INSERT INTO GraduationReview(
                                    review_id, student_id, review_schedule, review_result, reviewer,
                                    course_number, semester, admission_year, department_code)
                                 VALUES(:rid, :sid, :rs, :rr, :rev,
                                        :cno, :sem, :ay, :dept)";
            int affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("rid", reviewId),
                new OracleParameter("sid", studentId),
                new OracleParameter("rs", (object)reviewSchedule ?? DBNull.Value),
                new OracleParameter("rr", (object)reviewResult ?? DBNull.Value),
                new OracleParameter("rev", (object)reviewer ?? DBNull.Value),
                new OracleParameter("cno", (object)courseNumber ?? DBNull.Value),
                new OracleParameter("sem", (object)semester ?? DBNull.Value),
                new OracleParameter("ay", (object)admissionYear ?? DBNull.Value),
                new OracleParameter("dept", (object)departmentCode ?? DBNull.Value));
            return affected > 0;
        }

        public bool Update(string reviewId, string studentId = null, DateTime? reviewSchedule = null,
                           string reviewResult = null, string reviewer = null, string courseNumber = null,
                           string semester = null, int? admissionYear = null, string departmentCode = null)
        {
            var parts = new System.Collections.Generic.List<string>();
            var prms = new System.Collections.Generic.List<OracleParameter>();
            if (studentId != null) { parts.Add("student_id = :sid"); prms.Add(new OracleParameter("sid", studentId)); }
            if (reviewSchedule.HasValue) { parts.Add("review_schedule = :rs"); prms.Add(new OracleParameter("rs", reviewSchedule.Value)); }
            if (reviewResult != null) { parts.Add("review_result = :rr"); prms.Add(new OracleParameter("rr", reviewResult)); }
            if (reviewer != null) { parts.Add("reviewer = :rev"); prms.Add(new OracleParameter("rev", reviewer)); }
            if (courseNumber != null) { parts.Add("course_number = :cno"); prms.Add(new OracleParameter("cno", courseNumber)); }
            if (semester != null) { parts.Add("semester = :sem"); prms.Add(new OracleParameter("sem", semester)); }
            if (admissionYear.HasValue) { parts.Add("admission_year = :ay"); prms.Add(new OracleParameter("ay", admissionYear.Value)); }
            if (departmentCode != null) { parts.Add("department_code = :dept"); prms.Add(new OracleParameter("dept", departmentCode)); }
            if (parts.Count == 0) return false;
            string sql = "UPDATE GraduationReview SET " + string.Join(", ", parts) + " WHERE review_id = :rid";
            prms.Add(new OracleParameter("rid", reviewId));
            int affected = _db.ExecuteNonQuery(sql, prms.ToArray());
            return affected > 0;
        }

        public bool Delete(string reviewId)
        {
            const string sql = "DELETE FROM GraduationReview WHERE review_id = :rid";
            int affected = _db.ExecuteNonQuery(sql, new OracleParameter("rid", reviewId));
            return affected > 0;
        }
    }
}
