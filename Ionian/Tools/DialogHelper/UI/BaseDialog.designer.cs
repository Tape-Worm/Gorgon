#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, April 19, 2008 12:18:06 PM
// 
#endregion

namespace Dialogs
{
    partial class BaseDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseDialog));
			this.imageIcon = new System.Windows.Forms.PictureBox();
			this.OKButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// imageIcon
			// 
			this.imageIcon.Image = global::Dialogs.Properties.Resources.default_dialog_icon;
			this.imageIcon.Location = new System.Drawing.Point(15, 7);
			this.imageIcon.Margin = new System.Windows.Forms.Padding(4);
			this.imageIcon.MaximumSize = new System.Drawing.Size(56, 52);
			this.imageIcon.MinimumSize = new System.Drawing.Size(56, 52);
			this.imageIcon.Name = "imageIcon";
			this.imageIcon.Size = new System.Drawing.Size(56, 52);
			this.imageIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.imageIcon.TabIndex = 11;
			this.imageIcon.TabStop = false;
			this.imageIcon.Visible = false;
			// 
			// OKButton
			// 
			this.OKButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.OKButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
			this.OKButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.OKButton.Image = ((System.Drawing.Image)(resources.GetObject("OKButton.Image")));
			this.OKButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.OKButton.Location = new System.Drawing.Point(155, 96);
			this.OKButton.Margin = new System.Windows.Forms.Padding(4);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(77, 28);
			this.OKButton.TabIndex = 10;
			this.OKButton.Text = "OK";
			this.OKButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// BaseDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(245, 134);
			this.Controls.Add(this.imageIcon);
			this.Controls.Add(this.OKButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(249, 158);
			this.Name = "BaseDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Caption";
			((System.ComponentModel.ISupportInitialize)(this.imageIcon)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// Image used for the dialog icon.
        /// </summary>
        protected System.Windows.Forms.PictureBox imageIcon;
        /// <summary>
        /// OK button.
        /// </summary>
        protected System.Windows.Forms.Button OKButton;
    }
}