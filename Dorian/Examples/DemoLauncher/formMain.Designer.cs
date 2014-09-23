#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, August 26, 2013 10:59:42 PM
// 
#endregion

namespace GorgonLibrary.Examples
{
	partial class formMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
			this.panelContent = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// panelContent
			// 
			this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panelContent.AutoScroll = true;
			this.panelContent.BackColor = System.Drawing.Color.White;
			this.panelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelContent.Location = new System.Drawing.Point(6, 34);
			this.panelContent.Name = "panelContent";
			this.panelContent.Size = new System.Drawing.Size(1268, 760);
			this.panelContent.TabIndex = 5;
			// 
			// formMain
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Border = true;
			this.BorderColor = System.Drawing.Color.Gray;
			this.ClientSize = new System.Drawing.Size(1280, 800);
			this.Controls.Add(this.panelContent);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
			this.Name = "formMain";
			this.Padding = new System.Windows.Forms.Padding(6);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Gorgon";
			this.Controls.SetChildIndex(this.panelContent, 0);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelContent;
	}
}

