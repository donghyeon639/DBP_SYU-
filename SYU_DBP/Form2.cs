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
        private Grade_ScoreRepository _grades;
        private GraduationRequirementRepository _graduationReq;
        private GraduationReviewRepository _graduationReview;
        private InquiryRepository _inquiry; // 추가
        private const string DbUser = "syu";
        private const string DbPass = "1111";

        public Form2()
        {
            InitializeComponent();
            _db = new DBClass(DbUser, DbPass);
            _students = new StudentRepository(_db);
            _courses = new CourseRepository(_db);
            _grades = new Grade_ScoreRepository(_db);
            _graduationReq = new GraduationRequirementRepository(_db);
            _graduationReview = new GraduationReviewRepository(_db);
            _inquiry = new InquiryRepository(_db); // 초기화
            
            StdinfoGridView.CellClick += DataGridView1_CellClick;
            CourselistView.SelectedIndexChanged += CourselistView_SelectedIndexChanged;
            ReviewlistView.SelectedIndexChanged += ReviewlistView_SelectedIndexChanged;
        }

        private void Form2_Load(object sender, System.EventArgs e)
        {
            LoadStudentGrid();
            LoadCourseList();
            LoadCourseInfoList();
            LoadGradeList();
            LoadGraduationRequirementList();
            LoadGraduationReviewList();
            LoadInquiryManagementTab(); // 문의 관리 탭 로드 추가
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

        // ===== 졸업요건 목록 로드 =====
        private void LoadGraduationRequirementList()
        {
            try
            {
                var dt = _graduationReq.GetAll();
                Graduationlistview.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    // 요구 순서: 졸업ID, 학과코드, 총이수학점, 전공학점, 입학년도, 교양학점, 교양영역 이수횟수, 채플이수횟수, 필수교양상세
                    var item = new ListViewItem(dr["graduation_id"].ToString());                  // 0 졸업ID
                    item.SubItems.Add(dr["department_code"].ToString());                        // 1 학과코드
                    item.SubItems.Add(dr["earned_credits"].ToString());                         // 2 총이수학점
                    item.SubItems.Add(dr["major_credits"].ToString());                          // 3 전공학점
                    item.SubItems.Add(dr["admission_year"].ToString());                         // 4 입학년도
                    item.SubItems.Add(dr["general_credits"].ToString());                        // 5 교양학점
                    item.SubItems.Add(dr["material_completion_count"].ToString());              // 6 교양영역 이수횟수
                    item.SubItems.Add(dr["chapel_completion_count"].ToString());                // 7 채플이수횟수
                    item.SubItems.Add(dr["required_general_details"].ToString());               // 8 필수교양상세
                    Graduationlistview.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("졸업요건 목록 로드 오류: " + ex.Message);
            }
        }

        // ===== 졸업심사 목록 로드 =====
        private void LoadGraduationReviewList()
        {
            try
            {
                var dt = _graduationReview.GetAll();
                ReviewlistView.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    var item = new ListViewItem(dr["review_id"].ToString()); // 0 심사ID
                    item.SubItems.Add(dr["student_id"].ToString());          // 1 학번
                    var schObj = dr["review_schedule"];
                    item.SubItems.Add(schObj == DBNull.Value ? string.Empty : Convert.ToDateTime(schObj).ToString("yyyy-MM-dd")); // 2 심사일정
                    item.SubItems.Add(dr["review_result"].ToString());       // 3 심사결과
                    item.SubItems.Add(dr["reviewer"].ToString());            // 4 심사자
                    item.SubItems.Add(dr["semester"].ToString());            // 5 학기
                    item.SubItems.Add(dr["admission_year"].ToString());      // 6 입학연도
                    item.SubItems.Add(dr["department_code"].ToString());     // 7 학과코드
                    ReviewlistView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("졸업심사 목록 로드 오류: " + ex.Message);
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


        private void GraduationReset()
        {
            txtGraduationId.Text = string.Empty;
            txtAdmissionYear.Text = string.Empty;
            txtDepartmentCode.Text = string.Empty;
            txtEarnedCredits.Text = string.Empty;
            txtGeneralCredits.Text = string.Empty;
            txtMajorCredits.Text = string.Empty;
            txtMaterialCount.Text = string.Empty;
            cboGeneralCount.SelectedIndex = -1;
            if (txtgeneral_details != null) txtgeneral_details.Text = string.Empty;
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
        private void CourResetBtn_Click(object sender, EventArgs e)
        {
            // 과목정보 입력 컨트롤 초기화
            Courseinfono.Text = string.Empty;           // 수강번호
            Courinfocodetxt.Text = string.Empty;        // 과목코드
            Courinfonametxt.Text = string.Empty;        // 과목명
            DepartmentCodetxt.Text = string.Empty;      // 학과코드
            Profeidtxt.Text = string.Empty;             // 교수ID
            course_typetxt.SelectedIndex = -1;          // 이수구분 초기화
            grade_level_txt.SelectedIndex = -1;         // 수강학년 초기화
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

        // ================= 성적정보 영역 ================= //

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
        // --------------졸업요건 영역---------------------------

        private void Graduationlistview_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Graduationlistview.SelectedItems.Count == 0) return;
            var it = Graduationlistview.SelectedItems[0];
            // 매핑: 0 졸업ID,1 학과코드,2 총이수학점,3 전공학점,4 입학년도,5 교양학점,6 교양영역 이수횟수,7 채플이수횟수,8 필수교양상세
            txtGraduationId.Text = it.SubItems[0].Text;
            txtDepartmentCode.Text = it.SubItems[1].Text;
            txtEarnedCredits.Text = it.SubItems[2].Text;
            txtMajorCredits.Text = it.SubItems[3].Text;
            txtAdmissionYear.Text = it.SubItems[4].Text;
            txtGeneralCredits.Text = it.SubItems[5].Text;
            cboGeneralCount.Text = it.SubItems[6].Text; // 교양영역 이수횟수
            txtMaterialCount.Text = it.SubItems[7].Text; // 채플이수횟수
            if (txtgeneral_details != null) txtgeneral_details.Text = it.SubItems[8].Text;
        }

        private void BtnGraduationInsert_Click(object sender, EventArgs e)
        {
            string gid = txtGraduationId.Text.Trim();
            if (string.IsNullOrEmpty(gid)) { MessageBox.Show("졸업ID를 입력하세요."); return; }
            int ay; if (!int.TryParse(txtAdmissionYear.Text.Trim(), out ay)) { MessageBox.Show("입학년도를 숫자로 입력하세요."); return; }
            string dept = txtDepartmentCode.Text.Trim();
            int? earned = int.TryParse(txtEarnedCredits.Text.Trim(), out var ec) ? (int?)ec : null;               // earned_credits
            int? general = int.TryParse(txtGeneralCredits.Text.Trim(), out var gc) ? (int?)gc : null;             // general_credits
            int? major = int.TryParse(txtMajorCredits.Text.Trim(), out var mc) ? (int?)mc : null;                 // major_credits
            int? material = int.TryParse(cboGeneralCount.Text.Trim(), out var mat) ? (int?)mat : null;            // material_completion_count (교양영역 이수횟수)
            string details = txtgeneral_details != null ? txtgeneral_details.Text.Trim() : null;                  // required_general_details
            int? chapel = int.TryParse(txtMaterialCount.Text.Trim(), out var ch) ? (int?)ch : null;               // chapel_completion_count (채플이수횟수)

            try
            {
                if (_graduationReq.Insert(gid, ay, dept, earned, general, major, material, details, chapel))
                { MessageBox.Show("졸업요건 추가 완료."); LoadGraduationRequirementList(); GraduationReset(); }
                else MessageBox.Show("개설 실패.");
            }
            catch (Exception ex) { MessageBox.Show("추가 오류: " + ex.Message); }
        }

        private void BtnGraduationUpdate_Click(object sender, EventArgs e)
        {
            string gid = txtGraduationId.Text.Trim();
            if (string.IsNullOrEmpty(gid)) { MessageBox.Show("수정할 졸업ID를 입력하세요."); return; }
            int? ay = int.TryParse(txtAdmissionYear.Text.Trim(), out var ayv) ? (int?)ayv : null;                 // admission_year
            string dept = string.IsNullOrWhiteSpace(txtDepartmentCode.Text) ? null : txtDepartmentCode.Text.Trim();
            int? earned = int.TryParse(txtEarnedCredits.Text.Trim(), out var ec) ? (int?)ec : null;               // earned_credits
            int? general = int.TryParse(txtGeneralCredits.Text.Trim(), out var gc) ? (int?)gc : null;             // general_credits
            int? major = int.TryParse(txtMajorCredits.Text.Trim(), out var mc) ? (int?)mc : null;                 // major_credits
            int? material = int.TryParse(cboGeneralCount.Text.Trim(), out var mat) ? (int?)mat : null;            // material_completion_count
            string details = txtgeneral_details != null ? txtgeneral_details.Text.Trim() : null;                  // required_general_details
            int? chapel = int.TryParse(txtMaterialCount.Text.Trim(), out var ch) ? (int?)ch : null;               // chapel_completion_count

            try
            {
                if (_graduationReq.Update(gid, ay, dept, earned, general, major, material, details, chapel))
                { MessageBox.Show("졸업요건 수정 완료."); LoadGraduationRequirementList(); }
                else MessageBox.Show("변경할 값이 없습니다 또는 실패.");
            }
            catch (Exception ex) { MessageBox.Show("수정 오류: " + ex.Message); }
        }

        private void BtnGraduationDelete_Click(object sender, EventArgs e)
        {
            string gid = txtGraduationId.Text.Trim();
            if (string.IsNullOrEmpty(gid)) { MessageBox.Show("삭제할 졸업ID를 입력하세요."); return; }
            if (MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                if (_graduationReq.Delete(gid)) { MessageBox.Show("졸업요건 삭제 완료."); LoadGraduationRequirementList(); GraduationReset(); }
                else MessageBox.Show("삭제 실패.");
            }
            catch (Exception ex) { MessageBox.Show("삭제 오류: " + ex.Message); }
        }

        private void BtnGraduationReset_Click(object sender, EventArgs e)
        {
            GraduationReset();
        }

        // ===== 졸업심사 영역 이벤트 및 메서드 =====
        private void ReviewlistView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ReviewlistView.SelectedItems.Count == 0) return;
            var it = ReviewlistView.SelectedItems[0];
            txtReviewId.Text = it.SubItems[0].Text;
            txtStudentId.Text = it.SubItems[1].Text;
            dtpReviewSchedule.Text = it.SubItems[2].Text; // 텍스트 값 매핑
            CBResult.Text = it.SubItems[3].Text;
            txtReviewer.Text = it.SubItems[4].Text;
            txtSemester.Text = it.SubItems[5].Text;
            txtYearAdmission.Text = it.SubItems[6].Text;
            txtDeptCode.Text = it.SubItems[7].Text;
        }

        private void reviewInsertBtn_Click(object sender, EventArgs e)
        {
            string rid = txtReviewId.Text.Trim();
            string sid = txtStudentId.Text.Trim();
            DateTime? rs = null;
            if (DateTime.TryParseExact(dtpReviewSchedule.Text.Trim(), new[] { "yyyy-MM-dd", "yyyyMMdd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) rs = d;
            string rr = string.IsNullOrWhiteSpace(CBResult.Text) ? null : CBResult.Text.Trim();
            string rev = string.IsNullOrWhiteSpace(txtReviewer.Text) ? null : txtReviewer.Text.Trim();
            string sem = string.IsNullOrWhiteSpace(txtSemester.Text) ? null : txtSemester.Text.Trim();
            int? ay = int.TryParse(txtYearAdmission.Text.Trim(), out var ayv) ? (int?)ayv : null;
            string dept = string.IsNullOrWhiteSpace(txtDeptCode.Text) ? null : txtDeptCode.Text.Trim();
            string cno = null; // UI 없음

            if (string.IsNullOrEmpty(rid) || string.IsNullOrEmpty(sid)) { MessageBox.Show("필수 항목(심사ID, 학번)을 입력하세요."); return; }

            try
            {
                if (_graduationReview.Insert(rid, sid, rs, rr, rev, cno, sem, ay, dept))
                { MessageBox.Show("졸업심사 추가 완료."); LoadGraduationReviewList(); ReviewReset(); }
                else MessageBox.Show("추가 실패.");
            }
            catch (Exception ex) { MessageBox.Show("추가 오류: " + ex.Message); }
        }

        private void reviewUpdateBtn_Click(object sender, EventArgs e)
        {
            string rid = txtReviewId.Text.Trim();
            if (string.IsNullOrEmpty(rid)) { MessageBox.Show("수정할 심사ID를 입력하세요."); return; }
            string sid = string.IsNullOrWhiteSpace(txtStudentId.Text) ? null : txtStudentId.Text.Trim();
            DateTime? rs = null;
            if (DateTime.TryParseExact(dtpReviewSchedule.Text.Trim(), new[] { "yyyy-MM-dd", "yyyyMMdd" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) rs = d;
            string rr = string.IsNullOrWhiteSpace(CBResult.Text) ? null : CBResult.Text.Trim();
            string rev = string.IsNullOrWhiteSpace(txtReviewer.Text) ? null : txtReviewer.Text.Trim();
            string sem = string.IsNullOrWhiteSpace(txtSemester.Text) ? null : txtSemester.Text.Trim();
            int? ay = int.TryParse(txtYearAdmission.Text.Trim(), out var ayv) ? (int?)ayv : null;
            string dept = string.IsNullOrWhiteSpace(txtDeptCode.Text) ? null : txtDeptCode.Text.Trim();
            string cno = null;

            try
            {
                if (_graduationReview.Update(rid, sid, rs, rr, rev, cno, sem, ay, dept))
                { MessageBox.Show("졸업심사 수정 완료."); LoadGraduationReviewList(); }
                else MessageBox.Show("변경할 값이 없습니다 또는 실패.");
            }
            catch (Exception ex) { MessageBox.Show("수정 오류: " + ex.Message); }
        }

        private void reviewDeleteBtn_Click(object sender, EventArgs e)
        {
            string rid = txtReviewId.Text.Trim();
            if (string.IsNullOrEmpty(rid)) { MessageBox.Show("삭제할 심사ID를 입력하세요."); return; }
            if (MessageBox.Show("정말 삭제하시겠습니까?", "확인", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                if (_graduationReview.Delete(rid)) { MessageBox.Show("졸업심사 삭제 완료."); LoadGraduationReviewList(); ReviewReset(); }
                else MessageBox.Show("삭제 실패.");
            }
            catch (Exception ex) { MessageBox.Show("삭제 오류: " + ex.Message); }
        }

        private void ReviewResetBtn_Click(object sender, EventArgs e)
        {
            ReviewReset();
        }

        private void ReviewReset()
        {
            txtReviewId.Text = string.Empty;
            txtStudentId.Text = string.Empty;
            dtpReviewSchedule.Text = string.Empty;
            CBResult.SelectedIndex = -1;
            txtReviewer.Text = string.Empty;
            txtSemester.Text = string.Empty;
            txtYearAdmission.Text = string.Empty;
            txtDeptCode.Text = string.Empty;
        }

        // 디자이너 참조 이벤트 핸들러(빈 구현)
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void textBox18_TextChanged(object sender, EventArgs e) { }
        private void textBox21_TextChanged(object sender, EventArgs e) { }
        private void Searchstdidtxt_TextChanged(object sender, EventArgs e) { }
        private void GradeStidtxt_TextChanged(object sender, EventArgs e){}

        // 문의목록 답변 폼 5로 이동
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        // ============================================
        // 문의 관리 영역 (새로 추가)
        // ============================================

        /// <summary>
        /// 문의 관리 탭 초기 로드
        /// </summary>
        private void LoadInquiryManagementTab()
        {
            try
            {
                // 상태 필터 콤보박스 초기화 (comboBox1)
                if (comboBox1 != null && comboBox1.Items.Count == 0)
                {
                    comboBox1.Items.AddRange(new object[] { "전체", "대기중", "처리중", "답변완료" });
                    comboBox1.SelectedIndex = 0;
                }

                // ContextMenuStrip은 디자이너에서 이미 설정되어 있음
                // contextMenuStrip1의 "문의답변" 메뉴는 문의답변ToolStripMenuItem_Click과 연결됨

                // ListView에 ContextMenuStrip 연결 (디자이너에서 설정되어 있다면 생략 가능)
                var listView = this.Controls.Find("listViewInquiryManage", true).FirstOrDefault() as ListView;
                if (listView != null && listView.ContextMenuStrip == null)
                {
                    listView.ContextMenuStrip = contextMenuStrip1;
                }

                // 초기 문의 목록 로드
                LoadAllInquiries();
                LoadInquiryStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("문의 관리 탭 초기화 오류: " + ex.Message, "오류");
            }
        }

        /// <summary>
        /// 전체 문의 목록 로드
        /// </summary>
        private void LoadAllInquiries()
        {
            try
            {
                var listView = this.Controls.Find("listViewInquiryManage", true).FirstOrDefault() as ListView;
                if (listView == null)
                {
                    MessageBox.Show("문의 목록 ListView를 찾을 수 없습니다.", "오류");
                    return;
                }

                DataTable dt = _inquiry.GetAllInquiries();
                listView.Items.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        ListViewItem item = new ListViewItem(row["inquiry_id"].ToString());
                        item.SubItems.Add(row["student_id"].ToString());
                        item.SubItems.Add(row["student_name"].ToString());
                        item.SubItems.Add(row["category"].ToString());
                        item.SubItems.Add(row["title"].ToString());
                        item.SubItems.Add(row["inquiry_date"].ToString());
                        item.SubItems.Add(row["status"].ToString());

                        // 상태에 따라 색상 변경
                        string status = row["status"].ToString();
                        if (status == "대기중")
                            item.ForeColor = System.Drawing.Color.Red;
                        else if (status == "처리중")
                            item.ForeColor = System.Drawing.Color.Orange;
                        else if (status == "답변완료")
                            item.ForeColor = System.Drawing.Color.Green;

                        // Tag에 전체 데이터 저장 (Form5로 전달하기 위해)
                        item.Tag = row;

                        listView.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("문의 목록 로드 오류: " + ex.Message, "오류");
            }
        }

        /// <summary>
        /// 상태별 문의 필터링
        /// </summary>
        private void FilterInquiriesByStatus(string status)
        {
            try
            {
                var listView = this.Controls.Find("listViewInquiryManage", true).FirstOrDefault() as ListView;
                if (listView == null) return;

                DataTable dt;

                if (status == "전체")
                {
                    dt = _inquiry.GetAllInquiries();
                }
                else
                {
                    dt = _inquiry.GetInquiriesByStatus(status);
                }

                listView.Items.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        ListViewItem item = new ListViewItem(row["inquiry_id"].ToString());
                        item.SubItems.Add(row["student_id"].ToString());
                        item.SubItems.Add(row["student_name"].ToString());
                        item.SubItems.Add(row["category"].ToString());
                        item.SubItems.Add(row["title"].ToString());
                        item.SubItems.Add(row["inquiry_date"].ToString());
                        item.SubItems.Add(row["status"].ToString());

                        string rowStatus = row["status"].ToString();
                        if (rowStatus == "대기중")
                            item.ForeColor = System.Drawing.Color.Red;
                        else if (rowStatus == "처리중")
                            item.ForeColor = System.Drawing.Color.Orange;
                        else if (rowStatus == "답변완료")
                            item.ForeColor = System.Drawing.Color.Green;

                        item.Tag = row;
                        listView.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show($"'{status}' 상태의 문의가 없습니다.", "알림");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("필터링 오류: " + ex.Message, "오류");
            }
        }

        /// <summary>
        /// 문의 통계 로드 (대시보드)
        /// </summary>
        private void LoadInquiryStats()
        {
            try
            {
                // 대기중 건수
                int pendingCount = _inquiry.GetPendingInquiryCount();
                
                // 처리중 건수
                DataTable dtProcessing = _inquiry.GetInquiriesByStatus("처리중");
                int processingCount = dtProcessing != null ? dtProcessing.Rows.Count : 0;

                // 답변완료 건수
                DataTable dtCompleted = _inquiry.GetInquiriesByStatus("답변완료");
                int completedCount = dtCompleted != null ? dtCompleted.Rows.Count : 0;

                // 라벨 업데이트 (디자이너에서 생성한 라벨)
                var lblPending = this.Controls.Find("lblPendingCount", true).FirstOrDefault() as Label;
                var lblProcessing = this.Controls.Find("lblProcessingCount", true).FirstOrDefault() as Label;
                var lblCompleted = this.Controls.Find("lblCompletedCount", true).FirstOrDefault() as Label;

                if (lblPending != null) lblPending.Text = $"대기중: {pendingCount}건";
                if (lblProcessing != null) lblProcessing.Text = $"처리중: {processingCount}건";
                if (lblCompleted != null) lblCompleted.Text = $"답변완료: {completedCount}건";
            }
            catch (Exception ex)
            {
                MessageBox.Show("통계 로드 오류: " + ex.Message, "오류");
            }
        }

        /// <summary>
        /// ContextMenuStrip - "문의 답변" 클릭 시 Form5 열기
        /// </summary>
        private void MenuItemAnswer_Click(object sender, EventArgs e)
        {
            try
            {
                var listView = this.Controls.Find("listViewInquiryManage", true).FirstOrDefault() as ListView;
                if (listView == null || listView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("문의를 선택해주세요.", "선택 오류");
                    return;
                }

                // 선택된 항목에서 inquiry_id 추출
                ListViewItem selectedItem = listView.SelectedItems[0];
                string inquiryId = selectedItem.SubItems[0].Text; // 첫 번째 컬럼: inquiry_id

                // Form5 열기 (inquiry_id 전달)
                Form5 form5 = new Form5(inquiryId);
                DialogResult result = form5.ShowDialog();

                // Form5가 닫힌 후 목록 새로고침 (답변이 등록되었을 경우)
                if (result == DialogResult.OK)
                {
                    LoadAllInquiries();
                    LoadInquiryStats();
                    MessageBox.Show("답변이 처리되었습니다. 목록이 새로고침되었습니다.", "완료");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("문의 답변 폼 열기 오류: " + ex.Message + "\n\n" + ex.StackTrace, "오류");
            }
        }

        /// <summary>
        /// 문의 상태 변경
        /// </summary>
        private void ChangeInquiryStatus(string newStatus)
        {
            try
            {
                var listView = this.Controls.Find("listViewInquiryManage", true).FirstOrDefault() as ListView;
                if (listView == null || listView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("문의를 선택해주세요.", "선택 오류");
                    return;
                }

                string inquiryId = listView.SelectedItems[0].Text;
                string currentStatus = listView.SelectedItems[0].SubItems[6].Text;

                if (currentStatus == newStatus)
                {
                    MessageBox.Show($"이미 '{newStatus}' 상태입니다.", "알림");
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"문의 상태를 '{newStatus}'(으)로 변경하시겠습니까?",
                    "상태 변경 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes) return;

                bool success = _inquiry.UpdateStatus(inquiryId, newStatus);

                if (success)
                {
                    MessageBox.Show("상태가 변경되었습니다.", "성공");
                    LoadAllInquiries();
                    LoadInquiryStats();
                }
                else
                {
                    MessageBox.Show("상태 변경에 실패했습니다.", "오류");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("상태 변경 오류: " + ex.Message, "오류");
            }
        }

        /// <summary>
        /// 필터 버튼 클릭 이벤트 (button1) - comboBox1 사용
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1 != null && comboBox1.SelectedIndex >= 0)
            {
                string selectedStatus = comboBox1.SelectedItem.ToString();
                FilterInquiriesByStatus(selectedStatus);
            }
        }

        private void listViewInquiryManage_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ListView 선택 변경 시 필요한 로직 (선택사항)
        }

        /// <summary>
        /// ContextMenuStrip - "문의답변" 메뉴 클릭 이벤트
        /// </summary>
        private void 문의답변ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var listView = this.Controls.Find("listViewInquiryManage", true).FirstOrDefault() as ListView;
                if (listView == null || listView.SelectedItems.Count == 0)
                {
                    MessageBox.Show("문의를 선택해주세요.", "선택 오류");
                    return;
                }

                // 선택된 항목에서 inquiry_id 추출
                ListViewItem selectedItem = listView.SelectedItems[0];
                string inquiryId = selectedItem.SubItems[0].Text; // 첫 번째 컬럼: inquiry_id

                // Form5 열기 (inquiry_id 전달)
                Form5 form5 = new Form5(inquiryId);
                DialogResult result = form5.ShowDialog();

                // Form5가 닫힌 후 목록 새로고침 (답변이 등록되었을 경우)
                if (result == DialogResult.OK)
                {
                    LoadAllInquiries();
                    LoadInquiryStats();
                    MessageBox.Show("답변이 처리되었습니다. 목록이 새로고침되었습니다.", "완료");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("문의 답변 폼 열기 오류: " + ex.Message + "\n\n" + ex.StackTrace, "오류");
            }
        }
    }
}
