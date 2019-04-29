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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
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
        #region Properties.
        /// <summary>
        /// Property to return the available tool plug in button definitions for the application.
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> ToolButtons
        {
            get;
        }

        /// <summary>
        /// Property to set or return the layout for the window.
        /// </summary>
        byte[] Layout
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the active clipboard handler context.
        /// </summary>
        IClipboardHandler ClipboardContext
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
        /// Property to return the content file manager for managing content file systems through content plug ins.
        /// </summary>
        IContentFileManager ContentFileManager
        {
            get;
        }

        /// <summary>
        /// Property to return the file explorer view model for use with the file explorer subview.
        /// </summary>
        IFileExplorerVm FileExplorer
        {
            get;
        }

        /// <summary>
        /// Property to return the content previewer.
        /// </summary>
        IContentPreviewVm ContentPreviewer
        {
            get;
        }

        /// <summary>
        /// Property to return the current command context.
        /// </summary>
        string CommandContext
        {
            get;
        }

        /// <summary>
        /// Property to return the current content for the project.
        /// </summary>
        IEditorContent CurrentContent
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when the project is closing.
        /// </summary>
        IEditorAsyncCommand<CancelEventArgs> BeforeCloseCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a new content item.
        /// </summary>
        /// <param name="metadata">The metadata for the plug in associated with the content.</param>
        /// <param name="plugin">The plug in used to create the content.</param>
        /// <returns>A new content file containing the content data, or <b>null</b> if the content creation was cancelled.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/>, or the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
        Task<IContentFile> CreateNewContentItemAsync(IContentPlugInMetadata metadata, ContentPlugIn plugin);

        /// <summary>
        /// Function to persist the project data to a file.
        /// </summary>
        /// <param name="path">A path to the file that will hold the project data.</param>
        /// <param name="writer">The plug in used to write the project data.</param>
        /// <param name="progressCallback">The callback method that reports the saving progress to the UI.</param>
        /// <param name="cancelToken">The token used for cancellation of the operation.</param>
        /// <returns>A task for asynchronous operation.</returns>
        Task SaveToPackFileAsync(FileInfo path, FileWriterPlugIn writer, Action<int, int, bool> progressCallback,  CancellationToken cancelToken);

        /// <summary>
        /// Function to persist the project metadata to the disk.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        Task SaveProjectMetadataAsync();
        #endregion
    }
}
