﻿#region MIT
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
// Created: September 4, 2018 10:16:21 PM
// 
#endregion

using System;
using System.Collections.ObjectModel;
using Gorgon.Editor.Content;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for our file explorer.
    /// </summary>
    internal interface IFileExplorerVm
        : IViewModel, IDragDropHandler<IFileExplorerNodeDragData>, IDragDropHandler<IExplorerFilesDragData>
    {
        #region Events.
        /// <summary>
        /// Event triggered when the file system is changed.
        /// </summary>
        event EventHandler FileSystemChanged;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to show excluded nodes or not.
        /// </summary>
        bool ShowExcluded
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the currently selected node for the file system.
        /// </summary>
        IFileExplorerNodeVm SelectedNode
        {
            get;
        }

        /// <summary>
        /// Property to return the root node for the file system.
        /// </summary>
        IFileExplorerNodeVm RootNode
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when including or excluding all nodes.
        /// </summary>
        IEditorCommand<IncludeExcludeArgs> IncludeExcludeAllCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when including or excluding a node.
        /// </summary>
        IEditorCommand<IncludeExcludeArgs> IncludeExcludeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when a node is selected.
        /// </summary>
        IEditorCommand<IFileExplorerNodeVm> SelectNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when creating a node.
        /// </summary>
        IEditorCommand<CreateNodeArgs> CreateNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to rename the selected node.
        /// </summary>
        IEditorCommand<FileExplorerNodeRenameArgs> RenameNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to delete the selected node.
        /// </summary>
        IEditorCommand<DeleteNodeArgs> DeleteNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to copy a single node.
        /// </summary>
        IEditorCommand<CopyNodeArgs> CopyNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to move a single node.
        /// </summary>
        IEditorCommand<CopyNodeArgs> MoveNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to refresh the specified node children.
        /// </summary>
        IEditorCommand<IFileExplorerNodeVm> RefreshNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to delete the file system.
        /// </summary>
        IEditorCommand<object> DeleteFileSystemCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to export a node's contents to the physical file system
        /// </summary>
        IEditorCommand<IFileExplorerNodeVm> ExportNodeToCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to import files and directories into the specified node.
        /// </summary>
        IEditorAsyncCommand<IFileExplorerNodeVm> ImportIntoNodeCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command to execute when a content node is opened.
        /// </summary>
        IEditorCommand<IContentFile> OpenContentFile
        {
            get;
            set;
        }
        #endregion
    }
}
