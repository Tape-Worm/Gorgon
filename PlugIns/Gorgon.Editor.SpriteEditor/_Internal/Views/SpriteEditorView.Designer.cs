
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
        this.SpriteWrapping = new Gorgon.Editor.SpriteEditor.SpriteWrap();
        this.SpritePickMaskColor = new Gorgon.Editor.SpriteEditor.SpritePickMaskColor();
        this.SpriteColorSelector = new Gorgon.Editor.SpriteEditor.SpriteColor();
        this.SpriteAnchorSelector = new Gorgon.Editor.SpriteEditor.SpriteAnchor();
        this.PanelImageViewControls = new System.Windows.Forms.TableLayoutPanel();
        this.LabelSpriteInfo = new System.Windows.Forms.Label();
        this.LabelArrayIndexDetails = new System.Windows.Forms.Label();
        this.ButtonPrevArrayIndex = new System.Windows.Forms.Button();
        this.ButtonNextArrayIndex = new System.Windows.Forms.Button();
        this.LabelArrayIndex = new System.Windows.Forms.Label();
        this.StatusPanel.SuspendLayout();
        this.HostPanel.SuspendLayout();
        this.HostPanelControls.SuspendLayout();
        this.PanelImageViewControls.SuspendLayout();
        this.SuspendLayout();
        // 
        // StatusPanel
        // 
        this.StatusPanel.Controls.Add(this.PanelImageViewControls);
        this.StatusPanel.Location = new System.Drawing.Point(0, 760);
        this.StatusPanel.Size = new System.Drawing.Size(844, 24);
        // 
        // PresentationPanel
        // 
        this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
        this.PresentationPanel.Size = new System.Drawing.Size(844, 739);
        // 
        // HostPanel
        // 
        this.HostPanel.Location = new System.Drawing.Point(844, 21);
        this.HostPanel.Size = new System.Drawing.Size(393, 763);
        // 
        // HostPanelControls
        // 
        this.HostPanelControls.Controls.Add(this.SpriteAnchorSelector);
        this.HostPanelControls.Controls.Add(this.SpriteColorSelector);
        this.HostPanelControls.Controls.Add(this.SpriteWrapping);
        this.HostPanelControls.Controls.Add(this.SpritePickMaskColor);
        this.HostPanelControls.Size = new System.Drawing.Size(392, 762);
        // 
        // SpriteWrapping
        // 
        this.SpriteWrapping.AutoSize = true;
        this.SpriteWrapping.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.SpriteWrapping.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.SpriteWrapping.ForeColor = System.Drawing.Color.White;
        this.SpriteWrapping.Location = new System.Drawing.Point(0, 64);
        this.SpriteWrapping.Name = "SpriteWrapping";
        this.SpriteWrapping.Size = new System.Drawing.Size(389, 365);
        this.SpriteWrapping.TabIndex = 3;
        this.SpriteWrapping.Text = "Sprite Texture Wrapping";
        this.SpriteWrapping.Visible = false;
        // 
        // SpritePickMaskColor
        // 
        this.SpritePickMaskColor.AutoSize = true;
        this.SpritePickMaskColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.SpritePickMaskColor.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.SpritePickMaskColor.ForeColor = System.Drawing.Color.White;
        this.SpritePickMaskColor.IsModal = false;
        this.SpritePickMaskColor.Location = new System.Drawing.Point(0, 128);
        this.SpritePickMaskColor.Name = "SpritePickMaskColor";
        this.SpritePickMaskColor.Size = new System.Drawing.Size(389, 406);
        this.SpritePickMaskColor.TabIndex = 2;
        this.SpritePickMaskColor.Text = "Sprite Picker Mask Color";
        this.SpritePickMaskColor.Visible = false;
        // 
        // SpriteColorSelector
        // 
        this.SpriteColorSelector.AutoSize = true;
        this.SpriteColorSelector.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.SpriteColorSelector.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.SpriteColorSelector.ForeColor = System.Drawing.Color.White;
        this.SpriteColorSelector.Location = new System.Drawing.Point(0, 32);
        this.SpriteColorSelector.Name = "SpriteColorSelector";
        this.SpriteColorSelector.Size = new System.Drawing.Size(389, 406);
        this.SpriteColorSelector.TabIndex = 0;
        this.SpriteColorSelector.Text = "Sprite Color";
        this.SpriteColorSelector.Visible = false;
        // 
        // SpriteAnchorSelector
        // 
        this.SpriteAnchorSelector.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.SpriteAnchorSelector.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.SpriteAnchorSelector.ForeColor = System.Drawing.Color.White;
        this.SpriteAnchorSelector.Location = new System.Drawing.Point(0, 0);
        this.SpriteAnchorSelector.Name = "SpriteAnchorSelector";
        this.SpriteAnchorSelector.Size = new System.Drawing.Size(300, 289);
        this.SpriteAnchorSelector.TabIndex = 1;
        this.SpriteAnchorSelector.Text = "Sprite Anchor";
        this.SpriteAnchorSelector.Visible = false;
        // 
        // PanelImageViewControls
        // 
        this.PanelImageViewControls.AutoSize = true;
        this.PanelImageViewControls.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.PanelImageViewControls.ColumnCount = 7;
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 16F));
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.PanelImageViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.PanelImageViewControls.Controls.Add(this.LabelSpriteInfo, 0, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelArrayIndexDetails, 4, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonPrevArrayIndex, 3, 0);
        this.PanelImageViewControls.Controls.Add(this.ButtonNextArrayIndex, 5, 0);
        this.PanelImageViewControls.Controls.Add(this.LabelArrayIndex, 2, 0);
        this.PanelImageViewControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PanelImageViewControls.Location = new System.Drawing.Point(0, 0);
        this.PanelImageViewControls.MinimumSize = new System.Drawing.Size(0, 26);
        this.PanelImageViewControls.Name = "PanelImageViewControls";
        this.PanelImageViewControls.RowCount = 1;
        this.PanelImageViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.PanelImageViewControls.Size = new System.Drawing.Size(844, 26);
        this.PanelImageViewControls.TabIndex = 2;
        // 
        // LabelSpriteInfo
        // 
        this.LabelSpriteInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelSpriteInfo.AutoSize = true;
        this.LabelSpriteInfo.Location = new System.Drawing.Point(3, 0);
        this.LabelSpriteInfo.Name = "LabelSpriteInfo";
        this.LabelSpriteInfo.Size = new System.Drawing.Size(189, 28);
        this.LabelSpriteInfo.TabIndex = 11;
        this.LabelSpriteInfo.Text = "Sprite Dimenions: LxT - RxB (WxH)";
        this.LabelSpriteInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // LabelArrayIndexDetails
        // 
        this.LabelArrayIndexDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelArrayIndexDetails.AutoSize = true;
        this.LabelArrayIndexDetails.Location = new System.Drawing.Point(742, 0);
        this.LabelArrayIndexDetails.MinimumSize = new System.Drawing.Size(55, 0);
        this.LabelArrayIndexDetails.Name = "LabelArrayIndexDetails";
        this.LabelArrayIndexDetails.Size = new System.Drawing.Size(55, 28);
        this.LabelArrayIndexDetails.TabIndex = 9;
        this.LabelArrayIndexDetails.Text = "1/n";
        this.LabelArrayIndexDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // ButtonPrevArrayIndex
        // 
        this.ButtonPrevArrayIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonPrevArrayIndex.AutoSize = true;
        this.ButtonPrevArrayIndex.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonPrevArrayIndex.Enabled = false;
        this.ButtonPrevArrayIndex.FlatAppearance.BorderSize = 0;
        this.ButtonPrevArrayIndex.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonPrevArrayIndex.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonPrevArrayIndex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonPrevArrayIndex.Image = global::Gorgon.Editor.SpriteEditor.Properties.Resources.left_16x16;
        this.ButtonPrevArrayIndex.Location = new System.Drawing.Point(714, 3);
        this.ButtonPrevArrayIndex.Name = "ButtonPrevArrayIndex";
        this.ButtonPrevArrayIndex.Size = new System.Drawing.Size(22, 22);
        this.ButtonPrevArrayIndex.TabIndex = 8;
        this.ButtonPrevArrayIndex.Click += new System.EventHandler(this.ButtonPrevArrayIndex_Click);
        // 
        // ButtonNextArrayIndex
        // 
        this.ButtonNextArrayIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonNextArrayIndex.AutoSize = true;
        this.ButtonNextArrayIndex.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.ButtonNextArrayIndex.Enabled = false;
        this.ButtonNextArrayIndex.FlatAppearance.BorderSize = 0;
        this.ButtonNextArrayIndex.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonNextArrayIndex.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonNextArrayIndex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonNextArrayIndex.Image = global::Gorgon.Editor.SpriteEditor.Properties.Resources.right_16x16;
        this.ButtonNextArrayIndex.Location = new System.Drawing.Point(803, 3);
        this.ButtonNextArrayIndex.Name = "ButtonNextArrayIndex";
        this.ButtonNextArrayIndex.Size = new System.Drawing.Size(22, 22);
        this.ButtonNextArrayIndex.TabIndex = 10;
        this.ButtonNextArrayIndex.Click += new System.EventHandler(this.ButtonNextArrayIndex_Click);
        // 
        // LabelArrayIndex
        // 
        this.LabelArrayIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.LabelArrayIndex.AutoSize = true;
        this.LabelArrayIndex.Location = new System.Drawing.Point(638, 0);
        this.LabelArrayIndex.Name = "LabelArrayIndex";
        this.LabelArrayIndex.Size = new System.Drawing.Size(70, 28);
        this.LabelArrayIndex.TabIndex = 3;
        this.LabelArrayIndex.Text = "Array index:";
        this.LabelArrayIndex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // SpriteEditorView
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Name = "SpriteEditorView";
        this.Size = new System.Drawing.Size(1237, 784);
        this.StatusPanel.ResumeLayout(false);
        this.StatusPanel.PerformLayout();
        this.HostPanel.ResumeLayout(false);
        this.HostPanel.PerformLayout();
        this.HostPanelControls.ResumeLayout(false);
        this.HostPanelControls.PerformLayout();
        this.PanelImageViewControls.ResumeLayout(false);
        this.PanelImageViewControls.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

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
}
