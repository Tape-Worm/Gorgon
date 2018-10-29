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

namespace Gorgon.Editor.UI
{
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
            this.WorkspaceBrowser = new Gorgon.UI.GorgonFolderBrowser();
            this.panel1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.ButtonCancel = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.ButtonOK = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.panel1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // WorkspaceBrowser
            // 
            this.WorkspaceBrowser.BackColor = System.Drawing.Color.Transparent;
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
            this.WorkspaceBrowser.Size = new System.Drawing.Size(624, 401);
            this.WorkspaceBrowser.TabIndex = 3;
            this.WorkspaceBrowser.FolderSelected += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderSelected);
            this.WorkspaceBrowser.FolderEntered += new System.EventHandler<Gorgon.UI.FolderSelectedArgs>(this.WorkspaceBrowser_FolderEntered);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.ButtonCancel);
            this.panel1.Controls.Add(this.ButtonOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 401);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(624, 40);
            this.panel1.TabIndex = 1;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonCancel.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom2;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(514, 5);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(98, 32);
            this.ButtonCancel.TabIndex = 7;
            this.ButtonCancel.Values.Text = "&Cancel";
            // 
            // ButtonOK
            // 
            this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOK.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Custom2;
            this.ButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOK.Enabled = false;
            this.ButtonOK.Location = new System.Drawing.Point(410, 5);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(98, 32);
            this.ButtonOK.TabIndex = 6;
            this.ButtonOK.Values.Text = "&OK";
            // 
            // FormDirectoryLocator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.WorkspaceBrowser);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormDirectoryLocator";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.panel1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Gorgon.UI.GorgonFolderBrowser WorkspaceBrowser;
        private ComponentFactory.Krypton.Toolkit.KryptonPanel panel1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonCancel;
        private ComponentFactory.Krypton.Toolkit.KryptonButton ButtonOK;
    }
}