using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using System.Linq;

namespace SYU_DBP
{
    public partial class Form2 : Form
    {
        private DBClass _db;
        private StudentRepository _students;
        private CourseRepository _courses;
        private Grade_ScoreReposittory _grades;
        private const string DbUser = "syu";
        private const string DbPass = "1111";

        public Form2()
        {
            InitializeComponent();
            _db = new DBClass(DbUser, DbPass);
            _students = new StudentRepository(_db);
            _courses = new CourseRepository(_db);
            _grades = new Grade_ScoreReposittory(_db);
            StdinfoGridView.CellClick += DataGridView1_CellClick;
            CourselistView.SelectedIndexChanged += CourselistView_SelectedIndexChanged;
        }

        private void Form2_Load(object sender, System.EventArgs e)
        {
            LoadStudentGrid();
            LoadCourseList();
            LoadCourseInfoList();
            LoadGradeList();
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

        // ===== 성적 목록 로드 =====
        private void LoadGradeList()
        {
            try
            {
                var dt = _grades.GetGrades();
                GradelistView.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    // 디자이너 컬럼 순서: 학번, 과목명, 이수학점, 수강번호, 성적등급, 수강학기
                    var item = new ListViewItem(dr["student_id"].ToString());            // columnHeader2
                    item.SubItems.Add(dr["course_name"].ToString());                      // columnHeader3
                    item.SubItems.Add(dr["credits"].ToString());                          // columnHeader4
                    item.SubItems.Add(dr["course_number"].ToString());                    // columnHeader5
                    item.SubItems.Add(dr["grade_point"].ToString());                      // columnHeader8
                    item.SubItems.Add(dr["semester"].ToString());                         // columnHeader9
                    GradelistView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("성적 목록 로드 오류: " + ex.Message);
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

        private void ClearCourseInputs()
        {
            Coursenumtxt.Text = string.Empty;
            Stdidtxt.Text = string.Empty;
            Coursenametxt.Text = string.Empty;
            Coursecodetxt.Text = string.Empty;
            semestertxt.Text = string.Empty;
            comboBox3.SelectedIndex = -1;
            studentNameTxt.Text = string.Empty; // 학생 이름도 초기화
        }

        private void ClearGradeInputs()
        {
            Stdidtxt2.Text = string.Empty;
            Seemstertxt.Text = string.Empty;
            CourseNumbertxt.Text = string.Empty;
            Creditstxt.SelectedIndex = -1;
            Grade_point_txt.Text = string.Empty;
            CourseNametxt2.Text = string.Empty;
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

        private int ParseCourseCredits()
        {
            if (comboBox3.SelectedItem == null) return 0;
            var val = comboBox3.SelectedItem.ToString();
            int cr; return int.TryParse(val.Substring(0, 1), out cr) ? cr : 0;
        }

        // 학생 추가 버튼
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

        // 학생 수정 버튼
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

        // 학생 삭제 버튼
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

        // 수강 추가
        private void CourseInsertBtn_Click(object sender, EventArgs e)
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

        // 수강 수정 
        private void CourseUpdateBtn_Click(object sender, EventArgs e)
        {
            // 수정 가능 항목: 과목코드(Coursecodetxt), 과목명(Coursenametxt), 학점(comboBox3)
            // 비활성화 항목: 수강번호(Coursenumtxt), 학번(Stdidtxt), 학기(semestertxt), 학생명(studentNameTxt)
            Coursenumtxt.Enabled = false;
            Stdidtxt.Enabled = false;
            semestertxt.Enabled = false;
            studentNameTxt.Enabled = false;
            Coursecodetxt.Enabled = true;
            Coursenametxt.Enabled = true;
            comboBox3.Enabled = true;

            var courseNumber = Coursenumtxt.Text.Trim();
            var studentId = Stdidtxt.Text.Trim();
            var currentSemester = semestertxt.Text.Trim();
            var newCredits = ParseCourseCredits();
            string newCourseName = string.IsNullOrWhiteSpace(Coursenametxt.Text) ? null : Coursenametxt.Text.Trim();
            string newCourseCode = string.IsNullOrWhiteSpace(Coursecodetxt.Text) ? null : Coursecodetxt.Text.Trim();

            if (string.IsNullOrEmpty(courseNumber) || string.IsNullOrEmpty(currentSemester))
            { MessageBox.Show("수정할 과목번호/과목명이나 학기를 입력하세요."); ReEnableCourseInputs(); return; }

            int? creditsParam = newCredits > 0 ? (int?)newCredits : null;

            try
            {
                bool changed = false;

                // 1) 과목코드 변경
                if (!string.IsNullOrEmpty(newCourseCode))
                {
                    if (_courses.UpdateCourseCode(courseNumber, newCourseCode))
                        changed = true;
                }

                // 2) 과목명/학점 변경 (학기는 변경 불가 -> newSemester = null)
                if (newCourseName != null || creditsParam.HasValue)
                {
                    bool ok = _courses.UpdateEnrollmentPartial(studentId, courseNumber, currentSemester,
                                                               newCourseName: newCourseName,
                                                               newSemester: null, // 학기 변경 금지
                                                               newCredits: creditsParam);
                    if (ok) changed = true;
                }

                if (changed)
                {
                    MessageBox.Show("수강 수정 완료.");
                    LoadCourseList();
                }
                else
                {
                    MessageBox.Show("변경할 값이 없습니다 또는 실패.");
                }
            }
            catch (System.Exception ex) { MessageBox.Show("수정 오류: " + ex.Message); }
            finally
            {
                ReEnableCourseInputs();
            }
        }

        private void ReEnableCourseInputs()
        {
            Coursenumtxt.Enabled = true;
            Stdidtxt.Enabled = true;
            semestertxt.Enabled = true;
            studentNameTxt.Enabled = true;
            Coursecodetxt.Enabled = true;
            Coursenametxt.Enabled = true;
            comboBox3.Enabled = true;
        }

        // 수강 삭제
        private void CourseDeleteBtn_Click(object sender, EventArgs e)
        {
            var courseNumber = Coursenumtxt.Text.Trim();
            var studentId = Stdidtxt.Text.Trim();
            var semester = semestertxt.Text.Trim();
            if (string.IsNullOrEmpty(courseNumber) || string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(semester))
            { MessageBox.Show("삭제할 수강번호를 입력하세요."); return; }
            if (MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                if (_courses.DeleteEnrollment(studentId, courseNumber, semester))
                { MessageBox.Show("수강 삭제 완료."); LoadCourseList(); ClearCourseInputs(); }
                else MessageBox.Show("삭제 실패.");
            }
            catch (System.Exception ex) { MessageBox.Show("삭제 오류: " + ex.Message); }
        }

        // 도우미 조회 메서드
        private string LookupStudentNameById(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId)) return null;
            try
            {
                const string sql = "SELECT name FROM Student WHERE student_id = :sid";
                var dt = _db.GetDataTable(sql, new OracleParameter("sid", studentId.Trim()));
                if (dt.Rows.Count > 0) return dt.Rows[0]["name"].ToString();
            }
            catch { }
            return null;
        }

        private Tuple<string, string> LookupCourseByCode(string courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode)) return Tuple.Create<string, string>(null, null);
            try
            {
                const string sql = "SELECT course_name, course_number FROM Course WHERE course_code = :cc";
                var dt = _db.GetDataTable(sql, new OracleParameter("cc", courseCode.Trim()));
                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    return Tuple.Create(row["course_name"].ToString(), row["course_number"].ToString());
                }
            }
            catch { }
            return Tuple.Create<string, string>(null, null);
        }
        //학번으로 학생 이름 찾기 버튼
        private void SearchStBtn_Click(object sender, EventArgs e)
        {
            var sid = Stdidtxt.Text.Trim();
            var name = LookupStudentNameById(sid);
            if (!string.IsNullOrEmpty(name)) studentNameTxt.Text = name;
            else MessageBox.Show("학생 이름을 찾을 수 없습니다.");
        }
        // 과목코드 옆 과목명 찾기 버튼에서 사용 (명시적 형식 분해)
        private void SubjectsSearchBtn_Click(object sender, EventArgs e)
        {
            var code = Coursecodetxt.Text.Trim();
            Tuple<string, string> result = LookupCourseByCode(code);
            string cname = result.Item1;
            string cnum = result.Item2;
            if (!string.IsNullOrEmpty(cname))
            {
                Coursenametxt.Text = cname;
                if (!string.IsNullOrEmpty(cnum)) Coursenumtxt.Text = cnum;
            }
            else
            {
                MessageBox.Show("과목 정보를 찾을 수 없습니다.");
            }
        }
        private void CourseReset_Btn_Click(object sender, EventArgs e)
        {
            ClearCourseInputs();
        }

        // 목록 검색 (학번으로 조회) 및 필터링 로직 
        private void SearchBtn_Click(object sender, EventArgs e)
        {
            // 학생 정보 탭 검색 버튼 또는 수강 정보 탭 검색 버튼 공용 처리
            if (sender == SearchBtn)
            {
                var sidGrid = Searchstdidtxt.Text.Trim();
                if (string.IsNullOrWhiteSpace(sidGrid))
                {
                    LoadStudentGrid();
                    return;
                }
                try
                {
                    var dt = _students.GetStudentsById(sidGrid);
                    StdinfoGridView.DataSource = dt;
                    if (dt.Rows.Count == 0) MessageBox.Show("해당 학번의 학생이 없습니다.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("학생 검색 오류: " + ex.Message);
                }
                return;
            }

            string sid = null;
            if (sender == button7) // 수강정보 탭
                sid = textBox7.Text.Trim();
            else
                sid = Searchstdidtxt.Text.Trim(); // fallback

            if (string.IsNullOrWhiteSpace(sid))
            {
                MessageBox.Show("학번을 입력하세요.");
                LoadCourseList();
                return;
            }

            var name = LookupStudentNameById(sid);
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("해당 학번 학생이 존재하지 않습니다.");
                CourselistView.Items.Clear();
                studentNameTxt.Text = string.Empty;
                return;
            }

            LoadCourseList(sid);
            studentNameTxt.Text = name;
            if (CourselistView.Items.Count == 0)
            {
                MessageBox.Show($"학생 {name} 의 내역이 없습니다.");
            }

            // 성적 필터링은 GradeSearchBtn_Click에서 처리. 여기서는 기존 수강정보만.
        }

        // ===== 과목정보 목록 로드 =====
        private void LoadCourseInfoList()
        {
            try
            {
                var dt = _courses.GetCourses();
                listView1.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ListViewItem(dr["course_code"].ToString()); // columnHeader１
                    item.SubItems.Add(dr["course_name"].ToString());           // columnHeader２
                    item.SubItems.Add(dr["course_number"].ToString());         // columnHeader５
                    item.SubItems.Add(dr["course_type"].ToString());           // columnHeader３
                    item.SubItems.Add(dr["grade_level"].ToString());           // columnHeader４
                    item.SubItems.Add(dr["credit"].ToString());                // columnHeader6
                    item.SubItems.Add(dr["professor_id"].ToString());          // columnHeader7
                    item.SubItems.Add(dr["department_code"].ToString());       // columnHeader1
                    listView1.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("과목 목록 로드 오류: " + ex.Message);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var it = listView1.SelectedItems[0];
            // 매핑: 코드, 명, 수강번호, 이수구분, 수강학년, 학점, 교수ID, 학과코드
            Courinfocodetxt.Text = it.SubItems[0].Text;   // 과목코드
            Courinfonametxt.Text = it.SubItems[1].Text;   // 과목명
            Courseinfono.Text = it.SubItems[2].Text;      // 수강번호(과목번호)
            course_typetxt.SelectedItem = it.SubItems[3].Text; // 이수구분
            grade_level_txt.SelectedItem = it.SubItems[4].Text; // 수강학년
            // credit 컬럼은 입력 컨트롤이 없으므로 표시만 유지
            Profeidtxt.Text = it.SubItems[6].Text;        // 담당교수(교수ID)
            DepartmentCodetxt.Text = it.SubItems[7].Text; // 학과코드
        }

        private void CourseaddBtn_Click(object sender, EventArgs e)
        {
            string courseNumber = Courseinfono.Text.Trim();
            string courseCode = Courinfocodetxt.Text.Trim();
            string courseName = Courinfonametxt.Text.Trim();
            int? credit = null; // 과목정보 입력 UI에 학점 컨트롤이 없어 null 처리
            string courseType = course_typetxt.SelectedItem?.ToString();
            string departmentCode = DepartmentCodetxt.Text.Trim();
            int? gradeLevel = null;
            if (int.TryParse(grade_level_txt.SelectedItem?.ToString(), out var gl)) gradeLevel = gl;
            string professorId = Profeidtxt.Text.Trim();
            string generalArea = null; // UI 없음

            if (string.IsNullOrEmpty(courseNumber) || string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(courseName))
            { MessageBox.Show("필수 항목(수강번호, 과목코드, 과목명)을 입력하세요."); return; }

            try
            {
                bool ok = _courses.InsertCourse(courseNumber, courseCode, courseName, credit, courseType, departmentCode, gradeLevel, professorId, generalArea);
                if (ok) { MessageBox.Show("과목 개설 완료."); LoadCourseInfoList(); }
                else MessageBox.Show("개설 실패.");
            }
            catch (Exception ex) { MessageBox.Show("개설 오류: " + ex.Message); }
        }

        private void CoursefixBtn_Click(object sender, EventArgs e)
        {
            string courseNumber = Courseinfono.Text.Trim();
            if (string.IsNullOrEmpty(courseNumber)) { MessageBox.Show("수정할 과목번호를 입력하세요."); return; }

            string courseCode = string.IsNullOrWhiteSpace(Courinfocodetxt.Text) ? null : Courinfocodetxt.Text.Trim();
            string courseName = string.IsNullOrWhiteSpace(Courinfonametxt.Text) ? null : Courinfonametxt.Text.Trim();
            int? credit = null; // 학점 입력 UI 없음
            string courseType = course_typetxt.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(courseType)) courseType = null;
            string departmentCode = string.IsNullOrWhiteSpace(DepartmentCodetxt.Text) ? null : DepartmentCodetxt.Text.Trim();
            int? gradeLevel = null;
            if (int.TryParse(grade_level_txt.SelectedItem?.ToString(), out var gl)) gradeLevel = gl;
            string professorId = string.IsNullOrWhiteSpace(Profeidtxt.Text) ? null : Profeidtxt.Text.Trim();
            string generalArea = null;

            try
            {
                bool ok = _courses.UpdateCourse(courseNumber, courseCode, courseName, credit, courseType, departmentCode, gradeLevel, professorId, generalArea);
                if (ok) { MessageBox.Show("과목 수정 완료."); LoadCourseInfoList(); }
                else MessageBox.Show("변경할 값이 없습니다 또는 실패.");
            }
            catch (Exception ex) { MessageBox.Show("수정 오류: " + ex.Message); }
        }

        private void CourDeleteBtn_Click(object sender, EventArgs e)
        {
            string courseNumber = Courseinfono.Text.Trim();
            if (string.IsNullOrEmpty(courseNumber)) { MessageBox.Show("삭제할 과목번호를 입력하세요."); return; }
            if (MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                if (_courses.DeleteCourse(courseNumber)) { MessageBox.Show("과목 삭제 완료."); LoadCourseInfoList(); }
                else MessageBox.Show("삭제 실패.");
            }
            catch (Exception ex) { MessageBox.Show("삭제 오류: " + ex.Message); }
        }

        private void CourResetBtn_Click(object sender, EventArgs e)
        {
            Courseinfono.Text = string.Empty;
            Courinfocodetxt.Text = string.Empty;
            Courinfonametxt.Text = string.Empty;
            course_typetxt.SelectedIndex = -1;
            grade_level_txt.SelectedIndex = -1;
            Profeidtxt.Text = string.Empty;
            DepartmentCodetxt.Text = string.Empty;
        }
        // ================= 성적정보 영역 ================= //

        private void LoadGrade_scoreList()
        {
            try
            {
                var dt = _courses.GetCourses();
                listView1.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ListViewItem(dr["course_code"].ToString()); // columnHeader１
                    item.SubItems.Add(dr["course_name"].ToString());           // columnHeader２
                    item.SubItems.Add(dr["course_number"].ToString());         // columnHeader５
                    item.SubItems.Add(dr["course_type"].ToString());           // columnHeader３
                    item.SubItems.Add(dr["grade_level"].ToString());           // columnHeader４
                    item.SubItems.Add(dr["credit"].ToString());                // columnHeader6
                    item.SubItems.Add(dr["professor_id"].ToString());          // columnHeader7
                    item.SubItems.Add(dr["department_code"].ToString());       // columnHeader1
                    listView1.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("과목 목록 로드 오류: " + ex.Message);
            }
        }

        private void GradelistView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GradelistView.SelectedItems.Count == 0) return;
            var item = GradelistView.SelectedItems[0];
          
            Stdidtxt2.Text = item.SubItems[0].Text;          // 학번
            CourseNametxt2.Text = item.SubItems[1].Text;     // 과목명
            Creditstxt.Text = item.SubItems[2].Text;         // 이수학점
            CourseNumbertxt.Text = item.SubItems[3].Text;    // 수강번호
            Grade_point_txt.Text = item.SubItems[4].Text;    // 성적등급
            Seemstertxt.Text = item.SubItems[5].Text;        // 수강학기
        }
        private void GradeinsertBtn_Click(object sender, EventArgs e)
        {
            var sid = Stdidtxt2.Text.Trim();
            var sem = Seemstertxt.Text.Trim();
            var cno = CourseNumbertxt.Text.Trim();
            int credits = 0;
            if (Creditstxt.SelectedItem != null)
            {
                int.TryParse(Creditstxt.SelectedItem.ToString().Replace("학점", ""), out credits);
            }
            decimal gp = 0m;
            decimal.TryParse(Grade_point_txt.Text.Trim(), out gp);
            string aw = null; // 경고 플래그 UI 없음

            if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(sem) || string.IsNullOrEmpty(cno) || credits <= 0)
            { MessageBox.Show("필수 항목(학번, 학기, 과목번호, 학점)을 입력/선택하세요."); return; }

            try
            {
                if (_grades.InsertGrade(sid, sem, cno, credits, gp, aw))
                { MessageBox.Show("성적 추가 완료."); LoadGradeList(); GradeReset(); }
                else MessageBox.Show("추가 실패.");
            }
            catch (Exception ex) { MessageBox.Show("추가 오류: " + ex.Message); }
        }

        private void GradeUpdateBtn_Click(object sender, EventArgs e)
        {
            var sid = Stdidtxt2.Text.Trim();
            var sem = Seemstertxt.Text.Trim();
            var cno = CourseNumbertxt.Text.Trim();
            int? credits = null;
            if (Creditstxt.SelectedItem != null && int.TryParse(Creditstxt.SelectedItem.ToString().Replace("학점", ""), out var cr)) credits = cr;
            decimal? gp = null;
            if (decimal.TryParse(Grade_point_txt.Text.Trim(), out var d)) gp = d;
            string aw = null;

            if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(sem) || string.IsNullOrEmpty(cno))
            { MessageBox.Show("수정할 학번/학기/과목번호를 입력하세요."); return; }

            try
            {
                if (_grades.UpdateGrade(sid, sem, cno, credits, gp, aw))
                { MessageBox.Show("성적 수정 완료."); LoadGradeList(); }
                else MessageBox.Show("변경할 값이 없습니다 또는 실패.");
            }
            catch (Exception ex) { MessageBox.Show("수정 오류: " + ex.Message); }
        }

        private void GradeDeleteBtn_Click(object sender, EventArgs e)
        {
            var sid = Stdidtxt2.Text.Trim();
            var sem = Seemstertxt.Text.Trim();
            var cno = CourseNumbertxt.Text.Trim();
            if (string.IsNullOrEmpty(sid) || string.IsNullOrEmpty(sem) || string.IsNullOrEmpty(cno))
            { MessageBox.Show("삭제할 학번/학기/과목번호를 입력하세요."); return; }
            if (MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                if (_grades.DeleteGrade(sid, sem, cno))
                { MessageBox.Show("성적 삭제 완료."); LoadGradeList(); GradeReset(); }
                else MessageBox.Show("삭제 실패.");
            }
            catch (Exception ex) { MessageBox.Show("삭제 오류: " + ex.Message); }
        }

        private void GradeReset()
        {
            Stdidtxt2.Text = string.Empty;
            Seemstertxt.Text = string.Empty;
            CourseNumbertxt.Text = string.Empty;
            Creditstxt.SelectedIndex = -1;
            Grade_point_txt.Text = string.Empty;
            CourseNametxt2.Text = string.Empty;
        }
        private void GradeResetBtn_Click(object sender, EventArgs e)
        {
            GradeReset();
        }

        // 디자이너 참조 이벤트 핸들러(빈 구현)
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void textBox18_TextChanged(object sender, EventArgs e) { }
        private void textBox21_TextChanged(object sender, EventArgs e) { }
        private void Searchstdidtxt_TextChanged(object sender, EventArgs e) { }

        private void GradeSearchBtn_Click(object sender, EventArgs e)
        {
            var sid = GradeStidtxt.Text.Trim();
            if (string.IsNullOrWhiteSpace(sid))
            {
                MessageBox.Show("학번을 입력하세요.");
                LoadGradeList();
                return;
            }
            try
            {
                var dt = _grades.GetGradesByStudent(sid);
                GradelistView.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ListViewItem(dr["student_id"].ToString());
                    item.SubItems.Add(dr["course_name"].ToString());
                    item.SubItems.Add(dr["credits"].ToString());
                    item.SubItems.Add(dr["course_number"].ToString());
                    item.SubItems.Add(dr["grade_point"].ToString());
                    item.SubItems.Add(dr["semester"].ToString());
                    GradelistView.Items.Add(item);
                }
                if (GradelistView.Items.Count == 0)
                {
                    MessageBox.Show("해당 학번의 성적 내역이 없습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("성적 검색 오류: " + ex.Message);
            }
        }

        private void Coursecode_SearchBtn_Click(object sender, EventArgs e)
        {
            var code = GradeCourseCode_txt.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("과목코드를 입력하세요.");
                LoadCourseInfoList();
                return;
            }
            try
            {
                var dt = _courses.GetCoursesByCode(code);
                listView1.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ListViewItem(dr["course_code"].ToString());
                    item.SubItems.Add(dr["course_name"].ToString());
                    item.SubItems.Add(dr["course_number"].ToString());
                    item.SubItems.Add(dr["course_type"].ToString());
                    item.SubItems.Add(dr["grade_level"].ToString());
                    item.SubItems.Add(dr["credit"].ToString());
                    item.SubItems.Add(dr["professor_id"].ToString());
                    item.SubItems.Add(dr["department_code"].ToString());
                    listView1.Items.Add(item);
                }
                if (listView1.Items.Count == 0)
                {
                    MessageBox.Show("해당 과목코드의 과목이 없습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("과목코드 검색 오류: " + ex.Message);
            }
        }

        private void GradeStidtxt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
