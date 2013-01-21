#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, June 06, 2012 8:08:26 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GorgonLibrary.UI;
using GorgonLibrary.IO;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Importer form.
	/// </summary>
	partial class formImport : Form
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the destination path.
		/// </summary>
		public string DestinationPath
		{
			get
			{
				return textDestination.Text;
			}
			set
			{
				textDestination.Text = value.FormatDirectory('/');
			}
		}

		/// <summary>
		/// Property to set or return the last path used.
		/// </summary>
		public string LastFilePath
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the project folder.
		/// </summary>
		public ProjectFolder Folder
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the files to be imported.
		/// </summary>
		public IList<string> Files
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the checkOpenDoc control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void checkOpenDoc_Click(object sender, EventArgs e)
		{
			ValidateButtons();
		}

		/// <summary>
		/// Handles the TextChanged event of the textDestination control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textDestination_TextChanged(object sender, EventArgs e)
		{
			ValidateButtons();
		}

		/// <summary>
		/// Function to parse the file names from the text box.
		/// </summary>
		private void ParseFiles()
		{
			string files = textImportFile.Text.Replace("\" ", "*");

			Files = files.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < Files.Count; i++)
				Files[i] = Files[i].Replace("\"", string.Empty);
		}

		/// <summary>
		/// Handles the Click event of the buttonOK control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonOK_Click(object sender, EventArgs e)
		{
			try
			{
				Program.Settings.OpenDocsAfterImport = checkOpenDoc.Checked;

				if (checkOpenDoc.Checked)
					Program.Settings.OpenLastDocOnly = checkOpenLastDoc.Checked;

				ParseFiles();

				// Check to see if each file exists.
				foreach (var file in Files)
				{
					if (!File.Exists(file))
					{
						GorgonDialogs.ErrorBox(this, "The file '" + file + "' does not exist.");
						DialogResult = System.Windows.Forms.DialogResult.None;
						return;
					}
				}

				if ((textDestination.Text != "/") && (!string.IsNullOrEmpty(textDestination.Text)))
				{
					// Get destination folders.
					Folder = Program.Project.GetFolder(null, textDestination.Text);
				
					if (Folder == null) 
					{
						if (GorgonDialogs.ConfirmBox(this, "The project folder path '" + textDestination.Text + "' does not exist.  Would you like to create it?") == ConfirmationResult.No)
						{
							DialogResult = System.Windows.Forms.DialogResult.None;
							return;
						}

						Folder = Program.Project.CreateFolder(textDestination.Text);						
					}
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				DialogResult = System.Windows.Forms.DialogResult.None;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonSelectDestination control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSelectDestination_Click(object sender, EventArgs e)
		{
			formProjectFolderSelector folders = null;

			try
			{
				folders = new formProjectFolderSelector();
				folders.SelectedPath = DestinationPath;
				if (folders.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
					textDestination.Text = folders.SelectedPath.FormatDirectory('/');
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				if (folders != null)
					folders.Dispose();

				ValidateButtons();
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonImportFile control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonImportFile_Click(object sender, EventArgs e)
		{	
			try
			{
				string extensions = "All files (*.*)|*.*";
				// Get extension support.

				var types = from docType in Program.Project.DocumentTypes
							join docDesc in Program.Project.DocumentDescriptions on docType.Value equals docDesc.Value
							let Extension = docType.Key
							let TypeDesc = new { DocType = docType.Value, Description = docDesc.Key }
							group Extension by TypeDesc;
							
				foreach (var type in types)
				{					
					string wildCarded = string.Empty;
					extensions += "|" + type.Key.Description;

					foreach (var extension in type)
					{
						if (wildCarded.Length > 0)
							wildCarded += ";";
						wildCarded += "*." + extension;
					}
					extensions += "|" + wildCarded;
				}

				dialogFile.Filter = extensions;
				dialogFile.InitialDirectory = LastFilePath;
				if (dialogFile.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					foreach (var fileName in dialogFile.FileNames)
					{
						if (textImportFile.Text.Length > 0)
							textImportFile.Text += " ";

						if (dialogFile.FileNames.Length > 1)
							textImportFile.Text += "\"" + fileName + "\"";
						else
							textImportFile.Text += fileName;
					}

					LastFilePath = Path.GetDirectoryName(dialogFile.FileName).FormatDirectory(Path.DirectorySeparatorChar);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateButtons();
			}
		}

		/// <summary>
		/// Handles the TextChanged event of the textImportFile control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void textImportFile_TextChanged(object sender, EventArgs e)
		{
			ValidateButtons();
		}

		/// <summary>
		/// Function to validate the buttons on the form.
		/// </summary>
		private void ValidateButtons()
		{
			checkOpenLastDoc.Enabled = checkOpenDoc.Checked;
			textDestination.Enabled = buttonSelectDestination.Enabled = textImportFile.Text.Length > 0;
			buttonOK.Enabled = textDestination.Text.Length > 0 && textImportFile.Text.Length > 0;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			checkOpenDoc.Checked = Program.Settings.OpenDocsAfterImport;

			if (checkOpenDoc.Checked)
				checkOpenLastDoc.Checked = Program.Settings.OpenLastDocOnly;
			else
				checkOpenLastDoc.Checked = false;

			ValidateButtons();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formImport"/> class.
		/// </summary>
		public formImport()
		{
			InitializeComponent();

			textDestination.Text = "/";
		}
		#endregion
	}
}
