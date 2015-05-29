#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, January 18, 2015 11:07:17 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.Editor.SpriteEditorPlugIn
{
	partial class FormNewSprite
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNewSprite));
			this.textName = new System.Windows.Forms.TextBox();
			this.labelName = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.labelTexture = new System.Windows.Forms.Label();
			this.labelTexturePath = new System.Windows.Forms.Label();
			this.buttonSelectTexture = new System.Windows.Forms.Button();
			this.tipInfo = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// textName
			// 
			this.textName.BackColor = System.Drawing.Color.White;
			this.textName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textName.Location = new System.Drawing.Point(7, 46);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(376, 23);
			this.textName.TabIndex = 0;
			this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
			// 
			// labelName
			// 
			this.labelName.AutoSize = true;
			this.labelName.ForeColor = System.Drawing.Color.White;
			this.labelName.Location = new System.Drawing.Point(4, 28);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(107, 15);
			this.labelName.TabIndex = 11;
			this.labelName.Text = "name not localized";
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
			this.buttonCancel.ForeColor = System.Drawing.Color.White;
			this.buttonCancel.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(300, 120);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(87, 28);
			this.buttonCancel.TabIndex = 4;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.UseVisualStyleBackColor = false;
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonOK.ForeColor = System.Drawing.Color.White;
			this.buttonOK.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.ok_16x16;
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(207, 120);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(87, 28);
			this.buttonOK.TabIndex = 3;
			this.buttonOK.Text = "OK";
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.UseVisualStyleBackColor = false;
			// 
			// labelTexture
			// 
			this.labelTexture.AutoSize = true;
			this.labelTexture.ForeColor = System.Drawing.Color.White;
			this.labelTexture.Location = new System.Drawing.Point(4, 72);
			this.labelTexture.Name = "labelTexture";
			this.labelTexture.Size = new System.Drawing.Size(113, 15);
			this.labelTexture.TabIndex = 16;
			this.labelTexture.Text = "texture not localized";
			// 
			// labelTexturePath
			// 
			this.labelTexturePath.BackColor = System.Drawing.Color.White;
			this.labelTexturePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelTexturePath.ForeColor = System.Drawing.Color.Black;
			this.labelTexturePath.Location = new System.Drawing.Point(7, 87);
			this.labelTexturePath.Name = "labelTexturePath";
			this.labelTexturePath.Size = new System.Drawing.Size(337, 23);
			this.labelTexturePath.TabIndex = 1;
			// 
			// buttonSelectTexture
			// 
			this.buttonSelectTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSelectTexture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonSelectTexture.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonSelectTexture.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
			this.buttonSelectTexture.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(189)))), ((int)(((byte)(189)))));
			this.buttonSelectTexture.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.buttonSelectTexture.ForeColor = System.Drawing.Color.White;
			this.buttonSelectTexture.Image = global::Gorgon.Editor.SpriteEditorPlugIn.Properties.Resources.open_image_16x16;
			this.buttonSelectTexture.Location = new System.Drawing.Point(350, 87);
			this.buttonSelectTexture.Name = "buttonSelectTexture";
			this.buttonSelectTexture.Size = new System.Drawing.Size(33, 23);
			this.buttonSelectTexture.TabIndex = 2;
			this.buttonSelectTexture.UseVisualStyleBackColor = false;
			this.buttonSelectTexture.Click += new System.EventHandler(this.buttonSelectTexture_Click);
			// 
			// tipInfo
			// 
			this.tipInfo.AutoPopDelay = 5000;
			this.tipInfo.BackColor = System.Drawing.Color.White;
			this.tipInfo.ForeColor = System.Drawing.Color.Black;
			this.tipInfo.InitialDelay = 1500;
			this.tipInfo.ReshowDelay = 500;
			this.tipInfo.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.tipInfo.ToolTipTitle = "help not localized";
			// 
			// FormNewSprite
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Border = true;
			this.BorderColor = System.Drawing.Color.SteelBlue;
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(393, 152);
			this.Controls.Add(this.buttonSelectTexture);
			this.Controls.Add(this.labelTexturePath);
			this.Controls.Add(this.labelTexture);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.labelName);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.ForeColor = System.Drawing.Color.Silver;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.InactiveBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
			this.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormNewSprite";
			this.Padding = new System.Windows.Forms.Padding(1);
			this.Resizable = false;
			this.ResizeHandleSize = 1;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "new sprite not localized";
			this.Controls.SetChildIndex(this.labelName, 0);
			this.Controls.SetChildIndex(this.textName, 0);
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.labelTexture, 0);
			this.Controls.SetChildIndex(this.labelTexturePath, 0);
			this.Controls.SetChildIndex(this.buttonSelectTexture, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private TextBox textName;
		private Label labelName;
		private Button buttonCancel;
		private Button buttonOK;
		private Label labelTexture;
		private Label labelTexturePath;
		private Button buttonSelectTexture;
		private ToolTip tipInfo;
	}
}