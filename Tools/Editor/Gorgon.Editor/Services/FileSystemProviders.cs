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
// Created: September 24, 2018 11:05:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.IO.Providers;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Functionality to capture and load file system providers from plugins.
    /// </summary>
    internal class FileSystemProviders
        : IFileSystemProviders
    {
        #region Variables.
        // The list of available file extensions that are supported.
        private List<GorgonFileExtension> _readerExtensions = new List<GorgonFileExtension>();
        // A list of available file system reader providers.
        private readonly Dictionary<string, IGorgonFileSystemProvider> _readers = new Dictionary<string, IGorgonFileSystemProvider>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return a list of available reader provider extensions.
        /// </summary>
        public IReadOnlyList<GorgonFileExtension> ReaderExtensions => _readerExtensions;
                
        /// <summary>
        /// Property to return all loaded file system reader providers.
        /// </summary>
        public IReadOnlyDictionary<string, IGorgonFileSystemProvider> Readers => _readers;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the file system provider that supports the specified file.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns>The file system provider that can read the file.</returns>
        private IGorgonFileSystemProvider GetBestFit(FileInfo file)
        {
            foreach (IGorgonFileSystemProvider provider in _readers.Values)
            {
                if (provider.CanReadFileSystem(file.FullName))
                {
                    return provider;
                }
            }

            return null;
        }

        /// <summary>
        /// Function to find the most suitable provider for the file specified in the path.
        /// </summary>
        /// <param name="file">The file to evaluate.</param>
        /// <returns>The best suitable provider, or <b>null</b> if none could be located.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="file"/> parameter is <b>null</b>.</exception>
        public IGorgonFileSystemProvider GetBestProvider(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // First, try to locate by extension.
            IGorgonFileSystemProvider result = _readers.Select(item => item.Value)
                .FirstOrDefault(item => item.PreferredExtensions.Contains(file.Extension));

            // No provider is registered with that extension, fall back to trying to read each file.
            if (result == null)
            {
                return GetBestFit(file);
            }
            else
            {
                if (!result.CanReadFileSystem(file.FullName))
                {
                    return GetBestFit(file);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to build a file system reader filter string for file dialogs.
        /// </summary>
        /// <returns>The string containing the file dialog filter.</returns>
        public string GetReaderDialogFilterString()
        {
            var result = new StringBuilder();
            var filter = new StringBuilder();
            var allFilter = new StringBuilder();

            foreach (KeyValuePair<string, IGorgonFileSystemProvider> provider in _readers.OrderBy(item => item.Value.Description))
            {
                filter.Length = 0;

                if (provider.Value.PreferredExtensions.Count == 0)
                {
                    continue;
                }

                string description = provider.Value.PreferredExtensions.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.Description)).Description;

                if (string.IsNullOrWhiteSpace(description))
                {
                    continue;
                }

                if (result.Length > 0)
                {
                    result.Append("|");
                }

                result.Append(description);

                foreach (GorgonFileExtension extension in provider.Value.PreferredExtensions)
                {
                    if (allFilter.Length > 0)
                    {
                        allFilter.Append(";");
                    }

                    if (filter.Length > 0)
                    {
                        filter.Append(";");
                    }

                    filter.Append("*.");
                    filter.Append(extension.Extension);
                    allFilter.Append("*.");
                    allFilter.Append(extension.Extension);
                }

                result.Append(" (");
                result.Append(filter);
                result.Append(")|");
                result.Append(filter);
            }

            if (allFilter.Length > 0)
            {
                if (result.Length > 0)
                {
                    result.Append("|");
                }

                result.Append(string.Format(Resources.GOREDIT_TEXT_SUPPORTED_FILES, allFilter));
            }

            if (result.Length > 0)
            {
                result.Append("|");
            }
            
            result.Append(Resources.GOREDIT_TEXT_ALL_FILES);

            return result.ToString();
        }

        /// <summary>
        /// Function to add file system reader providers.
        /// </summary>
        /// <param name="providers">The list of providers to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="providers"/> parameter is <b>null</b>.</exception>
        public void AddReaders(IEnumerable<IGorgonFileSystemProvider> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException(nameof(providers));
            }

            foreach (IGorgonFileSystemProvider provider in providers)
            {
                _readers[provider.GetType().FullName] = provider;
            }
        }
        #endregion
    }
}
