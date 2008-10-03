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
// Created: Saturday, April 19, 2008 12:19:04 PM
// 
#endregion

namespace Dialogs
{
	partial class ConfirmationDialogEx
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
			this.checkToAll = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonNo
			// 
			this.buttonNo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonNo.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonNo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			// 
			// buttonCancel
			// 
			this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			// 
			// OKButton
			// 
			this.OKButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.OKButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.OKButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.OKButton.Location = new System.Drawing.Point(11, 78);
			// 
			// checkToAll
			// 
			this.checkToAll.AutoSize = true;
			this.checkToAll.Location = new System.Drawing.Point(12, 107);
			this.checkToAll.Name = "checkToAll";
			this.checkToAll.Size = new System.Drawing.Size(217, 21);
			this.checkToAll.TabIndex = 12;
			this.checkToAll.Text = "&Apply the answer to all items?";
			this.checkToAll.UseVisualStyleBackColor = true;
			// 
			// ConfirmationDialogEx
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(294, 129);
			this.Controls.Add(this.checkToAll);
			this.Location = new System.Drawing.Point(0, 0);
			this.MinimumSize = new System.Drawing.Size(188, 154);
			this.Name = "ConfirmationDialogEx";
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.buttonNo, 0);
			this.Controls.SetChildIndex(this.checkToAll, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.imageIcon, 0);
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkToAll;
	}
}
