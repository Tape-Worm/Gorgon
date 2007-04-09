namespace GorgonLibrary.Framework
{
	partial class SetupDialog
	{
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.ComboBox videoModes;
		private System.Windows.Forms.Button OK;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label lblVideoDevices;
		private System.Windows.Forms.Label lblVideoModes;
		private System.Windows.Forms.ComboBox videoDevices;
		private System.Windows.Forms.Button deviceInfo;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialog));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.lblVideoDevices = new System.Windows.Forms.Label();
			this.videoDevices = new System.Windows.Forms.ComboBox();
			this.lblVideoModes = new System.Windows.Forms.Label();
			this.videoModes = new System.Windows.Forms.ComboBox();
			this.OK = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.deviceInfo = new System.Windows.Forms.Button();
			this.buttonTips = new System.Windows.Forms.ToolTip(this.components);
			this.windowed = new System.Windows.Forms.CheckBox();
			this.checkAdvanced = new System.Windows.Forms.CheckBox();
			this.fadeTimer = new System.Windows.Forms.Timer(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.comboVSync = new System.Windows.Forms.ComboBox();
			this.checkAllowScreensaver = new System.Windows.Forms.CheckBox();
			this.checkAllowBackground = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Image = global::GorgonLibrary.Properties.Resources.GorgonLogo;
			this.pictureBox1.Location = new System.Drawing.Point(0, 2);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(352, 86);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// lblVideoDevices
			// 
			this.lblVideoDevices.BackColor = System.Drawing.Color.Gray;
			this.lblVideoDevices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblVideoDevices.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold);
			this.lblVideoDevices.ForeColor = System.Drawing.Color.White;
			this.lblVideoDevices.Location = new System.Drawing.Point(7, 91);
			this.lblVideoDevices.Name = "lblVideoDevices";
			this.lblVideoDevices.Size = new System.Drawing.Size(305, 16);
			this.lblVideoDevices.TabIndex = 1;
			this.lblVideoDevices.Text = "Video Devices";
			// 
			// videoDevices
			// 
			this.videoDevices.BackColor = System.Drawing.Color.WhiteSmoke;
			this.videoDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoDevices.Enabled = false;
			this.videoDevices.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.videoDevices.Location = new System.Drawing.Point(7, 107);
			this.videoDevices.Name = "videoDevices";
			this.videoDevices.Size = new System.Drawing.Size(305, 21);
			this.videoDevices.TabIndex = 2;
			this.videoDevices.SelectedIndexChanged += new System.EventHandler(this.videoDevices_SelectedIndexChanged);
			// 
			// lblVideoModes
			// 
			this.lblVideoModes.BackColor = System.Drawing.Color.Gray;
			this.lblVideoModes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblVideoModes.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold);
			this.lblVideoModes.ForeColor = System.Drawing.Color.White;
			this.lblVideoModes.Location = new System.Drawing.Point(7, 131);
			this.lblVideoModes.Name = "lblVideoModes";
			this.lblVideoModes.Size = new System.Drawing.Size(305, 16);
			this.lblVideoModes.TabIndex = 4;
			this.lblVideoModes.Text = "Video Modes";
			// 
			// videoModes
			// 
			this.videoModes.BackColor = System.Drawing.Color.WhiteSmoke;
			this.videoModes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoModes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.videoModes.Location = new System.Drawing.Point(7, 147);
			this.videoModes.Name = "videoModes";
			this.videoModes.Size = new System.Drawing.Size(305, 21);
			this.videoModes.TabIndex = 5;
			this.videoModes.SelectedIndexChanged += new System.EventHandler(this.videoModes_SelectedIndexChanged);
			// 
			// OK
			// 
			this.OK.BackColor = System.Drawing.SystemColors.Control;
			this.OK.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.OK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.OK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.OK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.OK.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.ForeColor = System.Drawing.Color.Black;
			this.OK.Image = global::GorgonLibrary.Properties.Resources.check;
			this.OK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.OK.Location = new System.Drawing.Point(214, 174);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(55, 24);
			this.OK.TabIndex = 7;
			this.OK.Text = "OK";
			this.OK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OK.UseVisualStyleBackColor = false;
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// cancel
			// 
			this.cancel.BackColor = System.Drawing.SystemColors.Control;
			this.cancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.cancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cancel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancel.ForeColor = System.Drawing.Color.Black;
			this.cancel.Image = global::GorgonLibrary.Properties.Resources.delete;
			this.cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.cancel.Location = new System.Drawing.Point(275, 174);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(71, 24);
			this.cancel.TabIndex = 8;
			this.cancel.Text = "Cancel";
			this.cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cancel.UseVisualStyleBackColor = false;
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// deviceInfo
			// 
			this.deviceInfo.BackColor = System.Drawing.SystemColors.Control;
			this.deviceInfo.Cursor = System.Windows.Forms.Cursors.Hand;
			this.deviceInfo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.deviceInfo.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.deviceInfo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
			this.deviceInfo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.deviceInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.deviceInfo.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.deviceInfo.ForeColor = System.Drawing.Color.Black;
			this.deviceInfo.Image = global::GorgonLibrary.Properties.Resources.monitor_rgb;
			this.deviceInfo.Location = new System.Drawing.Point(318, 104);
			this.deviceInfo.Name = "deviceInfo";
			this.deviceInfo.Size = new System.Drawing.Size(28, 24);
			this.deviceInfo.TabIndex = 9;
			this.buttonTips.SetToolTip(this.deviceInfo, "Show information about the currently selected video device.");
			this.deviceInfo.UseVisualStyleBackColor = false;
			this.deviceInfo.Click += new System.EventHandler(this.deviceInfo_Click);
			// 
			// buttonTips
			// 
			this.buttonTips.AutomaticDelay = 1500;
			this.buttonTips.IsBalloon = true;
			this.buttonTips.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.buttonTips.ToolTipTitle = "What\'s this?";
			// 
			// windowed
			// 
			this.windowed.Appearance = System.Windows.Forms.Appearance.Button;
			this.windowed.BackColor = System.Drawing.Color.Transparent;
			this.windowed.Cursor = System.Windows.Forms.Cursors.Hand;
			this.windowed.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.windowed.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
			this.windowed.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.windowed.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.windowed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.windowed.Image = global::GorgonLibrary.Properties.Resources.FullScreen16x16;
			this.windowed.Location = new System.Drawing.Point(318, 144);
			this.windowed.Name = "windowed";
			this.windowed.Size = new System.Drawing.Size(28, 24);
			this.windowed.TabIndex = 6;
			this.buttonTips.SetToolTip(this.windowed, "Set to");
			this.windowed.UseVisualStyleBackColor = false;
			this.windowed.CheckedChanged += new System.EventHandler(this.windowed_CheckedChanged);
			// 
			// checkAdvanced
			// 
			this.checkAdvanced.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkAdvanced.BackColor = System.Drawing.Color.Transparent;
			this.checkAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
			this.checkAdvanced.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkAdvanced.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
			this.checkAdvanced.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.checkAdvanced.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.checkAdvanced.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkAdvanced.Image = global::GorgonLibrary.Properties.Resources.DoubleDown;
			this.checkAdvanced.Location = new System.Drawing.Point(7, 174);
			this.checkAdvanced.Name = "checkAdvanced";
			this.checkAdvanced.Size = new System.Drawing.Size(28, 24);
			this.checkAdvanced.TabIndex = 10;
			this.buttonTips.SetToolTip(this.checkAdvanced, "Advanced Settings...");
			this.checkAdvanced.UseVisualStyleBackColor = false;
			this.checkAdvanced.CheckedChanged += new System.EventHandler(this.checkAdvanced_CheckedChanged);
			// 
			// fadeTimer
			// 
			this.fadeTimer.Interval = 10;
			this.fadeTimer.Tick += new System.EventHandler(this.fadeTimer_Tick);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(155)))), ((int)(((byte)(112)))));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.comboVSync);
			this.panel1.Controls.Add(this.checkAllowScreensaver);
			this.panel1.Controls.Add(this.checkAllowBackground);
			this.panel1.Location = new System.Drawing.Point(7, 205);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(339, 75);
			this.panel1.TabIndex = 11;
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.Gray;
			this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold);
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(3, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(328, 16);
			this.label1.TabIndex = 22;
			this.label1.Text = "Video Devices";
			// 
			// comboVSync
			// 
			this.comboVSync.BackColor = System.Drawing.Color.WhiteSmoke;
			this.comboVSync.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboVSync.Enabled = false;
			this.comboVSync.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboVSync.Location = new System.Drawing.Point(3, 20);
			this.comboVSync.Name = "comboVSync";
			this.comboVSync.Size = new System.Drawing.Size(328, 21);
			this.comboVSync.TabIndex = 18;
			this.comboVSync.SelectedIndexChanged += new System.EventHandler(this.comboVSync_SelectedIndexChanged);
			// 
			// checkAllowScreensaver
			// 
			this.checkAllowScreensaver.AutoSize = true;
			this.checkAllowScreensaver.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
			this.checkAllowScreensaver.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
			this.checkAllowScreensaver.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.checkAllowScreensaver.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkAllowScreensaver.Location = new System.Drawing.Point(213, 48);
			this.checkAllowScreensaver.Name = "checkAllowScreensaver";
			this.checkAllowScreensaver.Size = new System.Drawing.Size(118, 17);
			this.checkAllowScreensaver.TabIndex = 21;
			this.checkAllowScreensaver.Text = "Allow screen saver?";
			this.checkAllowScreensaver.UseVisualStyleBackColor = true;
			// 
			// checkAllowBackground
			// 
			this.checkAllowBackground.AutoSize = true;
			this.checkAllowBackground.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
			this.checkAllowBackground.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
			this.checkAllowBackground.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.checkAllowBackground.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkAllowBackground.Location = new System.Drawing.Point(3, 48);
			this.checkAllowBackground.Name = "checkAllowBackground";
			this.checkAllowBackground.Size = new System.Drawing.Size(161, 17);
			this.checkAllowBackground.TabIndex = 20;
			this.checkAllowBackground.Text = "Allow background rendering?";
			this.checkAllowBackground.UseVisualStyleBackColor = true;
			// 
			// SetupDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(352, 202);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.checkAdvanced);
			this.Controls.Add(this.deviceInfo);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.OK);
			this.Controls.Add(this.windowed);
			this.Controls.Add(this.videoModes);
			this.Controls.Add(this.lblVideoModes);
			this.Controls.Add(this.videoDevices);
			this.Controls.Add(this.lblVideoDevices);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "SetupDialog";
			this.Opacity = 0.01;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Setup";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.ToolTip buttonTips;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Timer fadeTimer;
		private System.Windows.Forms.CheckBox windowed;
		private System.Windows.Forms.CheckBox checkAdvanced;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox checkAllowScreensaver;
		private System.Windows.Forms.CheckBox checkAllowBackground;
		private System.Windows.Forms.ComboBox comboVSync;
		private System.Windows.Forms.Label label1;

	}
}
