#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Sunday, April 01, 2007 12:33:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GorgonLibrary.FileSystems;
using Dialogs;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Form used to start a new file system.
	/// </summary>
	public partial class formNewFileSystem 
		: Form
	{
		#region Variables.
		private FileSystemProvider _lastUsed;									// Last used file system.
		private FileSystem _newFS = null;									// New file system.
		private string _newName = "Untitled";								// Name of the file system.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the name of the file system.
		/// </summary>
		public string NewName
		{
			get
			{
				return _newName;
			}
			set
			{
				_newName = value;
			}
		}

		/// <summary>
		/// Property to return the selected file system.
		/// </summary>
		public FileSystem ActiveFileSystem
		{
			get
			{
				return _newFS;
			}
		}
		#endregion

		#region Methods.
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
		/// Handles the SelectedIndexChanged event of the comboFileSystemType control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void comboFileSystemType_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Get the current file system.
			_lastUsed = FileSystemProviderCache.Providers[comboFileSystemType.Text];
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
			{
				_newFS.Dispose();
				_newFS = null;
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the formNewFileSystem control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void formNewFileSystem_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				e.Handled = true;
				return;
			}

			if (e.KeyCode == Keys.Enter)
			{
				buttonOK_Click(this, EventArgs.Empty);
				e.Handled = true;
				return;
			}

			e.Handled = false;
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			FileSystemProvider fsInfo = null;		// File system information.

			// Attempt to create the file system.
			try
			{
				if (!FileSystemProviderCache.Providers.Contains(comboFileSystemType.Text))
				{
					UI.ErrorBox(this, "The file system type is not available.");
					return;
				}

				// Get file system information.
				fsInfo = FileSystemProviderCache.Providers[comboFileSystemType.Text];

				// Create the file system object.
				_newFS = FileSystem.Create(_newName, fsInfo);

                if (_newFS.Provider.IsEncrypted)
                {
                    if (_newFS.CreateAuthorization(this) == 0)
                    {
                        UI.ErrorBox(this, "This file system is encrypted and requires authorization parameters.");
                        _newFS.Dispose();
                        return;
                    }
                }

				// Save the last used file system.
				((formMain)Owner).LastUsed = _lastUsed;

				DialogResult = DialogResult.OK;
			}
			catch (Exception ex)
			{
				_newFS = null;
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// Get the last used file system.
			_lastUsed = ((formMain)Owner).LastUsed;

			RefreshCombo();
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
	}
}