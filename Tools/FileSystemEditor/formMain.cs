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
using SharpUtilities;
using SharpUtilities.Utility;
using GorgonLibrary;
using GorgonLibrary.PlugIns;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.FileSystems.Tools
{
	/// <summary>
	/// Main form for the application.
	/// </summary>
	public partial class formMain 
		: Form
	{
		#region Variables.
		private FileSystemType _lastUsed;						// Last used file system.
		private string _dataPath = string.Empty;				// Path to configuration files, etc...
		#endregion

		#region Methods.
		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateForm()
		{

		}

		/// <summary>
		/// Function to write out the file system types.
		/// </summary>
		private void WriteFileSystemTypes()
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
				foreach (FileSystemType fsInfo in Gorgon.FileSystems.Types)
				{
					element = fsTypes.CreateElement("FileSystemType");
					// Set the name as an attribute.
					attribute = fsTypes.CreateAttribute("Name");
					attribute.Value = fsInfo.Information.TypeName;
					element.Attributes.Append(attribute);

					// Set the location.
					if (fsInfo.PlugIn != null)
						element.InnerText = fsInfo.PlugIn.PlugInPath;
					elementRoot.AppendChild(element);
				}

				// Write misc. settings.
				elementRoot = fsTypes.CreateElement("Settings");
				elementXMLRoot.AppendChild(elementRoot);

				// Write out last used file system.
				element = fsTypes.CreateElement("LastUsedFS");
				element.InnerText = _lastUsed.Information.TypeName;
				elementRoot.AppendChild(element);

				// Write out window information.
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

				// Finally write the config document.
				fsTypes.Save(_dataPath + "Config.xml");
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Function to retrieve the available file system types.
		/// </summary>
		private void GetFileSystemTypes()
		{
			XmlDocument fsTypes = null;				// File system types.
			XmlNodeList nodes = null;				// Node list.
			XmlNode settingNode = null;				// Node used for settings.
			string fsPath = string.Empty;			// Path to the file system plug-in.
			string fsName = string.Empty;			// Name of the file system plug-in.
			FileSystemPlugIn fsPlugIn = null;		// File system plug-in.

			try
			{
				// Remove all information.
				Gorgon.FileSystems.ClearTypes();

				// Get the folder filesystem information.
				_lastUsed = Gorgon.FileSystems.Types[typeof(FolderFileSystem)];

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
					fsPath = node.InnerText;
					
					// Check to see if the file system plug-in exists.
					if (fsPath != string.Empty)
					{
						if ((!Directory.Exists(Path.GetDirectoryName(fsPath))) || (!File.Exists(fsPath)))
							UI.ErrorBox(this, "The file system plug-in '" + fsPath + "' could not be located.");
						else
							fsPlugIn = Gorgon.FileSystems.LoadPlugIn(fsPath);	// Load the file system plug-in.
					}
				}

				// Get the last used file system.
				settingNode = fsTypes.SelectSingleNode("//LastUsedFS");

				if (Gorgon.FileSystems.Types.Contains(settingNode.InnerText))
					_lastUsed = Gorgon.FileSystems.Types[settingNode.InnerText];
				else
					_lastUsed = Gorgon.FileSystems.Types[typeof(FolderFileSystem)];

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
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, sEx.Message, sEx.ErrorLog);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
		}
		#endregion

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Get the application data path.
				_dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				_dataPath = _dataPath + @"\Tape_Worm\Gorgon\" + Application.ProductName + @"\" + Application.ProductVersion + @"\";
				
				Gorgon.Initialize(true, false);

				// Get the available file system types.
				GetFileSystemTypes();
			}
			catch (SharpException sEx)
			{
				UI.ErrorBox(this, ErrorDialogIcons.Bug, sEx.Message, sEx.ErrorLog);
				Application.Exit();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
				Application.Exit();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Closing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"></see> that contains the event data.</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			// Write out configuration.
			WriteFileSystemTypes();

			Gorgon.Terminate();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
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
				formNew.LastUsed = _lastUsed;

				if (formNew.ShowDialog(this) == DialogResult.OK)
				{
					_lastUsed = formNew.LastUsed;

					// Create file system window.
					newFS = new formFileSystemWindow();
					newFS.MdiParent = this;
					newFS.FileSystem = formNew.ActiveFileSystemType;
					newFS.RootPath = formNew.FileSystemRootPath;
					newFS.Show();
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
		/// Handles the Click event of the menuItemOpen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void menuItemOpen_Click(object sender, EventArgs e)
		{
			formOpenFileSystem formOpen = null;		// Open file system form.
			formFileSystemWindow newFS = null;		// File system window.

			try
			{
				formOpen = new formOpenFileSystem();
				formOpen.LastUsed = _lastUsed;

				if (formOpen.ShowDialog(this) == DialogResult.OK)
				{
					_lastUsed = formOpen.LastUsed;

					// Create file system window.
					newFS = new formFileSystemWindow();
					newFS.MdiParent = this;
					newFS.FileSystem = formOpen.ActiveFileSystemType;
					// Mount the file system.
					newFS.FileSystem.Mount(@"\", true);
					newFS.RootPath = formOpen.FileSystemRootPath;
					newFS.Show();
				}

				ValidateForm();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, ex);
			}
			finally
			{
				if (formOpen != null)
					formOpen.Dispose();
				formOpen = null;
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
	}
}