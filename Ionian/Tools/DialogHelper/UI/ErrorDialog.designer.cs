#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 12:19:51 PM
// 
#endregion

namespace Dialogs
{
	partial class ErrorDialog
        : BaseDialog 
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialog));
			this.errorDetails = new System.Windows.Forms.TextBox();
			this.iconBug = new System.Windows.Forms.PictureBox();
			this.iconData = new System.Windows.Forms.PictureBox();
			this.iconDisk = new System.Windows.Forms.PictureBox();
			this.iconHardware = new System.Windows.Forms.PictureBox();
			this.iconRound = new System.Windows.Forms.PictureBox();
			this.detailsButton = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iconBug)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iconData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iconDisk)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iconHardware)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.iconRound)).BeginInit();
			this.SuspendLayout();
			// 
			// imageIcon
			// 
			this.imageIcon.Image = global::Dialogs.Properties.Resources.error_default;
			// 
			// OKButton
			// 
			this.OKButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.OKButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.OKButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.OKButton.Image = global::Dialogs.Properties.Resources.defaultTrueIcon;
			this.OKButton.TabIndex = 0;
			this.OKButton.Text = "&OK";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// errorDetails
			// 
			this.errorDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.errorDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.errorDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.errorDetails.Location = new System.Drawing.Point(8, 112);
			this.errorDetails.MaxLength = 2097152;
			this.errorDetails.Multiline = true;
			this.errorDetails.Name = "errorDetails";
			this.errorDetails.ReadOnly = true;
			this.errorDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.errorDetails.Size = new System.Drawing.Size(223, 192);
			this.errorDetails.TabIndex = 4;
			this.errorDetails.Visible = false;
			this.errorDetails.WordWrap = false;
			// 
			// iconBug
			// 
			this.iconBug.Image = global::Dialogs.Properties.Resources.error_bug;
			this.iconBug.Location = new System.Drawing.Point(11, 6);
			this.iconBug.Name = "iconBug";
			this.iconBug.Size = new System.Drawing.Size(56, 52);
			this.iconBug.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconBug.TabIndex = 12;
			this.iconBug.TabStop = false;
			this.iconBug.Visible = false;
			// 
			// iconData
			// 
			this.iconData.Image = global::Dialogs.Properties.Resources.error_data;
			this.iconData.Location = new System.Drawing.Point(11, 6);
			this.iconData.Name = "iconData";
			this.iconData.Size = new System.Drawing.Size(56, 52);
			this.iconData.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconData.TabIndex = 13;
			this.iconData.TabStop = false;
			this.iconData.Visible = false;
			// 
			// iconDisk
			// 
			this.iconDisk.Image = global::Dialogs.Properties.Resources.error_disk;
			this.iconDisk.Location = new System.Drawing.Point(11, 6);
			this.iconDisk.Name = "iconDisk";
			this.iconDisk.Size = new System.Drawing.Size(56, 52);
			this.iconDisk.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconDisk.TabIndex = 14;
			this.iconDisk.TabStop = false;
			this.iconDisk.Visible = false;
			// 
			// iconHardware
			// 
			this.iconHardware.Image = global::Dialogs.Properties.Resources.error_hardware;
			this.iconHardware.Location = new System.Drawing.Point(11, 6);
			this.iconHardware.Name = "iconHardware";
			this.iconHardware.Size = new System.Drawing.Size(56, 52);
			this.iconHardware.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconHardware.TabIndex = 15;
			this.iconHardware.TabStop = false;
			this.iconHardware.Visible = false;
			// 
			// iconRound
			// 
			this.iconRound.Image = global::Dialogs.Properties.Resources.error_round;
			this.iconRound.Location = new System.Drawing.Point(11, 6);
			this.iconRound.Name = "iconRound";
			this.iconRound.Size = new System.Drawing.Size(56, 52);
			this.iconRound.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.iconRound.TabIndex = 16;
			this.iconRound.TabStop = false;
			this.iconRound.Visible = false;
			// 
			// detailsButton
			// 
			this.detailsButton.Appearance = System.Windows.Forms.Appearance.Button;
			this.detailsButton.Image = ((System.Drawing.Image)(resources.GetObject("detailsButton.Image")));
			this.detailsButton.Location = new System.Drawing.Point(12, 78);
			this.detailsButton.Name = "detailsButton";
			this.detailsButton.Size = new System.Drawing.Size(77, 28);
			this.detailsButton.TabIndex = 1;
			this.detailsButton.Text = "&Details";
			this.detailsButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.detailsButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.detailsButton.UseVisualStyleBackColor = true;
			this.detailsButton.Click += new System.EventHandler(this.detailsButton_Click);
			// 
			// ErrorDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(243, 127);
			this.Controls.Add(this.detailsButton);
			this.Controls.Add(this.iconBug);
			this.Controls.Add(this.errorDetails);
			this.Controls.Add(this.iconRound);
			this.Controls.Add(this.iconHardware);
			this.Controls.Add(this.iconDisk);
			this.Controls.Add(this.iconData);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ErrorDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Error";
			this.Controls.SetChildIndex(this.iconData, 0);
			this.Controls.SetChildIndex(this.iconDisk, 0);
			this.Controls.SetChildIndex(this.iconHardware, 0);
			this.Controls.SetChildIndex(this.iconRound, 0);
			this.Controls.SetChildIndex(this.errorDetails, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.imageIcon, 0);
			this.Controls.SetChildIndex(this.iconBug, 0);
			this.Controls.SetChildIndex(this.detailsButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iconBug)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iconData)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iconDisk)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iconHardware)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.iconRound)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox errorDetails;
        private System.Windows.Forms.PictureBox iconBug;
        private System.Windows.Forms.PictureBox iconData;
        private System.Windows.Forms.PictureBox iconDisk;
        private System.Windows.Forms.PictureBox iconHardware;
        private System.Windows.Forms.PictureBox iconRound;
		private System.Windows.Forms.CheckBox detailsButton;
	}
}