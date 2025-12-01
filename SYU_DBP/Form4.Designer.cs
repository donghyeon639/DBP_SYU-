
namespace SYU_DBP
{
    partial class Form4
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
            this.Address = new System.Windows.Forms.Label();
            this.Phone = new System.Windows.Forms.Label();
            this.Email = new System.Windows.Forms.Label();
            this.AddresstxtBox = new System.Windows.Forms.TextBox();
            this.PhonetxtBox = new System.Windows.Forms.TextBox();
            this.EmailtxtBox = new System.Windows.Forms.TextBox();
            this.UpdatePersonalInfo = new System.Windows.Forms.Button();
            this.Password = new System.Windows.Forms.Label();
            this.PasswordtxtBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Address
            // 
            this.Address.AutoSize = true;
            this.Address.Location = new System.Drawing.Point(169, 144);
            this.Address.Name = "Address";
            this.Address.Size = new System.Drawing.Size(58, 24);
            this.Address.TabIndex = 0;
            this.Address.Text = "주소";
            // 
            // Phone
            // 
            this.Phone.AutoSize = true;
            this.Phone.Location = new System.Drawing.Point(169, 219);
            this.Phone.Name = "Phone";
            this.Phone.Size = new System.Drawing.Size(106, 24);
            this.Phone.TabIndex = 1;
            this.Phone.Text = "전화번호";
            // 
            // Email
            // 
            this.Email.AutoSize = true;
            this.Email.Location = new System.Drawing.Point(169, 310);
            this.Email.Name = "Email";
            this.Email.Size = new System.Drawing.Size(82, 24);
            this.Email.TabIndex = 2;
            this.Email.Text = "이메일";
            // 
            // AddresstxtBox
            // 
            this.AddresstxtBox.Location = new System.Drawing.Point(383, 132);
            this.AddresstxtBox.Name = "AddresstxtBox";
            this.AddresstxtBox.Size = new System.Drawing.Size(100, 35);
            this.AddresstxtBox.TabIndex = 3;
            // 
            // PhonetxtBox
            // 
            this.PhonetxtBox.Location = new System.Drawing.Point(383, 219);
            this.PhonetxtBox.Name = "PhonetxtBox";
            this.PhonetxtBox.Size = new System.Drawing.Size(100, 35);
            this.PhonetxtBox.TabIndex = 4;
            // 
            // EmailtxtBox
            // 
            this.EmailtxtBox.Location = new System.Drawing.Point(383, 307);
            this.EmailtxtBox.Name = "EmailtxtBox";
            this.EmailtxtBox.Size = new System.Drawing.Size(100, 35);
            this.EmailtxtBox.TabIndex = 5;
            // 
            // UpdatePersonalInfo
            // 
            this.UpdatePersonalInfo.Location = new System.Drawing.Point(253, 449);
            this.UpdatePersonalInfo.Name = "UpdatePersonalInfo";
            this.UpdatePersonalInfo.Size = new System.Drawing.Size(162, 49);
            this.UpdatePersonalInfo.TabIndex = 6;
            this.UpdatePersonalInfo.Text = "수정";
            this.UpdatePersonalInfo.UseVisualStyleBackColor = true;
            this.UpdatePersonalInfo.Click += new System.EventHandler(this.UpdatePersonalInfo_Click);
            // 
            // Password
            // 
            this.Password.AutoSize = true;
            this.Password.Location = new System.Drawing.Point(169, 382);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(106, 24);
            this.Password.TabIndex = 7;
            this.Password.Text = "비밀번호";
            // 
            // PasswordtxtBox
            // 
            this.PasswordtxtBox.Location = new System.Drawing.Point(383, 382);
            this.PasswordtxtBox.Name = "PasswordtxtBox";
            this.PasswordtxtBox.Size = new System.Drawing.Size(100, 35);
            this.PasswordtxtBox.TabIndex = 8;
            // 
            // Form4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1252, 798);
            this.Controls.Add(this.PasswordtxtBox);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.UpdatePersonalInfo);
            this.Controls.Add(this.EmailtxtBox);
            this.Controls.Add(this.PhonetxtBox);
            this.Controls.Add(this.AddresstxtBox);
            this.Controls.Add(this.Email);
            this.Controls.Add(this.Phone);
            this.Controls.Add(this.Address);
            this.Name = "Form4";
            this.Text = "Form4";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Address;
        private System.Windows.Forms.Label Phone;
        private System.Windows.Forms.Label Email;
        private System.Windows.Forms.TextBox AddresstxtBox;
        private System.Windows.Forms.TextBox PhonetxtBox;
        private System.Windows.Forms.TextBox EmailtxtBox;
        private System.Windows.Forms.Button UpdatePersonalInfo;
        private System.Windows.Forms.Label Password;
        private System.Windows.Forms.TextBox PasswordtxtBox;
    }
}