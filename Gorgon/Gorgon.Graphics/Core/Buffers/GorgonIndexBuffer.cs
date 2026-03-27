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
// Created: January 14, 2026 9:28:58 PM
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
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using WinRT;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A buffer used to hold arbitrary types of data used by the GPU.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something here.
/// </para>
/// </remarks>
public sealed unsafe class GorgonIndexBuffer
    : GorgonGpuBufferCommon, IGorgonIndexBufferInfo
{
    private ComPtr<D3D12MA_Allocation> _bufferAllocation;

    private CpuBufferAllocation _uploadAllocation = CpuBufferAllocation.Null;
    private readonly GorgonIndexBufferInfo _info;

    /// <summary>
    /// Property to set or return whether the dynamic buffer has data that needs to be uploaded.
    /// </summary>
    internal bool NeedsDataUpload
    {
        get;
        set;
    } = true;

    /// <inheritdoc cref="IGorgonIndexBufferInfo.Use32BitIndices"/>
    public bool Use32BitIndices => _info.Use32BitIndices;

    /// <inheritdoc/>
    private protected override void ValidateInfo()
    {
        if (SizeInBytes < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL, Name, 1));
        }
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {        
        if (disposing)
        {            
            this.UnregisterDisposable(Graphics);
        }

        _bufferAllocation.Dispose();

        base.Dispose(disposing);
    }

    /// <summary>
    /// Function to create the native backing resources for the buffer.
    /// </summary>
    private void CreateNative()
    {
        switch (Usage)
        {
            case BufferUsage.DynamicPerFrame:
            case BufferUsage.Default:
                {
                    using ComPtr<ID3D12Resource2> resource = default;
                    D3D12_RESOURCE_DESC1 desc = D3D12_RESOURCE_DESC1.Buffer((ulong)SizeInBytes);
                    D3D12MA_ALLOCATION_DESC allocDesc = new(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

                    Graphics.Allocator.Get()->CreateResource3(&allocDesc, &desc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
                        null, 0, null,
                        _bufferAllocation.GetAddressOf(), Win32.__uuidof<ID3D12Resource2>(), (void**)resource.GetAddressOf())
                        .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE));

                    SetResource(in resource);
                }
                break;
            default:
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_USAGE_UNKNOWN, Usage, Name));
        }
    }

    /// <inheritdoc/>
    private protected override void OnGetResourceInfo(out GpuResourceInfo resourceInfo)
    {
        D3D12_RESOURCE_DESC1 desc = D3DResource.Get()->GetDesc1();
        desc.Width = (ulong)SizeInBytes;
        // Index buffers don't need an alignment.
        desc.Alignment = 0;
        resourceInfo = GpuResourceInfo.FromD3D(in desc);
    }

    /// <summary>
    /// Function to return the transient upload buffer for a dynamic buffer.
    /// </summary>
    /// <returns>A read only reference to the CPU buffer allocation backing this dynamic buffer.</returns>
    internal ref readonly CpuBufferAllocation GetTransientBufferData()
    {
        if (!_uploadAllocation.IsAvailable)
        {
            Graphics.UploadHeaps.Allocate((ulong)SizeInBytes, 0, out _uploadAllocation);
        }

        Debug.Assert(_uploadAllocation.IsAvailable, $"Upload heap for dynamic buffer '{Name}' is no longer valid.");

        return ref _uploadAllocation;
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonIndexBuffer() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexBuffer"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <exception cref="GorgonException"><para>Thrown if the buffer cannot be created because the size is less than 1 byte.</para></exception>
    /// <remarks>
    /// <para>
    /// This creates a generic buffer that can hold data on the GPU, or upload to the GPU. The buffer data has no structure. That is defined by views that can be created from the buffer. The types of views 
    /// available are defined in the <see cref="GorgonGpuBufferInfo"/> flags.
    /// </para>
    /// <para>
    /// Most buffers will require a minimum <see cref="GorgonCommonBufferInfo.SizeInBytes"/> of 1 byte. However, some views will require the buffer have a specific minimum size (e.g. constant buffers must be at 
    /// least 256 bytes, render target views must be at least the size of a <see cref="BufferFormat"/> format size, etc...).  
    /// </para>
    /// <para>
    /// The <paramref name="info"/> also contains a <see cref="GorgonCommonBufferInfo.Usage"/> flag which indicates how often the data can be updated in a buffer. Below is a description of how to use the usage 
    /// flags with a buffer.
    /// <inheritdoc cref="IGorgonCommonBufferInfo.Usage" path="/remarks/para/list"/>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="BufferUsage"/>
    public GorgonIndexBuffer(GorgonGraphics graphics, string name, GorgonIndexBufferInfo info)
        : base(graphics, name, info)
    {
        _info = info;

        ValidateInfo();

        Graphics.Log.Print($"Creating Gorgon index buffer '{Name}'.", LoggingLevel.Simple);

        CreateNative();        

        this.RegisterDisposable(Graphics);
    }
}
