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
// Created: August 30, 2021 11:38:37 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view model for the text color panel.
/// </summary>
internal interface IFontOutline
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return the size of the outline.
    /// </summary>
    int OutlineSize
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the original color for the text.
    /// </summary>
    GorgonColor OriginalStartColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the currently selected color.
    /// </summary>
    GorgonColor SelectedStartColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the original color for the text.
    /// </summary>
    GorgonColor OriginalEndColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the currently selected color.
    /// </summary>
    GorgonColor SelectedEndColor
    {
        get;
        set;
    }
}
