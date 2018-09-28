#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: September 24, 2018 11:14:09 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.IO;
using Gorgon.IO.Providers;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Functionality to capture and load file system providers from plugins.
    /// </summary>
    internal interface IFileSystemProviders
    {
        #region Properties.
        /// <summary>
        /// Property to return a list of available reader provider extensions.
        /// </summary>
        IReadOnlyList<GorgonFileExtension> ReaderExtensions
        {
            get;
        }

        /// <summary>
        /// Property to return all loaded file system reader providers.
        /// </summary>
        IReadOnlyDictionary<string, IGorgonFileSystemProvider> Readers
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build a file system reader filter string for file dialogs.
        /// </summary>
        /// <returns>The string containing the file dialog filter.</returns>
        string GetReaderDialogFilterString();

        /// <summary>
        /// Function to add file system reader providers.
        /// </summary>
        /// <param name="providers">The list of providers to add.</param>
        void AddReaders(IEnumerable<IGorgonFileSystemProvider> providers);

        /// <summary>
        /// Function to find the most suitable provider for the file specified in the path.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns>The best suitable provider, or <b>null</b> if none could be located.</returns>
        IGorgonFileSystemProvider GetBestProvider(FileInfo file);
        #endregion
    }
}
