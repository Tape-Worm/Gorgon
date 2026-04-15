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
/// A buffer used to store index data.
/// </summary>
/// <remarks>
/// <para>
/// An index buffer uses indices that point to vertices within a buffer to help form a mesh. This allows an application to use smaller buffers for vertices and helps reduce bandwidth when rendering. 
/// </para>
/// <para>
/// For example, if a vertex buffer has a vertex that is 40 bytes, and describes a rectangle out of 2 triangles, that's 6 vertices * 40 bytes = 240 bytes. Now, with an index buffer, you can use 16 bit 
/// indices and 4 vertices to describe the same rectangle.  12 bytes for the indices, and 160 bytes for the vertices = 172 bytes, a difference of 68 bytes in total. Scale this up by meshes that use 10's of 
/// thousands of vertices, and you start seeing some massive gains.
/// </para>
/// <para>
/// The index buffer can consist of indices that are 32 bits wide, or 16 bits wide. The smaller data size means less overhead, but a reduced mesh size (32 bit can address 4,294,967,296 vertices, while 16 
/// bit can only address 65536 vertices). The type of data is specified upon creation of the buffer.
/// </para>
/// <para>
/// <h3>Why a separate buffer type?</h3>
/// </para>
/// <para>
/// Gorgon works on a system known as bindless rendering. Older systems like OpenGL or Direct 3D 11 forced data to be bound to a pipeline using slots of some kind. This system has several drawbacks around 
/// state tracking and is no longer really representative of how a GPU actually works. With Gorgon and its Direct 3D 12 back end, we no longer need this binding system and can just create resources and 
/// just use them directly from memory. No more state tracking for resources, no more costly pipeline switches, etc... 
/// </para>
/// <para>
/// However, while this system applies to almost all resources, index buffers are required to be bound. This is unavoidable, and as such, the memory architecture for these index buffers are slightly 
/// different and require they be treated differently than a generic buffer type like <see cref="GorgonGpuBuffer"/>. 
/// </para>
/// </remarks>
/// <seealso cref="GorgonVideoAdapterInfo"/>
/// <seealso cref="GorgonGpuBufferInfo"/>
/// <seealso cref="GorgonGpuBuffer"/>
public sealed unsafe class GorgonIndexBuffer
    : GorgonGpuBufferCommon, IGorgonIndexBufferInfo
{
    private ComPtr<D3D12MA_Allocation> _bufferAllocation;

    private D3D12_RESOURCE_DESC1 _d3dDesc;
    private readonly GorgonIndexBufferInfo _info;

    /// <inheritdoc/>
    /// <remarks>
    /// This buffer has its own resource and never has an offset. It will always return 0.
    /// </remarks>
    internal override ulong ResourceOffset => 0;

    /// <inheritdoc/>
    internal override bool IsMegaBufferResource => false;

    /// <inheritdoc cref="IGorgonIndexBufferInfo.Use32BitIndices"/>
    public bool Use32BitIndices => _info.Use32BitIndices;

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

    /// <inheritdoc/>
    private protected override void ValidateInfo()
    {
        if (SizeInBytes < (_info.Use32BitIndices ? 4 : 2))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL, Name, (_info.Use32BitIndices ? 4 : 2)));
        }
    }

    /// <summary>
    /// Function to create the native backing resources for the buffer.
    /// </summary>
    private void CreateNative()
    {
        using ComPtr<ID3D12Resource2> resource = default;
        D3D12_RESOURCE_DESC1 desc = D3D12_RESOURCE_DESC1.Buffer((ulong)SizeInBytes);
        D3D12MA_ALLOCATION_DESC allocDesc = new(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

        Graphics.Memory.Allocator.Get()->CreateResource3(&allocDesc, &desc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
            null, 0, null,
            _bufferAllocation.GetAddressOf(), Win32.__uuidof<ID3D12Resource2>(), (void**)resource.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE));

        resource.SetD3DDebugName(Name);

        _d3dDesc = desc;

        AssignResource(in resource);
    }

    /// <inheritdoc/>
    private protected override void OnGetResourceInfo(out GpuResourceInfo resourceInfo) => resourceInfo = GpuResourceInfo.FromD3D(in _d3dDesc);

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
    /// </remarks>
    /// <seealso cref="GorgonGpuBufferInfo"/>
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
