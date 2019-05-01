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
// Created: September 5, 2018 1:49:40 PM
// 
#endregion

using System;
using System.IO;
using System.Text;

namespace Gorgon.IO
{
    /// <summary>
    /// Extension methods for IO functionality.
    /// </summary>
    public static class IOExtensions
    {
        /// <summary>
        /// Function to convert a file system information object into a relative file system path.
        /// </summary>
        /// <param name="filesystemObject">The file system object containing the path to convert.</param>
        /// <param name="rootDirectory">The physical file system directory that is the root of the relative file system.</param>
        /// <param name="pathSeparator">[Optional] The desired path separator.</param>
        /// <returns>The path to the file system object, relative to the root of the file system. Or, an empty string if the file system object is not on the path of the <paramref name="rootDirectory"/>.</returns>
        public static string ToFileSystemPath(this FileSystemInfo filesystemObject, DirectoryInfo rootDirectory, char pathSeparator = '/')
        {
            if (filesystemObject == null)
            {
                throw new ArgumentNullException(nameof(filesystemObject));
            }

            if (rootDirectory == null)
            {
                throw new ArgumentNullException(nameof(rootDirectory));
            }

            if (!filesystemObject.FullName.StartsWith(rootDirectory.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string fileName = null;
            string dirPath;
            bool isDirectory = ((filesystemObject.Attributes & FileAttributes.Directory) == FileAttributes.Directory) && (filesystemObject is DirectoryInfo);

            if (isDirectory)
            {
                dirPath = filesystemObject.FullName.FormatDirectory(Path.DirectorySeparatorChar);
            }
            else
            {
                dirPath = Path.GetDirectoryName(filesystemObject.FullName).FormatDirectory(Path.DirectorySeparatorChar);
                fileName = filesystemObject.Name;
            }

            string rootDirPath = rootDirectory.FullName.FormatDirectory(Path.DirectorySeparatorChar);

            if ((string.Equals(dirPath, rootDirPath, StringComparison.OrdinalIgnoreCase)) && (isDirectory))
            {
                return pathSeparator.ToString();
            }

            var pathBuilder = new StringBuilder();
            pathBuilder.Append(pathSeparator.ToString());
            pathBuilder.Append(dirPath, rootDirPath.Length, dirPath.Length - rootDirPath.Length);
            if (pathSeparator != Path.DirectorySeparatorChar)
            {
                pathBuilder.Replace(Path.DirectorySeparatorChar, pathSeparator);
            }            

            return string.IsNullOrWhiteSpace(fileName) ? pathBuilder.ToString() : Path.Combine(pathBuilder.ToString(), fileName);
        }
    }
}
