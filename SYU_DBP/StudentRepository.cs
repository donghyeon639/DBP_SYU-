using System;
using System.Data;
using Oracle.DataAccess.Client;

namespace SYU_DBP
{
    public class StudentRepository
    {
        private readonly DBClass _db;
        public StudentRepository(DBClass db) { _db = db ?? throw new ArgumentNullException(nameof(db)); }

        public DataTable GetStudents()
        {
            const string sql = @"SELECT s.student_id, s.name, s.department_code,
                                        (SELECT d.department_name FROM Department d WHERE d.department_code = s.department_code) AS department_name,
                                        s.grade, s.major, p.phone_number, p.birth_date, p.address
                                   FROM Student s LEFT JOIN PersonalInfo p ON s.student_id = p.student_id ORDER BY s.student_id";
            return _db.GetDataTable(sql);
        }

        // 학번으로 학생 조회 (그리드 검색용)
        public DataTable GetStudentsById(string studentId)
        {
            const string sql = @"SELECT s.student_id, s.name, s.department_code,
                                        (SELECT d.department_name FROM Department d WHERE d.department_code = s.department_code) AS department_name,
                                        s.grade, s.major, p.phone_number, p.birth_date, p.address
                                   FROM Student s LEFT JOIN PersonalInfo p ON s.student_id = p.student_id
                                  WHERE s.student_id = :sid ORDER BY s.student_id";
            return _db.GetDataTable(sql, new OracleParameter("sid", studentId));
        }

        private string GetDepartmentCodeByName(string departmentName)
        {
            const string sql = "SELECT department_code FROM Department WHERE department_name = :dname";
            var result = _db.ExecuteScalar(sql, new OracleParameter("dname", departmentName));
            return result == null ? null : result.ToString();
        }

        private string ResolveDepartmentCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var byName = GetDepartmentCodeByName(input);
            if (!string.IsNullOrEmpty(byName)) return byName;
            const string sql = "SELECT COUNT(*) FROM Department WHERE department_code = :code";
            var count = Convert.ToInt32(_db.ExecuteScalar(sql, new OracleParameter("code", input)));
            return count > 0 ? input : null;
        }

        public bool InsertStudent(string studentId, string name, string departmentInput, int grade, string phone, DateTime? birthDate, string address)
        {
            var deptCode = ResolveDepartmentCode(departmentInput);
            if (string.IsNullOrEmpty(deptCode)) return false;

            _db.Open();
            using (var tx = _db.Connection.BeginTransaction())
            {
                try
                {
                    using (var sCmd = _db.Connection.CreateCommand())
                    {
                        sCmd.Transaction = tx;
                        sCmd.CommandText = "INSERT INTO Student(student_id, name, academic_status, department_code, grade, major) VALUES(:sid, :name, :status, :dept, :grade, :major)";
                        sCmd.Parameters.Add(new OracleParameter("sid", studentId));
                        sCmd.Parameters.Add(new OracleParameter("name", name));
                        sCmd.Parameters.Add(new OracleParameter("status", "재학"));
                        sCmd.Parameters.Add(new OracleParameter("dept", deptCode));
                        sCmd.Parameters.Add(new OracleParameter("grade", grade));
                        sCmd.Parameters.Add(new OracleParameter("major", departmentInput));
                        sCmd.ExecuteNonQuery();
                    }

                    using (var pCmd = _db.Connection.CreateCommand())
                    {
                        pCmd.Transaction = tx;
                        pCmd.CommandText = "INSERT INTO PersonalInfo(student_id, email, address, phone_number, birth_date, password) VALUES(:sid, :email, :addr, :phone, :birth, :pwd)";
                        pCmd.Parameters.Add(new OracleParameter("sid", studentId));
                        pCmd.Parameters.Add(new OracleParameter("email", DBNull.Value));
                        pCmd.Parameters.Add(new OracleParameter("addr", (object)address ?? DBNull.Value));
                        pCmd.Parameters.Add(new OracleParameter("phone", (object)phone ?? DBNull.Value));
                        pCmd.Parameters.Add(new OracleParameter("birth", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value));
                        pCmd.Parameters.Add(new OracleParameter("pwd", studentId));
                        pCmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return true;
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public bool UpdateStudent(string studentId, string name, string departmentInput, int grade, string phone, DateTime? birthDate, string address)
        {
            var deptCode = ResolveDepartmentCode(departmentInput);
            if (string.IsNullOrEmpty(deptCode)) return false;

            _db.Open();
            using (var tx = _db.Connection.BeginTransaction())
            {
                try
                {
                    using (var sCmd = _db.Connection.CreateCommand())
                    {
                        sCmd.Transaction = tx;
                        sCmd.CommandText = "UPDATE Student SET name = :name, department_code = :dept, grade = :grade, major = :major WHERE student_id = :sid";
                        sCmd.Parameters.Add(new OracleParameter("name", name));
                        sCmd.Parameters.Add(new OracleParameter("dept", deptCode));
                        sCmd.Parameters.Add(new OracleParameter("grade", grade));
                        sCmd.Parameters.Add(new OracleParameter("major", departmentInput));
                        sCmd.Parameters.Add(new OracleParameter("sid", studentId));
                        sCmd.ExecuteNonQuery();
                    }

                    using (var pCmd = _db.Connection.CreateCommand())
                    {
                        pCmd.Transaction = tx;
                        pCmd.CommandText = "UPDATE PersonalInfo SET address = :addr, phone_number = :phone, birth_date = :birth WHERE student_id = :sid";
                        pCmd.Parameters.Add(new OracleParameter("addr", (object)address ?? DBNull.Value));
                        pCmd.Parameters.Add(new OracleParameter("phone", (object)phone ?? DBNull.Value));
                        pCmd.Parameters.Add(new OracleParameter("birth", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value));
                        pCmd.Parameters.Add(new OracleParameter("sid", studentId));
                        pCmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return true;
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public bool DeleteStudent(string studentId)
        {
            _db.Open();
            using (var tx = _db.Connection.BeginTransaction())
            {
                try
                {
                    using (var pCmd = _db.Connection.CreateCommand())
                    {
                        pCmd.Transaction = tx;
                        pCmd.CommandText = "DELETE FROM PersonalInfo WHERE student_id = :sid";
                        pCmd.Parameters.Add(new OracleParameter("sid", studentId));
                        pCmd.ExecuteNonQuery();
                    }

                    using (var sCmd = _db.Connection.CreateCommand())
                    {
                        sCmd.Transaction = tx;
                        sCmd.CommandText = "DELETE FROM Student WHERE student_id = :sid";
                        sCmd.Parameters.Add(new OracleParameter("sid", studentId));
                        sCmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return true;
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }
    }
}
