#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: October 1, 2018 7:52:07 PM
// 
#endregion

namespace Gorgon.Editor.UI;

partial class FormDirectoryLocator
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDirectoryLocator));
        this.panel1 = new System.Windows.Forms.Panel();
        this.ButtonCancel = new System.Windows.Forms.Button();
        this.ButtonOK = new System.Windows.Forms.Button();
        this.WorkspaceBrowser = new Gorgon.UI.GorgonFolderBrowser();
        this.panel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // panel1
        // 
        this.panel1.AutoSize = true;
        this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.panel1.Controls.Add(this.ButtonCancel);
        this.panel1.Controls.Add(this.ButtonOK);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.panel1.Location = new System.Drawing.Point(0, 404);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(624, 37);
        this.panel1.TabIndex = 1;
        // 
        // ButtonCancel
        // 
        this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonCancel.AutoSize = true;
        this.ButtonCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ButtonCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonCancel.ForeColor = System.Drawing.Color.White;
        this.ButtonCancel.Location = new System.Drawing.Point(529, 5);
        this.ButtonCancel.Name = "ButtonCancel";
        this.ButtonCancel.Size = new System.Drawing.Size(83, 29);
        this.ButtonCancel.TabIndex = 7;
        this.ButtonCancel.Text = "&Cancel";
        this.ButtonCancel.UseVisualStyleBackColor = false;
        // 
        // ButtonOK
        // 
        this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.ButtonOK.AutoSize = true;
        this.ButtonOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.ButtonOK.Enabled = false;
        this.ButtonOK.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
        this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Orange;
        this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
        this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ButtonOK.ForeColor = System.Drawing.Color.White;
        this.ButtonOK.Location = new System.Drawing.Point(440, 5);
        this.ButtonOK.Name = "ButtonOK";
        this.ButtonOK.Size = new System.Drawing.Size(83, 29);
        this.ButtonOK.TabIndex = 6;
        this.ButtonOK.Text = "&OK";
        this.ButtonOK.UseVisualStyleBackColor = false;
        // 
        // WorkspaceBrowser
        // 
        this.WorkspaceBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(37)))));
        this.WorkspaceBrowser.CaptionFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.WorkspaceBrowser.CdRomImage = global::Gorgon.Editor.Properties.Resources.drive_cdrom_48x48;
        this.WorkspaceBrowser.DirectoryImage = global::Gorgon.Editor.Properties.Resources.folder_48x48;
        this.WorkspaceBrowser.DirectoryListFont = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.WorkspaceBrowser.DirectoryNameFont = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.WorkspaceBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
        this.WorkspaceBrowser.FileImage = global::Gorgon.Editor.Properties.Resources.file_48x48;
        this.WorkspaceBrowser.ForeColor = System.Drawing.Color.White;
        this.WorkspaceBrowser.HardDriveImage = global::Gorgon.Editor.Properties.Resources.drive_48x48;
        this.WorkspaceBrowser.Location = new System.Drawing.Point(0, 0);
        this.WorkspaceBrowser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
        this.WorkspaceBrowser.Name = "WorkspaceBrowser";
        this.WorkspaceBrowser.NetworkDriveImage = global::Gorgon.Editor.Properties.Resources.drive_network_48x48;
        this.WorkspaceBrowser.RamDriveImage = global::Gorgon.Editor.Properties.Resources.drive_ram_48x48;
        this.WorkspaceBrowser.RemovableDriveImage = global::Gorgon.Editor.Properties.Resources.drive_remove_48x48;
        this.WorkspaceBrowser.Size = new System.Drawing.Size(624, 404);
        this.WorkspaceBrowser.TabIndex = 3;
        this.WorkspaceBrowser.FolderSelected += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderSelected);
        this.WorkspaceBrowser.FolderEntered += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderEntered);
        // 
        // FormDirectoryLocator
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(28)))));
        this.CancelButton = this.ButtonCancel;
        this.ClientSize = new System.Drawing.Size(624, 441);
        this.Controls.Add(this.WorkspaceBrowser);
        this.Controls.Add(this.panel1);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.ForeColor = System.Drawing.Color.White;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "FormDirectoryLocator";
        this.ShowIcon = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.panel1.ResumeLayout(false);
        this.panel1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion
    private Gorgon.UI.GorgonFolderBrowser WorkspaceBrowser;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button ButtonCancel;
    private System.Windows.Forms.Button ButtonOK;
}