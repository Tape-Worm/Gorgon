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
// Created: Saturday, June 18, 2011 4:20:36 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.UI
{
    partial class BaseDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseDialog));
			this.buttonOK = new System.Windows.Forms.Button();
			this.pictureDialog = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.AutoSize = true;
			this.buttonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.buttonOK.Image = ((System.Drawing.Image)(resources.GetObject("buttonOK.Image")));
			this.buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonOK.Location = new System.Drawing.Point(167, 92);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(4);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(55, 30);
			this.buttonOK.TabIndex = 10;
			this.buttonOK.Text = global::Gorgon.Core.Properties.Resources.GOR_DLG_OK;
			this.buttonOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.buttonOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			// 
			// pictureDialog
			// 
			this.pictureDialog.Location = new System.Drawing.Point(2, 2);
			this.pictureDialog.Name = "pictureDialog";
			this.pictureDialog.Size = new System.Drawing.Size(48, 48);
			this.pictureDialog.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureDialog.TabIndex = 11;
			this.pictureDialog.TabStop = false;
			// 
			// BaseDialog
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(245, 134);
			this.Controls.Add(this.pictureDialog);
			this.Controls.Add(this.buttonOK);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(249, 158);
			this.Name = "BaseDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		/// <summary>
        /// OK button.
        /// </summary>
        protected Button buttonOK;
		protected PictureBox pictureDialog;
    }
}