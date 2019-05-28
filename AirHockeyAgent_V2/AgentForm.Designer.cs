namespace AirHockeyAgent
{
    partial class AgentForm
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
            this.TitleLbl = new System.Windows.Forms.Label();
            this.IPLbl = new System.Windows.Forms.Label();
            this.IDLbl = new System.Windows.Forms.Label();
            this.LogLbl = new System.Windows.Forms.Label();
            this.IPTxt = new System.Windows.Forms.TextBox();
            this.IDTxt = new System.Windows.Forms.TextBox();
            this.LogTxt = new System.Windows.Forms.TextBox();
            this.LoginBtn = new System.Windows.Forms.Button();
            this.LogoutBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TitleLbl
            // 
            this.TitleLbl.AutoSize = true;
            this.TitleLbl.Font = new System.Drawing.Font("Monotype Corsiva", 20.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TitleLbl.Location = new System.Drawing.Point(38, 25);
            this.TitleLbl.Name = "TitleLbl";
            this.TitleLbl.Size = new System.Drawing.Size(203, 33);
            this.TitleLbl.TabIndex = 0;
            this.TitleLbl.Text = "Air Hockey Agent";
            // 
            // IPLbl
            // 
            this.IPLbl.AutoSize = true;
            this.IPLbl.Location = new System.Drawing.Point(53, 92);
            this.IPLbl.Name = "IPLbl";
            this.IPLbl.Size = new System.Drawing.Size(23, 16);
            this.IPLbl.TabIndex = 1;
            this.IPLbl.Text = "IP:";
            // 
            // IDLbl
            // 
            this.IDLbl.AutoSize = true;
            this.IDLbl.Location = new System.Drawing.Point(53, 134);
            this.IDLbl.Name = "IDLbl";
            this.IDLbl.Size = new System.Drawing.Size(24, 16);
            this.IDLbl.TabIndex = 2;
            this.IDLbl.Text = "ID:";
            // 
            // LogLbl
            // 
            this.LogLbl.AutoSize = true;
            this.LogLbl.Location = new System.Drawing.Point(53, 176);
            this.LogLbl.Name = "LogLbl";
            this.LogLbl.Size = new System.Drawing.Size(59, 16);
            this.LogLbl.TabIndex = 3;
            this.LogLbl.Text = "Log File:";
            // 
            // IPTxt
            // 
            this.IPTxt.Location = new System.Drawing.Point(136, 89);
            this.IPTxt.Name = "IPTxt";
            this.IPTxt.Size = new System.Drawing.Size(151, 22);
            this.IPTxt.TabIndex = 4;
            this.IPTxt.TextChanged += new System.EventHandler(this.IPTxt_TextChanged);
            // 
            // IDTxt
            // 
            this.IDTxt.Location = new System.Drawing.Point(136, 131);
            this.IDTxt.Name = "IDTxt";
            this.IDTxt.Size = new System.Drawing.Size(151, 22);
            this.IDTxt.TabIndex = 5;
            this.IDTxt.TextChanged += new System.EventHandler(this.IDTxt_TextChanged);
            // 
            // LogTxt
            // 
            this.LogTxt.Location = new System.Drawing.Point(136, 173);
            this.LogTxt.Name = "LogTxt";
            this.LogTxt.Size = new System.Drawing.Size(151, 22);
            this.LogTxt.TabIndex = 6;
            this.LogTxt.TextChanged += new System.EventHandler(this.LogTxt_TextChanged);
            // 
            // LoginBtn
            // 
            this.LoginBtn.Location = new System.Drawing.Point(56, 226);
            this.LoginBtn.Name = "LoginBtn";
            this.LoginBtn.Size = new System.Drawing.Size(100, 28);
            this.LoginBtn.TabIndex = 7;
            this.LoginBtn.Text = "Login";
            this.LoginBtn.UseVisualStyleBackColor = true;
            this.LoginBtn.Click += new System.EventHandler(this.LoginBtn_Click);
            // 
            // LogoutBtn
            // 
            this.LogoutBtn.Location = new System.Drawing.Point(187, 226);
            this.LogoutBtn.Name = "LogoutBtn";
            this.LogoutBtn.Size = new System.Drawing.Size(100, 28);
            this.LogoutBtn.TabIndex = 8;
            this.LogoutBtn.Text = "Logout";
            this.LogoutBtn.UseVisualStyleBackColor = true;
            this.LogoutBtn.Click += new System.EventHandler(this.LogoutBtn_Click);
            // 
            // AgentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 298);
            this.Controls.Add(this.LogoutBtn);
            this.Controls.Add(this.LoginBtn);
            this.Controls.Add(this.LogTxt);
            this.Controls.Add(this.IDTxt);
            this.Controls.Add(this.IPTxt);
            this.Controls.Add(this.LogLbl);
            this.Controls.Add(this.IDLbl);
            this.Controls.Add(this.IPLbl);
            this.Controls.Add(this.TitleLbl);
            this.Name = "AgentForm";
            this.Text = "AHAgent";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AgentForm_FormClosing);
            this.Load += new System.EventHandler(this.AgentForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TitleLbl;
        private System.Windows.Forms.Label IPLbl;
        private System.Windows.Forms.Label IDLbl;
        private System.Windows.Forms.Label LogLbl;
        private System.Windows.Forms.TextBox IPTxt;
        private System.Windows.Forms.TextBox IDTxt;
        private System.Windows.Forms.TextBox LogTxt;
        private System.Windows.Forms.Button LoginBtn;
        private System.Windows.Forms.Button LogoutBtn;
    }
}

