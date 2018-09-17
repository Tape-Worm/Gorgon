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
// Created: September 15, 2018 9:18:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Editor.Metadata
{
    /// <summary>
    /// Manages metadata for a project.
    /// </summary>
    public interface IMetadataManager
    {
        /// <summary>
        /// Function to rename items in meta data.
        /// </summary>
        /// <param name="oldPath">The old path for the items.</param>
        /// <param name="newPath">The new path for the items.</param>
        void RenameIncludedPaths(string oldPath, string newPath);

        /// <summary>
        /// Function to exclude the specified paths in the project metadata.
        /// </summary>
        /// <param name="paths">The list of paths to evaluate.</param>
        void ExcludePaths(IReadOnlyList<string> paths);

        /// <summary>
        /// Function to include the specified paths in the project metadata.
        /// </summary>
        /// <param name="paths">The list of paths to evaluate.</param>        
        void IncludePaths(IReadOnlyList<string> paths);

        /// <summary>
        /// Function to remove the specified path from the included file/directory metadata.
        /// </summary>
        /// <param name="path">The path to evaluate.</param>
        void DeleteFromIncludeMetadata(string path);

        /// <summary>
        /// Function to determine if the path is in the project or not.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><b>true</b> if the path is included in the project, <b>false</b> if not.</returns>
        bool PathInProject(string path);

        /// <summary>
        /// Function to retrieve all directories included in the project under the root path.
        /// </summary>
        /// <param name="rootPath">The root path to evaluate.</param>
        /// <returns>A list of included directories.</returns>
        IEnumerable<DirectoryInfo> GetIncludedDirectories(string rootPath);

        /// <summary>
        /// Function to retrieve all file included in the project under the root path.
        /// </summary>
        /// <param name="rootPath">The root path to evaluate.</param>
        /// <returns>A list of included files.</returns>
        IEnumerable<FileInfo> GetIncludedFiles(string rootPath);
    }
}
