#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
			this.label1 = new System.Windows.Forms.Label();
			this.comboTrack = new System.Windows.Forms.ComboBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.labelUnit = new System.Windows.Forms.Label();
			this.numericTrackTime = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.radioMinutes = new System.Windows.Forms.RadioButton();
			this.radioSeconds = new System.Windows.Forms.RadioButton();
			this.radioMilliseconds = new System.Windows.Forms.RadioButton();
			this.trackTrack = new System.Windows.Forms.TrackBar();
			this.containerStatus = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.splitTrack.Panel2.SuspendLayout();
			this.splitTrack.SuspendLayout();
			this.panelTrackControls.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTrackTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackTrack)).BeginInit();
			this.containerStatus.BottomToolStripPanel.SuspendLayout();
			this.containerStatus.ContentPanel.SuspendLayout();
			this.containerStatus.SuspendLayout();
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
			this.splitTrack.Size = new System.Drawing.Size(684, 422);
			this.splitTrack.SplitterDistance = 310;
			this.splitTrack.TabIndex = 0;
			// 
			// panelTrackControls
			// 
			this.panelTrackControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTrackControls.Controls.Add(this.label1);
			this.panelTrackControls.Controls.Add(this.comboTrack);
			this.panelTrackControls.Controls.Add(this.groupBox1);
			this.panelTrackControls.Controls.Add(this.trackTrack);
			this.panelTrackControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTrackControls.Location = new System.Drawing.Point(2, 2);
			this.panelTrackControls.Margin = new System.Windows.Forms.Padding(6);
			this.panelTrackControls.Name = "panelTrackControls";
			this.panelTrackControls.Size = new System.Drawing.Size(680, 104);
			this.panelTrackControls.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 68);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 13);
			this.label1.TabIndex = 13;
			this.label1.Text = "Track type:";
			// 
			// comboTrack
			// 
			this.comboTrack.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboTrack.FormattingEnabled = true;
			this.comboTrack.Items.AddRange(new object[] {
            "Animation frames",
            "Color",
            "Transformation"});
			this.comboTrack.Location = new System.Drawing.Point(75, 65);
			this.comboTrack.Name = "comboTrack";
			this.comboTrack.Size = new System.Drawing.Size(125, 21);
			this.comboTrack.TabIndex = 1;
			this.comboTrack.SelectedIndexChanged += new System.EventHandler(this.comboTrack_SelectedIndexChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.labelUnit);
			this.groupBox1.Controls.Add(this.numericTrackTime);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.radioMinutes);
			this.groupBox1.Controls.Add(this.radioSeconds);
			this.groupBox1.Controls.Add(this.radioMilliseconds);
			this.groupBox1.Location = new System.Drawing.Point(228, 47);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(442, 45);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Time";
			// 
			// labelUnit
			// 
			this.labelUnit.AutoSize = true;
			this.labelUnit.Location = new System.Drawing.Point(158, 21);
			this.labelUnit.Name = "labelUnit";
			this.labelUnit.Size = new System.Drawing.Size(35, 13);
			this.labelUnit.TabIndex = 11;
			this.labelUnit.Text = "msec.";
			// 
			// numericTrackTime
			// 
			this.numericTrackTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericTrackTime.DecimalPlaces = 3;
			this.numericTrackTime.Location = new System.Drawing.Point(76, 19);
			this.numericTrackTime.Maximum = new decimal(new int[] {
            86000000,
            0,
            0,
            0});
			this.numericTrackTime.Name = "numericTrackTime";
			this.numericTrackTime.Size = new System.Drawing.Size(76, 20);
			this.numericTrackTime.TabIndex = 0;
			this.numericTrackTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericTrackTime.ValueChanged += new System.EventHandler(this.numericTrackTime_ValueChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 21);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Current time:";
			// 
			// radioMinutes
			// 
			this.radioMinutes.AutoSize = true;
			this.radioMinutes.Location = new System.Drawing.Point(374, 19);
			this.radioMinutes.Name = "radioMinutes";
			this.radioMinutes.Size = new System.Drawing.Size(62, 17);
			this.radioMinutes.TabIndex = 3;
			this.radioMinutes.Text = "Minutes";
			this.radioMinutes.UseVisualStyleBackColor = true;
			this.radioMinutes.CheckedChanged += new System.EventHandler(this.UnitUpdate);
			// 
			// radioSeconds
			// 
			this.radioSeconds.AutoSize = true;
			this.radioSeconds.Location = new System.Drawing.Point(301, 19);
			this.radioSeconds.Name = "radioSeconds";
			this.radioSeconds.Size = new System.Drawing.Size(67, 17);
			this.radioSeconds.TabIndex = 2;
			this.radioSeconds.Text = "Seconds";
			this.radioSeconds.UseVisualStyleBackColor = true;
			this.radioSeconds.CheckedChanged += new System.EventHandler(this.UnitUpdate);
			// 
			// radioMilliseconds
			// 
			this.radioMilliseconds.AutoSize = true;
			this.radioMilliseconds.Checked = true;
			this.radioMilliseconds.Location = new System.Drawing.Point(213, 19);
			this.radioMilliseconds.Name = "radioMilliseconds";
			this.radioMilliseconds.Size = new System.Drawing.Size(82, 17);
			this.radioMilliseconds.TabIndex = 1;
			this.radioMilliseconds.TabStop = true;
			this.radioMilliseconds.Text = "Milliseconds";
			this.radioMilliseconds.UseVisualStyleBackColor = true;
			this.radioMilliseconds.CheckedChanged += new System.EventHandler(this.UnitUpdate);
			// 
			// trackTrack
			// 
			this.trackTrack.BackColor = System.Drawing.SystemColors.Control;
			this.trackTrack.Dock = System.Windows.Forms.DockStyle.Top;
			this.trackTrack.Location = new System.Drawing.Point(0, 0);
			this.trackTrack.Name = "trackTrack";
			this.trackTrack.Size = new System.Drawing.Size(678, 45);
			this.trackTrack.TabIndex = 0;
			this.trackTrack.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
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
			this.containerStatus.ContentPanel.Controls.Add(this.splitTrack);
			this.containerStatus.ContentPanel.Size = new System.Drawing.Size(684, 422);
			this.containerStatus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerStatus.LeftToolStripPanelVisible = false;
			this.containerStatus.Location = new System.Drawing.Point(0, 0);
			this.containerStatus.Name = "containerStatus";
			this.containerStatus.RightToolStripPanelVisible = false;
			this.containerStatus.Size = new System.Drawing.Size(684, 444);
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
			// formAnimationEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(684, 444);
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
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTrackTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackTrack)).EndInit();
			this.containerStatus.BottomToolStripPanel.ResumeLayout(false);
			this.containerStatus.BottomToolStripPanel.PerformLayout();
			this.containerStatus.ContentPanel.ResumeLayout(false);
			this.containerStatus.ResumeLayout(false);
			this.containerStatus.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitTrack;
		private System.Windows.Forms.Panel panelTrackControls;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboTrack;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label labelUnit;
		private System.Windows.Forms.NumericUpDown numericTrackTime;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioMinutes;
		private System.Windows.Forms.RadioButton radioSeconds;
		private System.Windows.Forms.RadioButton radioMilliseconds;
		private System.Windows.Forms.TrackBar trackTrack;
		private System.Windows.Forms.ToolStripContainer containerStatus;
		private System.Windows.Forms.StatusStrip statusStrip1;

	}
}