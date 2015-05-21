#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Monday, November 17, 2014 9:56:53 PM
// 
#endregion

namespace Gorgon.Editor.SpriteEditorPlugIn.Design
{
	partial class SpriteAnchorEditor
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panelAnchor = new System.Windows.Forms.Panel();
			this.radioBottomRight = new System.Windows.Forms.RadioButton();
			this.radioBottomCenter = new System.Windows.Forms.RadioButton();
			this.radioBottomLeft = new System.Windows.Forms.RadioButton();
			this.radioMiddleRight = new System.Windows.Forms.RadioButton();
			this.radioCenter = new System.Windows.Forms.RadioButton();
			this.radioMiddleLeft = new System.Windows.Forms.RadioButton();
			this.radioTopRight = new System.Windows.Forms.RadioButton();
			this.radioTopCenter = new System.Windows.Forms.RadioButton();
			this.radioTopLeft = new System.Windows.Forms.RadioButton();
			this.panelAnchor.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelAnchor
			// 
			this.panelAnchor.Controls.Add(this.radioBottomRight);
			this.panelAnchor.Controls.Add(this.radioBottomCenter);
			this.panelAnchor.Controls.Add(this.radioBottomLeft);
			this.panelAnchor.Controls.Add(this.radioMiddleRight);
			this.panelAnchor.Controls.Add(this.radioCenter);
			this.panelAnchor.Controls.Add(this.radioMiddleLeft);
			this.panelAnchor.Controls.Add(this.radioTopRight);
			this.panelAnchor.Controls.Add(this.radioTopCenter);
			this.panelAnchor.Controls.Add(this.radioTopLeft);
			this.panelAnchor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelAnchor.Location = new System.Drawing.Point(0, 0);
			this.panelAnchor.Name = "panelAnchor";
			this.panelAnchor.Size = new System.Drawing.Size(123, 114);
			this.panelAnchor.TabIndex = 12;
			// 
			// radioBottomRight
			// 
			this.radioBottomRight.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioBottomRight.BackColor = System.Drawing.SystemColors.Control;
			this.radioBottomRight.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioBottomRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioBottomRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioBottomRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioBottomRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioBottomRight.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_se_16x16;
			this.radioBottomRight.Location = new System.Drawing.Point(82, 76);
			this.radioBottomRight.Name = "radioBottomRight";
			this.radioBottomRight.Size = new System.Drawing.Size(29, 26);
			this.radioBottomRight.TabIndex = 8;
			this.radioBottomRight.UseVisualStyleBackColor = false;
			this.radioBottomRight.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioBottomCenter
			// 
			this.radioBottomCenter.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioBottomCenter.BackColor = System.Drawing.SystemColors.Control;
			this.radioBottomCenter.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioBottomCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioBottomCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioBottomCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioBottomCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioBottomCenter.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_down_16x16;
			this.radioBottomCenter.Location = new System.Drawing.Point(47, 76);
			this.radioBottomCenter.Name = "radioBottomCenter";
			this.radioBottomCenter.Size = new System.Drawing.Size(29, 26);
			this.radioBottomCenter.TabIndex = 7;
			this.radioBottomCenter.UseVisualStyleBackColor = false;
			this.radioBottomCenter.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioBottomLeft
			// 
			this.radioBottomLeft.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioBottomLeft.BackColor = System.Drawing.SystemColors.Control;
			this.radioBottomLeft.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioBottomLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioBottomLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioBottomLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioBottomLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioBottomLeft.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_sw_16x16;
			this.radioBottomLeft.Location = new System.Drawing.Point(12, 76);
			this.radioBottomLeft.Name = "radioBottomLeft";
			this.radioBottomLeft.Size = new System.Drawing.Size(29, 26);
			this.radioBottomLeft.TabIndex = 6;
			this.radioBottomLeft.UseVisualStyleBackColor = false;
			this.radioBottomLeft.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioMiddleRight
			// 
			this.radioMiddleRight.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioMiddleRight.BackColor = System.Drawing.SystemColors.Control;
			this.radioMiddleRight.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioMiddleRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioMiddleRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioMiddleRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioMiddleRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioMiddleRight.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_right_16x16;
			this.radioMiddleRight.Location = new System.Drawing.Point(82, 44);
			this.radioMiddleRight.Name = "radioMiddleRight";
			this.radioMiddleRight.Size = new System.Drawing.Size(29, 26);
			this.radioMiddleRight.TabIndex = 5;
			this.radioMiddleRight.UseVisualStyleBackColor = false;
			this.radioMiddleRight.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioCenter
			// 
			this.radioCenter.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioCenter.BackColor = System.Drawing.SystemColors.Control;
			this.radioCenter.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioCenter.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.center_16x16;
			this.radioCenter.Location = new System.Drawing.Point(47, 44);
			this.radioCenter.Name = "radioCenter";
			this.radioCenter.Size = new System.Drawing.Size(29, 26);
			this.radioCenter.TabIndex = 4;
			this.radioCenter.UseVisualStyleBackColor = false;
			this.radioCenter.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioMiddleLeft
			// 
			this.radioMiddleLeft.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioMiddleLeft.BackColor = System.Drawing.SystemColors.Control;
			this.radioMiddleLeft.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioMiddleLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioMiddleLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioMiddleLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioMiddleLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioMiddleLeft.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_left_16x16;
			this.radioMiddleLeft.Location = new System.Drawing.Point(12, 44);
			this.radioMiddleLeft.Name = "radioMiddleLeft";
			this.radioMiddleLeft.Size = new System.Drawing.Size(29, 26);
			this.radioMiddleLeft.TabIndex = 3;
			this.radioMiddleLeft.UseVisualStyleBackColor = false;
			this.radioMiddleLeft.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioTopRight
			// 
			this.radioTopRight.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioTopRight.BackColor = System.Drawing.SystemColors.Control;
			this.radioTopRight.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioTopRight.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioTopRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioTopRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioTopRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioTopRight.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_ne_16x16;
			this.radioTopRight.Location = new System.Drawing.Point(82, 12);
			this.radioTopRight.Name = "radioTopRight";
			this.radioTopRight.Size = new System.Drawing.Size(29, 26);
			this.radioTopRight.TabIndex = 2;
			this.radioTopRight.UseVisualStyleBackColor = false;
			this.radioTopRight.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioTopCenter
			// 
			this.radioTopCenter.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioTopCenter.BackColor = System.Drawing.SystemColors.Control;
			this.radioTopCenter.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioTopCenter.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioTopCenter.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioTopCenter.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioTopCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioTopCenter.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_up_16x16;
			this.radioTopCenter.Location = new System.Drawing.Point(47, 12);
			this.radioTopCenter.Name = "radioTopCenter";
			this.radioTopCenter.Size = new System.Drawing.Size(29, 26);
			this.radioTopCenter.TabIndex = 1;
			this.radioTopCenter.UseVisualStyleBackColor = false;
			this.radioTopCenter.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// radioTopLeft
			// 
			this.radioTopLeft.Appearance = System.Windows.Forms.Appearance.Button;
			this.radioTopLeft.BackColor = System.Drawing.SystemColors.Control;
			this.radioTopLeft.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.radioTopLeft.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.radioTopLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.radioTopLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
			this.radioTopLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.radioTopLeft.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.arrow_nw_16x16;
			this.radioTopLeft.Location = new System.Drawing.Point(12, 12);
			this.radioTopLeft.Name = "radioTopLeft";
			this.radioTopLeft.Size = new System.Drawing.Size(29, 26);
			this.radioTopLeft.TabIndex = 0;
			this.radioTopLeft.UseVisualStyleBackColor = false;
			this.radioTopLeft.CheckedChanged += new System.EventHandler(this.radioTopLeft_CheckedChanged);
			// 
			// SpriteAnchorEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Controls.Add(this.panelAnchor);
			this.Name = "SpriteAnchorEditor";
			this.Size = new System.Drawing.Size(123, 114);
			this.panelAnchor.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelAnchor;
		private System.Windows.Forms.RadioButton radioBottomRight;
		private System.Windows.Forms.RadioButton radioBottomCenter;
		private System.Windows.Forms.RadioButton radioBottomLeft;
		private System.Windows.Forms.RadioButton radioMiddleRight;
		private System.Windows.Forms.RadioButton radioCenter;
		private System.Windows.Forms.RadioButton radioMiddleLeft;
		private System.Windows.Forms.RadioButton radioTopRight;
		private System.Windows.Forms.RadioButton radioTopCenter;
		private System.Windows.Forms.RadioButton radioTopLeft;
	}
}
