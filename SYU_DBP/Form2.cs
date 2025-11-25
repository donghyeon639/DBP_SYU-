using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;

namespace SYU_DBP
{
    public partial class Form2 : Form
    {
        private DBClass _db;
        private StudentRepository _students;
        private CourseRepository _courses;
        private const string DbUser = "syu";
        private const string DbPass = "1111";

        public Form2()
        {
            InitializeComponent();
            _db = new DBClass(DbUser, DbPass);
            _students = new StudentRepository(_db);
            _courses = new CourseRepository(_db);
            StdinfoGridView.CellClick += DataGridView1_CellClick;
            CourselistView.SelectedIndexChanged += CourselistView_SelectedIndexChanged;
        }

        private void Form2_Load(object sender, System.EventArgs e)
        {
            LoadStudentGrid();
            LoadCourseList();
        }

        private void LoadStudentGrid()
        {
            try
            {
                var dt = _students.GetStudents();
                StdinfoGridView.DataSource = dt;
                if (StdinfoGridView.Columns.Contains("student_id")) StdinfoGridView.Columns["student_id"].HeaderText = "학번";
                if (StdinfoGridView.Columns.Contains("name")) StdinfoGridView.Columns["name"].HeaderText = "이름";
                if (StdinfoGridView.Columns.Contains("department_name")) StdinfoGridView.Columns["department_name"].HeaderText = "학과";
                if (StdinfoGridView.Columns.Contains("grade")) StdinfoGridView.Columns["grade"].HeaderText = "학년";
                if (StdinfoGridView.Columns.Contains("phone_number")) StdinfoGridView.Columns["phone_number"].HeaderText = "연락처";
                if (StdinfoGridView.Columns.Contains("birth_date")) StdinfoGridView.Columns["birth_date"].HeaderText = "생년월일";
                if (StdinfoGridView.Columns.Contains("address")) StdinfoGridView.Columns["address"].HeaderText = "주소";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("학생 목록 로드 오류: " + ex.Message);
            }
        }

        private void ClearStudentInputs()
        {
            Stidtxt.Text = string.Empty;
            StdNametxt.Text = string.Empty;
            DepartcomboBox.SelectedIndex = -1;
            Gradetxt.SelectedIndex = -1;
            Phonenumtxt.Text = string.Empty;
            Birthtxt.Text = string.Empty;
            Adresstxt.Text = string.Empty;
        }

        private int ParseGrade()
        {
            if (Gradetxt.SelectedItem == null) return 0;
            var val = Gradetxt.SelectedItem.ToString();
            int grade;
            if (int.TryParse(val.Substring(0, 1), out grade)) return grade;
            return 0;
        }

        private System.DateTime? ParseBirth()
        {
            if (string.IsNullOrWhiteSpace(Birthtxt.Text)) return null;
            System.DateTime dt;
            if (System.DateTime.TryParseExact(Birthtxt.Text.Trim(), new[] { "yyyy-MM-dd", "yyyyMMdd" }, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                return dt;
            return null;
        }

        private void InsertBtn_Click(object sender, System.EventArgs e)
        {
            var sid = Stidtxt.Text.Trim();
            var name = StdNametxt.Text.Trim();
            var deptNameOrCode = DepartcomboBox.SelectedItem?.ToString();
            var grade = ParseGrade();
            var phone = Phonenumtxt.Text.Trim();
            var birth = ParseBirth();
            var addr = Adresstxt.Text.Trim();

            if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(deptNameOrCode) || grade == 0)
            {
                MessageBox.Show("필수 항목(학번, 이름, 학과, 학년)을 입력하세요.");
                return;
            }

            try
            {
                if (_students.InsertStudent(sid, name, deptNameOrCode, grade, phone, birth, addr))
                {
                    MessageBox.Show("학생 추가 완료.");
                    LoadStudentGrid();
                    ClearStudentInputs();
                }
                else
                {
                    MessageBox.Show("학과 코드/이름 확인 실패.");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("추가 오류: " + ex.Message);
            }
        }

        private void UpdateBtn_Click(object sender, System.EventArgs e)
        {
            var sid = Stidtxt.Text.Trim();
            var name = StdNametxt.Text.Trim();
            var deptNameOrCode = DepartcomboBox.SelectedItem?.ToString();
            var grade = ParseGrade();
            var phone = Phonenumtxt.Text.Trim();
            var birth = ParseBirth();
            var addr = Adresstxt.Text.Trim();

            if (string.IsNullOrEmpty(sid))
            {
                MessageBox.Show("수정할 학번을 입력하세요.");
                return;
            }

            try
            {
                if (_students.UpdateStudent(sid, name, deptNameOrCode, grade, phone, birth, addr))
                {
                    MessageBox.Show("학생 수정 완료.");
                    LoadStudentGrid();
                }
                else
                {
                    MessageBox.Show("수정 실패 (학과 코드/이름 문제).");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("수정 오류: " + ex.Message);
            }
        }

        private void DeleteBtn_Click(object sender, System.EventArgs e)
        {
            var sid = Stidtxt.Text.Trim();
            if (string.IsNullOrEmpty(sid))
            {
                MessageBox.Show("삭제할 학번을 입력하세요.");
                return;
            }
            if (MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                if (_students.DeleteStudent(sid))
                {
                    MessageBox.Show("학생 삭제 완료.");
                    LoadStudentGrid();
                    ClearStudentInputs();
                }
                else
                {
                    MessageBox.Show("삭제 실패.");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("삭제 오류: " + ex.Message);
            }
        }

        private void ResetBtn_Click(object sender, System.EventArgs e)
        {
            ClearStudentInputs();
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = StdinfoGridView.Rows[e.RowIndex];
            if (row == null) return;

            Stidtxt.Text = row.Cells["student_id"].Value?.ToString();
            StdNametxt.Text = row.Cells["name"].Value?.ToString();
            Phonenumtxt.Text = row.Cells["phone_number"].Value?.ToString();
            Adresstxt.Text = row.Cells["address"].Value?.ToString();

            var birthObj = row.Cells["birth_date"].Value;
            if (birthObj != null && birthObj != System.DBNull.Value)
            {
                System.DateTime bd;
                if (System.DateTime.TryParse(birthObj.ToString(), out bd))
                    Birthtxt.Text = bd.ToString("yyyy-MM-dd");
                else
                    Birthtxt.Text = birthObj.ToString();
            }
            else
            {
                Birthtxt.Text = string.Empty;
            }

            var gradeObj = row.Cells["grade"].Value?.ToString();
            if (!string.IsNullOrEmpty(gradeObj))
            {
                foreach (var item in Gradetxt.Items)
                {
                    if (item.ToString().StartsWith(gradeObj + "학년"))
                    {
                        Gradetxt.SelectedItem = item;
                        break;
                    }
                }
            }

            var deptName = row.Cells["department_name"].Value?.ToString();
            if (!string.IsNullOrEmpty(deptName))
            {
                for (int i = 0; i < DepartcomboBox.Items.Count; i++)
                {
                    if (DepartcomboBox.Items[i].ToString() == deptName)
                    {
                        DepartcomboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        // ================= 수강정보 영역 =================
        private void LoadCourseList(string studentFilter = null)
        {
            try
            {
                DataTable dt = string.IsNullOrWhiteSpace(studentFilter) ? _courses.GetEnrolledCourses() : _courses.GetEnrolledCoursesByStudent(studentFilter);
                CourselistView.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ListViewItem(dr["course_number"].ToString());
                    item.SubItems.Add(dr["student_id"].ToString());
                    item.SubItems.Add(dr["student_name"].ToString());
                    item.SubItems.Add(dr["course_code"].ToString());
                    item.SubItems.Add(dr["course_name"].ToString());
                    item.SubItems.Add(dr["credits"].ToString());
                    item.SubItems.Add(dr["semester"].ToString());
                    CourselistView.Items.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("수강 목록 로드 오류: " + ex.Message);
            }
        }

        private void ClearCourseInputs()
        {
            Coursenumtxt.Text = string.Empty;
            Stdidtxt.Text = string.Empty;
            Coursenametxt.Text = string.Empty;
            Coursecodetxt.Text = string.Empty;
            semestertxt.Text = string.Empty;
            comboBox3.SelectedIndex = -1;
        }

        private int ParseCourseCredits()
        {
            if (comboBox3.SelectedItem == null) return 0;
            var val = comboBox3.SelectedItem.ToString();
            int cr; return int.TryParse(val.Substring(0, 1), out cr) ? cr : 0;
        }

        private void CourselistView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CourselistView.SelectedItems.Count == 0) return;
            var item = CourselistView.SelectedItems[0];
            Coursenumtxt.Text = item.SubItems[0].Text;
            Stdidtxt.Text = item.SubItems[1].Text;
            studentNameTxt.Text = item.SubItems[2].Text;
            Coursecodetxt.Text = item.SubItems[3].Text;
            Coursenametxt.Text = item.SubItems[4].Text;
            var crStr = item.SubItems[5].Text;
            foreach (var comboItem in comboBox3.Items)
                if (comboItem.ToString().StartsWith(crStr + "학점")) { comboBox3.SelectedItem = comboItem; break; }
            semestertxt.Text = item.SubItems[6].Text;
        }

        // 추가
        private void button8_Click(object sender, EventArgs e)
        {
            var courseNumber = Coursenumtxt.Text.Trim();
            var studentId = Stdidtxt.Text.Trim();
            var semester = semestertxt.Text.Trim();
            var credits = ParseCourseCredits();
            if (string.IsNullOrEmpty(courseNumber) || string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(semester) || credits == 0)
            { MessageBox.Show("필수 항목(수강번호, 학번, 학기, 학점)을 입력/선택하세요."); return; }
            try
            {
                if (_courses.InsertEnrollment(studentId, courseNumber, semester, credits))
                { MessageBox.Show("수강 추가 완료."); LoadCourseList(); ClearCourseInputs(); }
                else MessageBox.Show("추가 실패.");
            }
            catch (System.Exception ex) { MessageBox.Show("추가 오류: " + ex.Message); }
        }

        // 수정: 선택한 항목 기준으로 부분 수정 허용
        private void button7_Click(object sender, EventArgs e)
        {
            var courseNumber = Coursenumtxt.Text.Trim();
            var studentId = Stdidtxt.Text.Trim();
            var currentSemester = semestertxt.Text.Trim();
            var newCredits = ParseCourseCredits();
            string newCourseName = string.IsNullOrWhiteSpace(Coursenametxt.Text) ? null : Coursenametxt.Text.Trim();
            string newSemester = string.IsNullOrWhiteSpace(currentSemester) ? null : currentSemester; // 입력된 학기 그대로 사용

            if (string.IsNullOrEmpty(courseNumber) || string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(currentSemester))
            { MessageBox.Show("수정할 수강번호/학번/학기를 입력하세요."); return; }

            // 학점도 선택되지 않았다면 null로 처리
            int? creditsParam = newCredits > 0 ? (int?)newCredits : null;

            try
            {
                bool ok = _courses.UpdateEnrollmentPartial(studentId, courseNumber, currentSemester,
                                                           newCourseName: newCourseName,
                                                           newSemester: newSemester,
                                                           newCredits: creditsParam);
                if (ok) { MessageBox.Show("수강 수정 완료."); LoadCourseList(); }
                else { MessageBox.Show("변경할 값이 없습니다 또는 실패."); }
            }
            catch (System.Exception ex) { MessageBox.Show("수정 오류: " + ex.Message); }
        }

        // 삭제
        private void button6_Click(object sender, EventArgs e)
        {
            var courseNumber = Coursenumtxt.Text.Trim();
            var studentId = Stdidtxt.Text.Trim();
            var semester = semestertxt.Text.Trim();
            if (string.IsNullOrEmpty(courseNumber) || string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(semester))
            { MessageBox.Show("삭제할 수강번호/학번/학기를 입력하세요."); return; }
            if (MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                if (_courses.DeleteEnrollment(studentId, courseNumber, semester))
                { MessageBox.Show("수강 삭제 완료."); LoadCourseList(); ClearCourseInputs(); }
                else MessageBox.Show("삭제 실패.");
            }
            catch (System.Exception ex) { MessageBox.Show("삭제 오류: " + ex.Message); }
        }

        private void button5_Click(object sender, EventArgs e) { ClearCourseInputs(); }

        // 디자이너 참조 이벤트 핸들러(빈 구현) 추가
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void textBox18_TextChanged(object sender, EventArgs e) { }
        private void textBox21_TextChanged(object sender, EventArgs e) { }
    }
}
