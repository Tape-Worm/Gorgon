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
        this.buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        this.buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
        this.buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
        resources.ApplyResources(this.buttonOK, "buttonOK");
        this.buttonOK.Name = "buttonOK";
        // 
        // pictureDialog
        // 
        resources.ApplyResources(this.pictureDialog, "pictureDialog");
        this.pictureDialog.Name = "pictureDialog";
        this.pictureDialog.TabStop = false;
        // 
        // BaseDialog
        // 
        this.AcceptButton = this.buttonOK;
        resources.ApplyResources(this, "$this");
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.Controls.Add(this.pictureDialog);
        this.Controls.Add(this.buttonOK);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.KeyPreview = true;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "BaseDialog";
        ((System.ComponentModel.ISupportInitialize)(this.pictureDialog)).EndInit();
        this.ResumeLayout(false);

    }

    #endregion

		/// <summary>
    /// OK button.
    /// </summary>
    protected Button buttonOK;
		protected PictureBox pictureDialog;
}