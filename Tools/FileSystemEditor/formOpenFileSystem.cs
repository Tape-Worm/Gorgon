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
// Created: Thursday, April 05, 2007 4:48:26 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	/// Form used to open a file system.
	/// </summary>
	public partial class formOpenFileSystem 
		: Form
	{
		#region Variables.
		private FileSystemProvider _lastUsed;									// Last used file system.
		private string _rootPath = string.Empty;							// Root path.
		private string _lastPath = string.Empty;							// Last open path.
		private FileSystem _newFS = null;									// New file system.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the full file system root path.
		/// </summary>
		public string FileSystemRootPath
		{
			get
			{
				return labelRoot.Text;
			}
			set
			{
				labelRoot.Text = value;				
				ValidateForm();
				buttonSelectRoot_Click(this, EventArgs.Empty);
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

			if (labelRoot.Text != string.Empty)
				buttonOK.Enabled = true;
		}

		/// <summary>
		/// Function to refresh the combo box list.
		/// </summary>
		private void RefreshCombo()
		{
			comboFileSystemType.Items.Clear();

			// List each file system.
			foreach (FileSystemProvider fsInfo in FileSystemProviderCache.Providers)
				comboFileSystemType.Items.Add(fsInfo.Name);

			if (comboFileSystemType.Items.Count == 1)
				comboFileSystemType.Enabled = false;

			// Check for the active item.
			if (_lastUsed.Name != string.Empty)
				comboFileSystemType.Text = _lastUsed.Name;
			else
				comboFileSystemType.Text = comboFileSystemType.Items[0].ToString();

			// Get last used.
			_lastUsed = FileSystemProviderCache.Providers[comboFileSystemType.Text];
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			((formMain)Owner).LastUsed = _lastUsed;
			((formMain)Owner).LastOpenPath = _lastPath;
			DialogResult = DialogResult.OK;
		}

		/// <summary>
		/// Handles the Click event of the buttonSelectRoot control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSelectRoot_Click(object sender, EventArgs e)
		{
			DialogResult result;							// Dialog result.
			FileSystemProvider fsInfo = null;					// File system info.
			string fsName = string.Empty;					// File system name.

			try
			{
				if (!FileSystemProviderCache.Providers.Contains(comboFileSystemType.Text))
				{
					UI.ErrorBox(this, "The file system type is not available.");
					return;
				}

				// Get file system information.
				fsInfo = FileSystemProviderCache.Providers[comboFileSystemType.Text];

				// Show the appropriate dialog.
				if (!fsInfo.IsPackedFile)
				{
					dialogFolders.SelectedPath = _lastPath;
					result = dialogFolders.ShowDialog(this);
				}
				else
				{
					dialogOpen.InitialDirectory = _lastPath;
					result = dialogOpen.ShowDialog(this);
				}

				// Get the path.
				if (result == DialogResult.OK)
				{
					if (!fsInfo.IsPackedFile)
					{
						fsName = _rootPath = dialogFolders.SelectedPath;
						if (fsName != string.Empty)
							fsName = fsName.Substring(fsName.LastIndexOf(@"\") + 1);

						if (fsName == string.Empty)
						{
							UI.ErrorBox(this, "You have selected the root of a drive.\nThis is a bad idea and is not allowed.\nFolder file systems must exist within a subdirectory.");
							_rootPath = string.Empty;
							return;
						}
					}
					else
					{
						_rootPath = dialogOpen.FileName;
						fsName = Path.GetFileNameWithoutExtension(_rootPath);
					}

					// Remove an existing file system.
					if (FileSystemCache.FileSystems.Contains(fsName))
					{
						if (UI.ConfirmBox(this, "The file system '" + fsName + "' is already loaded.\nDo you wish to unload the existing file system?") == ConfirmationResult.Yes)
						{
							for (int i = 0; i < Owner.MdiChildren.Length; i++)
							{
								if (((formFileSystemWindow)Owner.MdiChildren[i]).FileSystem == FileSystemCache.FileSystems[fsName])
									Owner.MdiChildren[i].Close();
							}
						}
						else
						{
							_rootPath = string.Empty;
							return;
						}
					}


					// If we're dealing with the folder file system, then create and add.
					_newFS = FileSystem.Create(fsName, fsInfo);

					_newFS.Root = _rootPath;
					if (!fsInfo.IsPackedFile)
						_lastPath = _rootPath;
					else
						_lastPath = Path.GetDirectoryName(_rootPath);
					labelRoot.Text = _rootPath;
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
			_lastUsed = FileSystemProviderCache.Providers[comboFileSystemType.Text];

			ValidateForm();
		}

		/// <summary>
		/// Handles the FormClosing event of the formOpenFileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
		private void formOpenFileSystem_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Clean up previous file system.
			if ((DialogResult != DialogResult.OK) && (_newFS != null))
			{
				_newFS.Dispose();
				_newFS = null;
			}			
		}

		/// <summary>
		/// Handles the KeyDown event of the formOpenFileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formOpenFileSystem_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				e.Handled = true;
			}
			else
				e.Handled = false;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			_lastUsed = ((formMain)Owner).LastUsed;
			_lastPath = ((formMain)Owner).LastOpenPath;

			RefreshCombo();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formOpenFileSystem()
		{
			InitializeComponent();
		}
		#endregion
	}
}