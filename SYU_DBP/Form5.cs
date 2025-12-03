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
        private string _inquiryId; // 문의 ID
        private InquiryRepository _inquiryRepo;

        // 기존 생성자 (하위 호환성 유지)
        public Form5(string inquiryId)
        {
            InitializeComponent();
            this._inquiryId = inquiryId;
        }

        private void Form5_Load_1(object sender, EventArgs e)
        {
            LoadInquiryDetail();
        }

        /// <summary>
        /// 문의 상세 정보 로드
        /// </summary>
        private void LoadInquiryDetail()
        {
            try
            {
                using (var db = new DBClass("syu", "1111"))
                {
                    _inquiryRepo = new InquiryRepository(db);
                    DataTable dt = _inquiryRepo.GetInquiryById(_inquiryId);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];

                        // 문의 정보 표시
                        txtInquiryer.Text = $"{row["student_name"]} ({row["student_id"]})";
                        Inquiry_context_txt.Text = row["content"].ToString();
                        
                        // 추가 정보 (다른 TextBox가 있다면)
                        SetTextBoxValue("txtAnswerInquiryId", row["inquiry_id"].ToString());
                        SetTextBoxValue("txtAnswerTitle", row["title"].ToString());

                        // 카테고리와 상태 정보 표시 (Label이 있다면)
                        SetLabelValue("lblInquiryCategory", $"유형: {row["category"]}");
                        SetLabelValue("lblInquiryStatus", $"상태: {row["status"]}");
                        SetLabelValue("lblInquiryDate", $"작성일: {row["inquiry_date"]}");

                        // 이미 답변이 있는 경우 답변 내용도 표시
                        if (row["status"].ToString() == "답변완료" && row["answer"] != DBNull.Value)
                        {
                            SetTextBoxValue("txtAnswerer", row["answerer"]?.ToString() ?? "");
                            SetTextBoxValue("txtAnswer", row["answer"]?.ToString() ?? "");
                            
                            // 답변 날짜 표시
                            if (row["answer_date"] != DBNull.Value)
                            {
                                SetLabelValue("lblAnswerDate", $"답변일: {row["answer_date"]}");
                            }
                            
                            MessageBox.Show("이미 답변이 완료된 문의입니다.\n답변을 수정하거나 철회할 수 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            // 답변 대기중일 때
                            SetLabelValue("lblAnswerDate", "답변일: (미등록)");
                        }

                        // 폼 제목 설정
                        this.Text = $"문의 답변 - {row["inquiry_id"]}";
                    }
                    else
                    {
                        MessageBox.Show("문의 정보를 찾을 수 없습니다.", "오류");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("문의 정보 로드 오류: " + ex.Message + "\n\n" + ex.StackTrace, "오류");
            }
        }

        /// <summary>
        /// 답변 등록 버튼 클릭
        /// </summary>
        private void btnSubmitAnswer_Click(object sender, EventArgs e)
        {
            try
            {
                // 답변자 이름
                string answerer = GetTextBoxValue("txtAnswerer");
                if (string.IsNullOrWhiteSpace(answerer))
                {
                    MessageBox.Show("답변자 이름을 입력해주세요.", "입력 오류");
                    FocusControl("txtAnswerer");
                    return;
                }

                // 답변 내용
                string answer = GetTextBoxValue("txtAnswer");
                if (string.IsNullOrWhiteSpace(answer))
                {
                    MessageBox.Show("답변 내용을 입력해주세요.", "입력 오류");
                    FocusControl("txtAnswer");
                    return;
                }

                // 확인 메시지
                DialogResult result = MessageBox.Show(
                    "답변을 등록하시겠습니까?\n\n답변 등록 시 문의 상태가 '답변완료'로 변경됩니다.",
                    "답변 등록 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes) return;

                // 답변 등록
                using (var db = new DBClass("syu", "1111"))
                {
                    _inquiryRepo = new InquiryRepository(db);
                    bool success = _inquiryRepo.UpdateAnswer(_inquiryId, answer, answerer);

                    if (success)
                    {
                        MessageBox.Show("답변이 성공적으로 등록되었습니다.", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("답변 등록에 실패했습니다.", "오류");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("답변 등록 오류: " + ex.Message, "오류");
            }
        }

        /// <summary>
        /// 처리중으로 변경 버튼 클릭
        /// </summary>
        public void BtnMarkProcessing_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "문의 상태를 '처리중'으로 변경하시겠습니까?",
                    "상태 변경 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes) return;

                using (var db = new DBClass("syu", "1111"))
                {
                    _inquiryRepo = new InquiryRepository(db);
                    bool success = _inquiryRepo.UpdateStatus(_inquiryId, "처리중");

                    if (success)
                    {
                        MessageBox.Show("상태가 '처리중'으로 변경되었습니다.", "성공");
                        LoadInquiryDetail(); // 화면 갱신
                    }
                    else
                    {
                        MessageBox.Show("상태 변경에 실패했습니다.", "오류");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("상태 변경 오류: " + ex.Message, "오류");
            }
        }

        /// <summary>
        /// 답변 철회 버튼 클릭
        /// </summary>
        public void BtnDeleteAnswer_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(
                    "답변을 철회하시겠습니까?\n\n답변이 삭제되고 문의 상태가 '대기중'으로 변경됩니다.",
                    "답변 철회 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result != DialogResult.Yes) return;

                using (var db = new DBClass("syu", "1111"))
                {
                    _inquiryRepo = new InquiryRepository(db);
                    bool success = _inquiryRepo.DeleteAnswer(_inquiryId);

                    if (success)
                    {
                        MessageBox.Show("답변이 철회되었습니다.", "성공");
                        
                        // 입력 필드 초기화
                        SetTextBoxValue("txtAnswerer", "");
                        SetTextBoxValue("txtAnswer", "");
                        
                        LoadInquiryDetail(); // 화면 갱신
                    }
                    else
                    {
                        MessageBox.Show("답변 철회에 실패했습니다.", "오류");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("답변 철회 오류: " + ex.Message, "오류");
            }
        }

        // ============================================
        // 헬퍼 메서드 (컨트롤 안전 접근)
        // ============================================

        private void SetTextBoxValue(string controlName, string value)
        {
            // 모든 컨트롤을 재귀적으로 검색
            Control control = FindControlRecursive(this, controlName);
            
            if (control is TextBox textBox)
            {
                textBox.Text = value;
            }
            else if (control != null)
            {
                // TextBox가 아닌 다른 컨트롤일 경우에도 Text 속성 설정 시도
                control.Text = value;
            }
        }

        private string GetTextBoxValue(string controlName)
        {
            Control control = FindControlRecursive(this, controlName);
            return control != null ? control.Text.Trim() : string.Empty;
        }

        private void SetLabelValue(string controlName, string value)
        {
            Control control = FindControlRecursive(this, controlName);
            if (control is Label label)
            {
                label.Text = value;
            }
        }

        private void FocusControl(string controlName)
        {
            Control control = FindControlRecursive(this, controlName);
            control?.Focus();
        }

        /// <summary>
        /// 컨트롤을 재귀적으로 검색하는 헬퍼 메서드
        /// </summary>
        private Control FindControlRecursive(Control parent, string controlName)
        {
            if (parent == null) return null;
            
            // 현재 컨트롤의 이름 확인
            if (parent.Name == controlName)
                return parent;
            
            // 자식 컨트롤들을 재귀적으로 검색
            foreach (Control child in parent.Controls)
            {
                Control found = FindControlRecursive(child, controlName);
                if (found != null)
                    return found;
            }
            
            return null;
        }

        // ============================================
        // 이벤트 핸들러
        // ============================================

        private void txtInquiryer_TextChanged(object sender, EventArgs e)
        {
            // 문의자 정보는 읽기 전용이므로 이벤트 처리 불필요
        }

        private void Inquiry_context_txt_TextChanged(object sender, EventArgs e)
        {
            // 문의 내용은 읽기 전용이므로 이벤트 처리 불필요
        }


    }
}
