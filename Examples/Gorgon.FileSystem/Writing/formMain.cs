﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Friday, January 18, 2013 8:47:30 AM
// 
#endregion

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Examples
{
	/// <summary>
	/// Main application interface.
	/// </summary>
	/// <remarks>
	/// In this example we will show how to write to the file system.  
	/// 
	/// Whenever we create a file system, it's created as a read-only file system.  This is fine for most uses, but we need to be
	/// able to write data into it.  The problem is that the file system data could be on read only media like a DVD or CD.  Gorgon's 
	/// virtual file system is based on PhysicsFS (http://icculus.org/physfs/), it uses the concept of a write directory.
	/// This means when you update a file or add one, it will get rerouted to the write directory.  The write directory MUST be a 
	/// physical directory on your hard drive and must be able to be written into.
	/// 
	/// Setting up a write directory is fairly simple:  Just set the WriteLocation property on your file system object.  Once this is
	/// done the directory is automatically mounted into the file system.  Once a write location is set, the files in it will take precedence
	/// over all files in the file system.  So, for example, if SomeText.txt exists in root of the mounted folder, and the same file
	/// name exists in the root of the WriteLocation, then the version in the write location will be used for file I/O operations.
	/// 
	/// Here, we do exactly this.  We take the file from the root of the directory and read it in.  By assigning the write directory
	/// after we can then load in the modified version of the file (if it exists).
	/// </remarks>
	public partial class formMain : Form
	{
		#region Variables.
		// Our file system.
		private GorgonFileSystem _fileSystem;
		// The file system writer.
		private GorgonFileSystemWriteArea _writer;
		// Write path.
		private string _writePath = string.Empty;
		// Original text.
		private string _originalText = string.Empty;
		// Changed text.
		private string _changedText = string.Empty;				
		#endregion

		#region Properties.
		/// <summary>
		/// Handles the TextChanged event of the textDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void textDisplay_TextChanged(object sender, EventArgs e)
		{
			buttonSave.Enabled = !string.Equals(_originalText, textDisplay.Text, StringComparison.CurrentCulture);
			itemLoadOriginal.Enabled = buttonSave.Enabled;
		}

		/// <summary>
		/// Handles the Click event of the buttonSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void buttonSave_Click(object sender, EventArgs e)
		{
			try
			{
				CommandEnable(false);
				textDisplay.Enabled = false;

				if (string.IsNullOrEmpty(textDisplay.Text))
				{
					_writer.DeleteFile("/SomeText.txt");
					LoadText();
					return;
				}

				using (Stream stream = _writer.OpenStream("/SomeText.txt", FileMode.Create))
				{
					 byte[] data = Encoding.UTF8.GetBytes(textDisplay.Text);
					stream.Write(data, 0, data.Length);
					_changedText = textDisplay.Text;
				}
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
			}
			finally
			{
				textDisplay.Enabled = true;
				CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
				UpdateInfo();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemLoadChanged control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void itemLoadChanged_Click(object sender, EventArgs e)
		{
			try
			{
				CommandEnable(false);
				LoadText();
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
			}
			finally
			{
				CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
				UpdateInfo();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemLoadOriginal control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void itemLoadOriginal_Click(object sender, EventArgs e)
		{
			try
			{
				CommandEnable(false);
				LoadText();
				textDisplay.Text = _originalText;
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
			}
			finally
			{
				CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
				UpdateInfo();
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to enable or disable the command buttons.
        /// </summary>
        /// <param name="value"><b>true</b> to enable the command buttons, <b>false</b> to disable.</param>
        private void CommandEnable(bool value)
        {
            itemLoadChanged.Enabled = itemLoadOriginal.Enabled = value;
	        buttonSave.Enabled = !string.Equals(textDisplay.Text, _originalText, StringComparison.CurrentCulture);
        }

        /// <summary>
		/// Function to update the information label.
		/// </summary>
		private void UpdateInfo()
        {
	        labelInfo.Text = string.Equals(_originalText, textDisplay.Text, StringComparison.CurrentCulture)
		                         ? $"Using original text from {Program.GetResourcePath(@"FolderSystem\").Ellipses(100, true)}"
		                         : $"Using modified text from {_writePath.Ellipses(100, true)}";
        }

		/// <summary>
		/// Function to load the text into the file system.
		/// </summary>
		private void LoadText()
		{
			string physicalPath = Program.GetResourcePath(@"FolderSystem\");

			// Unload the mounted files.
			_writer.Unmount();
            _fileSystem.Unmount(physicalPath);

			_fileSystem.Mount(physicalPath);

			// Load the original before we mount the write directory.
			IGorgonVirtualFile file = _fileSystem.GetFile("/SomeText.txt");

			using (Stream stream = file.OpenStream())
			{
				byte[] textData = new byte[stream.Length];

				stream.Read(textData, 0, textData.Length);
				_originalText = Encoding.UTF8.GetString(textData);
			}

			// Set the write location to the users app data folder.
			_writer.Mount();

			// Load the modified version (if it exists, if it doesn't, the original will be loaded instead).
			file = _fileSystem.GetFile("/SomeText.txt");

			using (Stream stream = file.OpenStream())
			{
				byte[] textData = new byte[stream.Length];

				stream.Read(textData, 0, textData.Length);
				_changedText = Encoding.UTF8.GetString(textData);

				textDisplay.Text = string.Equals(_changedText, _originalText, StringComparison.CurrentCulture) ? _originalText : _changedText;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			GorgonLogFile logFile = (GorgonLogFile)GorgonApplication.Log;

			try
			{
				_writePath = Path.GetDirectoryName(logFile.LogPath) + @"\Examples\FileSystem.Writing\";

				// Create our virtual file system.
				_fileSystem = new GorgonFileSystem(GorgonApplication.Log);
				_writer = new GorgonFileSystemWriteArea(_fileSystem, _writePath);

				LoadText();

				labelFileSystem.Text = $"{Program.GetResourcePath(@"FolderSystem\").Ellipses(100, true)} mounted as '/'.";
				labelWriteLocation.Text = $"{_writePath.Ellipses(100, true)} mounted as '/'";
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
				Application.Exit();
			}
			finally
			{
				CommandEnable(!string.Equals(_originalText, _changedText, StringComparison.CurrentCulture));
				UpdateInfo();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain" /> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
		}
		#endregion
	}
}
