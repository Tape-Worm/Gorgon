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
// Created: October 29, 2018 3:49:26 PM
// 
#endregion

using System;
using Gorgon.Editor.Content;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// The flags used to identify the current content state.
    /// </summary>
    public enum ContentState
    {
        /// <summary>
        /// Content is unmodified.
        /// </summary>
        Unmodified = 0,
        /// <summary>
        /// Content is new.
        /// </summary>
        New = 1,
        /// <summary>
        /// Content is modified.
        /// </summary>
        Modified = 2
    }

    /// <summary>
    /// The reason why the application is saving the content.
    /// </summary>
    public enum SaveReason
    {
        /// <summary>
        /// The user hit the save button.
        /// </summary>
        UserSave = 0,
        /// <summary>
        /// The application or project is shutting down.
        /// </summary>
        AppProjectShutdown = 1,
        /// <summary>
        /// The content is closing.
        /// </summary>
        ContentShutdown = 2
    }

    /// <summary>
    /// Common interface for editor content view models.
    /// </summary>
    public interface IEditorContent
        : IViewModel
    {
        #region Events.
        /// <summary>
        /// Event to notify the view that the content should close.
        /// </summary>
        event EventHandler CloseContent;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the current content state.
        /// </summary>
        ContentState ContentState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the content file.
        /// </summary>
        IContentFile File
        {
            get;
        }

        /// <summary>
        /// Property to return the type of content.
        /// </summary>
        string ContentType
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command used to close the content.
        /// </summary>
        IEditorCommand<CloseContentArgs> CloseContentCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the command used to save the content.
        /// </summary>
        IEditorAsyncCommand<SaveReason> SaveContentCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the current context for commands from this content.
        /// </summary>
        string CommandContext
        {
            get;
            set;
        }
        #endregion
    }
}
