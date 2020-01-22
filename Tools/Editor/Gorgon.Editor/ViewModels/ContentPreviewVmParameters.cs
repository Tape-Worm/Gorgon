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
// Created: December 23, 2018 1:59:47 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.Editor.Content;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IContentPreviewVm"/> view model.
    /// </summary>
    internal class ContentPreviewVmParameters
        : ViewModelCommonParameters
    {
        #region Properties.
        /// <summary>
        /// Property to return the file explorer view model.
        /// </summary>
        public IFileExplorerVm FileExplorer
        {
            get;
        }

        /// <summary>
        /// Property to return the file manager for content.
        /// </summary>
        public OLDE_IContentFileManager ContentFileManager
        {
            get;
        }

        /// <summary>
        /// Property to return the thumbnail directory for the previewer.
        /// </summary>
        public DirectoryInfo ThumbDirectory
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ViewModels.ContentPreviewVmParameters"/> class.</summary>
        /// <param name="fileExplorer">The file explorer view model.</param>
        /// <param name="contentFileManager">The file manager used for content files.</param>
        /// <param name="thumbDirectory">The thumbnail directory for the previewer.</param>
        /// <param name="viewModelFactory">The view model factory for creating view models.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public ContentPreviewVmParameters(IFileExplorerVm fileExplorer, OLDE_IContentFileManager contentFileManager, DirectoryInfo thumbDirectory, ViewModelFactory viewModelFactory)
            : base(viewModelFactory)
        {
            FileExplorer = fileExplorer ?? throw new ArgumentNullException(nameof(fileExplorer));
            ContentFileManager = contentFileManager ?? throw new ArgumentNullException(nameof(contentFileManager));
            ThumbDirectory = thumbDirectory ?? throw new ArgumentNullException(nameof(thumbDirectory));
        }
        #endregion
    }
}
