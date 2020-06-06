namespace Gorgon.Editor.AnimationEditor
{
    partial class AnimationColorKeyEditor
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
                UnassignEvents();
                DataContext?.OnUnload();
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
            this.PickerColor = new Gorgon.Editor.UI.Controls.ColorPicker();
            this.TableColors = new System.Windows.Forms.TableLayoutPanel();
            this.PickerAlpha = new Gorgon.Editor.UI.Controls.AlphaPicker();
            this.PanelBody.SuspendLayout();
            this.TableColors.SuspendLayout();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.TableColors);
            this.PanelBody.Size = new System.Drawing.Size(364, 411);
            // 
            // PickerColor
            // 
            this.PickerColor.AutoSize = true;
            this.PickerColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PickerColor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.PickerColor.Location = new System.Drawing.Point(0, 0);
            this.PickerColor.Margin = new System.Windows.Forms.Padding(0);
            this.PickerColor.Name = "PickerColor";
            this.PickerColor.Size = new System.Drawing.Size(364, 288);
            this.PickerColor.TabIndex = 0;
            this.PickerColor.ColorChanged += new System.EventHandler<Gorgon.Editor.UI.Controls.ColorChangedEventArgs>(this.Picker_ColorChanged);
            // 
            // TableColors
            // 
            this.TableColors.AutoSize = true;
            this.TableColors.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TableColors.ColumnCount = 1;
            this.TableColors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableColors.Controls.Add(this.PickerColor, 0, 0);
            this.TableColors.Controls.Add(this.PickerAlpha, 0, 1);
            this.TableColors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableColors.Location = new System.Drawing.Point(0, 0);
            this.TableColors.Name = "TableColors";
            this.TableColors.RowCount = 2;
            this.TableColors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableColors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableColors.Size = new System.Drawing.Size(364, 411);
            this.TableColors.TabIndex = 1;
            // 
            // PickerAlpha
            // 
            this.PickerAlpha.AlphaValue = 0;
            this.PickerAlpha.AutoSize = true;
            this.PickerAlpha.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
            this.PickerAlpha.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PickerAlpha.Location = new System.Drawing.Point(3, 291);
            this.PickerAlpha.Name = "PickerAlpha";
            this.PickerAlpha.Size = new System.Drawing.Size(358, 117);
            this.PickerAlpha.TabIndex = 1;
            this.PickerAlpha.AlphaValueChanged += new System.EventHandler(this.PickerAlpha_AlphaValueChanged);
            // 
            // AnimationColorKeyEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "AnimationColorKeyEditor";
            this.Size = new System.Drawing.Size(364, 468);
            this.Text = "Key Color";
            this.PanelBody.ResumeLayout(false);
            this.PanelBody.PerformLayout();
            this.TableColors.ResumeLayout(false);
            this.TableColors.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UI.Controls.ColorPicker PickerColor;
        private System.Windows.Forms.TableLayoutPanel TableColors;
        private UI.Controls.AlphaPicker PickerAlpha;
    }
}
