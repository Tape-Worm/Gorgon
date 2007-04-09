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
// Created: Sunday, April 01, 2007 12:33:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SharpUtilities;
using SharpUtilities.Utility;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form used to start a new file system.
	/// </summary>
	public partial class formNewFileSystem 
		: Form
	{
		#region Variables.
		private FileSystemType _lastUsed;									// Last used file system.
		private string _rootPath = string.Empty;							// Root path.
		private string _invalidChars = string.Empty;						// Invalid character list.
		private FileSystem _newFS = null;									// New file system.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the last used file system.
		/// </summary>
		public FileSystemType LastUsed
		{
			get
			{
				return _lastUsed;
			}
			set
			{
				_lastUsed = value;
			}
		}

		/// <summary>
		/// Property to return the full file system root path.
		/// </summary>
		public string FileSystemRootPath
		{
			get
			{
				return labelRoot.Text;
			}
		}

		/// <summary>
		/// Property to return the selected file system.
		/// </summary>
		public FileSystem ActiveFileSystemType
		{
			get
			{
				return _newFS;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the input on the form.
		/// </summary>
		private void ValidateForm()
		{
			buttonOK.Enabled = false;
			buttonSelectRoot.Enabled = false;

			if (textFileSystemName.Text != string.Empty)
				buttonSelectRoot.Enabled = true;

			if ((textFileSystemName.Text != string.Empty) && (labelRoot.Text != string.Empty))
				buttonOK.Enabled = true;
		}

		/// <summary>
		/// Function to refresh the combo box list.
		/// </summary>
		private void RefreshCombo()
		{
			comboFileSystemType.Items.Clear();

			// List each file system.
			foreach (FileSystemType fsInfo in Gorgon.FileSystems.Types)
				comboFileSystemType.Items.Add(fsInfo.Information.TypeName);

			if (comboFileSystemType.Items.Count == 1)
				comboFileSystemType.Enabled = false;

			// Check for the active item.
			if (_lastUsed.Information.TypeName != string.Empty)
				comboFileSystemType.Text = _lastUsed.Information.TypeName;
			else
				comboFileSystemType.Text = comboFileSystemType.Items[0].ToString();

			// Get last used.
			_lastUsed = Gorgon.FileSystems.Types[comboFileSystemType.Text];
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			RefreshCombo();

			// Get list of invalid characters.
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				if (Convert.ToInt32(invalidChar) > 32)
				{
					if (_invalidChars != string.Empty)
						_invalidChars += ", ";
					_invalidChars += invalidChar.ToString();
				}
			}

			// Set defaults.
			dialogFolders.RootFolder = Environment.SpecialFolder.Desktop;
			dialogFolders.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			dialogOpen.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			dialogOpen.Filter = "Gorgon packed files (*.gorPack)|*.gorPack|All files (*.*)|*.*";
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formNewFileSystem()
		{
			InitializeComponent();
		}
		#endregion

		/// <summary>
		/// Handles the Click event of the buttonCancel control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonCancel_Click(object sender, EventArgs e)
		{
			if (_newFS != null)
				Gorgon.FileSystems.Remove(_newFS);

			DialogResult = DialogResult.Cancel;
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Handles the TextChanged event of the textFileSystemName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textFileSystemName_TextChanged(object sender, EventArgs e)
		{
			string newText = string.Empty;		// New text.

			if (_rootPath != string.Empty)
				labelRoot.Text = _rootPath + @"\" + textFileSystemName.Text;

			ValidateForm();
		}

		/// <summary>
		/// Handles the KeyPress event of the textFileSystemName control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyPressEventArgs"/> instance containing the event data.</param>
		private void textFileSystemName_KeyPress(object sender, KeyPressEventArgs e)
		{			
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				if ((invalidChar == e.KeyChar) && (e.KeyChar != '\b') && (e.KeyChar != '\r'))
				{
					UI.WarningBox(this, e.KeyChar.ToString() + " is an invalid character.\nThe following characters are not allowed:\n" + _invalidChars);
					e.Handled = true;
				}
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonSelectRoot control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSelectRoot_Click(object sender, EventArgs e)
		{
			DialogResult result;							// Dialog result.
			FileSystemType fsInfo = null;					// File system info.

			try
			{
				if (!Gorgon.FileSystems.Types.Contains(comboFileSystemType.Text))
				{
					UI.ErrorBox(this, "The file system type is not available.");
					return;
				}

				// Get file system information.
				fsInfo = Gorgon.FileSystems.Types[comboFileSystemType.Text];

				// Show the appropriate dialog.
				if (!fsInfo.Information.IsPackFile)
					result = dialogFolders.ShowDialog(this);
				else
					result = dialogOpen.ShowDialog(this);

				// Get the path.
				if (result == DialogResult.OK)
				{
					// Remove an existing file system.
					if (Gorgon.FileSystems.Contains(textFileSystemName.Text))
					{
						if (UI.YesNoBox("The file system '" + textFileSystemName.Text + "' is already loaded.\nDo you wish to unload the existing file system?"))
						{
							for (int i = 0; i < Owner.MdiChildren.Length;i++)
							{
								if (((formFileSystemWindow)Owner.MdiChildren[i]).FileSystem == Gorgon.FileSystems[textFileSystemName.Text])
									Owner.MdiChildren[i].Close();
							}
						}
						else
							return;
					}

					if (!fsInfo.Information.IsPackFile)
						_rootPath = dialogFolders.SelectedPath;
					else
						_rootPath = Path.GetDirectoryName(dialogOpen.FileName);

					labelRoot.Text = _rootPath + @"\" + textFileSystemName.Text;

					// Create the file system object.
					if (fsInfo == Gorgon.FileSystems.Types[0])
						_newFS = new FolderFileSystem(textFileSystemName.Text);
					else
						_newFS = Gorgon.FileSystems.Create(textFileSystemName.Text, null);
				}			

				ValidateForm();
			}
			catch (SharpException sEx)
			{
				_newFS = null;
				labelRoot.Text = _rootPath = string.Empty;
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				_newFS = null;
				labelRoot.Text = _rootPath = string.Empty;
				UI.ErrorBox(this, ex);
			}
		}
		
		/// <summary>
		/// Handles the SelectedIndexChanged event of the comboFileSystemType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboFileSystemType_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Get the current file system.
			_lastUsed = Gorgon.FileSystems.Types[comboFileSystemType.Text];

			ValidateForm();
		}

		/// <summary>
		/// Handles the FormClosing event of the formNewFileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
		private void formNewFileSystem_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Clean up previous file system.
			if ((DialogResult != DialogResult.OK) && (_newFS != null))
				Gorgon.FileSystems.Remove(_newFS);
		}
	}
}