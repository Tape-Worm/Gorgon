// Gorgon.
// Copyright (C) 2024 Michael Winsor
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
// Created: April 7, 2024 11:18:41 PM
//
 
using Gorgon.Memory;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An allocator used to retrieve <see cref="GorgonBlendState"/> objects from a pool
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="GorgonBlendStatePoolAllocator"/> class.</remarks>
/// <param name="size">[Optional] The number of items that can be stored in this pool.</param>
public sealed class GorgonBlendStatePoolAllocator(int size = 4096)
    : GorgonRingPool<GorgonBlendState>(size, () => new())    
{
}
