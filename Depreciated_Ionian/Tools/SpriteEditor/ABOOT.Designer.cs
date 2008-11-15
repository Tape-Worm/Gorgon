namespace GorgonLibrary.Graphics.Tools
{
	partial class ABOOT
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ABOOT));
			this.labelName = new System.Windows.Forms.Label();
			this.labelVersion = new System.Windows.Forms.Label();
			this.labelCopyright = new System.Windows.Forms.Label();
			this.labelWho = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.pictureIcon = new System.Windows.Forms.PictureBox();
			this.buttonOK = new System.Windows.Forms.Button();
			this.linkWeebsite = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// labelName
			// 
			this.labelName.Font = new System.Drawing.Font("Arial Black", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelName.Location = new System.Drawing.Point(107, 77);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(173, 53);
			this.labelName.TabIndex = 7;
			this.labelName.Text = "[Name]";
			this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelVersion
			// 
			this.labelVersion.Location = new System.Drawing.Point(108, 130);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(172, 16);
			this.labelVersion.TabIndex = 8;
			this.labelVersion.Text = "[version]";
			this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelCopyright
			// 
			this.labelCopyright.Location = new System.Drawing.Point(106, 146);
			this.labelCopyright.Name = "labelCopyright";
			this.labelCopyright.Size = new System.Drawing.Size(174, 16);
			this.labelCopyright.TabIndex = 9;
			this.labelCopyright.Text = "[copyright]";
			this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelWho
			// 
			this.labelWho.Location = new System.Drawing.Point(107, 162);
			this.labelWho.Name = "labelWho";
			this.labelWho.Size = new System.Drawing.Size(173, 16);
			this.labelWho.TabIndex = 10;
			this.labelWho.Text = "Michael Winsor (Tape_Worm)";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(12, 206);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(264, 26);
			this.label4.TabIndex = 11;
			this.label4.Text = "This software is licensed under the LGPL.  Please \r\nread the LGPL.txt file for mo" +
				"re details.";
			// 
			// pictureBox2
			// 
			this.pictureBox2.BackColor = System.Drawing.Color.White;
			this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox2.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.GorgonTextureLogo;
			this.pictureBox2.Location = new System.Drawing.Point(12, 8);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(265, 63);
			this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox2.TabIndex = 12;
			this.pictureBox2.TabStop = false;
			// 
			// pictureIcon
			// 
			this.pictureIcon.BackColor = System.Drawing.Color.White;
			this.pictureIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureIcon.Location = new System.Drawing.Point(12, 77);
			this.pictureIcon.Name = "pictureIcon";
			this.pictureIcon.Size = new System.Drawing.Size(88, 101);
			this.pictureIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureIcon.TabIndex = 6;
			this.pictureIcon.TabStop = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(256, 235);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 24);
			this.buttonOK.TabIndex = 5;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// linkWeebsite
			// 
			this.linkWeebsite.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkWeebsite.Location = new System.Drawing.Point(12, 181);
			this.linkWeebsite.Name = "linkWeebsite";
			this.linkWeebsite.Size = new System.Drawing.Size(265, 20);
			this.linkWeebsite.TabIndex = 14;
			this.linkWeebsite.TabStop = true;
			this.linkWeebsite.Text = "http://www.tape-worm.net/";
			this.linkWeebsite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkWeebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkWeebsite_LinkClicked);
			// 
			// ABOOT
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 271);
			this.Controls.Add(this.linkWeebsite);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.labelWho);
			this.Controls.Add(this.labelCopyright);
			this.Controls.Add(this.labelVersion);
			this.Controls.Add(this.labelName);
			this.Controls.Add(this.pictureIcon);
			this.Controls.Add(this.buttonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ABOOT";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "ABOOT";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.PictureBox pictureIcon;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.Label labelVersion;
		private System.Windows.Forms.Label labelCopyright;
		private System.Windows.Forms.Label labelWho;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.LinkLabel linkWeebsite;
	}
}