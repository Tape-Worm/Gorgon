
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 24, 2020 11:25:55 PM
// 


using Gorgon.Editor.Services;
using Gorgon.Editor.Tools;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Editor.ExtractSpriteTool;

/// <summary>
/// The view model for the main UI
/// </summary>
internal interface IExtract
    : IEditorTool
{
    /// <summary>
    /// Property to return the progress status of the extraction operation.
    /// </summary>
    ref readonly ProgressData ExtractTaskProgress
    {
        get;
    }

    /// <summary>
    /// Property to return the texture used to extract the sprites.
    /// </summary>
    GorgonTexture2DView Texture
    {
        get;
    }

    /// <summary>
    /// Property to return the flag to indicate that sprite generation is executing.
    /// </summary>
    bool IsGenerating
    {
        get;
    }

    /// <summary>
    /// Property to set or return the number of columns/rows in the grid.
    /// </summary>
    GorgonPoint GridSize
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the maximum columns and rows allowed in the grid.
    /// </summary>
    GorgonPoint MaxGridSize
    {
        get;
    }

    /// <summary>
    /// Property to set or return the offset of the grid, in pixels.
    /// </summary>
    GorgonPoint GridOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the size of a grid cell.
    /// </summary>
    GorgonPoint CellSize
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the extractor should skip empty regions based on a color value.
    /// </summary>
    bool SkipEmpty
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the mask color to use when skipping empty regions.
    /// </summary>
    /// <remarks>
    /// This value is only used when the <see cref="SkipEmpty"/> property is set to <b>true</b>.
    /// </remarks>
    GorgonColor SkipMaskColor
    {
        get;
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
    /// Property to set or return the number of array indices to use.
    /// </summary>
    int ArrayCount
    {
        get;
        set;
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
    /// Property to set or return whether sprite preview mode is active or not.
    /// </summary>
    bool IsInSpritePreview
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the list of sprites retrieved from extraction.
    /// </summary>
    IReadOnlyList<GorgonSprite> Sprites
    {
        get;
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
    /// Property to return the command used to assign the color used to skip empty regions.
    /// </summary>
    IEditorCommand<object> SetEmptySpriteMaskColorCommand
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
    /// Property to return the command that will generate the sprite data.
    /// </summary>
    IEditorAsyncCommand<object> GenerateSpritesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to cancel sprite generation.
    /// </summary>
    IEditorCommand<object> CancelSpriteGenerationCommand
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
}
