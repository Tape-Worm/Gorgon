
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
// Created: March 28, 2019 9:45:09 AM
// 

using Gorgon.Graphics;

namespace Gorgon.Editor.UI.Controls;

/// <summary>
/// Event arguments for the <see cref="ColorPicker.ColorChanged"/> event
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ColorChangedEventArgs"/> class.</remarks>
/// <param name="newColor">The new color.</param>
/// <param name="originalColor">The original color.</param>
public class ColorChangedEventArgs(GorgonColor newColor, GorgonColor originalColor)
        : EventArgs
{
    /// <summary>
    /// Property to return the selected color.
    /// </summary>
    public GorgonColor Color
    {
        get;
    } = newColor;

    /// <summary>
    /// Property to return the original color.
    /// </summary>
    public GorgonColor OriginalColor
    {
        get;
    } = originalColor;

}
