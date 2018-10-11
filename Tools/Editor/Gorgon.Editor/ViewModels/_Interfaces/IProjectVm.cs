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
// Created: September 4, 2018 11:11:06 AM
// 
#endregion

using System.IO;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The state of the project.
    /// </summary>
    internal enum ProjectState
    {
        /// <summary>
        /// Project is new.
        /// </summary>
        New = 0,
        /// <summary>
        /// Project is modified.
        /// </summary>
        Modified = 1,
        /// <summary>
        /// Project is unmodified.
        /// </summary>
        Unmodified = 2
    }

    /// <summary>
    /// The view model for interacting with the project data.
    /// </summary>
    internal interface IProjectVm
        : IViewModel
    {
        #region Variables.

        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the active clipboard handler context.
        /// </summary>
        IClipboardHandler ClipboardContext
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to show external items that are not included in the project file system.
        /// </summary>
        bool ShowExternalItems
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current state of the project.
        /// </summary>
        ProjectState ProjectState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the title for the project.
        /// </summary>
        string ProjectTitle
        {
            get;
        }

        /// <summary>
        /// Property to return the file information for the project if it was opened from a file.
        /// </summary>
        FileInfo ProjectFile
        {
            get;
        }

        /// <summary>
        /// Property to set or return the file explorer view model for use with the file explorer subview.
        /// </summary>
        IFileExplorerVm FileExplorer
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
