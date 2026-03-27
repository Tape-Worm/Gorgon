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
// Created: August 14, 2025 2:14:16 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides architectural information about the video adapter.
/// </summary>
/// <param name="NodeIndex">The node index for the physical adapter in multi GPU scenarios</param>
/// <param name="IsTileBasedRenderer">Returns whether the hardware and driver support a tile-based renderer.</param>
/// <param name="HasUnifiedMemoryArchitecture">Returns whether the hardware supports a unified memory architecture (UMA) or not.</param>
/// <param name="HasCacheCoherentUnifiedMemoryArchitecture">Returns whether the hardware supports a cache coherent unified memory architecture or not.</param>
/// <param name="HasIsolatedMemoryManagementUnit">Returns whether the hardware supports isolated memory management.</param>
/// <remarks>
/// <para>
/// The runtime sets <see cref="HasIsolatedMemoryManagementUnit"/> to <b>true</b> if the GPU honors CPU page table properties like MEM_WRITE_WATCH (for more information, see 
/// <a href="https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualalloc" target="_blank">VirtualAlloc</a>) and PAGE_READONLY (for more information, see 
/// <a href="https://learn.microsoft.com/en-us/windows/win32/Memory/memory-protection-constants" target="_blank">Memory Protection Constants</a>).
/// </para>
/// <para>
/// If the <see cref="HasIsolatedMemoryManagementUnit"/> is <b>true</b>, the application must take care to no use memory with these page table properties with the GPU, as the GPU might trigger these page 
/// table properties in unexpected ways. For example, GPU write operations might be coarser than the application expects, particularly writes from within shaders. Certain write-watch pages might appear 
/// dirty, even when it isn't obvious how GPU writes may have affected them. GPU operations associated with upload and readback heap usage scenarios work well with write-watch pages, but might occasionally 
/// generate false positives that can be safely ignored.
/// </para>
/// </remarks>
public readonly record struct GorgonVideoAdapterArchitecture(int NodeIndex, bool IsTileBasedRenderer, bool HasUnifiedMemoryArchitecture, bool HasCacheCoherentUnifiedMemoryArchitecture, bool HasIsolatedMemoryManagementUnit);
