using System;
using Oracle.DataAccess.Client;

namespace SYU_DBP
{
    internal class LoginRepository
    {
        private readonly DBClass _db;
        public LoginRepository(DBClass db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        public bool ValidateStudentLogin(string studentId, string password)
        {
            const string sql = "SELECT COUNT(*) FROM PersonalInfo WHERE student_id = :sid AND password = :pwd";
            var count = Convert.ToInt32(_db.ExecuteScalar(sql,
                new OracleParameter("sid", studentId),
                new OracleParameter("pwd", password)));
            return count > 0;
        }

        public bool ValidateProfessorLogin(string professorId, string password)
        {
            const string sql = "SELECT COUNT(*) FROM Professor WHERE professor_id = :pid AND password = :pwd";
            var count = Convert.ToInt32(_db.ExecuteScalar(sql,
                new OracleParameter("pid", professorId),
                new OracleParameter("pwd", password)));
            return count > 0;
        }
    }
}
