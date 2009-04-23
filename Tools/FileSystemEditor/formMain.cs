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
// Created: Sunday, April 01, 2007 12:05:35 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Linq;
using GorgonLibrary;
using GorgonLibrary.PlugIns;
using GorgonLibrary.FileSystems;
using Dialogs;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Main form for the application.
	/// </summary>
	public partial class formMain 
		: Form
	{
		#region Variables.
		private FileSystemProvider _lastUsed;								// Last used file system.
		private string _lastOpenDir = @".\";								// Last file open dialog directory.
		private string _lastViewSetting = "LargeIcon";						// Last file system view setting.
		private string _dataPath = string.Empty;							// Path to configuration files, etc...
		private int _nextUntitled = 1;										// Next untitled file system.
		private decimal _importFileCount = 0;								// File total count for import.
		private decimal _fileCounter = 0;									// File counter.		
		private bool _inFileOperation = false;								// Flag to indicate that we're in a file operation.
		private string _lastSaveDir = @".\";								// Last save directory.
		private ConfirmationResult _confirmResult = ConfirmationResult.None;// Overwrite confirmation result.
		private bool _export = false;										// Flag to indicate that we're exporting files.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the last used file system type.
		/// </summary>
		public FileSystemProvider LastUsed
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
		/// Property to return whether we're in a blocking file operation or not.
		/// </summary>
		public bool InFileOperation
		{
			get
			{
				return _inFileOperation;
			}
		}

		/// <summary>
		/// Property to set or return the overwrite confirmation result.
		/// </summary>
		public ConfirmationResult OverwriteResult
		{
			get
			{
				return _confirmResult;
			}
			set
			{
				_confirmResult = value;
			}
		}

		/// <summary>
		/// Property to set or return the last export directory used.
		/// </summary>
		public string LastExportDirectory
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the last open path.
		/// </summary>
		public string LastOpenPath
		{
			get
			{
				return _lastOpenDir;
			}
			set
			{
				_lastOpenDir = value;
			}
		}

		/// <summary>
		/// Property to set or return the last file system view.
		/// </summary>
		public string LastView
		{
			get
			{
				return _lastViewSetting;
			}
			set
			{
				_lastViewSetting = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the menuItemAbout control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemAbout_Click(object sender, EventArgs e)
		{
			ABOOT about = null;		// About form.
			try
			{
				about = new ABOOT();
				about.ShowDialog(this);
			}
			finally
			{
				if (about != null)
					about.Dispose();

				about = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonSaveFS control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void buttonSaveFS_Click(object sender, EventArgs e)
		{
			try
			{
				SaveWindow((formFileSystemWindow)ActiveMdiChild);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSave_Click(object sender, EventArgs e)
		{
			formFileSystemWindow current = null;				// Currently active file system window.

			try
			{
				Cursor.Current = Cursors.WaitCursor;
				current = (formFileSystemWindow)this.ActiveMdiChild;

				// Save.
				current.Save(current.RootPath);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemSaveAs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemSaveAs_Click(object sender, EventArgs e)
		{
			formFileSystemWindow current = null;		// Currently active file system window.
			System.Windows.Forms.DialogResult result;	// Dialog result.

			try
			{
				Cursor.Current = Cursors.WaitCursor;
				current = (formFileSystemWindow)this.ActiveMdiChild;

				// Show the dialog.				
				if (current.FileSystem.Provider.IsPackedFile)
				{
					dialogFileSave.Filter = current.FileSystem.Provider.FileExtensions + "|All files (*.*)|*.*";					
					dialogFileSave.InitialDirectory = _lastSaveDir;
					result = dialogFileSave.ShowDialog(this);
				}
				else
				{
					dialogFolderSave.SelectedPath = _lastSaveDir;
					result = dialogFolderSave.ShowDialog(this);
				}

				// Save the file.
				if (result == DialogResult.OK)
				{
					if (current.FileSystem.Provider.IsPackedFile)
					{
						current.Save(dialogFileSave.FileName);
						_lastSaveDir = Path.GetDirectoryName(dialogFileSave.FileName);
					}
					else
					{
						current.Save(dialogFolderSave.SelectedPath);
						_lastSaveDir = dialogFolderSave.SelectedPath;
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Handles the Click event of the menuItemTileHorizontal control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemTileHorizontal_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.TileHorizontal);
		}

		/// <summary>
		/// Handles the Click event of the menuItemTileVertical control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemTileVertical_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.TileVertical);
		}

		/// <summary>
		/// Handles the Click event of the menuItemCascade control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemCascade_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.Cascade);
		}

		/// <summary>
		/// Handles the Click event of the menuItemArrangeIcons control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemArrangeIcons_Click(object sender, EventArgs e)
		{
			LayoutMdi(MdiLayout.ArrangeIcons);
		}

		/// <summary>
		/// Handles the Click event of the menuItemFileSystems control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemFileSystems_Click(object sender, EventArgs e)
		{
			formFileSystems fileSystemList = null;		// File system editor dialog.

			try
			{
				// Open the dialog.
				fileSystemList = new formFileSystems();
				fileSystemList.ShowDialog(this);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (fileSystemList != null)
					fileSystemList.Dispose();
				fileSystemList = null;
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemNew control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemNew_Click(object sender, EventArgs e)
		{
			formNewFileSystem formNew = null;		// New file system form.
			formFileSystemWindow newFS = null;		// File system window.

			try
			{
				formNew = new formNewFileSystem();

				// Check to ensure the name doesn't already exist.
				while (FileSystemCache.FileSystems.Contains("Untitled" + _nextUntitled.ToString()))
					_nextUntitled += 1;

				formNew.NewName = "Untitled" + _nextUntitled.ToString();

				if (formNew.ShowDialog(this) == DialogResult.OK)
				{
					// Create file system window.
					newFS = new formFileSystemWindow();
					newFS.IsNew = true;
					newFS.MdiParent = this;
					newFS.FileSystem = formNew.ActiveFileSystem;
					newFS.IsChanged = true;
					newFS.RootPath = formNew.NewName;
					newFS.Show();

					_nextUntitled += 1;
				}

				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (formNew != null)
					formNew.Dispose();
				formNew = null;
			}
		}

		/// <summary>
		/// Function to retrive the file system type from a file system file.
		/// </summary>
		/// <param name="filePath">Path to the file system.</param>
		/// <param name="newFileSystem">Newly loaded file system.</param>
		/// <returns>TRUE if we can load the file system, FALSE if not.</returns>
		private bool GetFileSystemFromFile(string filePath, out FileSystem newFileSystem)
		{
			string fsName = string.Empty;		// File system name.
			Stream stream = null;				// Stream to the file system.

			Cursor.Current = Cursors.WaitCursor;
			newFileSystem = null;
            try
            {
                // Open the selector.
                if (string.IsNullOrEmpty(filePath))
                    return true;

                if (!File.Exists(filePath))
                {
                    UI.ErrorBox(this, "The file system '" + filePath + "' does not exist.");
                    return false;
                }

				stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Try to match up the header.
                foreach (FileSystemProvider provider in FileSystemProviderCache.Providers)
                {
					if (!provider.IsPackedFile)
						fsName = Path.GetDirectoryName(filePath);
					else
						fsName = Path.GetFileNameWithoutExtension(filePath);

					// Do nothing if this file system is loaded already.
					if (FileSystemCache.FileSystems.Contains(fsName))
					{
						newFileSystem = null;
						return true;
					}
					newFileSystem = FileSystem.Create(fsName, provider);

					if (newFileSystem.IsValidForProvider(provider, stream))
					{
						// If the file system provider is encrypted, then we need access.
						if (provider.IsEncrypted)
						{
							if (newFileSystem.GetAuthorization(this) == 0)
							{
								UI.ErrorBox(this, "This file system needs authorization to open.");
								newFileSystem.Dispose();
								newFileSystem = null;
								return true;
							}
						}
						if (!provider.IsPackedFile)
							newFileSystem.AssignRoot(Path.GetDirectoryName(filePath));
						else
							newFileSystem.AssignRoot(filePath);
						break;
					}
					else
					{
						if (newFileSystem != null)
							newFileSystem.Dispose();
						newFileSystem = null;
					}
                }

                if (newFileSystem == null)
                    return false;

                return true;
            }
            catch (GorgonException gEx)
            {
                if (newFileSystem != null)
                    newFileSystem.Dispose();
                newFileSystem = null;

				if (gEx.ResultCode == GorgonErrors.AccessDenied)
				{
					GorgonException.Catch(gEx, (message) => UI.ErrorBox(this, "Access to the file system '" + filePath + "' is denied."));
					return true;
				}
				else
				{
					GorgonException.Catch(gEx, (message) => UI.ErrorBox(this, gEx));
					return false;
				}
            }
            catch (Exception ex)
            {
                if (newFileSystem != null)
                    newFileSystem.Dispose();
                newFileSystem = null;

                UI.ErrorBox(this, "There was an error trying to open the file system '" + filePath + "'.", ex);
                return false;
            }
			finally
			{
				if (stream != null)
					stream.Dispose();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to open a file.
		/// </summary>
		/// <param name="rootPath">Path to the root of the file system.</param>
		private void OpenFile(string rootPath)
		{
			formFileSystemWindow newFS = null;		// File system window.
			FileSystem fileSystem = null;			// File system to load.

			try
			{
				// Do nothing if we fail to open the file system.
                if (!GetFileSystemFromFile(rootPath, out fileSystem))
                {
                    UI.ErrorBox(this, "Cannot load '" + rootPath + "'.  There is no file system provider plug-in that can handle this format.");
                    return;
                }

				// Fall back to the selection method.
                if ((fileSystem == null) || (string.IsNullOrEmpty(rootPath)))
                    return;

				// Add a new file system window containing the loaded file system.
				newFS = new formFileSystemWindow();
				newFS.MdiParent = this;
				newFS.FileSystem = fileSystem;
				newFS.RootPath = fileSystem.Root;
				newFS.Show();				
			}
			catch (Exception ex)
			{
				if (newFS != null)
					newFS.Dispose();

				newFS = null;

				UI.ErrorBox(this, ex);
			}
			finally
			{
				ValidateForm();
			}
		}

		/// <summary>
		/// Handles the Click event of the menuItemOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemOpen_Click(object sender, EventArgs e)
		{
			List<string> extensions = new List<string>();
			string extensionList = string.Empty;
			
			// Gather all file extensions from all providers.
			foreach (FileSystemProvider provider in FileSystemProviderCache.Providers)
			{
				if (extensions.Count((extension) => string.Compare(extension, provider.FileExtensions, true) == 0) == 0)
					extensions.Add(provider.FileExtensions);
			}

			var sortedExtensions = from extensionFilters in extensions
								   orderby extensionFilters
								   select extensionFilters;

			foreach (var extension in sortedExtensions)
				extensionList += extension + "|";

			extensionList += "All files (*.*)|*.*";

			dialogOpenFileSystem.Filter = extensionList;
			if (dialogOpenFileSystem.ShowDialog(this) == DialogResult.OK)
			{
				foreach(string fileName in dialogOpenFileSystem.FileNames)
					OpenFile(fileName);
			}
		}

        /// <summary>
        /// Function to check whether a file system provider exists for the file system.
        /// </summary>
        /// <param name="filePath">Path to the file system.</param>
        /// <returns>TRUE to continue, FALSE to cancel.</returns>
        private bool CheckForFileSystemProvider(string filePath)
        {
            Stream headerStream = null;			// Stream for reading the header.
			FileSystem tempFS = null;			// Temporary file system.

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Open the selector.
                if (string.IsNullOrEmpty(filePath))
                    return false;

                if (!File.Exists(filePath))
                {
                    UI.ErrorBox(this, "The file system '" + filePath + "' does not exist.");
                    return false;
                }

                // Open the file and read the header ID.
                headerStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Try to match up the header.
                foreach (FileSystemProvider provider in FileSystemProviderCache.Providers)
                {
					using (tempFS = FileSystem.Create("TempFS" + provider.Name, provider))
					{
						if (tempFS.IsValidForProvider(provider, headerStream))
							return true;
					}
					tempFS = null;
                }
				
                return false;
            }
            catch (Exception ex)
            {
                UI.ErrorBox(this, "There was an error trying to find file system provider for '" + filePath + "'.", ex);
                return false;
            }
            finally
            {
				if (tempFS != null)
					tempFS.Dispose();
                if (headerStream != null)
                    headerStream.Dispose();
                headerStream = null;
                Cursor.Current = Cursors.Default;
            }
        }
        
        /// <summary>
		/// Function to retrieve the settings.
		/// </summary>
		private void GetSettings()
		{
			XmlDocument fsTypes = null;				// File system types.
			XmlNodeList nodes = null;				// Node list.
			XmlNode settingNode = null;				// Node used for settings.
			string fsPath = string.Empty;			// Path to the file system plug-in.
			string fsName = string.Empty;			// Name of the file system plug-in.
			string fsProvider = string.Empty;		// Name of the file system provider.
			FileSystemProvider fsPlugIn = null;			// File system plug-in.

			try
			{
				// Remove all information.
				FileSystemProviderCache.UnloadAll();

				// Get the folder filesystem information.
				_lastUsed = FileSystemProvider.Create(typeof(FolderFileSystem));

				// If the directory does not exist, then create it and leave.
				if (!Directory.Exists(_dataPath))
					Directory.CreateDirectory(_dataPath);					

				// The file doesn't exist, then add the default folder system and exit.
				if (!File.Exists(_dataPath + "Config.xml"))
					return;

				// Load the config file.
				fsTypes = new XmlDocument();
				fsTypes.Load(_dataPath + "Config.xml");

				// Get the configuration info.
				nodes = fsTypes.SelectNodes("//FileSystemType");

				// Get all previous file systems.
				foreach (XmlNode node in nodes)
				{
					fsName = node.Attributes["Name"].Value;
					if (node.Attributes["PlugInName"] != null)
						fsProvider = node.Attributes["PlugInName"].Value;
					fsPath = node.InnerText;					
					
					// Check to see if the file system plug-in exists.
					if ((fsPath != string.Empty) && (!string.IsNullOrEmpty(fsProvider)))
					{
                        try
                        {
                            if ((Directory.Exists(Path.GetDirectoryName(fsPath))) && (File.Exists(fsPath)))
                                fsPlugIn = FileSystemProvider.Load(fsPath, fsProvider);
                        }
                        catch (Exception ex)
                        {
                            UI.ErrorBox(this, "There was an error attempting to load the file system provider '" + fsProvider + "'.", ex);
                        }
					}
				}

				// Get the last used file system.
				settingNode = fsTypes.SelectSingleNode("//LastUsedFS");

				if (FileSystemProviderCache.Providers.Contains(settingNode.InnerText))
					_lastUsed = FileSystemProviderCache.Providers[settingNode.InnerText];
				else
					_lastUsed = FileSystemProviderCache.Providers[typeof(FolderFileSystem)];

				// Get misc. settings.
				settingNode = fsTypes.SelectSingleNode("//LastOpenPath");
				if (settingNode != null)
					_lastOpenDir = settingNode.InnerText;

				if (_lastOpenDir == string.Empty)
					_lastOpenDir = @".\";

				// Get misc. settings.
				settingNode = fsTypes.SelectSingleNode("//LastSavePath");
				if (settingNode != null)
					_lastSaveDir = settingNode.InnerText;

				if (_lastSaveDir == string.Empty)
					_lastSaveDir = @".\";

				settingNode = fsTypes.SelectSingleNode("//LastExportPath");
				if (settingNode != null)
					LastExportDirectory = settingNode.InnerText;

				settingNode = fsTypes.SelectSingleNode("//LastViewType");
				if (settingNode != null)
					_lastViewSetting = settingNode.InnerText;

				if (_lastViewSetting == string.Empty)
					_lastViewSetting = "LargeIcon";

				// Get the window settings.
				settingNode = fsTypes.SelectSingleNode("//WindowX");

				if (settingNode != null)
					Left = Convert.ToInt32(settingNode.InnerText);

				settingNode = fsTypes.SelectSingleNode("//WindowY");

				if (settingNode != null)
					Top = Convert.ToInt32(settingNode.InnerText);

				settingNode = fsTypes.SelectSingleNode("//WindowWidth");

				if (settingNode != null)
					Width = Convert.ToInt32(settingNode.InnerText);

				settingNode = fsTypes.SelectSingleNode("//WindowHeight");

				if (settingNode != null)
					Height = Convert.ToInt32(settingNode.InnerText);

				// Load last open file systems.
				nodes = fsTypes.SelectNodes("//LastOpen/File");

				if (nodes != null)
				{
					// Load each file system.
					foreach (XmlNode node in nodes)
					{
						XmlAttribute isFolder = null;		// Is folder attribute.

						if (!string.IsNullOrEmpty(node.InnerText))
						{
							isFolder = node.Attributes["IsFolder"];
                                                        
							if ((isFolder != null) && (string.Compare(isFolder.Value, "Yes", true) == 0))
							{
                                if (File.Exists(node.InnerText + @"\header.folderSystem"))
                                {
                                    if (!CheckForFileSystemProvider(node.InnerText + @"\header.folderSystem"))
                                    {
                                        if (UI.ConfirmBox(this, "Cannot open '" + node.InnerText + "' because there was no file system provider plug-in that can handle its format.\n\nWould you like to try and find the plug-in provider?") == ConfirmationResult.No)
                                            continue;
                                        else
                                            menuItemFileSystems.PerformClick();
                                    }

                                    OpenFile(node.InnerText + @"\header.folderSystem");
                                }
							}
							else
							{
                                if (File.Exists(node.InnerText))
                                {
                                    if (!CheckForFileSystemProvider(node.InnerText))
                                    {
                                        if (UI.ConfirmBox(this, "Cannot open '" + node.InnerText + "' because there was no file system provider plug-in that can handle its format.\n\nWould you like to try and find the plug-in provider?") == ConfirmationResult.No)
                                            continue;                                            
                                        else
                                            menuItemFileSystems.PerformClick();                                            
                                    }
                                    
                                    OpenFile(node.InnerText);
                                }
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to disable all the menu items.
		/// </summary>
		private void DisableMenus(ToolStripMenuItem container)
		{
			if (container == null)
			{
				foreach (ToolStripItem item in menuMain.Items)
				{
					if (item is ToolStripMenuItem)
					{
						if ((((ToolStripMenuItem)item).DropDownItems.Count > 0) && (item != menuMain.MdiWindowListItem))
							DisableMenus((ToolStripMenuItem)item);
					}

					item.Enabled = false;
				}
				return;
			}
			else
			{
				// Disable child menus.
				foreach (ToolStripItem item in container.DropDownItems)
				{
					if (item is ToolStripMenuItem)
					{
						if (((ToolStripMenuItem)item).DropDownItems.Count > 0)
							DisableMenus((ToolStripMenuItem)item);
					}

					item.Enabled = false;
				}
				return;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			string[] commandArgs = null;		// Command line arguments.

			try
			{
				// Get the application data path.
				_dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				_dataPath = _dataPath + @"\Tape_Worm\Gorgon\" + Application.ProductName + @"\";

				Gorgon.Initialize(false, true);

				// Get the available file system types.
				GetSettings();

				ValidateForm();

				// Find out if we've specified a file on the command line.
				commandArgs = Environment.GetCommandLineArgs();
				if ((commandArgs != null) && (commandArgs.Length > 1))
				{
					Visible = true;

					// Open several file systems if we've asked for it.
					for (int i = 1; i < commandArgs.Length; i++)
					{
                        if (!string.IsNullOrEmpty(commandArgs[i]))
                        {
                            if (!CheckForFileSystemProvider(commandArgs[i]))
                            {
                                if (UI.ConfirmBox("The file '" + commandArgs[i] + "' could not be opened because a file system provider plug-in was not found for it.\n\nWould you like to try and find it?") == ConfirmationResult.Yes)
                                {
                                    menuItemFileSystems.PerformClick();
                                    OpenFile(commandArgs[i]);
                                }
                            }
                            else
                                OpenFile(commandArgs[i]);
                        }
					}
				}
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
				Application.Exit();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.MdiChildActivate"></see> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnMdiChildActivate(EventArgs e)
		{
			base.OnMdiChildActivate(e);

			ValidateForm();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			try
			{
				base.OnFormClosing(e);

				// Don't allow close while in a file operation.
				if (((_inFileOperation) && (e.CloseReason == CloseReason.UserClosing)) || (_confirmResult == ConfirmationResult.Cancel))
				{
					e.Cancel = true;
					return;
				}

				WriteSettings();

				Gorgon.Terminate();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				// Reset the confirmation result.
				_confirmResult = ConfirmationResult.None;
			}
		}

		/// <summary>
		/// Function to write out the settings.
		/// </summary>
		public void WriteSettings()
		{
			XmlDocument fsTypes = null;						// File system types.
			XmlProcessingInstruction instruction = null;	// Processing instruction.
			XmlElement elementRoot = null;					// Root element.
			XmlElement elementXMLRoot = null;				// Root of the document.
			XmlAttribute attribute = null;					// Attribute.
			XmlElement element = null;						// Element.

			try
			{
				// If the directory does not exist, then create it and leave.
				if (!Directory.Exists(_dataPath))
					Directory.CreateDirectory(_dataPath);

				// Set up the config file.
				fsTypes = new XmlDocument();
				instruction = fsTypes.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\"");
				fsTypes.AppendChild(instruction);

				elementXMLRoot = fsTypes.CreateElement("FileSystemEditorSettings");
				fsTypes.AppendChild(elementXMLRoot);
				elementRoot = fsTypes.CreateElement("FileSystemTypes");
				elementXMLRoot.AppendChild(elementRoot);

				// Get all previous file systems.
				foreach (FileSystemProvider fsInfo in FileSystemProviderCache.Providers)
				{
					element = fsTypes.CreateElement("FileSystemType");
					// Set the name as an attribute.
					attribute = fsTypes.CreateAttribute("Name");
					attribute.Value = fsInfo.Name;
					element.Attributes.Append(attribute);

					// Get provider plug-in name.
					attribute = fsTypes.CreateAttribute("PlugInName");
					attribute.Value = fsInfo.ProviderPlugInName;
					element.Attributes.Append(attribute);

					// Set the location.
					if (fsInfo.IsPlugIn)
						element.InnerText = fsInfo.PlugInPath;
					elementRoot.AppendChild(element);
				}

				// Write misc. settings.
				elementRoot = fsTypes.CreateElement("Settings");
				elementXMLRoot.AppendChild(elementRoot);

				// Write out last used file system.
				element = fsTypes.CreateElement("LastUsedFS");
				element.InnerText = _lastUsed.Name;
				elementRoot.AppendChild(element);

				// Write out misc. settings.
				element = fsTypes.CreateElement("LastOpenPath");
				element.InnerText = _lastOpenDir;
				elementRoot.AppendChild(element);

				element = fsTypes.CreateElement("LastSavePath");
				element.InnerText = _lastSaveDir;
				elementRoot.AppendChild(element);

				element = fsTypes.CreateElement("LastExportPath");
				element.InnerText = LastExportDirectory;
				elementRoot.AppendChild(element);

				element = fsTypes.CreateElement("LastViewType");
				element.InnerText = _lastViewSetting;
				elementRoot.AppendChild(element);

				// Write out window information.
				if (WindowState != FormWindowState.Normal)
					WindowState = FormWindowState.Normal;

				element = fsTypes.CreateElement("WindowX");
				element.InnerText = this.Left.ToString();
				elementRoot.AppendChild(element);
				element = fsTypes.CreateElement("WindowY");
				element.InnerText = this.Top.ToString();
				elementRoot.AppendChild(element);
				element = fsTypes.CreateElement("WindowWidth");
				element.InnerText = this.Width.ToString();
				elementRoot.AppendChild(element);
				element = fsTypes.CreateElement("WindowHeight");
				element.InnerText = this.Height.ToString();
				elementRoot.AppendChild(element);

				elementRoot = fsTypes.CreateElement("LastOpen");
				elementXMLRoot.AppendChild(elementRoot);
				
				// Now we need to write out the last open file systems.
				foreach (FileSystem fileSystem in FileSystemCache.FileSystems)
				{
					XmlAttribute isFolder = null;		// Folder attribute.

					// Determine if this is a folder.
					isFolder = fsTypes.CreateAttribute("IsFolder");
					if (!fileSystem.Provider.IsPackedFile)
						isFolder.Value = "Yes";
					else
						isFolder.Value = "No";
					
					element = fsTypes.CreateElement("File");
					element.Attributes.Append(isFolder);
					element.InnerText = fileSystem.Root;
					elementRoot.AppendChild(element);
				}

				// Finally write the config document.
				fsTypes.Save(_dataPath + "Config.xml");
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		public void ValidateForm()
		{
			formFileSystemWindow fsWindow = null;		// File system window.

			// Disable by default.
			DisableMenus(null);
			labelStats.Visible = false;
			buttonSaveFS.Enabled = false;

			// If we're a file operation, then disable the majority of the interface.
			if (!_inFileOperation)
			{
				// Default enabled items.
				menuItemFile.Enabled = true;
				menuItemExit.Enabled = true;
				menuItemWindow.Enabled = true;
				menuItemHelp.Enabled = true;
				menuItemNew.Enabled = true;
				menuItemOpen.Enabled = true;
				menuItemAbout.Enabled = true;
				menuItemArrangeIcons.Enabled = true;
				menuItemCascade.Enabled = true;
				menuItemTileHorizontal.Enabled = true;
				menuItemTileVertical.Enabled = true;
				menuItemFileSystems.Enabled = true;

				FormWindowState lastState = FormWindowState.Normal;

				foreach (Form form in MdiChildren)
				{
					lastState = form.WindowState;
					form.WindowState = FormWindowState.Normal;
					form.Enabled = true;
					form.WindowState = lastState;
				}
				progressFileImport.Visible = false;
				labelFileName.Visible = false;

				if ((MdiChildren.Length > 0) && (ActiveMdiChild != null))
				{
					buttonSaveFS.Enabled = true;
					menuItemSaveAs.Enabled = true;
				}

				if (ActiveMdiChild != null)
				{
					fsWindow = (formFileSystemWindow)ActiveMdiChild;

					// Activate the save icon.
					if ((fsWindow.IsChanged) && (!fsWindow.IsNew))
						menuItemSave.Enabled = true;

					labelStats.Visible = true;
					labelStats.Text = "# of files: " + fsWindow.FileSystem.FileCount.ToString() + ", Size: " + Utilities.FormatByteUnits(fsWindow.FileSystem.FileSystemSize(false));
					if (fsWindow.FileSystem.Provider.IsCompressed)
						labelStats.Text += ", Compressed size: " + Utilities.FormatByteUnits(fsWindow.FileSystem.FileSystemSize(true));
				}
			}
			else
			{
				foreach (Form form in MdiChildren)
					form.Enabled = false;
				progressFileImport.Visible = true;
				labelFileName.Visible = true;
			}				
		}

		/// <summary>
		/// Function to initialize the file import bar.
		/// </summary>
		/// <param name="fileCount">Number of files being imported.</param>
		/// <param name="exportFile">TRUE to set for file export, FALSE for import.</param>
		public void InitFileImport(int fileCount, bool exportFile)
		{
			_inFileOperation = true;
			_importFileCount = fileCount;
			_fileCounter = 0;
			_export = exportFile;

			progressFileImport.Value = 0;
			labelFileName.Owner.Refresh();

			ValidateForm();
		}

		/// <summary>
		/// Function to update the status bar for file import.
		/// </summary>
		/// <param name="fileName">Name of the file being imported.</param>
		public void UpdateFileImport(string fileName)
		{
			decimal percentage = 0;		// Percentage.

			Cursor.Current = Cursors.WaitCursor;
			if (!_export)
				labelFileName.Text = "Importing file: " + fileName;
			else
				labelFileName.Text = "Exporting file: " + fileName;
			_fileCounter += 1;

			percentage = (_fileCounter / _importFileCount) * 100;

			if (percentage > 100.0M)
				percentage = 100.0M;

			progressFileImport.Value = Convert.ToInt32(percentage);
			labelFileName.Invalidate();
			progressFileImport.Invalidate();
			progressFileImport.Owner.Invalidate();
			progressFileImport.Owner.Invalidate();
			progressFileImport.Owner.Refresh();
		}

		/// <summary>
		/// Function to clean up after a file import.
		/// </summary>
		public void EndFileImport()
		{
			_inFileOperation = false;
			Cursor.Current = Cursors.Default;

			Application.DoEvents();
			ValidateForm();
		}

		/// <summary>
		/// Function to perform save as.
		/// </summary>
		/// <param name="window">Window to save.</param>
		public void SaveWindow(formFileSystemWindow window)
		{
			if (window.IsNew)
				menuItemSaveAs_Click(this, EventArgs.Empty);
			else
				menuItemSave_Click(this, EventArgs.Empty);

			WriteSettings();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
		}
		#endregion
	}
}