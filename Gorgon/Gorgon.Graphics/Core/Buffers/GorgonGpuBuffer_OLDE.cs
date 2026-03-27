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
public sealed unsafe class GorgonGpuBuffer_OLDE
    : GorgonGpuResource, IGorgonGpuBufferInfo
{
    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="Format">The format of the view.</param>
    /// <param name="Value1">The first key value.</param>
    /// <param name="Value2">The second key value.</param>
    private readonly record struct ViewKey(BufferFormat Format, long Value1, long Value2);

    private ComPtr<D3D12MA_Allocation> _allocation;

    private CpuBufferAllocation _uploadAllocation;
    private readonly Lock _viewLock = new();
    private readonly Dictionary<ViewKey, GorgonBufferRenderTargetView> _rtvs = [];
    private readonly Dictionary<ViewKey, GorgonConstantBufferView> _cbvs = [];
    private readonly Dictionary<ViewKey, GorgonStructuredBufferView> _structs = [];
    private readonly Dictionary<ViewKey, GorgonShaderBufferView> _srvs = [];
    private readonly Dictionary<ViewKey, GorgonResourceView> _uavs = [];
    private readonly GorgonGpuBufferInfo _info;
    private bool _hasNewData = true;
    private readonly GorgonNativeBuffer<byte> _dynamicCpuData = [];

    /// <summary>
    /// Property to return the buffer containing the CPU data to send to the GPU for dynamic buffers.
    /// </summary>
    internal GorgonPtr<byte> CpuData
    {
        get;
        private set;
    } = GorgonPtr<byte>.NullPtr;

    /// <inheritdoc/>
    public long SizeInBytes => _info.SizeInBytes;

    /// <inheritdoc/>
    public BufferUsage Usage => _info.Usage;

    /// <inheritdoc/>
    public bool IsRenderTarget => _info.IsRenderTarget;

    /// <inheritdoc/>
    public bool IsConstantBuffer => _info.IsConstantBuffer;

    /// <inheritdoc/>
    public bool IsUnorderedAccess => _info.IsUnorderedAccess;

    /// <summary>
    /// We not using this.
    /// </summary>
    public int Alignment => 0;

    /// <summary>
    /// Function to validate the settings for the buffer.
    /// </summary>
    private void ValidateInfo()
    {
        switch (_info.Usage)
        {
            case BufferUsage.Download when _info.IsConstantBuffer || _info.IsRenderTarget || _info.IsUnorderedAccess:
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_DOWNLOAD, Name));
            case BufferUsage.Upload or BufferUsage.DynamicPerFrame when _info.IsRenderTarget:
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RTV_BUFFER_NOT_DEFAULT, Name));
        }

        if (SizeInBytes < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CALL_ON_ACTIVE_FRAME, SizeInBytes, 1));
        }
    }

    /// <summary>
    /// Function to flush dynamic per frame data to the GPU.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FlushDynamicData()
    {
        if ((Usage != BufferUsage.DynamicPerFrame)
             || ((_uploadAllocation.IsAvailable) && (!_hasNewData)))
        {
            return;
        }

        Graphics.UploadHeaps.Allocate((ulong)SizeInBytes, IsConstantBuffer ? D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT : 0, out _uploadAllocation);
        Debug.Assert(_uploadAllocation.IsAvailable, $"Failed to allocate transient memory for buffer {Name}.");
        NativeMemory.Copy((void*)_dynamicCpuData, _uploadAllocation.CpuPointer, (nuint)SizeInBytes);

        _hasNewData = false;
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {        
        if (disposing)
        {
            _uploadAllocation = default;

            if ((Usage is BufferUsage.Upload or BufferUsage.Download) && (!CpuData.Equals(GorgonPtr<byte>.NullPtr)))
            {
                D3DResource.Get()->Unmap(0, null);
            }

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

            CpuData = GorgonPtr<byte>.NullPtr;

            this.UnregisterDisposable(Graphics);
        }

        
        _allocation.Dispose();

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    private protected override void OnGetResourceInfo(out GpuResourceInfo resourceInfo)
    {
        D3D12_RESOURCE_DESC1 desc = D3DResource.Get()->GetDesc1();
        desc.Width = (ulong)SizeInBytes;
        resourceInfo = GpuResourceInfo.FromD3D(in desc);
    }

    /// <inheritdoc/>
    private protected override ComPtr<ID3D12Resource2> OnCreateNative(out D3D12_RESOURCE_DESC1 desc)
    {
        ulong sizeInBytes = (ulong)(IsConstantBuffer ? _info.SizeInBytes.AlignUp(D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT) : _info.SizeInBytes);

        // For dynamic resources, get the memory from our upload resource pool.
        if (_info.Usage == BufferUsage.DynamicPerFrame)
        {
            desc = D3D12_RESOURCE_DESC1.Buffer(sizeInBytes, D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS);
            return default;
        }

        D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;

        if (IsRenderTarget)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
        }

        if (IsUnorderedAccess)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
        }

        if (Graphics.Adapter.HasTightAlignmentSupport)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_USE_TIGHT_ALIGNMENT;
        }

        ComPtr<ID3D12Resource2> result = default;
        ComPtr<D3D12MA_Allocation> resourcePtr = default;
        string bufferName = $"D3D12 Buffer {Name}";        

        Graphics.Log.Print($"Created D3D 12 buffer resource object for '{Name}' with flags {flags}.", LoggingLevel.Verbose);

        D3D12MA_ALLOCATION_DESC allocDesc = new(Usage.ToD3DHeapType(Graphics));
        desc = D3D12_RESOURCE_DESC1.Buffer(sizeInBytes, flags);

        fixed (D3D12_RESOURCE_DESC1* descPtr = &desc)
        {
            Graphics.Allocator.Get()->CreateResource3(&allocDesc, descPtr,
                D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
                null, 0, null,
                resourcePtr.GetAddressOf(), null, null)
                .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE);

            _allocation = resourcePtr;

            using ComPtr<ID3D12Resource> newRes = new(resourcePtr.Get()->GetResource());
            newRes.As(ref result)
                .ThrowIfFailed(hr => throw new InvalidCastException());

            result.SetD3DDebugName(bufferName);

            return result;
        }
    }

    /// <summary>
    /// Function to inform this buffer that the buffer has had its data changed.
    /// </summary>
    internal void DynamicDataChanged() => _hasNewData = true;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref readonly CpuBufferAllocation GetGpuAddress()
    {        
        FlushDynamicData();

        Debug.Assert(_uploadAllocation.IsAvailable, "The allocated dynamic resource memory is invalid.");

        return ref _uploadAllocation;
    }

    /// <summary>
    /// Function to create a new constant buffer view for this buffer.
    /// </summary>
    /// <param name="offset">[Optional] The offset, in bytes, within the buffer to start viewing at.</param>
    /// <param name="size">[Optional[ The size, in bytes, of the buffer to view.</param>
    /// <returns>A new <see cref="GorgonConstantBufferView"/> used to send constant data to the GPU.</returns>
    /// <exception cref="NotSupportedException">Thrown if the <see cref="Usage"/> is set to <see cref="BufferUsage.Download"/>.</exception>
    /// <exception cref="GorgonException">Thrown if the view could not be created because the buffer is less than 256 bytes in size.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> is less than 0, or the <paramref name="size"/> is less than 256 bytes.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the <paramref name="size"/>, aligned to 256 bytes, is larger than <see cref="SizeInBytes"/>.</exception>
    /// <remarks>
    /// <para>
    /// To access constant data in the shaders, a constant buffer view must be passed to the shader. This view will indicate that the entire buffer, or a portion of it can be used to represent shader 
    /// constants. 
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// The constant buffer view <b>MUST</b> be aligned to 256 bytes. This method will adjust the <paramref name="size"/> and the <paramref name="offset"/> values internally, however the aligned 
    /// <paramref name="size"/> and <paramref name="offset"/> will be used in determining if the view fits within the <see cref="SizeInBytes"/> of the buffer. Therefore, the error message will reflect this 
    /// alignment and may differ from the values passed in to the <paramref name="size"/> and <paramref name="offset"/> parameters.
    /// </para>
    /// <para>
    /// These aligned values will also reflect in the <see cref="GorgonConstantBufferView.Size"/> and <see cref="GorgonConstantBufferView.Offset"/> properties on the <see cref="GorgonConstantBufferView"/> 
    /// returned from this method, and therefore may not match the parameter values passed to the method.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonConstantBufferView"/>
    /// <seealso cref="BufferUsage"/>
    public GorgonConstantBufferView GetConstantBufferView(long offset = 0, long? size = null)
    {
        using (_viewLock.EnterScope())
        {
            if (!IsConstantBuffer)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_NOT_CONSTANT_BUFFER, Name));
            }

            size ??= SizeInBytes;
            long alignedSize = size.Value.AlignUp(D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
            long alignedOffset = offset.AlignUp(D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);

            GorgonConstantBufferView.ValidateCbv(Name, Usage, SizeInBytes, alignedOffset, alignedSize);

            ViewKey key = new(BufferFormat.Unknown, alignedOffset, alignedSize);

            if ((_cbvs.TryGetValue(key, out GorgonConstantBufferView? result)) && (result.D3DCpuHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT))
            {
                return result;
            }

            return _cbvs[key] = new GorgonConstantBufferView(Graphics, Name, this, alignedOffset, alignedSize, false);            
        }
    }

#warning FINISH: This is not complete, just here as a marker/template.  Needs validation and documentation.
    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="format"></param>
    /// <param name="startElement"></param>
    /// <param name="elementCount"></param>
    /// <returns></returns>
    /// <exception cref="GorgonException"></exception>
    public GorgonBufferRenderTargetView GetRenderTargetView(BufferFormat format, long startElement = 0, long? elementCount = null)
    {
        using (_viewLock.EnterScope())
        {
            if (!Graphics.FormatSupport[format].IsRenderTargetFormat)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_RENDER_TARGET_FORMAT, format));
            }

            GorgonFormatInfo info = new(format);

            elementCount ??= SizeInBytes / info.SizeInBytes;
            startElement /= info.SizeInBytes;

            ArgumentOutOfRangeException.ThrowIfLessThan(startElement, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(elementCount.Value, 1);

            ViewKey key = new(format, startElement, elementCount.Value);

            if ((_rtvs.TryGetValue(key, out GorgonBufferRenderTargetView? result))
                && (result.D3DCpuHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT))
            {
                return result;
            }

            return _rtvs[key] = new(Graphics, $"{Name} Render Target View", this, format, info, startElement, elementCount.Value, false);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuBuffer_OLDE"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <exception cref="GorgonException"><para>Thrown if the buffer cannot be created because the size is less than 1 byte.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the buffer is a render target, constant buffer, or unordered access buffer and the <see cref="BufferUsage"/> is set to <see cref="BufferUsage.Download"/>.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the buffer is a render target and the <see cref="BufferUsage"/> is not set to <see cref="BufferUsage.Default"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This creates a generic buffer that can hold data on the GPU, or upload to the GPU. The buffer data has no structure. That is defined by views that can be created from the buffer. The types of views 
    /// available are defined in the <see cref="GorgonGpuBufferInfo"/> flags.
    /// </para>
    /// <para>
    /// Most buffers will require a minimum <see cref="GorgonGpuBufferInfo.SizeInBytes"/> of 1 byte. However, some views will require the buffer have a specific minimum size (e.g. constant buffers must be at 
    /// least 256 bytes, render target views must be at least the size of a <see cref="BufferFormat"/> format size, etc...).  
    /// </para>
    /// <para>
    /// The <paramref name="info"/> also contains a <see cref="GorgonGpuBufferInfo.Usage"/> flag which indicates how often the data can be updated in a buffer. Below is a description of how to use the usage 
    /// flags with a buffer.
    /// <inheritdoc cref="IGorgonGpuBufferInfo.Usage" path="/remarks/para/list"/>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="BufferUsage"/>
    public GorgonGpuBuffer_OLDE(GorgonGraphics graphics, string name, GorgonGpuBufferInfo info)
        : base(graphics, name)
    {        
        static GorgonPtr<byte> MapBuffer(ComPtr<ID3D12Resource2> resource, long sizeInBytes)
        {
            byte* result = null;
            resource.Get()->Map(0, null, (void **)&result);

            return new GorgonPtr<byte>(result, sizeInBytes);
        }

        _info = new GorgonGpuBufferInfo(info);
        ValidateInfo();

        Graphics.Log.Print($"Creating Gorgon buffer '{Name}'.", LoggingLevel.Simple);

        CreateNative_OLDE();

        long sizeInBytes = info.IsConstantBuffer ? info.SizeInBytes.AlignUp(D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT) : info.SizeInBytes;

        CpuData = info.Usage switch
        {
            BufferUsage.DynamicPerFrame => _dynamicCpuData = new GorgonNativeBuffer<byte>(sizeInBytes),
            BufferUsage.Upload or BufferUsage.Download => MapBuffer(D3DResource, sizeInBytes),
            _ => GorgonPtr<byte>.NullPtr
        };

        this.RegisterDisposable(Graphics);
    }
}
