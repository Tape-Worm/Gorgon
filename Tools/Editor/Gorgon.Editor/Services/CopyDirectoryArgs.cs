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
// Created: October 11, 2018 2:56:31 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;
using Gorgon.IO;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Arguments to pass to the <see cref="IFileSystemService.CopyDirectoryAsync(CopyDirectoryArgs, System.Threading.CancellationToken)"/> method.
    /// </summary>
    internal class CopyDirectoryArgs
    {
        /// <summary>
        /// Property to return the directory to use as the source.
        /// </summary>
        public DirectoryInfo SourceDirectory
        {
            get;
        }

        /// <summary>
        /// Property to return the directory to use as the destination.
        /// </summary>
        public DirectoryInfo DestinationDirectory
        {
            get;
        }

        /// <summary>
        /// Property to set or return the action to execute to report progress for the copy.
        /// </summary>
        public Action<FileSystemInfo, FileSystemInfo, int, int> OnCopyProgress
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the function to execute to handle a file conflict.
        /// </summary>
        public Func<FileSystemInfo, FileSystemInfo, FileSystemConflictResolution> OnResolveConflict
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDirectoryArgs"/> class.
        /// </summary>
        /// <param name="sourceDirectory">The path to the source directory to copy.</param>
        /// <param name="destDirectory">The path to the destination directory that will receive the copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceDirectory"/>, or the <paramref name="destDirectory"/> parameter is <b>null</b>.</exception>
        public CopyDirectoryArgs(DirectoryInfo sourceDirectory, DirectoryInfo destDirectory)
        {
            if (sourceDirectory == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            if (destDirectory == null)
            {
                throw new ArgumentNullException(nameof(destDirectory));
            }

            SourceDirectory = sourceDirectory;
            DestinationDirectory = destDirectory;
        }
    }
}
