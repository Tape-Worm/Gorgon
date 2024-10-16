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
// Created: March 6, 2020 1:37:30 PM
// 

using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The settings view model for the edge detect effect
/// </summary>
internal interface IFxEdgeDetect
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return the threshold for detecting edges (as a percentage).
    /// </summary>
    int Threshold
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the amount to offset the edge line widths.
    /// </summary>
    float Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the color of the edge lines.
    /// </summary>
    GorgonColor LineColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the edges should be overlaid on top of the original image or not.
    /// </summary>
    bool Overlay
    {
        get;
        set;
    }
}
