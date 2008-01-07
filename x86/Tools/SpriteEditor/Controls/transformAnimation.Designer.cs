namespace GorgonLibrary.Graphics.Tools
{
	partial class transformAnimation
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
			this.panelTransformControls = new System.Windows.Forms.Panel();
			this.tabTransforms = new System.Windows.Forms.TabControl();
			this.pageStandard = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.numericAxisY = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.numericAxisX = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.numericAngle = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.numericScaleY = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.numericScaleX = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.numericTranslateY = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.numericTranslateX = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.tabMisc = new System.Windows.Forms.TabPage();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.numericSpriteHeight = new System.Windows.Forms.NumericUpDown();
			this.label11 = new System.Windows.Forms.Label();
			this.numericSpriteWidth = new System.Windows.Forms.NumericUpDown();
			this.label12 = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.numericImageY = new System.Windows.Forms.NumericUpDown();
			this.label9 = new System.Windows.Forms.Label();
			this.numericImageX = new System.Windows.Forms.NumericUpDown();
			this.label10 = new System.Windows.Forms.Label();
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
			this.panelTransformControls.SuspendLayout();
			this.tabTransforms.SuspendLayout();
			this.pageStandard.SuspendLayout();
			this.groupBox4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAxisY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAxisX)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAngle)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericScaleY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericScaleX)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTranslateY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericTranslateX)).BeginInit();
			this.tabMisc.SuspendLayout();
			this.groupBox6.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericSpriteHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericSpriteWidth)).BeginInit();
			this.groupBox5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericImageY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageX)).BeginInit();
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
			this.splitRender.IsSplitterFixed = true;
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
			this.splitRender.Panel2.Controls.Add(this.panelTransformControls);
			this.splitRender.Size = new System.Drawing.Size(629, 427);
			this.splitRender.SplitterDistance = 240;
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
			this.panelRender.Size = new System.Drawing.Size(629, 240);
			this.panelRender.TabIndex = 4;
			// 
			// panelTransformControls
			// 
			this.panelTransformControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTransformControls.Controls.Add(this.tabTransforms);
			this.panelTransformControls.Controls.Add(this.comboInterpolation);
			this.panelTransformControls.Controls.Add(this.label3);
			this.panelTransformControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelTransformControls.Location = new System.Drawing.Point(0, 0);
			this.panelTransformControls.Name = "panelTransformControls";
			this.panelTransformControls.Size = new System.Drawing.Size(629, 183);
			this.panelTransformControls.TabIndex = 0;
			// 
			// tabTransforms
			// 
			this.tabTransforms.Controls.Add(this.pageStandard);
			this.tabTransforms.Controls.Add(this.tabMisc);
			this.tabTransforms.Location = new System.Drawing.Point(6, 3);
			this.tabTransforms.Name = "tabTransforms";
			this.tabTransforms.SelectedIndex = 0;
			this.tabTransforms.Size = new System.Drawing.Size(618, 133);
			this.tabTransforms.TabIndex = 6;
			// 
			// pageStandard
			// 
			this.pageStandard.Controls.Add(this.groupBox4);
			this.pageStandard.Controls.Add(this.groupBox3);
			this.pageStandard.Controls.Add(this.groupBox2);
			this.pageStandard.Controls.Add(this.groupBox1);
			this.pageStandard.Location = new System.Drawing.Point(4, 22);
			this.pageStandard.Name = "pageStandard";
			this.pageStandard.Padding = new System.Windows.Forms.Padding(3);
			this.pageStandard.Size = new System.Drawing.Size(610, 107);
			this.pageStandard.TabIndex = 0;
			this.pageStandard.Text = "Standard";
			this.pageStandard.UseVisualStyleBackColor = true;
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.numericAxisY);
			this.groupBox4.Controls.Add(this.label6);
			this.groupBox4.Controls.Add(this.numericAxisX);
			this.groupBox4.Controls.Add(this.label8);
			this.groupBox4.Location = new System.Drawing.Point(420, 6);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(132, 76);
			this.groupBox4.TabIndex = 3;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Axis";
			// 
			// numericAxisY
			// 
			this.numericAxisY.Location = new System.Drawing.Point(31, 46);
			this.numericAxisY.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericAxisY.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericAxisY.Name = "numericAxisY";
			this.numericAxisY.Size = new System.Drawing.Size(95, 20);
			this.numericAxisY.TabIndex = 3;
			this.numericAxisY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(8, 48);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(17, 13);
			this.label6.TabIndex = 2;
			this.label6.Text = "Y:";
			// 
			// numericAxisX
			// 
			this.numericAxisX.Location = new System.Drawing.Point(31, 20);
			this.numericAxisX.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericAxisX.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericAxisX.Name = "numericAxisX";
			this.numericAxisX.Size = new System.Drawing.Size(95, 20);
			this.numericAxisX.TabIndex = 1;
			this.numericAxisX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(8, 22);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(17, 13);
			this.label8.TabIndex = 0;
			this.label8.Text = "X:";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.numericAngle);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Location = new System.Drawing.Point(282, 6);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(132, 76);
			this.groupBox3.TabIndex = 2;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Rotation";
			// 
			// numericAngle
			// 
			this.numericAngle.DecimalPlaces = 1;
			this.numericAngle.Location = new System.Drawing.Point(51, 20);
			this.numericAngle.Maximum = new decimal(new int[] {
            200000,
            0,
            0,
            0});
			this.numericAngle.Minimum = new decimal(new int[] {
            200000,
            0,
            0,
            -2147483648});
			this.numericAngle.Name = "numericAngle";
			this.numericAngle.Size = new System.Drawing.Size(75, 20);
			this.numericAngle.TabIndex = 1;
			this.numericAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(8, 22);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(37, 13);
			this.label7.TabIndex = 0;
			this.label7.Text = "Angle:";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.numericScaleY);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.numericScaleX);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Location = new System.Drawing.Point(144, 6);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(132, 76);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Scale";
			// 
			// numericScaleY
			// 
			this.numericScaleY.DecimalPlaces = 4;
			this.numericScaleY.Location = new System.Drawing.Point(31, 46);
			this.numericScaleY.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericScaleY.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            262144});
			this.numericScaleY.Name = "numericScaleY";
			this.numericScaleY.Size = new System.Drawing.Size(95, 20);
			this.numericScaleY.TabIndex = 3;
			this.numericScaleY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericScaleY.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 48);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(17, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "Y:";
			// 
			// numericScaleX
			// 
			this.numericScaleX.DecimalPlaces = 4;
			this.numericScaleX.Location = new System.Drawing.Point(31, 20);
			this.numericScaleX.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericScaleX.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            262144});
			this.numericScaleX.Name = "numericScaleX";
			this.numericScaleX.Size = new System.Drawing.Size(95, 20);
			this.numericScaleX.TabIndex = 1;
			this.numericScaleX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericScaleX.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(8, 22);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(17, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "X:";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.numericTranslateY);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.numericTranslateX);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(6, 6);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(132, 76);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Translation";
			// 
			// numericTranslateY
			// 
			this.numericTranslateY.Location = new System.Drawing.Point(31, 46);
			this.numericTranslateY.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericTranslateY.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericTranslateY.Name = "numericTranslateY";
			this.numericTranslateY.Size = new System.Drawing.Size(95, 20);
			this.numericTranslateY.TabIndex = 3;
			this.numericTranslateY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Y:";
			// 
			// numericTranslateX
			// 
			this.numericTranslateX.Location = new System.Drawing.Point(31, 20);
			this.numericTranslateX.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericTranslateX.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericTranslateX.Name = "numericTranslateX";
			this.numericTranslateX.Size = new System.Drawing.Size(95, 20);
			this.numericTranslateX.TabIndex = 1;
			this.numericTranslateX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "X:";
			// 
			// tabMisc
			// 
			this.tabMisc.Controls.Add(this.groupBox6);
			this.tabMisc.Controls.Add(this.groupBox5);
			this.tabMisc.Location = new System.Drawing.Point(4, 22);
			this.tabMisc.Name = "tabMisc";
			this.tabMisc.Padding = new System.Windows.Forms.Padding(3);
			this.tabMisc.Size = new System.Drawing.Size(610, 107);
			this.tabMisc.TabIndex = 1;
			this.tabMisc.Text = "Misc.";
			this.tabMisc.UseVisualStyleBackColor = true;
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.numericSpriteHeight);
			this.groupBox6.Controls.Add(this.label11);
			this.groupBox6.Controls.Add(this.numericSpriteWidth);
			this.groupBox6.Controls.Add(this.label12);
			this.groupBox6.Location = new System.Drawing.Point(144, 6);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(132, 76);
			this.groupBox6.TabIndex = 2;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Sprite size";
			// 
			// numericSpriteHeight
			// 
			this.numericSpriteHeight.Location = new System.Drawing.Point(31, 46);
			this.numericSpriteHeight.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericSpriteHeight.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericSpriteHeight.Name = "numericSpriteHeight";
			this.numericSpriteHeight.Size = new System.Drawing.Size(95, 20);
			this.numericSpriteHeight.TabIndex = 3;
			this.numericSpriteHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(8, 48);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(17, 13);
			this.label11.TabIndex = 2;
			this.label11.Text = "Y:";
			// 
			// numericSpriteWidth
			// 
			this.numericSpriteWidth.Location = new System.Drawing.Point(31, 20);
			this.numericSpriteWidth.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericSpriteWidth.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericSpriteWidth.Name = "numericSpriteWidth";
			this.numericSpriteWidth.Size = new System.Drawing.Size(95, 20);
			this.numericSpriteWidth.TabIndex = 1;
			this.numericSpriteWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(8, 22);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(17, 13);
			this.label12.TabIndex = 0;
			this.label12.Text = "X:";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.numericImageY);
			this.groupBox5.Controls.Add(this.label9);
			this.groupBox5.Controls.Add(this.numericImageX);
			this.groupBox5.Controls.Add(this.label10);
			this.groupBox5.Location = new System.Drawing.Point(6, 6);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(132, 76);
			this.groupBox5.TabIndex = 1;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Image location";
			// 
			// numericImageY
			// 
			this.numericImageY.Location = new System.Drawing.Point(31, 46);
			this.numericImageY.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericImageY.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericImageY.Name = "numericImageY";
			this.numericImageY.Size = new System.Drawing.Size(95, 20);
			this.numericImageY.TabIndex = 3;
			this.numericImageY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(8, 48);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(17, 13);
			this.label9.TabIndex = 2;
			this.label9.Text = "Y:";
			// 
			// numericImageX
			// 
			this.numericImageX.Location = new System.Drawing.Point(31, 20);
			this.numericImageX.Maximum = new decimal(new int[] {
            2000000000,
            0,
            0,
            0});
			this.numericImageX.Minimum = new decimal(new int[] {
            2000000000,
            0,
            0,
            -2147483648});
			this.numericImageX.Name = "numericImageX";
			this.numericImageX.Size = new System.Drawing.Size(95, 20);
			this.numericImageX.TabIndex = 1;
			this.numericImageX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(8, 22);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(17, 13);
			this.label10.TabIndex = 0;
			this.label10.Text = "X:";
			// 
			// comboInterpolation
			// 
			this.comboInterpolation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboInterpolation.FormattingEnabled = true;
			this.comboInterpolation.Items.AddRange(new object[] {
            "Linear",
            "Spline",
            "None"});
			this.comboInterpolation.Location = new System.Drawing.Point(77, 142);
			this.comboInterpolation.Name = "comboInterpolation";
			this.comboInterpolation.Size = new System.Drawing.Size(149, 21);
			this.comboInterpolation.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 145);
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
			// transformAnimation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.containerFrameAnimation);
			this.Name = "transformAnimation";
			this.Size = new System.Drawing.Size(629, 452);
			this.containerFrameAnimation.ContentPanel.ResumeLayout(false);
			this.containerFrameAnimation.TopToolStripPanel.ResumeLayout(false);
			this.containerFrameAnimation.TopToolStripPanel.PerformLayout();
			this.containerFrameAnimation.ResumeLayout(false);
			this.containerFrameAnimation.PerformLayout();
			this.splitRender.Panel1.ResumeLayout(false);
			this.splitRender.Panel2.ResumeLayout(false);
			this.splitRender.ResumeLayout(false);
			this.panelTransformControls.ResumeLayout(false);
			this.panelTransformControls.PerformLayout();
			this.tabTransforms.ResumeLayout(false);
			this.pageStandard.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAxisY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericAxisX)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericAngle)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericScaleY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericScaleX)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericTranslateY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericTranslateX)).EndInit();
			this.tabMisc.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericSpriteHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericSpriteWidth)).EndInit();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericImageY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericImageX)).EndInit();
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
		private System.Windows.Forms.Panel panelTransformControls;
		private System.Windows.Forms.ComboBox comboInterpolation;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TabControl tabTransforms;
		private System.Windows.Forms.TabPage pageStandard;
		private System.Windows.Forms.TabPage tabMisc;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.NumericUpDown numericScaleY;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericScaleX;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.NumericUpDown numericTranslateY;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericTranslateX;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.NumericUpDown numericAngle;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.NumericUpDown numericAxisY;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numericAxisX;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.NumericUpDown numericSpriteHeight;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown numericSpriteWidth;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.NumericUpDown numericImageY;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown numericImageX;
		private System.Windows.Forms.Label label10;
	}
}
