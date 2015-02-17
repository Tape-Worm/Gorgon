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
	partial class FormMain
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.tabCategories = new KRBTabControl.KRBTabControl();
			this.ContentArea.SuspendLayout();
			this.SuspendLayout();
			// 
			// ContentArea
			// 
			this.ContentArea.Controls.Add(this.tabCategories);
			this.ContentArea.Location = new System.Drawing.Point(6, 31);
			this.ContentArea.Size = new System.Drawing.Size(1268, 763);
			// 
			// tabCategories
			// 
			this.tabCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabCategories.BackgroundColor = System.Drawing.SystemColors.Control;
			this.tabCategories.BackgroundHatcher.HatchType = System.Drawing.Drawing2D.HatchStyle.DashedVertical;
			this.tabCategories.BorderColor = System.Drawing.SystemColors.Control;
			this.tabCategories.IsCaptionVisible = false;
			this.tabCategories.IsDrawHeader = false;
			this.tabCategories.IsUserInteraction = false;
			this.tabCategories.ItemSize = new System.Drawing.Size(0, 26);
			this.tabCategories.Location = new System.Drawing.Point(0, 0);
			this.tabCategories.Name = "tabCategories";
			this.tabCategories.Size = new System.Drawing.Size(1268, 758);
			this.tabCategories.TabGradient.ColorEnd = System.Drawing.Color.White;
			this.tabCategories.TabIndex = 0;
			this.tabCategories.UpDownStyle = KRBTabControl.KRBTabControl.UpDown32Style.Default;
			// 
			// FormMain
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Border = true;
			this.ClientSize = new System.Drawing.Size(1280, 800);
			this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
			this.Name = "FormMain";
			this.Padding = new System.Windows.Forms.Padding(6);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Gorgon - Examples";
			this.ContentArea.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private KRBTabControl.KRBTabControl tabCategories;
	}
}

