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
// Created: December 22, 2018 11:36:11 PM
// 
#endregion

using System.IO;
using Gorgon.Editor.Content;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ViewModels
{
    /// <summary>
    /// The view model for the content preview window.
    /// </summary>
    internal interface IContentPreviewVm
        : IViewModel
    {
        /// <summary>
        /// Property to return the preview image to display.
        /// </summary>
        IGorgonImage PreviewImage
        {
            get;
        }

        /// <summary>
        /// Property to return the title for the previewed content.
        /// </summary>
        string Title
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to refresh the preview image.
        /// </summary>
        IEditorAsyncCommand<IContentFile> RefreshPreviewCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to reset the preview back to its inital state.
        /// </summary>
        IEditorAsyncCommand<object> ResetPreviewCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the preview is enabled.
        /// </summary>
        bool IsEnabled
        {
            get;
            set;
        }
    }
}
