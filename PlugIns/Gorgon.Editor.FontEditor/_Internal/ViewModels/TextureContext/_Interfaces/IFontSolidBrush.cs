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
// Created: September 5, 2021 8:00:54 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view model for creating a solid color brush.
/// </summary>
internal interface IFontSolidBrush
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return the currently selected solid color brush.
    /// </summary>
    GorgonGlyphSolidBrush Brush
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the original color.
    /// </summary>
    GorgonColor OriginalColor
    {
        get;
        set;
    }
}
