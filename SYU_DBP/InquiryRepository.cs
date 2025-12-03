using Oracle.DataAccess.Client;
using System;
using System.Data;

namespace SYU_DBP
{
    /// <summary>
    /// 학생 문의/건의 게시판 데이터 접근 클래스
    /// </summary>
    public class InquiryRepository
    {
        private readonly DBClass _db;

        public InquiryRepository(DBClass db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// 특정 학생의 문의 목록 조회 (학생용)
        /// </summary>
        public DataTable GetInquiriesByStudent(string studentId)
        {
            string query = @"
                SELECT 
                    i.inquiry_id,
                    i.category,
                    i.title,
                    i.content,
                    TO_CHAR(i.inquiry_date, 'YYYY-MM-DD HH24:MI') AS inquiry_date,
                    i.status,
                    i.answer,
                    TO_CHAR(i.answer_date, 'YYYY-MM-DD HH24:MI') AS answer_date,
                    i.answerer
                FROM Inquiry i
                WHERE i.student_id = :sid
                ORDER BY i.inquiry_date DESC";

            OracleParameter param = new OracleParameter("sid", studentId);
            return _db.GetDataTable(query, param);
        }

        /// <summary>
        /// 전체 문의 목록 조회 (교직원용)
        /// </summary>
        public DataTable GetAllInquiries()
        {
            string query = @"
                SELECT 
                    i.inquiry_id,
                    i.student_id,
                    s.name AS student_name,
                    i.category,
                    i.title,
                    i.content,
                    TO_CHAR(i.inquiry_date, 'YYYY-MM-DD HH24:MI') AS inquiry_date,
                    i.status,
                    i.answer,
                    TO_CHAR(i.answer_date, 'YYYY-MM-DD HH24:MI') AS answer_date,
                    i.answerer
                FROM Inquiry i
                JOIN Student s ON i.student_id = s.student_id
                ORDER BY 
                    CASE i.status 
                        WHEN '대기중' THEN 1 
                        WHEN '처리중' THEN 2 
                        ELSE 3 
                    END,
                    i.inquiry_date DESC";

            return _db.GetDataTable(query);
        }

        /// <summary>
        /// 특정 상태의 문의 목록 조회 (교직원용 - 필터링)
        /// </summary>
        public DataTable GetInquiriesByStatus(string status)
        {
            string query = @"
                SELECT 
                    i.inquiry_id,
                    i.student_id,
                    s.name AS student_name,
                    i.category,
                    i.title,
                    i.content,
                    TO_CHAR(i.inquiry_date, 'YYYY-MM-DD HH24:MI') AS inquiry_date,
                    i.status,
                    i.answer,
                    TO_CHAR(i.answer_date, 'YYYY-MM-DD HH24:MI') AS answer_date,
                    i.answerer
                FROM Inquiry i
                JOIN Student s ON i.student_id = s.student_id
                WHERE i.status = :status
                ORDER BY i.inquiry_date DESC";

            OracleParameter param = new OracleParameter("status", status);
            return _db.GetDataTable(query, param);
        }

        /// <summary>
        /// 문의 ID로 단일 문의 상세 조회
        /// </summary>
        public DataTable GetInquiryById(string inquiryId)
        {
            string query = @"
                SELECT 
                    i.inquiry_id,
                    i.student_id,
                    s.name AS student_name,
                    i.category,
                    i.title,
                    i.content,
                    TO_CHAR(i.inquiry_date, 'YYYY-MM-DD HH24:MI') AS inquiry_date,
                    i.status,
                    i.answer,
                    TO_CHAR(i.answer_date, 'YYYY-MM-DD HH24:MI') AS answer_date,
                    i.answerer
                FROM Inquiry i
                JOIN Student s ON i.student_id = s.student_id
                WHERE i.inquiry_id = :iid";

            OracleParameter param = new OracleParameter("iid", inquiryId);
            return _db.GetDataTable(query, param);
        }

        /// <summary>
        /// 새 문의 작성 (학생용)
        /// </summary>
        public bool InsertInquiry(string studentId, string category, string title, string content)
        {
            // inquiry_id 생성: INQ_YYYY_순번
            string inquiryId = GenerateInquiryId();

            string query = @"
                INSERT INTO Inquiry (
                    inquiry_id, student_id, category, title, content, 
                    inquiry_date, status
                ) VALUES (
                    :iid, :sid, :cat, :title, :content, 
                    SYSDATE, '대기중'
                )";

            OracleParameter[] parameters = {
                new OracleParameter("iid", inquiryId),
                new OracleParameter("sid", studentId),
                new OracleParameter("cat", category),
                new OracleParameter("title", title),
                new OracleParameter("content", content)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// 문의 수정 (학생용 - 답변 대기 중일 때만 가능)
        /// </summary>
        public bool UpdateInquiry(string inquiryId, string category, string title, string content)
        {
            string query = @"
                UPDATE Inquiry 
                SET category = :cat, 
                    title = :title, 
                    content = :content
                WHERE inquiry_id = :iid 
                  AND status = '대기중'";

            OracleParameter[] parameters = {
                new OracleParameter("cat", category),
                new OracleParameter("title", title),
                new OracleParameter("content", content),
                new OracleParameter("iid", inquiryId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// 문의 삭제 (학생용 - 답변 대기 중일 때만 가능)
        /// </summary>
        public bool DeleteInquiry(string inquiryId)
        {
            string query = @"
                DELETE FROM Inquiry 
                WHERE inquiry_id = :iid 
                  AND status = '대기중'";

            OracleParameter param = new OracleParameter("iid", inquiryId);
            int rowsAffected = _db.ExecuteNonQuery(query, param);
            return rowsAffected > 0;
        }

        /// <summary>
        /// 답변 등록 (교직원용)
        /// </summary>
        public bool UpdateAnswer(string inquiryId, string answer, string answerer)
        {
            string query = @"
                UPDATE Inquiry 
                SET answer = :ans, 
                    answer_date = SYSDATE, 
                    answerer = :answerer,
                    status = '답변완료'
                WHERE inquiry_id = :iid";

            OracleParameter[] parameters = {
                new OracleParameter("ans", answer),
                new OracleParameter("answerer", answerer),
                new OracleParameter("iid", inquiryId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// 문의 상태 변경 (교직원용)
        /// </summary>
        public bool UpdateStatus(string inquiryId, string status)
        {
            string query = @"
                UPDATE Inquiry 
                SET status = :status
                WHERE inquiry_id = :iid";

            OracleParameter[] parameters = {
                new OracleParameter("status", status),
                new OracleParameter("iid", inquiryId)
            };

            int rowsAffected = _db.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// 답변 삭제 및 상태 변경 (교직원용 - 답변 철회)
        /// </summary>
        public bool DeleteAnswer(string inquiryId)
        {
            string query = @"
                UPDATE Inquiry 
                SET answer = NULL, 
                    answer_date = NULL, 
                    answerer = NULL,
                    status = '대기중'
                WHERE inquiry_id = :iid";

            OracleParameter param = new OracleParameter("iid", inquiryId);
            int rowsAffected = _db.ExecuteNonQuery(query, param);
            return rowsAffected > 0;
        }

        /// <summary>
        /// 대기 중인 문의 개수 조회 (교직원용 - 대시보드)
        /// </summary>
        public int GetPendingInquiryCount()
        {
            string query = "SELECT COUNT(*) FROM Inquiry WHERE status = '대기중'";
            object result = _db.ExecuteScalar(query);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// 특정 학생의 문의 개수 조회
        /// </summary>
        public int GetInquiryCountByStudent(string studentId)
        {
            string query = "SELECT COUNT(*) FROM Inquiry WHERE student_id = :sid";
            OracleParameter param = new OracleParameter("sid", studentId);
            object result = _db.ExecuteScalar(query, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// 문의 ID 자동 생성 (INQ_YYYY_00001 형식)
        /// </summary>
        private string GenerateInquiryId()
        {
            string year = DateTime.Now.Year.ToString();
            string query = @"
                SELECT NVL(MAX(TO_NUMBER(SUBSTR(inquiry_id, 10))), 0) + 1 AS next_num
                FROM Inquiry 
                WHERE SUBSTR(inquiry_id, 5, 4) = :year";

            OracleParameter param = new OracleParameter("year", year);
            object result = _db.ExecuteScalar(query, param);
            
            int nextNum = result != null ? Convert.ToInt32(result) : 1;
            return $"INQ_{year}_{nextNum:D5}";
        }

        /// <summary>
        /// 카테고리별 문의 통계 (교직원용 - 대시보드)
        /// </summary>
        public DataTable GetInquiryStatsByCategory()
        {
            string query = @"
                SELECT 
                    category,
                    COUNT(*) AS total_count,
                    SUM(CASE WHEN status = '대기중' THEN 1 ELSE 0 END) AS pending_count,
                    SUM(CASE WHEN status = '답변완료' THEN 1 ELSE 0 END) AS completed_count
                FROM Inquiry
                GROUP BY category
                ORDER BY category";

            return _db.GetDataTable(query);
        }
    }
}
