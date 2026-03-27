
// 
// Gorgon
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
/// Information about video memory for a <see cref="GorgonVideoAdapterInfo"/>
/// </summary>
/// <param name="System">The amount of dedicated system memory available, in bytes.</param>
/// <param name="Video">The amount of memory available on the GPU, in bytes.</param>
/// <param name="Shared">The amount of memory shared between the CPU and GPU, in bytes.</param>
public readonly record struct GorgonVideoAdapterMemory(long System, long Video, long Shared);