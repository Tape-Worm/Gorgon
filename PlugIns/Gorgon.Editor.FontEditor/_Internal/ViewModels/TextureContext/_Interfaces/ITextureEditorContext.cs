#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: September 1, 2021 2:35:03 AM
// 
#endregion

using System.Collections.Generic;
using DX = SharpDX;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// A context editor for modifying the textures on the font.
/// </summary>
internal interface ITextureEditorContext
    : IEditorContext
{
    /// <summary>
    /// Property to return the solid brush viewmodel.
    /// </summary>
    IFontSolidBrush SolidBrush
    {
        get;
    }

    /// <summary>
    /// Property to return the pattern brush viewmodel.
    /// </summary>
    IFontPatternBrush PatternBrush
    {
        get;
    }

    /// <summary>
    /// Property to return the gradient brush viewmodel.
    /// </summary>
    IFontGradientBrush GradientBrush
    {
        get;
    }

    /// <summary>
    /// Property to return the texture brush view model.
    /// </summary>
    IFontTextureBrush TextureBrush
    {
        get;
    }

    /// <summary>
    /// Property to return the font texture size viewmodel.
    /// </summary>
    IFontTextureSize FontTextureSize
    {
        get;
    }

    /// <summary>
    /// Property to return the font padding viewmodel.
    /// </summary>
    IFontPadding FontPadding
    {
        get;
    }

    /// <summary>
    /// Property to set or return the current hosted panel view model.
    /// </summary>
    IHostedPanelViewModel CurrentPanel
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the textures for the font.
    /// </summary>
    IReadOnlyList<GorgonTexture2DView> Textures
    {
        get;
    }

    /// <summary>
    /// Property to return the total number of textures, and array indices in the font.
    /// </summary>
    int TotalTextureArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to set or return whether the textures use premultiplied alpha or not.
    /// </summary>
    bool UsePremultipliedAlpha
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the currently selected texture.
    /// </summary>
    int SelectedTexture
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the currently selected array index for the <see cref="SelectedTexture"/>.
    /// </summary>
    int SelectedArrayIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the size of the font textures.
    /// </summary>
    DX.Size2 TextureSize
    {
        get;
    }

    /// <summary>
    /// Property to return the padding around the glyphs on the textures, in pixels.
    /// </summary>
    int FontGlyphPadding
    {
        get;
    }

    /// <summary>
    /// Property to return the current brush.
    /// </summary>
    GorgonGlyphBrush CurrentBrush
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to change the premultiplied state on the font.
    /// </summary>
    IEditorAsyncCommand<bool> SetPremultipliedCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the next font texture.
    /// </summary>
    IEditorCommand<object> NextTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the previous font texture.
    /// </summary>
    IEditorCommand<object> PrevTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the first font texture.
    /// </summary>
    IEditorCommand<object> FirstTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to switch to the last font texture.
    /// </summary>
    IEditorCommand<object> LastTextureCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the texture size editor.
    /// </summary>
    IEditorCommand<object> ActivateTextureSizeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the padding editor.
    /// </summary>
    IEditorCommand<object> ActivatePaddingCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the solid brush editor.
    /// </summary>
    IEditorCommand<object> ActivateSolidBrushCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the pattern brush editor.
    /// </summary>
    IEditorCommand<object> ActivatePatternBrushCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the gradient brush editor.
    /// </summary>
    IEditorCommand<object> ActivateGradientBrushCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the texture brush editor.
    /// </summary>
    IEditorCommand<object> ActivateTextureBrushCommand
    {
        get;
    }
}
