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
// Created: May 7, 2019 9:10:31 AM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.TextureAtlasTool
{
    /// <summary>
    /// A view model for the sprite file explorer.
    /// </summary>
    internal interface ISpriteFiles
        : IViewModel
    {
        /// <summary>
        /// Property to return the image used for previewing.
        /// </summary>
        IGorgonImage PreviewImage
        {
            get;
        }

        /// <summary>
        /// Property to return the sprite file entries.
        /// </summary>
        IReadOnlyList<ContentFileExplorerDirectoryEntry> SpriteFileEntries
        {
            get;
        }

        /// <summary>
        /// Property to return the list of selected files.
        /// </summary>
        IReadOnlyList<ContentFileExplorerFileEntry> SelectedFiles
        {
            get;
        }

        /// <summary>
        /// Property to return the command that will refresh the sprite preview data.
        /// </summary>
        IEditorAsyncCommand<IReadOnlyList<ContentFileExplorerFileEntry>> RefreshSpritePreviewCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to search through the file list.
        /// </summary>
        IEditorCommand<string> SearchCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command used confirm loading of the sprite files.
        /// </summary>
        IEditorCommand<object> ConfirmLoadCommand
        {
            get;
            set;
        }
    }
}
