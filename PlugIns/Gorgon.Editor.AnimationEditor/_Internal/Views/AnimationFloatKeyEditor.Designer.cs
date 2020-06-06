namespace Gorgon.Editor.AnimationEditor
{
    partial class AnimationFloatKeyEditor
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
                DataContext?.OnUnload();
                UnassignEvents();
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
            this.TableKeyValues = new System.Windows.Forms.TableLayoutPanel();
            this.LabelValue1 = new System.Windows.Forms.Label();
            this.NumericValue1 = new System.Windows.Forms.NumericUpDown();
            this.LabelValue2 = new System.Windows.Forms.Label();
            this.LabelValue3 = new System.Windows.Forms.Label();
            this.NumericValue2 = new System.Windows.Forms.NumericUpDown();
            this.NumericValue3 = new System.Windows.Forms.NumericUpDown();
            this.LabelValue4 = new System.Windows.Forms.Label();
            this.NumericValue4 = new System.Windows.Forms.NumericUpDown();
            this.PanelBody.SuspendLayout();
            this.TableKeyValues.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue4)).BeginInit();
            this.SuspendLayout();
            // 
            // PanelBody
            // 
            this.PanelBody.Controls.Add(this.TableKeyValues);
            this.PanelBody.Size = new System.Drawing.Size(360, 436);
            // 
            // TableKeyValues
            // 
            this.TableKeyValues.ColumnCount = 4;
            this.TableKeyValues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableKeyValues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableKeyValues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.TableKeyValues.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TableKeyValues.Controls.Add(this.LabelValue1, 0, 0);
            this.TableKeyValues.Controls.Add(this.NumericValue1, 1, 0);
            this.TableKeyValues.Controls.Add(this.LabelValue2, 2, 0);
            this.TableKeyValues.Controls.Add(this.LabelValue3, 0, 1);
            this.TableKeyValues.Controls.Add(this.NumericValue2, 3, 0);
            this.TableKeyValues.Controls.Add(this.NumericValue3, 1, 1);
            this.TableKeyValues.Controls.Add(this.LabelValue4, 2, 1);
            this.TableKeyValues.Controls.Add(this.NumericValue4, 3, 1);
            this.TableKeyValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableKeyValues.Location = new System.Drawing.Point(0, 0);
            this.TableKeyValues.Name = "TableKeyValues";
            this.TableKeyValues.RowCount = 3;
            this.TableKeyValues.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableKeyValues.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.TableKeyValues.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableKeyValues.Size = new System.Drawing.Size(360, 436);
            this.TableKeyValues.TabIndex = 0;
            // 
            // LabelValue1
            // 
            this.LabelValue1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelValue1.AutoSize = true;
            this.LabelValue1.Location = new System.Drawing.Point(3, 7);
            this.LabelValue1.Margin = new System.Windows.Forms.Padding(3);
            this.LabelValue1.Name = "LabelValue1";
            this.LabelValue1.Size = new System.Drawing.Size(47, 15);
            this.LabelValue1.TabIndex = 0;
            this.LabelValue1.Text = "Value 1:";
            this.LabelValue1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NumericValue1
            // 
            this.NumericValue1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericValue1.AutoSize = true;
            this.NumericValue1.BackColor = System.Drawing.Color.White;
            this.NumericValue1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericValue1.DecimalPlaces = 3;
            this.NumericValue1.ForeColor = System.Drawing.Color.Black;
            this.NumericValue1.Location = new System.Drawing.Point(56, 3);
            this.NumericValue1.Name = "NumericValue1";
            this.NumericValue1.Size = new System.Drawing.Size(121, 23);
            this.NumericValue1.TabIndex = 1;
            this.NumericValue1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericValue1.ValueChanged += new System.EventHandler(this.NumericValue1_ValueChanged);
            // 
            // LabelValue2
            // 
            this.LabelValue2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelValue2.AutoSize = true;
            this.LabelValue2.Location = new System.Drawing.Point(183, 7);
            this.LabelValue2.Margin = new System.Windows.Forms.Padding(3);
            this.LabelValue2.Name = "LabelValue2";
            this.LabelValue2.Size = new System.Drawing.Size(47, 15);
            this.LabelValue2.TabIndex = 0;
            this.LabelValue2.Text = "Value 2:";
            this.LabelValue2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelValue3
            // 
            this.LabelValue3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelValue3.AutoSize = true;
            this.LabelValue3.Location = new System.Drawing.Point(3, 36);
            this.LabelValue3.Margin = new System.Windows.Forms.Padding(3);
            this.LabelValue3.Name = "LabelValue3";
            this.LabelValue3.Size = new System.Drawing.Size(47, 15);
            this.LabelValue3.TabIndex = 0;
            this.LabelValue3.Text = "Value 3:";
            this.LabelValue3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NumericValue2
            // 
            this.NumericValue2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericValue2.AutoSize = true;
            this.NumericValue2.BackColor = System.Drawing.Color.White;
            this.NumericValue2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericValue2.DecimalPlaces = 3;
            this.NumericValue2.ForeColor = System.Drawing.Color.Black;
            this.NumericValue2.Location = new System.Drawing.Point(236, 3);
            this.NumericValue2.Name = "NumericValue2";
            this.NumericValue2.Size = new System.Drawing.Size(121, 23);
            this.NumericValue2.TabIndex = 1;
            this.NumericValue2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericValue2.ValueChanged += new System.EventHandler(this.NumericValue2_ValueChanged);
            // 
            // NumericValue3
            // 
            this.NumericValue3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericValue3.AutoSize = true;
            this.NumericValue3.BackColor = System.Drawing.Color.White;
            this.NumericValue3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericValue3.DecimalPlaces = 3;
            this.NumericValue3.ForeColor = System.Drawing.Color.Black;
            this.NumericValue3.Location = new System.Drawing.Point(56, 32);
            this.NumericValue3.Name = "NumericValue3";
            this.NumericValue3.Size = new System.Drawing.Size(121, 23);
            this.NumericValue3.TabIndex = 1;
            this.NumericValue3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericValue3.ValueChanged += new System.EventHandler(this.NumericValue3_ValueChanged);
            // 
            // LabelValue4
            // 
            this.LabelValue4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.LabelValue4.AutoSize = true;
            this.LabelValue4.Location = new System.Drawing.Point(183, 36);
            this.LabelValue4.Margin = new System.Windows.Forms.Padding(3);
            this.LabelValue4.Name = "LabelValue4";
            this.LabelValue4.Size = new System.Drawing.Size(47, 15);
            this.LabelValue4.TabIndex = 0;
            this.LabelValue4.Text = "Value 4:";
            this.LabelValue4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NumericValue4
            // 
            this.NumericValue4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NumericValue4.AutoSize = true;
            this.NumericValue4.BackColor = System.Drawing.Color.White;
            this.NumericValue4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NumericValue4.DecimalPlaces = 3;
            this.NumericValue4.ForeColor = System.Drawing.Color.Black;
            this.NumericValue4.Location = new System.Drawing.Point(236, 32);
            this.NumericValue4.Name = "NumericValue4";
            this.NumericValue4.Size = new System.Drawing.Size(121, 23);
            this.NumericValue4.TabIndex = 1;
            this.NumericValue4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumericValue4.ValueChanged += new System.EventHandler(this.NumericValue4_ValueChanged);
            // 
            // AnimationFloatKeyEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "AnimationFloatKeyEditor";
            this.Size = new System.Drawing.Size(360, 493);
            this.Text = "Floating Point Values";
            this.PanelBody.ResumeLayout(false);
            this.TableKeyValues.ResumeLayout(false);
            this.TableKeyValues.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericValue4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel TableKeyValues;
        private System.Windows.Forms.Label LabelValue1;
        private System.Windows.Forms.NumericUpDown NumericValue1;
        private System.Windows.Forms.Label LabelValue2;
        private System.Windows.Forms.Label LabelValue3;
        private System.Windows.Forms.Label LabelValue4;
        private System.Windows.Forms.NumericUpDown NumericValue2;
        private System.Windows.Forms.NumericUpDown NumericValue3;
        private System.Windows.Forms.NumericUpDown NumericValue4;
    }
}
