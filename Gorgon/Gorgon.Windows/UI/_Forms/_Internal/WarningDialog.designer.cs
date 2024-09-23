
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, February 26, 2014 6:18:59 PM
// 


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



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        var resources = new ComponentResourceManager(typeof(WarningDialog));
        textWarningDetails = new TextBox();
        checkDetail = new CheckBox();
        ((ISupportInitialize)pictureDialog).BeginInit();
        SuspendLayout();
        // 
        // buttonOK
        // 
        buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
        buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
        buttonOK.Location = new System.Drawing.Point(180, 78);
        buttonOK.TabIndex = 0;
        buttonOK.Click += OKButton_Click;
        // 
        // pictureDialog
        // 
        pictureDialog.Image = Windows.Properties.Resources.Warning_48x48;
        // 
        // textWarningDetails
        // 
        textWarningDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        textWarningDetails.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        textWarningDetails.BorderStyle = BorderStyle.FixedSingle;
        textWarningDetails.Font = new System.Drawing.Font("Consolas", 9F);
        textWarningDetails.Location = new System.Drawing.Point(8, 112);
        textWarningDetails.MaxLength = 2097152;
        textWarningDetails.Multiline = true;
        textWarningDetails.Name = "textWarningDetails";
        textWarningDetails.ReadOnly = true;
        textWarningDetails.ScrollBars = ScrollBars.Both;
        textWarningDetails.Size = new System.Drawing.Size(283, 192);
        textWarningDetails.TabIndex = 4;
        textWarningDetails.Visible = false;
        textWarningDetails.WordWrap = false;
        // 
        // checkDetail
        // 
        checkDetail.Appearance = Appearance.Button;
        checkDetail.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        checkDetail.FlatAppearance.BorderSize = 2;
        checkDetail.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        checkDetail.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        checkDetail.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        checkDetail.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        checkDetail.Location = new System.Drawing.Point(12, 78);
        checkDetail.Name = "checkDetail";
        checkDetail.Size = new System.Drawing.Size(69, 25);
        checkDetail.TabIndex = 1;
        checkDetail.Text = "&Details";
        checkDetail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        checkDetail.Click += DetailsButton_Click;
        // 
        // WarningDialog
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(303, 130);
        Controls.Add(checkDetail);
        Controls.Add(textWarningDetails);
        DialogImage = Windows.Properties.Resources.Warning_48x48;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Name = "WarningDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Warning";
        Controls.SetChildIndex(pictureDialog, 0);
        Controls.SetChildIndex(textWarningDetails, 0);
        Controls.SetChildIndex(buttonOK, 0);
        Controls.SetChildIndex(checkDetail, 0);
        ((ISupportInitialize)pictureDialog).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }



    private TextBox textWarningDetails;
        private CheckBox checkDetail;
    }
