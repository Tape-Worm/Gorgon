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
// Created: Saturday, June 18, 2011 4:21:42 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace GorgonLibrary.UI
{
	partial class ErrorDialog
        : BaseDialog 
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorDialog));
			this.errorDetails = new System.Windows.Forms.TextBox();
			this.checkDetail = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
			this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
			this.buttonOK.Location = new System.Drawing.Point(180, 78);
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// pictureDialog
			// 
			this.pictureDialog.Image = ((System.Drawing.Image)(resources.GetObject("pictureDialog.Image")));
			// 
			// errorDetails
			// 
			this.errorDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.errorDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.errorDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.errorDetails.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.errorDetails.Location = new System.Drawing.Point(8, 112);
			this.errorDetails.MaxLength = 2097152;
			this.errorDetails.Multiline = true;
			this.errorDetails.Name = "errorDetails";
			this.errorDetails.ReadOnly = true;
			this.errorDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.errorDetails.Size = new System.Drawing.Size(223, 192);
			this.errorDetails.TabIndex = 4;
			this.errorDetails.Visible = false;
			this.errorDetails.WordWrap = false;
			// 
			// checkDetail
			// 
			this.checkDetail.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkDetail.AutoSize = true;
			this.checkDetail.FlatAppearance.BorderColor = System.Drawing.Color.Black;
			this.checkDetail.FlatAppearance.BorderSize = 2;
			this.checkDetail.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.checkDetail.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.checkDetail.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.checkDetail.Image = ((System.Drawing.Image)(resources.GetObject("checkDetail.Image")));
			this.checkDetail.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDetail.Location = new System.Drawing.Point(12, 78);
			this.checkDetail.Name = "checkDetail";
			this.checkDetail.Size = new System.Drawing.Size(81, 30);
			this.checkDetail.TabIndex = 1;
			this.checkDetail.Text = global::Gorgon.Core.Properties.Resources.GOR_DLG_DETAILS;
			this.checkDetail.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkDetail.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.checkDetail.Click += new System.EventHandler(this.detailsButton_Click);
			// 
			// ErrorDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(243, 130);
			this.Controls.Add(this.checkDetail);
			this.Controls.Add(this.errorDetails);
			this.DialogImage = ((System.Drawing.Image)(resources.GetObject("$this.DialogImage")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ErrorDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = @"Error";
			this.Controls.SetChildIndex(this.pictureDialog, 0);
			this.Controls.SetChildIndex(this.errorDetails, 0);
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.checkDetail, 0);
			((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private TextBox errorDetails;
		private CheckBox checkDetail;
	}
}