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
// Created: April 24, 2019 10:54:18 PM
// 
#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ExtractSpriteTool
{
    /// <summary>
    /// The view model for the main UI.
    /// </summary>
    internal interface IExtract
        : IViewModel
    {
        /// <summary>
        /// Property to return the progress status of the extraction operation.
        /// </summary>
        ref readonly ProgressData ExtractTaskProgress
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether we are in sprite preview mode.
        /// </summary>
        bool InSpritePreview
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the number of sprites for previewing.
        /// </summary>
        int SpritePreviewCount
        {
            get;
        }

        /// <summary>
        /// Property to return the current preview sprite index.
        /// </summary>
        int CurrentPreviewSprite
        {
            get;
        }

        /// <summary>
        /// Property to return the list of sprites retrieved from extraction.
        /// </summary>
        IReadOnlyList<GorgonSprite> Sprites
        {
            get;
        }

        /// <summary>
        /// Property to return the texture that is to be rendered.
        /// </summary>
        GorgonTexture2DView Texture
        {
            get;
        }

        /// <summary>
        /// Property to set or return the offset of the grid, in pixels.
        /// </summary>
        DX.Point GridOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size of a grid cell.
        /// </summary>
        DX.Size2 CellSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the number of columns/rows in the grid.
        /// </summary>
        DX.Size2 GridSize
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the maximum columns and rows allowed in the grid.
        /// </summary>
        DX.Size2 MaxGridSize
        {
            get;
        }

        /// <summary>
        /// Property to return the maximum array indices in the texture for previewing.
        /// </summary>
        int PreviewArrayCount
        {
            get;
        }

        /// <summary>
        /// Property to return the current array index being previewed.
        /// </summary>
        int CurrentPreviewArrayIndex
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the texture is an array.
        /// </summary>
        bool HasArray
        {
            get;
        }

        /// <summary>
        /// Property to set or return the number of array indices used.
        /// </summary>
        int ArrayCount
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the maximum number of array indices.
        /// </summary>
        int MaxArrayCount
        {
            get;
        }

        /// <summary>
        /// Property to return the maximum number of array indices.
        /// </summary>
        int MaxArrayIndex
        {
            get;
        }

        /// <summary>
        /// Property to set or return the starting array index to use.
        /// </summary>
        int StartArrayIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the UI is in a maximized state or not.
        /// </summary>
        bool IsMaximized
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the color used when determining which sprites are considered empty.
        /// </summary>
        GorgonColor SkipMaskColor
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether to allow skipping empty sprites.
        /// </summary>
        bool AllowSkipEmpty
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the task used to generate the sprite data.
        /// </summary>
        Task SpriteGenerationTask
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to assign the masking color used for skipping empty sprites.
        /// </summary>
        IEditorCommand<object> SetEmptySpriteMaskColorCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command that will generate the sprite data.
        /// </summary>
        IEditorCommand<object> GenerateSpritesCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to shut down any running operations.
        /// </summary>
        IEditorAsyncCommand<object> ShutdownCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to save the sprite data.
        /// </summary>
        IEditorCommand<SaveSpritesArgs> SaveSpritesCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to go to the next preview sprite.
        /// </summary>
        IEditorCommand<object> NextPreviewSpriteCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to go to the previous preview sprite.
        /// </summary>
        IEditorCommand<object> PrevPreviewSpriteCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to go to the next preview array index.
        /// </summary>
        IEditorCommand<object> NextPreviewArrayCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to go to the previous preview array index.
        /// </summary>
        IEditorCommand<object> PrevPreviewArrayCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to copy the current array preview index into the array range start value.
        /// </summary>
        IEditorCommand<object> SendPreviewArrayToStartCommand
        {
            get;
        }
    }
}
