namespace GorgonLibrary.Graphics.Tools
{
	partial class colorAnimation
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
			this.components = new System.ComponentModel.Container();
			this.containerFrameAnimation = new System.Windows.Forms.ToolStripContainer();
			this.splitRender = new System.Windows.Forms.SplitContainer();
			this.panelRender = new System.Windows.Forms.Panel();
			this.panelColorControls = new System.Windows.Forms.Panel();
			this.tabColorControls = new System.Windows.Forms.TabControl();
			this.pageColors = new System.Windows.Forms.TabPage();
			this.label7 = new System.Windows.Forms.Label();
			this.numericB = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.numericG = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.numericR = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.numericA = new System.Windows.Forms.NumericUpDown();
			this.pictureColor = new System.Windows.Forms.PictureBox();
			this.pageMasking = new System.Windows.Forms.TabPage();
			this.label2 = new System.Windows.Forms.Label();
			this.numericAlpha = new System.Windows.Forms.NumericUpDown();
			this.comboInterpolation = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.stripFrameAnimation = new System.Windows.Forms.ToolStrip();
			this.buttonClearKeys = new System.Windows.Forms.ToolStripButton();
			this.buttonSetKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonRemoveKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonFirstKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonPreviousKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.buttonNextKeyframe = new System.Windows.Forms.ToolStripButton();
			this.buttonLastKeyFrame = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.buttonPlay = new System.Windows.Forms.ToolStripButton();
			this.buttonStop = new System.Windows.Forms.ToolStripButton();
			this.imagesSprites = new System.Windows.Forms.ImageList(this.components);
			this.containerFrameAnimation.ContentPanel.SuspendLayout();
			this.containerFrameAnimation.TopToolStripPanel.SuspendLayout();
			this.containerFrameAnimation.SuspendLayout();
			this.splitRender.Panel1.SuspendLayout();
			this.splitRender.Panel2.SuspendLayout();
			this.splitRender.SuspendLayout();
			this.panelColorControls.SuspendLayout();
			this.tabColorControls.SuspendLayout();
			this.pageColors.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericB)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericG)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericR)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericA)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureColor)).BeginInit();
			this.pageMasking.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAlpha)).BeginInit();
			this.stripFrameAnimation.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerFrameAnimation
			// 
			// 
			// containerFrameAnimation.ContentPanel
			// 
			this.containerFrameAnimation.ContentPanel.Controls.Add(this.splitRender);
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
			// splitRender
			// 
			this.splitRender.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitRender.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitRender.Location = new System.Drawing.Point(0, 0);
			this.splitRender.Name = "splitRender";
			this.splitRender.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitRender.Panel1
			// 
			this.splitRender.Panel1.Controls.Add(this.panelRender);
			// 
			// splitRender.Panel2
			// 
			this.splitRender.Panel2.Controls.Add(this.panelColorControls);
			this.splitRender.Size = new System.Drawing.Size(629, 427);
			this.splitRender.SplitterDistance = 276;
			this.splitRender.TabIndex = 0;
			// 
			// panelRender
			// 
			this.panelRender.AllowDrop = true;
			this.panelRender.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.panelRender.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelRender.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelRender.Location = new System.Drawing.Point(0, 0);
			this.panelRender.Name = "panelRender";
			this.panelRender.Size = new System.Drawing.Size(629, 276);
			this.panelRender.TabIndex = 4;
			// 
			// panelColorControls
			// 
			this.panelColorControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelColorControls.Controls.Add(this.tabColorControls);
			this.panelColorControls.Controls.Add(this.comboInterpolation);
			this.panelColorControls.Controls.Add(this.label3);
			this.panelColorControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelColorControls.Location = new System.Drawing.Point(0, 0);
			this.panelColorControls.Name = "panelColorControls";
			this.panelColorControls.Size = new System.Drawing.Size(629, 147);
			this.panelColorControls.TabIndex = 0;
			// 
			// tabColorControls
			// 
			this.tabColorControls.Controls.Add(this.pageColors);
			this.tabColorControls.Controls.Add(this.pageMasking);
			this.tabColorControls.Location = new System.Drawing.Point(3, 3);
			this.tabColorControls.Name = "tabColorControls";
			this.tabColorControls.SelectedIndex = 0;
			this.tabColorControls.Size = new System.Drawing.Size(621, 100);
			this.tabColorControls.TabIndex = 7;
			// 
			// pageColors
			// 
			this.pageColors.Controls.Add(this.label7);
			this.pageColors.Controls.Add(this.numericB);
			this.pageColors.Controls.Add(this.label6);
			this.pageColors.Controls.Add(this.numericG);
			this.pageColors.Controls.Add(this.label5);
			this.pageColors.Controls.Add(this.numericR);
			this.pageColors.Controls.Add(this.label4);
			this.pageColors.Controls.Add(this.numericA);
			this.pageColors.Controls.Add(this.pictureColor);
			this.pageColors.Location = new System.Drawing.Point(4, 22);
			this.pageColors.Name = "pageColors";
			this.pageColors.Padding = new System.Windows.Forms.Padding(3);
			this.pageColors.Size = new System.Drawing.Size(613, 74);
			this.pageColors.TabIndex = 0;
			this.pageColors.Text = "Colors";
			this.pageColors.UseVisualStyleBackColor = true;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(186, 6);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(28, 13);
			this.label7.TabIndex = 11;
			this.label7.Text = "Blue";
			// 
			// numericB
			// 
			this.numericB.Location = new System.Drawing.Point(189, 21);
			this.numericB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericB.Name = "numericB";
			this.numericB.Size = new System.Drawing.Size(54, 20);
			this.numericB.TabIndex = 10;
			this.numericB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericB.ValueChanged += new System.EventHandler(this.colorComponent_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(126, 6);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(36, 13);
			this.label6.TabIndex = 9;
			this.label6.Text = "Green";
			// 
			// numericG
			// 
			this.numericG.Location = new System.Drawing.Point(129, 21);
			this.numericG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericG.Name = "numericG";
			this.numericG.Size = new System.Drawing.Size(54, 20);
			this.numericG.TabIndex = 8;
			this.numericG.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericG.ValueChanged += new System.EventHandler(this.colorComponent_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(68, 6);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(27, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "Red";
			// 
			// numericR
			// 
			this.numericR.Location = new System.Drawing.Point(69, 22);
			this.numericR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericR.Name = "numericR";
			this.numericR.Size = new System.Drawing.Size(54, 20);
			this.numericR.TabIndex = 6;
			this.numericR.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericR.ValueChanged += new System.EventHandler(this.colorComponent_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(6, 6);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(34, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "Alpha";
			// 
			// numericA
			// 
			this.numericA.Location = new System.Drawing.Point(9, 22);
			this.numericA.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericA.Name = "numericA";
			this.numericA.Size = new System.Drawing.Size(54, 20);
			this.numericA.TabIndex = 4;
			this.numericA.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericA.ValueChanged += new System.EventHandler(this.colorComponent_ValueChanged);
			// 
			// pictureColor
			// 
			this.pictureColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureColor.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.ColorBar;
			this.pictureColor.Location = new System.Drawing.Point(296, 6);
			this.pictureColor.Name = "pictureColor";
			this.pictureColor.Size = new System.Drawing.Size(311, 62);
			this.pictureColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureColor.TabIndex = 2;
			this.pictureColor.TabStop = false;
			this.pictureColor.DoubleClick += new System.EventHandler(this.pictureColor_DoubleClick);
			this.pictureColor.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureColor_Paint);
			// 
			// pageMasking
			// 
			this.pageMasking.Controls.Add(this.label2);
			this.pageMasking.Controls.Add(this.numericAlpha);
			this.pageMasking.Location = new System.Drawing.Point(4, 22);
			this.pageMasking.Name = "pageMasking";
			this.pageMasking.Padding = new System.Windows.Forms.Padding(3);
			this.pageMasking.Size = new System.Drawing.Size(613, 74);
			this.pageMasking.TabIndex = 1;
			this.pageMasking.Text = "Masking";
			this.pageMasking.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Alpha mask:";
			// 
			// numericAlpha
			// 
			this.numericAlpha.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.numericAlpha.Location = new System.Drawing.Point(9, 19);
			this.numericAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.numericAlpha.Name = "numericAlpha";
			this.numericAlpha.Size = new System.Drawing.Size(120, 20);
			this.numericAlpha.TabIndex = 4;
			this.numericAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// comboInterpolation
			// 
			this.comboInterpolation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboInterpolation.FormattingEnabled = true;
			this.comboInterpolation.Items.AddRange(new object[] {
            "Linear",
            "Spline",
            "None"});
			this.comboInterpolation.Location = new System.Drawing.Point(78, 109);
			this.comboInterpolation.Name = "comboInterpolation";
			this.comboInterpolation.Size = new System.Drawing.Size(149, 21);
			this.comboInterpolation.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(4, 112);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(68, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Interpolation:";
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
            this.buttonStop});
			this.stripFrameAnimation.Location = new System.Drawing.Point(0, 0);
			this.stripFrameAnimation.Name = "stripFrameAnimation";
			this.stripFrameAnimation.Size = new System.Drawing.Size(629, 25);
			this.stripFrameAnimation.Stretch = true;
			this.stripFrameAnimation.TabIndex = 0;
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
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
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
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
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
			// imagesSprites
			// 
			this.imagesSprites.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imagesSprites.ImageSize = new System.Drawing.Size(64, 64);
			this.imagesSprites.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// colorAnimation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.containerFrameAnimation);
			this.Name = "colorAnimation";
			this.Size = new System.Drawing.Size(629, 452);
			this.containerFrameAnimation.ContentPanel.ResumeLayout(false);
			this.containerFrameAnimation.TopToolStripPanel.ResumeLayout(false);
			this.containerFrameAnimation.TopToolStripPanel.PerformLayout();
			this.containerFrameAnimation.ResumeLayout(false);
			this.containerFrameAnimation.PerformLayout();
			this.splitRender.Panel1.ResumeLayout(false);
			this.splitRender.Panel2.ResumeLayout(false);
			this.splitRender.ResumeLayout(false);
			this.panelColorControls.ResumeLayout(false);
			this.panelColorControls.PerformLayout();
			this.tabColorControls.ResumeLayout(false);
			this.pageColors.ResumeLayout(false);
			this.pageColors.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericB)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericG)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericR)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericA)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureColor)).EndInit();
			this.pageMasking.ResumeLayout(false);
			this.pageMasking.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAlpha)).EndInit();
			this.stripFrameAnimation.ResumeLayout(false);
			this.stripFrameAnimation.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer containerFrameAnimation;
		private System.Windows.Forms.ToolStrip stripFrameAnimation;
		private System.Windows.Forms.ToolStripButton buttonSetKeyFrame;
		private System.Windows.Forms.ToolStripButton buttonRemoveKeyFrame;
		private System.Windows.Forms.ImageList imagesSprites;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton buttonFirstKeyFrame;
		private System.Windows.Forms.ToolStripButton buttonPreviousKeyFrame;
		private System.Windows.Forms.ToolStripButton buttonNextKeyframe;
		private System.Windows.Forms.ToolStripButton buttonLastKeyFrame;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton buttonPlay;
		private System.Windows.Forms.ToolStripButton buttonStop;
		private System.Windows.Forms.ToolStripButton buttonClearKeys;
		private System.Windows.Forms.SplitContainer splitRender;
		private System.Windows.Forms.Panel panelRender;
		private System.Windows.Forms.Panel panelColorControls;
		private System.Windows.Forms.ComboBox comboInterpolation;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TabControl tabColorControls;
		private System.Windows.Forms.TabPage pageColors;
		private System.Windows.Forms.PictureBox pictureColor;
		private System.Windows.Forms.TabPage pageMasking;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericAlpha;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown numericB;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numericG;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numericR;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericA;
	}
}
