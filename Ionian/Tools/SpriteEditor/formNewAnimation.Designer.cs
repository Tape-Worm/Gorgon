#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Sunday, July 08, 2007 11:33:14 PM
// 
#endregion

namespace GorgonLibrary.Graphics.Tools
{
	partial class formNewAnimation
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNewAnimation));
			this.label1 = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.numericLength = new System.Windows.Forms.NumericUpDown();
			this.labelUnit = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioMinutes = new System.Windows.Forms.RadioButton();
			this.radioSeconds = new System.Windows.Forms.RadioButton();
			this.radioMilliseconds = new System.Windows.Forms.RadioButton();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.checkLoop = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.numericLength)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name:";
			// 
			// textName
			// 
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textName.Location = new System.Drawing.Point(66, 13);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(206, 20);
			this.textName.TabIndex = 0;
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Length:";
			// 
			// numericLength
			// 
			this.numericLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.numericLength.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numericLength.Location = new System.Drawing.Point(66, 39);
			this.numericLength.Maximum = new decimal(new int[] {
            86400000,
            0,
            0,
            0});
			this.numericLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericLength.Name = "numericLength";
			this.numericLength.Size = new System.Drawing.Size(101, 20);
			this.numericLength.TabIndex = 1;
			this.numericLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.numericLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// labelUnit
			// 
			this.labelUnit.Location = new System.Drawing.Point(173, 41);
			this.labelUnit.Name = "labelUnit";
			this.labelUnit.Size = new System.Drawing.Size(108, 18);
			this.labelUnit.TabIndex = 4;
			this.labelUnit.Text = "milliseconds";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.radioMinutes);
			this.groupBox1.Controls.Add(this.radioSeconds);
			this.groupBox1.Controls.Add(this.radioMilliseconds);
			this.groupBox1.Location = new System.Drawing.Point(3, 65);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(278, 45);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Time units";
			// 
			// radioMinutes
			// 
			this.radioMinutes.AutoSize = true;
			this.radioMinutes.Location = new System.Drawing.Point(170, 19);
			this.radioMinutes.Name = "radioMinutes";
			this.radioMinutes.Size = new System.Drawing.Size(62, 17);
			this.radioMinutes.TabIndex = 2;
			this.radioMinutes.Text = "Minutes";
			this.radioMinutes.UseVisualStyleBackColor = true;
			this.radioMinutes.Click += new System.EventHandler(this.UnitUpdate);
			// 
			// radioSeconds
			// 
			this.radioSeconds.AutoSize = true;
			this.radioSeconds.Location = new System.Drawing.Point(97, 19);
			this.radioSeconds.Name = "radioSeconds";
			this.radioSeconds.Size = new System.Drawing.Size(67, 17);
			this.radioSeconds.TabIndex = 1;
			this.radioSeconds.Text = "Seconds";
			this.radioSeconds.UseVisualStyleBackColor = true;
			this.radioSeconds.Click += new System.EventHandler(this.UnitUpdate);
			// 
			// radioMilliseconds
			// 
			this.radioMilliseconds.AutoSize = true;
			this.radioMilliseconds.Checked = true;
			this.radioMilliseconds.Location = new System.Drawing.Point(9, 19);
			this.radioMilliseconds.Name = "radioMilliseconds";
			this.radioMilliseconds.Size = new System.Drawing.Size(82, 17);
			this.radioMilliseconds.TabIndex = 0;
			this.radioMilliseconds.TabStop = true;
			this.radioMilliseconds.Text = "Milliseconds";
			this.radioMilliseconds.UseVisualStyleBackColor = true;
			this.radioMilliseconds.Click += new System.EventHandler(this.UnitUpdate);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(248, 149);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 23);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.Image = global::GorgonLibrary.Graphics.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(218, 149);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 23);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// checkLoop
			// 
			this.checkLoop.AutoSize = true;
			this.checkLoop.Location = new System.Drawing.Point(12, 117);
			this.checkLoop.Name = "checkLoop";
			this.checkLoop.Size = new System.Drawing.Size(122, 17);
			this.checkLoop.TabIndex = 3;
			this.checkLoop.Text = "Loop the animation?";
			this.checkLoop.UseVisualStyleBackColor = true;
			// 
			// formNewAnimation
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 184);
			this.Controls.Add(this.checkLoop);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelUnit);
			this.Controls.Add(this.numericLength);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formNewAnimation";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Animation";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formNewAnimation_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.numericLength)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericLength;
		private System.Windows.Forms.Label labelUnit;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioMinutes;
		private System.Windows.Forms.RadioButton radioSeconds;
		private System.Windows.Forms.RadioButton radioMilliseconds;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.CheckBox checkLoop;
	}
}