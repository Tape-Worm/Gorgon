#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 28, 2019 11:11:43 AM
// 
#endregion

using Gorgon.Editor.Services;
using Gorgon.Editor.ViewModels;

namespace Gorgon.Editor.FileSystem
{
    /// <summary>
    /// An interface used to browse the file system folder structure.
    /// </summary>
    internal interface IEditorFileSystemFolderBrowseService
		: IFileSystemFolderBrowseService
    {
		/// <summary>
        /// Property to set or return the file system for the currently loaded project.
        /// </summary>
		IFileExplorerVm FileSystem
        {
            get;
            set;
        }
    }
}
