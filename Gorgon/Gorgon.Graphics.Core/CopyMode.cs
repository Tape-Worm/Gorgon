
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: April 10, 2018 9:03:19 AM
// 

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the available modes for copying subresource data
/// </summary>
public enum CopyMode
{
    /// <summary>
    /// Data is copied into the buffer, overwriting existing data.
    /// </summary>
    None = 0,
    /// <summary>
    /// Data is copied into the buffer, but existing data cannot be overwritten.
    /// </summary>
    NoOverwrite = D3D11.CopyFlags.NoOverwrite,
    /// <summary>
    /// Data is copied into the buffer, but any existing data is discarded.
    /// </summary>
    Discard = D3D11.CopyFlags.Discard
}
