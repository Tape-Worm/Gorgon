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
// Created: Wednesday, February 26, 2014 6:18:59 PM
// 
#endregion

using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.UI;

	partial class WarningDialog
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WarningDialog));
        this.textWarningDetails = new System.Windows.Forms.TextBox();
        this.checkDetail = new System.Windows.Forms.CheckBox();
        ((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).BeginInit();
        this.SuspendLayout();
        // 
        // buttonOK
        // 
        this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
        this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
        resources.ApplyResources(this.buttonOK, "buttonOK");
        this.buttonOK.Click += new System.EventHandler(this.OKButton_Click);
        // 
        // pictureDialog
        // 
        this.pictureDialog.Image = global::Gorgon.Windows.Properties.Resources.Warning_48x48;
        // 
        // textWarningDetails
        // 
        resources.ApplyResources(this.textWarningDetails, "textWarningDetails");
        this.textWarningDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this.textWarningDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.textWarningDetails.Name = "textWarningDetails";
        this.textWarningDetails.ReadOnly = true;
        // 
        // checkDetail
        // 
        resources.ApplyResources(this.checkDetail, "checkDetail");
        this.checkDetail.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        this.checkDetail.FlatAppearance.BorderSize = 2;
        this.checkDetail.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this.checkDetail.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this.checkDetail.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
        this.checkDetail.Name = "checkDetail";
        this.checkDetail.Click += new System.EventHandler(this.DetailsButton_Click);
        // 
        // WarningDialog
        // 
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.checkDetail);
        this.Controls.Add(this.textWarningDetails);
        this.DialogImage = global::Gorgon.Windows.Properties.Resources.Warning_48x48;
        this.Name = "WarningDialog";
        this.Controls.SetChildIndex(this.pictureDialog, 0);
        this.Controls.SetChildIndex(this.textWarningDetails, 0);
        this.Controls.SetChildIndex(this.buttonOK, 0);
        this.Controls.SetChildIndex(this.checkDetail, 0);
        ((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

		}

		#endregion

		private TextBox textWarningDetails;
		private CheckBox checkDetail;
	}