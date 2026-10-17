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

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A buffer used to hold data to be used by the GPU.
/// </summary>
/// <remarks>
/// <para>
/// This is a generic data buffer that is used by the GPU to read, and write, data. Applications can use these buffers to send various types of data, such as vertices, or user defined types to the GPU and 
/// then use shaders to read from, or using read/write views, write to the buffer. The type of data in the buffer can be anything a user needs.
/// </para>
/// <para>
/// Buffers, in some scenarios, require alignment in order for the shader(s) to read/write correctly. For example, buffers containing constant values for a shader (i.e. a constant buffer) <b>must</b> be 
/// aligned to 256 bytes. While buffers containing structured data might require the buffer to be aligned to the size, in bytes, of that data structure. Applications can create a buffer with an alignment 
/// using the <see cref="GorgonGpuBufferInfo.Alignment"/> property on the <see cref="GorgonGpuBufferInfo"/> type that is passed to the constructor.
/// </para>
/// <para>
/// Because the data in the buffer is nothing more than a blob of bytes, the GPU needs a way to interpret how to read the data. This can be done through views (which the buffer can create through one of its 
/// <c>Get*View</c> methods). For example, if a buffer needs to be treated as a constant buffer, the user will create a <see cref="GorgonConstantBufferView"/>, or structured data through a 
/// <see cref="GorgonStructuredBufferView"/>. Users can pass these views to shaders by passing their appropriate handle as a constant value via a <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> 
/// method on a <see cref="GorgonCommandList"/>, or via a <see cref="GorgonConstantBufferView"/> (obviously, a parent view needs to be passed by the aforementioned Write method).
/// </para>
/// <para>
/// <note type="information">
/// <para>
/// While these buffers can hold many different types of data, they cannot be used as an index buffer. For more information, please consult the <see cref="GorgonIndexBuffer"/> documentation.
/// </para>
/// </note>
/// </para>
/// <para>
/// Read/write views (e.g. <see cref="GorgonStructuredBufferRwView"/>) allow shaders to write to the buffer. These are created through one of the <c>Get*ReadWriteView</c> methods, and require that the buffer 
/// be created with the <see cref="GorgonCommonBufferInfo.HasReadWriteAccess"/> property set to <b>true</b>.
/// </para>
/// <para type="uav_readwrite">
/// <note type="information">
/// <para>
/// In Gorgon, read/write views are synonymous with Unordered Access Views (UAVs).
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonVideoAdapterInfo"/>
/// <seealso cref="GorgonGpuBufferInfo"/>
/// <seealso cref="GorgonIndexBuffer"/>
/// <seealso cref="GorgonConstantBufferView"/>
/// <seealso cref="GorgonStructuredBufferView"/>
/// <seealso cref="GorgonCommandList"/>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
public sealed unsafe class GorgonGpuBuffer
    : GorgonGpuBufferCommon, IGorgonGpuBufferInfo
{
    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="Format">The format of the view.</param>
    /// <param name="HasCounter"><b>true</b> if the view has a counter attached (structured read/write views only), <b>false</b> if not.</param>
    /// <param name="StartIndex">The index of the first element in the view.</param>
    /// <param name="Count">The number of elements in the view.</param>
    /// <param name="StructSize">The size, in bytes, of a structured element, or 0 for raw and typed views.</param>
    private readonly record struct ViewKey(byte Format, bool HasCounter, int StartIndex, int Count, int StructSize);

    private readonly Lock _viewLock = new();
    private readonly GorgonGpuBufferInfo _info;
    private GorgonConstantBufferView? _cbv;
    private readonly Dictionary<ViewKey, GorgonStructuredBufferView> _structs = [];
    private readonly Dictionary<ViewKey, GorgonRawBufferView> _raws = [];
    private readonly Dictionary<ViewKey, GorgonTypedBufferView> _typeds = [];
    private readonly Dictionary<ViewKey, GorgonStructuredBufferRwView> _structUavs = [];
    private readonly Dictionary<ViewKey, GorgonRawBufferRwView> _rawUavs = [];
    private readonly Dictionary<ViewKey, GorgonTypedBufferRwView> _typedUavs = [];
    private GpuBufferAllocation _bufferAllocation = GpuBufferAllocation.Null;
    private readonly MegaBufferPool _megaBuffer;

    /// <summary>
    /// Property to return the offset, in bytes, of a suballocated buffer within a larger buffer.
    /// </summary>
    internal override ulong ResourceOffset => _bufferAllocation.Offset;

    /// <inheritdoc/>
    internal override bool IsMegaBufferResource => true;

    /// <inheritdoc/>
    public int Alignment => _info.Alignment;


    /// <summary>
    /// Function to create the native backing resources for the buffer.
    /// </summary>
    private void CreateNative()
    {
        _megaBuffer.Allocate((ulong)SizeInBytes, out _bufferAllocation, (uint)Alignment);
        AssignResource(in _megaBuffer[in _bufferAllocation]);
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Remove all the child views.
            GorgonConstantBufferView? cbv = Interlocked.Exchange(ref _cbv, null);

            // If the cbv owns this resource, then do not call its dispose method.
            if ((cbv is not null) && (!cbv.OwnsResource))
            {
                cbv?.Dispose();
            }

            // One of the few places where I don't care about using LINQ.
            // We only call dispose for the views that do not own this resource.
            foreach (GorgonResourceView view in _structs.Values.Cast<GorgonResourceView>()
                                                               .Concat(_raws.Values.Cast<GorgonResourceView>())
                                                               .Concat(_typeds.Values.Cast<GorgonResourceView>())
                                                               .Concat(_structUavs.Values.Cast<GorgonResourceView>())
                                                               .Concat(_typedUavs.Values.Cast<GorgonResourceView>())
                                                               .Concat(_rawUavs.Values.Cast<GorgonResourceView>())
                                                               .Where(v => !v.OwnsResource))
            {
                view.Dispose();
            }

            _typeds.Clear();
            _structs.Clear();
            _raws.Clear();
            _typedUavs.Clear();
            _structUavs.Clear();
            _rawUavs.Clear();

            if (!_bufferAllocation.IsNull)
            {
                _megaBuffer.Free(ref _bufferAllocation);
            }
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    /// <exception cref="GorgonException"><para>Thrown if the alignment for the buffer is less than 0.</para>
    /// <para>Thrown if the buffer size is less than 1 byte.</para>
    /// </exception>
    private protected override void ValidateInfo()
    {
        if (Alignment < 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_INVALID_ALIGNMENT, Alignment));
        }

        if (SizeInBytes < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL, Name, 1));
        }
    }

    /// <inheritdoc/>
    private protected override void OnGetResourceInfo(out GpuResourceInfo resourceInfo)
    {
        D3D12_RESOURCE_DESC1 desc = D3DResource.Get()->GetDesc1();
        desc.Width = (ulong)SizeInBytes;
        desc.Alignment = (uint)Alignment;
        resourceInfo = GpuResourceInfo.FromD3D(in desc);
    }

    /// <summary>
    /// Function to create a new constant buffer view for this buffer.
    /// </summary>
    /// <param name="owned"><b>true</b> if the buffer is owned by the view, or <b>false</b> if not.</param>
    /// <returns>A <see cref="GorgonConstantBufferView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonConstantBufferView.ValidateConstantView(string, int, ulong, ulong)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to access data in the buffer as shader constants. This type of view is typically used for scenarios where the buffer is updated once per frame, or less. Otherwise, applications 
    /// should use one of the <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> methods on the command list.
    /// </para>
    /// <para type="bindless_doc">
    /// In Gorgon's bindless model, passing the view is done by writing a constant value via the <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> method. The value is the handle of the view, which 
    /// is retrieved by the <c>GetViewHandle</c> method on the view. Then, in the shader, the resource can be indexed easily by the <c>ResourceDescriptorHeap[handle]</c> intrinsic function (where <c>handle</c> 
    /// is the name of the constant that was updated).
    /// </para>
    /// <para>
    /// Unlike other view types, only 1 constant buffer view can be used by a buffer. This is because the constant buffer covers the range of the entire buffer's size.
    /// </para>
    /// <para type="constant_alignment">
    /// <note type="important">
    /// <para>
    /// The buffer used for the constant buffer view <b>MUST</b> be aligned to <see cref="GorgonConstantBufferView.AlignmentRequirement"/> (256 bytes). Ensure the <see cref="GorgonGpuBufferInfo.Alignment"/> 
    /// property on the <see cref="GorgonGpuBufferInfo"/> passed to the buffer's constructor is set to match the <see cref="GorgonConstantBufferView.AlignmentRequirement"/> upon buffer creation.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonConstantBufferView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="GorgonConstantBufferView"/>
    internal GorgonConstantBufferView GetConstantBufferView(bool owned)
    {
        using (_viewLock.EnterScope())
        {
            ulong alignedSize = _bufferAllocation.SizeInBytes.AlignDown((uint)GorgonConstantBufferView.AlignmentRequirement);

            GorgonConstantBufferView.ValidateConstantView(Name, Alignment, alignedSize, ResourceOffset);

            if (_cbv is not null)
            {
                return _cbv;
            }

            _cbv = new GorgonConstantBufferView(Graphics, Name, this, (uint)alignedSize, owned);
            return _cbv;
        }
    }

    /// <summary>
    /// Function to create a structured buffer view.
    /// </summary>
    /// <param name="structSize">The size, in bytes, of a single element in the view. Must be a multiple of <see cref="GorgonStructuredBufferView.MinimumElementSize"/> (4 bytes).</param>
    /// <param name="startIndex">[Optional] The index of the first element in the buffer to start the view at.</param>
    /// <param name="count">[Optional] The number of elements to view.</param>
    /// <param name="owned"><inheritdoc cref="GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonStructuredBufferView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonStructuredBufferView.ValidateStructuredView(string, int, long, ulong)" path="/exception"/>
    /// <remarks>
    /// <para type="intro">
    /// This allows shaders to access buffer data as a custom data structure.
    /// </para>
    /// <para type="requirements">
    /// The <paramref name="structSize"/> must be a multiple of <see cref="GorgonStructuredBufferView.MinimumElementSize"/> (4 bytes). If it is not, then an exception is thrown.
    /// </para>
    /// <para type="requirements">
    /// Buffers used as a structured data buffer must be at least the size of a single element, which is the size of a single structure in the buffer. If the buffer is too small, an exception is thrown.
    /// </para>
    /// <para type="requirements">
    /// When the structured buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the <paramref name="structSize"/>. Otherwise, an exception will be thrown when creating 
    /// the structured view. This provides protection against data misalignment.
    /// </para>
    /// <para type="range">
    /// The <paramref name="startIndex"/> is clipped to the last element in the buffer. The <paramref name="count"/> is clipped to the number of elements from the <paramref name="startIndex"/> to the end of 
    /// the buffer, and is at least 1. If the <paramref name="count"/> is <b>null</b>, then the view covers every element from the <paramref name="startIndex"/> to the end of the buffer.
    /// </para>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonShaderBufferView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="GorgonStructuredBufferView"/>
    internal GorgonStructuredBufferView GetStructuredBufferView(int structSize, long startIndex, int? count, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            GorgonStructuredBufferView.ValidateStructuredView(Name, structSize, SizeInBytes, ResourceOffset);

            long maxElements = (SizeInBytes / structSize).Max(1);

            startIndex = startIndex.Max(0).Min(maxElements - 1);
            count ??= (int)(maxElements - startIndex);
            count = count.Value.Max(1).Min((int)(maxElements - startIndex));

            ViewKey key = new((byte)BufferFormat.Unknown, false, (int)startIndex, count.Value, structSize);

            if (_structs.TryGetValue(key, out GorgonStructuredBufferView? result))
            {
                return result;
            }

            return _structs[key] = new GorgonStructuredBufferView(Graphics, Name, this, startIndex.Min(maxElements - 1), structSize, count.Value, owned);
        }
    }

    /// <summary>
    /// Function to create a structured read/write view for the buffer.
    /// </summary>
    /// <param name="structSize"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='structSize']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <param name="includeCounter"><b>true</b> to attach a counter to the view, <b>false</b> to create the view without one.</param>
    /// <param name="owned"><inheritdoc cref="GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonStructuredBufferRwView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonStructuredBufferRwView.ValidateStructuredView(string, int, long, ulong, bool)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to read and write buffer data as a custom data structure.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='requirements']"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <para>
    /// A view with a counter, and a view without one, are separate views, even when they cover the same elements.
    /// </para>
    /// <inheritdoc cref="GorgonStructuredBufferRwView" path="/remarks/para[@type='counter']"/>
    /// <inheritdoc cref="GorgonStructuredBufferRwView" path="/remarks/para[@type='counter_note']"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonShaderBufferRwView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="GorgonStructuredBufferRwView"/>
    internal GorgonStructuredBufferRwView GetStructuredBufferReadWriteView(int structSize, long startIndex, int? count, bool includeCounter, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            GorgonStructuredBufferRwView.ValidateStructuredView(Name, structSize, SizeInBytes, ResourceOffset, HasReadWriteAccess);

            long maxElements = (SizeInBytes / structSize).Max(1);

            startIndex = startIndex.Max(0).Min(maxElements - 1);
            count ??= (int)(maxElements - startIndex);
            count = count.Value.Max(1).Min((int)(maxElements - startIndex));

            ViewKey key = new((byte)BufferFormat.Unknown, includeCounter, (int)startIndex, count.Value, structSize);

            if (_structUavs.TryGetValue(key, out GorgonStructuredBufferRwView? result))
            {
                return result;
            }

            return _structUavs[key] = new GorgonStructuredBufferRwView(Graphics, Name, this, startIndex, structSize, count.Value, includeCounter, owned);
        }
    }

    /// <summary>
    /// Function to create a raw buffer view.
    /// </summary>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <param name="owned"><inheritdoc cref="GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonRawBufferView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonRawBufferView.ValidateRawView(string, long, ulong)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to access buffer data as a series of raw byte data.
    /// </para>
    /// <para type="requirements">
    /// Buffers used as a raw data buffer must be at least the size of a <see cref="GorgonRawBufferView.MinimumElementSize"/> (4 bytes). If the buffer is too small, an exception is thrown.
    /// </para>
    /// <para type="requirements">
    /// When the raw buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the <see cref="GorgonRawBufferView.AlignmentRequirement"/>. Otherwise, an exception will be 
    /// thrown when creating the raw view. This provides protection against data misalignment.
    /// </para>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonShaderBufferView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="GorgonRawBufferView"/>
    internal GorgonRawBufferView GetRawBufferView(long startIndex, int? count, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            GorgonRawBufferView.ValidateRawView(Name, SizeInBytes, ResourceOffset);

            long maxElements = (SizeInBytes / GorgonRawBufferView.MinimumElementSize).Max(1);

            startIndex = startIndex.Max(0).Min(maxElements - 1);
            count ??= (int)(maxElements - startIndex);
            count = count.Value.Max(1).Min((int)(maxElements - startIndex));

            ViewKey key = new((byte)BufferFormat.Unknown, false, (int)startIndex, count.Value, 0);

            if (_raws.TryGetValue(key, out GorgonRawBufferView? result))
            {
                return result;
            }

            return _raws[key] = new GorgonRawBufferView(Graphics, Name, this, startIndex, count.Value, owned);
        }
    }

    /// <summary>
    /// Function to create a raw read/write view for the buffer.
    /// </summary>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <param name="owned"><inheritdoc cref="GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonRawBufferRwView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonRawBufferRwView.ValidateRawView(string, long, ulong, bool)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to read and write buffer data as a series of raw byte data.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/remarks/para[@type='requirements']"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonShaderBufferRwView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
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

            ViewKey key = new((byte)BufferFormat.Unknown, false, (int)startIndex, count.Value, 0);

            if (_rawUavs.TryGetValue(key, out GorgonRawBufferRwView? result))
            {
                return result;
            }

            return _rawUavs[key] = new GorgonRawBufferRwView(Graphics, Name, this, startIndex, count.Value, owned);
        }
    }

    /// <summary>
    /// Function to create a typed buffer view.
    /// </summary>
    /// <param name="format">The format type for the view.</param>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <param name="owned"><inheritdoc cref="GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonTypedBufferView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonTypedBufferView.ValidateTypedView(string, GorgonFormatInfo, GorgonBufferFormatSupport, long, ulong)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to access buffer data as a series of simply typed data elements that match a <see cref="BufferFormat"/>.
    /// </para>
    /// <para type="requirements">
    /// Buffers used as a typed data buffer must be at least the size of a <see cref="BufferFormat"/>. If the buffer is too small, an exception is thrown. Applications can get the format size by using the 
    /// <see cref="GorgonFormatInfo"/> object and reading the <see cref="GorgonFormatInfo.SizeInBytes"/> property.
    /// </para>
    /// <para type="requirements">
    /// When the typed buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the size of a <see cref="BufferFormat"/>. Otherwise, an exception will be thrown when creating 
    /// the typed view. This provides protection against data misalignment.
    /// </para>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonShaderBufferView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="GorgonTypedBufferView"/>
    internal GorgonTypedBufferView GetTypedBufferView(BufferFormat format, long startIndex, int? count, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            GorgonFormatInfo formatInfo = new(format);
            GorgonTypedBufferView.ValidateTypedView(Name, formatInfo, Graphics.FormatSupport[format], SizeInBytes, ResourceOffset);

            long maxElements = (SizeInBytes / formatInfo.SizeInBytes).Max(1);

            startIndex = startIndex.Max(0).Min(maxElements - 1);
            count ??= (int)(maxElements - startIndex);
            count = count.Value.Max(1).Min((int)(maxElements - startIndex));

            ViewKey key = new((byte)format, false, (int)startIndex, count.Value, 0);

            if (_typeds.TryGetValue(key, out GorgonTypedBufferView? result))
            {
                return result;
            }

            return _typeds[key] = new GorgonTypedBufferView(Graphics, Name, this, formatInfo, startIndex, count.Value, owned);
        }
    }

    /// <summary>
    /// Function to create a typed read/write view for the buffer.
    /// </summary>
    /// <param name="format"><inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/param[@name='format']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <param name="owned"><inheritdoc cref="GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonTypedBufferRwView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonTypedBufferRwView.ValidateTypedView(string, GorgonFormatInfo, GorgonBufferFormatSupport, long, ulong, bool)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to read and write buffer data as a series of simply typed data elements that match a <see cref="BufferFormat"/>.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/remarks/para[@type='requirements']"/>
    /// <inheritdoc cref="GorgonTypedBufferRwView" path="/remarks/para[@type='format_support']"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonShaderBufferRwView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="GorgonFormatInfo"/>
    /// <seealso cref="GorgonTypedBufferRwView"/>
    internal GorgonTypedBufferRwView GetTypedBufferReadWriteView(BufferFormat format, long startIndex, int? count, bool owned)
    {
        using (_viewLock.EnterScope())
        {
            GorgonFormatInfo formatInfo = new(format);
            GorgonTypedBufferRwView.ValidateTypedView(Name, formatInfo, Graphics.FormatSupport[format], SizeInBytes, ResourceOffset, HasReadWriteAccess);

            long maxElements = (SizeInBytes / formatInfo.SizeInBytes).Max(1);

            startIndex = startIndex.Max(0).Min(maxElements - 1);
            count ??= (int)(maxElements - startIndex);
            count = count.Value.Max(1).Min((int)(maxElements - startIndex));

            ViewKey key = new((byte)format, false, (int)startIndex, count.Value, 0);

            if (_typedUavs.TryGetValue(key, out GorgonTypedBufferRwView? result))
            {
                return result;
            }

            return _typedUavs[key] = new GorgonTypedBufferRwView(Graphics, Name, this, formatInfo, startIndex, count.Value, owned);
        }
    }

    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/summary"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/returns"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/exception"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonConstantBufferView GetConstantBufferView() => GetConstantBufferView(false);

    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/summary"/>
    /// <param name="structSize"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='structSize']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/returns"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, and <b>null</b> for the <paramref name="count"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonStructuredBufferView GetStructuredBufferView(int structSize, long startIndex = 0, int? count = null) => GetStructuredBufferView(structSize, startIndex, count, false);

    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/summary"/>
    /// <typeparam name="T">The type of data to view the buffer as. Must be an unmanaged type.</typeparam>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/returns"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='intro']"/>
    /// <para>
    /// The size of the type <typeparamref name="T"/>, in bytes, must be a multiple of <see cref="GorgonStructuredBufferView.MinimumElementSize"/> (4 bytes). If it is not, then an exception is thrown.
    /// </para>
    /// <para type="requirements_t">
    /// Buffers used as a structured data buffer must be at least the size of a single element, which is the size of the type <typeparamref name="T"/>, in bytes. If the buffer is too small, an exception is 
    /// thrown.
    /// </para>
    /// <para type="requirements_t">
    /// When the structured buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the size of the type <typeparamref name="T"/>. Otherwise, an exception will be thrown when 
    /// creating the structured view. This provides protection against data misalignment.
    /// </para>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, and <b>null</b> for the <paramref name="count"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonStructuredBufferView GetStructuredBufferView<T>(long startIndex = 0, int? count = null) where T : unmanaged
        => GetStructuredBufferView(sizeof(T), startIndex, count, false);

    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/summary"/>
    /// <param name="startIndex"><inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/returns"/>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/remarks/para"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, and <b>null</b> for the <paramref name="count"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonRawBufferView GetRawBufferView(long startIndex = 0, int? count = null) => GetRawBufferView(startIndex, count, false);

    /// <inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/summary"/>
    /// <param name="format"><inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/param[@name='format']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/returns"/>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/remarks/para"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, and <b>null</b> for the <paramref name="count"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonTypedBufferView GetTypedBufferView(BufferFormat format, long startIndex = 0, int? count = null) => GetTypedBufferView(format, startIndex, count, false);

    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/summary"/>
    /// <param name="structSize"><inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/param[@name='structSize']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/param[@name='count']"/></param>
    /// <param name="includeCounter">[Optional] <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/param[@name='includeCounter']"/></param>
    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/remarks/para"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, <b>null</b> for the <paramref name="count"/>, and <b>false</b> for the <paramref name="includeCounter"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonStructuredBufferRwView GetStructuredBufferReadWriteView(int structSize, long startIndex = 0, int? count = null, bool includeCounter = false) => GetStructuredBufferReadWriteView(structSize, startIndex, count, includeCounter, false);

    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/summary"/>
    /// <typeparam name="T">The type of data to view the buffer as. Must be an unmanaged type.</typeparam>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/param[@name='count']"/></param>
    /// <param name="includeCounter">[Optional] <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/param[@name='includeCounter']"/></param>
    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/returns"/>
    /// <inheritdoc cref="GetStructuredBufferView{T}(long, int?)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to read and write buffer data as a custom data structure.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <para>
    /// The size of the type <typeparamref name="T"/>, in bytes, must be a multiple of <see cref="GorgonStructuredBufferView.MinimumElementSize"/> (4 bytes). If it is not, then an exception is thrown.
    /// </para>
    /// <inheritdoc cref="GetStructuredBufferView{T}(long, int?)" path="/remarks/para[@type='requirements_t']"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[@type='range']"/>
    /// <inheritdoc cref="GorgonStructuredBufferRwView" path="/remarks/para[@type='counter']"/>
    /// <inheritdoc cref="GorgonStructuredBufferRwView" path="/remarks/para[@type='counter_note']"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// <para>
    /// The default values are 0 for the <paramref name="startIndex"/>, <b>null</b> for the <paramref name="count"/>, and <b>false</b> for the <paramref name="includeCounter"/>.
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetStructuredBufferReadWriteView(int, long, int?, bool, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonStructuredBufferRwView GetStructuredBufferReadWriteView<T>(long startIndex = 0, int? count = null, bool includeCounter = false) where T : unmanaged
        => GetStructuredBufferReadWriteView(sizeof(T), startIndex, count, includeCounter, false);

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
    /// Initializes a new instance of the <see cref="GorgonGpuBuffer"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the buffer cannot be created because the size is less than 1 byte.
    /// </para>
    /// <para>Thrown if the <see cref="GorgonGpuBufferInfo.Alignment"/> value is negative.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This creates a generic buffer that holds data on the GPU. The buffer data has no structure. That is defined by the views that can be created from the buffer (e.g. <see cref="GetConstantBufferView()"/>, 
    /// <see cref="GetStructuredBufferView(int, long, int?)"/>, etc...).
    /// </para>
    /// <para>
    /// Most buffers will require a minimum <see cref="GorgonCommonBufferInfo.SizeInBytes"/> of 1 byte. However, some views will require the buffer have a specific minimum size (e.g. constant buffers must be 
    /// at least 256 bytes, etc...).
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    public GorgonGpuBuffer(GorgonGraphics graphics, string name, GorgonGpuBufferInfo info)
        : base(graphics, name, info)
    {
        _info = info;
        _megaBuffer = graphics.Memory.MegaBuffer;

        ValidateInfo();

        Graphics.Log.Print($"Creating Gorgon buffer '{Name}'.", LoggingLevel.Simple);

        CreateNative();        
    }
}
