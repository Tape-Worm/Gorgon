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
// Created: October 10, 2018 8:46:12 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Core;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Arguments to pass to the <see cref="IFileSystemService.ImportIntoDirectoryAsync(ImportArgs, System.Threading.CancellationToken)"/> method.
    /// </summary>
    internal class ImportArgs
    {
        /// <summary>
        /// Property to return the common root directory of the items to import.
        /// </summary>
        public DirectoryInfo ImportRoot
        {
            get;
        }

        /// <summary>
        /// Property to return the relative paths to the import root path to import.
        /// </summary>
        public IReadOnlyList<string> Items
        {
            get;
        }

        /// <summary>
        /// Property to return the destination directory that will receive the files and sub directories.
        /// </summary>
        public DirectoryInfo DestinationDirectory
        {
            get;
        }

        /// <summary>
        /// Property to set or return the callback used to indicate the import progress.
        /// </summary>
        public Action<FileSystemInfo, FileSystemInfo, int, int> OnImportFile
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the callback used to handle file conflicts.
        /// </summary>
        public Func<FileSystemInfo, FileSystemInfo, FileSystemConflictResolution> ConflictResolver
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportArgs"/> class.
        /// </summary>
        /// <param name="items">The relative paths to the import root path to import.</param>
        /// <param name="destination">The destination path to copy into.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="items"/>, or the <paramref name="destination"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="items"/>, or the <paramref name="destination"/> parameter is empty.</exception>
        public ImportArgs(IReadOnlyList<string> items, string destination)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (string.IsNullOrWhiteSpace(destination))
            {
                throw new ArgumentEmptyException(nameof(destination));
            }

            if (items.Count == 0)
            {
                throw new ArgumentEmptyException(nameof(items));
            }

            string firstEntry = items.OrderBy(item => item.Length).FirstOrDefault(item => !string.IsNullOrWhiteSpace(item));

            if (string.IsNullOrWhiteSpace(firstEntry))
            {                
                throw new DirectoryNotFoundException();
            }

            firstEntry = Path.GetDirectoryName(firstEntry);

            if (string.IsNullOrWhiteSpace(firstEntry))
            {
                throw new DirectoryNotFoundException();
            }

            ImportRoot = new DirectoryInfo(firstEntry);
            DestinationDirectory = new DirectoryInfo(destination);
            Items = items;
        }
    }
}
