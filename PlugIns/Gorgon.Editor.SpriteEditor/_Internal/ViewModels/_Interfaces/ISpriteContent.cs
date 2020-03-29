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

using System.Collections.Generic;
using System.Threading.Tasks;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using DX = SharpDX;

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
        CornerResize = 3
    }

    /// <summary>
    /// The view model for sprite content.
    /// </summary>
    internal interface ISpriteContent
        : IVisualEditorContent, IUndoHandler
    {
        #region Properties.
        /*/// <summary>
        /// Property to return the editor used to modify the texture wrapping state for a sprite.
        /// </summary>
        ISpriteWrappingEditor WrappingEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the sprite color editor.
        /// </summary>
        ISpriteColorEdit ColorEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the sprite anchor editor.
        /// </summary>
        ISpriteAnchorEdit AnchorEditor
        {
            get;
        }*/

        /// <summary>Property to return the currently active panel.</summary>
        IHostedPanelViewModel CurrentPanel
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the plug in settings.
        /// </summary>
        IImportSettings Settings
        {
            get;
        }

/*        /// <summary>
        /// Property to return the view model for the manual rectangle editor interface.
        /// </summary>
        IManualRectangleEditor ManualRectangleEditor
        {
            get;
        }

        /// <summary>
        /// Property to return the view model for the manual vertex editor interface.
        /// </summary>
        IManualVertexEditor ManualVertexEditor
        {
            get;
        }

        /// <summary>
        /// Property tor return the view model for the sprite picker mask color editor.
        /// </summary>
        ISpritePickMaskEditor SpritePickMaskEditor
        {
            get;
        }*/



        /// <summary>
        /// Property to return the buffer that contains the image data for the <see cref="Texture"/>.
        /// </summary>
        IGorgonImage ImageData
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
        /// Property to set or return the color of each sprite vertex.
        /// </summary>
        IReadOnlyList<GorgonColor> VertexColors
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return offsets for each vertex in the sprite.
        /// </summary>
        IReadOnlyList<DX.Vector3> VertexOffsets
        {
            get;
        }

        /// <summary>
        /// Property to return the anchor position for for the sprite.
        /// </summary>
        DX.Vector2 Anchor
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
        /// Property to return whether the sprite will use nearest neighbour filtering, or bilinear filtering.
        /// </summary>
        bool IsPixellated
        {
            get;
        }

        /// <summary>
        /// Property to return the current sampler state for the sprite.
        /// </summary>
        GorgonSamplerState SamplerState
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
        /// Property to return the command to execute when picking a sprite.
        /// </summary>
        IEditorCommand<object> SpritePickCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when clipping a sprite.
        /// </summary>
        IEditorCommand<object> SpriteClipCommand
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

        /// <summary>
        /// Property to return the command to execute when toggling the manual clipping rectangle.
        /// </summary>
        IEditorCommand<object> ToggleManualClipRectCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to execute when toggling the manual vertex editing.
        /// </summary>
        IEditorCommand<object> ToggleManualVertexEditCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the sprite sprite picker mask color editor.
        /// </summary>
        IEditorCommand<object> ShowSpritePickMaskEditorCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the sprite color editor.
        /// </summary>
        IEditorCommand<object> ShowColorEditorCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the sprite anchor editor.
        /// </summary>
        IEditorCommand<object> ShowAnchorEditorCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the sprite texture wrapping editor.
        /// </summary>
        IEditorCommand<object> ShowWrappingEditorCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to execut when adjusting the sprite vertex offsets.
        /// </summary>
        IEditorCommand<object> SpriteVertexOffsetCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to apply the vertex offsets to the sprite.
        /// </summary>
        IEditorCommand<IReadOnlyList<DX.Vector3>> SetVertexOffsetsCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to apply texture filtering to the sprite.
        /// </summary>
        IEditorCommand<SampleFilter> SetTextureFilteringCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to set the texture on the sprite.
        /// </summary>
        IEditorAsyncCommand<SetTextureArgs> SetTextureCommand
        {
            get;
        }
        #endregion

        #region Methods.
#warning Should be a command
        /// <summary>
        /// Function to extract the image data from a sprite.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        Task ExtractImageDataAsync();
        #endregion
    }
}
