// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: August 26, 2025 9:52:31 PM
//

using Gorgon.Core;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information that was used to build a swap chain.
/// </summary>
public interface IGorgonSwapChainInfo
{
    /// <summary>
    /// Property to return whether to use discard or sequential flip modes.
    /// </summary>
    bool FlipDiscard
    {
        get;
    }

    /// <summary>
    /// Property to return the format of the swap chain back buffer.
    /// </summary>
    BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return whether the swap chain will be triple buffered or not.
    /// </summary>
    /// <remarks>
    /// If this value is <b>true</b>, the number of back buffers for the swap chain will be 3, otherwise, it will be 2.
    /// </remarks>
    bool TripleBuffer
    {
        get;
    }

    /// <summary>
    /// Property to return the height of the swap chain back buffers, in pixels.
    /// </summary>
    int Height
    {
        get;
    }

    /// <summary>
    /// Property to return the width of the swap chain back buffers, in pixels.
    /// </summary>
    int Width
    {
        get;
    }

    /// <summary>
    /// Property to return whether the back buffer should be scaled on window resize, or not.
    /// </summary>
    bool AllowScaling
    {
        get;
    }
}