#region MIT.
// 
// Examples.
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
// Created: Thursday, October 02, 2008 10:46:02 PM
// 
#endregion

namespace GorgonLibrary.Example
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.splitViews = new System.Windows.Forms.SplitContainer();
			this.groupControl1 = new System.Windows.Forms.Panel();
			this.trackSpeed = new System.Windows.Forms.TrackBar();
			this.buttonAnimation = new System.Windows.Forms.Button();
			this.groupControl2 = new System.Windows.Forms.Panel();
			this.splitViews.Panel1.SuspendLayout();
			this.splitViews.Panel2.SuspendLayout();
			this.splitViews.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackSpeed)).BeginInit();
			this.SuspendLayout();
			// 
			// splitViews
			// 
			this.splitViews.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitViews.Location = new System.Drawing.Point(0, 0);
			this.splitViews.Name = "splitViews";
			// 
			// splitViews.Panel1
			// 
			this.splitViews.Panel1.Controls.Add(this.groupControl1);
			this.splitViews.Panel1MinSize = 160;
			// 
			// splitViews.Panel2
			// 
			this.splitViews.Panel2.Controls.Add(this.trackSpeed);
			this.splitViews.Panel2.Controls.Add(this.buttonAnimation);
			this.splitViews.Panel2.Controls.Add(this.groupControl2);
			this.splitViews.Panel2MinSize = 140;
			this.splitViews.Size = new System.Drawing.Size(599, 414);
			this.splitViews.SplitterDistance = 261;
			this.splitViews.TabIndex = 4;
			this.splitViews.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitViews_SplitterMoving);
			this.splitViews.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitViews_SplitterMoved);
			// 
			// groupControl1
			// 
			this.groupControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.groupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupControl1.Location = new System.Drawing.Point(0, 0);
			this.groupControl1.Name = "groupControl1";
			this.groupControl1.Size = new System.Drawing.Size(261, 414);
			this.groupControl1.TabIndex = 1;
			this.groupControl1.Text = "Control 1";
			// 
			// trackSpeed
			// 
			this.trackSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.trackSpeed.LargeChange = 1;
			this.trackSpeed.Location = new System.Drawing.Point(3, 364);
			this.trackSpeed.Minimum = 1;
			this.trackSpeed.Name = "trackSpeed";
			this.trackSpeed.Size = new System.Drawing.Size(328, 45);
			this.trackSpeed.TabIndex = 5;
			this.trackSpeed.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trackSpeed.Value = 5;
			// 
			// buttonAnimation
			// 
			this.buttonAnimation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAnimation.Location = new System.Drawing.Point(3, 335);
			this.buttonAnimation.Name = "buttonAnimation";
			this.buttonAnimation.Size = new System.Drawing.Size(328, 23);
			this.buttonAnimation.TabIndex = 4;
			this.buttonAnimation.Text = "Start/Stop Animation";
			this.buttonAnimation.UseVisualStyleBackColor = true;
			this.buttonAnimation.Click += new System.EventHandler(this.buttonAnimation_Click);
			// 
			// groupControl2
			// 
			this.groupControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupControl2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.groupControl2.Location = new System.Drawing.Point(0, 0);
			this.groupControl2.Name = "groupControl2";
			this.groupControl2.Size = new System.Drawing.Size(334, 329);
			this.groupControl2.TabIndex = 2;
			this.groupControl2.Text = "Control 2";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(599, 414);
			this.Controls.Add(this.splitViews);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Example - Give me some control";
			this.splitViews.Panel1.ResumeLayout(false);
			this.splitViews.Panel2.ResumeLayout(false);
			this.splitViews.Panel2.PerformLayout();
			this.splitViews.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.trackSpeed)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitViews;
		private System.Windows.Forms.Panel groupControl1;
		private System.Windows.Forms.TrackBar trackSpeed;
		private System.Windows.Forms.Button buttonAnimation;
		private System.Windows.Forms.Panel groupControl2;
	}
}

