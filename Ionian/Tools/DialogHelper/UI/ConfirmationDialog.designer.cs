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
// Created: Saturday, April 19, 2008 12:18:47 PM
// 
#endregion

namespace Dialogs
{
	partial class ConfirmationDialog
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
			this.buttonNo = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// imageIcon
			// 
			this.imageIcon.Image = global::Dialogs.Properties.Resources.question_and_answer;
			this.imageIcon.Visible = true;
			// 
			// OKButton
			// 
			this.OKButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.OKButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.OKButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.OKButton.Image = global::Dialogs.Properties.Resources.defaultTrueIcon;
			this.OKButton.Location = new System.Drawing.Point(12, 78);
			this.OKButton.TabIndex = 0;
			this.OKButton.Text = "&Yes";
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// buttonNo
			// 
			this.buttonNo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonNo.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonNo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.buttonNo.Image = global::Dialogs.Properties.Resources.defaultFalseIcon;
			this.buttonNo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonNo.Location = new System.Drawing.Point(150, 78);
			this.buttonNo.Name = "buttonNo";
			this.buttonNo.Size = new System.Drawing.Size(77, 28);
			this.buttonNo.TabIndex = 1;
			this.buttonNo.Text = "&No";
			this.buttonNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonNo.UseVisualStyleBackColor = true;
			this.buttonNo.Click += new System.EventHandler(this.buttonNo_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.buttonCancel.Image = global::Dialogs.Properties.Resources.error;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(141, 78);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(77, 28);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Visible = false;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// ConfirmationDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(294, 127);
			this.Controls.Add(this.buttonNo);
			this.Controls.Add(this.buttonCancel);
			this.MinimumSize = new System.Drawing.Size(300, 158);
			this.Name = "ConfirmationDialog";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.imageIcon, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.buttonNo, 0);
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		/// <summary>
		/// 
		/// </summary>
		protected System.Windows.Forms.Button buttonNo;
		/// <summary>
		/// 
		/// </summary>
		protected System.Windows.Forms.Button buttonCancel;
	}
}
