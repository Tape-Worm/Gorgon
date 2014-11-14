using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn.Controls
{
	partial class PanelGradient
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

				if (_selectedColorImage != null)
				{
					_selectedColorImage.Dispose();
					_selectedColorImage = null;
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
            this.panelBrushGradient = new System.Windows.Forms.Panel();
            this.panelGradientDisplay = new System.Windows.Forms.Panel();
            this.stripCommands = new System.Windows.Forms.ToolStrip();
            this.buttonAddNode = new System.Windows.Forms.ToolStripButton();
            this.buttonRemoveNode = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonDuplicateNode = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonClearNodes = new System.Windows.Forms.ToolStripButton();
            this.panelGradControls = new System.Windows.Forms.Panel();
            this.labelAngle = new System.Windows.Forms.Label();
            this.numericAngle = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.checkScaleAngle = new System.Windows.Forms.CheckBox();
            this.checkUseGamma = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelSelectedColor = new GorgonLibrary.UI.GorgonSelectablePanel();
            this.labelPreview = new System.Windows.Forms.Label();
            this.labelSelectedColor = new System.Windows.Forms.Label();
            this.numericSelectedWeight = new System.Windows.Forms.NumericUpDown();
            this.labelSelectedWeight = new System.Windows.Forms.Label();
            this.panelPreview = new System.Windows.Forms.Panel();
            this.popupNodeEdit = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemAddNode = new System.Windows.Forms.ToolStripMenuItem();
            this.itemRemoveNode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.itemDuplicateNode = new System.Windows.Forms.ToolStripMenuItem();
            this.panelBrushGradient.SuspendLayout();
            this.stripCommands.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericAngle)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericSelectedWeight)).BeginInit();
            this.popupNodeEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBrushGradient
            // 
            this.panelBrushGradient.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.panelBrushGradient.Controls.Add(this.panelGradientDisplay);
            this.panelBrushGradient.Controls.Add(this.stripCommands);
            this.panelBrushGradient.Controls.Add(this.panelGradControls);
            this.panelBrushGradient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBrushGradient.Location = new System.Drawing.Point(0, 0);
            this.panelBrushGradient.Name = "panelBrushGradient";
            this.panelBrushGradient.Size = new System.Drawing.Size(530, 86);
            this.panelBrushGradient.TabIndex = 0;
            // 
            // panelGradientDisplay
            // 
            this.panelGradientDisplay.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelGradientDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelGradientDisplay.Location = new System.Drawing.Point(0, 25);
            this.panelGradientDisplay.Margin = new System.Windows.Forms.Padding(0);
            this.panelGradientDisplay.Name = "panelGradientDisplay";
            this.panelGradientDisplay.Size = new System.Drawing.Size(530, 45);
            this.panelGradientDisplay.TabIndex = 1;
            this.panelGradientDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.panelGradientDisplay_Paint);
            this.panelGradientDisplay.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelGradientDisplay_MouseDoubleClick);
            // 
            // stripCommands
            // 
            this.stripCommands.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.stripCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddNode,
            this.buttonRemoveNode,
            this.toolStripSeparator1,
            this.buttonDuplicateNode,
            this.toolStripSeparator2,
            this.buttonClearNodes});
            this.stripCommands.Location = new System.Drawing.Point(0, 0);
            this.stripCommands.Name = "stripCommands";
            this.stripCommands.Size = new System.Drawing.Size(530, 25);
            this.stripCommands.Stretch = true;
            this.stripCommands.TabIndex = 0;
            // 
            // buttonAddNode
            // 
            this.buttonAddNode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAddNode.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.add_16x16;
            this.buttonAddNode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddNode.Name = "buttonAddNode";
            this.buttonAddNode.Size = new System.Drawing.Size(23, 22);
            this.buttonAddNode.Text = "add node";
            this.buttonAddNode.Click += new System.EventHandler(this.itemAddNode_Click);
            // 
            // buttonRemoveNode
            // 
            this.buttonRemoveNode.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRemoveNode.Enabled = false;
            this.buttonRemoveNode.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.remove_16x16;
            this.buttonRemoveNode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemoveNode.Name = "buttonRemoveNode";
            this.buttonRemoveNode.Size = new System.Drawing.Size(23, 22);
            this.buttonRemoveNode.Text = "remove node";
            this.buttonRemoveNode.Click += new System.EventHandler(this.itemRemoveNode_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonDuplicateNode
            // 
            this.buttonDuplicateNode.Enabled = false;
            this.buttonDuplicateNode.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.duplicate_16x16;
            this.buttonDuplicateNode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonDuplicateNode.Name = "buttonDuplicateNode";
            this.buttonDuplicateNode.Size = new System.Drawing.Size(106, 22);
            this.buttonDuplicateNode.Text = "duplicate node";
            this.buttonDuplicateNode.Click += new System.EventHandler(this.itemDuplicateNode_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonClearNodes
            // 
            this.buttonClearNodes.Enabled = false;
            this.buttonClearNodes.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.clear_16x16;
            this.buttonClearNodes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonClearNodes.Name = "buttonClearNodes";
            this.buttonClearNodes.Size = new System.Drawing.Size(87, 22);
            this.buttonClearNodes.Text = "clear nodes";
            this.buttonClearNodes.Click += new System.EventHandler(this.buttonClearNodes_Click);
            // 
            // panelGradControls
            // 
            this.panelGradControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelGradControls.Location = new System.Drawing.Point(0, 70);
            this.panelGradControls.Margin = new System.Windows.Forms.Padding(0);
            this.panelGradControls.Name = "panelGradControls";
            this.panelGradControls.Size = new System.Drawing.Size(530, 16);
            this.panelGradControls.TabIndex = 2;
            this.panelGradControls.Paint += new System.Windows.Forms.PaintEventHandler(this.panelGradControls_Paint);
            this.panelGradControls.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelGradControls_MouseDoubleClick);
            this.panelGradControls.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelGradControls_MouseDown);
            this.panelGradControls.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelGradControls_MouseMove);
            this.panelGradControls.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelGradControls_MouseUp);
            // 
            // labelAngle
            // 
            this.labelAngle.AutoSize = true;
            this.labelAngle.Location = new System.Drawing.Point(6, 9);
            this.labelAngle.Name = "labelAngle";
            this.labelAngle.Size = new System.Drawing.Size(33, 13);
            this.labelAngle.TabIndex = 1;
            this.labelAngle.Text = "angle";
            // 
            // numericAngle
            // 
            this.numericAngle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericAngle.DecimalPlaces = 1;
            this.numericAngle.Location = new System.Drawing.Point(49, 5);
            this.numericAngle.Maximum = new decimal(new int[] {
            3599,
            0,
            0,
            65536});
            this.numericAngle.Name = "numericAngle";
            this.numericAngle.Size = new System.Drawing.Size(52, 20);
            this.numericAngle.TabIndex = 0;
            this.numericAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericAngle.ValueChanged += new System.EventHandler(this.numericAngle_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(103, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "°";
            // 
            // checkScaleAngle
            // 
            this.checkScaleAngle.AutoSize = true;
            this.checkScaleAngle.Location = new System.Drawing.Point(9, 33);
            this.checkScaleAngle.Name = "checkScaleAngle";
            this.checkScaleAngle.Size = new System.Drawing.Size(86, 17);
            this.checkScaleAngle.TabIndex = 1;
            this.checkScaleAngle.Text = "scale angle?";
            this.checkScaleAngle.UseVisualStyleBackColor = true;
            this.checkScaleAngle.Click += new System.EventHandler(this.checkScaleAngle_Click);
            // 
            // checkUseGamma
            // 
            this.checkUseGamma.AutoSize = true;
            this.checkUseGamma.Location = new System.Drawing.Point(9, 56);
            this.checkUseGamma.Name = "checkUseGamma";
            this.checkUseGamma.Size = new System.Drawing.Size(136, 17);
            this.checkUseGamma.TabIndex = 2;
            this.checkUseGamma.Text = "use gamma correction?";
            this.checkUseGamma.UseVisualStyleBackColor = true;
            this.checkUseGamma.CheckedChanged += new System.EventHandler(this.checkUseGamma_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelSelectedColor);
            this.panel1.Controls.Add(this.labelPreview);
            this.panel1.Controls.Add(this.labelSelectedColor);
            this.panel1.Controls.Add(this.numericSelectedWeight);
            this.panel1.Controls.Add(this.labelSelectedWeight);
            this.panel1.Controls.Add(this.labelAngle);
            this.panel1.Controls.Add(this.checkUseGamma);
            this.panel1.Controls.Add(this.numericAngle);
            this.panel1.Controls.Add(this.checkScaleAngle);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.panelPreview);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 86);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(530, 122);
            this.panel1.TabIndex = 1;
            // 
            // panelSelectedColor
            // 
            this.panelSelectedColor.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelSelectedColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelSelectedColor.Enabled = false;
            this.panelSelectedColor.Location = new System.Drawing.Point(282, 31);
            this.panelSelectedColor.Name = "panelSelectedColor";
            this.panelSelectedColor.Size = new System.Drawing.Size(57, 20);
            this.panelSelectedColor.TabIndex = 5;
            this.panelSelectedColor.Paint += new System.Windows.Forms.PaintEventHandler(this.panelSelectedColor_Paint);
            this.panelSelectedColor.DoubleClick += new System.EventHandler(this.panelSelectedColor_DoubleClick);
            this.panelSelectedColor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelSelectedColor_PreviewKeyDown);
            // 
            // labelPreview
            // 
            this.labelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelPreview.Location = new System.Drawing.Point(432, 3);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(95, 22);
            this.labelPreview.TabIndex = 12;
            this.labelPreview.Text = "preview";
            this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelSelectedColor
            // 
            this.labelSelectedColor.AutoSize = true;
            this.labelSelectedColor.Location = new System.Drawing.Point(189, 35);
            this.labelSelectedColor.Name = "labelSelectedColor";
            this.labelSelectedColor.Size = new System.Drawing.Size(73, 13);
            this.labelSelectedColor.TabIndex = 10;
            this.labelSelectedColor.Text = "selected color";
            // 
            // numericSelectedWeight
            // 
            this.numericSelectedWeight.DecimalPlaces = 3;
            this.numericSelectedWeight.Enabled = false;
            this.numericSelectedWeight.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericSelectedWeight.Location = new System.Drawing.Point(282, 5);
            this.numericSelectedWeight.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericSelectedWeight.Name = "numericSelectedWeight";
            this.numericSelectedWeight.Size = new System.Drawing.Size(57, 20);
            this.numericSelectedWeight.TabIndex = 4;
            this.numericSelectedWeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericSelectedWeight.ValueChanged += new System.EventHandler(this.numericSelectedWeight_ValueChanged);
            // 
            // labelSelectedWeight
            // 
            this.labelSelectedWeight.AutoSize = true;
            this.labelSelectedWeight.Location = new System.Drawing.Point(189, 9);
            this.labelSelectedWeight.Name = "labelSelectedWeight";
            this.labelSelectedWeight.Size = new System.Drawing.Size(81, 13);
            this.labelSelectedWeight.TabIndex = 8;
            this.labelSelectedWeight.Text = "selected weight";
            // 
            // panelPreview
            // 
            this.panelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPreview.BackgroundImage = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
            this.panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPreview.Location = new System.Drawing.Point(432, 24);
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Size = new System.Drawing.Size(95, 95);
            this.panelPreview.TabIndex = 6;
            this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
            // 
            // popupNodeEdit
            // 
            this.popupNodeEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemAddNode,
            this.itemRemoveNode,
            this.toolStripMenuItem1,
            this.itemDuplicateNode});
            this.popupNodeEdit.Name = "contextMenuStrip1";
            this.popupNodeEdit.Size = new System.Drawing.Size(154, 76);
            // 
            // itemAddNode
            // 
            this.itemAddNode.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.add_16x16;
            this.itemAddNode.Name = "itemAddNode";
            this.itemAddNode.Size = new System.Drawing.Size(153, 22);
            this.itemAddNode.Text = "add node";
            this.itemAddNode.Click += new System.EventHandler(this.itemAddNode_Click);
            // 
            // itemRemoveNode
            // 
            this.itemRemoveNode.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.remove_16x16;
            this.itemRemoveNode.Name = "itemRemoveNode";
            this.itemRemoveNode.Size = new System.Drawing.Size(153, 22);
            this.itemRemoveNode.Text = "remove node...";
            this.itemRemoveNode.Click += new System.EventHandler(this.itemRemoveNode_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(150, 6);
            // 
            // itemDuplicateNode
            // 
            this.itemDuplicateNode.Image = global::GorgonLibrary.Editor.FontEditorPlugIn.Properties.Resources.duplicate_16x16;
            this.itemDuplicateNode.Name = "itemDuplicateNode";
            this.itemDuplicateNode.Size = new System.Drawing.Size(153, 22);
            this.itemDuplicateNode.Text = "duplicate node";
            this.itemDuplicateNode.Click += new System.EventHandler(this.itemDuplicateNode_Click);
            // 
            // PanelGradient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelBrushGradient);
            this.Controls.Add(this.panel1);
            this.Name = "PanelGradient";
            this.Size = new System.Drawing.Size(530, 208);
            this.panelBrushGradient.ResumeLayout(false);
            this.panelBrushGradient.PerformLayout();
            this.stripCommands.ResumeLayout(false);
            this.stripCommands.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericAngle)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericSelectedWeight)).EndInit();
            this.popupNodeEdit.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelBrushGradient;
		private System.Windows.Forms.Label labelAngle;
		private System.Windows.Forms.NumericUpDown numericAngle;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkScaleAngle;
		private System.Windows.Forms.CheckBox checkUseGamma;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panelGradientDisplay;
		private System.Windows.Forms.Panel panelGradControls;
		private System.Windows.Forms.Label labelSelectedColor;
		private System.Windows.Forms.NumericUpDown numericSelectedWeight;
		private System.Windows.Forms.Label labelSelectedWeight;
		private System.Windows.Forms.Panel panelPreview;
		private System.Windows.Forms.Label labelPreview;
		private System.Windows.Forms.ContextMenuStrip popupNodeEdit;
		private System.Windows.Forms.ToolStripMenuItem itemDuplicateNode;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem itemRemoveNode;
        private System.Windows.Forms.ToolStripMenuItem itemAddNode;
		private System.Windows.Forms.ToolStrip stripCommands;
		private System.Windows.Forms.ToolStripButton buttonAddNode;
		private System.Windows.Forms.ToolStripButton buttonRemoveNode;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton buttonDuplicateNode;
		private GorgonSelectablePanel panelSelectedColor;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton buttonClearNodes;
	}
}
