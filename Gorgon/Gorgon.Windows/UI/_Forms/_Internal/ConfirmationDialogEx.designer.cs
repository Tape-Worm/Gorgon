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
// Created: Saturday, June 18, 2011 4:21:15 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.UI;

	partial class ConfirmationDialogEx
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmationDialogEx));
			this.checkToAll = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonNo
			// 
			this.buttonNo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonNo.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonNo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			// 
			// buttonCancel
			// 
			this.buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			// 
			// buttonOK
			// 
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			resources.ApplyResources(this.buttonOK, "buttonOK");
			// 
			// checkToAll
			// 
			resources.ApplyResources(this.checkToAll, "checkToAll");
			this.checkToAll.BackColor = System.Drawing.Color.White;
			this.checkToAll.Name = "checkToAll";
			this.checkToAll.UseVisualStyleBackColor = false;
			// 
			// ConfirmationDialogEx
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			resources.ApplyResources(this, "$this");
			this.Controls.Add(this.checkToAll);
			this.Name = "ConfirmationDialogEx";
			this.Controls.SetChildIndex(this.pictureDialog, 0);
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.buttonNo, 0);
			this.Controls.SetChildIndex(this.checkToAll, 0);
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

    #endregion

    private CheckBox checkToAll;
}
