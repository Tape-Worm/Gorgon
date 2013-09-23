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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Interface used to import content files or load/save 
    /// </summary>
    static class FileManagement
    {
        #region Classes.
		/// <summary>
		/// A case insensitive string comparer.
		/// </summary>
	    private class StringOrdinalCaseInsensitiveComparer
			: IEqualityComparer<string>
		{
			#region IEqualityComparer<string> Members
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type string to compare.</param>
			/// <param name="y">The second object of type string to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			public bool Equals(string x, string y)
			{
				return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <param name="obj">The object.</param>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public int GetHashCode(string obj)
			{
				return obj.ToUpperInvariant().GetHashCode();
			}
			#endregion
		}

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
            /// <param name="x">The first object of type string to compare.</param>
            /// <param name="y">The second object of type string to compare.</param>
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
        private readonly static HashSet<string> _blockedFiles;
        private readonly static Dictionary<GorgonFileExtension, ContentPlugIn> _contentFiles;
        private readonly static Dictionary<GorgonFileExtension, GorgonFileSystemProvider> _readerFiles;
        private readonly static Dictionary<GorgonFileExtension, FileWriterPlugIn> _writerFiles;
        #endregion

        #region Properties.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the content file types available in the plug-in.
        /// </summary>
        /// <param name="plugIn">Plug-in containing the content types.</param>
        private static void GetContentFileTypes(ContentPlugIn plugIn)
        {
            if (plugIn.FileExtensions.Count == 0)
            {
                return;
            }

            // Associate the content file type with the plug-in.
            foreach (var extension in plugIn.FileExtensions)
            {
                _contentFiles[extension] = plugIn;
            }
        }

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
		/// Function to return whether or not a file is in the blocked list.
		/// </summary>
		/// <param name="file">File to check.</param>
		/// <returns>TRUE if blocked, FALSE if not.</returns>
	    public static bool IsBlocked(GorgonFileSystemFileEntry file)
		{
			return _blockedFiles.Contains(file.Name);
		}

        /// <summary>
        /// Function to initialize the file types available to the application.
        /// </summary>
        public static void InitializeFileTypes()
        {
            // Get reader extensions
            foreach (var readerProvider in ScratchArea.ScratchFiles.Providers)
            {
                GetReaderFileTypes(readerProvider);
            }

            // Get writer extensions
            foreach (var writerPlugIn in PlugIns.WriterPlugIns)
            {
                GetWriterFileTypes(writerPlugIn.Value);
            }

            // Get content extensions.
            foreach (var contentPlugIn in PlugIns.ContentPlugIns)
            {
                GetContentFileTypes(contentPlugIn.Value);
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
        /// Function to retrieve the list of available content extensions.
        /// </summary>
        /// <returns>A list of content file name extensions.</returns>
        public static IEnumerable<GorgonFileExtension> GetContentExtensions()
        {
            return _contentFiles.Keys;
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
        /// Function to find a writer plug-in for a given file name extension.
        /// </summary>
        /// <param name="fileName">Full file name or extension of the file to write.</param>
        /// <returns>The plug-in used to write the file.</returns>
        public static FileWriterPlugIn GetWriterPlugIn(string fileName)
        {
            XDocument tempMetaData = ScratchArea.GetMetaData();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            // Get the metadata file.
            var extension = new GorgonFileExtension(Path.GetExtension(fileName));

            // If we cannot find the extension, then return nothing.
            if ((string.IsNullOrWhiteSpace(extension.Extension)) || ((!_writerFiles.ContainsKey(extension) && (tempMetaData == null))))
            {
                return null;
            }

            FileWriterPlugIn plugInByExtension = _writerFiles[extension];

            // There's no metadata, so just rely on the extension.
            if (tempMetaData == null)
            {
                return plugInByExtension;
            }

            // Read the meta data for the file.
            var plugInElement = tempMetaData.Descendants("WriterPlugIn").FirstOrDefault();

            if (plugInElement == null)
            {
                return plugInByExtension;
            }

            // Ensure this is properly formed.
            if ((!plugInElement.HasAttributes)
                || (plugInElement.Attribute("TypeName") == null)
                || (string.IsNullOrWhiteSpace(plugInElement.Attribute("TypeName").Value)))
            {
                return plugInByExtension;
            }

            return (from plugIn in PlugIns.WriterPlugIns
                    where string.Equals(plugIn.Value.GetType().FullName,
                                        plugInElement.Attribute("TypeName").Value,
                                        StringComparison.OrdinalIgnoreCase)
                    select plugIn.Value).FirstOrDefault() ?? plugInByExtension;
        }

        /// <summary>
        /// Function to return a related plug-in for the given content file.
        /// </summary>
        /// <param name="fileName">Name of the content file.</param>
        /// <returns>The plug-in used to access the file.</returns>
        public static ContentPlugIn GetContentPlugInForFile(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            return !CanOpenContent(extension) ? null : _contentFiles[new GorgonFileExtension(extension, null)];
        }

        /// <summary>
        /// Function to determine if a certain type of content can be opened by a plug-in.
        /// </summary>
        /// <param name="fileName">Filename of the content.</param>
        /// <returns>TRUE if the content can be opened, FALSE if not.</returns>
        public static bool CanOpenContent(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            string extension = Path.GetExtension(fileName);

            return !string.IsNullOrWhiteSpace(extension) && _contentFiles.ContainsKey(new GorgonFileExtension(extension, null));
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes the <see cref="FileManagement"/> class.
        /// </summary>
        static FileManagement()
        {
            _contentFiles = new Dictionary<GorgonFileExtension, ContentPlugIn>(new FileExtensionComparer());
            _readerFiles = new Dictionary<GorgonFileExtension, GorgonFileSystemProvider>(new FileExtensionComparer());
            _writerFiles = new Dictionary<GorgonFileExtension, FileWriterPlugIn>(new FileExtensionComparer());

	        _blockedFiles = new HashSet<string>(new[]
	                                            {
		                                            ScratchArea.MetaDataFile
	                                            }, new StringOrdinalCaseInsensitiveComparer());
        }
        #endregion
    }
}
