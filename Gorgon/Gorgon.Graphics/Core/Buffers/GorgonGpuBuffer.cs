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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
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
/// then use shaders to read from, or using Unordered Access Views, write to the buffer. The type of data in the buffer can be anything a user needs.
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
/// <para type="BufferUsage">
/// <para>
/// The buffer can be used in two modes: <see cref="BufferUsage.Default"/> and <see cref="BufferUsage.DynamicPerFrame"/>. These modes indicate where the memory is stored for the buffer.
/// </para>
/// <para>
/// <h3><see cref="BufferUsage.Default"/></h3>
/// </para>
/// <para>
/// When in default mode, the buffer's memory is stored on the GPU and is inaccessible by the CPU. That is, the developer cannot write the memory directly. This has the effect of having extremely fast read 
/// performance, but poor update performance and should be used in write once, read many scenarios.
/// </para>
/// <para>
/// <h3><see cref="BufferUsage.DynamicPerFrame"/></h3>
/// </para>
/// <para>
/// With this mode, the buffer can be updated from the CPU and copied through the PCIE bus and into GPU memory*. This allows decent performance when updating the buffer multiple times per frame. This mode is 
/// similar to the old Direct3D 11 style "Dynamic Buffer" type. 
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// <see cref="BufferUsage.DynamicPerFrame"/> is as the name suggests. Data uploaded to the buffer is only valid for the current frame. Use a <see cref="BufferUsage.Default"/> if you need data to persist for 
/// more than a single frame.
/// </para>
/// </note>
/// </para>
/// <para>
/// <i>
/// * - If the GPU supports GPU Uploads and ReBar is enabled, then the copy is done directly into video memory. If Gorgon detects that the video adapter is GPU upload capable, it will use that mode as it 
/// will provide better performance. To determine if your GPU has GPU upload capability, check the <see cref="GorgonVideoAdapterInfo.HasGpuUploadSupport"/> flag on the <see cref="GorgonVideoAdapterInfo"/> 
/// type.
/// </i>
/// </para>
/// </para>
/// </remarks>
/// <seealso cref="GorgonVideoAdapterInfo"/>
/// <seealso cref="GorgonGpuBufferInfo"/>
/// <seealso cref="GorgonIndexBuffer"/>
/// <seealso cref="GorgonConstantBufferView"/>
/// <seealso cref="GorgonStructuredBufferView"/>
/// <seealso cref="GorgonCommandList"/>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="BufferUsage"/>
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
    private readonly Dictionary<ViewKey, GorgonConstantBufferView> _cbvs = [];
    private readonly Dictionary<ViewKey, GorgonStructuredBufferView> _structs = [];
    private readonly Dictionary<ViewKey, GorgonShaderBufferView> _srvs = [];
    private readonly Dictionary<ViewKey, GorgonResourceView> _uavs = [];    
    private GpuBufferAllocation _bufferAllocation = GpuBufferAllocation.Null;
    private CpuBufferAllocation _uploadAllocation = CpuBufferAllocation.Null;    

    /// <summary>
    /// Property to return the offset, in bytes, of a suballocated buffer within a larger buffer.
    /// </summary>
    internal override ulong ResourceOffset => _bufferAllocation.Offset;

    /// <inheritdoc/>
    public int Alignment => _info.Alignment;


    /// <inheritdoc/>
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
    private protected sealed override void Dispose(bool disposing)
    {        
        if (disposing)
        {           
            // Remove all the child views.
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

    /// <inheritdoc/>
    internal override ref readonly CpuBufferAllocation GetTransientBufferData()
    {
        if (!_uploadAllocation.IsAvailable)
        {
            Graphics.UploadHeaps.Allocate((ulong)SizeInBytes, 0, out _uploadAllocation);
        }

        Debug.Assert(_uploadAllocation.IsAvailable, $"Upload heap for dynamic buffer '{Name}' is no longer valid.");

        return ref _uploadAllocation;
    }

    /// <summary>
    /// Function to create a new constant buffer view for this buffer.
    /// </summary>
    /// <param name="offset">[Optional] The offset, in bytes, within the buffer to start viewing at.</param>
    /// <param name="size">[Optional[ The size, in bytes, of the buffer to view.</param>
    /// <returns>A new <see cref="GorgonConstantBufferView"/> used to send constant data to the GPU.</returns>
    /// <exception cref="GorgonException">Thrown if the view could not be created because the buffer is less than 256 bytes in size.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="offset"/> is less than 0, or the <paramref name="size"/> is less than 256 bytes.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="offset"/> plus the <paramref name="size"/>, aligned to 256 bytes, is larger than <see cref="GorgonGpuBufferCommon.SizeInBytes"/>.</exception>
    /// <remarks>
    /// <para>
    /// To access constant data in the shaders, a constant buffer view must be passed to the shader. This view will indicate that the entire buffer, or a portion of it can be used to represent shader 
    /// constants. 
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// The constant buffer view <b>MUST</b> be aligned to 256 bytes. This method will adjust the <paramref name="size"/> and the <paramref name="offset"/> values internally, however the aligned 
    /// <paramref name="size"/> and <paramref name="offset"/> will be used in determining if the view fits within the <see cref="GorgonGpuBufferCommon.SizeInBytes"/> of the buffer. Therefore, the error message will reflect this 
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
            size ??= SizeInBytes;
            long alignedSize = size.Value.AlignUp(D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);
            long alignedOffset = offset.AlignUp(D3D12.D3D12_CONSTANT_BUFFER_DATA_PLACEMENT_ALIGNMENT);

            GorgonConstantBufferView.ValidateCbv(Name, Alignment, SizeInBytes, alignedOffset, alignedSize);

            ViewKey key = new(BufferFormat.Unknown, alignedOffset, alignedSize);

            if ((_cbvs.TryGetValue(key, out GorgonConstantBufferView? result)) && (result.D3DCpuHandle != D3D12_CPU_DESCRIPTOR_HANDLE.DEFAULT))
            {
                return result;
            }

            return _cbvs[key] = new GorgonConstantBufferView(Graphics, Name, this, alignedOffset, (int)alignedSize, false);
        }
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
