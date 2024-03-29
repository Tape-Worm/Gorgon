﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Saturday, June 18, 2011 4:20:55 PM
// 


using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.UI;

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



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        var resources = new ComponentResourceManager(typeof(ConfirmationDialog));
        buttonNo = new Button();
        buttonCancel = new Button();
        ((ISupportInitialize)pictureDialog).BeginInit();
        SuspendLayout();
        // 
        // buttonOK
        // 
        buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
        buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        buttonOK.Location = new System.Drawing.Point(12, 78);
        buttonOK.TabIndex = 0;
        buttonOK.Text = "&Yes";
        buttonOK.Click += OKButton_Click;
        // 
        // pictureDialog
        // 
        pictureDialog.Image = Windows.Properties.Resources.Confirm_48x48;
        // 
        // buttonNo
        // 
        buttonNo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonNo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        buttonNo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        buttonNo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        buttonNo.Location = new System.Drawing.Point(74, 78);
        buttonNo.Name = "buttonNo";
        buttonNo.Size = new System.Drawing.Size(69, 25);
        buttonNo.TabIndex = 1;
        buttonNo.Text = "&No";
        buttonNo.Click += ButtonNo_Click;
        // 
        // buttonCancel
        // 
        buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        buttonCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        buttonCancel.Location = new System.Drawing.Point(135, 78);
        buttonCancel.Name = "buttonCancel";
        buttonCancel.Size = new System.Drawing.Size(69, 25);
        buttonCancel.TabIndex = 2;
        buttonCancel.Text = "&Cancel";
        buttonCancel.Visible = false;
        buttonCancel.Click += ButtonCancel_Click;
        // 
        // ConfirmationDialog
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new System.Drawing.Size(303, 130);
        Controls.Add(buttonNo);
        Controls.Add(buttonCancel);
        DialogImage = Windows.Properties.Resources.Confirm_48x48;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        Name = "ConfirmationDialog";
        Text = "Confirmation";
        Controls.SetChildIndex(pictureDialog, 0);
        Controls.SetChildIndex(buttonOK, 0);
        Controls.SetChildIndex(buttonCancel, 0);
        Controls.SetChildIndex(buttonNo, 0);
        ((ISupportInitialize)pictureDialog).EndInit();
        ResumeLayout(false);
    }


    /// <summary>
    /// 
    /// </summary>
    protected Button buttonNo;
        /// <summary>
        /// 
        /// </summary>
        protected Button buttonCancel;
    }
