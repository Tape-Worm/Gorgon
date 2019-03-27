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
// Created: March 14, 2019 11:39:34 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Tool states for the sprite editor.
    /// </summary>
    internal enum SpriteEditTool
    {
        None = 0,
        SpriteClip = 1,
        SpritePick = 2,
        CornerResize = 3,
        CornerColor = 4,
        TextureResize = 5,
        SetAnchor = 6
    }

    /// <summary>
    /// The type of sub panel to make active.
    /// </summary>
    internal enum EditorSubPanel
    {
        None = 0,
        SpriteClipManualInput = 1
    }

    /// <summary>
    /// The view model for sprite content.
    /// </summary>
    internal interface ISpriteContent
        : IEditorContent, IUndoHandler, IDragDropHandler<IContentFileDragData>
    {
        #region Properties.
        /// <summary>
        /// Property to return the view model for the plug in settings.
        /// </summary>
        ISettings Settings
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the manual input interface.
        /// </summary>
        IManualRectInputVm ManualInput
        {
            get;
        }

        /// <summary>
        /// Property to set or return the sub panel to make active.
        /// </summary>
        EditorSubPanel ActiveSubPanel
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the buffer that contains the image data for the <see cref="Texture"/>, at the <see cref="ArrayIndex"/> associated with the sprite.
        /// </summary>
        IGorgonImageBuffer ImageData
        {
            get;
        }

        /// <summary>
        /// Property to return the texture associated with the sprite.
        /// </summary>
        GorgonTexture2DView Texture
        {
            get;
        }

        /// <summary>
        /// Property to set or return the texture coordinates used by the sprite.
        /// </summary>
        DX.RectangleF TextureCoordinates
        {
            get;
        }

        /// <summary>
        /// Property to return the size of the sprite.
        /// </summary>
        DX.Size2F Size
        {
            get;
        }

        /// <summary>
        /// Property to return the index of the texture array that the sprite uses.
        /// </summary>
        int ArrayIndex
        {
            get;
        }

        /// <summary>
        /// Property to return whether the currently loaded texture supports array changes.
        /// </summary>
        bool SupportsArrayChange
        {
            get;
        }

        /// <summary>
        /// Property to set or return the currently active tool for editing the sprite.
        /// </summary>
        SpriteEditTool CurrentTool
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the command to execute when applying texture coordinates to the sprite.
        /// </summary>
        /// <remarks>
        /// The command takes a tuple containing the texture coordinate values, and the texture array index to use.
        /// </remarks>
        IEditorCommand<(DX.RectangleF, int)> SetTextureCoordinatesCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when picking a sprite.
        /// </summary>
        IEditorCommand<object> SpritePickCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when creating a new sprite.
        /// </summary>
        IEditorCommand<object> NewSpriteCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to extract the image data from a sprite.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        Task ExtractImageDataAsync();
        #endregion
    }
}
