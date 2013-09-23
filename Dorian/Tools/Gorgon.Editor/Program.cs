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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;

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
		/// Property to return the 2D renderer interface.
		/// </summary>
		public static Gorgon2D Renderer2D
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

            var file = ScratchArea.ScratchFiles.GetFile(MetaDataFilePath);
	        var extension = Path.GetExtension(path);

	        var firstPlugIn = (from plugIn in WriterPlugIns
                               where extension != null && plugIn.Value.FileExtensions.Contains(extension)
                               select plugIn.Value).FirstOrDefault();

            if (file == null)
            {
                // We don't have any meta data in this file, so use the first 
                // available plug-in.
                return firstPlugIn;
            }

            // Read in the meta data file.
			using (var stream = ScratchArea.ScratchFiles.OpenStream(file, false))
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
            if ((!plugInElement.HasAttributes)
                || (plugInElement.Attribute("TypeName") == null)
                || (string.IsNullOrWhiteSpace(plugInElement.Attribute("TypeName").Value)))
            {
                return firstPlugIn;
            }

            // Ensure we actually have the plug-in loaded.
            var writer = (from plugIn in WriterPlugIns
                let plugInType = plugIn.Value.GetType().FullName
                where string.Equals(plugInType, plugInElement.Attribute("TypeName").Value, StringComparison.OrdinalIgnoreCase)
                select plugIn.Value).FirstOrDefault();

            if (writer != null)
            {
                firstPlugIn = writer;
            }

            return firstPlugIn;
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
				ScratchArea.DestroyScratchArea();
				ScratchArea.InitializeScratch();

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
					ScratchArea.ScratchFiles.CreateDirectory(directory.FullPath);
				}

				// Copy our files.
				foreach (var file in files)
				{
					using (var inputStream = packFileSystem.OpenStream(file, false))
					{
						using (var outputStream = ScratchArea.ScratchFiles.OpenStream(file.FullPath, true))
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
				ScratchArea.DestroyScratchArea();
				ScratchArea.InitializeScratch();
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
			// Initialize the scratch area.
			ScratchArea.DestroyScratchArea();
			ScratchArea.InitializeScratch();

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
            using (var metaDataStream = ScratchArea.ScratchFiles.OpenStream(MetaDataFilePath, true))
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

				Gorgon.PlugIns.AssemblyResolver = (appDomain, e) =>
					{
						var assemblies = appDomain.GetAssemblies();

						// ReSharper disable once LoopCanBeConvertedToQuery
						// ReSharper disable once ForCanBeConvertedToForeach
						for (int i = 0; i < assemblies.Length; i++)
						{
							var assembly = assemblies[i];

							if (assembly.FullName == e.Name)
							{
								return assembly;
							}
						}
						return null;
					};

				Gorgon.Run(new AppContext());
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
			}
			finally
			{
                Gorgon.PlugIns.AssemblyResolver = null;

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
					ScratchArea.DestroyScratchArea();
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
