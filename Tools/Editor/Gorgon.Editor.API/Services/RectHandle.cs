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
// Created: March 19, 2019 8:19:27 PM
// 
#endregion

using System.Windows.Forms;
using Gorgon.Graphics.Core;
using DX = SharpDX;

namespace Gorgon.Editor.Services;

/// <summary>
/// A handle on a selection rectangle.
/// </summary>
public class RectHandle
{
    /// <summary>
    /// Property to return the boundaries for the handle.
    /// </summary>
    public DX.RectangleF HandleBounds
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the cursor to use for the handle.
    /// </summary>
    public Cursor HandleCursor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to assign a texture to the handle instead of an empty rectangle.
    /// </summary>
    public GorgonTexture2DView Texture
    {
        get;
        set;
    }
}
