#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, April 15, 2014 10:10:10 PM
// 
#endregion

namespace Gorgon.Editor.FontEditorPlugIn
{
    partial class PanelFontPreferences
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.labelGlyphEdit = new System.Windows.Forms.Label();
			this.labelZoomWindowSize = new System.Windows.Forms.Label();
			this.numericZoomWindowSize = new System.Windows.Forms.NumericUpDown();
			this.labelZoomAmount = new System.Windows.Forms.Label();
			this.numericZoomAmount = new System.Windows.Forms.NumericUpDown();
			this.checkZoomSnap = new System.Windows.Forms.CheckBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.flowLayoutPanel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.textPreviewText = new System.Windows.Forms.TextBox();
			this.labelPreviewText = new System.Windows.Forms.Label();
			this.panel4 = new System.Windows.Forms.Panel();
			this.labelBackgroundColor = new System.Windows.Forms.Label();
			this.flowLayoutPanel3 = new System.Windows.Forms.Panel();
			this.checkShadowEnabled = new System.Windows.Forms.CheckBox();
			this.labelShadowOffset = new System.Windows.Forms.Label();
			this.numericShadowOffsetX = new System.Windows.Forms.NumericUpDown();
			this.labelSep = new System.Windows.Forms.Label();
			this.numericShadowOffsetY = new System.Windows.Forms.NumericUpDown();
			this.labelShadowOpacity = new System.Windows.Forms.Label();
			this.panelShadowOpacity = new Gorgon.UI.GorgonSelectablePanel();
			this.labelTextColor = new System.Windows.Forms.Label();
			this.panelTextColor = new Gorgon.UI.GorgonSelectablePanel();
			this.panelBackColor = new Gorgon.UI.GorgonSelectablePanel();
			this.labelPreview = new System.Windows.Forms.Label();
			this.panel5 = new System.Windows.Forms.Panel();
			this.panel6 = new System.Windows.Forms.Panel();
			this.checkShowAnimations = new System.Windows.Forms.CheckBox();
			this.labelFontEditorSettings = new System.Windows.Forms.Label();
			this.labelBlendMode = new System.Windows.Forms.Label();
			this.comboBlendMode = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomWindowSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomAmount)).BeginInit();
			this.panel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel4.SuspendLayout();
			this.flowLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericShadowOffsetX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericShadowOffsetY)).BeginInit();
			this.panel5.SuspendLayout();
			this.panel6.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelGlyphEdit
			// 
			this.labelGlyphEdit.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelGlyphEdit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelGlyphEdit.Location = new System.Drawing.Point(0, 0);
			this.labelGlyphEdit.Name = "labelGlyphEdit";
			this.labelGlyphEdit.Size = new System.Drawing.Size(623, 18);
			this.labelGlyphEdit.TabIndex = 0;
			this.labelGlyphEdit.Text = "glyph editor settings";
			// 
			// labelZoomWindowSize
			// 
			this.labelZoomWindowSize.AutoSize = true;
			this.labelZoomWindowSize.Location = new System.Drawing.Point(4, 28);
			this.labelZoomWindowSize.Name = "labelZoomWindowSize";
			this.labelZoomWindowSize.Size = new System.Drawing.Size(104, 15);
			this.labelZoomWindowSize.TabIndex = 1;
			this.labelZoomWindowSize.Text = "zoom window size";
			this.labelZoomWindowSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericZoomWindowSize
			// 
			this.numericZoomWindowSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericZoomWindowSize.Increment = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.numericZoomWindowSize.Location = new System.Drawing.Point(7, 46);
			this.numericZoomWindowSize.Maximum = new decimal(new int[] {
            384,
            0,
            0,
            0});
			this.numericZoomWindowSize.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
			this.numericZoomWindowSize.Name = "numericZoomWindowSize";
			this.numericZoomWindowSize.Size = new System.Drawing.Size(99, 23);
			this.numericZoomWindowSize.TabIndex = 1;
			this.numericZoomWindowSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericZoomWindowSize.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
			// 
			// labelZoomAmount
			// 
			this.labelZoomAmount.AutoSize = true;
			this.labelZoomAmount.Location = new System.Drawing.Point(123, 28);
			this.labelZoomAmount.Name = "labelZoomAmount";
			this.labelZoomAmount.Size = new System.Drawing.Size(82, 15);
			this.labelZoomAmount.TabIndex = 3;
			this.labelZoomAmount.Text = "zoom amount";
			this.labelZoomAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericZoomAmount
			// 
			this.numericZoomAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericZoomAmount.DecimalPlaces = 1;
			this.numericZoomAmount.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.numericZoomAmount.Location = new System.Drawing.Point(126, 46);
			this.numericZoomAmount.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
			this.numericZoomAmount.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numericZoomAmount.Name = "numericZoomAmount";
			this.numericZoomAmount.Size = new System.Drawing.Size(79, 23);
			this.numericZoomAmount.TabIndex = 2;
			this.numericZoomAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericZoomAmount.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// checkZoomSnap
			// 
			this.checkZoomSnap.AutoSize = true;
			this.checkZoomSnap.Location = new System.Drawing.Point(7, 6);
			this.checkZoomSnap.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.checkZoomSnap.Name = "checkZoomSnap";
			this.checkZoomSnap.Size = new System.Drawing.Size(185, 19);
			this.checkZoomSnap.TabIndex = 0;
			this.checkZoomSnap.Text = "snap zoom window to corners";
			this.checkZoomSnap.UseVisualStyleBackColor = true;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.flowLayoutPanel1);
			this.panel1.Controls.Add(this.labelGlyphEdit);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(3, 50);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(623, 95);
			this.panel1.TabIndex = 1;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.flowLayoutPanel1.Controls.Add(this.labelZoomWindowSize);
			this.flowLayoutPanel1.Controls.Add(this.numericZoomAmount);
			this.flowLayoutPanel1.Controls.Add(this.labelZoomAmount);
			this.flowLayoutPanel1.Controls.Add(this.numericZoomWindowSize);
			this.flowLayoutPanel1.Controls.Add(this.checkZoomSnap);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 18);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(623, 77);
			this.flowLayoutPanel1.TabIndex = 1;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.panel3);
			this.panel2.Controls.Add(this.panel4);
			this.panel2.Controls.Add(this.labelPreview);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(3, 145);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(623, 294);
			this.panel2.TabIndex = 2;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.panel3.Controls.Add(this.textPreviewText);
			this.panel3.Controls.Add(this.labelPreviewText);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel3.Location = new System.Drawing.Point(0, 142);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(623, 152);
			this.panel3.TabIndex = 7;
			// 
			// textPreviewText
			// 
			this.textPreviewText.AcceptsReturn = true;
			this.textPreviewText.AcceptsTab = true;
			this.textPreviewText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textPreviewText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textPreviewText.Location = new System.Drawing.Point(0, 18);
			this.textPreviewText.MaxLength = 4096;
			this.textPreviewText.Multiline = true;
			this.textPreviewText.Name = "textPreviewText";
			this.textPreviewText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textPreviewText.Size = new System.Drawing.Size(623, 134);
			this.textPreviewText.TabIndex = 0;
			this.textPreviewText.TextChanged += new System.EventHandler(this.textPreviewText_TextChanged);
			// 
			// labelPreviewText
			// 
			this.labelPreviewText.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelPreviewText.Location = new System.Drawing.Point(0, 0);
			this.labelPreviewText.Name = "labelPreviewText";
			this.labelPreviewText.Size = new System.Drawing.Size(623, 18);
			this.labelPreviewText.TabIndex = 3;
			this.labelPreviewText.Text = "preview text";
			this.labelPreviewText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.panel4.Controls.Add(this.comboBlendMode);
			this.panel4.Controls.Add(this.labelBlendMode);
			this.panel4.Controls.Add(this.labelBackgroundColor);
			this.panel4.Controls.Add(this.flowLayoutPanel3);
			this.panel4.Controls.Add(this.labelTextColor);
			this.panel4.Controls.Add(this.panelTextColor);
			this.panel4.Controls.Add(this.panelBackColor);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel4.Location = new System.Drawing.Point(0, 18);
			this.panel4.Name = "panel4";
			this.panel4.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.panel4.Size = new System.Drawing.Size(623, 124);
			this.panel4.TabIndex = 0;
			// 
			// labelBackgroundColor
			// 
			this.labelBackgroundColor.AutoSize = true;
			this.labelBackgroundColor.Location = new System.Drawing.Point(113, 74);
			this.labelBackgroundColor.Name = "labelBackgroundColor";
			this.labelBackgroundColor.Size = new System.Drawing.Size(101, 15);
			this.labelBackgroundColor.TabIndex = 4;
			this.labelBackgroundColor.Text = "background color";
			this.labelBackgroundColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// flowLayoutPanel3
			// 
			this.flowLayoutPanel3.AutoSize = true;
			this.flowLayoutPanel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.flowLayoutPanel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.flowLayoutPanel3.Controls.Add(this.checkShadowEnabled);
			this.flowLayoutPanel3.Controls.Add(this.labelShadowOffset);
			this.flowLayoutPanel3.Controls.Add(this.numericShadowOffsetX);
			this.flowLayoutPanel3.Controls.Add(this.labelSep);
			this.flowLayoutPanel3.Controls.Add(this.numericShadowOffsetY);
			this.flowLayoutPanel3.Controls.Add(this.labelShadowOpacity);
			this.flowLayoutPanel3.Controls.Add(this.panelShadowOpacity);
			this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
			this.flowLayoutPanel3.Location = new System.Drawing.Point(3, 3);
			this.flowLayoutPanel3.Name = "flowLayoutPanel3";
			this.flowLayoutPanel3.Size = new System.Drawing.Size(617, 68);
			this.flowLayoutPanel3.TabIndex = 0;
			// 
			// checkShadowEnabled
			// 
			this.checkShadowEnabled.AutoSize = true;
			this.checkShadowEnabled.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.checkShadowEnabled.Dock = System.Windows.Forms.DockStyle.Top;
			this.checkShadowEnabled.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkShadowEnabled.Location = new System.Drawing.Point(0, 0);
			this.checkShadowEnabled.Name = "checkShadowEnabled";
			this.checkShadowEnabled.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.checkShadowEnabled.Size = new System.Drawing.Size(615, 19);
			this.checkShadowEnabled.TabIndex = 0;
			this.checkShadowEnabled.Text = "enable shadow";
			this.checkShadowEnabled.UseVisualStyleBackColor = false;
			this.checkShadowEnabled.CheckedChanged += new System.EventHandler(this.checkShadowEnabled_CheckedChanged);
			// 
			// labelShadowOffset
			// 
			this.labelShadowOffset.AutoSize = true;
			this.labelShadowOffset.Enabled = false;
			this.labelShadowOffset.Location = new System.Drawing.Point(3, 22);
			this.labelShadowOffset.Name = "labelShadowOffset";
			this.labelShadowOffset.Size = new System.Drawing.Size(81, 15);
			this.labelShadowOffset.TabIndex = 2;
			this.labelShadowOffset.Text = "shadow offset";
			this.labelShadowOffset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericShadowOffsetX
			// 
			this.numericShadowOffsetX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericShadowOffsetX.Enabled = false;
			this.numericShadowOffsetX.Location = new System.Drawing.Point(5, 40);
			this.numericShadowOffsetX.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.numericShadowOffsetX.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            -2147483648});
			this.numericShadowOffsetX.Name = "numericShadowOffsetX";
			this.numericShadowOffsetX.Size = new System.Drawing.Size(43, 23);
			this.numericShadowOffsetX.TabIndex = 0;
			this.numericShadowOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelSep
			// 
			this.labelSep.AutoSize = true;
			this.labelSep.Enabled = false;
			this.labelSep.Location = new System.Drawing.Point(51, 42);
			this.labelSep.Margin = new System.Windows.Forms.Padding(0, 5, 0, 3);
			this.labelSep.Name = "labelSep";
			this.labelSep.Size = new System.Drawing.Size(10, 15);
			this.labelSep.TabIndex = 1;
			this.labelSep.Text = ",";
			this.labelSep.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// numericShadowOffsetY
			// 
			this.numericShadowOffsetY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericShadowOffsetY.Enabled = false;
			this.numericShadowOffsetY.Location = new System.Drawing.Point(64, 40);
			this.numericShadowOffsetY.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.numericShadowOffsetY.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            -2147483648});
			this.numericShadowOffsetY.Name = "numericShadowOffsetY";
			this.numericShadowOffsetY.Size = new System.Drawing.Size(43, 23);
			this.numericShadowOffsetY.TabIndex = 2;
			this.numericShadowOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelShadowOpacity
			// 
			this.labelShadowOpacity.AutoSize = true;
			this.labelShadowOpacity.Enabled = false;
			this.labelShadowOpacity.Location = new System.Drawing.Point(122, 22);
			this.labelShadowOpacity.Name = "labelShadowOpacity";
			this.labelShadowOpacity.Size = new System.Drawing.Size(90, 15);
			this.labelShadowOpacity.TabIndex = 4;
			this.labelShadowOpacity.Text = "shadow opacity";
			this.labelShadowOpacity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelShadowOpacity
			// 
			this.panelShadowOpacity.BackgroundImage = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
			this.panelShadowOpacity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelShadowOpacity.Location = new System.Drawing.Point(125, 40);
			this.panelShadowOpacity.Name = "panelShadowOpacity";
			this.panelShadowOpacity.Size = new System.Drawing.Size(87, 23);
			this.panelShadowOpacity.TabIndex = 2;
			this.panelShadowOpacity.Paint += new System.Windows.Forms.PaintEventHandler(this.panelShadowOpacity_Paint);
			this.panelShadowOpacity.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelShadowOpacity_MouseDoubleClick);
			this.panelShadowOpacity.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelShadowOpacity_PreviewKeyDown);
			// 
			// labelTextColor
			// 
			this.labelTextColor.AutoSize = true;
			this.labelTextColor.Location = new System.Drawing.Point(4, 74);
			this.labelTextColor.Name = "labelTextColor";
			this.labelTextColor.Size = new System.Drawing.Size(56, 15);
			this.labelTextColor.TabIndex = 2;
			this.labelTextColor.Text = "text color";
			this.labelTextColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panelTextColor
			// 
			this.panelTextColor.BackgroundImage = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
			this.panelTextColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelTextColor.Location = new System.Drawing.Point(4, 92);
			this.panelTextColor.Name = "panelTextColor";
			this.panelTextColor.Size = new System.Drawing.Size(99, 23);
			this.panelTextColor.TabIndex = 1;
			this.panelTextColor.Paint += new System.Windows.Forms.PaintEventHandler(this.panelTextColor_Paint);
			this.panelTextColor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelTextColor_MouseDoubleClick);
			this.panelTextColor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelTextColor_PreviewKeyDown);
			// 
			// panelBackColor
			// 
			this.panelBackColor.BackgroundImage = global::Gorgon.Editor.FontEditorPlugIn.Properties.Resources.Pattern;
			this.panelBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelBackColor.Location = new System.Drawing.Point(113, 92);
			this.panelBackColor.Name = "panelBackColor";
			this.panelBackColor.Size = new System.Drawing.Size(99, 23);
			this.panelBackColor.TabIndex = 2;
			this.panelBackColor.Paint += new System.Windows.Forms.PaintEventHandler(this.panelBackColor_Paint);
			this.panelBackColor.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelBackColor_MouseDoubleClick);
			this.panelBackColor.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.panelBackColor_PreviewKeyDown);
			// 
			// labelPreview
			// 
			this.labelPreview.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelPreview.Location = new System.Drawing.Point(0, 0);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(623, 18);
			this.labelPreview.TabIndex = 0;
			this.labelPreview.Text = "font preview settings";
			// 
			// panel5
			// 
			this.panel5.Controls.Add(this.panel6);
			this.panel5.Controls.Add(this.labelFontEditorSettings);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel5.Location = new System.Drawing.Point(3, 3);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(623, 47);
			this.panel5.TabIndex = 0;
			// 
			// panel6
			// 
			this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.panel6.Controls.Add(this.checkShowAnimations);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel6.Location = new System.Drawing.Point(0, 18);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(623, 29);
			this.panel6.TabIndex = 1;
			// 
			// checkShowAnimations
			// 
			this.checkShowAnimations.AutoSize = true;
			this.checkShowAnimations.Location = new System.Drawing.Point(7, 6);
			this.checkShowAnimations.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
			this.checkShowAnimations.Name = "checkShowAnimations";
			this.checkShowAnimations.Size = new System.Drawing.Size(169, 19);
			this.checkShowAnimations.TabIndex = 0;
			this.checkShowAnimations.Text = "show transition animations";
			this.checkShowAnimations.UseVisualStyleBackColor = true;
			// 
			// labelFontEditorSettings
			// 
			this.labelFontEditorSettings.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelFontEditorSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelFontEditorSettings.Location = new System.Drawing.Point(0, 0);
			this.labelFontEditorSettings.Name = "labelFontEditorSettings";
			this.labelFontEditorSettings.Size = new System.Drawing.Size(623, 18);
			this.labelFontEditorSettings.TabIndex = 0;
			this.labelFontEditorSettings.Text = "font editor settings";
			// 
			// labelBlendMode
			// 
			this.labelBlendMode.AutoSize = true;
			this.labelBlendMode.Location = new System.Drawing.Point(220, 74);
			this.labelBlendMode.Name = "labelBlendMode";
			this.labelBlendMode.Size = new System.Drawing.Size(74, 15);
			this.labelBlendMode.TabIndex = 5;
			this.labelBlendMode.Text = "not localized";
			this.labelBlendMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboBlendMode
			// 
			this.comboBlendMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBlendMode.FormattingEnabled = true;
			this.comboBlendMode.Location = new System.Drawing.Point(222, 92);
			this.comboBlendMode.Name = "comboBlendMode";
			this.comboBlendMode.Size = new System.Drawing.Size(121, 23);
			this.comboBlendMode.TabIndex = 6;
			// 
			// PanelFontPreferences
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.panel5);
			this.Name = "PanelFontPreferences";
			this.Size = new System.Drawing.Size(629, 442);
			((System.ComponentModel.ISupportInitialize)(this.numericZoomWindowSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericZoomAmount)).EndInit();
			this.panel1.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel4.ResumeLayout(false);
			this.panel4.PerformLayout();
			this.flowLayoutPanel3.ResumeLayout(false);
			this.flowLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericShadowOffsetX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericShadowOffsetY)).EndInit();
			this.panel5.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.panel6.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelGlyphEdit;
        private System.Windows.Forms.Label labelZoomWindowSize;
        private System.Windows.Forms.NumericUpDown numericZoomWindowSize;
        private System.Windows.Forms.Label labelZoomAmount;
        private System.Windows.Forms.NumericUpDown numericZoomAmount;
        private System.Windows.Forms.CheckBox checkZoomSnap;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.Panel flowLayoutPanel3;
        private System.Windows.Forms.CheckBox checkShadowEnabled;
        private System.Windows.Forms.Label labelShadowOffset;
        private System.Windows.Forms.Label labelShadowOpacity;
        private UI.GorgonSelectablePanel panelShadowOpacity;
        private System.Windows.Forms.Label labelTextColor;
        private UI.GorgonSelectablePanel panelTextColor;
        private System.Windows.Forms.Label labelBackgroundColor;
        private UI.GorgonSelectablePanel panelBackColor;
        private System.Windows.Forms.NumericUpDown numericShadowOffsetX;
        private System.Windows.Forms.Label labelSep;
        private System.Windows.Forms.NumericUpDown numericShadowOffsetY;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox textPreviewText;
        private System.Windows.Forms.Label labelPreviewText;
        private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.CheckBox checkShowAnimations;
		private System.Windows.Forms.Label labelFontEditorSettings;
		private System.Windows.Forms.ComboBox comboBlendMode;
		private System.Windows.Forms.Label labelBlendMode;

    }
}
