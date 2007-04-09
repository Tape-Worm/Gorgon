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
	/// Form used to open a file system.
	/// </summary>
	public partial class formOpenFileSystem 
		: Form
	{
		#region Variables.
		private FileSystemType _lastUsed;									// Last used file system.
		private string _rootPath = string.Empty;							// Root path.
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
		public formOpenFileSystem()
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
		/// Handles the Click event of the buttonSelectRoot control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSelectRoot_Click(object sender, EventArgs e)
		{
			DialogResult result;							// Dialog result.
			FileSystemType fsInfo = null;					// File system info.
			string fsName = string.Empty;					// File system name.

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
					if (!fsInfo.Information.IsPackFile)
					{
						fsName = _rootPath = dialogFolders.SelectedPath;
						if (fsName != string.Empty)
							fsName = fsName.Substring(fsName.LastIndexOf(@"\") + 1);
					}
					else
					{
						_rootPath = dialogOpen.FileName;
						fsName = Path.GetFileNameWithoutExtension(_rootPath);
					}

					// Remove an existing file system.
					if (Gorgon.FileSystems.Contains(fsName))
					{
						if (UI.YesNoBox("The file system '" + fsName + "' is already loaded.\nDo you wish to unload the existing file system?"))
						{
							for (int i = 0; i < Owner.MdiChildren.Length; i++)
							{
								if (((formFileSystemWindow)Owner.MdiChildren[i]).FileSystem == Gorgon.FileSystems[fsName])
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
					if (fsInfo == Gorgon.FileSystems.Types[0])
						_newFS = new FolderFileSystem(fsName);						
					else
						_newFS = Gorgon.FileSystems.Create(fsName, null);

					// Set the root path.
					_newFS.RootPath = _rootPath;
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
			_lastUsed = Gorgon.FileSystems.Types[comboFileSystemType.Text];

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
				Gorgon.FileSystems.Remove(_newFS);
		}
	}
}