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
// Created: Tuesday, April 24, 2007 2:56:35 PM
// 
#endregion

namespace GorgonLibrary.FileSystems.Tools
{
	partial class formComment
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formComment));
			this.panel1 = new System.Windows.Forms.Panel();
			this.textComment = new System.Windows.Forms.TextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonSave = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Black;
			this.panel1.Controls.Add(this.textComment);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(284, 224);
			this.panel1.TabIndex = 3;
			// 
			// textComment
			// 
			this.textComment.AcceptsReturn = true;
			this.textComment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textComment.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.textComment.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllSystemSources;
			this.textComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textComment.Location = new System.Drawing.Point(0, 0);
			this.textComment.Margin = new System.Windows.Forms.Padding(0);
			this.textComment.MaxLength = 2048;
			this.textComment.Multiline = true;
			this.textComment.Name = "textComment";
			this.textComment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textComment.Size = new System.Drawing.Size(284, 223);
			this.textComment.TabIndex = 1;
			this.textComment.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textComment_KeyDown);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.buttonCancel);
			this.panel2.Controls.Add(this.buttonSave);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(0, 224);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(284, 32);
			this.panel2.TabIndex = 4;
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.error;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(216, 6);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(65, 23);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "&Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonSave
			// 
			this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonSave.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.disk_blue;
			this.buttonSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonSave.Location = new System.Drawing.Point(145, 6);
			this.buttonSave.Name = "buttonSave";
			this.buttonSave.Size = new System.Drawing.Size(65, 23);
			this.buttonSave.TabIndex = 3;
			this.buttonSave.Text = "&Save";
			this.buttonSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonSave.UseVisualStyleBackColor = true;
			// 
			// formComment
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 256);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel2);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(256, 256);
			this.Name = "formComment";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Comment";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox textComment;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonSave;

	}
}