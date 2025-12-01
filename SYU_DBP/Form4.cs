
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

        public Form4(string userId)
        {
            InitializeComponent();
            _studentId = userId;

        }

        private void UpdatePersonalInfo_Click(object sender, EventArgs e)
        {
            using (var db = new DBClass("syu", "1111"))
            {
                string Address = AddresstxtBox.Text.Trim();
                string Phone = PhonetxtBox.Text.Trim();
                string Email = EmailtxtBox.Text.Trim();
                string Password = PasswordtxtBox.Text.Trim();

                int rowsAffected = db.ExecuteNonQuery(
                    "UPDATE PersonalInfo " +
                "SET PASSWORD = :pwd, ADDRESS = :address, PHONE_number = :phone, EMAIL = :email " +
                "WHERE STUDENT_ID = :sid",

                 new OracleParameter("pwd", Password),
                 new OracleParameter("address", Address),
                 new OracleParameter("phone", Phone),
                 new OracleParameter("email", Email),
                 new OracleParameter("sid", _studentId)
                );

                if (rowsAffected > 0)
                    MessageBox.Show("신상정보가 성공적으로 변경되었습니다!");
                else
                    MessageBox.Show("해당 학번을 찾을 수 없습니다.");
            }

        }
    }
}


