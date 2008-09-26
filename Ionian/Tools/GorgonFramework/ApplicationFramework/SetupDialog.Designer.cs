#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Saturday, May 12, 2007 1:23:45 AM
// 
#endregion

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
			this.label1 = new System.Windows.Forms.Label();
			this.comboVSync = new System.Windows.Forms.ComboBox();
			this.checkAllowScreensaver = new System.Windows.Forms.CheckBox();
			this.checkAllowBackground = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.pictureBox1.Image = global::GorgonLibrary.Framework.Properties.Resources.GorgonLogo;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(346, 86);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// lblVideoDevices
			// 
			this.lblVideoDevices.AutoSize = true;
			this.lblVideoDevices.Location = new System.Drawing.Point(7, 93);
			this.lblVideoDevices.Name = "lblVideoDevices";
			this.lblVideoDevices.Size = new System.Drawing.Size(76, 13);
			this.lblVideoDevices.TabIndex = 1;
			this.lblVideoDevices.Text = "Video Devices";
			// 
			// videoDevices
			// 
			this.videoDevices.BackColor = System.Drawing.Color.WhiteSmoke;
			this.videoDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoDevices.Enabled = false;
			this.videoDevices.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.videoDevices.Location = new System.Drawing.Point(7, 110);
			this.videoDevices.Name = "videoDevices";
			this.videoDevices.Size = new System.Drawing.Size(305, 21);
			this.videoDevices.TabIndex = 0;
			this.videoDevices.SelectedIndexChanged += new System.EventHandler(this.videoDevices_SelectedIndexChanged);
			// 
			// lblVideoModes
			// 
			this.lblVideoModes.AutoSize = true;
			this.lblVideoModes.Location = new System.Drawing.Point(7, 136);
			this.lblVideoModes.Name = "lblVideoModes";
			this.lblVideoModes.Size = new System.Drawing.Size(69, 13);
			this.lblVideoModes.TabIndex = 4;
			this.lblVideoModes.Text = "Video Modes";
			// 
			// videoModes
			// 
			this.videoModes.BackColor = System.Drawing.Color.WhiteSmoke;
			this.videoModes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.videoModes.Location = new System.Drawing.Point(7, 153);
			this.videoModes.Name = "videoModes";
			this.videoModes.Size = new System.Drawing.Size(305, 21);
			this.videoModes.TabIndex = 2;
			this.videoModes.SelectedIndexChanged += new System.EventHandler(this.videoModes_SelectedIndexChanged);
			// 
			// OK
			// 
			this.OK.Cursor = System.Windows.Forms.Cursors.Hand;
			this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.OK.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OK.ForeColor = System.Drawing.Color.Black;
			this.OK.Image = global::GorgonLibrary.Framework.Properties.Resources.check;
			this.OK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.OK.Location = new System.Drawing.Point(212, 182);
			this.OK.Name = "OK";
			this.OK.Size = new System.Drawing.Size(55, 24);
			this.OK.TabIndex = 8;
			this.OK.Text = "&OK";
			this.OK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OK.UseVisualStyleBackColor = true;
			this.OK.Click += new System.EventHandler(this.OK_Click);
			// 
			// cancel
			// 
			this.cancel.Cursor = System.Windows.Forms.Cursors.Hand;
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.cancel.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancel.ForeColor = System.Drawing.Color.Black;
			this.cancel.Image = global::GorgonLibrary.Framework.Properties.Resources.delete;
			this.cancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.cancel.Location = new System.Drawing.Point(273, 182);
			this.cancel.Name = "cancel";
			this.cancel.Size = new System.Drawing.Size(71, 24);
			this.cancel.TabIndex = 9;
			this.cancel.Text = "&Cancel";
			this.cancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.cancel.UseVisualStyleBackColor = true;
			// 
			// deviceInfo
			// 
			this.deviceInfo.Cursor = System.Windows.Forms.Cursors.Hand;
			this.deviceInfo.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.deviceInfo.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.deviceInfo.ForeColor = System.Drawing.Color.Black;
			this.deviceInfo.Image = global::GorgonLibrary.Framework.Properties.Resources.monitor_rgb;
			this.deviceInfo.Location = new System.Drawing.Point(316, 106);
			this.deviceInfo.Name = "deviceInfo";
			this.deviceInfo.Size = new System.Drawing.Size(28, 28);
			this.deviceInfo.TabIndex = 1;
			this.buttonTips.SetToolTip(this.deviceInfo, "Show information about the currently selected video device.");
			this.deviceInfo.UseVisualStyleBackColor = true;
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
			this.windowed.Image = global::GorgonLibrary.Framework.Properties.Resources.FullScreen16x16;
			this.windowed.Location = new System.Drawing.Point(316, 148);
			this.windowed.Name = "windowed";
			this.windowed.Size = new System.Drawing.Size(28, 28);
			this.windowed.TabIndex = 3;
			this.buttonTips.SetToolTip(this.windowed, "Set to");
			this.windowed.UseVisualStyleBackColor = false;
			this.windowed.CheckedChanged += new System.EventHandler(this.windowed_CheckedChanged);
			// 
			// checkAdvanced
			// 
			this.checkAdvanced.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkAdvanced.BackColor = System.Drawing.Color.Transparent;
			this.checkAdvanced.Cursor = System.Windows.Forms.Cursors.Hand;
			this.checkAdvanced.Image = global::GorgonLibrary.Framework.Properties.Resources.DoubleDown;
			this.checkAdvanced.Location = new System.Drawing.Point(7, 182);
			this.checkAdvanced.Name = "checkAdvanced";
			this.checkAdvanced.Size = new System.Drawing.Size(27, 26);
			this.checkAdvanced.TabIndex = 4;
			this.buttonTips.SetToolTip(this.checkAdvanced, "Advanced Settings...");
			this.checkAdvanced.UseVisualStyleBackColor = false;
			this.checkAdvanced.CheckedChanged += new System.EventHandler(this.checkAdvanced_CheckedChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(7, 213);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(82, 13);
			this.label1.TabIndex = 26;
			this.label1.Text = "V-Sync Settings";
			// 
			// comboVSync
			// 
			this.comboVSync.BackColor = System.Drawing.Color.WhiteSmoke;
			this.comboVSync.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboVSync.Enabled = false;
			this.comboVSync.Location = new System.Drawing.Point(7, 230);
			this.comboVSync.Name = "comboVSync";
			this.comboVSync.Size = new System.Drawing.Size(333, 21);
			this.comboVSync.TabIndex = 5;
			this.comboVSync.Visible = false;
			this.comboVSync.SelectedIndexChanged += new System.EventHandler(this.comboVSync_SelectedIndexChanged);
			// 
			// checkAllowScreensaver
			// 
			this.checkAllowScreensaver.AutoSize = true;
			this.checkAllowScreensaver.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
			this.checkAllowScreensaver.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
			this.checkAllowScreensaver.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.checkAllowScreensaver.Location = new System.Drawing.Point(219, 257);
			this.checkAllowScreensaver.Name = "checkAllowScreensaver";
			this.checkAllowScreensaver.Size = new System.Drawing.Size(121, 17);
			this.checkAllowScreensaver.TabIndex = 7;
			this.checkAllowScreensaver.Text = "Allow screen saver?";
			this.checkAllowScreensaver.UseVisualStyleBackColor = true;
			this.checkAllowScreensaver.Visible = false;
			// 
			// checkAllowBackground
			// 
			this.checkAllowBackground.AutoSize = true;
			this.checkAllowBackground.FlatAppearance.CheckedBackColor = System.Drawing.Color.Gray;
			this.checkAllowBackground.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
			this.checkAllowBackground.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.checkAllowBackground.Location = new System.Drawing.Point(7, 257);
			this.checkAllowBackground.Name = "checkAllowBackground";
			this.checkAllowBackground.Size = new System.Drawing.Size(164, 17);
			this.checkAllowBackground.TabIndex = 6;
			this.checkAllowBackground.Text = "Allow background rendering?";
			this.checkAllowBackground.UseVisualStyleBackColor = true;
			this.checkAllowBackground.Visible = false;
			// 
			// SetupDialog
			// 
			this.AcceptButton = this.OK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(346, 213);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboVSync);
			this.Controls.Add(this.checkAllowScreensaver);
			this.Controls.Add(this.checkAllowBackground);
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
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SetupDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Setup";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.ToolTip buttonTips;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.CheckBox windowed;
		private System.Windows.Forms.CheckBox checkAdvanced;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboVSync;
		private System.Windows.Forms.CheckBox checkAllowScreensaver;
		private System.Windows.Forms.CheckBox checkAllowBackground;

	}
}
