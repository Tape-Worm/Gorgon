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
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;
using GorgonLibrary.Editor.Properties;

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
		private static Guid _scratchID = Guid.NewGuid();
		private static readonly string[] _systemDirs;
        private static readonly XElement _metadataRootNode;
        private static FileWriterPlugIn _writerPlugIn;
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
		/// Property to set or return whether the editor file has been changed.
		/// </summary>
		public static bool EditorFileChanged
		{
			get;
			set;
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
	        var extension = Path.GetExtension(path);

	        var firstPlugIn = (from plugIn in WriterPlugIns
                               where extension != null && plugIn.Value.FileExtensions.ContainsKey(extension.ToLower())
                               select plugIn.Value).FirstOrDefault();

            if (file == null)
            {
                // We don't have any meta data in this file, so use the first 
                // available plug-in.
                return firstPlugIn;
            }

            // Read in the meta data file.
            using (var stream = ScratchFiles.OpenStream(file, false))
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
                              where String.Compare(plugInType, plugInElement.Attribute("TypeName").Value, StringComparison.OrdinalIgnoreCase) == 0
                              select plugIn.Value).FirstOrDefault();

                if (writer != null)
                {
                    firstPlugIn = writer;
                }
            }

            return firstPlugIn;
        }

		/// <summary>
		/// Function to determine if the path is a root path or system location.
		/// </summary>
		/// <param name="path">Path to evaluate.</param>
		/// <returns>TRUE if a system location or root directory.</returns>
		public static bool IsSystemLocation(string path)
		{
			var sysRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
			var info = new DirectoryInfo(Path.GetFullPath(path));

			// Don't allow the root of the system drive as a scratch area.
			if ((info.Parent == null) && (string.Compare(sysRoot, info.FullName, StringComparison.OrdinalIgnoreCase) == 0))
			{
				return true;
			}

			// Ensure the system files are not accessible.
			return (_systemDirs.Any(item => String.Compare(path, item, StringComparison.OrdinalIgnoreCase) == 0));
		}

		/// <summary>
        /// Function to open the editor file.
        /// </summary>
        /// <param name="path">Path to the editor file.</param>
        public static void OpenEditorFile(string path)
        {
			var packFileSystem = new GorgonFileSystem();

			EditorFileChanged = false;

			try
			{
				// Remove our scratch data.
				CleanUpScratchArea();
				InitializeScratch();

				// Add the new file system as a mount point.
				packFileSystem.Providers.LoadAllProviders();
				packFileSystem.Mount(path);

				// At this point we should have a clean scratch area, so all files will exist in the packed file.
				// Unpack the file structure so we can work with it.
				var directories = packFileSystem.FindDirectories("*", true);
				var files = packFileSystem.FindFiles("*", true);

				// Create our directories.
				foreach (var directory in directories)
				{
					ScratchFiles.CreateDirectory(directory.FullPath);
				}

				// Copy our files.
				foreach (var file in files)
				{
					using (var inputStream = packFileSystem.OpenStream(file, false))
					{
						using (var outputStream = ScratchFiles.OpenStream(file.FullPath, true))
						{
							inputStream.CopyTo(outputStream);
						}
					}
				}

				// At this point we have no 
				EditorFilePath = string.Empty;
				EditorFile = Path.GetFileName(path);
				Settings.LastEditorFile = path;

				// Find the writer plug-in that can write this file.
				CurrentWriterPlugIn = GetWriterPlugIn(path);

				// If we can't write the file, then leave the editor file path as blank.
				if (CurrentWriterPlugIn != null)
				{
					EditorFilePath = path;
				}
			}
			catch
			{
				// We have a problem, reset whatever changes we've made.
				CleanUpScratchArea();
				InitializeScratch();
				throw;
			}
			finally
			{
				// At this point we don't need this file system any more.  We'll 
				// be using our scratch system instead.
				packFileSystem.Clear();
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

			EditorFile = "Untitled";
			EditorFilePath = string.Empty;
			_metadataRootNode.Descendants().Remove();

			Settings.LastEditorFile = string.Empty;

			EditorFileChanged = false;
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
                throw new IOException(string.Format(Resources.GOREDIT_NO_WRITER_PLUGIN, path));                
            }

			// Save any outstanding edits on the current content.
			if ((CurrentContent != null) && (CurrentContent.HasChanges))
			{
				CurrentContent.Persist(CurrentContent.File);
			}

            // Write the meta data file to the file system.
            using (var metaDataStream = ScratchFiles.OpenStream(MetaDataFilePath, true))
            {
                EditorFileMetaData.Save(metaDataStream);
            }

            // Write the file.
			if (!CurrentWriterPlugIn.Save(path))
			{
				return;
			}
            
            EditorFile = Path.GetFileName(path);
            EditorFilePath = path;
			Settings.LastEditorFile = path;

            // Remove all changed items.
			EditorFileChanged = false;
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
				dialog = new FolderBrowserDialog
					{
						Description = Resources.GOREDIT_SCRATCH_PATH_DLG_DESC
					};

				if (Directory.Exists(Settings.ScratchPath))
				{
					dialog.SelectedPath = Settings.ScratchPath;
				}
				dialog.ShowNewFolderButton = true;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					// We chose poorly.
					if (IsSystemLocation(dialog.SelectedPath))
					{
						GorgonDialogs.ErrorBox(null, Resources.GOREDIT_CANNOT_USESYS_SCRATCH);
						return null;
					}

					// Append the scratch directory.
					return dialog.SelectedPath.FormatDirectory(Path.DirectorySeparatorChar);
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
			_scratchID = Guid.NewGuid();
			ScratchFiles.Clear();
            ScratchFiles.WriteLocation = Settings.ScratchPath + ("Gorgon.Editor." + _scratchID.ToString("N")).FormatDirectory(Path.DirectorySeparatorChar); 

            // Set the directory to hidden.  We don't want users really messing around in here if we can help it.
            var scratchDir = new DirectoryInfo(ScratchFiles.WriteLocation);
            if (((scratchDir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                || ((scratchDir.Attributes & FileAttributes.NotContentIndexed) != FileAttributes.NotContentIndexed))
            {
                scratchDir.Attributes = FileAttributes.NotContentIndexed | FileAttributes.Hidden;
            }
        }

		/// <summary>
		/// Function to clean up the scratch area when the program exits.
		/// </summary>
        /// <returns>TRUE if the clean up operation was successful, FALSE if not.</returns>
		public static bool CleanUpScratchArea()
		{
			if (Settings == null) 
			{
				throw new ApplicationException(Resources.GOREDIT_NO_APP_SETTINGS);
			}
            			
			try
			{
                string scratchLocation = Settings.ScratchPath + _scratchID.ToString("N").FormatDirectory(Path.DirectorySeparatorChar);

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

				var directory = new DirectoryInfo(scratchLocation);

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
                        
			Graphics = new GorgonGraphics(bestDevice);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="Program"/> class.
		/// </summary>
		static Program()
		{
			_systemDirs = ((Environment.SpecialFolder[])Enum.GetValues(typeof(Environment.SpecialFolder)))
							.Select(Environment.GetFolderPath)
							.ToArray();
			Settings = new GorgonEditorSettings();
            ContentPlugIns = new Dictionary<string, ContentPlugIn>();
            WriterPlugIns = new Dictionary<string, FileWriterPlugIn>();
			EditorFileChanged = false;
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
				if (Settings != null)
				{
					CleanUpScratchArea();
				}

				// Close the logging file.
				if (LogFile != null)
				{
					LogFile.Close();
				}
			}
		}
	}
}
