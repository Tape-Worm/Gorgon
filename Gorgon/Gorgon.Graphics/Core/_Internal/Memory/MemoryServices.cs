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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 6, 2025 3:49:07 PM
//

using System;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Services for handling memory.
/// </summary>
/// <remarks>
/// <para>
/// <b>Re: <see cref="MegaBuffer"/></b>
/// </para>
/// <inheritdoc cref="Core.MegaBuffer" path="/remarks/para"/>
/// </remarks>
/// <param name="megaBuffer">The mega buffer implementation for bindless buffers.</param>
/// <param name="textureTilePool">The virtual texture tile pool.</param>
/// <param name="allocator">The D3D 12 memory allocator interface pointer.</param>
internal sealed class MemoryServices(MegaBufferPool megaBuffer, VirtualTextureTilePool textureTilePool, ref readonly ComPtr<D3D12MA_Allocator> allocator)
        : IDisposable
{
    private readonly ComPtr<D3D12MA_Allocator> _allocator = allocator;

    /// <summary>
    /// Property to return the D3D 12 memory allocator COM pointer.
    /// </summary>
    public ref readonly ComPtr<D3D12MA_Allocator> Allocator => ref _allocator;

    /// <summary>
    /// Property to return the mega buffer implementation for bindless buffers.
    /// </summary>
    public MegaBufferPool MegaBuffer
    {
        get;
    } = megaBuffer;

    /// <summary>
    /// Property to return the pool for texture tiles.
    /// </summary>
    public VirtualTextureTilePool TextureTilePool
    {
        get;
    } = textureTilePool;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            MegaBuffer.Dispose();
            TextureTilePool.Dispose();
        }

        _allocator.Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to perform garbage collection on the heaps.
    /// </summary>
    public void GarbageCollect()
    {
        MegaBuffer.GarbageCollect();
        TextureTilePool.GarbageCollect();
    }
}
