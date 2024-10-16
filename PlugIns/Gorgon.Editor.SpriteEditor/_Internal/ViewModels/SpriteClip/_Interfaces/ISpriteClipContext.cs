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
// Created: May 4, 2020 12:12:25 AM
// 

using System.Numerics;
using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The context view model for the sprite clipping context options
/// </summary>
internal interface ISpriteClipContext
    : IEditorContext, IArrayUpdate
{
    /// <summary>
    /// Property to set or return the rectangle representing the sprite.
    /// </summary>
    GorgonRectangleF SpriteRectangle
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the size of the fixed width and height for sprite clipping.
    /// </summary>
    Vector2? FixedSize
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to set the sprite size to the full size of the sprite texture.
    /// </summary>
    IEditorCommand<object> FullSizeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to enable or disable fixed size clipping.
    /// </summary>
    IEditorCommand<Vector2?> FixedSizeCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return the command used to apply the updated clipping coordinates.
    /// </summary>
    IEditorCommand<object> ApplyCommand
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command used to cancel the sprite clipping operation.
    /// </summary>
    IEditorCommand<object> CancelCommand
    {
        get;
    }
}
