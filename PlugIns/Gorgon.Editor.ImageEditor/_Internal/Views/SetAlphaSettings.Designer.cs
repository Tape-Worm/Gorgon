namespace Gorgon.Editor.ImageEditor;

partial class SetAlphaSettings
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
        }
        base.Dispose(disposing);
    }



    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.TableBody = new System.Windows.Forms.TableLayoutPanel();
        this.NumericAlphaValue = new System.Windows.Forms.NumericUpDown();
        this.LabelAlphaValue = new System.Windows.Forms.Label();
        this.label1 = new System.Windows.Forms.Label();
        this.NumericMinAlpha = new System.Windows.Forms.NumericUpDown();
        this.label4 = new System.Windows.Forms.Label();
        this.NumericMaxAlpha = new System.Windows.Forms.NumericUpDown();
        this.label2 = new System.Windows.Forms.Label();
        this.ImageAlpha = new System.Windows.Forms.PictureBox();
        this.PanelBody.SuspendLayout();
        this.TableBody.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericAlphaValue)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMinAlpha)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMaxAlpha)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ImageAlpha)).BeginInit();
        this.SuspendLayout();
        // 
        // PanelBody
        // 
        this.PanelBody.Controls.Add(this.TableBody);
        this.PanelBody.Size = new System.Drawing.Size(299, 389);
        this.PanelBody.TabIndex = 0;
        // 
        // TableBody
        // 
        this.TableBody.AutoSize = true;
        this.TableBody.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.TableBody.ColumnCount = 2;
        this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.TableBody.Controls.Add(this.ImageAlpha, 1, 6);
        this.TableBody.Controls.Add(this.label2, 0, 2);
        this.TableBody.Controls.Add(this.NumericAlphaValue, 1, 1);
        this.TableBody.Controls.Add(this.LabelAlphaValue, 0, 1);
        this.TableBody.Controls.Add(this.label1, 0, 3);
        this.TableBody.Controls.Add(this.NumericMinAlpha, 1, 3);
        this.TableBody.Controls.Add(this.label4, 0, 5);
        this.TableBody.Controls.Add(this.NumericMaxAlpha, 1, 5);
        this.TableBody.Dock = System.Windows.Forms.DockStyle.Fill;
        this.TableBody.Location = new System.Drawing.Point(0, 0);
        this.TableBody.Name = "TableBody";
        this.TableBody.RowCount = 7;
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 3F));
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
        this.TableBody.Size = new System.Drawing.Size(299, 389);
        this.TableBody.TabIndex = 0;
        // 
        // NumericAlphaValue
        // 
        this.NumericAlphaValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericAlphaValue.Location = new System.Drawing.Point(79, 8);
        this.NumericAlphaValue.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.NumericAlphaValue.Maximum = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericAlphaValue.Name = "NumericAlphaValue";
        this.NumericAlphaValue.Size = new System.Drawing.Size(201, 23);
        this.NumericAlphaValue.TabIndex = 0;
        this.NumericAlphaValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericAlphaValue.ValueChanged += new System.EventHandler(this.NumericAlphaValue_ValueChanged);
        // 
        // LabelAlphaValue
        // 
        this.LabelAlphaValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
        this.LabelAlphaValue.AutoSize = true;
        this.LabelAlphaValue.Location = new System.Drawing.Point(3, 11);
        this.LabelAlphaValue.Margin = new System.Windows.Forms.Padding(3);
        this.LabelAlphaValue.Name = "LabelAlphaValue";
        this.LabelAlphaValue.Size = new System.Drawing.Size(41, 17);
        this.LabelAlphaValue.TabIndex = 21;
        this.LabelAlphaValue.Text = "Alpha:";
        this.LabelAlphaValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label1
        // 
        this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(3, 75);
        this.label1.Margin = new System.Windows.Forms.Padding(3);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(68, 17);
        this.label1.TabIndex = 27;
        this.label1.Text = "Min. Alpha:";
        this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // NumericMinAlpha
        // 
        this.NumericMinAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericMinAlpha.Location = new System.Drawing.Point(79, 72);
        this.NumericMinAlpha.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.NumericMinAlpha.Maximum = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericMinAlpha.Name = "NumericMinAlpha";
        this.NumericMinAlpha.Size = new System.Drawing.Size(201, 23);
        this.NumericMinAlpha.TabIndex = 1;
        this.NumericMinAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericMinAlpha.ValueChanged += new System.EventHandler(this.NumericMinAlpha_ValueChanged);
        // 
        // label4
        // 
        this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
        this.label4.AutoSize = true;
        this.label4.Location = new System.Drawing.Point(3, 101);
        this.label4.Margin = new System.Windows.Forms.Padding(3);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(70, 17);
        this.label4.TabIndex = 31;
        this.label4.Text = "Max. Alpha:";
        this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // NumericMaxAlpha
        // 
        this.NumericMaxAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.NumericMaxAlpha.Location = new System.Drawing.Point(79, 98);
        this.NumericMaxAlpha.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
        this.NumericMaxAlpha.Maximum = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericMaxAlpha.Name = "NumericMaxAlpha";
        this.NumericMaxAlpha.Size = new System.Drawing.Size(201, 23);
        this.NumericMaxAlpha.TabIndex = 2;
        this.NumericMaxAlpha.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.NumericMaxAlpha.Value = new decimal(new int[] {
        255,
        0,
        0,
        0});
        this.NumericMaxAlpha.ValueChanged += new System.EventHandler(this.NumericMaxAlpha_ValueChanged);
        // 
        // label2
        // 
        this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.label2.AutoSize = true;
        this.TableBody.SetColumnSpan(this.label2, 2);
        this.label2.Location = new System.Drawing.Point(3, 39);
        this.label2.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
        this.label2.MaximumSize = new System.Drawing.Size(300, 0);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(293, 30);
        this.label2.TabIndex = 29;
        this.label2.Text = "Alpha values in the image below the minimum and above the maximum will NOT be upd" +
"ated.";
        // 
        // ImageAlpha
        // 
        this.ImageAlpha.Anchor = System.Windows.Forms.AnchorStyles.Top;
        this.ImageAlpha.BackgroundImage = global::Gorgon.Editor.ImageEditor.Properties.Resources.Transparency_Pattern;
        this.ImageAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.TableBody.SetColumnSpan(this.ImageAlpha, 2);
        this.ImageAlpha.Image = global::Gorgon.Editor.ImageEditor.Properties.Resources.alpha_48x48;
        this.ImageAlpha.Location = new System.Drawing.Point(99, 137);
        this.ImageAlpha.Margin = new System.Windows.Forms.Padding(3, 16, 3, 16);
        this.ImageAlpha.Name = "ImageAlpha";
        this.ImageAlpha.Size = new System.Drawing.Size(100, 100);
        this.ImageAlpha.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.ImageAlpha.TabIndex = 26;
        this.ImageAlpha.TabStop = false;
        this.ImageAlpha.Paint += new System.Windows.Forms.PaintEventHandler(this.ImageAlpha_Paint);
        // 
        // SetAlphaSettings
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ForeColor = System.Drawing.Color.White;
        this.Name = "SetAlphaSettings";
        this.Size = new System.Drawing.Size(299, 446);
        this.Text = "Set Alpha Value";
        this.PanelBody.ResumeLayout(false);
        this.PanelBody.PerformLayout();
        this.TableBody.ResumeLayout(false);
        this.TableBody.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.NumericAlphaValue)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMinAlpha)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.NumericMaxAlpha)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ImageAlpha)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }


    private System.Windows.Forms.TableLayoutPanel TableBody;
    private System.Windows.Forms.Label LabelAlphaValue;
    private System.Windows.Forms.NumericUpDown NumericAlphaValue;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.NumericUpDown NumericMinAlpha;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.NumericUpDown NumericMaxAlpha;
    private System.Windows.Forms.PictureBox ImageAlpha;
}
