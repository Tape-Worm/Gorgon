// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: January 3, 2026 1:54:46 PM
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A view for a render target buffer resource.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something here.
/// </para>
/// </remarks>
public unsafe sealed class GorgonBufferRenderTargetView
    : GorgonRenderTargetView
{
    private CpuDescriptorAllocation _allocation;

    /// <summary>
    /// Property to return the buffer associated with this view.
    /// </summary>
    /// <remarks>
    /// This value is a strongly typed version of the <see cref="GorgonResourceView.Resource"/> property and point to the same object.
    /// </remarks>
    public GorgonGpuBuffer_OLDE Buffer
    {
        get;
    }

    /// <summary>
    /// Property to return the starting element index in the buffer for the view.
    /// </summary>
    public long StartElementIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the number elements in the buffer being viewed.
    /// </summary>
    public long ElementCount
    {
        get;
    } = 1;

    /// <inheritdoc/>
    private protected override (D3D12_CPU_DESCRIPTOR_HANDLE CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE GpuHandle) OnCreateViewHandles()
    {
        D3D12_RENDER_TARGET_VIEW_DESC desc = new()
        {
            ViewDimension = D3D12_RTV_DIMENSION.D3D12_RTV_DIMENSION_BUFFER,
            Format = (DXGI_FORMAT)Format,
            Buffer = new D3D12_BUFFER_RTV
            {
                FirstElement = (ulong)StartElementIndex,
                NumElements = (uint)ElementCount
            }           
        };

        Graphics.Log.Print($"Allocating CPU handle for {Name}.", LoggingLevel.Verbose);
        Graphics.RtvDescriptors.Allocate(1, out _allocation);

        Debug.Assert(_allocation.Heap is not null, "Heap not found for allocation.");

        Graphics.D3DDevice.Get()->CreateRenderTargetView((PID3D12Resource2)Resource.D3DResource.Get(), &desc, _allocation.CpuHandle);
        return (_allocation.CpuHandle, D3D12_GPU_DESCRIPTOR_HANDLE.DEFAULT);
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Graphics.Log.Print($"Destroying view '{Name}'...", LoggingLevel.Simple);

            CpuDescriptorAllocation allocation = _allocation;
            _allocation = CpuDescriptorAllocation.Null;

            if (!allocation.Equals(in CpuDescriptorAllocation.Null))
            {
                CpuDescriptorHeap? heap = allocation.Heap;
                Graphics.Log.Print($"Freeing CPU handle allocation for {Name}.", LoggingLevel.Verbose);
                heap?.Free(ref allocation);
            }

            this.UnregisterDisposable(Graphics);
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBufferRenderTargetView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonRenderTargetView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, GorgonFormatInfo, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonRenderTargetView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, GorgonFormatInfo, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer">The buffer for the view.</param>
    /// <param name="format"><inheritdoc cref="GorgonRenderTargetView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, GorgonFormatInfo, bool)" path="/param[@name='format']"/></param>
    /// <param name="formatInfo"><inheritdoc cref="GorgonRenderTargetView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, GorgonFormatInfo, bool)" path="/param[@name='formatInfo']"/></param>
    /// <param name="start">The starting element in the buffer to view.</param>
    /// <param name="count">The number of elements for the view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonRenderTargetView(GorgonGraphics, string, GorgonGpuResource, BufferFormat, GorgonFormatInfo, bool)" path="/param[@name='owned']"/></param>
    internal GorgonBufferRenderTargetView(GorgonGraphics graphics, string name, GorgonGpuBuffer_OLDE buffer, BufferFormat format, GorgonFormatInfo formatInfo, long start, long count, bool owned)
        : base(graphics, name, buffer, format, formatInfo, owned)
    {
        Buffer = buffer;
        StartElementIndex = start;
        ElementCount = count;

        this.RegisterDisposable(Graphics);

        CreateNative();
    }
}
