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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
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
/// For example, if a vertex buffer has a vertex that is 40 bytes, and describes a rectangle out of 2 triangles, that's 6 vertices * 40 bytes = 240 bytes. Now, with an index buffer, you can use 16-bit indices 
/// and 4 vertices to describe the same rectangle. 12 bytes for the indices, and 160 bytes for the vertices = 172 bytes, a difference of 68 bytes in total. Scale this up by meshes that use tens of thousands of 
/// vertices, and you start seeing some massive gains.
/// </para>
/// <para>
/// The index buffer can consist of indices that are 32 bits wide, or 16 bits wide. The smaller data size means less overhead, but a reduced mesh size (32-bit indices can address 4,294,967,296 vertices, while 
/// 16-bit indices can only address 65536 vertices). The type of data is specified upon creation of the buffer.
/// </para>
/// <para>
/// Shaders can read and write index data through a raw or typed read/write view (see <see cref="GetRawBufferReadWriteView(long, int?)"/> and 
/// <see cref="GetTypedBufferReadWriteView(BufferFormat, long, int?)"/>). This requires that the buffer be created with the <see cref="GorgonCommonBufferInfo.HasReadWriteAccess"/> property set to <b>true</b>.
/// </para>
/// <para>
/// <h3>Why a separate buffer type?</h3>
/// </para>
/// <para>
/// Gorgon works on a system known as bindless rendering. Older graphics APIs forced data to be bound to a pipeline using slots of some kind. This system has several drawbacks around state tracking and is no 
/// longer really representative of how a GPU actually works. With Gorgon, we no longer need this binding system and can just create resources and use them directly from memory. No more state tracking for 
/// resources, no more costly pipeline switches, etc...
/// </para>
/// <para>
/// However, while this system applies to almost all resources, index buffers are required to be bound. This is unavoidable, and as such, the memory architecture for these index buffers is slightly different 
/// and requires they be treated differently than a generic buffer type like <see cref="GorgonGpuBuffer"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonVideoAdapterInfo"/>
/// <seealso cref="GorgonIndexBufferInfo"/>
/// <seealso cref="GorgonGpuBuffer"/>
public sealed unsafe class GorgonIndexBuffer
    : GorgonGpuBufferCommon, IGorgonIndexBufferInfo
{
    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="Format">The format of the view.</param>
    /// <param name="StartIndex">The index of the first element in the view.</param>
    /// <param name="Count">The number of elements in the view.</param>
    private readonly record struct ViewKey(byte Format, int StartIndex, int Count);

    private ComPtr<D3D12MA_Allocation> _bufferAllocation;

    private D3D12_RESOURCE_DESC1 _d3dDesc;
    private readonly GorgonIndexBufferInfo _info;
    private readonly Dictionary<ViewKey, GorgonRawBufferRwView> _rawUavs = [];
    private readonly Dictionary<ViewKey, GorgonTypedBufferRwView> _typedUavs = [];
    private readonly Lock _viewLock = new();

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
            // One of the few places where I don't care about using LINQ.
            // We only call dispose for the views that do not own this resource.
            foreach (GorgonResourceView view in _typedUavs.Values.Cast<GorgonResourceView>()
                                                               .Concat(_rawUavs.Values.Cast<GorgonResourceView>())
                                                               .Where(v => !v.OwnsResource))
            {
                view.Dispose();
            }

            _typedUavs.Clear();
            _rawUavs.Clear();
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
        D3D12_RESOURCE_FLAGS flags = HasReadWriteAccess ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS : D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;
        D3D12_RESOURCE_DESC1 desc = D3D12_RESOURCE_DESC1.Buffer((ulong)SizeInBytes, flags);
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
    /// Function to create a raw read/write view for the buffer.
    /// </summary>
    /// <param name="startIndex"><inheritdoc cref="GorgonGpuBuffer.GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GorgonGpuBuffer.GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <param name="owned"><inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonRawBufferRwView"/> for the buffer.</returns>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="GorgonRawBufferRwView.ValidateRawView(string, long, ulong, bool)" path="/exception/para[@type='norw']"/>
    /// <inheritdoc cref="GorgonRawBufferRwView.ValidateRawView(string, long, ulong, bool)" path="/exception/para[@type='size']"/>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This allows shaders to read and write the index data as a series of raw byte data.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <para type="raw_size">
    /// The buffer must be at least <see cref="GorgonRawBufferRwView.MinimumElementSize"/> (4 bytes) in size. If the buffer is too small, an exception is thrown.
    /// </para>
    /// <inheritdoc cref="GorgonGpuBuffer.GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonShaderBufferRwView.GetViewHandle()"/>
    /// <seealso cref="GorgonIndexBufferInfo"/>
    /// <seealso cref="GorgonRawBufferRwView"/>
    internal GorgonRawBufferRwView GetRawBufferReadWriteView(long startIndex, int? count, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            GorgonRawBufferRwView.ValidateRawView(Name, SizeInBytes, ResourceOffset, HasReadWriteAccess);

            long maxElements = (SizeInBytes / GorgonRawBufferRwView.MinimumElementSize).Max(1);

            startIndex = startIndex.Max(0).Min(maxElements - 1);
            count ??= (int)(maxElements - startIndex);
            count = count.Value.Max(1).Min((int)(maxElements - startIndex));

            ViewKey key = new((byte)BufferFormat.Unknown, (int)startIndex, count.Value);

            if (_rawUavs.TryGetValue(key, out GorgonRawBufferRwView? result))
            {
                return result;
            }

            return _rawUavs[key] = new GorgonRawBufferRwView(Graphics, Name, this, startIndex, count.Value, owned);
        }
    }

    /// <summary>
    /// Function to create a typed read/write view for the buffer.
    /// </summary>
    /// <param name="format">The format type for the view.</param>
    /// <param name="startIndex"><inheritdoc cref="GorgonGpuBuffer.GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GorgonGpuBuffer.GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <param name="owned"><inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonTypedBufferRwView"/> for the buffer.</returns>
    /// <exception cref="GorgonException"><para type="index_format">
    /// Thrown if the <paramref name="format"/> is a floating point format, or its size does not match the size of an index (2 bytes for 16-bit indices, or 4 bytes for 32-bit indices).
    /// </para>
    /// <inheritdoc cref="GorgonTypedBufferRwView.ValidateTypedView(string, GorgonFormatInfo, GorgonBufferFormatSupport, long, ulong, bool)" path="/exception/para[@type='norw']"/>
    /// <inheritdoc cref="GorgonTypedBufferRwView.ValidateTypedView(string, GorgonFormatInfo, GorgonBufferFormatSupport, long, ulong, bool)" path="/exception/para[@type='support']"/>
    /// <inheritdoc cref="GorgonTypedBufferRwView.ValidateTypedView(string, GorgonFormatInfo, GorgonBufferFormatSupport, long, ulong, bool)" path="/exception/para[@type='format']"/>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This allows shaders to read and write the index data as a series of simply typed data elements that match a <see cref="BufferFormat"/>.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <para>
    /// The <paramref name="format"/> must be the same size as an index in the buffer (2 bytes for 16-bit indices, or 4 bytes for 32-bit indices), and cannot be a floating point format. For example, 
    /// <see cref="BufferFormat.R32_UInt"/> or <see cref="BufferFormat.R32_SInt"/> for 32-bit indices, and <see cref="BufferFormat.R16_UInt"/> or <see cref="BufferFormat.R16_SInt"/> for 16-bit indices.
    /// </para>
    /// <inheritdoc cref="GorgonTypedBufferRwView" path="/remarks/para[@type='format_support']"/>
    /// <inheritdoc cref="GorgonGpuBuffer.GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonShaderBufferRwView.GetViewHandle()"/>
    /// <seealso cref="GorgonIndexBufferInfo"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="GorgonTypedBufferRwView"/>
    internal GorgonTypedBufferRwView GetTypedBufferReadWriteView(BufferFormat format, long startIndex, int? count, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            GorgonFormatInfo formatInfo = new(format);

            if ((formatInfo.IsFloatingPoint) 
                || (((Use32BitIndices) && (formatInfo.SizeInBytes != sizeof(uint))) || (!Use32BitIndices) && (formatInfo.SizeInBytes != sizeof(ushort))))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INDEXBUFFER_TYPED_VIEW_INCOMPATIBLE, Name, format, formatInfo.BitDepth));
            }

            GorgonTypedBufferRwView.ValidateTypedView(Name, formatInfo, Graphics.FormatSupport[format], SizeInBytes, ResourceOffset, HasReadWriteAccess);

            long maxElements = (SizeInBytes / formatInfo.SizeInBytes).Max(1);

            startIndex = startIndex.Max(0).Min(maxElements - 1);
            count ??= (int)(maxElements - startIndex);
            count = count.Value.Max(1).Min((int)(maxElements - startIndex));

            ViewKey key = new((byte)format, (int)startIndex, count.Value);

            if (_typedUavs.TryGetValue(key, out GorgonTypedBufferRwView? result))
            {
                return result;
            }

            return _typedUavs[key] = new GorgonTypedBufferRwView(Graphics, Name, this, formatInfo, startIndex, count.Value, owned);
        }
    }

    /// <inheritdoc cref="GetRawBufferReadWriteView(long, int?, bool)" path="/summary"/>
    /// <param name="startIndex"><inheritdoc cref="GetRawBufferReadWriteView(long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetRawBufferReadWriteView(long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetRawBufferReadWriteView(long, int?, bool)" path="/returns"/>
    /// <inheritdoc cref="GetRawBufferReadWriteView(long, int?, bool)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GetRawBufferReadWriteView(long, int?, bool)" path="/remarks/para"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, and <b>null</b> for the <paramref name="count"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetRawBufferReadWriteView(long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonRawBufferRwView GetRawBufferReadWriteView(long startIndex = 0, int? count = null) => GetRawBufferReadWriteView(startIndex, count, false);

    /// <inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/summary"/>
    /// <param name="format"><inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/param[@name='format']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/returns"/>
    /// <inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/remarks/para"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, and <b>null</b> for the <paramref name="count"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetTypedBufferReadWriteView(BufferFormat, long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonTypedBufferRwView GetTypedBufferReadWriteView(BufferFormat format, long startIndex = 0, int? count = null) => GetTypedBufferReadWriteView(format, startIndex, count, false);

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
    /// <exception cref="GorgonException">Thrown if the size of the buffer is less than the size of a single index (2 bytes for 16-bit indices, or 4 bytes for 32-bit indices).</exception>
    /// <remarks>
    /// <para>
    /// This creates a buffer that holds index data on the GPU. The size of each index is defined by the <see cref="GorgonIndexBufferInfo.Use32BitIndices"/> value on the <paramref name="info"/> parameter.
    /// </para>
    /// <para>
    /// The <see cref="GorgonCommonBufferInfo.SizeInBytes"/> must be at least the size of a single index.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonIndexBufferInfo"/>
    public GorgonIndexBuffer(GorgonGraphics graphics, string name, GorgonIndexBufferInfo info)
        : base(graphics, name, info)
    {
        _info = info;

        ValidateInfo();

        Graphics.Log.Print($"Creating Gorgon index buffer '{Name}'.", LoggingLevel.Simple);

        CreateNative();        
    }
}
