using Oracle.DataAccess.Client;
using System;
using System.Data;

namespace SYU_DBP
{
    public class GraduationRequirementRepository
    {
        private readonly DBClass _db;
        public GraduationRequirementRepository(DBClass db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        public DataTable GetAll()
        {
            const string sql = @"SELECT graduation_id, admission_year, department_code,
                                        earned_credits, general_credits, major_credits,
                                        material_completion_count, required_general_details,
                                        chapel_completion_count
                                   FROM GraduationRequirement ORDER BY admission_year, department_code";
            return _db.GetDataTable(sql);
        }

        public DataRow GetById(string graduationId)
        {
            const string sql = @"SELECT graduation_id, admission_year, department_code,
                                        earned_credits, general_credits, major_credits,
                                        material_completion_count, required_general_details,
                                        chapel_completion_count
                                   FROM GraduationRequirement WHERE graduation_id = :gid";
            var dt = _db.GetDataTable(sql, new OracleParameter("gid", graduationId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public bool Insert(string graduationId, int admissionYear, string departmentCode,
                           int? earnedCredits = null, int? generalCredits = null, int? majorCredits = null,
                           int? materialCompletionCount = null, string requiredGeneralDetails = null,
                           int? chapelCompletionCount = null)
        {
            const string sql = @"INSERT INTO GraduationRequirement(
                                    graduation_id, admission_year, department_code,
                                    earned_credits, general_credits, major_credits,
                                    material_completion_count, required_general_details,
                                    chapel_completion_count)
                                 VALUES(:gid, :ay, :dept, :earned, :general, :major, :mat, :details, :chapel)";
            int affected = _db.ExecuteNonQuery(sql,
                new OracleParameter("gid", graduationId),
                new OracleParameter("ay", admissionYear),
                new OracleParameter("dept", departmentCode),
                new OracleParameter("earned", (object)earnedCredits ?? DBNull.Value),
                new OracleParameter("general", (object)generalCredits ?? DBNull.Value),
                new OracleParameter("major", (object)majorCredits ?? DBNull.Value),
                new OracleParameter("mat", (object)materialCompletionCount ?? DBNull.Value),
                new OracleParameter("details", (object)requiredGeneralDetails ?? DBNull.Value),
                new OracleParameter("chapel", (object)chapelCompletionCount ?? DBNull.Value));
            return affected > 0;
        }

        public bool Update(string graduationId, int? admissionYear = null, string departmentCode = null,
                           int? earnedCredits = null, int? generalCredits = null, int? majorCredits = null,
                           int? materialCompletionCount = null, string requiredGeneralDetails = null,
                           int? chapelCompletionCount = null)
        {
            var parts = new System.Collections.Generic.List<string>();
            var prms = new System.Collections.Generic.List<OracleParameter>();
            if (admissionYear.HasValue) { parts.Add("admission_year = :ay"); prms.Add(new OracleParameter("ay", admissionYear.Value)); }
            if (departmentCode != null) { parts.Add("department_code = :dept"); prms.Add(new OracleParameter("dept", departmentCode)); }
            if (earnedCredits.HasValue) { parts.Add("earned_credits = :earned"); prms.Add(new OracleParameter("earned", earnedCredits.Value)); }
            if (generalCredits.HasValue) { parts.Add("general_credits = :general"); prms.Add(new OracleParameter("general", generalCredits.Value)); }
            if (majorCredits.HasValue) { parts.Add("major_credits = :major"); prms.Add(new OracleParameter("major", majorCredits.Value)); }
            if (materialCompletionCount.HasValue) { parts.Add("material_completion_count = :mat"); prms.Add(new OracleParameter("mat", materialCompletionCount.Value)); }
            if (requiredGeneralDetails != null) { parts.Add("required_general_details = :details"); prms.Add(new OracleParameter("details", requiredGeneralDetails)); }
            if (chapelCompletionCount.HasValue) { parts.Add("chapel_completion_count = :chapel"); prms.Add(new OracleParameter("chapel", chapelCompletionCount.Value)); }
            if (parts.Count == 0) return false;

            string sql = "UPDATE GraduationRequirement SET " + string.Join(", ", parts) + " WHERE graduation_id = :gid";
            prms.Add(new OracleParameter("gid", graduationId));
            int affected = _db.ExecuteNonQuery(sql, prms.ToArray());
            return affected > 0;
        }

        public bool Delete(string graduationId)
        {
            const string sql = "DELETE FROM GraduationRequirement WHERE graduation_id = :gid";
            int affected = _db.ExecuteNonQuery(sql, new OracleParameter("gid", graduationId));
            return affected > 0;
        }
    }
}
