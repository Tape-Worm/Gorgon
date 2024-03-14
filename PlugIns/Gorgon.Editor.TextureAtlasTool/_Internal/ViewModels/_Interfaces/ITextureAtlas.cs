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

using System.ComponentModel;
using Gorgon.Editor.Tools;
using Gorgon.Editor.UI;
using Gorgon.Renderers.Services;
using DX = SharpDX;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The view model for the main UI.
/// </summary>
internal interface ITextureAtlas
    : IEditorTool
{
    /// <summary>
    /// Property to return the view model for the sprite file loader.
    /// </summary>
    ISpriteFiles SpriteFiles
    {
        get;
    }

    /// <summary>
    /// Property to return the number of sprites that were loaded.
    /// </summary>
    int LoadedSpriteCount
    {
        get;
    }

    /// <summary>Property to return the path for the output files.</summary>
    string OutputPath
    {
        get;
    }

    /// <summary>Property to set or return the maximum size for the atlas texture.</summary>
    DX.Size2 MaxTextureSize
    {
        get;
        set;
    }

    /// <summary>Property to set or return the maximum number of array indices for the atlas texture.</summary>
    int MaxArrayCount
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the amount of padding, in pixels around each sprite on the texture.
    /// </summary>
    int Padding
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the base atlas texture name.
    /// </summary>
    string BaseTextureName
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the atlas after it's been generated.
    /// </summary>
    GorgonTextureAtlas Atlas
    {
        get;
    }

    /// <summary>
    /// Property to return the preview array index.
    /// </summary>
    int PreviewArrayIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the preview texture index.
    /// </summary>
    int PreviewTextureIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to calculate best fit sizes.
    /// </summary>
    IEditorCommand<object> CalculateSizesCommand
    {
        get;
    }

    /// <summary>Property to return the folder selection command.</summary>
    IEditorCommand<object> SelectFolderCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to generate the atlas.
    /// </summary>
    IEditorCommand<object> GenerateCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to move to the next preview item.
    /// </summary>
    IEditorCommand<object> NextPreviewCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to move to the previous preview item.
    /// </summary>
    IEditorCommand<object> PrevPreviewCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to commit the atlas data back to the file system.
    /// </summary>
    IEditorCommand<CancelEventArgs> CommitAtlasCommand
    {
        get;
    }
}
