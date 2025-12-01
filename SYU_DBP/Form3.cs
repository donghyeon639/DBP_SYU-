
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

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

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
                    if (!isTab3Loaded)
                    {
                        //LoadGraduationStatus();
                        isTab3Loaded = true;
                    }
                    break;

                case 3: // tabPage4: 졸업심사 현황
                    if (!isTab4Loaded)
                    {
                        //LoadGraduationReview();
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
    }
}


