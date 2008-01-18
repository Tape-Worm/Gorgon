#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Thursday, April 05, 2007 4:48:59 PM
// 
#endregion

namespace GorgonLibrary.FileSystems.Tools
{
	partial class formNewFileSystem
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formNewFileSystem));
			this.label2 = new System.Windows.Forms.Label();
			this.comboFileSystemType = new System.Windows.Forms.ComboBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.dialogFolders = new System.Windows.Forms.FolderBrowserDialog();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(90, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "File System Type:";
			// 
			// comboFileSystemType
			// 
			this.comboFileSystemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFileSystemType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboFileSystemType.FormattingEnabled = true;
			this.comboFileSystemType.Location = new System.Drawing.Point(16, 30);
			this.comboFileSystemType.Name = "comboFileSystemType";
			this.comboFileSystemType.Size = new System.Drawing.Size(346, 21);
			this.comboFileSystemType.TabIndex = 0;
			this.comboFileSystemType.SelectedIndexChanged += new System.EventHandler(this.comboFileSystemType_SelectedIndexChanged);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(338, 57);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 24);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(308, 57);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 24);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// dialogFolders
			// 
			this.dialogFolders.SelectedPath = ".\\";
			// 
			// formNewFileSystem
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(374, 88);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.comboFileSystemType);
			this.Controls.Add(this.label2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formNewFileSystem";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New File System";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formNewFileSystem_FormClosing);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formNewFileSystem_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboFileSystemType;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.FolderBrowserDialog dialogFolders;
	}
}