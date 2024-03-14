using Gorgon.UI;

namespace Gorgon.Editor.FontEditor;

    partial class FontPatternBrushView
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
                if (ComboHatch != null)
                {
                    ComboHatch.SelectedIndexChanged -= ComboHatch_SelectedIndexChanged;
                }

            _previewBitmap?.Dispose();
            _previewBitmap = null;
            }

            base.Dispose(disposing);
        }

    

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        this.PanelPreview = new System.Windows.Forms.Panel();
        this.LabelPatternType = new System.Windows.Forms.Label();
        this.LabelForegroundColor = new System.Windows.Forms.Label();
        this.LabelBackgroundColor = new System.Windows.Forms.Label();
        this.ComboHatch = new Gorgon.Editor.FontEditor.ComboPatterns();
        this.LabelPreview = new System.Windows.Forms.Label();
        this.TableControls = new System.Windows.Forms.TableLayoutPanel();
        this.TableOptions = new System.Windows.Forms.TableLayoutPanel();
        this.PickerBackground = new Gorgon.Editor.UI.Controls.ColorPicker();
        this.PickerForeground = new Gorgon.Editor.UI.Controls.ColorPicker();
        this.PanelBody.SuspendLayout();
        this.TableControls.SuspendLayout();
        this.TableOptions.SuspendLayout();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableControls);
        this.PanelBody.Size = new System.Drawing.Size(364, 717);
        // 
        // PanelPreview
        // 
        this.PanelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.PanelPreview.BackgroundImage = global::Gorgon.Editor.FontEditor.Properties.Resources.Transparency_Pattern;
        this.PanelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.PanelPreview.Location = new System.Drawing.Point(6, 679);
        this.PanelPreview.Margin = new System.Windows.Forms.Padding(6, 0, 6, 6);
        this.PanelPreview.Name = "PanelPreview";
        this.PanelPreview.Size = new System.Drawing.Size(352, 32);
        this.PanelPreview.TabIndex = 2;
        this.PanelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelPreview_Paint);
        // 
        // LabelPatternType
        // 
        this.LabelPatternType.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.LabelPatternType.AutoSize = true;
        this.LabelPatternType.Location = new System.Drawing.Point(3, 3);
        this.LabelPatternType.Margin = new System.Windows.Forms.Padding(3);
        this.LabelPatternType.Name = "LabelPatternType";
        this.LabelPatternType.Size = new System.Drawing.Size(75, 15);
        this.LabelPatternType.TabIndex = 0;
        this.LabelPatternType.Text = "Pattern Type:";
        // 
        // LabelForegroundColor
        // 
        this.LabelForegroundColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.LabelForegroundColor.AutoSize = true;
        this.LabelForegroundColor.Location = new System.Drawing.Point(3, 51);
        this.LabelForegroundColor.Margin = new System.Windows.Forms.Padding(3);
        this.LabelForegroundColor.Name = "LabelForegroundColor";
        this.LabelForegroundColor.Size = new System.Drawing.Size(104, 15);
        this.LabelForegroundColor.TabIndex = 2;
        this.LabelForegroundColor.Text = "Foreground Color:";
        // 
        // LabelBackgroundColor
        // 
        this.LabelBackgroundColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.LabelBackgroundColor.AutoSize = true;
        this.LabelBackgroundColor.Location = new System.Drawing.Point(3, 366);
        this.LabelBackgroundColor.Margin = new System.Windows.Forms.Padding(3);
        this.LabelBackgroundColor.Name = "LabelBackgroundColor";
        this.LabelBackgroundColor.Size = new System.Drawing.Size(106, 15);
        this.LabelBackgroundColor.TabIndex = 4;
        this.LabelBackgroundColor.Text = "Background Color:";
        // 
        // ComboHatch
        // 
        this.ComboHatch.BackColor = System.Drawing.Color.White;
        this.ComboHatch.Dock = System.Windows.Forms.DockStyle.Fill;
        this.ComboHatch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.ComboHatch.ForeColor = System.Drawing.Color.Black;
        this.ComboHatch.FormattingEnabled = true;
        this.ComboHatch.Location = new System.Drawing.Point(3, 21);
        this.ComboHatch.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
        this.ComboHatch.Name = "ComboHatch";
        this.ComboHatch.Size = new System.Drawing.Size(358, 24);
        this.ComboHatch.Style = System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal;
        this.ComboHatch.TabIndex = 1;
        // 
        // LabelPreview
        // 
        this.LabelPreview.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.LabelPreview.AutoSize = true;
        this.LabelPreview.Location = new System.Drawing.Point(158, 661);
        this.LabelPreview.Margin = new System.Windows.Forms.Padding(3);
        this.LabelPreview.Name = "LabelPreview";
        this.LabelPreview.Size = new System.Drawing.Size(48, 15);
        this.LabelPreview.TabIndex = 1;
        this.LabelPreview.Text = "Preview";
        this.LabelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // TableControls
        // 
        this.TableControls.AutoSize = true;
        this.TableControls.ColumnCount = 1;
        this.TableControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableControls.Controls.Add(this.TableOptions, 0, 0);
        this.TableControls.Controls.Add(this.PanelPreview, 0, 2);
        this.TableControls.Controls.Add(this.LabelPreview, 0, 1);
        this.TableControls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableControls.Location = new System.Drawing.Point(0, 0);
        this.TableControls.Margin = new System.Windows.Forms.Padding(0);
        this.TableControls.Name = "TableControls";
        this.TableControls.RowCount = 3;
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableControls.Size = new System.Drawing.Size(364, 717);
        this.TableControls.TabIndex = 0;
        // 
        // TableOptions
        // 
        this.TableOptions.AutoSize = true;
        this.TableOptions.ColumnCount = 1;
        this.TableOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableOptions.Controls.Add(this.PickerBackground, 0, 5);
        this.TableOptions.Controls.Add(this.LabelPatternType, 0, 0);
        this.TableOptions.Controls.Add(this.LabelBackgroundColor, 0, 4);
        this.TableOptions.Controls.Add(this.LabelForegroundColor, 0, 2);
        this.TableOptions.Controls.Add(this.ComboHatch, 0, 1);
        this.TableOptions.Controls.Add(this.PickerForeground, 0, 3);
        this.TableOptions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableOptions.Location = new System.Drawing.Point(0, 0);
        this.TableOptions.Margin = new System.Windows.Forms.Padding(0);
        this.TableOptions.Name = "TableOptions";
        this.TableOptions.RowCount = 6;
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableOptions.Size = new System.Drawing.Size(364, 658);
        this.TableOptions.TabIndex = 0;
        // 
        // PickerBackground
        // 
        this.PickerBackground.AutoSize = true;
        this.PickerBackground.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PickerBackground.Location = new System.Drawing.Point(0, 384);
        this.PickerBackground.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
        this.PickerBackground.Name = "PickerBackground";
        this.PickerBackground.Size = new System.Drawing.Size(364, 288);
        this.PickerBackground.TabIndex = 5;
        // 
        // PickerForeground
        // 
        this.PickerForeground.AutoSize = true;
        this.PickerForeground.Dock = System.Windows.Forms.DockStyle.Fill;
        this.PickerForeground.Location = new System.Drawing.Point(0, 72);
        this.PickerForeground.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
        this.PickerForeground.Name = "PickerForeground";
        this.PickerForeground.Size = new System.Drawing.Size(364, 288);
        this.PickerForeground.TabIndex = 3;
        // 
        // FontPatternBrushView
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.Name = "FontPatternBrushView";
        this.Size = new System.Drawing.Size(364, 774);
        this.Text = "Font Brush - Pattern";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.TableControls.ResumeLayout(false);
        this.TableControls.PerformLayout();
        this.TableOptions.ResumeLayout(false);
        this.TableOptions.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

        }

    

        private System.Windows.Forms.Panel PanelPreview;
        private System.Windows.Forms.Label LabelPatternType;
        private ComboPatterns ComboHatch;
        private System.Windows.Forms.Label LabelForegroundColor;
        private System.Windows.Forms.Label LabelBackgroundColor;
        private System.Windows.Forms.Label LabelPreview;
    private System.Windows.Forms.TableLayoutPanel TableControls;
    private System.Windows.Forms.TableLayoutPanel TableOptions;
    private UI.Controls.ColorPicker PickerBackground;
    private UI.Controls.ColorPicker PickerForeground;
}
