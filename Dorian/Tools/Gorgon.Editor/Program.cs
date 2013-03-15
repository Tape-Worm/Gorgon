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
// Created: Monday, April 30, 2012 6:28:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using GorgonLibrary;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application interface.
	/// </summary>
	static class Program
    {
        #region Constants.
        /// <summary>
        /// Meta data file name.
        /// </summary>
        public const string MetaDataFile = ".gorgon.editor.metadata";
        /// <summary>
        /// Meta data file path.
        /// </summary>
        public const string MetaDataFilePath = "/" + MetaDataFile;
        #endregion

        #region Variables.
        private static XElement _metadataRootNode = null;
        private static FileWriterPlugIn _writerPlugIn = null;
        #endregion

        #region Properties.
        /// <summary>
		/// Property to set or return the file writer plug-in for the currently loaded file.
		/// </summary>
		public static FileWriterPlugIn CurrentWriterPlugIn
		{
            get
            {
                return _writerPlugIn;
            }
            set
            {
                _writerPlugIn = value;

                if (EditorFileMetaData == null)
                {
                    return;
                }

                XElement writerNode = EditorFileMetaData.Descendants("WriterPlugIn").FirstOrDefault();

                // Only write out the meta data info if we have a plug-in.  Otherwise, remove the element.
                if (value != null)
                {
                    if (writerNode == null)
                    {
                        writerNode = new XElement("WriterPlugIn");
                        _metadataRootNode.Add(writerNode);
                    }

                    var attribute = writerNode.Attribute("TypeName");
                    if (attribute == null)
                    {
                        writerNode.Add(new XAttribute("TypeName", CurrentWriterPlugIn.GetType().FullName));
                    }
                    else
                    {
                        attribute.Value = CurrentWriterPlugIn.GetType().FullName;
                    }
                }
                else
                {
                    if (writerNode != null)
                    {
                        writerNode.Remove();
                    }
                }
            }
		}

        /// <summary>
        /// Property to return a list of disabled plug-ins.
        /// </summary>
        public static Dictionary<EditorPlugIn, string> DisabledPlugIns
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a list of loaded content plug-ins.
        /// </summary>
        public static Dictionary<string, ContentPlugIn> ContentPlugIns
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a list of loaded file writer plug-ins.
        /// </summary>
        public static Dictionary<string, FileWriterPlugIn> WriterPlugIns
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Property to return a list of items that have been changed since the file was opened.
        /// </summary>
        public static Dictionary<string, bool> ChangedItems
        {
            get;
            private set;
        }
        
		/// <summary>
		/// Property to return the name of the editor file.
		/// </summary>
		public static string EditorFile
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the path to the editor file.
        /// </summary>
        public static string EditorFilePath
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the logging interface for the application.
		/// </summary>
		public static GorgonLogFile LogFile
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the file system for the scratch files.
		/// </summary>
		public static GorgonFileSystem ScratchFiles
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the currently loaded content.
		/// </summary>
		public static ContentObject CurrentContent
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the settings for the application.
		/// </summary>
		public static GorgonEditorSettings Settings
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the graphics interface.
		/// </summary>
		public static GorgonGraphics Graphics
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the meta data for the file.
        /// </summary>
        public static XDocument EditorFileMetaData
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return a hashset of file names that will not be allowed in the editor.
        /// </summary>
        public static HashSet<string> BlockedFiles
        {
            get;
            private set;
        }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to retrieve the writer plug-in that was used to write this file.
        /// </summary>
        /// <param name="path">Path to the current file.</param>
        /// <returns>The writer plug-in, if found.  NULL if not.</returns>
        private static FileWriterPlugIn GetWriterPlugIn(string path)
        {
            // Short circuit.
            if (WriterPlugIns.Count == 0)
            {
                return null;
            }

            var file = ScratchFiles.GetFile(MetaDataFilePath);
            var firstPlugIn = (from plugIn in WriterPlugIns
                               where plugIn.Value.FileExtensions.ContainsKey(Path.GetExtension(path).ToLower())
                               select plugIn.Value).FirstOrDefault();

            if (file == null)
            {
                // We don't have any meta data in this file, so use the first 
                // available plug-in.
                return firstPlugIn;
            }

            // Read in the meta data file.
            using (var stream = file.OpenStream(false))
            {
                EditorFileMetaData = XDocument.Load(stream);
            }

            // Find the writer plug-in element.
            var plugInElement = EditorFileMetaData.Descendants("WriterPlugIn").FirstOrDefault();

            // No element, use the first plug-in.
            if (plugInElement == null)
            {
                return firstPlugIn;
            }

            // Ensure the element is properly formed.
            if ((plugInElement.HasAttributes) 
                && (plugInElement.Attribute("TypeName") != null) 
                && (!string.IsNullOrWhiteSpace(plugInElement.Attribute("TypeName").Value)))
            {
                // Ensure we actually have the plug-in loaded.
                var writer = (from plugIn in WriterPlugIns
                              let plugInType = plugIn.Value.GetType().FullName
                              where string.Compare(plugInType, plugInElement.Attribute("TypeName").Value, true) == 0
                              select plugIn.Value).FirstOrDefault();

                if (writer != null)
                {
                    firstPlugIn = writer;
                }
            }

            return firstPlugIn;
        }

        /// <summary>
        /// Function to open the editor file.
        /// </summary>
        /// <param name="path">Path to the editor file.</param>
        public static void OpenEditorFile(string path)
        {
            IList<GorgonFileSystemMountPoint> mountPoints = new List<GorgonFileSystemMountPoint>();

			ChangedItems.Clear();

			// Remove our scratch data.
			CleanUpScratchArea();
			InitializeScratch();

            // Get the currently mounted file systems.
            foreach(var mountPoint in ScratchFiles.MountPoints)
            {
                mountPoints.Add(mountPoint);
            }

            // Add the new file system as a mount point.
            ScratchFiles.Mount(path);

            // Remove the mount point.
            foreach (var mountPoint in mountPoints)
            {
                ScratchFiles.Unmount(mountPoint);
            }

            Program.EditorFilePath = string.Empty;
            Program.EditorFile = Path.GetFileName(path);
            Program.Settings.LastEditorFile = path;
                        
            // Find the writer plug-in that can write this file.
            CurrentWriterPlugIn = GetWriterPlugIn(path);

            // If we can't write the file, then leave the editor file path as blank.
            if (CurrentWriterPlugIn != null)
            {
                Program.EditorFilePath = path;
            }
        }

		/// <summary>
		/// Function to create a new editor file.
		/// </summary>
		public static void NewEditorFile()
		{
			// Unmount all the file systems.
			foreach (var mountPoint in ScratchFiles.MountPoints.ToArray())
			{
				ScratchFiles.Unmount(mountPoint);
			}

			// Initialize the scratch area.
			CleanUpScratchArea();
			InitializeScratch();

			Program.EditorFile = "Untitled";
			Program.EditorFilePath = string.Empty;
			_metadataRootNode.Descendants().Remove();

			Program.Settings.LastEditorFile = string.Empty;

			Program.ChangedItems.Clear();
		}

        /// <summary>
        /// Function to save the editor file.
        /// </summary>
        /// <param name="path">Path to the new file.</param>
        public static void SaveEditorFile(string path)
        {
            // We don't have a writer plug-in, at this point, that's not good.
            if (CurrentWriterPlugIn == null)
            {
                throw new IOException("Cannot write the file '" + path + "'.  There is no plug-in available to write the file with.");                
            }

            // Write the meta data file to the file system.
            using (var metaDataStream = ScratchFiles.OpenStream(MetaDataFilePath, true))
            {
                EditorFileMetaData.Save(metaDataStream);
            }

            // Write the file.
            CurrentWriterPlugIn.WriteFile(path);
            
            EditorFile = Path.GetFileName(path);
            EditorFilePath = path;
			Program.Settings.LastEditorFile = path;

            // Remove all changed items.
            ChangedItems.Clear();

			// Clean up and reinitialize our scratch data.
			CleanUpScratchArea();
			InitializeScratch();
        }

		/// <summary>
		/// Function to allow the user to select a new scratch file location.
		/// </summary>
		/// <returns>The new path for the scratch data, or NULL if canceled.</returns>
		public static string SetScratchLocation()
		{
			FolderBrowserDialog dialog = null;

			try
			{
				dialog = new FolderBrowserDialog();
				dialog.Description = "Select a new temporary scratch path.";
				if (Directory.Exists(Settings.ScratchPath))
				{
					dialog.SelectedPath = Settings.ScratchPath;
				}
				dialog.ShowNewFolderButton = true;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					// Append the scratch directory.
					return dialog.SelectedPath.FormatDirectory(Path.DirectorySeparatorChar) + "Gorgon.Editor." + Guid.NewGuid().ToString("N").FormatDirectory(Path.DirectorySeparatorChar);
				}

				return null;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				return null;
			}
			finally
			{
				if (dialog != null)
				{
					dialog.Dispose();
				}
			}
		}

        /// <summary>
        /// Function to intialize the scratch area.
        /// </summary>
        public static void InitializeScratch()
        {
            Program.ScratchFiles.WriteLocation = Program.Settings.ScratchPath;

            // Set the directory to hidden.  We don't want users really messing around in here if we can help it.
            var scratchDir = new System.IO.DirectoryInfo(Program.Settings.ScratchPath);
            if (((scratchDir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                || ((scratchDir.Attributes & FileAttributes.NotContentIndexed) != FileAttributes.NotContentIndexed))
            {
                scratchDir.Attributes = System.IO.FileAttributes.NotContentIndexed | System.IO.FileAttributes.Hidden;
            }
        }

		/// <summary>
		/// Function to clean up the scratch area when the program exits.
		/// </summary>
        /// <returns>TRUE if the clean up operation was successful, FALSE if not.</returns>
		public static bool CleanUpScratchArea()
		{
			if (Program.Settings == null) 
			{
				throw new ApplicationException("The application settings were not loaded.");
			}
            			
			try
			{
                string scratchLocation = Settings.ScratchPath;

                // Use the write location of the currently mounted file system if possible.
                if ((ScratchFiles != null) && (!string.IsNullOrWhiteSpace(ScratchFiles.WriteLocation)))
                {
                    scratchLocation = ScratchFiles.WriteLocation;
                }

                // The scratch directory is gone, nothing to clean up.
                if (!Directory.Exists(scratchLocation))
                {
                    return true;
                }

				LogFile.Print("Cleaning up scratch area at \"{0}\".", LoggingLevel.Simple, scratchLocation);

				DirectoryInfo directory = new DirectoryInfo(scratchLocation);

				// Wipe out everything in this directory and the directory proper.
				directory.Delete(true);
			}
			catch (Exception ex)
			{
				GorgonException.Log = LogFile;
				GorgonException.Catch(ex);
				GorgonException.Log = Gorgon.Log;

                return false;
			}

            return true;
		}

		/// <summary>
		/// Function to initialize the graphics interface.
		/// </summary>
		/// <param name="control">Main content control.</param>
		public static void InitializeGraphics()
		{
			if (Graphics != null)
			{
				Graphics.Dispose();
				Graphics = null;
			}

            // Find the best device in the system.
            GorgonVideoDeviceEnumerator.Enumerate(false, false);

            GorgonVideoDevice bestDevice = GorgonVideoDeviceEnumerator.VideoDevices[0];

            // If we have more than one device, use the best available device.
            if (GorgonVideoDeviceEnumerator.VideoDevices.Count > 1)
            {
                bestDevice = (from device in GorgonVideoDeviceEnumerator.VideoDevices
                             orderby device.SupportedFeatureLevel descending, GorgonVideoDeviceEnumerator.VideoDevices.IndexOf(device)
                             select device).First();
            }

			//Graphics = new GorgonGraphics(DeviceFeatureLevel.SM2_a_b);
			Graphics = new GorgonGraphics(bestDevice);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="Program"/> class.
		/// </summary>
		static Program()
		{
			Settings = new GorgonEditorSettings();
            ContentPlugIns = new Dictionary<string, ContentPlugIn>();
            WriterPlugIns = new Dictionary<string, FileWriterPlugIn>();
            ChangedItems = new Dictionary<string, bool>();
            DisabledPlugIns = new Dictionary<EditorPlugIn, string>();
            EditorFileMetaData = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            BlockedFiles = new HashSet<string>();

            // Create our meta data root node.
            _metadataRootNode = new XElement("Gorgon.Editor.MetaData");
            EditorFileMetaData.Add(_metadataRootNode);

            // Add our list of blocked files.
            BlockedFiles.Add(MetaDataFile);

			EditorFile = "Untitled";
            EditorFilePath = string.Empty;
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				Settings.Load();
								
				Gorgon.Run(new AppContext());
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
				// Destroy the current content.
				if (CurrentContent != null)
				{
					CurrentContent.Dispose();
					CurrentContent = null;
				}

				// Shut down the graphics interface.
				if (Graphics != null)
				{
					Graphics.Dispose();
					Graphics = null;
				}

				// Clean up temporary files in scratch area.
				if (Program.Settings != null)
				{
					CleanUpScratchArea();
				}

				// Close the logging file.
				if (Program.LogFile != null)
				{
					Program.LogFile.Close();
				}
			}
		}
	}
}
