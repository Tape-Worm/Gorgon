
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
// Created: May 19, 2020 12:51:31 PM
// 

using System.Numerics;
using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model for the sprite anchor editor
/// </summary>
internal interface ISpriteAnchorEdit
    : IHostedPanelViewModel
{

    /// <summary>
    /// Property to set or return whether to preview rotation with the current anchor setting.
    /// </summary>
    bool PreviewRotation
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to preview scaling with the current anchor setting.
    /// </summary>
    bool PreviewScale
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the boundaries for the anchor point.
    /// </summary>
    GorgonRectangle Bounds
    {
        get;
    }

    /// <summary>
    /// Property to return the mid point of the sprite, based on its vertices.
    /// </summary>
    Vector2 MidPoint
    {
        get;
    }

    /// <summary>
    /// Property to set or return the boundaries of the sprite (vertices).
    /// </summary>
    IReadOnlyList<Vector2> SpriteBounds
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the anchor point.
    /// </summary>
    Vector2 Anchor
    {
        get;
        set;
    }
}
