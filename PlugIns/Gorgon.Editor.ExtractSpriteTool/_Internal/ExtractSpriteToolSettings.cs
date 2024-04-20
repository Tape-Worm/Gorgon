﻿
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
// Created: April 25, 2019 9:37:40 AM
// 

using Gorgon.Graphics;

namespace Gorgon.Editor.ExtractSpriteTool;

/// <summary>
/// The settings for the plug in
/// </summary>
internal class ExtractSpriteToolSettings
{
    /// <summary>
    /// Property to set or return whether to allow the skipping of empty sprites.
    /// </summary>
    public bool AllowEmptySpriteSkip
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return the color to use when skipping empty sprites.
    /// </summary>
    public int SkipColor
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the last directory path used to output sprites into.
    /// </summary>
    public string LastOutputDir
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether the window is in a maximized state or not.
    /// </summary>
    public bool IsMaximized
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the size, in pixels, of a single cell in the grid.
    /// </summary>
    public GorgonPoint GridCellSize
    {
        get;
        set;
    } = new GorgonPoint(32, 32);
}
