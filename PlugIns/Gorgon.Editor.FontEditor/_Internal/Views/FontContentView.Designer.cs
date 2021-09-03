using System;

namespace Gorgon.Editor.FontEditor
{
    partial class FontContentView
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
                UnregisterChildPanel(typeof(FontPadding).FullName);
                UnregisterChildPanel(typeof(FontOutline).FullName);
                UnregisterChildPanel(typeof(FontTextureSize).FullName);
                UnregisterChildPanel(typeof(FontCharacterSelection).FullName);
                UnregisterChildPanel(typeof(FontSolidBrush).FullName);
                UnregisterChildPanel(typeof(FontPatternBrush).FullName);
                UnregisterChildPanel(typeof(FontGradientBrush).FullName);
                UnregisterChildPanel(typeof(FontTextureBrush).FullName);
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
            this.FontOutline = new Gorgon.Editor.FontEditor.FontOutlineView();
            this.FontTextureSize = new Gorgon.Editor.FontEditor.FontTextureSizeView();
            this.FontPadding = new Gorgon.Editor.FontEditor.FontPaddingView();
            this.FontCharacterSelection = new Gorgon.Editor.FontEditor.FontCharacterSelectionView();
            this.FontSolidBrush = new Gorgon.Editor.FontEditor.FontSolidBrushView();
            this.FontPatternBrush = new Gorgon.Editor.FontEditor.FontPatternBrushView();
            this.FontGradientBrush = new Gorgon.Editor.FontEditor.FontGradientBrushView();
            this.FontTextureBrush = new Gorgon.Editor.FontEditor.FontTextureBrushView();
            this.HostPanel.SuspendLayout();
            this.HostPanelControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusPanel
            // 
            this.StatusPanel.Location = new System.Drawing.Point(0, 852);
            this.StatusPanel.Size = new System.Drawing.Size(732, 27);
            this.StatusPanel.Visible = false;
            // 
            // PresentationPanel
            // 
            this.PresentationPanel.Location = new System.Drawing.Point(0, 21);
            this.PresentationPanel.Size = new System.Drawing.Size(732, 831);
            // 
            // HostPanel
            // 
            this.HostPanel.Location = new System.Drawing.Point(732, 21);
            this.HostPanel.Size = new System.Drawing.Size(693, 858);
            // 
            // HostPanelControls
            // 
            this.HostPanelControls.Controls.Add(this.FontTextureBrush);
            this.HostPanelControls.Controls.Add(this.FontGradientBrush);
            this.HostPanelControls.Controls.Add(this.FontPatternBrush);
            this.HostPanelControls.Controls.Add(this.FontSolidBrush);
            this.HostPanelControls.Controls.Add(this.FontCharacterSelection);
            this.HostPanelControls.Controls.Add(this.FontPadding);
            this.HostPanelControls.Controls.Add(this.FontTextureSize);
            this.HostPanelControls.Controls.Add(this.FontOutline);
            this.HostPanelControls.Size = new System.Drawing.Size(692, 857);
            // 
            // FontOutline
            // 
            this.FontOutline.AutoSize = true;
            this.FontOutline.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontOutline.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontOutline.ForeColor = System.Drawing.Color.White;
            this.FontOutline.IsModal = false;
            this.FontOutline.Location = new System.Drawing.Point(0, 0);
            this.FontOutline.Name = "FontOutline";
            this.FontOutline.Size = new System.Drawing.Size(392, 675);
            this.FontOutline.TabIndex = 0;
            this.FontOutline.Text = "Font Outline";
            // 
            // FontTextureSize
            // 
            this.FontTextureSize.AutoSize = true;
            this.FontTextureSize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontTextureSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontTextureSize.ForeColor = System.Drawing.Color.White;
            this.FontTextureSize.Location = new System.Drawing.Point(0, 24);
            this.FontTextureSize.Name = "FontTextureSize";
            this.FontTextureSize.Size = new System.Drawing.Size(254, 177);
            this.FontTextureSize.TabIndex = 1;
            this.FontTextureSize.Text = "Font Texture Size";
            // 
            // FontPadding
            // 
            this.FontPadding.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontPadding.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontPadding.ForeColor = System.Drawing.Color.White;
            this.FontPadding.Location = new System.Drawing.Point(0, 48);
            this.FontPadding.Name = "FontPadding";
            this.FontPadding.Size = new System.Drawing.Size(254, 177);
            this.FontPadding.TabIndex = 2;
            this.FontPadding.Text = "Font Glyph Texture Padding";
            // 
            // FontCharacterSelection
            // 
            this.FontCharacterSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontCharacterSelection.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontCharacterSelection.ForeColor = System.Drawing.Color.White;
            this.FontCharacterSelection.Location = new System.Drawing.Point(0, 72);
            this.FontCharacterSelection.Name = "FontCharacterSelection";
            this.FontCharacterSelection.Size = new System.Drawing.Size(689, 605);
            this.FontCharacterSelection.TabIndex = 3;
            this.FontCharacterSelection.Text = "Font Character Selection";
            // 
            // FontSolidBrush
            // 
            this.FontSolidBrush.AutoSize = true;
            this.FontSolidBrush.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontSolidBrush.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontSolidBrush.ForeColor = System.Drawing.Color.White;
            this.FontSolidBrush.Location = new System.Drawing.Point(0, 96);
            this.FontSolidBrush.Name = "FontSolidBrush";
            this.FontSolidBrush.Size = new System.Drawing.Size(367, 387);
            this.FontSolidBrush.TabIndex = 4;
            this.FontSolidBrush.Text = "Font Brush - Solid";
            // 
            // FontPatternBrush
            // 
            this.FontPatternBrush.AutoSize = true;
            this.FontPatternBrush.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontPatternBrush.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontPatternBrush.ForeColor = System.Drawing.Color.White;
            this.FontPatternBrush.Location = new System.Drawing.Point(0, 120);
            this.FontPatternBrush.Name = "FontPatternBrush";
            this.FontPatternBrush.Size = new System.Drawing.Size(364, 791);
            this.FontPatternBrush.TabIndex = 5;
            this.FontPatternBrush.Text = "Font Brush - Pattern";
            // 
            // FontGradientBrush
            // 
            this.FontGradientBrush.AutoSize = true;
            this.FontGradientBrush.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FontGradientBrush.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontGradientBrush.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontGradientBrush.ForeColor = System.Drawing.Color.White;
            this.FontGradientBrush.Location = new System.Drawing.Point(0, 144);
            this.FontGradientBrush.MinimumSize = new System.Drawing.Size(364, 361);
            this.FontGradientBrush.Name = "FontGradientBrush";
            this.FontGradientBrush.Size = new System.Drawing.Size(366, 687);
            this.FontGradientBrush.TabIndex = 6;
            this.FontGradientBrush.Text = "Font Brush - Gradient";
            // 
            // FontTextureBrush
            // 
            this.FontTextureBrush.AutoSize = true;
            this.FontTextureBrush.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
            this.FontTextureBrush.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FontTextureBrush.ForeColor = System.Drawing.Color.White;
            this.FontTextureBrush.Location = new System.Drawing.Point(0, 168);
            this.FontTextureBrush.Name = "FontTextureBrush";
            this.FontTextureBrush.Size = new System.Drawing.Size(364, 289);
            this.FontTextureBrush.TabIndex = 7;
            this.FontTextureBrush.Text = "Font Brush - Texture";
            // 
            // FontContentView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "FontContentView";
            this.Size = new System.Drawing.Size(1425, 879);
            this.HostPanel.ResumeLayout(false);
            this.HostPanel.PerformLayout();
            this.HostPanelControls.ResumeLayout(false);
            this.HostPanelControls.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FontOutlineView FontOutline;
        private FontTextureSizeView FontTextureSize;
        private FontPaddingView FontPadding;
        private FontCharacterSelectionView FontCharacterSelection;
        private FontSolidBrushView FontSolidBrush;
        private FontPatternBrushView FontPatternBrush;
        private FontGradientBrushView FontGradientBrush;
        private FontTextureBrushView FontTextureBrush;
    }
}
