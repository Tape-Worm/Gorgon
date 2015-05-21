#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, June 18, 2011 4:20:55 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace GorgonLibrary.UI
{
	partial class ConfirmationDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmationDialog));
			this.buttonNo = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.buttonOK.Location = new System.Drawing.Point(12, 78);
			this.buttonOK.Size = new System.Drawing.Size(55, 30);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = global::Gorgon.Core.Properties.Resources.GOR_DLG_YES;
			this.buttonOK.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// pictureDialog
			// 
			this.pictureDialog.Image = global::Gorgon.Core.Properties.Resources.Confirm_48x48;
			// 
			// buttonNo
			// 
			this.buttonNo.AutoSize = true;
			this.buttonNo.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.buttonNo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonNo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.buttonNo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.buttonNo.Image = ((System.Drawing.Image)(resources.GetObject("buttonNo.Image")));
			this.buttonNo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonNo.Location = new System.Drawing.Point(74, 78);
			this.buttonNo.Name = "buttonNo";
			this.buttonNo.Size = new System.Drawing.Size(55, 30);
			this.buttonNo.TabIndex = 1;
			this.buttonNo.Text = global::Gorgon.Core.Properties.Resources.GOR_DLG_NO;
			this.buttonNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonNo.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.buttonNo.Click += new System.EventHandler(this.buttonNo_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.AutoSize = true;
			this.buttonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.buttonCancel.Image = global::Gorgon.Core.Properties.Resources.cancel_16x16;
			this.buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonCancel.Location = new System.Drawing.Point(135, 78);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(79, 30);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = global::Gorgon.Core.Properties.Resources.GOR_DLG_CANCEL;
			this.buttonCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.buttonCancel.Visible = false;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// ConfirmationDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(243, 130);
			this.Controls.Add(this.buttonNo);
			this.Controls.Add(this.buttonCancel);
			this.DialogImage = global::Gorgon.Core.Properties.Resources.Confirm_48x48;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ConfirmationDialog";
			this.Controls.SetChildIndex(this.pictureDialog, 0);
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.buttonNo, 0);
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		/// <summary>
		/// 
		/// </summary>
		protected Button buttonNo;
		/// <summary>
		/// 
		/// </summary>
		protected Button buttonCancel;
	}
}
