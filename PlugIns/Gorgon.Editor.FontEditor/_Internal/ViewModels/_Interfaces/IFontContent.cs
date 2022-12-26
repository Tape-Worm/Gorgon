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
// Created: August 29, 2021 3:55:17 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// This is the interface for our view model that we use to present data back to the font content view.
/// </summary>
internal interface IFontContent
    : IVisualEditorContent
{
    /// <summary>
    /// Property to return the outline interface view model.
    /// </summary>
    IFontOutline FontOutline
    {
        get;
    }

    /// <summary>
    /// Property to return the font character selection view model.
    /// </summary>
    IFontCharacterSelection FontCharacterSelection
    {
        get;
    }

    /// <summary>
    /// Property to return the texture editor context.
    /// </summary>
    ITextureEditorContext TextureEditor
    {
        get;
    }

    /// <summary>
    /// Property to return the working font.
    /// </summary>
    GorgonFont WorkingFont
    {
        get;
    }

    /// <summary>
    /// Property to return the font family.
    /// </summary>
    string FontFamily
    {
        get;            
    }

    /// <summary>
    /// Property to return the font size.
    /// </summary>
    float FontSize
    {
        get;
    }

    /// <summary>
    /// Property to return the units for the font size.
    /// </summary>
    FontHeightMode FontUnits
    {
        get;
    }

    /// <summary>
    /// Property to return whether the font is bold or not.
    /// </summary>
    bool IsBold
    {
        get;
    }

    /// <summary>
    /// Property to return whether the font is italic or not.
    /// </summary>
    bool IsItalic
    {
        get;
    }

    /// <summary>
    /// Property to return whether the sprite is antialiased or not.
    /// </summary>
    bool IsAntiAliased
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font family.
    /// </summary>
    IEditorAsyncCommand<string> SetFontFamilyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font size.
    /// </summary>
    IEditorAsyncCommand<float> SetSizeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font style to Bold.
    /// </summary>
    IEditorAsyncCommand<bool> SetBoldCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font style to Italic.
    /// </summary>
    IEditorAsyncCommand<bool> SetItalicCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font glyphs to be antialiased or not.
    /// </summary>
    IEditorAsyncCommand<bool> SetAntiAliasCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the font measure units.
    /// </summary>
    IEditorAsyncCommand<bool> SetFontUnitsCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the outline interface.
    /// </summary>
    IEditorCommand<bool> ActivateOutlineCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the texture context.
    /// </summary>
    IEditorCommand<object> ActivateTextureContextCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the character selection interface.
    /// </summary>
    IEditorCommand<object> ActivateCharacterSelectionCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to create a new font derived from the current font.
    /// </summary>
    IEditorAsyncCommand<object> NewFontCommand
    {
        get;
    }
}
