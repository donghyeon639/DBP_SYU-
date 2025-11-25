using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.DataAccess.Client;

namespace SYU_DBP
{
    public partial class loginForm : Form
    {

        public loginForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void loginForm_Load(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            var userId = textBox1.Text.Trim();
            var password = textBox2.Text; // 비밀번호

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("아이디와 비밀번호를 입력하세요.");
                return;
            }

            const string dbUser = "syu"; // 스키마 계정
            const string dbPass = "1111";  // 스키마 비밀번호

            try
            {
                using (var db = new DBClass(dbUser, dbPass))
                {
                    var loginRepo = new LoginRepository(db);

                    bool prologinOk = false;
                    bool stuloginOk = false;

                    if (Proferadio.Checked) // 교수
                    {
                        prologinOk = loginRepo.ValidateProfessorLogin(userId, password);
                    }
                    else if (Studentradio.Checked) // 학생
                    {
                        stuloginOk = loginRepo.ValidateStudentLogin(userId, password);
                    }
                    else
                    {
                        MessageBox.Show("로그인 유형을 선택하세요.");
                        return;
                    }

                    if (prologinOk)
                    {
                        Form2 newForm = new Form2();
                        newForm.Show();
                    }
                    else if(stuloginOk)
                    {
                        Form3 newForm = new Form3();
                        newForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("로그인 실패: 아이디 또는 비밀번호를 확인하세요.");
                    }
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("DB 오류: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류: " + ex.Message);
            }
        }

        private void Proferadio_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Studentradio_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
