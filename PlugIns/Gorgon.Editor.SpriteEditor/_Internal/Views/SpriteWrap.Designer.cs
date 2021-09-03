namespace Gorgon.Editor.SpriteEditor
{
    partial class SpriteWrap
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
			this.TableHWrapping = new System.Windows.Forms.TableLayoutPanel();
			this.RadioHBorder = new System.Windows.Forms.RadioButton();
			this.RadioHMirrorOnce = new System.Windows.Forms.RadioButton();
			this.RadioHMirror = new System.Windows.Forms.RadioButton();
			this.RadioHWrap = new System.Windows.Forms.RadioButton();
			this.RadioHClamp = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.RadioVClamp = new System.Windows.Forms.RadioButton();
			this.RadioVWrap = new System.Windows.Forms.RadioButton();
			this.RadioVMirror = new System.Windows.Forms.RadioButton();
			this.RadioVMirrorOnce = new System.Windows.Forms.RadioButton();
			this.RadioVBorder = new System.Windows.Forms.RadioButton();
			this.LabelBorderColor = new System.Windows.Forms.Label();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.TableVWrapping = new System.Windows.Forms.TableLayoutPanel();
			this.PanelBody.SuspendLayout();
			this.TableHWrapping.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.TableVWrapping.SuspendLayout();
			this.SuspendLayout();
			// 
			// PanelBody
			// 
			this.PanelBody.Controls.Add(this.tableLayoutPanel2);
			this.PanelBody.Size = new System.Drawing.Size(364, 486);
			this.PanelBody.TabIndex = 0;
			// 
			// Picker
			// 
			this.Picker.AutoSize = true;
			this.tableLayoutPanel2.SetColumnSpan(this.Picker, 2);
			this.Picker.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Picker.Location = new System.Drawing.Point(3, 184);
			this.Picker.Name = "Picker";
			this.Picker.Size = new System.Drawing.Size(360, 288);
			this.Picker.TabIndex = 2;
			this.Picker.Visible = false;
			this.Picker.ColorChanged += new System.EventHandler<Gorgon.Editor.UI.Controls.ColorChangedEventArgs>(this.Picker_ColorChanged);
			// 
			// TableHWrapping
			// 
			this.TableHWrapping.AutoSize = true;
			this.TableHWrapping.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TableHWrapping.ColumnCount = 1;
			this.TableHWrapping.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableHWrapping.Controls.Add(this.RadioHBorder, 0, 5);
			this.TableHWrapping.Controls.Add(this.RadioHMirrorOnce, 0, 4);
			this.TableHWrapping.Controls.Add(this.RadioHMirror, 0, 3);
			this.TableHWrapping.Controls.Add(this.RadioHWrap, 0, 2);
			this.TableHWrapping.Controls.Add(this.RadioHClamp, 0, 1);
			this.TableHWrapping.Controls.Add(this.label1, 0, 0);
			this.TableHWrapping.Location = new System.Drawing.Point(3, 3);
			this.TableHWrapping.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.TableHWrapping.Name = "TableHWrapping";
			this.TableHWrapping.RowCount = 6;
			this.TableHWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableHWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableHWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableHWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableHWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableHWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableHWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableHWrapping.Size = new System.Drawing.Size(144, 142);
			this.TableHWrapping.TabIndex = 0;
			// 
			// RadioHBorder
			// 
			this.RadioHBorder.AutoSize = true;
			this.RadioHBorder.Location = new System.Drawing.Point(6, 120);
			this.RadioHBorder.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioHBorder.Name = "RadioHBorder";
			this.RadioHBorder.Size = new System.Drawing.Size(60, 19);
			this.RadioHBorder.TabIndex = 4;
			this.RadioHBorder.Text = "Border";
			this.RadioHBorder.UseVisualStyleBackColor = true;
			this.RadioHBorder.Click += new System.EventHandler(this.RadioHWrapping_Click);
			// 
			// RadioHMirrorOnce
			// 
			this.RadioHMirrorOnce.AutoSize = true;
			this.RadioHMirrorOnce.Location = new System.Drawing.Point(6, 95);
			this.RadioHMirrorOnce.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioHMirrorOnce.Name = "RadioHMirrorOnce";
			this.RadioHMirrorOnce.Size = new System.Drawing.Size(87, 19);
			this.RadioHMirrorOnce.TabIndex = 3;
			this.RadioHMirrorOnce.Text = "Mirror once";
			this.RadioHMirrorOnce.UseVisualStyleBackColor = true;
			this.RadioHMirrorOnce.Click += new System.EventHandler(this.RadioHWrapping_Click);
			// 
			// RadioHMirror
			// 
			this.RadioHMirror.AutoSize = true;
			this.RadioHMirror.Location = new System.Drawing.Point(6, 70);
			this.RadioHMirror.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioHMirror.Name = "RadioHMirror";
			this.RadioHMirror.Size = new System.Drawing.Size(58, 19);
			this.RadioHMirror.TabIndex = 2;
			this.RadioHMirror.Text = "Mirror";
			this.RadioHMirror.UseVisualStyleBackColor = true;
			this.RadioHMirror.Click += new System.EventHandler(this.RadioHWrapping_Click);
			// 
			// RadioHWrap
			// 
			this.RadioHWrap.AutoSize = true;
			this.RadioHWrap.Location = new System.Drawing.Point(6, 45);
			this.RadioHWrap.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioHWrap.Name = "RadioHWrap";
			this.RadioHWrap.Size = new System.Drawing.Size(53, 19);
			this.RadioHWrap.TabIndex = 1;
			this.RadioHWrap.Text = "Wrap";
			this.RadioHWrap.UseVisualStyleBackColor = true;
			this.RadioHWrap.Click += new System.EventHandler(this.RadioHWrapping_Click);
			// 
			// RadioHClamp
			// 
			this.RadioHClamp.AutoSize = true;
			this.RadioHClamp.Checked = true;
			this.RadioHClamp.Location = new System.Drawing.Point(6, 20);
			this.RadioHClamp.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioHClamp.Name = "RadioHClamp";
			this.RadioHClamp.Size = new System.Drawing.Size(60, 19);
			this.RadioHClamp.TabIndex = 0;
			this.RadioHClamp.TabStop = true;
			this.RadioHClamp.Text = "Clamp";
			this.RadioHClamp.UseVisualStyleBackColor = true;
			this.RadioHClamp.Click += new System.EventHandler(this.RadioHWrapping_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F);
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(138, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Horizontal Wrapping:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F);
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(118, 17);
			this.label2.TabIndex = 1;
			this.label2.Text = "Vertical Wrapping:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// RadioVClamp
			// 
			this.RadioVClamp.AutoSize = true;
			this.RadioVClamp.Checked = true;
			this.RadioVClamp.Location = new System.Drawing.Point(6, 20);
			this.RadioVClamp.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioVClamp.Name = "RadioVClamp";
			this.RadioVClamp.Size = new System.Drawing.Size(60, 19);
			this.RadioVClamp.TabIndex = 0;
			this.RadioVClamp.TabStop = true;
			this.RadioVClamp.Text = "Clamp";
			this.RadioVClamp.UseVisualStyleBackColor = true;
			this.RadioVClamp.Click += new System.EventHandler(this.RadioVWrapping_Click);
			// 
			// RadioVWrap
			// 
			this.RadioVWrap.AutoSize = true;
			this.RadioVWrap.Location = new System.Drawing.Point(6, 45);
			this.RadioVWrap.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioVWrap.Name = "RadioVWrap";
			this.RadioVWrap.Size = new System.Drawing.Size(53, 19);
			this.RadioVWrap.TabIndex = 1;
			this.RadioVWrap.Text = "Wrap";
			this.RadioVWrap.UseVisualStyleBackColor = true;
			this.RadioVWrap.Click += new System.EventHandler(this.RadioVWrapping_Click);
			// 
			// RadioVMirror
			// 
			this.RadioVMirror.AutoSize = true;
			this.RadioVMirror.Location = new System.Drawing.Point(6, 70);
			this.RadioVMirror.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioVMirror.Name = "RadioVMirror";
			this.RadioVMirror.Size = new System.Drawing.Size(58, 19);
			this.RadioVMirror.TabIndex = 2;
			this.RadioVMirror.Text = "Mirror";
			this.RadioVMirror.UseVisualStyleBackColor = true;
			this.RadioVMirror.Click += new System.EventHandler(this.RadioVWrapping_Click);
			// 
			// RadioVMirrorOnce
			// 
			this.RadioVMirrorOnce.AutoSize = true;
			this.RadioVMirrorOnce.Location = new System.Drawing.Point(6, 95);
			this.RadioVMirrorOnce.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioVMirrorOnce.Name = "RadioVMirrorOnce";
			this.RadioVMirrorOnce.Size = new System.Drawing.Size(87, 19);
			this.RadioVMirrorOnce.TabIndex = 3;
			this.RadioVMirrorOnce.Text = "Mirror once";
			this.RadioVMirrorOnce.UseVisualStyleBackColor = true;
			this.RadioVMirrorOnce.Click += new System.EventHandler(this.RadioVWrapping_Click);
			// 
			// RadioVBorder
			// 
			this.RadioVBorder.AutoSize = true;
			this.RadioVBorder.Location = new System.Drawing.Point(6, 120);
			this.RadioVBorder.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
			this.RadioVBorder.Name = "RadioVBorder";
			this.RadioVBorder.Size = new System.Drawing.Size(60, 19);
			this.RadioVBorder.TabIndex = 4;
			this.RadioVBorder.Text = "Border";
			this.RadioVBorder.UseVisualStyleBackColor = true;
			this.RadioVBorder.Click += new System.EventHandler(this.RadioVWrapping_Click);
			// 
			// LabelBorderColor
			// 
			this.LabelBorderColor.AutoSize = true;
			this.LabelBorderColor.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F);
			this.LabelBorderColor.Location = new System.Drawing.Point(3, 164);
			this.LabelBorderColor.Name = "LabelBorderColor";
			this.LabelBorderColor.Size = new System.Drawing.Size(89, 17);
			this.LabelBorderColor.TabIndex = 13;
			this.LabelBorderColor.Text = "Border Color:";
			this.LabelBorderColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.LabelBorderColor.Visible = false;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel2.Controls.Add(this.TableHWrapping, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.LabelBorderColor, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.Picker, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.TableVWrapping, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 6;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(364, 486);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// TableVWrapping
			// 
			this.TableVWrapping.AutoSize = true;
			this.TableVWrapping.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.TableVWrapping.ColumnCount = 1;
			this.TableVWrapping.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableVWrapping.Controls.Add(this.label2, 0, 0);
			this.TableVWrapping.Controls.Add(this.RadioVBorder, 0, 5);
			this.TableVWrapping.Controls.Add(this.RadioVWrap, 0, 2);
			this.TableVWrapping.Controls.Add(this.RadioVClamp, 0, 1);
			this.TableVWrapping.Controls.Add(this.RadioVMirrorOnce, 0, 4);
			this.TableVWrapping.Controls.Add(this.RadioVMirror, 0, 3);
			this.TableVWrapping.Location = new System.Drawing.Point(155, 3);
			this.TableVWrapping.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
			this.TableVWrapping.Name = "TableVWrapping";
			this.TableVWrapping.RowCount = 6;
			this.TableVWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableVWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableVWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableVWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableVWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableVWrapping.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.TableVWrapping.Size = new System.Drawing.Size(124, 142);
			this.TableVWrapping.TabIndex = 1;
			// 
			// SpriteWrap
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "SpriteWrap";
			this.Size = new System.Drawing.Size(364, 543);
			this.Text = "Sprite Texture Wrapping";
			this.PanelBody.ResumeLayout(false);
			this.PanelBody.PerformLayout();
			this.TableHWrapping.ResumeLayout(false);
			this.TableHWrapping.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.TableVWrapping.ResumeLayout(false);
			this.TableVWrapping.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private UI.Controls.ColorPicker Picker;
        private System.Windows.Forms.TableLayoutPanel TableHWrapping;
        private System.Windows.Forms.Label LabelBorderColor;
        private System.Windows.Forms.RadioButton RadioHBorder;
        private System.Windows.Forms.RadioButton RadioHMirrorOnce;
        private System.Windows.Forms.RadioButton RadioHMirror;
        private System.Windows.Forms.RadioButton RadioHWrap;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton RadioHClamp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton RadioVClamp;
        private System.Windows.Forms.RadioButton RadioVWrap;
        private System.Windows.Forms.RadioButton RadioVMirror;
        private System.Windows.Forms.RadioButton RadioVMirrorOnce;
        private System.Windows.Forms.RadioButton RadioVBorder;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel TableVWrapping;
    }
}
