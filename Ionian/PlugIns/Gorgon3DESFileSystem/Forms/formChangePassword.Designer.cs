namespace GorgonLibrary.FileSystems
{
    partial class formChangePassword
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formChangePassword));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textOldPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textNewPassword = new System.Windows.Forms.TextBox();
            this.textRetype = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(166, 135);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(247, 135);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Old Password:";
            // 
            // textOldPassword
            // 
            this.textOldPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textOldPassword.Location = new System.Drawing.Point(16, 30);
            this.textOldPassword.Name = "textOldPassword";
            this.textOldPassword.Size = new System.Drawing.Size(306, 20);
            this.textOldPassword.TabIndex = 0;
            this.textOldPassword.UseSystemPasswordChar = true;
            this.textOldPassword.TextChanged += new System.EventHandler(this.textOldPassword_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "New Password:";
            // 
            // textNewPassword
            // 
            this.textNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textNewPassword.Location = new System.Drawing.Point(16, 70);
            this.textNewPassword.Name = "textNewPassword";
            this.textNewPassword.Size = new System.Drawing.Size(306, 20);
            this.textNewPassword.TabIndex = 1;
            this.textNewPassword.UseSystemPasswordChar = true;
            this.textNewPassword.TextChanged += new System.EventHandler(this.textOldPassword_TextChanged);
            // 
            // textRetype
            // 
            this.textRetype.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textRetype.Location = new System.Drawing.Point(16, 109);
            this.textRetype.Name = "textRetype";
            this.textRetype.Size = new System.Drawing.Size(306, 20);
            this.textRetype.TabIndex = 2;
            this.textRetype.UseSystemPasswordChar = true;
            this.textRetype.TextChanged += new System.EventHandler(this.textOldPassword_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Retype Password:";
            // 
            // formChangePassword
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(334, 167);
            this.Controls.Add(this.textRetype);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textNewPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textOldPassword);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "formChangePassword";
            this.Text = "Change Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textOldPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textNewPassword;
        private System.Windows.Forms.TextBox textRetype;
        private System.Windows.Forms.Label label3;
    }
}