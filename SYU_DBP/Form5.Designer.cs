namespace SYU_DBP
{
    partial class Form5
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtAnswer = new System.Windows.Forms.TextBox();
            this.txtAnswerer = new System.Windows.Forms.TextBox();
            this.btnSubmitAnswer = new System.Windows.Forms.Button();
            this.label51 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtInquiryer = new System.Windows.Forms.TextBox();
            this.Inquiry_context_txt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtAnswer
            // 
            this.txtAnswer.Location = new System.Drawing.Point(336, 410);
            this.txtAnswer.Multiline = true;
            this.txtAnswer.Name = "txtAnswer";
            this.txtAnswer.Size = new System.Drawing.Size(438, 170);
            this.txtAnswer.TabIndex = 8;
            // 
            // txtAnswerer
            // 
            this.txtAnswerer.Location = new System.Drawing.Point(336, 323);
            this.txtAnswerer.Name = "txtAnswerer";
            this.txtAnswerer.Size = new System.Drawing.Size(257, 35);
            this.txtAnswerer.TabIndex = 9;
            // 
            // btnSubmitAnswer
            // 
            this.btnSubmitAnswer.Location = new System.Drawing.Point(825, 535);
            this.btnSubmitAnswer.Name = "btnSubmitAnswer";
            this.btnSubmitAnswer.Size = new System.Drawing.Size(180, 45);
            this.btnSubmitAnswer.TabIndex = 7;
            this.btnSubmitAnswer.Text = "답변";
            this.btnSubmitAnswer.UseVisualStyleBackColor = true;
            this.btnSubmitAnswer.Click += new System.EventHandler(this.btnSubmitAnswer_Click);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(215, 413);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(106, 24);
            this.label51.TabIndex = 5;
            this.label51.Text = "답변내용";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(217, 323);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(82, 24);
            this.label50.TabIndex = 6;
            this.label50.Text = "답변자";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(215, 179);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 24);
            this.label1.TabIndex = 6;
            this.label1.Text = "문의내용";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(217, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 24);
            this.label2.TabIndex = 6;
            this.label2.Text = "문의자";
            // 
            // txtInquiryer
            // 
            this.txtInquiryer.Location = new System.Drawing.Point(336, 106);
            this.txtInquiryer.Name = "txtInquiryer";
            this.txtInquiryer.Size = new System.Drawing.Size(257, 35);
            this.txtInquiryer.TabIndex = 9;
            this.txtInquiryer.TextChanged += new System.EventHandler(this.txtInquiryer_TextChanged);
            // 
            // Inquiry_context_txt
            // 
            this.Inquiry_context_txt.Location = new System.Drawing.Point(336, 176);
            this.Inquiry_context_txt.Multiline = true;
            this.Inquiry_context_txt.Name = "Inquiry_context_txt";
            this.Inquiry_context_txt.Size = new System.Drawing.Size(438, 117);
            this.Inquiry_context_txt.TabIndex = 9;
            this.Inquiry_context_txt.TextChanged += new System.EventHandler(this.Inquiry_context_txt_TextChanged);
            // 
            // Form5
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1363, 906);
            this.Controls.Add(this.txtAnswer);
            this.Controls.Add(this.txtInquiryer);
            this.Controls.Add(this.Inquiry_context_txt);
            this.Controls.Add(this.txtAnswerer);
            this.Controls.Add(this.btnSubmitAnswer);
            this.Controls.Add(this.label51);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label50);
            this.Name = "Form5";
            this.Text = "Form5";
            this.Load += new System.EventHandler(this.Form5_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtAnswer;
        private System.Windows.Forms.TextBox txtAnswerer;
        private System.Windows.Forms.Button btnSubmitAnswer;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtInquiryer;
        private System.Windows.Forms.TextBox Inquiry_context_txt;
    }
}