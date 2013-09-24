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
// Created: Tuesday, September 24, 2013 10:37:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
    /// <summary>
    /// Interface for content management.
    /// </summary>
    static class ContentManagement
    {
        #region Variables.
        private readonly static Dictionary<GorgonFileExtension, ContentPlugIn> _contentFiles;
        #endregion

        #region Properties.

        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the list of available content extensions.
        /// </summary>
        /// <returns>A list of content file name extensions.</returns>
        public static IEnumerable<GorgonFileExtension> GetContentExtensions()
        {
            return _contentFiles.Keys;
        }

        /// <summary>
        /// Function to return a related plug-in for the given content file.
        /// </summary>
        /// <param name="fileExtension">Name of the content file.</param>
        /// <returns>The plug-in used to access the file.</returns>
        public static ContentPlugIn GetContentPlugInForFile(string fileExtension)
        {
            if (fileExtension.IndexOf('.') > 0)
            {
                fileExtension = Path.GetExtension(fileExtension);
            }

            return !CanOpenContent(fileExtension) ? null : _contentFiles[new GorgonFileExtension(fileExtension, null)];
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

            if (fileName.IndexOf('.') > 0)
            {
                fileName = Path.GetExtension(fileName);
            }

            return !string.IsNullOrWhiteSpace(fileName) && _contentFiles.ContainsKey(new GorgonFileExtension(fileName, null));
        }

        /// <summary>
        /// Function used to initialize the file types for the content types.
        /// </summary>
        public static void InitializeContentFileTypes()
        {
            // Get content extensions.
            foreach (var contentPlugIn in PlugIns.ContentPlugIns)
            {
                if (contentPlugIn.Value.FileExtensions.Count == 0)
                {
                    continue;
                }

                // Associate the content file type with the plug-in.
                foreach (var extension in contentPlugIn.Value.FileExtensions)
                {
                    _contentFiles[extension] = contentPlugIn.Value;
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes the <see cref="ContentManagement"/> class.
        /// </summary>
        static ContentManagement()
        {
            _contentFiles = new Dictionary<GorgonFileExtension, ContentPlugIn>();
        }
        #endregion
    }
}
