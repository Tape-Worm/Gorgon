namespace Gorgon.Editor.ExtractSpriteTool
{
    partial class FormColorPicker
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormColorPicker));
			this.Picker = new Gorgon.Editor.UI.Controls.ColorPicker();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.TableDialogControls = new System.Windows.Forms.TableLayoutPanel();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOk = new System.Windows.Forms.Button();
			this.tableLayoutPanel1.SuspendLayout();
			this.TableDialogControls.SuspendLayout();
			this.SuspendLayout();
			// 
			// Picker
			// 
			this.Picker.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Picker.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Picker.Location = new System.Drawing.Point(3, 3);
			this.Picker.Name = "Picker";
			this.Picker.Size = new System.Drawing.Size(394, 282);
			this.Picker.TabIndex = 0;
			this.Picker.ColorChanged += new System.EventHandler<Gorgon.Editor.UI.Controls.ColorChangedEventArgs>(this.Picker_ColorChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.Picker, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.TableDialogControls, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 327);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// TableDialogControls
			// 
			this.TableDialogControls.AutoSize = true;
			this.TableDialogControls.ColumnCount = 2;
			this.TableDialogControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableDialogControls.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.TableDialogControls.Controls.Add(this.ButtonCancel, 0, 0);
			this.TableDialogControls.Controls.Add(this.ButtonOk, 0, 0);
			this.TableDialogControls.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TableDialogControls.Location = new System.Drawing.Point(3, 291);
			this.TableDialogControls.Name = "TableDialogControls";
			this.TableDialogControls.RowCount = 1;
			this.TableDialogControls.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TableDialogControls.Size = new System.Drawing.Size(394, 33);
			this.TableDialogControls.TabIndex = 1;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.AutoSize = true;
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonCancel.Location = new System.Drawing.Point(300, 3);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(91, 27);
			this.ButtonCancel.TabIndex = 1;
			this.ButtonCancel.Text = "&Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOk
			// 
			this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOk.AutoSize = true;
			this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ButtonOk.Enabled = false;
			this.ButtonOk.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.ButtonOk.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
			this.ButtonOk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.ButtonOk.Location = new System.Drawing.Point(203, 3);
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.Size = new System.Drawing.Size(91, 27);
			this.ButtonOk.TabIndex = 0;
			this.ButtonOk.Text = "&OK";
			this.ButtonOk.UseVisualStyleBackColor = true;
			// 
			// FormColorPicker
			// 
			this.AcceptButton = this.ButtonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(400, 327);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormColorPicker";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Color Picker";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.TableDialogControls.ResumeLayout(false);
			this.TableDialogControls.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private UI.Controls.ColorPicker Picker;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel TableDialogControls;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
    }
}