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
public sealed unsafe class GorgonGpuBuffer
    : GorgonGpuBufferCommon, IGorgonGpuBufferInfo
{
    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="Format">The format of the view.</param>
    /// <param name="Value1">The first key value.</param>
    /// <param name="Value2">The second key value.</param>
    private readonly record struct ViewKey(BufferFormat Format, long Value1, long Value2);

    private readonly Lock _viewLock = new();
    private readonly GorgonGpuBufferInfo _info;
    private readonly Dictionary<ViewKey, GorgonBufferRenderTargetView> _rtvs = [];
    private readonly Dictionary<ViewKey, GorgonConstantBufferView> _cbvs = [];
    private readonly Dictionary<ViewKey, GorgonStructuredBufferView> _structs = [];
    private readonly Dictionary<ViewKey, GorgonShaderBufferView> _srvs = [];
    private readonly Dictionary<ViewKey, GorgonResourceView> _uavs = [];    
    private GpuBufferAllocation _bufferAllocation = GpuBufferAllocation.Null;
    private CpuBufferAllocation _uploadAllocation = CpuBufferAllocation.Null;    

    /// <summary>
    /// Property to return the internal buffer allocation from the <see cref="MegaBuffer"/>.
    /// </summary>
    internal ref readonly GpuBufferAllocation GpuAllocation
    {
        get
        {
            switch (Usage)
            {
                case BufferUsage.DynamicPerFrame:
                case BufferUsage.Default:
                    return ref _bufferAllocation;
                default:
                    return ref GpuBufferAllocation.Null;
            }
        }
    }

    /// <summary>
    /// Property to set or return whether the dynamic buffer has data that needs to be uploaded.
    /// </summary>
    internal bool NeedsDataUpload
    {
        get;
        set;
    } = true;

    /// <inheritdoc/>
    public bool IsRenderTarget => _info.IsRenderTarget;

    /// <inheritdoc/>
    public bool IsConstantBuffer => _info.IsConstantBuffer;

    /// <inheritdoc/>
    public int Alignment => _info.Alignment;


    /// <inheritdoc/>
    private protected override void ValidateInfo()
    {
        if (Alignment < 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_ALIGNMENT, Alignment));
        }

        switch (Usage)
        {
            case BufferUsage.Download when IsConstantBuffer || IsRenderTarget || IsUnorderedAccess:
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_DOWNLOAD, Name));
            case BufferUsage.Upload or BufferUsage.DynamicPerFrame when IsRenderTarget:
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RTV_BUFFER_NOT_DEFAULT, Name));
        }

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
            // Remove all the child views.
            foreach (KeyValuePair<ViewKey, GorgonBufferRenderTargetView> view in _rtvs)
            {
                view.Value.Dispose();
            }

            foreach (KeyValuePair<ViewKey, GorgonConstantBufferView> view in _cbvs)
            {
                view.Value.Dispose();
            }

            foreach (KeyValuePair<ViewKey, GorgonStructuredBufferView> view in _structs)
            {
                view.Value.Dispose();
            }

            foreach (KeyValuePair<ViewKey, GorgonShaderBufferView> view in _srvs)
            {
                view.Value.Dispose();
            }

            foreach (KeyValuePair<ViewKey, GorgonResourceView> view in _uavs)
            {
                view.Value.Dispose();
            }

            _cbvs.Clear();
            _rtvs.Clear();
            _structs.Clear();
            _srvs.Clear();
            _uavs.Clear();

            if (!_bufferAllocation.IsNull)
            {
                Graphics.MegaBuffer.Free(ref _bufferAllocation);
            }

            this.UnregisterDisposable(Graphics);
        }

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
                Graphics.MegaBuffer.Allocate((ulong)SizeInBytes, out _bufferAllocation, (uint)Alignment);
                SetResource(in Graphics.MegaBuffer.D3DBuffer);
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
        // All mega buffer buffers are aligned on a 256 byte boundary (for worst case scenario - constant buffers).
        desc.Alignment = (uint)Alignment;
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
    /// Function to create a structured buffer view.
    /// </summary>
    /// <param name="structSize">The size, in bytes, of a single element in the view. Must be a multiple of 16.</param>
    /// <param name="startIndex">[Optional] The index in the buffer to start the view at.</param>
    /// <param name="count">[Optional] The number of elements to view.</param>
    /// <returns>A new <see cref="GorgonStructuredBufferView"/>.</returns>
    /// <exception cref="GorgonException"><para>Thrown if the <paramref name="structSize"/> is less than 16, or is not a multiple of 16.</para></exception>
    /// TODO:
    public GorgonStructuredBufferView GetStructuredBufferView(int structSize, long startIndex = 0, int? count = null)
    {
        using (_viewLock.EnterScope())
        {
            if (SizeInBytes < structSize)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_STRUCTURE, SizeInBytes, structSize));
            }

            long maxElements = (SizeInBytes / structSize).Max(1);
            count ??= (int)(maxElements - startIndex);

            ViewKey key = new(BufferFormat.Unknown, startIndex, ((long)count.Value << 32) | (uint)structSize);

            if ((_structs.TryGetValue(key, out GorgonStructuredBufferView? result)) && (result.D3DCpuHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT))
            {
                return result;
            }

            return _structs[key] = new GorgonStructuredBufferView(Graphics, Name, this, startIndex, structSize, count.Value, false);
        }
    }

    /// <summary>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?)"/>
    /// </summary>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?)" path="/exception"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?)" path="/remarks"/>
    public GorgonStructuredBufferView GetStructuredBufferView<T>(long startIndex = 0, int? count = null) where T : unmanaged
        => GetStructuredBufferView(sizeof(T), startIndex, count);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuBuffer"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <exception cref="GorgonException"><para>Thrown if the buffer cannot be created because the size is less than 1 byte.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the buffer is a render target, constant buffer, or unordered access buffer and the <see cref="BufferUsage"/> is set to <see cref="BufferUsage.Download"/>.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the buffer is a render target and the <see cref="BufferUsage"/> is not set to <see cref="BufferUsage.Default"/>.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the <see cref="GorgonGpuBufferInfo.Alignment"/> value is negative, or not a power of 2.</para>
    /// </exception>
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
    public GorgonGpuBuffer(GorgonGraphics graphics, string name, GorgonGpuBufferInfo info)
        : base(graphics, name, info)
    {
        _info = info;

        ValidateInfo();

        Graphics.Log.Print($"Creating Gorgon buffer '{Name}'.", LoggingLevel.Simple);

        CreateNative();        

        this.RegisterDisposable(Graphics);
    }
}
