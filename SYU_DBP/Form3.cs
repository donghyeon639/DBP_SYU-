
using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SYU_DBP
{
    public partial class Form3 : Form
    {
        private string _studentId;

        private bool isTab1Loaded = false; // 개인정보
        private bool isTab2Loaded = false; // 수강/성적
        private bool isTab3Loaded = false; // 졸업현황
        private bool isTab4Loaded = false; // 졸업심사 현황
        private bool isTab5Loaded = false;

        private DataTable cachedCourseResults = null;     // 검색 결과를 저장할 메모리 테이블
        private string selectedCourseNumber = string.Empty; // 선택된 과목의 PK 저장

        public Form3(string studentId)
        {
            InitializeComponent();
            _studentId = studentId;
        }


        private void Form3_Load(object sender, EventArgs e)
        {
            // 1. 첫 번째 탭의 데이터만 바로 로드합니다.
            LoadPersonalInfo();

            // 2. 이미 로드했으므로 플래그를 true로 설정합니다.
            isTab1Loaded = true;
        }



        private void UpdatePersonalInfoBtn_Click(object sender, EventArgs e)
        {
            var Form4 = new Form4(_studentId);
            Form4.Show();
        }

       

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0: // tabPage1: 개인정보
                    if (!isTab1Loaded)
                    {
                        LoadPersonalInfo();
                        isTab1Loaded = true;
                    }
                    break;

                case 1: // tabPage2: 수강/성적
                    if (!isTab2Loaded)
                    {
                        LoadGradesAndStats();
                        isTab2Loaded = true;
                    }
                    break;

                case 2: // tabPage3: 졸업현황
                    if (!isTab5Loaded)
                    {
                        LoadCourseSearchTab();
                        isTab5Loaded = true;
                    }
                    break;

                case 3: // tabPage4: 졸업심사 현황
                    if (!isTab4Loaded)
                    {
                        LoadGraduationSimulation();
                        isTab4Loaded = true;
                    }
                    break;
            }
        }

        private void LoadPersonalInfo()
        {


            // 모든 학생 정보가 있다고 가정한 쿼리
            // 실제 DB 테이블명과 컬럼명에 맞게 수정이 필요합니다.
            string query = @"
    SELECT 
        s.STUDENT_ID,
        s.NAME,
        s.ACADEMIC_STATUS,
        (SELECT d.DEPARTMENT_NAME 
         FROM DEPARTMENT d 
         WHERE d.DEPARTMENT_CODE = s.DEPARTMENT_CODE) AS DEPARTMENT_NAME,
        s.GRADE,
        s.MAJOR,
        p.EMAIL,
        p.ADDRESS,
        p.PHONE_NUMBER
    FROM STUDENT s
    JOIN PERSONALINFO p
        ON s.STUDENT_ID = p.STUDENT_ID
    WHERE s.STUDENT_ID = :p_id";
            // 매개변수 사용

            try
            {
                // 1. DBClass 인스턴스 생성 (본인 환경에 맞게 ID/PW 수정)
                using (DBClass db = new DBClass("syu", "1111"))
                {
                    // 2. 매개변수 생성 및 값 할당
                    OracleParameter pId = new OracleParameter("p_id", OracleDbType.Varchar2, 20);
                    pId.Value = _studentId;

                    // 3. 쿼리 실행 및 DataTable 가져오기
                    DataTable dt = db.GetDataTable(query, pId);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0]; // 결과는 한 명의 학생이므로 첫 번째 행만 가져옴

                        // 4. Form3의 Label 컨트롤에 데이터 매핑
                        // (Label 이름은 디자이너 코드에서 확인된 이름입니다.)
                        lblstudentId.Text = row["STUDENT_ID"].ToString();
                        lblname.Text = row["NAME"].ToString();
                        lbldepartment.Text = row["DEPARTMENT_NAME"].ToString();
                        lblgrade.Text = row["GRADE"].ToString();
                        lblmajor.Text = row["MAJOR"].ToString();
                        lblemail.Text = row["EMAIL"].ToString();
                        lblphone.Text = row["PHONE_NUMBER"].ToString();
                        lbladdress.Text = row["ADDRESS"].ToString();
                        //lblbirth.Text = Convert.ToDateTime(row["BIRTH_DATE"]).ToString("yyyy-MM-dd");

                        // 디버깅 용
                        MessageBox.Show($"학생 ID {_studentId}의 정보를 성공적으로 로드했습니다.", "로드 완료");
                    }
                    else
                    {
                        MessageBox.Show($"학생 ID {_studentId}의 데이터를 찾을 수 없습니다. DB를 확인하세요.", "데이터 없음");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("개인 정보 로드 중 오류 발생: " + ex.Message, "DB 오류");
            }
        }
        private void LoadGradesAndStats()
        {
            // ... (통계 쿼리용 매개변수 및 통계 쿼리는 별도로 작성해야 합니다.)
            OracleParameter pIdDetail = new OracleParameter("sid", _studentId);

            // 1. 상세 과목 목록 쿼리 (ListBox 채우기) - 6개 항목 포함
            string detailQuery = @"
        SELECT 
            C.course_code,      -- 과목코드
            C.course_name,      -- 과목명
            G.credits,          -- 학점
            G.grade_point,      -- 성적 (점수)
            G.semester,         -- 수강학기
            C.general_area      -- 교양영역
        FROM
            Grade G
        JOIN 
            Course C ON G.course_number = C.course_number
        WHERE
            G.student_id = :sid
        ORDER BY
            G.semester DESC, C.course_code";


            // 2. 통계 정보 쿼리 (상단 패널 채우기) - SUM, AVG, COUNT
            // [Credit(학점)의 합계, Grade_point(성적)의 평균, 과목 수(COUNT)]를 계산합니다.
            OracleParameter pIdStat = new OracleParameter("sid", _studentId);
            string statQuery = @"
        SELECT 
            SUM(G.credits) AS TOTAL_CREDITS,            -- 총 이수학점
            AVG(G.grade_point) AS AVERAGE_GRADE,        -- 평균 학점
            COUNT(G.course_number) AS COURSE_COUNT      -- 수강 과목 수
        FROM 
            Grade G
        WHERE 
            G.student_id = :sid";

            try
            {
                using (var db = new DBClass("syu", "1111"))
                {
                    // A. 통계 데이터 로드 및 출력 (추가된 부분)
                    DataTable dtStats = db.GetDataTable(statQuery, pIdStat);

                    if (dtStats != null && dtStats.Rows.Count > 0)
                    {
                        DataRow stats = dtStats.Rows[0];

                        // DBNull 체크: 수강 기록이 없는 경우를 대비 (SUM, AVG는 NULL 반환 가능)
                        // 만약 NULL이면 0으로 처리합니다.
                        decimal totalCr = stats["TOTAL_CREDITS"] != DBNull.Value ? Convert.ToDecimal(stats["TOTAL_CREDITS"]) : 0m;
                        decimal avgGr = stats["AVERAGE_GRADE"] != DBNull.Value ? Convert.ToDecimal(stats["AVERAGE_GRADE"]) : 0m;
                        int courseCnt = stats["COURSE_COUNT"] != DBNull.Value ? Convert.ToInt32(stats["COURSE_COUNT"]) : 0;

                        // 상단 라벨에 값 매핑 (디자이너 분석 기반)
                        total.Text = totalCr.ToString("F1");              // 총 이수학점 (예: 60.0)
                        average.Text = avgGr.ToString("F2");                // 평균 학점 (예: 3.85)
                        courseNumber.Text = courseCnt.ToString();           // 수강 과목 수
                    }
                    else
                    {
                        // 데이터가 없을 때 기본값 설정 (수강 기록이 없는 경우)
                        total.Text = "0.0";
                        average.Text = "0.00";
                        courseNumber.Text = "0";
                    }

                    // B. 상세 데이터 로드 및 ListBox 출력
                    DataTable dtDetails = db.GetDataTable(detailQuery, pIdDetail);
                    listBox1.Items.Clear();

                    if (dtDetails != null && dtDetails.Rows.Count > 0)
                    {
                        // 헤더 출력
                        string header = string.Format("{0,-12}{1,-25}{2,8}{3,8}{4,12}{5,15}",
                                                      "코드", "과목명", "학점", "성적", "수강학기", "영역");
                        listBox1.Items.Add(header);
                        listBox1.Items.Add(new string('=', 70));

                        foreach (DataRow row in dtDetails.Rows)
                        {
                            // 성적은 소수점 둘째 자리까지 포맷팅 (DECIMAL(3,2)이므로)
                            string gradePoint = Convert.ToDecimal(row["grade_point"]).ToString("F2");

                            string listItem = string.Format("{0,-12}{1,-25}{2,8}{3,8}{4,12}{5,15}",
                                row["course_code"],
                                row["course_name"],
                                row["credits"],
                                gradePoint,
                                row["semester"],
                                row["general_area"]
                            );
                            listBox1.Items.Add(listItem);
                        }
                    }
                    else
                    {
                        listBox1.Items.Add("수강 기록이 없습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("수강 정보 로드 중 오류 발생: " + ex.Message, "DB 오류");
            }
        }
        private void LoadCourseSearchTab()
        {


            // 2. ComboBox 초기값 설정
            // grade_level_txt와 course_typetxt는 드롭다운 리스트가 이미 채워져 있으므로,
            // 초기 선택 항목만 설정합니다. (선택: 전체)
            grade_level_txt.Items.Insert(0, "전체");
            course_typetxt.Items.Insert(0, "전체");
            grade_level_txt.SelectedIndex = 0;
            course_typetxt.SelectedIndex = 0;

            // 3. ListView 초기 데이터 로드 (선택 사항: 탭 열릴 때 전체 과목 리스트를 보여주려면)
            // SearchBtn_Click(null, null); // 초기 검색 실행 (검색어 없이 전체 과목 로드)

            // 4. 이벤트 핸들러 연결 (디자이너에서 연결되지 않았다면 여기에 추가)
            SearchBtn.Click += new EventHandler(SearchBtn_Click);
            Coursecode_PrintBtn.Click += new EventHandler(Coursecode_PrintBtn_Click);
            ResetBtn.Click += new EventHandler(ResetBtn_Click);

        }


        private void ResetBtn_Click(object sender, EventArgs e)
        {
            // TextBoxes 비우기
            Courseinfono.Clear();
            Courinfocodetxt.Clear();
            Courinfonametxt.Clear();
            DepartmentCodetxt.Clear();
            Profeidtxt.Clear();

            // ComboBox 선택 초기화
            grade_level_txt.SelectedIndex = 0; // "전체"
            course_typetxt.SelectedIndex = 0;  // "전체"

            // ListView 및 캐시 비우기 (선택 사항)
            cachedCourseResults = null;
            selectedCourseNumber = string.Empty;
        }

        private void Coursecode_PrintBtn_Click(object sender, EventArgs e)
        {
            // 1. 현재 선택된 행이 있는지 확인
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Index < 0)
            {
                MessageBox.Show("목록에서 과목을 먼저 선택해 주세요.", "선택 오류");
                return;
            }

            try
            {
                // 2. ★PK 값 추출 (선택된 행의 'course_number' 컬럼 값)★
                // DataGridView는 컬럼 이름으로 값에 접근할 수 있어 매우 편리합니다.
                selectedCourseNumber = dataGridView1.CurrentRow.Cells["course_number"].Value.ToString();

                // 3. 폼 5 호출 (이전 로직 유지)
                Form5 newForm = new Form5(selectedCourseNumber);
                newForm.Show();
            }
            catch (Exception ex)
            {
                // 컬럼 이름을 찾지 못했거나 값이 null일 경우 오류 처리
                MessageBox.Show("상세 폼 로드 중 오류 발생: " + ex.Message, "오류");
            }
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            // 1. 동적 WHERE 절 생성
            string whereClause = " 1 = 1 "; // 항상 true인 기본 조건

            // TextBox 값 가져오기
            string courseNo = Courseinfono.Text.Trim(); // Courseinfono (학점)
            string courseCode = Courinfocodetxt.Text.Trim();
            string courseName = Courinfonametxt.Text.Trim();
            string deptCode = DepartmentCodetxt.Text.Trim();
            string professorId = Profeidtxt.Text.Trim();

            // ComboBox 값 가져오기
            string courseType = course_typetxt.Text;
            string gradeLevel = grade_level_txt.Text;

            // 2. 검색 조건 추가 (빈 값이 아니거나 "전체"가 아닐 때)
            if (!string.IsNullOrEmpty(courseNo)) whereClause += $" AND CREDIT = {courseNo}";
            if (!string.IsNullOrEmpty(courseCode)) whereClause += $" AND COURSE_CODE LIKE '%{courseCode}%'";
            if (!string.IsNullOrEmpty(courseName)) whereClause += $" AND COURSE_NAME LIKE '%{courseName}%'";
            if (!string.IsNullOrEmpty(deptCode)) whereClause += $" AND DEPARTMENT_CODE = '{deptCode}'";
            if (!string.IsNullOrEmpty(professorId)) whereClause += $" AND PROFESSOR_ID = '{professorId}'";

            if (courseType != "전체" && !string.IsNullOrEmpty(courseType)) whereClause += $" AND COURSE_TYPE = '{courseType}'";
            if (gradeLevel != "전체" && !string.IsNullOrEmpty(gradeLevel)) whereClause += $" AND GRADE_LEVEL = {gradeLevel}";

            // 3. SQL 쿼리 구성
            string query = $@"
    SELECT 
        C.course_code, C.course_name, C.course_type, C.grade_level, 
        C.credit, P.professor_name,
        D.department_name
    FROM 
        Course C
    LEFT JOIN 
        Department D ON C.department_code = D.department_code
    LEFT JOIN 
        Professor P ON C.professor_id = P.professor_id
    WHERE 
        {whereClause} 
    ORDER BY 
        C.course_code";

            try
            {
                using (var db = new DBClass("syu", "1111"))
                {
                    // ★핵심 변경: DB에서 결과 가져와서 메모리에 저장
                    cachedCourseResults = db.GetDataTable(query);

                    // ★★★ ListView 루프가 사라지고 단 한 줄로 대체됨 ★★★
                    dataGridView1.DataSource = cachedCourseResults;
                    SetDGVHeaderNames();

                    if (cachedCourseResults == null || cachedCourseResults.Rows.Count == 0)
                    {
                        MessageBox.Show("검색 조건에 맞는 과목이 없습니다.");
                    }

                    // 옵션: DataGridView의 컬럼 헤더 이름이 DB 컬럼명과 다를 경우 수동 매핑 필요
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("검색 중 오류 발생: " + ex.Message, "DB 오류");
            }
        }
        private void SetDGVHeaderNames()
        {
            // DataGridView에 데이터가 바인딩된 후에만 실행합니다.
            if (dataGridView1.DataSource == null) return;

            // 컬럼 순서나 이름이 바뀌더라도 안전하게 접근하기 위해 Try-Catch 사용
            try
            {
                // 1. 필요한 컬럼 헤더 이름 변경

                // C#에서는 DB 컬럼명 (대소문자 구분 없이)으로 컬럼에 접근합니다.
                // DGV는 DB에서 가져온 컬럼명을 내부적으로 사용합니다.

                dataGridView1.Columns["course_code"].HeaderText = "과목 코드";
                dataGridView1.Columns["course_name"].HeaderText = "과목명";
                dataGridView1.Columns["course_type"].HeaderText = "이수 구분";
                dataGridView1.Columns["grade_level"].HeaderText = "수강 학년";
                dataGridView1.Columns["credit"].HeaderText = "이수 학점";
                dataGridView1.Columns["professor_name"].HeaderText = "담당 교수 ID";
                dataGridView1.Columns["department_name"].HeaderText = "학과명";

                // 2. (옵션) 사용자에게 보이지 않게 할 컬럼 숨기기
                // 예: 내부적으로 필요한 PK이지만 사용자에게는 보여줄 필요 없는 컬럼
                // dataGridView1.Columns["course_number"].Visible = false;

                // 3. (옵션) 모든 컬럼이 DataGridView 크기에 맞게 자동으로 채워지도록 설정
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            }
            catch (Exception ex)
            {
                // 컬럼 이름을 찾지 못했거나 오류가 발생할 경우 (예: DataGridView에 아직 데이터가 없는 경우)
                // MessageBox.Show("컬럼 헤더 설정 오류: " + ex.Message);
            }
        }
        
        private void LoadGraduationSimulation()
        {
            // ====================================================
            // A. 변수 초기화 및 학번/학과 코드 추출
            // ====================================================

            // 요구사항 변수 (GraduationRequirement 테이블)
            int requiredTotal = 0;
            int requiredMajor = 0;
            int requiredGeneral = 0;
            int requiredChapel = 0;
            int requiredEssentialGeneral = 0; // 필수 교양 요구 학점 (INT 타입으로 최종 변경됨)

            // 이수 현황 변수 (Grade, Course 테이블)
            int earnedTotal = 0;
            int earnedMajor = 0;
            int earnedGeneral = 0;
            int earnedChapel = 0;
            int earnedEssentialGeneral = 0; // 필수 교양 취득 학점 (현재 쿼리상 0으로 초기화)

            // 학과 코드 및 입학년도 변수
            string studentDeptCode = string.Empty;
            string admissionYear = _studentId.Substring(0, 4); // 학번 앞 4자리 추출

            // ----------------------------------------------------
            // B. ListView 설정 (View.Details 및 컬럼 정의)
            // ----------------------------------------------------
            // 주의: Columns.Clear()를 호출하지 않으면 탭 로드 시 컬럼이 중복 추가됩니다.
            listViewGradStatus.Clear();
            listViewGradStatus.View = View.Details;
            listViewGradStatus.FullRowSelect = true;
            listViewGradStatus.GridLines = true;

            // 컬럼 정의 (수평 구조에 맞춤)
            if (listViewGradStatus.Columns.Count == 0)
            {
                listViewGradStatus.Columns.Add("구분", 120, HorizontalAlignment.Left);
                listViewGradStatus.Columns.Add("총 이수 학점", 120, HorizontalAlignment.Center);
                listViewGradStatus.Columns.Add("전공 학점", 120, HorizontalAlignment.Center);
                listViewGradStatus.Columns.Add("교양 학점", 120, HorizontalAlignment.Center);
                listViewGradStatus.Columns.Add("필수교양 학점", 120, HorizontalAlignment.Center);
                listViewGradStatus.Columns.Add("채플 횟수", 120, HorizontalAlignment.Center);
            }

            try
            {
                using (var db = new DBClass("syu", "1111"))
                {
                    // ----------------------------------------------------
                    // C. DB 조회: 학과 코드 확보
                    // ----------------------------------------------------
                    string deptQuery = "SELECT DEPARTMENT_CODE FROM STUDENT WHERE STUDENT_ID = :sid";
                    OracleParameter pSidDept = new OracleParameter("sid", _studentId);
                    object deptResult = db.ExecuteScalar(deptQuery, pSidDept);

                    if (deptResult != null)
                    {
                        studentDeptCode = deptResult.ToString();
                    }
                    else
                    {
                        MessageBox.Show("학생의 학과 코드를 찾을 수 없습니다.", "오류");
                        return;
                    }

                    // ----------------------------------------------------
                    // D. DB 조회: 요구사항(Requirement) 가져오기
                    // ----------------------------------------------------
                    string reqQuery = @"
                SELECT
    GRADUATION_ID,
    ADMISSION_YEAR,
    DEPARTMENT_CODE,
    EARNED_CREDITS,      
    GENERAL_CREDITS,      
    MAJOR_CREDITS,        
    MATERIAL_COMPLETION_COUNT, 
    REQUIRED_GENERAL_DETAILS
FROM
    GraduationRequirement
WHERE
    ADMISSION_YEAR = :year
    AND DEPARTMENT_CODE = :dept";

                    OracleParameter pYear = new OracleParameter("year", admissionYear);
                    OracleParameter pDept = new OracleParameter("dept", studentDeptCode);
                    DataTable dtReq = db.GetDataTable(reqQuery, pYear, pDept);

                    if (dtReq != null && dtReq.Rows.Count > 0)
                    {
                        requiredTotal = Convert.ToInt32(dtReq.Rows[0]["earned_credits"]);
                        requiredMajor = Convert.ToInt32(dtReq.Rows[0]["major_credits"]);
                        requiredGeneral = Convert.ToInt32(dtReq.Rows[0]["general_credits"]);
                        requiredChapel = Convert.ToInt32(dtReq.Rows[0]["material_completion_count"]);
                        requiredEssentialGeneral = Convert.ToInt32(dtReq.Rows[0]["REQUIRED_GENERAL_DETAILS"]);
                    }
                    /*
                    // ----------------------------------------------------
                    // E. DB 조회: 학생 이수 현황(Progress) 계산
                    // ----------------------------------------------------
                    string progQuery = @"
    SELECT 
        COALESCE(SUM(G.credits), 0) AS EARNED_TOTAL, 
        COALESCE(SUM(CASE WHEN C.course_type IN ('전공필수', '전공선택') THEN G.credits ELSE 0 END), 0) AS EARNED_MAJOR,
        COALESCE(SUM(CASE WHEN C.course_type IN ('교양필수', '교양선택') THEN G.credits ELSE 0 END), 0) AS EARNED_GENERAL,
        COALESCE(SUM(CASE WHEN C.course_type = '교양필수' THEN G.credits ELSE 0 END), 0) AS EARNED_ESSENTIAL_GENERAL,
        (SELECT COUNT(*) FROM ChapelCompletion WHERE STUDENT_ID = :sid) AS EARNED_CHAPEL 
    FROM Grade G
    JOIN Course C ON G.course_number = C.course_number
    WHERE G.student_id = :sid";

                    OracleParameter pSidProg = new OracleParameter("sid", _studentId);
                    DataTable dtProg = db.GetDataTable(progQuery, pSidProg);

                    if (dtProg != null && dtProg.Rows.Count > 0 && dtProg.Rows[0]["EARNED_TOTAL"] != DBNull.Value)
                    {
                        earnedTotal = Convert.ToInt32(dtProg.Rows[0]["EARNED_TOTAL"]);
                        earnedMajor = Convert.ToInt32(dtProg.Rows[0]["EARNED_MAJOR"]);
                        earnedGeneral = Convert.ToInt32(dtProg.Rows[0]["EARNED_GENERAL"]);
                        earnedChapel = Convert.ToInt32(dtProg.Rows[0]["EARNED_CHAPEL"]);
                        earnedEssentialGeneral = Convert.ToInt32(dtReq.Rows[0]["EARNED_GENERAL_DETAILS"]);

                        // earnedEssentialGeneral은 현재 복잡한 쿼리가 필요하므로 0으로 유지
                    }
                    */
                    // ----------------------------------------------------
                    // F. ListView에 수평 구조로 출력
                    // ----------------------------------------------------
                    
                    // Row 1: 졸업 요구사항 (Required) 출력
                    ListViewItem itemReq = new ListViewItem("졸업 요구");
                    itemReq.SubItems.Add(requiredTotal.ToString());     // 총 이수 학점
                    itemReq.SubItems.Add(requiredMajor.ToString());     // 전공 학점
                    itemReq.SubItems.Add(requiredGeneral.ToString());   // 교양 학점
                    itemReq.SubItems.Add(requiredEssentialGeneral.ToString()); // 필수 교양 학점 (INT)
                    itemReq.SubItems.Add(requiredChapel.ToString());    // 채플 횟수
                    listViewGradStatus.Items.Add(itemReq);

                    /*
                    // Row 2: 학생 취득 현황 (Earned) 출력
                    ListViewItem itemEarned = new ListViewItem("취득 현황");
                    itemEarned.SubItems.Add(earnedTotal.ToString());     // 총 이수 학점
                    itemEarned.SubItems.Add(earnedMajor.ToString());     // 전공 학점
                    itemEarned.SubItems.Add(earnedGeneral.ToString());   // 교양 학점
                    itemEarned.SubItems.Add(earnedEssentialGeneral.ToString()); // 필수 교양 (0 또는 실제 취득 값)
                    itemEarned.SubItems.Add(earnedChapel.ToString());    // 채플 횟수
                    listViewGradStatus.Items.Add(itemEarned);
                    */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("졸업 현황 로드 중 오류 발생: " + ex.Message, "DB 오류");
            }
        }

    }
}


