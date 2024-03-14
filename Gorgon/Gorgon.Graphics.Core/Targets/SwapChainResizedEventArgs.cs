﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 9, 2017 4:32:41 PM
// 
#endregion

using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Event arguments for the <see cref="GorgonSwapChain.SwapChainResized"/> event.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SwapChainResizingEventArgs"/> class.
/// </remarks>
/// <param name="newSize">The new size.</param>
/// <param name="oldSize">The old size.</param>
public class SwapChainResizedEventArgs(DX.Size2 newSize, DX.Size2 oldSize)
        : EventArgs
{
    #region Properties.
    /// <summary>
    /// Property to return the size of the swap chain backbuffers.
    /// </summary>
    public DX.Size2 Size
    {
        get;
    } = newSize;

    /// <summary>
    /// Property to return the previous size of the swap chain backbuffers.
    /// </summary>
    public DX.Size2 OldSize
    {
        get;
    } = oldSize;

    #endregion
}
