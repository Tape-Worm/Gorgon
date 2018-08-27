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
// Created: August 29, 2018 8:16:04 PM
// 
#endregion

using System.IO;
using Gorgon.Editor.Metadata;

namespace Gorgon.Editor.ProjectData
{
    /// <summary>
    /// The project data used by the editor.
    /// </summary>
    public interface IProject
    {
        /// <summary>
        /// Property to set or return the name of the project.
        /// </summary>
        string ProjectName
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the workspace used by the project.
        /// </summary>
        /// <remarks>
        /// A project work space is a folder on the local file system that contains a copy of the project content. This folder is transitory, and will be cleaned up upon application exit.
        /// </remarks>
        DirectoryInfo ProjectWorkSpace
        {
            get;
        }


        /// <summary>
        /// Property to return the list of excluded paths.
        /// </summary>
        IProjectMetadata Metadata
        {
            get;
        }

    }
}
