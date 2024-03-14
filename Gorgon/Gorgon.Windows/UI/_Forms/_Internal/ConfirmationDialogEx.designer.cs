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
        checkToAll = new CheckBox();
        ((ISupportInitialize)pictureDialog).BeginInit();
        SuspendLayout();
        // 
        // buttonNo
        // 
        buttonNo.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonNo.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
        buttonNo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        // 
        // buttonCancel
        // 
        buttonCancel.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
        buttonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        // 
        // buttonOK
        // 
        buttonOK.FlatAppearance.BorderColor = System.Drawing.Color.Black;
        buttonOK.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ControlDark;
        buttonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(224, 224, 224);
        buttonOK.Location = new System.Drawing.Point(11, 78);
        // 
        // checkToAll
        // 
        checkToAll.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        checkToAll.AutoSize = true;
        checkToAll.BackColor = System.Drawing.Color.White;
        checkToAll.Font = new System.Drawing.Font("Segoe UI", 9F);
        checkToAll.ImeMode = ImeMode.NoControl;
        checkToAll.Location = new System.Drawing.Point(3, 131);
        checkToAll.Name = "checkToAll";
        checkToAll.Padding = new Padding(3, 0, 0, 0);
        checkToAll.Size = new System.Drawing.Size(61, 19);
        checkToAll.TabIndex = 13;
        checkToAll.Text = "&To all?";
        checkToAll.UseVisualStyleBackColor = false;
        // 
        // ConfirmationDialogEx
        // 
        AutoScaleMode = AutoScaleMode.Inherit;
        ClientSize = new System.Drawing.Size(303, 150);
        Controls.Add(checkToAll);
        Location = new System.Drawing.Point(0, 0);
        Name = "ConfirmationDialogEx";
        Controls.SetChildIndex(pictureDialog, 0);
        Controls.SetChildIndex(buttonOK, 0);
        Controls.SetChildIndex(buttonCancel, 0);
        Controls.SetChildIndex(buttonNo, 0);
        Controls.SetChildIndex(checkToAll, 0);
        ((ISupportInitialize)pictureDialog).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private CheckBox checkToAll;
}
