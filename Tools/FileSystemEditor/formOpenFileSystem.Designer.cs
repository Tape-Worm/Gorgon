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
// Created: Tuesday, April 24, 2007 2:32:28 AM
// 
#endregion

namespace GorgonLibrary.FileSystems.Tools
{
	partial class formOpenFileSystem
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formOpenFileSystem));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonSelectRoot = new System.Windows.Forms.Button();
			this.labelRoot = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.comboFileSystemType = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.dialogOpen = new System.Windows.Forms.OpenFileDialog();
			this.dialogFolders = new System.Windows.Forms.FolderBrowserDialog();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(335, 98);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 24);
			this.buttonCancel.TabIndex = 12;
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOK
			// 
			this.buttonOK.Enabled = false;
			this.buttonOK.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(305, 98);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 24);
			this.buttonOK.TabIndex = 10;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonSelectRoot
			// 
			this.buttonSelectRoot.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_cubes;
			this.buttonSelectRoot.Location = new System.Drawing.Point(335, 68);
			this.buttonSelectRoot.Name = "buttonSelectRoot";
			this.buttonSelectRoot.Size = new System.Drawing.Size(24, 24);
			this.buttonSelectRoot.TabIndex = 9;
			this.buttonSelectRoot.UseVisualStyleBackColor = true;
			this.buttonSelectRoot.Click += new System.EventHandler(this.buttonSelectRoot_Click);
			// 
			// labelRoot
			// 
			this.labelRoot.AutoEllipsis = true;
			this.labelRoot.BackColor = System.Drawing.SystemColors.Window;
			this.labelRoot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelRoot.Location = new System.Drawing.Point(12, 69);
			this.labelRoot.Name = "labelRoot";
			this.labelRoot.Size = new System.Drawing.Size(320, 20);
			this.labelRoot.TabIndex = 8;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 52);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(89, 13);
			this.label3.TabIndex = 11;
			this.label3.Text = "File System Root:";
			// 
			// comboFileSystemType
			// 
			this.comboFileSystemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboFileSystemType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.comboFileSystemType.FormattingEnabled = true;
			this.comboFileSystemType.Location = new System.Drawing.Point(12, 24);
			this.comboFileSystemType.Name = "comboFileSystemType";
			this.comboFileSystemType.Size = new System.Drawing.Size(346, 21);
			this.comboFileSystemType.TabIndex = 6;
			this.comboFileSystemType.SelectedIndexChanged += new System.EventHandler(this.comboFileSystemType_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 7);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(90, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "File System Type:";
			// 
			// dialogOpen
			// 
			this.dialogOpen.Filter = "Gorgon pack files (*.gorPack)|*.gorPack|All files (*.*)|*.*";
			this.dialogOpen.InitialDirectory = ".\\";
			// 
			// dialogFolders
			// 
			this.dialogFolders.SelectedPath = ".\\";
			this.dialogFolders.ShowNewFolderButton = false;
			// 
			// formOpenFileSystem
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(370, 128);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonSelectRoot);
			this.Controls.Add(this.labelRoot);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.comboFileSystemType);
			this.Controls.Add(this.label2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formOpenFileSystem";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Open File System";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formOpenFileSystem_FormClosing);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formOpenFileSystem_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonSelectRoot;
		private System.Windows.Forms.Label labelRoot;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox comboFileSystemType;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.OpenFileDialog dialogOpen;
		private System.Windows.Forms.FolderBrowserDialog dialogFolders;
	}
}