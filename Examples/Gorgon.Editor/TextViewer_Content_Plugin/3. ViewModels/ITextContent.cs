﻿
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
// Created: August 3, 2020 3:46:18 PM
// 

using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Examples;

/// <summary>
/// The current font face to use
/// </summary>
internal enum FontFace
{
    Arial,
    TimesNewRoman,
    Papyrus
}

/// <summary>
/// This is the interface for our view model that we use to present data back to the text viewer view
/// </summary>
internal interface ITextContent
    : IVisualEditorContent
{
    /// <summary>
    /// Property to return the view model for the text color editor.
    /// </summary>
    ITextColor TextColor
    {
        get;
    }

    /// <summary>
    /// Property to return the text to display.
    /// </summary>
    string Text
    {
        get;
    }

    /// <summary>
    /// Property to set or return the font face to use.
    /// </summary>
    FontFace FontFace
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the color of the text.
    /// </summary>
    GorgonColor Color
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to activate the text color editor.
    /// </summary>
    IEditorCommand<object> ActivateTextColorCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to alter the text of the content.
    /// </summary>
    IEditorCommand<object> ChangeTextCommand
    {
        get;
    }
}
