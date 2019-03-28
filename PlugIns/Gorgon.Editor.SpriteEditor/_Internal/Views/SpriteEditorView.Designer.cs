#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 14, 2019 11:34:27 AM
// 
#endregion

namespace Gorgon.Editor.SpriteEditor
{
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
                if (_parentForm != null)
                {
                    _parentForm.ResizeEnd -= ParentForm_Move;
                    _parentForm = null;
                }

                _manualRectInput.ResizeEnd -= ManualRectInput_ResizeEnd;
                _manualRectInput.FormClosing -= ManualInput_ClosePanel;
                _manualRectInput.Dispose();
                PanelRenderWindow.MouseWheel -= PanelRenderWindow_MouseWheel;
                _ribbonForm.ImageZoomed -= RibbonForm_ImageZoomed;
                _ribbonForm?.Dispose();
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
            this.TableViewControls = new System.Windows.Forms.TableLayoutPanel();
            this.ScrollHorizontal = new System.Windows.Forms.HScrollBar();
            this.ScrollVertical = new System.Windows.Forms.VScrollBar();
            this.PanelRenderWindow = new Gorgon.Windows.UI.GorgonSelectablePanel();
            this.SpriteAnchorSelector = new Gorgon.Editor.SpriteEditor.SpriteAnchor();
            this.SpriteColorSelector = new Gorgon.Editor.SpriteEditor.SpriteColor();
            this.ButtonCenter = new System.Windows.Forms.Button();
            this.PanelBottomBar = new System.Windows.Forms.Panel();
            this.PanelImageViewControls = new System.Windows.Forms.TableLayoutPanel();
            this.LabelSpriteInfo = new System.Windows.Forms.Label();
            this.LabelArrayIndexDetails = new System.Windows.Forms.Label();
            this.ButtonPrevArrayIndex = new System.Windows.Forms.Button();
            this.ButtonNextArrayIndex = new System.Windows.Forms.Button();
            this.LabelArrayIndex = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PresentationPanel)).BeginInit();
            this.PresentationPanel.SuspendLayout();
            this.TableViewControls.SuspendLayout();
            this.PanelRenderWindow.SuspendLayout();
            this.PanelBottomBar.SuspendLayout();
            this.PanelImageViewControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // PresentationPanel
            // 
            this.PresentationPanel.Controls.Add(this.TableViewControls);
            this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
            this.PresentationPanel.Size = new System.Drawing.Size(948, 637);
            // 
            // TableViewControls
            // 
            this.TableViewControls.ColumnCount = 2;
            this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableViewControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableViewControls.Controls.Add(this.ScrollHorizontal, 0, 1);
            this.TableViewControls.Controls.Add(this.ScrollVertical, 1, 0);
            this.TableViewControls.Controls.Add(this.PanelRenderWindow, 0, 0);
            this.TableViewControls.Controls.Add(this.ButtonCenter, 1, 1);
            this.TableViewControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableViewControls.Location = new System.Drawing.Point(0, 0);
            this.TableViewControls.Name = "TableViewControls";
            this.TableViewControls.RowCount = 2;
            this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableViewControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.TableViewControls.Size = new System.Drawing.Size(948, 637);
            this.TableViewControls.TabIndex = 1;
            // 
            // ScrollHorizontal
            // 
            this.ScrollHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScrollHorizontal.Enabled = false;
            this.ScrollHorizontal.Location = new System.Drawing.Point(0, 615);
            this.ScrollHorizontal.Maximum = 110;
            this.ScrollHorizontal.Minimum = -100;
            this.ScrollHorizontal.Name = "ScrollHorizontal";
            this.ScrollHorizontal.Size = new System.Drawing.Size(926, 22);
            this.ScrollHorizontal.TabIndex = 0;
            this.ScrollHorizontal.ValueChanged += new System.EventHandler(this.ScrollHorizontal_ValueChanged);
            // 
            // ScrollVertical
            // 
            this.ScrollVertical.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScrollVertical.Enabled = false;
            this.ScrollVertical.Location = new System.Drawing.Point(926, 0);
            this.ScrollVertical.Maximum = 110;
            this.ScrollVertical.Minimum = -100;
            this.ScrollVertical.Name = "ScrollVertical";
            this.ScrollVertical.Size = new System.Drawing.Size(22, 615);
            this.ScrollVertical.TabIndex = 1;
            this.ScrollVertical.ValueChanged += new System.EventHandler(this.ScrollHorizontal_ValueChanged);
            // 
            // PanelRenderWindow
            // 
            this.PanelRenderWindow.AllowDrop = true;
            this.PanelRenderWindow.Controls.Add(this.SpriteAnchorSelector);
            this.PanelRenderWindow.Controls.Add(this.SpriteColorSelector);
            this.PanelRenderWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelRenderWindow.Location = new System.Drawing.Point(3, 3);
            this.PanelRenderWindow.Name = "PanelRenderWindow";
            this.PanelRenderWindow.ShowFocus = false;
            this.PanelRenderWindow.Size = new System.Drawing.Size(920, 609);
            this.PanelRenderWindow.TabIndex = 2;
            this.PanelRenderWindow.RenderToBitmap += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.PanelRenderWindow_RenderToBitmap);
            this.PanelRenderWindow.DragDrop += new System.Windows.Forms.DragEventHandler(this.PanelRenderWindow_DragDrop);
            this.PanelRenderWindow.DragEnter += new System.Windows.Forms.DragEventHandler(this.PanelRenderWindow_DragEnter);
            this.PanelRenderWindow.Enter += new System.EventHandler(this.RenderControl_GotFocus);
            this.PanelRenderWindow.Leave += new System.EventHandler(this.RenderControl_LostFocus);
            this.PanelRenderWindow.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelRenderWindow_MouseDown);
            this.PanelRenderWindow.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelRenderWindow_MouseMove);
            this.PanelRenderWindow.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelRenderWindow_MouseUp);
            this.PanelRenderWindow.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.PanelRenderWindow_PreviewKeyDown);
            // 
            // SpriteAnchorSelector
            // 
            this.SpriteAnchorSelector.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.SpriteAnchorSelector.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.SpriteAnchorSelector.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.SpriteAnchorSelector.Location = new System.Drawing.Point(117, 67);
            this.SpriteAnchorSelector.Name = "SpriteAnchorSelector";
            this.SpriteAnchorSelector.Size = new System.Drawing.Size(248, 241);
            this.SpriteAnchorSelector.TabIndex = 1;
            this.SpriteAnchorSelector.Text = "Sprite Anchor";
            this.SpriteAnchorSelector.Visible = false;
            // 
            // SpriteColorSelector
            // 
            this.SpriteColorSelector.AutoSize = true;
            this.SpriteColorSelector.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.SpriteColorSelector.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.SpriteColorSelector.ForeColor = System.Drawing.Color.White;
            this.SpriteColorSelector.Location = new System.Drawing.Point(414, 47);
            this.SpriteColorSelector.Name = "SpriteColorSelector";
            this.SpriteColorSelector.Size = new System.Drawing.Size(423, 406);
            this.SpriteColorSelector.TabIndex = 0;
            this.SpriteColorSelector.Text = "Sprite Color";
            this.SpriteColorSelector.Visible = false;
            // 
            // ButtonCenter
            // 
            this.ButtonCenter.AutoSize = true;
            this.ButtonCenter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ButtonCenter.FlatAppearance.BorderSize = 0;
            this.ButtonCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
            this.ButtonCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.ButtonCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCenter.Image = global::Gorgon.Editor.SpriteEditor.Properties.Resources.center_16x16;
            this.ButtonCenter.Location = new System.Drawing.Point(926, 615);
            this.ButtonCenter.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonCenter.Name = "ButtonCenter";
            this.ButtonCenter.Size = new System.Drawing.Size(22, 22);
            this.ButtonCenter.TabIndex = 3;
            this.ButtonCenter.Click += new System.EventHandler(this.ButtonCenter_Click);
            // 
            // PanelBottomBar
            // 
            this.PanelBottomBar.AutoSize = true;
            this.PanelBottomBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PanelBottomBar.Controls.Add(this.PanelImageViewControls);
            this.PanelBottomBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PanelBottomBar.Location = new System.Drawing.Point(0, 658);
            this.PanelBottomBar.Name = "PanelBottomBar";
            this.PanelBottomBar.Size = new System.Drawing.Size(948, 28);
            this.PanelBottomBar.TabIndex = 8;
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
            this.PanelImageViewControls.Size = new System.Drawing.Size(948, 28);
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
            this.LabelSpriteInfo.Size = new System.Drawing.Size(187, 28);
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
            this.LabelArrayIndexDetails.Location = new System.Drawing.Point(846, 0);
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
            this.ButtonPrevArrayIndex.Location = new System.Drawing.Point(818, 3);
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
            this.ButtonNextArrayIndex.Location = new System.Drawing.Point(907, 3);
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
            this.LabelArrayIndex.Location = new System.Drawing.Point(743, 0);
            this.LabelArrayIndex.Name = "LabelArrayIndex";
            this.LabelArrayIndex.Size = new System.Drawing.Size(69, 28);
            this.LabelArrayIndex.TabIndex = 3;
            this.LabelArrayIndex.Text = "Array index:";
            this.LabelArrayIndex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SpriteEditorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PanelBottomBar);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "SpriteEditorView";
            this.RenderControl = this.PanelRenderWindow;
            this.Size = new System.Drawing.Size(948, 686);
            this.Controls.SetChildIndex(this.PanelBottomBar, 0);
            this.Controls.SetChildIndex(this.PresentationPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.PresentationPanel)).EndInit();
            this.PresentationPanel.ResumeLayout(false);
            this.TableViewControls.ResumeLayout(false);
            this.TableViewControls.PerformLayout();
            this.PanelRenderWindow.ResumeLayout(false);
            this.PanelRenderWindow.PerformLayout();
            this.PanelBottomBar.ResumeLayout(false);
            this.PanelBottomBar.PerformLayout();
            this.PanelImageViewControls.ResumeLayout(false);
            this.PanelImageViewControls.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TableViewControls;
        private System.Windows.Forms.HScrollBar ScrollHorizontal;
        private System.Windows.Forms.VScrollBar ScrollVertical;
        private Gorgon.Windows.UI.GorgonSelectablePanel PanelRenderWindow;
        private System.Windows.Forms.Button ButtonCenter;
        private System.Windows.Forms.Panel PanelBottomBar;
        private System.Windows.Forms.TableLayoutPanel PanelImageViewControls;
        private System.Windows.Forms.Label LabelSpriteInfo;
        private System.Windows.Forms.Label LabelArrayIndexDetails;
        private System.Windows.Forms.Button ButtonPrevArrayIndex;
        private System.Windows.Forms.Button ButtonNextArrayIndex;
        private System.Windows.Forms.Label LabelArrayIndex;
        private SpriteColor SpriteColorSelector;
        private SpriteAnchor SpriteAnchorSelector;
    }
}
