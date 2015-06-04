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
// Created: Monday, September 23, 2013 10:28:11 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Editor.Properties;
using Gorgon.IO;

namespace Gorgon.Editor
{
    /// <summary>
    /// Interface used to import content files or load/save the current editor file.
    /// </summary>
    static class FileManagement
    {
        #region Classes.
        /// <summary>
        /// A case insensitive comparer for file extensions.
        /// </summary>
        private class FileExtensionComparer
            : IEqualityComparer<GorgonFileExtension>
        {
            #region IEqualityComparer<FileExtension> Members
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type <see cref="GorgonFileExtension"/> to compare.</param>
			/// <param name="y">The second object of type <see cref="GorgonFileExtension"/> to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			public bool Equals(GorgonFileExtension x, GorgonFileExtension y)
            {
                return GorgonFileExtension.Equals(ref x, ref y);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(GorgonFileExtension obj)
            {
                return obj.GetHashCode();
            }
            #endregion
        }
        #endregion

		#region Variables.
		private readonly static Dictionary<GorgonFileExtension, GorgonFileSystemProvider> _readerFiles;
        private readonly static Dictionary<GorgonFileExtension, FileWriterPlugIn> _writerFiles;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the file has changed or not.
        /// </summary>
        public static bool FileChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the name of the file.
        /// </summary>
        public static string Filename
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the path to the file.
        /// </summary>
        public static string FilePath
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the file types that can be read by the available file system providers.
        /// </summary>
        /// <param name="provider">File system provider to evaluate.</param>
        private static void GetReaderFileTypes(GorgonFileSystemProvider provider)
        {
            foreach (var extension in provider.PreferredExtensions)
            {
                _readerFiles[extension] = provider;
            }
        }

        /// <summary>
        /// Function to retrieve the file types that can be written by the available file system writers.
        /// </summary>
        /// <param name="plugIn">File system writer plug-in.</param>
        private static void GetWriterFileTypes(FileWriterPlugIn plugIn)
        {
            foreach (var extension in plugIn.FileExtensions)
            {
	            _writerFiles[extension] = plugIn;
            }
        }

        /// <summary>
        /// Function to reset the file data.
        /// </summary>
        private static void ResetFile()
        {
            ScratchArea.DestroyScratchArea();
            ScratchArea.InitializeScratch();
			EditorMetaDataFile.Reset();
        }

        /// <summary>
        /// Function to initialize the file types available to the application.
        /// </summary>
        public static void InitializeFileTypes()
        {
            // Get reader extensions
            foreach (var readerProvider in ScratchArea.ScratchFiles.YeOldeProviders)
            {
                GetReaderFileTypes(readerProvider);
            }

            // Get writer extensions
            foreach (var writerPlugIn in PlugIns.WriterPlugIns)
            {
                GetWriterFileTypes(writerPlugIn.Value);
            }
        }

        /// <summary>
        /// Function to retrieve the list of file writer extensions.
        /// </summary>
        /// <returns>A list of writable file name extensions.</returns>
        public static IEnumerable<GorgonFileExtension> GetWriterExtensions()
        {
            return _writerFiles.Keys;
        }

        /// <summary>
        /// Function to retrieve the list of available reader extensions.
        /// </summary>
        /// <returns>A list of readable file name extensions.</returns>
        public static IEnumerable<GorgonFileExtension> GetReaderExtensions()
        {
            return _readerFiles.Keys;
        }

        /// <summary>
        /// Function to assign the specified writer plug-in as current in the meta data.
        /// </summary>
        /// <param name="plugIn">Plug-in to assign.</param>
        public static void SetWriterPlugIn(FileWriterPlugIn plugIn)
        {
	        if (plugIn == null)
	        {
		        EditorMetaDataFile.WriterPlugInType = string.Empty;

		        return;
	        }

			EditorMetaDataFile.WriterPlugInType = plugIn.Name;
        }

        /// <summary>
        /// Function to find a writer plug-in for a given file name extension.
        /// </summary>
        /// <param name="fileExtension">Full file name or extension of the file to write.</param>
        /// <param name="skipMetaData"><b>true</b> to skip the metadata check, <b>false</b> to use it.</param>
        /// <returns>The plug-in used to write the file.</returns>
        public static FileWriterPlugIn GetWriterPlugIn(string fileExtension = null, bool skipMetaData = false)
        {
			FileWriterPlugIn result;

			// If we have meta-data, then use that to determine which file writer is used.
	        if ((!skipMetaData)
				&& (!string.IsNullOrWhiteSpace(EditorMetaDataFile.WriterPlugInType)))
	        {
				result = (from plugIn in PlugIns.WriterPlugIns
						  where string.Equals(plugIn.Value.Name,
											  EditorMetaDataFile.WriterPlugInType,
											  StringComparison.OrdinalIgnoreCase)
						  select plugIn.Value).FirstOrDefault();

		        if (result != null)
		        {
			        return result;
		        }
		    }

            // We did not find a file writer in the meta data, try to derive which plug-in to use from the extension.
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                // We didn't give an extension, try and take it from the file name.
                fileExtension = Path.GetExtension(Path.GetExtension(Filename));
            }

            // If we passed in a full file name, then get its extension.
            if ((!string.IsNullOrWhiteSpace(fileExtension)) && (fileExtension.IndexOf('.') > 0))
            {
                fileExtension = Path.GetExtension(fileExtension);
            }

			// No extension?  Then likely there's no plug-in.
	        if (string.IsNullOrWhiteSpace(fileExtension))
	        {
		        return null;
	        }
            
            var extension = new GorgonFileExtension(fileExtension);
            
            // Try to find the plug-in.
            _writerFiles.TryGetValue(extension, out result);

            return result;
        }

        /// <summary>
        /// Function to create a new file.
        /// </summary>
        public static void New()
        {
            // Initialize the scratch area.
            ResetFile();

            Filename = "Untitled";
            FilePath = string.Empty;

            FileChanged = false;
        }

        /// <summary>
        /// Function to save the editor file.
        /// </summary>
        /// <param name="path">Path to the new file.</param>
        /// <param name="plugIn">The plug-in used to save the file.</param>
        public static void Save(string path, FileWriterPlugIn plugIn)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(APIResources.GOREDIT_ERR_PARAMETER_MUST_NOT_BE_EMPTY, "path");
            }

            // We don't have a writer plug-in, at this point, that's not good.
            if (plugIn == null)
            {
                throw new IOException(string.Format(APIResources.GOREDIT_ERR_NO_WRITER_PLUGIN, path));
            }

            // Write the meta data file to the file system.
			EditorMetaDataFile.Save();

            // Write the file.
            if (!plugIn.Save(path))
            {
                return;
            }

            Filename = Path.GetFileName(path);
            FilePath = path;

            // Remove all changed items.
            FileChanged = false;
        }

        /// <summary>
        /// Function to open the editor file.
        /// </summary>
        /// <param name="path">Path to the editor file.</param>
        public static void Open(string path)
        {
            var packFileSystem = new GorgonFileSystem(GorgonApplication.Log);

            FileChanged = false;

            // Add the new file system as a mount point.
	        var plugIns = from plugIn in GorgonApplication.PlugIns
	                      where plugIn is GorgonFileSystemProviderPlugIn 
						  && !PlugIns.IsDisabled(plugIn)
	                      select plugIn;

	        foreach (var plugIn in plugIns)
	        {
		        packFileSystem.YeOldeProviders.LoadProvider(plugIn.Name);
	        }

	        if (!packFileSystem.YeOldeProviders.Any(item => item.CanReadFile(path)))
            {
                throw new FileLoadException(string.Format(APIResources.GOREDIT_ERR_NO_READ_PROVIDERS,
                                                          Path.GetFileName(path)));
            }

            packFileSystem.Mount(path);

            try
            {
                // Remove our previous scratch data.
                ResetFile();

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
                
                FilePath = string.Empty;
                Filename = Path.GetFileName(path);

				// Load the meta data if it exists.
				EditorMetaDataFile.Load();

                // If we can't write the file, then leave the editor file path as blank.
                // If the file path is blank, then the Save As function will be triggered if we attempt to save so we 
                // can save it in a format that we DO understand.  This is of course assuming we have any plug-ins loaded
                // that will allow us to save.
                if (GetWriterPlugIn(path) != null)
                {
                    FilePath = path;
                }
            }
            catch
            {
                // We have a problem, reset whatever changes we've made.
                ResetFile();
                throw;
            }
            finally
            {
                // At this point we don't need this file system any more.  We'll 
                // be using our scratch system instead.
                packFileSystem.Clear();
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes the <see cref="FileManagement"/> class.
        /// </summary>
        static FileManagement()
        {
            _readerFiles = new Dictionary<GorgonFileExtension, GorgonFileSystemProvider>(new FileExtensionComparer());
            _writerFiles = new Dictionary<GorgonFileExtension, FileWriterPlugIn>(new FileExtensionComparer());

            Filename = "Untitled";
            FilePath = string.Empty;
        }
        #endregion
    }
}
