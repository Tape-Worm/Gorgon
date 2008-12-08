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
// Created: Tuesday, July 10, 2007 11:59:27 AM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class formAnimationEditor
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formAnimationEditor));
			this.splitTrack = new System.Windows.Forms.SplitContainer();
			this.panelTrackControls = new System.Windows.Forms.Panel();
			this.numericTrackFrames = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.comboTrack = new System.Windows.Forms.ComboBox();
			this.trackTrack = new System.Windows.Forms.TrackBar();
			this.containerStatus = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.splitTrackView = new System.Windows.Forms.SplitContainer();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label3 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.checkShowAll = new System.Windows.Forms.CheckBox();
			this.panelTrackNames = new GorgonLibrary.Graphics.Tools.BetterPanel();
			this.panelTrackDisplay = new GorgonLibrary.Graphics.Tools.BetterPanel();
			this.splitTrack.Panel2.SuspendLayout();
			this.splitTrack.SuspendLayout();
			this.panelTrackControls.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTrackFrames)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackTrack)).BeginInit();
			this.containerStatus.BottomToolStripPanel.SuspendLayout();
			this.containerStatus.ContentPanel.SuspendLayout();
			this.containerStatus.SuspendLayout();
			this.splitTrackView.Panel1.SuspendLayout();
			this.splitTrackView.Panel2.SuspendLayout();
			this.splitTrackView.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitTrack
			// 
			this.splitTrack.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitTrack.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitTrack.IsSplitterFixed = true;
			this.splitTrack.Location = new System.Drawing.Point(0, 0);
			this.splitTrack.Name = "splitTrack";
			this.splitTrack.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitTrack.Panel1
			// 
			this.splitTrack.Panel1.Padding = new System.Windows.Forms.Padding(2);
			// 
			// splitTrack.Panel2
			// 
			this.splitTrack.Panel2.Controls.Add(this.panelTrackControls);
			this.splitTrack.Panel2.Padding = new System.Windows.Forms.Padding(2);
			this.splitTrack.Size = new System.Drawing.Size(684, 393);
			this.splitTrack.SplitterDistance = 303;
			this.splitTrack.TabIndex = 0;
			// 
			// panelTrackControls
			// 
			this.panelTrackControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTrackControls.Controls.Add(this.numericTrackFrames);
			this.panelTrackControls.Controls.Add(this.label1);
			this.panelTrackControls.Controls.Add(this.label2);
			this.panelTrackControls.Controls.Add(this.comboTrack);
			this.panelTrackControls.Controls.Add(this.trackTrack);
			this.panelTrackControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTrackControls.Location = new System.Drawing.Point(2, 2);
			this.panelTrackControls.Margin = new System.Windows.Forms.Padding(6);
			this.panelTrackControls.Name = "panelTrackControls";
			this.panelTrackControls.Size = new System.Drawing.Size(680, 82);
			this.panelTrackControls.TabIndex = 0;
			// 
			// numericTrackFrames
			// 
			this.numericTrackFrames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericTrackFrames.Location = new System.Drawing.Point(287, 46);
			this.numericTrackFrames.Maximum = new decimal(new int[] {
            86000000,
            0,
            0,
            0});
			this.numericTrackFrames.Name = "numericTrackFrames";
			this.numericTrackFrames.Size = new System.Drawing.Size(76, 20);
			this.numericTrackFrames.TabIndex = 0;
			this.numericTrackFrames.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericTrackFrames.ValueChanged += new System.EventHandler(this.numericTrackTime_ValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 13;
			this.label1.Text = "Track type:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(208, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(73, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Current frame:";
			// 
			// comboTrack
			// 
			this.comboTrack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboTrack.FormattingEnabled = true;
			this.comboTrack.Location = new System.Drawing.Point(77, 45);
			this.comboTrack.Name = "comboTrack";
			this.comboTrack.Size = new System.Drawing.Size(125, 21);
			this.comboTrack.TabIndex = 1;
			this.comboTrack.SelectedIndexChanged += new System.EventHandler(this.comboTrack_SelectedIndexChanged);
			// 
			// trackTrack
			// 
			this.trackTrack.BackColor = System.Drawing.SystemColors.Control;
			this.trackTrack.Dock = System.Windows.Forms.DockStyle.Top;
			this.trackTrack.Location = new System.Drawing.Point(0, 0);
			this.trackTrack.Margin = new System.Windows.Forms.Padding(0);
			this.trackTrack.Name = "trackTrack";
			this.trackTrack.Size = new System.Drawing.Size(678, 45);
			this.trackTrack.TabIndex = 0;
			this.trackTrack.Scroll += new System.EventHandler(this.trackTrack_Scroll);
			// 
			// containerStatus
			// 
			// 
			// containerStatus.BottomToolStripPanel
			// 
			this.containerStatus.BottomToolStripPanel.Controls.Add(this.statusStrip1);
			// 
			// containerStatus.ContentPanel
			// 
			this.containerStatus.ContentPanel.Controls.Add(this.splitTrackView);
			this.containerStatus.ContentPanel.Size = new System.Drawing.Size(684, 525);
			this.containerStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerStatus.LeftToolStripPanelVisible = false;
			this.containerStatus.Location = new System.Drawing.Point(0, 0);
			this.containerStatus.Name = "containerStatus";
			this.containerStatus.RightToolStripPanelVisible = false;
			this.containerStatus.Size = new System.Drawing.Size(684, 547);
			this.containerStatus.TabIndex = 1;
			this.containerStatus.Text = "toolStripContainer1";
			this.containerStatus.TopToolStripPanelVisible = false;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.statusStrip1.Location = new System.Drawing.Point(0, 0);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(684, 22);
			this.statusStrip1.TabIndex = 0;
			// 
			// splitTrackView
			// 
			this.splitTrackView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitTrackView.Location = new System.Drawing.Point(0, 0);
			this.splitTrackView.Name = "splitTrackView";
			this.splitTrackView.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitTrackView.Panel1
			// 
			this.splitTrackView.Panel1.Controls.Add(this.splitTrack);
			this.splitTrackView.Panel1MinSize = 380;
			// 
			// splitTrackView.Panel2
			// 
			this.splitTrackView.Panel2.Controls.Add(this.panelTrackNames);
			this.splitTrackView.Panel2.Controls.Add(this.panelTrackDisplay);
			this.splitTrackView.Panel2.Controls.Add(this.panel1);
			this.splitTrackView.Panel2.Controls.Add(this.panel2);
			this.splitTrackView.Panel2.Padding = new System.Windows.Forms.Padding(2);
			this.splitTrackView.Size = new System.Drawing.Size(684, 525);
			this.splitTrackView.SplitterDistance = 393;
			this.splitTrackView.TabIndex = 1;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.label3);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(2, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(680, 19);
			this.panel1.TabIndex = 0;
			// 
			// label3
			// 
			this.label3.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label3.Dock = System.Windows.Forms.DockStyle.Top;
			this.label3.Location = new System.Drawing.Point(0, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(680, 19);
			this.label3.TabIndex = 0;
			this.label3.Text = "Track View";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.checkShowAll);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel2.Location = new System.Drawing.Point(2, 102);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(680, 24);
			this.panel2.TabIndex = 2;
			// 
			// checkShowAll
			// 
			this.checkShowAll.AutoSize = true;
			this.checkShowAll.Checked = true;
			this.checkShowAll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowAll.Location = new System.Drawing.Point(6, 6);
			this.checkShowAll.Name = "checkShowAll";
			this.checkShowAll.Size = new System.Drawing.Size(98, 17);
			this.checkShowAll.TabIndex = 2;
			this.checkShowAll.Text = "Show all tracks";
			this.checkShowAll.UseVisualStyleBackColor = true;
			this.checkShowAll.Click += new System.EventHandler(this.checkShowAll_Click);
			// 
			// panelTrackNames
			// 
			this.panelTrackNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.panelTrackNames.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.panelTrackNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTrackNames.Location = new System.Drawing.Point(2, 20);
			this.panelTrackNames.Name = "panelTrackNames";
			this.panelTrackNames.ShowScrollBars = false;
			this.panelTrackNames.Size = new System.Drawing.Size(149, 82);
			this.panelTrackNames.TabIndex = 3;
			this.panelTrackNames.Resize += new System.EventHandler(this.panelTrackNames_Resize);
			// 
			// panelTrackDisplay
			// 
			this.panelTrackDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelTrackDisplay.AutoScroll = true;
			this.panelTrackDisplay.AutoScrollMargin = new System.Drawing.Size(3, 3);
			this.panelTrackDisplay.AutoScrollMinSize = new System.Drawing.Size(3, 3);
			this.panelTrackDisplay.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.panelTrackDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTrackDisplay.Location = new System.Drawing.Point(150, 20);
			this.panelTrackDisplay.Name = "panelTrackDisplay";
			this.panelTrackDisplay.ShowScrollBars = false;
			this.panelTrackDisplay.Size = new System.Drawing.Size(532, 82);
			this.panelTrackDisplay.TabIndex = 1;
			this.panelTrackDisplay.Scroll += new System.Windows.Forms.ScrollEventHandler(this.panelTrackDisplay_Scroll);
			// 
			// formAnimationEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(684, 547);
			this.Controls.Add(this.containerStatus);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "formAnimationEditor";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Animation editor - ";
			this.splitTrack.Panel2.ResumeLayout(false);
			this.splitTrack.ResumeLayout(false);
			this.panelTrackControls.ResumeLayout(false);
			this.panelTrackControls.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTrackFrames)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackTrack)).EndInit();
			this.containerStatus.BottomToolStripPanel.ResumeLayout(false);
			this.containerStatus.BottomToolStripPanel.PerformLayout();
			this.containerStatus.ContentPanel.ResumeLayout(false);
			this.containerStatus.ResumeLayout(false);
			this.containerStatus.PerformLayout();
			this.splitTrackView.Panel1.ResumeLayout(false);
			this.splitTrackView.Panel2.ResumeLayout(false);
			this.splitTrackView.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitTrack;
		private System.Windows.Forms.Panel panelTrackControls;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboTrack;
		private System.Windows.Forms.NumericUpDown numericTrackFrames;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TrackBar trackTrack;
		private System.Windows.Forms.ToolStripContainer containerStatus;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.SplitContainer splitTrackView;
		private BetterPanel panelTrackDisplay;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.CheckBox checkShowAll;
		private BetterPanel panelTrackNames;

	}
}