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
    public partial class Form5 : Form
    {
        private String _CourseNumber;
        public Form5(String courseNumber)
        {
            InitializeComponent();
            this._CourseNumber = courseNumber;
        }
        private void Form5_Load(object sender, EventArgs e)
        {
            // 폼 로드 시, 전달받은 PK로 DB 조회 및 출력 함수 호출
            LoadDetailedData();
        }

        private void LoadDetailedData()
        {/*
            string query = @"
        SELECT 
            C.*, 
            P.professor_name,
            D.department_name,
            L.lecture_time,
            L.lecture_location,
            L.syllabus
        FROM 
            Course C
        LEFT JOIN 
            Professor P ON C.professor_id = P.professor_id
        LEFT JOIN
            Department D ON C.department_code = D.department_code
        LEFT JOIN 
            LectureInfo L ON C.course_number = L.course_number
        WHERE 
            C.course_number = :pk";

            try
            {
                using (var db = new DBClass("syu", "1111"))
                {
                    OracleParameter pPK = new OracleParameter("pk", this._courseNumber);
                    DataTable dt = db.GetDataTable(query, pPK);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        // Form5 컨트롤에 데이터 매핑 (예시 Label/TextBox 이름 사용)

                        // A. 기본 과목 정보
                        lblDetailCode.Text = row["course_code"].ToString();
                        lblDetailName.Text = row["course_name"].ToString();
                        lblDetailCredit.Text = row["credit"].ToString();
                        lblDetailLevel.Text = row["grade_level"].ToString();
                        lblDetailType.Text = row["course_type"].ToString();

                        // B. JOIN을 통한 추가 정보
                        lblDetailProfessorName.Text = row["professor_name"] == DBNull.Value ? "정보 없음" : row["professor_name"].ToString();
                        lblDetailDeptName.Text = row["department_name"] == DBNull.Value ? "정보 없음" : row["department_name"].ToString();

                        // C. LectureInfo 상세 정보 (새로 추가된 부분)
                        lblDetailTime.Text = row["lecture_time"] == DBNull.Value ? "미정" : row["lecture_time"].ToString();
                        lblDetailLocation.Text = row["lecture_location"] == DBNull.Value ? "미정" : row["lecture_location"].ToString();
                        txtSyllabus.Text = row["syllabus"] == DBNull.Value ? "강의 계획서 없음" : row["syllabus"].ToString();

                        MessageBox.Show($"과목 '{row["course_name"]}'의 상세 정보 및 강의 정보를 로드했습니다.");
                    }
                    else
                    {
                        MessageBox.Show($"과목 번호 {_courseNumber}에 해당하는 상세 정보를 찾을 수 없습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("상세 데이터 로드 중 오류 발생: " + ex.Message, "DB 오류");
            }
            */
        }
    }
}
