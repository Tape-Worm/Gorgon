
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: March 14, 2019 11:34:27 AM
// 


namespace Gorgon.Editor.SpriteEditor;

partial class SpriteEditorView
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
            if (RenderWindow != null)
            {
                RenderWindow.PreviewKeyDown -= PanelRenderWindow_PreviewKeyDown;
            }

            if (_ribbonForm != null)
            {
                _ribbonForm.ButtonSpriteCornerManualInput.Click -= VertexEditViewer_ToggleManualInput;
                _ribbonForm.ButtonClipManualInput.Click -= ClipViewer_ToggleManualInput;
            }

            if (_parentForm != null)
            {
                _parentForm.Move -= ParentForm_Move;
                _parentForm = null;
            }

            UnregisterChildPanel(typeof(SpriteTextureWrapEdit).FullName);
            UnregisterChildPanel(typeof(SpritePickMaskEditor).FullName);
            UnregisterChildPanel(typeof(SpriteColorEdit).FullName);
            UnregisterChildPanel(typeof(SpriteAnchorEdit).FullName);

            _manualRectEditor.ResizeEnd -= ManualInput_ResizeEnd;
            _manualRectEditor.FormClosing -= ManualInput_ClosePanel;
            _manualVertexEditor.ResizeEnd -= ManualVertexInput_ResizeEnd;
            _manualVertexEditor.FormClosing -= ManualVertexInput_ClosePanel;

            _manualRectEditor.Dispose();
            _manualVertexEditor.Dispose();
            _anchorTexture?.Dispose();

            _ribbonForm?.Dispose();
        }

        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        SpriteWrapping = new SpriteWrap();
        SpritePickMaskColor = new SpritePickMaskColor();
        SpriteColorSelector = new SpriteColor();
        SpriteAnchorSelector = new SpriteAnchor();
        PanelImageViewControls = new TableLayoutPanel();
        LabelSpriteInfo = new Label();
        LabelArrayIndexDetails = new Label();
        ButtonPrevArrayIndex = new Button();
        ButtonNextArrayIndex = new Button();
        LabelArrayIndex = new Label();
        flowLayoutPanel1 = new FlowLayoutPanel();
        StatusPanel.SuspendLayout();
        HostPanel.SuspendLayout();
        HostPanelControls.SuspendLayout();
        PanelImageViewControls.SuspendLayout();
        flowLayoutPanel1.SuspendLayout();
        SuspendLayout();
        // 
        // StatusPanel
        // 
        StatusPanel.Controls.Add(PanelImageViewControls);
        StatusPanel.Location = new Point(0, 760);
        StatusPanel.Size = new Size(712, 24);
        // 
        // PresentationPanel
        // 
        PresentationPanel.Location = new Point(0, 21);
        PresentationPanel.Size = new Size(712, 739);
        // 
        // HostPanel
        // 
        HostPanel.Location = new Point(712, 21);
        HostPanel.Size = new Size(396, 763);
        // 
        // HostPanelControls
        // 
        HostPanelControls.Controls.Add(flowLayoutPanel1);
        HostPanelControls.Size = new Size(395, 762);
        // 
        // SpriteWrapping
        // 
        SpriteWrapping.AutoSize = true;
        SpriteWrapping.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        SpriteWrapping.BackColor = Color.FromArgb(28, 28, 28);
        SpriteWrapping.Font = new Font("Segoe UI", 9F);
        SpriteWrapping.ForeColor = Color.White;
        SpriteWrapping.Location = new Point(3, 929);
        SpriteWrapping.Name = "SpriteWrapping";
        SpriteWrapping.Size = new Size(282, 229);
        SpriteWrapping.TabIndex = 3;
        SpriteWrapping.Text = "Sprite Texture Wrapping";
        SpriteWrapping.Visible = false;
        // 
        // SpritePickMaskColor
        // 
        SpritePickMaskColor.AutoSize = true;
        SpritePickMaskColor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        SpritePickMaskColor.BackColor = Color.FromArgb(28, 28, 28);
        SpritePickMaskColor.Font = new Font("Segoe UI", 9F);
        SpritePickMaskColor.ForeColor = Color.White;
        SpritePickMaskColor.IsModal = false;
        SpritePickMaskColor.Location = new Point(3, 264);
        SpritePickMaskColor.Name = "SpritePickMaskColor";
        SpritePickMaskColor.Size = new Size(389, 308);
        SpritePickMaskColor.TabIndex = 2;
        SpritePickMaskColor.Text = "Sprite Picker Mask Color";
        SpritePickMaskColor.Visible = false;
        // 
        // SpriteColorSelector
        // 
        SpriteColorSelector.AutoSize = true;
        SpriteColorSelector.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        SpriteColorSelector.BackColor = Color.FromArgb(28, 28, 28);
        SpriteColorSelector.Font = new Font("Segoe UI", 9F);
        SpriteColorSelector.ForeColor = Color.White;
        SpriteColorSelector.Location = new Point(3, 578);
        SpriteColorSelector.Name = "SpriteColorSelector";
        SpriteColorSelector.Size = new Size(360, 345);
        SpriteColorSelector.TabIndex = 0;
        SpriteColorSelector.Text = "Sprite Color";
        SpriteColorSelector.Visible = false;
        // 
        // SpriteAnchorSelector
        // 
        SpriteAnchorSelector.AutoSize = true;
        SpriteAnchorSelector.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        SpriteAnchorSelector.BackColor = Color.FromArgb(28, 28, 28);
        SpriteAnchorSelector.Font = new Font("Segoe UI", 9F);
        SpriteAnchorSelector.ForeColor = Color.White;
        SpriteAnchorSelector.Location = new Point(3, 3);
        SpriteAnchorSelector.Name = "SpriteAnchorSelector";
        SpriteAnchorSelector.Size = new Size(197, 255);
        SpriteAnchorSelector.TabIndex = 1;
        SpriteAnchorSelector.Text = "Sprite Anchor";
        SpriteAnchorSelector.Visible = false;
        // 
        // PanelImageViewControls
        // 
        PanelImageViewControls.AutoSize = true;
        PanelImageViewControls.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        PanelImageViewControls.ColumnCount = 7;
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle());
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle());
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle());
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle());
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle());
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16F));
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        PanelImageViewControls.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        PanelImageViewControls.Controls.Add(LabelSpriteInfo, 0, 0);
        PanelImageViewControls.Controls.Add(LabelArrayIndexDetails, 4, 0);
        PanelImageViewControls.Controls.Add(ButtonPrevArrayIndex, 3, 0);
        PanelImageViewControls.Controls.Add(ButtonNextArrayIndex, 5, 0);
        PanelImageViewControls.Controls.Add(LabelArrayIndex, 2, 0);
        PanelImageViewControls.Dock = DockStyle.Fill;
        PanelImageViewControls.Location = new Point(0, 0);
        PanelImageViewControls.MinimumSize = new Size(0, 26);
        PanelImageViewControls.Name = "PanelImageViewControls";
        PanelImageViewControls.RowCount = 1;
        PanelImageViewControls.RowStyles.Add(new RowStyle());
        PanelImageViewControls.Size = new Size(712, 26);
        PanelImageViewControls.TabIndex = 2;
        // 
        // LabelSpriteInfo
        // 
        LabelSpriteInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        LabelSpriteInfo.AutoSize = true;
        LabelSpriteInfo.Location = new Point(3, 0);
        LabelSpriteInfo.Name = "LabelSpriteInfo";
        LabelSpriteInfo.Size = new Size(189, 28);
        LabelSpriteInfo.TabIndex = 11;
        LabelSpriteInfo.Text = "Sprite Dimenions: LxT - RxB (WxH)";
        LabelSpriteInfo.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // LabelArrayIndexDetails
        // 
        LabelArrayIndexDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        LabelArrayIndexDetails.AutoSize = true;
        LabelArrayIndexDetails.Location = new Point(610, 0);
        LabelArrayIndexDetails.MinimumSize = new Size(55, 0);
        LabelArrayIndexDetails.Name = "LabelArrayIndexDetails";
        LabelArrayIndexDetails.Size = new Size(55, 28);
        LabelArrayIndexDetails.TabIndex = 9;
        LabelArrayIndexDetails.Text = "1/n";
        LabelArrayIndexDetails.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // ButtonPrevArrayIndex
        // 
        ButtonPrevArrayIndex.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        ButtonPrevArrayIndex.AutoSize = true;
        ButtonPrevArrayIndex.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonPrevArrayIndex.Enabled = false;
        ButtonPrevArrayIndex.FlatAppearance.BorderSize = 0;
        ButtonPrevArrayIndex.FlatAppearance.MouseDownBackColor = Color.Orange;
        ButtonPrevArrayIndex.FlatAppearance.MouseOverBackColor = Color.SteelBlue;
        ButtonPrevArrayIndex.FlatStyle = FlatStyle.Flat;
        ButtonPrevArrayIndex.Image = Properties.Resources.left_16x16;
        ButtonPrevArrayIndex.Location = new Point(582, 3);
        ButtonPrevArrayIndex.Name = "ButtonPrevArrayIndex";
        ButtonPrevArrayIndex.Size = new Size(22, 22);
        ButtonPrevArrayIndex.TabIndex = 8;
        ButtonPrevArrayIndex.Click += ButtonPrevArrayIndex_Click;
        // 
        // ButtonNextArrayIndex
        // 
        ButtonNextArrayIndex.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        ButtonNextArrayIndex.AutoSize = true;
        ButtonNextArrayIndex.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        ButtonNextArrayIndex.Enabled = false;
        ButtonNextArrayIndex.FlatAppearance.BorderSize = 0;
        ButtonNextArrayIndex.FlatAppearance.MouseDownBackColor = Color.Orange;
        ButtonNextArrayIndex.FlatAppearance.MouseOverBackColor = Color.SteelBlue;
        ButtonNextArrayIndex.FlatStyle = FlatStyle.Flat;
        ButtonNextArrayIndex.Image = Properties.Resources.right_16x16;
        ButtonNextArrayIndex.Location = new Point(671, 3);
        ButtonNextArrayIndex.Name = "ButtonNextArrayIndex";
        ButtonNextArrayIndex.Size = new Size(22, 22);
        ButtonNextArrayIndex.TabIndex = 10;
        ButtonNextArrayIndex.Click += ButtonNextArrayIndex_Click;
        // 
        // LabelArrayIndex
        // 
        LabelArrayIndex.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        LabelArrayIndex.AutoSize = true;
        LabelArrayIndex.Location = new Point(506, 0);
        LabelArrayIndex.Name = "LabelArrayIndex";
        LabelArrayIndex.Size = new Size(70, 28);
        LabelArrayIndex.TabIndex = 3;
        LabelArrayIndex.Text = "Array index:";
        LabelArrayIndex.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // flowLayoutPanel1
        // 
        flowLayoutPanel1.AutoSize = true;
        flowLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        flowLayoutPanel1.Controls.Add(SpriteAnchorSelector);
        flowLayoutPanel1.Controls.Add(SpritePickMaskColor);
        flowLayoutPanel1.Controls.Add(SpriteColorSelector);
        flowLayoutPanel1.Controls.Add(SpriteWrapping);
        flowLayoutPanel1.Dock = DockStyle.Fill;
        flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
        flowLayoutPanel1.Location = new Point(0, 0);
        flowLayoutPanel1.Name = "flowLayoutPanel1";
        flowLayoutPanel1.Size = new Size(395, 762);
        flowLayoutPanel1.TabIndex = 4;
        flowLayoutPanel1.WrapContents = false;
        // 
        // SpriteEditorView
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Name = "SpriteEditorView";
        Size = new Size(1108, 784);
        StatusPanel.ResumeLayout(false);
        StatusPanel.PerformLayout();
        HostPanel.ResumeLayout(false);
        HostPanel.PerformLayout();
        HostPanelControls.ResumeLayout(false);
        HostPanelControls.PerformLayout();
        PanelImageViewControls.ResumeLayout(false);
        PanelImageViewControls.PerformLayout();
        flowLayoutPanel1.ResumeLayout(false);
        flowLayoutPanel1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.TableLayoutPanel PanelImageViewControls;
    private System.Windows.Forms.Label LabelSpriteInfo;
    private System.Windows.Forms.Label LabelArrayIndexDetails;
    private System.Windows.Forms.Button ButtonPrevArrayIndex;
    private System.Windows.Forms.Button ButtonNextArrayIndex;
    private System.Windows.Forms.Label LabelArrayIndex;
    private SpriteColor SpriteColorSelector;
    private SpriteAnchor SpriteAnchorSelector;
    private SpritePickMaskColor SpritePickMaskColor;
    private SpriteWrap SpriteWrapping;
    private FlowLayoutPanel flowLayoutPanel1;
}
