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
// Created: Saturday, December 06, 2008 2:02:35 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class TrackKeyDisplay
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

			if ((!DesignMode) && (disposing))
			{
				if (_numberFont != null)
					_numberFont.Dispose();

				if (_backBrush != null)
					_backBrush.Dispose();
				if (_hiliteBrush != null)
					_hiliteBrush.Dispose();
				if (_backSelBrush != null)
					_backSelBrush.Dispose();
				if (_hiliteSelBrush != null)
					_hiliteSelBrush.Dispose();
				if (_textBrush != null)
					_textBrush.Dispose();

				_numberFont = null;
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelKeyList = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// panelKeyList
			// 
			this.panelKeyList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.panelKeyList.BackColor = System.Drawing.Color.White;
			this.panelKeyList.Location = new System.Drawing.Point(3, 1);
			this.panelKeyList.Name = "panelKeyList";
			this.panelKeyList.Size = new System.Drawing.Size(141, 31);
			this.panelKeyList.TabIndex = 1;
			this.panelKeyList.Paint += new System.Windows.Forms.PaintEventHandler(this.panelKeyList_Paint);
			this.panelKeyList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelKeyList_MouseClick);
			this.panelKeyList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelKeyList_MouseDown);
			// 
			// TrackKeyDisplay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelKeyList);
			this.MinimumSize = new System.Drawing.Size(147, 32);
			this.Name = "TrackKeyDisplay";
			this.Size = new System.Drawing.Size(147, 32);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelKeyList;
	}
}
