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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.comboFileSystemType = new System.Windows.Forms.ComboBox();
			this.textFileSystemName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.labelRoot = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonSelectRoot = new System.Windows.Forms.Button();
			this.dialogFolders = new System.Windows.Forms.FolderBrowserDialog();
			this.dialogOpen = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 54);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "File System Name:";
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
			// textFileSystemName
			// 
			this.textFileSystemName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textFileSystemName.Location = new System.Drawing.Point(16, 71);
			this.textFileSystemName.Name = "textFileSystemName";
			this.textFileSystemName.Size = new System.Drawing.Size(346, 20);
			this.textFileSystemName.TabIndex = 1;
			this.textFileSystemName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textFileSystemName_KeyPress);
			this.textFileSystemName.TextChanged += new System.EventHandler(this.textFileSystemName_TextChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 94);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(89, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "File System Root:";
			// 
			// labelRoot
			// 
			this.labelRoot.BackColor = System.Drawing.SystemColors.Window;
			this.labelRoot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelRoot.Location = new System.Drawing.Point(15, 111);
			this.labelRoot.Name = "labelRoot";
			this.labelRoot.Size = new System.Drawing.Size(320, 20);
			this.labelRoot.TabIndex = 2;
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonCancel.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.delete;
			this.buttonCancel.Location = new System.Drawing.Point(338, 140);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(24, 24);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Enabled = false;
			this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonOK.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.check;
			this.buttonOK.Location = new System.Drawing.Point(308, 140);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(24, 24);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonSelectRoot
			// 
			this.buttonSelectRoot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonSelectRoot.Image = global::GorgonLibrary.FileSystems.Tools.Properties.Resources.folder_cubes;
			this.buttonSelectRoot.Location = new System.Drawing.Point(338, 110);
			this.buttonSelectRoot.Name = "buttonSelectRoot";
			this.buttonSelectRoot.Size = new System.Drawing.Size(24, 24);
			this.buttonSelectRoot.TabIndex = 3;
			this.buttonSelectRoot.UseVisualStyleBackColor = true;
			this.buttonSelectRoot.Click += new System.EventHandler(this.buttonSelectRoot_Click);
			// 
			// dialogOpen
			// 
			this.dialogOpen.FileName = "openFileDialog1";
			// 
			// formNewFileSystem
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(374, 171);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonSelectRoot);
			this.Controls.Add(this.labelRoot);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textFileSystemName);
			this.Controls.Add(this.comboFileSystemType);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "formNewFileSystem";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New File System";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formNewFileSystem_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboFileSystemType;
		private System.Windows.Forms.TextBox textFileSystemName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label labelRoot;
		private System.Windows.Forms.Button buttonSelectRoot;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.FolderBrowserDialog dialogFolders;
		private System.Windows.Forms.OpenFileDialog dialogOpen;
	}
}