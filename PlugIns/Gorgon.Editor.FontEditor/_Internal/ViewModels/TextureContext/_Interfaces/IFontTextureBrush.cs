
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
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
// Created: September 9, 2021 12:57:16 PM
// 


using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view model for the texture brush editing interface
/// </summary>
internal interface IFontTextureBrush
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return the brush used to render the glyphs.
    /// </summary>
    GorgonGlyphTextureBrush Brush
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the texture rendered into the glyphs.
    /// </summary>
    IGorgonImage Texture
    {
        get;
    }

    /// <summary>
    /// Property to return the region on the texture.
    /// </summary>
    GorgonRectangleF Region
    {
        get;
    }

    /// <summary>
    /// Property to return the wrapping mode for the glyph brush.
    /// </summary>
    GlyphBrushWrapMode WrapMode
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to assign the boundaries for the texture.
    /// </summary>
    IEditorCommand<GorgonRectangleF> SetRegionCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to assign the wrapping mode.
    /// </summary>
    IEditorCommand<GlyphBrushWrapMode> SetWrappingModeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to load the texture to use for the brush.
    /// </summary>
    IEditorAsyncCommand<SetTextureArgs> LoadTextureCommand
    {
        get;
    }
}
