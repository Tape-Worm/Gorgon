namespace GorgonLibrary.Editor
{
	partial class ValueComponentDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValueComponentDialog));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.labelValueCaption = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.labelComma1 = new System.Windows.Forms.Label();
			this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
			this.labelComma2 = new System.Windows.Forms.Label();
			this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
			this.labelComma3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonCancel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::GorgonLibrary.Editor.Properties.APIResources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(283, 88);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::GorgonLibrary.Editor.Properties.APIResources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(190, 88);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "ok";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			// 
			// labelValueCaption
			// 
			this.labelValueCaption.AutoSize = true;
			this.labelValueCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelValueCaption.ForeColor = System.Drawing.Color.White;
			this.labelValueCaption.Location = new System.Drawing.Point(13, 33);
			this.labelValueCaption.Name = "labelValueCaption";
			this.labelValueCaption.Size = new System.Drawing.Size(35, 15);
			this.labelValueCaption.TabIndex = 18;
			this.labelValueCaption.Text = "value";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numericUpDown1.Location = new System.Drawing.Point(15, 50);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(72, 23);
			this.numericUpDown1.TabIndex = 0;
			this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			this.numericUpDown1.Enter += new System.EventHandler(this.numericUpDown1_Enter);
			// 
			// labelComma1
			// 
			this.labelComma1.AutoSize = true;
			this.labelComma1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelComma1.ForeColor = System.Drawing.Color.White;
			this.labelComma1.Location = new System.Drawing.Point(94, 59);
			this.labelComma1.Name = "labelComma1";
			this.labelComma1.Size = new System.Drawing.Size(10, 15);
			this.labelComma1.TabIndex = 20;
			this.labelComma1.Text = ",";
			// 
			// numericUpDown2
			// 
			this.numericUpDown2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numericUpDown2.Location = new System.Drawing.Point(109, 50);
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new System.Drawing.Size(72, 23);
			this.numericUpDown2.TabIndex = 1;
			this.numericUpDown2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDown2.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			this.numericUpDown2.Enter += new System.EventHandler(this.numericUpDown1_Enter);
			// 
			// numericUpDown3
			// 
			this.numericUpDown3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numericUpDown3.Location = new System.Drawing.Point(203, 50);
			this.numericUpDown3.Name = "numericUpDown3";
			this.numericUpDown3.Size = new System.Drawing.Size(72, 23);
			this.numericUpDown3.TabIndex = 2;
			this.numericUpDown3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDown3.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			this.numericUpDown3.Enter += new System.EventHandler(this.numericUpDown1_Enter);
			// 
			// labelComma2
			// 
			this.labelComma2.AutoSize = true;
			this.labelComma2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelComma2.ForeColor = System.Drawing.Color.White;
			this.labelComma2.Location = new System.Drawing.Point(188, 59);
			this.labelComma2.Name = "labelComma2";
			this.labelComma2.Size = new System.Drawing.Size(10, 15);
			this.labelComma2.TabIndex = 22;
			this.labelComma2.Text = ",";
			// 
			// numericUpDown4
			// 
			this.numericUpDown4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericUpDown4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.numericUpDown4.Location = new System.Drawing.Point(297, 50);
			this.numericUpDown4.Name = "numericUpDown4";
			this.numericUpDown4.Size = new System.Drawing.Size(72, 23);
			this.numericUpDown4.TabIndex = 3;
			this.numericUpDown4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericUpDown4.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			this.numericUpDown4.Enter += new System.EventHandler(this.numericUpDown1_Enter);
			// 
			// labelComma3
			// 
			this.labelComma3.AutoSize = true;
			this.labelComma3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelComma3.ForeColor = System.Drawing.Color.White;
			this.labelComma3.Location = new System.Drawing.Point(282, 59);
			this.labelComma3.Name = "labelComma3";
			this.labelComma3.Size = new System.Drawing.Size(10, 15);
			this.labelComma3.TabIndex = 24;
			this.labelComma3.Text = ",";
			// 
			// ValueComponentDialog
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(383, 129);
			this.Controls.Add(this.numericUpDown4);
			this.Controls.Add(this.labelComma3);
			this.Controls.Add(this.numericUpDown3);
			this.Controls.Add(this.labelComma2);
			this.Controls.Add(this.numericUpDown2);
			this.Controls.Add(this.labelComma1);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.labelValueCaption);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Margin = new System.Windows.Forms.Padding(5, 8, 5, 8);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ValueComponentDialog";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.labelValueCaption, 0);
			this.Controls.SetChildIndex(this.numericUpDown1, 0);
			this.Controls.SetChildIndex(this.labelComma1, 0);
			this.Controls.SetChildIndex(this.numericUpDown2, 0);
			this.Controls.SetChildIndex(this.labelComma2, 0);
			this.Controls.SetChildIndex(this.numericUpDown3, 0);
			this.Controls.SetChildIndex(this.labelComma3, 0);
			this.Controls.SetChildIndex(this.numericUpDown4, 0);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Label labelValueCaption;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.Label labelComma1;
		private System.Windows.Forms.NumericUpDown numericUpDown2;
		private System.Windows.Forms.NumericUpDown numericUpDown3;
		private System.Windows.Forms.Label labelComma2;
		private System.Windows.Forms.NumericUpDown numericUpDown4;
		private System.Windows.Forms.Label labelComma3;
	}
}