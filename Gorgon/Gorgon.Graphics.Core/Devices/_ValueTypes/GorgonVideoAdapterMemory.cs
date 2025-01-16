
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: November 6, 2017 5:56:11 PM
// 

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information about video memory for a <see cref="IGorgonVideoAdapterInfo"/>
/// </summary>
public readonly struct GorgonVideoAdapterMemory
{
    /// <summary>
    /// The amount of dedicated system memory available, in bytes.
    /// </summary>
    public readonly long System;

    /// <summary>
    /// The amount of memory available on the GPU, in bytes.
    /// </summary>
    public readonly long Video;

    /// <summary>
    /// The amount of memory shared between the CPU and GPU, in bytes.
    /// </summary>
    public readonly long Shared;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVideoAdapterMemory"/> struct.
    /// </summary>
    /// <param name="system">The system memory available.</param>
    /// <param name="shared">The shared memory available.</param>
    /// <param name="gpu">The gpu memory available.</param>
    internal GorgonVideoAdapterMemory(long system, long shared, long gpu)
    {
        System = system;
        Shared = shared;
        Video = gpu;
    }
}
