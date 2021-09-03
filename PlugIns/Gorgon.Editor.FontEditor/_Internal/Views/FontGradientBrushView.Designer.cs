using Gorgon.UI;

namespace Gorgon.Editor.FontEditor
{
	partial class FontGradientBrushView
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

			if (disposing)
			{
				if (_controlPanelImage != null)
				{
					_controlPanelImage.Dispose();
					_controlPanelImage = null;
				}

				if (_gradDisplayImage != null)
				{
					_gradDisplayImage.Dispose();
					_gradDisplayImage = null;
				}

				if (_gradientImage != null)
				{
					_gradientImage.Dispose();
					_gradientImage = null;
				}

				if (_gradPreviewImage != null)
				{
					_gradPreviewImage.Dispose();
					_gradPreviewImage = null;
				}
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
            this.PanelGradientDisplay = new System.Windows.Forms.Panel();
            this.PanelGradControls = new System.Windows.Forms.Panel();
            this.LabelAngle = new System.Windows.Forms.Label();
            this.NumericAngle = new System.Windows.Forms.NumericUpDown();
            this.LabelDegrees = new System.Windows.Forms.Label();
            this.CheckScaleAngle = new System.Windows.Forms.CheckBox();
            this.CheckUseGamma = new System.Windows.Forms.CheckBox();
            this.LabelPreview = new System.Windows.Forms.Label();
            this.NumericSelectedWeight = new System.Windows.Forms.NumericUpDown();
            this.LabelSelectedWeight = new System.Windows.Forms.Label();
            this.PanelPreview = new System.Windows.Forms.Panel();
            this.PopupNodeEdit = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemAddNode = new System.Windows.Forms.ToolStripMenuItem();
            this.ItemRemoveNode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.ItemDuplicateNode = new System.Windows.Forms.ToolStripMenuItem();
            this.TableControls = new System.Windows.Forms.TableLayoutPanel();
            this.TableButtons = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonDuplicateNode = new System.Windows.Forms.Button();
            this.ButtonRemoveNode = new System.Windows.Forms.Button();
            this.ButtonAddNode = new System.Windows.Forms.Button();
            this.ButtonClearNodes = new System.Windows.Forms.Button();
            this.TableOptions = new System.Windows.Forms.TableLayoutPanel();
            this.PickerNodeColor = new Gorgon.Editor.UI.Controls.ColorPicker();
            this.LabelNodeColor = new System.Windows.Forms.Label();
            this.PanelBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSelectedWeight)).BeginInit();
            this.PopupNodeEdit.SuspendLayout();
            this.TableControls.SuspendLayout();
            this.TableButtons.SuspendLayout();
            this.TableOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.TableControls);
            this.PanelBody.Size = new System.Drawing.Size(364, 625);
            // 
            // PanelGradientDisplay
            // 
            this.PanelGradientDisplay.BackgroundImage = global::Gorgon.Editor.FontEditor.Properties.Resources.Transparency_Pattern;
            this.PanelGradientDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelGradientDisplay.Location = new System.Drawing.Point(0, 0);
            this.PanelGradientDisplay.Margin = new System.Windows.Forms.Padding(0);
            this.PanelGradientDisplay.MinimumSize = new System.Drawing.Size(0, 37);
            this.PanelGradientDisplay.Name = "PanelGradientDisplay";
            this.PanelGradientDisplay.Size = new System.Drawing.Size(364, 37);
            this.PanelGradientDisplay.TabIndex = 1;
            this.PanelGradientDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelGradientDisplay_Paint);
            this.PanelGradientDisplay.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.PanelGradientDisplay_MouseDoubleClick);
            // 
            // PanelGradControls
            // 
            this.PanelGradControls.AutoSize = true;
            this.PanelGradControls.BackColor = System.Drawing.Color.WhiteSmoke;
            this.PanelGradControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelGradControls.Location = new System.Drawing.Point(0, 37);
            this.PanelGradControls.Margin = new System.Windows.Forms.Padding(0);
            this.PanelGradControls.MinimumSize = new System.Drawing.Size(0, 18);
            this.PanelGradControls.Name = "PanelGradControls";
            this.PanelGradControls.Size = new System.Drawing.Size(364, 18);
            this.PanelGradControls.TabIndex = 2;
            this.PanelGradControls.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelGradControls_Paint);
            this.PanelGradControls.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelGradControls_MouseDown);
            this.PanelGradControls.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelGradControls_MouseMove);
            this.PanelGradControls.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelGradControls_MouseUp);
            // 
            // LabelAngle
            // 
            this.LabelAngle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelAngle.AutoSize = true;
            this.LabelAngle.Location = new System.Drawing.Point(3, 5);
            this.LabelAngle.Margin = new System.Windows.Forms.Padding(3);
            this.LabelAngle.Name = "LabelAngle";
            this.LabelAngle.Size = new System.Drawing.Size(41, 15);
            this.LabelAngle.TabIndex = 0;
            this.LabelAngle.Text = "Angle:";
            // 
            // NumericAngle
            // 
            this.NumericAngle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericAngle.BackColor = System.Drawing.Color.White;
            this.NumericAngle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericAngle.DecimalPlaces = 1;
            this.NumericAngle.ForeColor = System.Drawing.Color.Black;
            this.NumericAngle.Location = new System.Drawing.Point(47, 0);
            this.NumericAngle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.NumericAngle.Maximum = new decimal(new int[] {
            3599,
            0,
            0,
            65536});
            this.NumericAngle.Name = "NumericAngle";
            this.NumericAngle.Size = new System.Drawing.Size(113, 23);
            this.NumericAngle.TabIndex = 1;
            this.NumericAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LabelDegrees
            // 
            this.LabelDegrees.AutoSize = true;
            this.LabelDegrees.Location = new System.Drawing.Point(163, 0);
            this.LabelDegrees.Name = "LabelDegrees";
            this.LabelDegrees.Size = new System.Drawing.Size(12, 15);
            this.LabelDegrees.TabIndex = 3;
            this.LabelDegrees.Text = "°";
            // 
            // CheckScaleAngle
            // 
            this.CheckScaleAngle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.CheckScaleAngle.AutoSize = true;
            this.TableOptions.SetColumnSpan(this.CheckScaleAngle, 2);
            this.CheckScaleAngle.Location = new System.Drawing.Point(3, 29);
            this.CheckScaleAngle.Name = "CheckScaleAngle";
            this.CheckScaleAngle.Size = new System.Drawing.Size(92, 19);
            this.CheckScaleAngle.TabIndex = 2;
            this.CheckScaleAngle.Text = "Scale Angle?";
            this.CheckScaleAngle.UseVisualStyleBackColor = true;
            // 
            // CheckUseGamma
            // 
            this.CheckUseGamma.AutoSize = true;
            this.TableOptions.SetColumnSpan(this.CheckUseGamma, 2);
            this.CheckUseGamma.Location = new System.Drawing.Point(195, 29);
            this.CheckUseGamma.Name = "CheckUseGamma";
            this.CheckUseGamma.Size = new System.Drawing.Size(154, 19);
            this.CheckUseGamma.TabIndex = 3;
            this.CheckUseGamma.Text = "Use Gamma Correction?";
            this.CheckUseGamma.UseVisualStyleBackColor = true;
            // 
            // LabelPreview
            // 
            this.LabelPreview.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.LabelPreview.AutoSize = true;
            this.LabelPreview.Location = new System.Drawing.Point(158, 463);
            this.LabelPreview.Margin = new System.Windows.Forms.Padding(3);
            this.LabelPreview.Name = "LabelPreview";
            this.LabelPreview.Size = new System.Drawing.Size(48, 15);
            this.LabelPreview.TabIndex = 0;
            this.LabelPreview.Text = "Preview";
            this.LabelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // NumericSelectedWeight
            // 
            this.NumericSelectedWeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericSelectedWeight.BackColor = System.Drawing.Color.White;
            this.NumericSelectedWeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericSelectedWeight.DecimalPlaces = 3;
            this.NumericSelectedWeight.Enabled = false;
            this.NumericSelectedWeight.ForeColor = System.Drawing.Color.Black;
            this.NumericSelectedWeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.NumericSelectedWeight.Location = new System.Drawing.Point(293, 0);
            this.NumericSelectedWeight.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.NumericSelectedWeight.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumericSelectedWeight.Name = "NumericSelectedWeight";
            this.NumericSelectedWeight.Size = new System.Drawing.Size(71, 23);
            this.NumericSelectedWeight.TabIndex = 5;
            this.NumericSelectedWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LabelSelectedWeight
            // 
            this.LabelSelectedWeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelSelectedWeight.AutoSize = true;
            this.LabelSelectedWeight.Location = new System.Drawing.Point(195, 5);
            this.LabelSelectedWeight.Margin = new System.Windows.Forms.Padding(3);
            this.LabelSelectedWeight.Name = "LabelSelectedWeight";
            this.LabelSelectedWeight.Size = new System.Drawing.Size(95, 15);
            this.LabelSelectedWeight.TabIndex = 4;
            this.LabelSelectedWeight.Text = "Selected Weight:";
            // 
            // PanelPreview
            // 
            this.PanelPreview.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PanelPreview.BackgroundImage = global::Gorgon.Editor.FontEditor.Properties.Resources.Transparency_Pattern;
            this.PanelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PanelPreview.Location = new System.Drawing.Point(54, 481);
            this.PanelPreview.Margin = new System.Windows.Forms.Padding(6, 0, 6, 6);
            this.PanelPreview.MaximumSize = new System.Drawing.Size(256, 128);
            this.PanelPreview.MinimumSize = new System.Drawing.Size(256, 128);
            this.PanelPreview.Name = "PanelPreview";
            this.PanelPreview.Size = new System.Drawing.Size(256, 128);
            this.PanelPreview.TabIndex = 1;
            this.PanelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPreview_Paint);
            // 
            // PopupNodeEdit
            // 
            this.PopupNodeEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemAddNode,
            this.ItemRemoveNode,
            this.toolStripMenuItem1,
            this.ItemDuplicateNode});
            this.PopupNodeEdit.Name = "contextMenuStrip1";
            this.PopupNodeEdit.Size = new System.Drawing.Size(159, 76);
            // 
            // ItemAddNode
            // 
            this.ItemAddNode.BackColor = System.Drawing.Color.White;
            this.ItemAddNode.Image = global::Gorgon.Editor.FontEditor.Properties.Resources.add_16x16;
            this.ItemAddNode.Name = "ItemAddNode";
            this.ItemAddNode.Size = new System.Drawing.Size(158, 22);
            this.ItemAddNode.Text = "Add Node";
            this.ItemAddNode.Click += new System.EventHandler(this.ItemAddNode_Click);
            // 
            // ItemRemoveNode
            // 
            this.ItemRemoveNode.Image = global::Gorgon.Editor.FontEditor.Properties.Resources.remove_16x16;
            this.ItemRemoveNode.Name = "ItemRemoveNode";
            this.ItemRemoveNode.Size = new System.Drawing.Size(158, 22);
            this.ItemRemoveNode.Text = "Remove Node...";
            this.ItemRemoveNode.Click += new System.EventHandler(this.ItemRemoveNode_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(155, 6);
            // 
            // ItemDuplicateNode
            // 
            this.ItemDuplicateNode.Image = global::Gorgon.Editor.FontEditor.Properties.Resources.copy_16x16;
            this.ItemDuplicateNode.Name = "ItemDuplicateNode";
            this.ItemDuplicateNode.Size = new System.Drawing.Size(158, 22);
            this.ItemDuplicateNode.Text = "Duplicate node";
            this.ItemDuplicateNode.Click += new System.EventHandler(this.ItemDuplicateNode_Click);
            // 
            // TableControls
            // 
            this.TableControls.AutoSize = true;
            this.TableControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableControls.ColumnCount = 1;
            this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableControls.Controls.Add(this.TableButtons, 0, 2);
            this.TableControls.Controls.Add(this.TableOptions, 0, 3);
            this.TableControls.Controls.Add(this.PanelGradientDisplay, 0, 0);
            this.TableControls.Controls.Add(this.PanelGradControls, 0, 1);
            this.TableControls.Controls.Add(this.PanelPreview, 0, 7);
            this.TableControls.Controls.Add(this.LabelPreview, 0, 6);
            this.TableControls.Controls.Add(this.PickerNodeColor, 0, 5);
            this.TableControls.Controls.Add(this.LabelNodeColor, 0, 4);
            this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableControls.Location = new System.Drawing.Point(0, 0);
            this.TableControls.Name = "TableControls";
            this.TableControls.RowCount = 9;
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.TableControls.Size = new System.Drawing.Size(364, 625);
            this.TableControls.TabIndex = 0;
            // 
            // TableButtons
            // 
            this.TableButtons.AutoSize = true;
            this.TableButtons.ColumnCount = 5;
            this.TableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.TableButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableButtons.Controls.Add(this.ButtonDuplicateNode, 2, 0);
            this.TableButtons.Controls.Add(this.ButtonRemoveNode, 1, 0);
            this.TableButtons.Controls.Add(this.ButtonAddNode, 0, 0);
            this.TableButtons.Controls.Add(this.ButtonClearNodes, 4, 0);
            this.TableButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableButtons.Location = new System.Drawing.Point(3, 58);
            this.TableButtons.Name = "TableButtons";
            this.TableButtons.RowCount = 1;
            this.TableButtons.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableButtons.Size = new System.Drawing.Size(358, 33);
            this.TableButtons.TabIndex = 5;
            // 
            // ButtonDuplicateNode
            // 
            this.ButtonDuplicateNode.AutoSize = true;
            this.ButtonDuplicateNode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonDuplicateNode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonDuplicateNode.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(47)))));
            this.ButtonDuplicateNode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonDuplicateNode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonDuplicateNode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonDuplicateNode.Location = new System.Drawing.Point(175, 3);
            this.ButtonDuplicateNode.MinimumSize = new System.Drawing.Size(80, 27);
            this.ButtonDuplicateNode.Name = "ButtonDuplicateNode";
            this.ButtonDuplicateNode.Size = new System.Drawing.Size(80, 27);
            this.ButtonDuplicateNode.TabIndex = 2;
            this.ButtonDuplicateNode.Text = "&Duplicate";
            this.ButtonDuplicateNode.UseVisualStyleBackColor = false;
            this.ButtonDuplicateNode.Click += new System.EventHandler(this.ItemDuplicateNode_Click);
            // 
            // ButtonRemoveNode
            // 
            this.ButtonRemoveNode.AutoSize = true;
            this.ButtonRemoveNode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonRemoveNode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonRemoveNode.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(47)))));
            this.ButtonRemoveNode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonRemoveNode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonRemoveNode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonRemoveNode.Location = new System.Drawing.Point(89, 3);
            this.ButtonRemoveNode.MinimumSize = new System.Drawing.Size(80, 27);
            this.ButtonRemoveNode.Name = "ButtonRemoveNode";
            this.ButtonRemoveNode.Size = new System.Drawing.Size(80, 27);
            this.ButtonRemoveNode.TabIndex = 1;
            this.ButtonRemoveNode.Text = "&Remove";
            this.ButtonRemoveNode.UseVisualStyleBackColor = false;
            this.ButtonRemoveNode.Click += new System.EventHandler(this.ItemRemoveNode_Click);
            // 
            // ButtonAddNode
            // 
            this.ButtonAddNode.AutoSize = true;
            this.ButtonAddNode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonAddNode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonAddNode.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(47)))));
            this.ButtonAddNode.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonAddNode.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonAddNode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonAddNode.Location = new System.Drawing.Point(3, 3);
            this.ButtonAddNode.MinimumSize = new System.Drawing.Size(80, 27);
            this.ButtonAddNode.Name = "ButtonAddNode";
            this.ButtonAddNode.Size = new System.Drawing.Size(80, 27);
            this.ButtonAddNode.TabIndex = 0;
            this.ButtonAddNode.Text = "&Add";
            this.ButtonAddNode.UseVisualStyleBackColor = false;
            this.ButtonAddNode.Click += new System.EventHandler(this.ItemAddNode_Click);
            // 
            // ButtonClearNodes
            // 
            this.ButtonClearNodes.AutoSize = true;
            this.ButtonClearNodes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonClearNodes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.ButtonClearNodes.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(47)))));
            this.ButtonClearNodes.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkOrange;
            this.ButtonClearNodes.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonClearNodes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonClearNodes.Location = new System.Drawing.Point(277, 3);
            this.ButtonClearNodes.MinimumSize = new System.Drawing.Size(80, 27);
            this.ButtonClearNodes.Name = "ButtonClearNodes";
            this.ButtonClearNodes.Size = new System.Drawing.Size(80, 27);
            this.ButtonClearNodes.TabIndex = 3;
            this.ButtonClearNodes.Text = "&Clear";
            this.ButtonClearNodes.UseVisualStyleBackColor = false;
            this.ButtonClearNodes.Click += new System.EventHandler(this.ButtonClearNodes_Click);
            // 
            // TableOptions
            // 
            this.TableOptions.AutoSize = true;
            this.TableOptions.ColumnCount = 5;
            this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableOptions.Controls.Add(this.LabelAngle, 0, 0);
            this.TableOptions.Controls.Add(this.NumericAngle, 1, 0);
            this.TableOptions.Controls.Add(this.NumericSelectedWeight, 4, 0);
            this.TableOptions.Controls.Add(this.LabelDegrees, 2, 0);
            this.TableOptions.Controls.Add(this.CheckScaleAngle, 0, 1);
            this.TableOptions.Controls.Add(this.LabelSelectedWeight, 3, 0);
            this.TableOptions.Controls.Add(this.CheckUseGamma, 3, 1);
            this.TableOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableOptions.Location = new System.Drawing.Point(0, 94);
            this.TableOptions.Margin = new System.Windows.Forms.Padding(0);
            this.TableOptions.Name = "TableOptions";
            this.TableOptions.RowCount = 2;
            this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableOptions.Size = new System.Drawing.Size(364, 51);
            this.TableOptions.TabIndex = 0;
            // 
            // PickerNodeColor
            // 
            this.PickerNodeColor.AutoSize = true;
            this.PickerNodeColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PickerNodeColor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.PickerNodeColor.Location = new System.Drawing.Point(3, 169);
            this.PickerNodeColor.Name = "PickerNodeColor";
            this.PickerNodeColor.Size = new System.Drawing.Size(358, 288);
            this.PickerNodeColor.TabIndex = 3;
            // 
            // LabelNodeColor
            // 
            this.LabelNodeColor.AutoSize = true;
            this.LabelNodeColor.Location = new System.Drawing.Point(3, 148);
            this.LabelNodeColor.Margin = new System.Windows.Forms.Padding(3);
            this.LabelNodeColor.Name = "LabelNodeColor";
            this.LabelNodeColor.Size = new System.Drawing.Size(39, 15);
            this.LabelNodeColor.TabIndex = 6;
            this.LabelNodeColor.Text = "Color:";
            // 
            // FontGradientBrushView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.Name = "FontGradientBrushView";
            this.Size = new System.Drawing.Size(364, 682);
            this.Controls.SetChildIndex(this.PanelBody, 0);
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSelectedWeight)).EndInit();
            this.PopupNodeEdit.ResumeLayout(false);
            this.TableControls.ResumeLayout(false);
            this.TableControls.PerformLayout();
            this.TableButtons.ResumeLayout(false);
            this.TableButtons.PerformLayout();
            this.TableOptions.ResumeLayout(false);
            this.TableOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label LabelAngle;
		private System.Windows.Forms.NumericUpDown NumericAngle;
		private System.Windows.Forms.Label LabelDegrees;
		private System.Windows.Forms.CheckBox CheckScaleAngle;
		private System.Windows.Forms.CheckBox CheckUseGamma;
		private System.Windows.Forms.Panel PanelGradientDisplay;
		private System.Windows.Forms.Panel PanelGradControls;
		private System.Windows.Forms.NumericUpDown NumericSelectedWeight;
		private System.Windows.Forms.Label LabelSelectedWeight;
		private System.Windows.Forms.Panel PanelPreview;
		private System.Windows.Forms.Label LabelPreview;
		private System.Windows.Forms.ContextMenuStrip PopupNodeEdit;
		private System.Windows.Forms.ToolStripMenuItem ItemDuplicateNode;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem ItemRemoveNode;
        private System.Windows.Forms.ToolStripMenuItem ItemAddNode;
        private System.Windows.Forms.TableLayoutPanel TableOptions;
        private System.Windows.Forms.TableLayoutPanel TableControls;
        private System.Windows.Forms.TableLayoutPanel TableButtons;
        private System.Windows.Forms.Button ButtonDuplicateNode;
        private System.Windows.Forms.Button ButtonRemoveNode;
        private System.Windows.Forms.Button ButtonAddNode;
        private System.Windows.Forms.Button ButtonClearNodes;
        private UI.Controls.ColorPicker PickerNodeColor;
        private System.Windows.Forms.Label LabelNodeColor;
    }
}
