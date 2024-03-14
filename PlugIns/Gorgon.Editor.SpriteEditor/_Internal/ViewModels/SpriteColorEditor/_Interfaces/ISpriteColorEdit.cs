
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 4, 2019 8:49:29 PM
// 


using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model for the sprite color panel
/// </summary>
internal interface ISpriteColorEdit
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return the selected vertex.
    /// </summary>
    IReadOnlyList<bool> SelectedVertices
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the original color for the sprite.
    /// </summary>
    IReadOnlyList<GorgonColor> OriginalSpriteColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the color to apply to the sprite.
    /// </summary>
    IReadOnlyList<GorgonColor> SpriteColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the currently selected color for an individual vertex.
    /// </summary>
    GorgonColor SelectedColor
    {
        get;
        set;
    }
}
