namespace Gorgon.Editor.SpriteEditor
{
    partial class SpritePickMaskColor
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
                DataContext?.Unload();
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
			this.Picker = new Gorgon.Editor.UI.Controls.ColorPicker();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.RadioColorAlpha = new System.Windows.Forms.RadioButton();
			this.RadioAlpha = new System.Windows.Forms.RadioButton();
			this.TableAlphaOnly = new System.Windows.Forms.TableLayoutPanel();
			this.SliderAlpha = new Fetze.WinFormsColor.ColorSlider();
			this.ColorShow = new Fetze.WinFormsColor.ColorShowBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.NumericAlpha = new System.Windows.Forms.NumericUpDown();
			this.LabelAlpha = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.PanelBody.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.TableAlphaOnly.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumericAlpha)).BeginInit();
			this.SuspendLayout();
			// 
			// PanelBody
			// 
			this.PanelBody.Controls.Add(this.tableLayoutPanel1);
			this.PanelBody.Size = new System.Drawing.Size(408, 551);
			this.PanelBody.TabIndex = 0;
			// 
			// Picker
			// 
			this.Picker.AutoSize = true;
			this.Picker.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Picker.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Picker.Location = new System.Drawing.Point(3, 230);
			this.Picker.Name = "Picker";
			this.Picker.Size = new System.Drawing.Size(402, 288);
			this.Picker.TabIndex = 3;
			this.Picker.Visible = false;
			this.Picker.ColorChanged += new System.EventHandler<Gorgon.Editor.UI.Controls.ColorChangedEventArgs>(this.Picker_ColorChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.RadioColorAlpha, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.RadioAlpha, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.TableAlphaOnly, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.Picker, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 4);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(408, 551);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// RadioColorAlpha
			// 
			this.RadioColorAlpha.AutoSize = true;
			this.RadioColorAlpha.Location = new System.Drawing.Point(3, 205);
			this.RadioColorAlpha.Name = "RadioColorAlpha";
			this.RadioColorAlpha.Size = new System.Drawing.Size(140, 19);
			this.RadioColorAlpha.TabIndex = 2;
			this.RadioColorAlpha.TabStop = true;
			this.RadioColorAlpha.Text = "Color and alpha value";
			this.RadioColorAlpha.UseVisualStyleBackColor = true;
			this.RadioColorAlpha.Click += new System.EventHandler(this.RadioAlpha_Click);
			// 
			// RadioAlpha
			// 
			this.RadioAlpha.AutoSize = true;
			this.RadioAlpha.Checked = true;
			this.RadioAlpha.Location = new System.Drawing.Point(3, 3);
			this.RadioAlpha.Name = "RadioAlpha";
			this.RadioAlpha.Size = new System.Drawing.Size(113, 19);
			this.RadioAlpha.TabIndex = 0;
			this.RadioAlpha.TabStop = true;
			this.RadioAlpha.Text = "Alpha value only";
			this.RadioAlpha.UseVisualStyleBackColor = true;
			this.RadioAlpha.Click += new System.EventHandler(this.RadioAlpha_Click);
			// 
			// TableAlphaOnly
			// 
			this.TableAlphaOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TableAlphaOnly.AutoSize = true;
			this.TableAlphaOnly.ColumnCount = 3;
			this.TableAlphaOnly.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableAlphaOnly.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableAlphaOnly.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.TableAlphaOnly.Controls.Add(this.SliderAlpha, 0, 0);
			this.TableAlphaOnly.Controls.Add(this.ColorShow, 1, 1);
			this.TableAlphaOnly.Controls.Add(this.label1, 1, 0);
			this.TableAlphaOnly.Controls.Add(this.label2, 1, 2);
			this.TableAlphaOnly.Controls.Add(this.NumericAlpha, 1, 5);
			this.TableAlphaOnly.Controls.Add(this.LabelAlpha, 1, 4);
			this.TableAlphaOnly.Location = new System.Drawing.Point(3, 28);
			this.TableAlphaOnly.Name = "TableAlphaOnly";
			this.TableAlphaOnly.RowCount = 6;
			this.TableAlphaOnly.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableAlphaOnly.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableAlphaOnly.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableAlphaOnly.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TableAlphaOnly.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableAlphaOnly.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableAlphaOnly.Size = new System.Drawing.Size(402, 171);
			this.TableAlphaOnly.TabIndex = 1;
			// 
			// SliderAlpha
			// 
			this.SliderAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.SliderAlpha.Location = new System.Drawing.Point(3, 3);
			this.SliderAlpha.Maximum = System.Drawing.Color.White;
			this.SliderAlpha.Minimum = System.Drawing.Color.Transparent;
			this.SliderAlpha.Name = "SliderAlpha";
			this.TableAlphaOnly.SetRowSpan(this.SliderAlpha, 6);
			this.SliderAlpha.Size = new System.Drawing.Size(165, 165);
			this.SliderAlpha.TabIndex = 0;
			this.SliderAlpha.ValuePercentual = 0F;
			this.SliderAlpha.PercentualValueChanged += new System.EventHandler(this.SliderAlpha_PercentualValueChanged);
			// 
			// ColorShow
			// 
			this.ColorShow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ColorShow.Color = System.Drawing.Color.White;
			this.ColorShow.Location = new System.Drawing.Point(174, 18);
			this.ColorShow.LowerColor = System.Drawing.Color.Transparent;
			this.ColorShow.Name = "ColorShow";
			this.ColorShow.Size = new System.Drawing.Size(70, 50);
			this.ColorShow.TabIndex = 4;
			this.ColorShow.UpperColor = System.Drawing.Color.White;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(196, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(26, 15);
			this.label1.TabIndex = 5;
			this.label1.Text = "Old";
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(193, 71);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(31, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "New";
			// 
			// NumericAlpha
			// 
			this.NumericAlpha.BackColor = System.Drawing.Color.White;
			this.NumericAlpha.ForeColor = System.Drawing.Color.Black;
			this.NumericAlpha.Location = new System.Drawing.Point(174, 124);
			this.NumericAlpha.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
			this.NumericAlpha.Name = "NumericAlpha";
			this.NumericAlpha.Size = new System.Drawing.Size(70, 23);
			this.NumericAlpha.TabIndex = 1;
			this.NumericAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NumericAlpha.ValueChanged += new System.EventHandler(this.NumericAlpha_ValueChanged);
			// 
			// LabelAlpha
			// 
			this.LabelAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LabelAlpha.AutoSize = true;
			this.LabelAlpha.Location = new System.Drawing.Point(174, 106);
			this.LabelAlpha.Name = "LabelAlpha";
			this.LabelAlpha.Size = new System.Drawing.Size(38, 15);
			this.LabelAlpha.TabIndex = 7;
			this.LabelAlpha.Text = "Alpha";
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 521);
			this.label3.MaximumSize = new System.Drawing.Size(384, 96);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(383, 60);
			this.label3.TabIndex = 4;
			this.label3.Text = "Note: \r\n\r\nYou can still use the picker tool to select sprites while this panel is" +
    " open.\r\nThis will allow you to test the settings prior to committing them.";
			// 
			// SpritePickMaskColor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "SpritePickMaskColor";
			this.Size = new System.Drawing.Size(408, 608);
			this.Text = "Sprite Pick Tool Mask Color";
			this.PanelBody.ResumeLayout(false);
			this.PanelBody.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.TableAlphaOnly.ResumeLayout(false);
			this.TableAlphaOnly.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumericAlpha)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private UI.Controls.ColorPicker Picker;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton RadioAlpha;
        private System.Windows.Forms.RadioButton RadioColorAlpha;
        private System.Windows.Forms.TableLayoutPanel TableAlphaOnly;
        private Fetze.WinFormsColor.ColorSlider SliderAlpha;
        private Fetze.WinFormsColor.ColorShowBox ColorShow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label LabelAlpha;
        private System.Windows.Forms.NumericUpDown NumericAlpha;
        private System.Windows.Forms.Label label3;
    }
}
