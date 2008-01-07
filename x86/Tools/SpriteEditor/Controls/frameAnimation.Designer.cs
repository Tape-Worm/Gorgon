namespace GorgonLibrary.Graphics.Tools
{
	partial class frameAnimation
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
				if (_animation != null)
				{
					_animation.CurrentTime = 0.0f;
					_animation.AnimationState = AnimationState.Stopped;
					_animation.AnimationStopped -= new System.EventHandler(_animation_AnimationStopped);
				}

				CleanUp();

				if (_current != null)
					_current.Dispose();
			}

			_animationDisplay = null;
			_current = null;
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.containerFrameAnimation = new System.Windows.Forms.ToolStripContainer();
			this.splitFrameAnimation = new System.Windows.Forms.SplitContainer();
			this.panelRender = new System.Windows.Forms.Panel();
			this.listFrames = new System.Windows.Forms.ListView();
			this.imagesSprites = new System.Windows.Forms.ImageList(this.components);
			this.stripFrameAnimation = new System.Windows.Forms.ToolStrip();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonClearKeys = new System.Windows.Forms.ToolStripButton();
			this.buttonSetKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonRemoveKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonFirstKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonPreviousKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonNextKeyframe = new System.Windows.Forms.ToolStripButton();
			this.buttonLastKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonPlay = new System.Windows.Forms.ToolStripButton();
			this.buttonStop = new System.Windows.Forms.ToolStripButton();
			this.buttonGetAll = new System.Windows.Forms.ToolStripButton();
			this.containerFrameAnimation.ContentPanel.SuspendLayout();
			this.containerFrameAnimation.TopToolStripPanel.SuspendLayout();
			this.containerFrameAnimation.SuspendLayout();
			this.splitFrameAnimation.Panel1.SuspendLayout();
			this.splitFrameAnimation.Panel2.SuspendLayout();
			this.splitFrameAnimation.SuspendLayout();
			this.stripFrameAnimation.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerFrameAnimation
			// 
			this.containerFrameAnimation.BottomToolStripPanelVisible = false;
			// 
			// containerFrameAnimation.ContentPanel
			// 
			this.containerFrameAnimation.ContentPanel.Controls.Add(this.splitFrameAnimation);
			this.containerFrameAnimation.ContentPanel.Size = new System.Drawing.Size(629, 427);
			this.containerFrameAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerFrameAnimation.LeftToolStripPanelVisible = false;
			this.containerFrameAnimation.Location = new System.Drawing.Point(0, 0);
			this.containerFrameAnimation.Name = "containerFrameAnimation";
			this.containerFrameAnimation.RightToolStripPanelVisible = false;
			this.containerFrameAnimation.Size = new System.Drawing.Size(629, 452);
			this.containerFrameAnimation.TabIndex = 0;
			this.containerFrameAnimation.Text = "toolStripContainer1";
			// 
			// containerFrameAnimation.TopToolStripPanel
			// 
			this.containerFrameAnimation.TopToolStripPanel.Controls.Add(this.stripFrameAnimation);
			// 
			// splitFrameAnimation
			// 
			this.splitFrameAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitFrameAnimation.Location = new System.Drawing.Point(0, 0);
			this.splitFrameAnimation.Name = "splitFrameAnimation";
			this.splitFrameAnimation.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitFrameAnimation.Panel1
			// 
			this.splitFrameAnimation.Panel1.Controls.Add(this.panelRender);
			this.splitFrameAnimation.Panel1.Padding = new System.Windows.Forms.Padding(3);
			// 
			// splitFrameAnimation.Panel2
			// 
			this.splitFrameAnimation.Panel2.Controls.Add(this.listFrames);
			this.splitFrameAnimation.Panel2.Padding = new System.Windows.Forms.Padding(3);
			this.splitFrameAnimation.Size = new System.Drawing.Size(629, 427);
			this.splitFrameAnimation.SplitterDistance = 274;
			this.splitFrameAnimation.SplitterWidth = 3;
			this.splitFrameAnimation.TabIndex = 0;
			// 
			// panelRender
			// 
			this.panelRender.AllowDrop = true;
			this.panelRender.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.panelRender.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelRender.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelRender.Location = new System.Drawing.Point(3, 3);
			this.panelRender.Name = "panelRender";
			this.panelRender.Size = new System.Drawing.Size(623, 268);
			this.panelRender.TabIndex = 2;
			this.panelRender.DragOver += new System.Windows.Forms.DragEventHandler(this.panelRender_DragOver);
			this.panelRender.DragDrop += new System.Windows.Forms.DragEventHandler(this.panelRender_DragDrop);
			// 
			// listFrames
			// 
			this.listFrames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listFrames.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listFrames.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listFrames.HideSelection = false;
			this.listFrames.LargeImageList = this.imagesSprites;
			this.listFrames.Location = new System.Drawing.Point(3, 3);
			this.listFrames.Name = "listFrames";
			this.listFrames.ShowItemToolTips = true;
			this.listFrames.Size = new System.Drawing.Size(623, 144);
			this.listFrames.SmallImageList = this.imagesSprites;
			this.listFrames.TabIndex = 0;
			this.listFrames.TileSize = new System.Drawing.Size(66, 66);
			this.listFrames.UseCompatibleStateImageBehavior = false;
			this.listFrames.SelectedIndexChanged += new System.EventHandler(this.listFrames_SelectedIndexChanged);
			this.listFrames.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listFrames_ItemDrag);
			// 
			// imagesSprites
			// 
			this.imagesSprites.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imagesSprites.ImageSize = new System.Drawing.Size(64, 64);
			this.imagesSprites.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// stripFrameAnimation
			// 
			this.stripFrameAnimation.Dock = System.Windows.Forms.DockStyle.None;
			this.stripFrameAnimation.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.stripFrameAnimation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonClearKeys,
            this.buttonSetKeyFrame,
            this.buttonRemoveKeyFrame,
            this.toolStripSeparator1,
            this.buttonFirstKeyFrame,
            this.buttonPreviousKeyFrame,
            this.buttonNextKeyframe,
            this.buttonLastKeyFrame,
            this.toolStripSeparator2,
            this.buttonPlay,
            this.buttonStop,
            this.toolStripSeparator3,
            this.buttonGetAll});
			this.stripFrameAnimation.Location = new System.Drawing.Point(0, 0);
			this.stripFrameAnimation.Name = "stripFrameAnimation";
			this.stripFrameAnimation.Size = new System.Drawing.Size(629, 25);
			this.stripFrameAnimation.Stretch = true;
			this.stripFrameAnimation.TabIndex = 0;
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
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// buttonClearKeys
			// 
			this.buttonClearKeys.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonClearKeys.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.document_dirty;
			this.buttonClearKeys.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonClearKeys.Name = "buttonClearKeys";
			this.buttonClearKeys.Size = new System.Drawing.Size(23, 22);
			this.buttonClearKeys.Text = "Clear all keyframes.";
			this.buttonClearKeys.Click += new System.EventHandler(this.buttonClearKeys_Click);
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
			// buttonGetAll
			// 
			this.buttonGetAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.buttonGetAll.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.import1;
			this.buttonGetAll.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.buttonGetAll.Name = "buttonGetAll";
			this.buttonGetAll.Size = new System.Drawing.Size(23, 22);
			this.buttonGetAll.Text = "Use all sprites.";
			this.buttonGetAll.Click += new System.EventHandler(this.buttonGetAll_Click);
			// 
			// frameAnimation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.containerFrameAnimation);
			this.Name = "frameAnimation";
			this.Size = new System.Drawing.Size(629, 452);
			this.containerFrameAnimation.ContentPanel.ResumeLayout(false);
			this.containerFrameAnimation.TopToolStripPanel.ResumeLayout(false);
			this.containerFrameAnimation.TopToolStripPanel.PerformLayout();
			this.containerFrameAnimation.ResumeLayout(false);
			this.containerFrameAnimation.PerformLayout();
			this.splitFrameAnimation.Panel1.ResumeLayout(false);
			this.splitFrameAnimation.Panel2.ResumeLayout(false);
			this.splitFrameAnimation.ResumeLayout(false);
			this.stripFrameAnimation.ResumeLayout(false);
			this.stripFrameAnimation.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer containerFrameAnimation;
		private System.Windows.Forms.ToolStrip stripFrameAnimation;
		private System.Windows.Forms.ToolStripButton buttonSetKeyFrame;
		private System.Windows.Forms.ToolStripButton buttonRemoveKeyFrame;
		private System.Windows.Forms.SplitContainer splitFrameAnimation;
		private System.Windows.Forms.ListView listFrames;
		private System.Windows.Forms.ImageList imagesSprites;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton buttonFirstKeyFrame;
		private System.Windows.Forms.ToolStripButton buttonPreviousKeyFrame;
		private System.Windows.Forms.ToolStripButton buttonNextKeyframe;
		private System.Windows.Forms.ToolStripButton buttonLastKeyFrame;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton buttonPlay;
		private System.Windows.Forms.ToolStripButton buttonStop;
		private System.Windows.Forms.Panel panelRender;
		private System.Windows.Forms.ToolStripButton buttonClearKeys;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton buttonGetAll;
	}
}
