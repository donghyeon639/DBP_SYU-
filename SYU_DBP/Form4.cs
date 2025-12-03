using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace SYU_DBP
{
    public partial class Form4 : Form
    {
        private string _studentId;

        // 기존 생성자 (하위 호환성 유지)
        public Form4(string userId)
        {
            InitializeComponent();
            _studentId = userId;
            LoadStudentInfo(); // 학생 정보 자동 로드
        }

        // 새로운 생성자 - 학생 정보를 직접 받아서 표시
        public Form4(string userId, string address, string phone, string email) : this(userId)
        {
            // 전달받은 정보를 텍스트박스에 채우기
            AddresstxtBox.Text = address ?? string.Empty;
            PhonetxtBox.Text = phone ?? string.Empty;
            EmailtxtBox.Text = email ?? string.Empty;
            // 비밀번호는 보안상 빈 값으로 유지
            PasswordtxtBox.Text = string.Empty;
        }

        // 학생 정보를 DB에서 로드하는 메서드
        private void LoadStudentInfo()
        {
            try
            {
                using (var db = new DBClass("syu", "1111"))
                {
                    string query = @"
                        SELECT ADDRESS, PHONE_NUMBER, EMAIL 
                        FROM PersonalInfo 
                        WHERE STUDENT_ID = :sid";

                    OracleParameter pSid = new OracleParameter("sid", _studentId);
                    DataTable dt = db.GetDataTable(query, pSid);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        AddresstxtBox.Text = row["ADDRESS"]?.ToString() ?? string.Empty;
                        PhonetxtBox.Text = row["PHONE_NUMBER"]?.ToString() ?? string.Empty;
                        EmailtxtBox.Text = row["EMAIL"]?.ToString() ?? string.Empty;
                        // 비밀번호는 보안상 빈 값으로 유지
                        PasswordtxtBox.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("학생 정보 로드 중 오류 발생: " + ex.Message, "오류");
            }
        }

        private void UpdatePersonalInfo_Click(object sender, EventArgs e)
        {
            using (var db = new DBClass("syu", "1111"))
            {
                string Address = AddresstxtBox.Text.Trim();
                string Phone = PhonetxtBox.Text.Trim();
                string Email = EmailtxtBox.Text.Trim();
                string Password = PasswordtxtBox.Text.Trim();

                // 비밀번호가 입력되지 않은 경우 비밀번호는 업데이트하지 않음
                string updateQuery;
                OracleParameter[] parameters;

                if (string.IsNullOrEmpty(Password))
                {
                    // 비밀번호 제외하고 업데이트
                    updateQuery = @"
                        UPDATE PersonalInfo 
                        SET ADDRESS = :address, PHONE_NUMBER = :phone, EMAIL = :email 
                        WHERE STUDENT_ID = :sid";

                    parameters = new OracleParameter[]
                    {
                        new OracleParameter("address", Address),
                        new OracleParameter("phone", Phone),
                        new OracleParameter("email", Email),
                        new OracleParameter("sid", _studentId)
                    };
                }
                else
                {
                    // 비밀번호 포함하여 업데이트
                    updateQuery = @"
                        UPDATE PersonalInfo 
                        SET PASSWORD = :pwd, ADDRESS = :address, PHONE_NUMBER = :phone, EMAIL = :email 
                        WHERE STUDENT_ID = :sid";

                    parameters = new OracleParameter[]
                    {
                        new OracleParameter("pwd", Password),
                        new OracleParameter("address", Address),
                        new OracleParameter("phone", Phone),
                        new OracleParameter("email", Email),
                        new OracleParameter("sid", _studentId)
                    };
                }

                int rowsAffected = db.ExecuteNonQuery(updateQuery, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("신상정보가 성공적으로 변경되었습니다!");
                    this.Close(); // 수정 완료 후 폼 닫기
                }
                else
                {
                    MessageBox.Show("해당 학번을 찾을 수 없습니다.");
                }
            }
        }
    }
}


