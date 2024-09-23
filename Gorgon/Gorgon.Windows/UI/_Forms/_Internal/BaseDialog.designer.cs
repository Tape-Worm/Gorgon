
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
// Created: Saturday, June 18, 2011 4:20:36 PM
// 


using System.ComponentModel;
using System.Windows.Forms;

namespace Gorgon.UI;

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



    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        var resources = new ComponentResourceManager(typeof(BaseDialog));
        buttonOK = new Button();
        pictureDialog = new PictureBox();
        ((ISupportInitialize)pictureDialog).BeginInit();
        SuspendLayout();
        // 
        // buttonOK
        // 
        buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
        buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
        buttonOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
        buttonOK.Location = new System.Drawing.Point(141, 92);
        buttonOK.Margin = new Padding(4);
        buttonOK.Name = "buttonOK";
        buttonOK.Size = new System.Drawing.Size(69, 25);
        buttonOK.TabIndex = 10;
        buttonOK.Text = "&OK";
        // 
        // pictureDialog
        // 
        pictureDialog.Location = new System.Drawing.Point(0, 0);
        pictureDialog.Name = "pictureDialog";
        pictureDialog.Padding = new Padding(2);
        pictureDialog.Size = new System.Drawing.Size(48, 48);
        pictureDialog.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureDialog.TabIndex = 11;
        pictureDialog.TabStop = false;
        // 
        // BaseDialog
        // 
        AcceptButton = buttonOK;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(304, 134);
        Controls.Add(pictureDialog);
        Controls.Add(buttonOK);
        Font = new System.Drawing.Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
        KeyPreview = true;
        Margin = new Padding(4);
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new System.Drawing.Size(319, 158);
        Name = "BaseDialog";
        StartPosition = FormStartPosition.CenterScreen;
        ((ISupportInitialize)pictureDialog).EndInit();
        ResumeLayout(false);
    }



    /// <summary>
    /// OK button.
    /// </summary>
    protected Button buttonOK;
        protected PictureBox pictureDialog;
}
