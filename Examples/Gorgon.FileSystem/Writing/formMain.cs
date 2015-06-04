#region MIT.
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
	/// physical directory on your harddrive and must be able to be written into.
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
		private GorgonFileSystem _fileSystem;			        // Our file system.
		private string _writePath = string.Empty;				// Write path.
		private string _originalText = string.Empty;			// Original text.
		#endregion

		#region Properties.
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
				buttonSave.Enabled = false;
				textDisplay.Enabled = false;
				byte[] data = Encoding.UTF8.GetBytes(textDisplay.Text);
				_fileSystem.WriteFile("/SomeText.txt", data);
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
			}
			finally
			{
				CommandEnable(true);
				itemLoadChanged.Enabled = !string.Equals(textDisplay.Text, _originalText, StringComparison.CurrentCulture);
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
				CommandEnable(true);
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
				CommandEnable(true);
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
            buttonSave.Enabled = buttonReload.Enabled = textDisplay.Enabled = value;
        }

        /// <summary>
		/// Function to update the information label.
		/// </summary>
		private void UpdateInfo()
        {
	        labelInfo.Text = string.Equals(_originalText, textDisplay.Text)
		                         ? string.Format("Using original text from {0}", Program.GetResourcePath(@"FolderSystem\").Ellipses(100, true))
		                         : string.Format("Using modified text from {0}", _writePath.Ellipses(100, true));
        }

		/// <summary>
		/// Function to load the text into the file system.
		/// </summary>
		private void LoadText()
		{
		    _fileSystem.Clear();
			_fileSystem.Mount(Program.GetResourcePath(@"FolderSystem\"));

			// Load the original before we mount the write directory.
			byte[] textData = _fileSystem.ReadFile("/SomeText.txt");
			_originalText = Encoding.UTF8.GetString(textData);

			// Set the write location to the users app data folder.
			_fileSystem.WriteLocation = _writePath;

			// Load the modified version.			
			textData = _fileSystem.ReadFile("/SomeText.txt");
			string modifiedText = Encoding.UTF8.GetString(textData);

			textDisplay.Text = string.Equals(modifiedText, _originalText) ? _originalText : modifiedText;
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

				LoadText();

				labelFileSystem.Text = string.Format("{0} mounted as '/'.", Program.GetResourcePath(@"FolderSystem\").Ellipses(100, true));
				labelWriteLocation.Text = string.Format("{0} mounted as '/'", _writePath.Ellipses(100, true));
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
				Application.Exit();
			}
			finally
			{
				itemLoadChanged.Enabled = !string.Equals(textDisplay.Text, _originalText, StringComparison.CurrentCulture);
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
