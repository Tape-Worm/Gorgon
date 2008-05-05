#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Friday, May 02, 2008 9:21:16 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class AnimationDropIn
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
				CleanUp();

			_display = null;
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.containerAnimation = new System.Windows.Forms.ToolStripContainer();
			this.splitAnimation = new System.Windows.Forms.SplitContainer();
			this.panelRender = new System.Windows.Forms.Panel();
			this.stripAnimation = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonSetKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonRemoveKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonClearKeys = new System.Windows.Forms.ToolStripButton();
			this.buttonCopyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonCutFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonPasteFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonFirstKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonPreviousKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonNextKeyframe = new System.Windows.Forms.ToolStripButton();
			this.buttonLastKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonPlay = new System.Windows.Forms.ToolStripButton();
			this.buttonStop = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.containerAnimation.ContentPanel.SuspendLayout();
			this.containerAnimation.TopToolStripPanel.SuspendLayout();
			this.containerAnimation.SuspendLayout();
			this.splitAnimation.Panel1.SuspendLayout();
			this.splitAnimation.SuspendLayout();
			this.stripAnimation.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerAnimation
			// 
			this.containerAnimation.BottomToolStripPanelVisible = false;
			// 
			// containerAnimation.ContentPanel
			// 
			this.containerAnimation.ContentPanel.Controls.Add(this.splitAnimation);
			this.containerAnimation.ContentPanel.Size = new System.Drawing.Size(629, 427);
			this.containerAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerAnimation.LeftToolStripPanelVisible = false;
			this.containerAnimation.Location = new System.Drawing.Point(0, 0);
			this.containerAnimation.Name = "containerAnimation";
			this.containerAnimation.RightToolStripPanelVisible = false;
			this.containerAnimation.Size = new System.Drawing.Size(629, 452);
			this.containerAnimation.TabIndex = 0;
			this.containerAnimation.Text = "toolStripContainer1";
			// 
			// containerAnimation.TopToolStripPanel
			// 
			this.containerAnimation.TopToolStripPanel.Controls.Add(this.stripAnimation);
			// 
			// splitAnimation
			// 
			this.splitAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitAnimation.Location = new System.Drawing.Point(0, 0);
			this.splitAnimation.Name = "splitAnimation";
			this.splitAnimation.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitAnimation.Panel1
			// 
			this.splitAnimation.Panel1.Controls.Add(this.panelRender);
			this.splitAnimation.Panel1.Padding = new System.Windows.Forms.Padding(3);
			// 
			// splitAnimation.Panel2
			// 
			this.splitAnimation.Panel2.Padding = new System.Windows.Forms.Padding(3);
			this.splitAnimation.Size = new System.Drawing.Size(629, 427);
			this.splitAnimation.SplitterDistance = 353;
			this.splitAnimation.SplitterWidth = 3;
			this.splitAnimation.TabIndex = 0;
			// 
			// panelRender
			// 
			this.panelRender.AllowDrop = true;
			this.panelRender.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.panelRender.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelRender.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelRender.Location = new System.Drawing.Point(3, 3);
			this.panelRender.Name = "panelRender";
			this.panelRender.Size = new System.Drawing.Size(623, 347);
			this.panelRender.TabIndex = 2;
			// 
			// stripAnimation
			// 
			this.stripAnimation.Dock = System.Windows.Forms.DockStyle.None;
			this.stripAnimation.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripAnimation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSetKeyFrame,
            this.toolStripSeparator4,
            this.buttonRemoveKeyFrame,
            this.buttonClearKeys,
            this.toolStripSeparator3,
            this.buttonCopyFrame,
            this.buttonCutFrame,
            this.buttonPasteFrame,
            this.toolStripSeparator1,
            this.buttonFirstKeyFrame,
            this.buttonPreviousKeyFrame,
            this.buttonNextKeyframe,
            this.buttonLastKeyFrame,
            this.toolStripSeparator2,
            this.buttonPlay,
            this.buttonStop});
			this.stripAnimation.Location = new System.Drawing.Point(0, 0);
			this.stripAnimation.Name = "stripAnimation";
			this.stripAnimation.Size = new System.Drawing.Size(629, 25);
			this.stripAnimation.Stretch = true;
			this.stripAnimation.TabIndex = 0;
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonSetKeyFrame
			// 
			this.buttonSetKeyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonSetKeyFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.add2;
			this.buttonSetKeyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonSetKeyFrame.Name = "buttonSetKeyFrame";
			this.buttonSetKeyFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonSetKeyFrame.Text = "Set key frame to the current time.";
			this.buttonSetKeyFrame.Click += new System.EventHandler(this.buttonSetKeyFrame_Click);
			// 
			// buttonRemoveKeyFrame
			// 
			this.buttonRemoveKeyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonRemoveKeyFrame.Enabled = false;
			this.buttonRemoveKeyFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete2;
			this.buttonRemoveKeyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonRemoveKeyFrame.Name = "buttonRemoveKeyFrame";
			this.buttonRemoveKeyFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonRemoveKeyFrame.Text = "Remove key frame from the current time.";
			this.buttonRemoveKeyFrame.Click += new System.EventHandler(this.buttonRemoveKeyFrame_Click);
			// 
			// buttonClearKeys
			// 
			this.buttonClearKeys.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonClearKeys.Enabled = false;
			this.buttonClearKeys.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.selection_delete;
			this.buttonClearKeys.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonClearKeys.Name = "buttonClearKeys";
			this.buttonClearKeys.Size = new System.Drawing.Size(23, 22);
			this.buttonClearKeys.Text = "Clear all keyframes.";
			this.buttonClearKeys.Click += new System.EventHandler(this.buttonClearKeys_Click);
			// 
			// buttonCopyFrame
			// 
			this.buttonCopyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonCopyFrame.Enabled = false;
			this.buttonCopyFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.copy;
			this.buttonCopyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonCopyFrame.Name = "buttonCopyFrame";
			this.buttonCopyFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonCopyFrame.Text = "Copy this keyframe to a new frame.";
			this.buttonCopyFrame.Click += new System.EventHandler(this.buttonCopyFrame_Click);
			// 
			// buttonCutFrame
			// 
			this.buttonCutFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonCutFrame.Enabled = false;
			this.buttonCutFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.cut;
			this.buttonCutFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonCutFrame.Name = "buttonCutFrame";
			this.buttonCutFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonCutFrame.Text = "Cut this frame for pasting elsewhere.";
			this.buttonCutFrame.Click += new System.EventHandler(this.buttonCutFrame_Click);
			// 
			// buttonPasteFrame
			// 
			this.buttonPasteFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPasteFrame.Enabled = false;
			this.buttonPasteFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.paste;
			this.buttonPasteFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPasteFrame.Name = "buttonPasteFrame";
			this.buttonPasteFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonPasteFrame.Text = "Paste a cut/copied key frame.";
			this.buttonPasteFrame.Click += new System.EventHandler(this.buttonPasteFrame_Click);
			// 
			// buttonFirstKeyFrame
			// 
			this.buttonFirstKeyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonFirstKeyFrame.Enabled = false;
			this.buttonFirstKeyFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.media_beginning;
			this.buttonFirstKeyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonFirstKeyFrame.Name = "buttonFirstKeyFrame";
			this.buttonFirstKeyFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonFirstKeyFrame.Text = "Goto first key frame.";
			this.buttonFirstKeyFrame.Click += new System.EventHandler(this.buttonFirstKeyFrame_Click);
			// 
			// buttonPreviousKeyFrame
			// 
			this.buttonPreviousKeyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPreviousKeyFrame.Enabled = false;
			this.buttonPreviousKeyFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.media_step_back;
			this.buttonPreviousKeyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPreviousKeyFrame.Name = "buttonPreviousKeyFrame";
			this.buttonPreviousKeyFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonPreviousKeyFrame.Text = "Go to previous key frame.";
			this.buttonPreviousKeyFrame.Click += new System.EventHandler(this.buttonPreviousKeyFrame_Click);
			// 
			// buttonNextKeyframe
			// 
			this.buttonNextKeyframe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonNextKeyframe.Enabled = false;
			this.buttonNextKeyframe.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.media_step_forward;
			this.buttonNextKeyframe.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonNextKeyframe.Name = "buttonNextKeyframe";
			this.buttonNextKeyframe.Size = new System.Drawing.Size(23, 22);
			this.buttonNextKeyframe.Text = "Go to next key frame.";
			this.buttonNextKeyframe.Click += new System.EventHandler(this.buttonNextKeyframe_Click);
			// 
			// buttonLastKeyFrame
			// 
			this.buttonLastKeyFrame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonLastKeyFrame.Enabled = false;
			this.buttonLastKeyFrame.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.media_end;
			this.buttonLastKeyFrame.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonLastKeyFrame.Name = "buttonLastKeyFrame";
			this.buttonLastKeyFrame.Size = new System.Drawing.Size(23, 22);
			this.buttonLastKeyFrame.Text = "Go to last key frame.";
			this.buttonLastKeyFrame.Click += new System.EventHandler(this.buttonLastKeyFrame_Click);
			// 
			// buttonPlay
			// 
			this.buttonPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonPlay.Enabled = false;
			this.buttonPlay.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.media_play;
			this.buttonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonPlay.Name = "buttonPlay";
			this.buttonPlay.Size = new System.Drawing.Size(23, 22);
			this.buttonPlay.Text = "Play animation.";
			this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
			// 
			// buttonStop
			// 
			this.buttonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonStop.Enabled = false;
			this.buttonStop.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.media_stop_red;
			this.buttonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonStop.Name = "buttonStop";
			this.buttonStop.Size = new System.Drawing.Size(23, 22);
			this.buttonStop.Text = "Stop animation.";
			this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// AnimationDropIn
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.containerAnimation);
			this.Name = "AnimationDropIn";
			this.Size = new System.Drawing.Size(629, 452);
			this.containerAnimation.ContentPanel.ResumeLayout(false);
			this.containerAnimation.TopToolStripPanel.ResumeLayout(false);
			this.containerAnimation.TopToolStripPanel.PerformLayout();
			this.containerAnimation.ResumeLayout(false);
			this.containerAnimation.PerformLayout();
			this.splitAnimation.Panel1.ResumeLayout(false);
			this.splitAnimation.ResumeLayout(false);
			this.stripAnimation.ResumeLayout(false);
			this.stripAnimation.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		protected System.Windows.Forms.ToolStripButton buttonSetKeyFrame;
		protected System.Windows.Forms.ToolStripButton buttonRemoveKeyFrame;
		protected System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		protected System.Windows.Forms.ToolStripButton buttonFirstKeyFrame;
		protected System.Windows.Forms.ToolStripButton buttonPreviousKeyFrame;
		protected System.Windows.Forms.ToolStripButton buttonNextKeyframe;
		protected System.Windows.Forms.ToolStripButton buttonLastKeyFrame;
		protected System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		protected System.Windows.Forms.ToolStripButton buttonPlay;
		protected System.Windows.Forms.ToolStripButton buttonStop;
		protected System.Windows.Forms.ToolStripButton buttonClearKeys;
		protected System.Windows.Forms.ToolStripContainer containerAnimation;
		protected System.Windows.Forms.SplitContainer splitAnimation;
		protected System.Windows.Forms.ToolStrip stripAnimation;
		protected System.Windows.Forms.Panel panelRender;
		private System.Windows.Forms.ToolStripButton buttonCopyFrame;
		private System.Windows.Forms.ToolStripButton buttonPasteFrame;
		private System.Windows.Forms.ToolStripButton buttonCutFrame;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
	}
}
