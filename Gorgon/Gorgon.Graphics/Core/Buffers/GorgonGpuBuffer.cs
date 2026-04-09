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

using System.Diagnostics;
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
/// <para type="uav_readwrite">
/// <note type="Information">
/// In Gorgon, read/write views are synonymous with Unordered Access Views (UAVs).
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
    /// <param name="Value1">The first key value.</param>
    /// <param name="Value2">The second key value.</param>
    private readonly record struct ViewKey(BufferFormat Format, long Value1, long Value2);

    private readonly Lock _viewLock = new();
    private readonly GorgonGpuBufferInfo _info;
    private GorgonConstantBufferView? _cbv;
    private readonly Dictionary<ViewKey, GorgonStructuredBufferView> _structs = [];
    private readonly Dictionary<ViewKey, GorgonRawBufferView> _raws = [];
    private readonly Dictionary<ViewKey, GorgonTypedBufferView> _typeds = [];
    private readonly Dictionary<ViewKey, GorgonResourceView> _uavs = [];    
    private GpuBufferAllocation _bufferAllocation = GpuBufferAllocation.Null;

    /// <summary>
    /// Property to return the offset, in bytes, of a suballocated buffer within a larger buffer.
    /// </summary>
    internal override ulong ResourceOffset => _bufferAllocation.Offset;

    /// <inheritdoc/>
    public int Alignment => _info.Alignment;


    /// <summary>
    /// Function to create the native backing resources for the buffer.
    /// </summary>
    private void CreateNative()
    {
        Graphics.MegaBuffer.Allocate((ulong)SizeInBytes, out _bufferAllocation, (uint)Alignment);
        AssignResource(in Graphics.MegaBuffer.D3DBuffer);
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
                                                               .Concat(_uavs.Values.Cast<GorgonResourceView>())
                                                               .Where(v => !v.OwnsResource))
            {
                view.Dispose();
            }

            _structs.Clear();
            _raws.Clear();
            _uavs.Clear();

            if (!_bufferAllocation.IsNull)
            {
                Graphics.MegaBuffer.Free(ref _bufferAllocation);
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
    /// is retrieved by the <see cref="GorgonShaderBufferView.GetViewHandle()"/> method on the view. Then, in the shader, the resource can be indexed easily by the <c>ResourceDescriptorHeap[cbv_handle]</c> 
    /// intrinsic function (where <c>cbv_handle</c> is the name of the constant that was updated).
    /// </para>
    /// <para>
    /// Unlike other view types, only 1 constant buffer view can be used by a buffer. This is because the constant buffer covers the range of the entire buffer's size.
    /// </para>
    /// <para type="constant_alignment">
    /// <note type="important">
    /// <para>
    /// The buffer used for the constant buffer view <b>MUST</b> be aligned to <see cref="GorgonConstantBufferView.AlignmentRequirement"/> (256 bytes). Ensure the <see cref="GorgonGpuBufferInfo.Alignment"/> 
    /// property on the <see cref="GorgonGpuBufferInfo"/> passed to the buffer's constructor is set to match the <see cref="GorgonConstantBufferView.AlignmentRequirement"/> 
    /// upon buffer creation.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
    /// <seealso cref="GorgonShaderBufferView.GetViewHandle()"/>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    /// <seealso cref="GorgonConstantBufferView"/>
    internal GorgonConstantBufferView GetConstantBufferView(bool owned)
    {
        using (_viewLock.EnterScope())
        {
            ulong alignedSize = _bufferAllocation.SizeInBytes.AlignDown((ulong)GorgonConstantBufferView.AlignmentRequirement);

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
    /// <param name="structSize">The size, in bytes, of a single element in the view. Must be a multiple of 16.</param>
    /// <param name="startIndex">[Optional] The index in the buffer to start the view at.</param>
    /// <param name="count">[Optional] The number of elements to view.</param>
    /// <param name="owned"><inheritdoc cref="GetConstantBufferView(bool)" path="/param[@name='owned']"/></param>
    /// <returns>A <see cref="GorgonStructuredBufferView"/> for the buffer.</returns>
    /// <inheritdoc cref="GorgonStructuredBufferView.ValidateStructuredView(string, int, long, ulong)" path="/exception"/>
    /// <remarks>
    /// <para>
    /// This allows shaders to access buffer data as a custom data structure.
    /// </para>
    /// <para>
    /// The <paramref name="structSize"/> must be a multiple of <see cref="GorgonStructuredBufferView.MinimumElementSize"/> (4 bytes). If it is not, then an exception is thrown.
    /// </para>
    /// <para>
    /// Buffers used as a structured data buffer must be at least the size of a single element, which is the size of a single structure in the buffer. If the buffer is too small, an exception is thrown.
    /// </para>
    /// <para>
    /// When the structured buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the <paramref name="structSize"/>. Otherwise, an exception will be thrown when creating 
    /// the structured view. This provides protection against data misalignment. 
    /// </para>
    /// <para>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </para>
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
            count = count.Value.Max(1).Min((int)maxElements);

            ViewKey key = new(BufferFormat.Unknown, startIndex, ((long)count.Value << 32) | (uint)structSize);

            if (_structs.TryGetValue(key, out GorgonStructuredBufferView? result))
            {
                return result;
            }

            return _structs[key] = new GorgonStructuredBufferView(Graphics, Name, this, startIndex.Min(maxElements - 1), structSize, count.Value, owned);
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
    /// <para>
    /// Buffers used as a raw data buffer must be at least the size of a <see cref="GorgonRawBufferView.MinimumElementSize"/> (4 bytes). If the buffer is too small, an exception is thrown.
    /// </para>
    /// <para>
    /// When the raw buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the <see cref="GorgonRawBufferView.AlignmentRequirement"/>. Otherwise, an exception will be 
    /// thrown when creating the raw view. This provides protection against data misalignment. 
    /// </para>
    /// <para>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </para>
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
            count = count.Value.Max(1).Min((int)maxElements);

            ViewKey key = new(BufferFormat.Unknown, startIndex, count.Value);

            if (_raws.TryGetValue(key, out GorgonRawBufferView? result))
            {
                return result;
            }

            return _raws[key] = new GorgonRawBufferView(Graphics, Name, this, startIndex, count.Value, owned);
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
    /// <para>
    /// Buffers used as a typed data buffer must be at least the size of a <see cref="BufferFormat"/>. If the buffer is too small, an exception is thrown. Applications can get the format size by using the 
    /// <see cref="GorgonFormatInfo"/> object and reading the <see cref="GorgonFormatInfo.SizeInBytes"/> property.
    /// </para>
    /// <para>
    /// When the typed buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the size of a <see cref="BufferFormat"/>. Otherwise, an exception will be thrown when 
    /// creating the typed view. This provides protection against data misalignment. 
    /// </para>
    /// <para>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </para>
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
            count = count.Value.Max(1).Min((int)maxElements);

            ViewKey key = new(format, startIndex, count.Value);

            if (_typeds.TryGetValue(key, out GorgonTypedBufferView? result))
            {
                return result;
            }

            return _typeds[key] = new GorgonTypedBufferView(Graphics, Name, this, formatInfo, startIndex, count.Value, owned);
        }
    }

    /// <summary>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/summary"/>
    /// </summary>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/returns"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/exception"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks"/>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonConstantBufferView GetConstantBufferView() => GetConstantBufferView(false);

    /// <summary>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/summary"/>
    /// </summary>
    /// <param name="structSize"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='structSize']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/returns"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/exception"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks"/>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonStructuredBufferView GetStructuredBufferView(int structSize, long startIndex = 0, int? count = null) => GetStructuredBufferView(structSize, startIndex, count, false);

    /// <summary>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)"/>
    /// </summary>
    /// <typeparam name="T">The type of data to view the buffer as. Must be an unmanaged type.</typeparam>
    /// <param name="startIndex"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/returns"/>
    /// <remarks>
    /// <para>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/remarks/para[1]"/>
    /// </para>
    /// <para>
    /// Buffers used as a structured data buffer must be at least the size of a single element, which is the size of the type <typeparamref name="T"/>, in bytes. If the buffer is too small, an exception is 
    /// thrown.
    /// </para>
    /// <para>
    /// When the structured buffer is created, it must use an <see cref="GorgonGpuBufferInfo.Alignment"/> that matches the size of the type <typeparamref name="T"/>. Otherwise, an exception will be thrown 
    /// when creating the structured view. This provides protection against data misalignment. 
    /// </para>
    /// <para>
    /// <inheritdoc cref="GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="GetStructuredBufferView(int, long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonStructuredBufferView GetStructuredBufferView<T>(long startIndex = 0, int? count = null) where T : unmanaged
        => GetStructuredBufferView(sizeof(T), startIndex, count, false);

    /// <summary>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)"/>
    /// </summary>
    /// <param name="startIndex"><inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/exception"/>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/remarks"/>
    /// <inheritdoc cref="GetRawBufferView(long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonRawBufferView GetRawBufferView(long startIndex = 0, int? count = null) => GetRawBufferView(startIndex, count, false);

    /// <summary>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat, long, int?, bool)"/>
    /// </summary>
    /// <param name="format"><inheritdoc cref="GetTypedBufferView(BufferFormat,long, int?, bool)" path="/param[@name='format']"/></param>
    /// <param name="startIndex"><inheritdoc cref="GetTypedBufferView(BufferFormat,long, int?, bool)" path="/param[@name='startIndex']"/></param>
    /// <param name="count"><inheritdoc cref="GetTypedBufferView(BufferFormat,long, int?, bool)" path="/param[@name='count']"/></param>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat,long, int?, bool)" path="/exception"/>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat,long, int?, bool)" path="/remarks"/>
    /// <inheritdoc cref="GetTypedBufferView(BufferFormat,long, int?, bool)" path="/seealso"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GorgonTypedBufferView GetTypedBufferView(BufferFormat format, long startIndex = 0, int? count = null) => GetTypedBufferView(format, startIndex, count, false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGpuBuffer"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <exception cref="GorgonException"><para>Thrown if the buffer cannot be created because the size is less than 1 byte.</para>
    /// <para>Thrown if the <see cref="GorgonGpuBufferInfo.Alignment"/> value is negative.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This creates a generic buffer that can hold data on the GPU, or upload to the GPU. The buffer data has no structure. That is defined by views that can be created from the buffer. The types of views 
    /// available are defined in the <see cref="GorgonGpuBufferInfo"/> flags.
    /// </para>
    /// <para>
    /// Most buffers will require a minimum <see cref="GorgonCommonBufferInfo.SizeInBytes"/> of 1 byte. However, some views will require the buffer have a specific minimum size (e.g. constant buffers must be at 
    /// least 256 bytes, etc...).  
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBufferInfo"/>
    public GorgonGpuBuffer(GorgonGraphics graphics, string name, GorgonGpuBufferInfo info)
        : base(graphics, name, info)
    {
        _info = info;

        ValidateInfo();

        Graphics.Log.Print($"Creating Gorgon buffer '{Name}'.", LoggingLevel.Simple);

        CreateNative();        
    }
}
