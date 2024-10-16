﻿
// 
// Gorgon
// Copyright (C) 2013 Michael Winsor
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
// Created: Tuesday, June 4, 2013 8:25:47 PM
// 

namespace Gorgon.Graphics.Core;

/// <summary> 
/// Settings for defining the set up for a swap chain
/// </summary>
/// <param name="Width">The width of the swap chain backbuffer.</param>
/// <param name="Height">The height of the swap chain backbuffer.</param>
/// <param name="Format">The pixel format of the swap chain backbuffer.</param>
/// <remarks>
/// <para>
/// The <paramref cref="Format"/> parameter must support being used a backbuffer format and this can be determined by using the <see cref="GorgonGraphics.FormatSupport"/> 
/// property
/// </para>
/// </remarks>
public record GorgonSwapChainInfo(int Width, int Height, BufferFormat Format)
    : IGorgonSwapChainInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
    /// </summary>
    /// <param name="info">A <see cref="IGorgonSwapChainInfo"/> to copy the settings from.</param>
    /// <param name="newName">[Optional] A new name for the swap chain.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonSwapChainInfo(IGorgonSwapChainInfo info, string newName = null)
        : this(info?.Width ?? throw new ArgumentNullException(nameof(info)), info.Height, info.Format)
    {
        Name = string.IsNullOrEmpty(newName) ? info.Name : newName;
        StretchBackBuffer = info.StretchBackBuffer;
    }

    /// <summary>
    /// Property to return whether the back buffer contents will be stretched to fit the size of the presentation target area (typically the client area of the window).
    /// </summary>
    /// <remarks>
    /// The default value for this value is <b>true</b>.
    /// </remarks>
    public bool StretchBackBuffer
    {
        get;
        init;
    } = true;

    /// <summary>
    /// Property to return the name of the swap chain.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = GorgonGraphicsResource.GenerateName(GorgonSwapChain.NamePrefix);

}