#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 9, 2016 3:54:15 PM
// 
#endregion

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The types of elements that the view will interpret the raw data as.
/// </summary>
public enum RawBufferElementType
{
    /// <summary>
    /// Each element is a 32 bit signed integer.
    /// </summary>
    Int32 = 0,
    /// <summary>
    /// Each element is a 32 bit unsigned integer.
    /// </summary>
    UInt32 = 1,
    /// <summary>
    /// Each element is a single precision floating point value.
    /// </summary>
    Single = 2
}

/// <summary>
/// A base class that provides functionality that is common across any buffer type.
/// </summary>
public abstract class GorgonBufferCommon
    : GorgonGraphicsResource
{
    #region Variables.
    // A cache of shader views for the buffer.
    private Dictionary<BufferShaderViewKey, GorgonShaderResourceView> _shaderViews = [];
    // A cache of unordered access views for the buffer.
    private Dictionary<BufferShaderViewKey, GorgonReadWriteView> _uavs = [];
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the log used to log debug messages.
    /// </summary>
    protected IGorgonLog Log
    {
        get;
    }

    /// <summary>
    /// Property to set or return the D3D 11 buffer.
    /// </summary>
    internal D3D11.Buffer Native
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the type of data in the resource.
    /// </summary>
    public override GraphicsResourceType ResourceType => GraphicsResourceType.Buffer;

    /// <summary>
    /// Property to return whether or not the buffer is directly readable by the CPU via one of the <see cref="GetData{T}(int, int?)"/> methods.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Buffers must meet the following criteria in order to qualify for direct CPU read:
    /// <list type="bullet">
    ///     <item>Must have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/> (or <see cref="ResourceUsage.Staging"/>).</item>
    ///     <item>Must be bindable to a shader resource view (<see cref="ResourceUsage.Default"/> only).</item>
    /// </list>
    /// </para>
    /// <para>
    /// If this value is <b>false</b>, then the buffer can still be read, but it will take a slower path by copying to a staging buffer.
    /// </para>
    /// <para>
    /// <note type="information">
    /// Any buffer created with a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/> will always be directly readable by the CPU. Therefore, this value will always
    /// return <b>true</b> in that case.
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GetData{T}(Span{T}, int, int?)"/>
    /// <seealso cref="GetData{T}(out T, int)"/>
    /// <seealso cref="GetData{T}(int, int?)"/>
    public abstract bool IsCpuReadable
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to validate the bindings for a given buffer.
    /// </summary>
    /// <param name="usage">The usage flags for the buffer.</param>
    /// <param name="binding">The bindings to apply to the buffer, pass <b>null</b> to skip usage and binding check.</param>
    /// <param name="structureSize">The size of a structure within the buffer, in bytes.</param>
    protected static void ValidateBufferBindings(ResourceUsage usage, BufferBinding binding, int structureSize)
    {
        if (structureSize > 2048)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_STRUCTURE_SIZE_INVALID, structureSize));
        }

        if ((usage != ResourceUsage.Staging) && (binding == BufferBinding.None))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_NON_STAGING_NEEDS_BINDING, usage));
        }

        switch (usage)
        {
            case ResourceUsage.Dynamic:
            case ResourceUsage.Immutable:
                if ((binding & BufferBinding.StreamOut) == BufferBinding.StreamOut)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_NO_SO, usage));
                }

                if ((binding & BufferBinding.ReadWrite) == BufferBinding.ReadWrite)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_NO_UAV, usage));
                }
                break;
            case ResourceUsage.Staging:
                if (binding != BufferBinding.None)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_STAGING_CANNOT_BE_BOUND_TO_GPU, binding));
                }
                break;
        }
    }

    /// <summary>
    /// Function to retrieve the total number of elements that can be placed in the buffer.
    /// </summary>
    /// <param name="info">Information about the element format.</param>
    /// <returns>The number of elements in the buffer.</returns>
    protected int GetTotalElementCount(GorgonFormatInfo info) => info.IsTypeless ? 0 : SizeInBytes / info.SizeInBytes;

    /// <summary>
    /// Function to retrieve the appropriate CPU flags for the buffer.
    /// </summary>
    /// <param name="requestDefaultUsageReadAccess"><b>true</b> if the buffer needs direct read access with a usage of default, <b>false</b> if not.</param>
    /// <param name="binding">The binding flags being used by the buffer.</param>
    /// <returns>The D3D11 CPU access flags.</returns>
    internal D3D11.CpuAccessFlags GetCpuFlags(bool requestDefaultUsageReadAccess, D3D11.BindFlags binding)
    {
        D3D11.CpuAccessFlags result = D3D11.CpuAccessFlags.None;

        switch (Usage)
        {
            case ResourceUsage.Dynamic:
                result = D3D11.CpuAccessFlags.Write;
                break;
            case ResourceUsage.Staging:
                // Staging resources are implicitly readable.
                result = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
                break;
            case ResourceUsage.Default:
                if (binding is not D3D11.BindFlags.ShaderResource and not D3D11.BindFlags.UnorderedAccess and not (D3D11.BindFlags.ShaderResource | D3D11.BindFlags.UnorderedAccess))
                {
                    break;
                }

                result = requestDefaultUsageReadAccess ? D3D11.CpuAccessFlags.Read : D3D11.CpuAccessFlags.None;
                break;
        }

        return result;
    }

    /// <summary>
    /// Function to return a cached shader resource view.
    /// </summary>
    /// <param name="key">The key associated with the view.</param>
    /// <returns>The shader resource view for the buffer, or <b>null</b> if no resource view is registered.</returns>
    internal GorgonShaderResourceView GetView(BufferShaderViewKey key)
    {
        if ((_shaderViews.TryGetValue(key, out GorgonShaderResourceView view))
            && (view.Native is not null))
        {
            return view;
        }

        if (view is not null)
        {
            _shaderViews.Remove(key);
        }

        return null;
    }

    /// <summary>
    /// Function to register the shader resource view in the cache.
    /// </summary>
    /// <param name="key">The unique key for the shader view.</param>
    /// <param name="view">The view to register.</param>
    internal void RegisterView(BufferShaderViewKey key, GorgonShaderResourceView view) => _shaderViews[key] = view;

    /// <summary>
    /// Function to return a cached shader resource view.
    /// </summary>
    /// <param name="key">The key associated with the view.</param>
    /// <returns>The shader resource view for the buffer, or <b>null</b> if no resource view is registered.</returns>
    internal T GetReadWriteView<T>(BufferShaderViewKey key)
        where T : GorgonReadWriteView
    {
        if ((_uavs.TryGetValue(key, out GorgonReadWriteView view))
            && (view.Native is not null))
        {
            return view as T;
        }

        if (view is not null)
        {
            _uavs.Remove(key);
        }

        return null;
    }

    /// <summary>
    /// Function to register an unordered access view in the cache.
    /// </summary>
    /// <param name="key">The unique key for the shader view.</param>
    /// <param name="view">The view to register.</param>
    internal void RegisterReadWriteView(BufferShaderViewKey key, GorgonReadWriteView view) => _uavs[key] = view;

    /// <summary>
    /// Function to validate the parameters for the Get/SetData methods.
    /// </summary>
    /// <param name="sourceOffset">The source offset, in bytes..</param>
    /// <param name="destOffset">The destination offset, in bytes.</param>
    /// <param name="copySize">The total amount of data to copy, in bytes.</param>
    /// <param name="sourceSize">The total size of the source data, in bytes.</param>
    /// <param name="destSize">The total size of the destination, in bytes.</param>        
    private void ValidateGetSetData(int sourceOffset, int destOffset, int copySize, int sourceSize, int destSize)
    {
        if (Usage == ResourceUsage.Immutable)
        {
            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_ERR_BUFFER_CANT_UPDATE_IMMUTABLE);
        }

        if (sourceOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceOffset), string.Format(Resources.GORGFX_ERR_VALUE_OUT_OF_RANGE, sourceSize));
        }

        if (copySize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(copySize), Resources.GORGFX_ERR_COUNT_OUT_OF_RANGE);
        }

        if (sourceOffset + copySize > sourceSize)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_SOURCE_INDEX_AND_COUNT_TOO_LARGE, (sourceOffset + copySize), sourceSize));
        }

        if (destOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(destOffset), string.Format(Resources.GORGFX_ERR_VALUE_OUT_OF_RANGE, destSize));
        }

        if ((destOffset + copySize) > destSize)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_DEST_INDEX_AND_COUNT_TOO_LARGE, (destOffset + copySize), destSize));
        }
    }


    /// <summary>
    /// Function to read the contents of this buffer into memory pointed at by a pointer.
    /// </summary>
    /// <typeparam name="T">The type of data to copy. Must be an unmanaged value type.</typeparam>
    /// <param name="destPtr">The pointer pointing at the memory that will receive the data.</param>
    /// <param name="srcOffset">The offset within this buffer, in bytes, to start reading from.</param>
    /// <param name="size">The number of bytes to read/write.</param>
    private unsafe void GetDataPtr<T>(T* destPtr, int srcOffset, int size)
        where T : unmanaged
    {
        if (size == 0)
        {
            return;
        }

        GorgonBufferCommon stage = this;

        // If we cannot read default usage buffers directly, then we need to make a copy.
        if ((!IsCpuReadable) || ((Usage != ResourceUsage.Default) && (Usage != ResourceUsage.Staging)))
        {
            stage = GetStagingInternal();
        }

        DX.DataBox dataBox = Graphics.D3DDeviceContext.MapSubresource(stage.Native, 0, D3D11.MapMode.Read, D3D11.MapFlags.None);

        try
        {
            byte* srcPtr = (byte*)dataBox.DataPointer + srcOffset;

            Unsafe.CopyBlock(destPtr, srcPtr, (uint)size);
        }
        finally
        {
            Graphics.D3DDeviceContext.UnmapSubresource(stage.Native, 0);

            if (stage != this)
            {
                stage.Dispose();
            }
        }
    }

    /// <summary>
    /// Function to upload data into the buffer.
    /// </summary>
    /// <typeparam name="T">The type of data to copy. Must be an unmanaged value type.</typeparam>
    /// <param name="srcPtr">The pointer to the data to upload.</param>
    /// <param name="typeSize">The size of an element in the buffer.</param>
    /// <param name="srcSize">The total element size of the upload data.</param>
    /// <param name="destOffset">The destination offset, in bytes, in the destination buffer to write into.</param>
    /// <param name="count">The number of elements to write.</param>
    /// <param name="map"><b>true</b> to use mapping, or <b>false</b> to use a direct upload.</param>
    /// <param name="copyMode">The mode used to determine how to copy the data.</param>
    private unsafe void SetDataPtr<T>(T* srcPtr, int typeSize, int srcSize, int destOffset, int count, bool map, CopyMode copyMode)
        where T : unmanaged
    {
        if ((count == 0)
            || (typeSize == 0)
            || (srcSize == 0))
        {
            return;
        }

        count *= typeSize;

        if (!map)
        {
            D3D11.ResourceRegion? region = null;
            if (copyMode != CopyMode.None)
            {
                region = new D3D11.ResourceRegion
                {
                    Left = destOffset,
                    Right = destOffset + count,
                    Back = 1,
                    Bottom = 1
                };
            }

            Graphics.D3DDeviceContext.UpdateSubresource1(Native,
                                                         0,
                                                         region,
                                                         (nint)srcPtr, count, count, (int)copyMode);
            return;
        }

        D3D11.MapMode mapMode = D3D11.MapMode.Write;

        if (Usage != ResourceUsage.Staging)
        {
            mapMode = copyMode switch
            {
                CopyMode.NoOverwrite => D3D11.MapMode.WriteNoOverwrite,
                _ => D3D11.MapMode.WriteDiscard,
            };
        }

        DX.DataBox mapData = Graphics.D3DDeviceContext.MapSubresource(Native, 0, mapMode, D3D11.MapFlags.None);

        byte* destPtr = (byte*)mapData.DataPointer + destOffset;
        Unsafe.CopyBlock(destPtr, srcPtr, (uint)count);

        Graphics.D3DDeviceContext.UnmapSubresource(Native, 0);
    }

    /// <summary>
    /// Function to write data into the buffer from a read only span.
    /// </summary>
    /// <typeparam name="T">The type of element in the source span. Must be an unmanaged value type.</typeparam>
    /// <param name="data">The read only span containing the data to upload into the buffer.</param>
    /// <param name="destOffset">[Optional] The offset, in bytes, within this buffer to start copying into.</param>
    /// <param name="copyMode">[Optional] Flags to indicate how to copy the data.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destOffset"/> parameter is less than zero.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="destOffset"/> + span length exceeds the size of the destination buffer.</exception>
    /// <remarks>
    /// <para>
    /// This will upload data from an read only span to this buffer. The method will determine how to best upload the data depending on the <see cref="GorgonGraphicsResource.Usage"/> of the buffer. For example,
    /// if the buffer has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/>, then internally, this method will update to the GPU directly. Otherwise, if it is
    /// <see cref="ResourceUsage.Dynamic"/>, or <see cref="ResourceUsage.Staging"/>, it will use a locking pattern which uses the CPU to write data to the buffer. The latter pattern is good if the
    /// buffer has to change one or more times per frame, otherwise, the former is better where the buffer is updated less than once per frame (i.e. Dynamic is good for multiple times per frame, Default
    /// is good for once per frame or less).
    /// </para>
    /// <para>
    /// If the user supplies a <paramref name="destOffset"/>, then a portion of the data will be copied to the buffer at the offset provided by <paramref name="destOffset"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="copyMode"/> parameter defines how the copy will be performed. If the buffer has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Dynamic"/> or
    /// <see cref="ResourceUsage.Default"/> and the <paramref name="copyMode"/> is set to <see cref="CopyMode.Discard"/> then the contents of the buffer are discarded before updating, if it is set to
    /// <see cref="CopyMode.NoOverwrite"/>, then the data will be copied to the destination if we know the GPU is not using the portion being updated. If the <paramref name="copyMode"/> is set to
    /// <see cref="CopyMode.None"/>, then <see cref="CopyMode.Discard"/> is used. For buffers created with a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>, the
    /// <see cref="CopyMode"/> will be ignored and act as though <see cref="CopyMode.None"/> were passed. If the mode is set to <see cref="CopyMode.None"/>, then the <paramref name="destOffset"/> 
    /// parameter is ignored.
    /// </para>
    /// <note type="caution">
    /// <para>
    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </remarks>
    /// <example>
    /// The following is an example showing how to upload vertices into a vertex buffer using different techniques:
    /// <code language="csharp">
    /// <![CDATA[
    /// // Our vertex, with a position and color component.
    /// [StructLayout(LayoutKind = LayoutKind.Sequential)] 
    /// struct MyVertex
    /// {
    ///		public Vector4 Position;
    ///		public Vector4 Color;
    /// }
    /// 
    /// GorgonGraphics graphics;
    /// MyVertex[] _vertices = new MyVertex[100];
    /// GorgonVertexBuffer _vertexBuffer;
    /// 
    /// void InitializeVertexBuffer()
    /// {
    ///		_vertices = ... // Fill your vertex array here.
    /// 
    ///		// Create the vertex buffer large enough so that it'll hold 100 vertices.
    ///		_vertexBuffer = new GorgonVertexBuffer(graphics, GorgonVertexBufferInfo.CreateFromType<MyVertex>(_vertices.Length, Usage.Default));
    /// 
    ///		// Copy our data to the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan());
    /// 
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, and 25 vertices.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25));
    ///
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, 25 vertices, and storing at index 2 in the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///     // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///     _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    /// 
    ///     // Copy our data from a GorgonNativeBuffer.
    ///     using (GorgonNativeBuffer<MyVertex> vertices = new GorgonNativeBuffer<MyVertex>(100))
    ///     {
    ///        // Copy vertices into the native buffer here....
    ///
    ///        // Copy everything.
    ///        _vertexBuffer.SetData(vertices);
    /// 
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer.
    ///        _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///        _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    ///        
    ///        // Get the data back out from the buffer, using index 5 and up to 10 vertices, storing at index 2 of the native buffer.
    ///        _vertexBuffer.GetData<MyVertex>(vertices.ToSpan(2), 5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>()); 
    ///     }
    ///
    ///     // Get the data back out from the buffer.
    ///     MyVertex[] readBack = _vertexBuffer.GetData<MyVertex>();
    ///
    ///     // Get the data back out from the buffer, starting at index 5 and a count of 10 vertices.
    ///     readBack = _vertexBuffer.GetData<MyVertex>(5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>());
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public void SetData<T>(ReadOnlySpan<T> data, int destOffset = 0, CopyMode copyMode = CopyMode.None)
        where T : unmanaged
    {
        if (data.Length == 0)
        {
            return;
        }

        unsafe
        {
            int typeSize = sizeof(T);

#if DEBUG            
            ValidateGetSetData(0, copyMode == CopyMode.None ? 0 : destOffset, data.Length * typeSize, data.Length * typeSize, SizeInBytes);
#endif

            fixed (T* srcPtr = &data[0])
            {
                SetDataPtr(srcPtr, typeSize, data.Length, destOffset, data.Length, Usage != ResourceUsage.Default, copyMode);
            }
        }
    }

    /// <summary>
    /// Function to write a single value into the buffer.
    /// </summary>
    /// <typeparam name="T">The type of value, must be an unmanaged value type.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <param name="destOffset">[Optional] The offset, in bytes, from the beginning in the buffer to start writing into.</param>
    /// <param name="copyMode">[Optional] Flags to indicate how to copy the data.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="destOffset"/> parameter is less than zero.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="destOffset"/> + size of <typeparamref name="T"/> (in bytes) exceeds the size of the destination buffer.</exception>
    /// <remarks>
    /// <para>
    /// This will upload the specified <paramref name="value"/> to this buffer. The method will determine how to best upload the data depending on the <see cref="GorgonGraphicsResource.Usage"/> of the
    /// buffer. For example, if the buffer has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/>, then internally, this method will update to the GPU directly.
    /// Otherwise, if it is <see cref="ResourceUsage.Dynamic"/>, or <see cref="ResourceUsage.Staging"/>, it will use a locking pattern which uses the CPU to write data to the buffer. The latter pattern 
    /// is good if the buffer has to change one or more times per frame, otherwise, the former is better where the buffer is updated less than once per frame (i.e. Dynamic is good for multiple times 
    /// per frame, Default is good for once per frame or less).
    /// </para>
    /// <para>
    /// If the user supplies a <paramref name="destOffset"/> the data will be copied to the buffer at the offset provided by <paramref name="destOffset"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="copyMode"/> parameter defines how the copy will be performed. If the buffer has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Dynamic"/> or
    /// <see cref="ResourceUsage.Default"/> and the <paramref name="copyMode"/> is set to <see cref="CopyMode.Discard"/> then the contents of the buffer are discarded before updating, if it is set to
    /// <see cref="CopyMode.NoOverwrite"/>, then the data will be copied to the destination if we know the GPU is not using the portion being updated. If the <paramref name="copyMode"/> is set to
    /// <see cref="CopyMode.None"/>, then <see cref="CopyMode.Discard"/> is used. For buffers created with a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>, the
    /// <see cref="CopyMode"/> will be ignored and act as though <see cref="CopyMode.None"/> were passed. If the mode is set to <see cref="CopyMode.None"/>, then the <paramref name="destOffset"/> 
    /// parameter is ignored.
    /// </para>
    /// <note type="caution">
    /// <para>
    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </remarks>
    /// <example>
    /// The following is an example showing how to upload data into a constant buffer using different techniques:
    /// <code language="csharp">
    /// <![CDATA[
    /// // Our constant buffer data.  A matrix and a 4 component vector.
    /// [StructLayout(LayoutKind = LayoutKind.Sequential)] 
    /// struct MyCBData
    /// {
    ///		public Matrix ConstantValue1;
    ///		public Vector4 ConstantValue2;
    /// }
    /// 
    /// GorgonGraphics graphics;
    /// MyCBData _cbData;
    /// GorgonConstantBuffer _constantBuffer;
    /// 
    /// void InitializeConstantBuffer()
    /// {
    ///		_cbData = new MyCBData
    ///     {
    ///         ConstantValue1 = WorldMatrix,
    ///         ConstantValue2 = AVector4
    ///     };
    ///
    ///     // Create the constant buffer.
    ///		_constantBuffer = new GorgonConstantBuffer(graphics, GorgonConstantBufferInfo.CreateFromType<MyCBData>(count: 4));
    /// 
    ///		// Copy our data to the constant buffer.
    ///     _constantBuffer.SetData<MyCBData>(in _cbData);
    ///
    ///     _cbData = new MyCBData
    ///     {
    ///         ConstantValue1 = ProjectionMatrix,
    ///         ConstantValue2 = Vector4.One
    ///     };
    ///
    ///     // Write these constants to the 2nd index.  
    ///     _constantBuffer.SetData<MyCBData>(in _cbData, 2 * Unsafe.SizeOf<MyCBData>());
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public unsafe void SetData<T>(in T value, int destOffset = 0, CopyMode copyMode = CopyMode.None)
        where T : unmanaged
    {
        // Actual byte offset in the buffer.
        int typeSize = sizeof(T);

#if DEBUG            
        ValidateGetSetData(0, copyMode == CopyMode.None ? 0 : destOffset, typeSize, typeSize, SizeInBytes);
#endif

        fixed (T* valuePtr = &value)
        {
            SetDataPtr(valuePtr, typeSize, 1, destOffset, 1, Usage != ResourceUsage.Default, copyMode);
        }
    }

    /// <summary>
    /// Function to return the contents of this buffer into an array.
    /// </summary>
    /// <typeparam name="T">The type of data in the array, must be an unmanaged value type.</typeparam>
    /// <param name="sourceOffset">The offset, in bytes, within this buffer to start reading from.</param>
    /// <param name="size">The number of bytes to read from this buffer.</param>
    /// <returns>An array with a copy of the data in this buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/>, or the <paramref name="size"/> parameter is less than zero.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="sourceOffset"/> + <paramref name="size"/> exceeds the size of this buffer.</exception>
    /// <remarks>
    /// <para>
    /// This will retrieve the data from this buffer and copy it into an array which is returned back to the caller as the type specified by <typeparamref name="T"/>. If the <see cref="IsCpuReadable"/>
    /// flag is set to <b>true</b>, then the data in the buffer can be read directly from the CPU. If not, then the data will be copied to a staging buffer and then transferred (which, obviously, is
    /// not as performant).
    /// </para>
    /// <para>
    /// <i>Some</i> buffers will allow the user to directly set the <see cref="IsCpuReadable"/> flag via their corresponding info object upon creation. The purpose of this is to tell the GPU where to
    /// best locate the memory for the buffer. If the user decides to not allow direct CPU read, then the buffer will be located in memory that is much faster to access for the GPU, while allowing
    /// direct CPU read will put the buffer into an area where the CPU and GPU can access the data, which is in typically less performant memory ranges. Buffers must meet the following criteria in
    /// order to qualify for direct CPU read:
    /// <list type="bullet">
    ///     <item>Must have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/> (or <see cref="ResourceUsage.Staging"/>).</item>
    ///     <item>Must be bindable to a shader resource view (<see cref="ResourceUsage.Default"/> only).</item>
    /// </list>
    /// </para>
    /// <para>
    /// If the user supplies a one of the <paramref name="sourceOffset"/>, or <paramref name="size"/> parameters, then a portion of the data will be copied from the buffer at the index provided by
    /// <paramref name="sourceOffset"/>. 
    /// </para>
    /// <note type="caution">
    /// <para>
    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </remarks>
    /// <seealso cref="IsCpuReadable"/>
    /// <example>
    /// The following is an example showing how to upload vertices into a vertex buffer using different techniques:
    /// <code language="csharp">
    /// <![CDATA[
    /// // Our vertex, with a position and color component.
    /// [StructLayout(LayoutKind = LayoutKind.Sequential)] 
    /// struct MyVertex
    /// {
    ///		public Vector4 Position;
    ///		public Vector4 Color;
    /// }
    /// 
    /// GorgonGraphics graphics;
    /// MyVertex[] _vertices = new MyVertex[100];
    /// GorgonVertexBuffer _vertexBuffer;
    /// 
    /// void InitializeVertexBuffer()
    /// {
    ///		_vertices = ... // Fill your vertex array here.
    /// 
    ///		// Create the vertex buffer large enough so that it'll hold 100 vertices.
    ///		_vertexBuffer = new GorgonVertexBuffer(graphics, GorgonVertexBufferInfo.CreateFromType<MyVertex>(_vertices.Length, Usage.Default));
    /// 
    ///		// Copy our data to the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan());
    /// 
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, and 25 vertices.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25));
    ///
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, 25 vertices, and storing at index 2 in the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///     // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///     _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    /// 
    ///     // Copy our data from a GorgonNativeBuffer.
    ///     using (GorgonNativeBuffer<MyVertex> vertices = new GorgonNativeBuffer<MyVertex>(100))
    ///     {
    ///        // Copy vertices into the native buffer here....
    ///
    ///        // Copy everything.
    ///        _vertexBuffer.SetData(vertices);
    /// 
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer.
    ///        _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///        _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    ///        
    ///        // Get the data back out from the buffer, using index 5 and up to 10 vertices, storing at index 2 of the native buffer.
    ///        _vertexBuffer.GetData<MyVertex>(vertices.ToSpan(2), 5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>()); 
    ///     }
    ///
    ///     // Get the data back out from the buffer.
    ///     MyVertex[] readBack = _vertexBuffer.GetData<MyVertex>();
    ///
    ///     // Get the data back out from the buffer, starting at index 5 and a count of 10 vertices.
    ///     readBack = _vertexBuffer.GetData<MyVertex>(5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>());
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public unsafe T[] GetData<T>(int sourceOffset = 0, int? size = null)
        where T : unmanaged
    {
        int typeSize = sizeof(T);

        size ??= SizeInBytes - sourceOffset;

        int arraySize = (int)(((float)size.Value) / typeSize).FastFloor();

#if DEBUG
        ValidateGetSetData(sourceOffset, 0, size.Value, SizeInBytes, size.Value);
#endif

        var result = new T[arraySize];

        fixed (T* resultPtr = &result[0])
        {
            GetDataPtr(resultPtr, sourceOffset, size.Value);
        }

        return result;
    }

    /// <summary>
    /// Function to return the contents of this buffer into the specified span.
    /// </summary>
    /// <typeparam name="T">The type of data in the span, must be an unmanaged value type.</typeparam>
    /// <param name="destination">The span that will receive the data.</param>
    /// <param name="sourceOffset">The offset, in bytes, within this buffer to start reading from.</param>
    /// <param name="size">The number of bytes to read from this buffer.</param>
    /// <returns>An span with a copy of the data in this buffer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/>, or the <paramref name="size"/> parameter is less than zero.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="sourceOffset"/> + <paramref name="size"/> exceeds the size of this buffer.</exception>
    /// <remarks>
    /// <para>
    /// This will retrieve the data from this buffer and copy it into the <paramref name="destination"/> span. If the <see cref="IsCpuReadable"/> flag is set to <b>true</b>, then the data in the
    /// buffer can be read directly from the CPU. If not, then the data will be copied to a staging buffer and then transferred (which, obviously, is not as performant).
    /// </para>
    /// <para>
    /// <i>Some</i> buffers will allow the user to directly set the <see cref="IsCpuReadable"/> flag via their corresponding info object upon creation. The purpose of this is to tell the GPU where to
    /// best locate the memory for the buffer. If the user decides to not allow direct CPU read, then the buffer will be located in memory that is much faster to access for the GPU, while allowing
    /// direct CPU read will put the buffer into an area where the CPU and GPU can access the data, which is in typically less performant memory ranges. Buffers must meet the following criteria in
    /// order to qualify for direct CPU read:
    /// <list type="bullet">
    ///     <item>Must have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/> (or <see cref="ResourceUsage.Staging"/>).</item>
    ///     <item>Must be bindable to a shader resource view (<see cref="ResourceUsage.Default"/> only).</item>
    /// </list>
    /// </para>
    /// <para>
    /// If the user supplies a one of the <paramref name="sourceOffset"/>, or <paramref name="size"/> parameters, then a portion of the data will be copied from the buffer at the index provided by 
    /// <paramref name="sourceOffset"/>, and written into the beginning of the span. 
    /// </para>
    /// <note type="caution">
    /// <para>
    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </remarks>
    /// <seealso cref="IsCpuReadable"/>
    /// <example>
    /// The following is an example showing how to upload vertices into a vertex buffer using different techniques:
    /// <code language="csharp">
    /// <![CDATA[
    /// // Our vertex, with a position and color component.
    /// [StructLayout(LayoutKind = LayoutKind.Sequential)] 
    /// struct MyVertex
    /// {
    ///		public Vector4 Position;
    ///		public Vector4 Color;
    /// }
    /// 
    /// GorgonGraphics graphics;
    /// MyVertex[] _vertices = new MyVertex[100];
    /// GorgonVertexBuffer _vertexBuffer;
    /// 
    /// void InitializeVertexBuffer()
    /// {
    ///		_vertices = ... // Fill your vertex array here.
    /// 
    ///		// Create the vertex buffer large enough so that it'll hold 100 vertices.
    ///		_vertexBuffer = new GorgonVertexBuffer(graphics, GorgonVertexBufferInfo.CreateFromType<MyVertex>(_vertices.Length, Usage.Default));
    /// 
    ///		// Copy our data to the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan());
    /// 
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, and 25 vertices.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25));
    ///
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, 25 vertices, and storing at index 2 in the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///     // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///     _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    /// 
    ///     // Copy our data from a GorgonNativeBuffer.
    ///     using (GorgonNativeBuffer<MyVertex> vertices = new GorgonNativeBuffer<MyVertex>(100))
    ///     {
    ///        // Copy vertices into the native buffer here....
    ///
    ///        // Copy everything.
    ///        _vertexBuffer.SetData(vertices);
    /// 
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer.
    ///        _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///        _vertexBuffer.SetData(vertices.ToReadOnlySpan(), vertices.ToReadOnlySpan(5, 25), 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    ///        
    ///        // Get the data back out from the buffer, using index 5 and up to 10 vertices, storing at index 2 of the native buffer.
    ///        _vertexBuffer.GetData<MyVertex>(vertices.ToSpan(2), 5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>()); 
    ///     }
    ///
    ///     // Get the data back out from the buffer.
    ///     MyVertex[] readBack = _vertexBuffer.GetData<MyVertex>();
    ///
    ///     // Get the data back out from the buffer, starting at index 5 and a count of 10 vertices.
    ///     readBack = _vertexBuffer.GetData<MyVertex>(5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>());
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public unsafe void GetData<T>(Span<T> destination, int sourceOffset = 0, int? size = null)
        where T : unmanaged
    {
        int typeSize = sizeof(T);

        size ??= SizeInBytes - sourceOffset;

#if DEBUG
        ValidateGetSetData(sourceOffset, 0, size.Value, SizeInBytes, destination.Length * typeSize);
#endif

        unsafe
        {
            fixed (T* destPtr = &destination[0])
            {
                GetDataPtr(destPtr, sourceOffset, size.Value);
            }
        }
    }

    /// <summary>
    /// Function to read a single value from the buffer.
    /// </summary>
    /// <typeparam name="T">The type of value, must be an unmanaged value type.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <param name="sourceOffset">[Optional] The offset, in bytes, from the beginning in the buffer to start reading from.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/> parameter is less than zero.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="sourceOffset"/> + size of <typeparamref name="T"/> (in bytes) exceeds the size of the destination buffer.</exception>
    /// <remarks>
    /// <para>
    /// This will upload the specified <paramref name="value"/> to this buffer. The method will determine how to best upload the data depending on the <see cref="GorgonGraphicsResource.Usage"/> of the
    /// buffer. For example, if the buffer has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/>, then internally, this method will update to the GPU directly.
    /// Otherwise, if it is <see cref="ResourceUsage.Dynamic"/>, or <see cref="ResourceUsage.Staging"/>, it will use a locking pattern which uses the CPU to write data to the buffer. The latter pattern 
    /// is good if the buffer has to change one or more times per frame, otherwise, the former is better where the buffer is updated less than once per frame (i.e. Dynamic is good for multiple times 
    /// per frame, Default is good for once per frame or less).
    /// </para>
    /// <para>
    /// If the user supplies a <paramref name="sourceOffset"/> the data will be copied to the buffer at the offset provided by <paramref name="sourceOffset"/>. 
    /// </para>
    /// <note type="caution">
    /// <para>
    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </remarks>
    /// <example>
    /// The following is an example showing how to upload vertices into a vertex buffer using different techniques:
    /// <code language="csharp">
    /// <![CDATA[
    /// // Our vertex, with a position and color component.
    /// [StructLayout(LayoutKind = LayoutKind.Sequential)] 
    /// struct MyVertex
    /// {
    ///		public Vector4 Position;
    ///		public Vector4 Color;
    /// }
    /// 
    /// GorgonGraphics graphics;
    /// MyVertex[] _vertices = new MyVertex[100];
    /// GorgonVertexBuffer _vertexBuffer;
    /// 
    /// void InitializeVertexBuffer()
    /// {
    ///		_vertices = ... // Fill your vertex array here.
    /// 
    ///		// Create the vertex buffer large enough so that it'll hold 100 vertices.
    ///		_vertexBuffer = new GorgonVertexBuffer(graphics, GorgonVertexBufferInfo.CreateFromType<MyVertex>(_vertices.Length, Usage.Default));
    /// 
    ///		// Copy our data to the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices);
    /// 
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, and 25 vertices.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices, 5, 25);
    ///
    ///		// Copy our data to the vertex buffer, using the 5th index in the vertex array, 25 vertices, and storing at index 2 in the vertex buffer.
    ///     _vertexBuffer.SetData<MyVertex>(_vertices, 5, 25, 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///     // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///     _vertexBuffer.SetData(vertices, 5, 25, 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    /// 
    ///     // Copy our data from a GorgonNativeBuffer.
    ///     using (GorgonNativeBuffer<MyVertex> vertices = new GorgonNativeBuffer<MyVertex>(100))
    ///     {
    ///        // Copy vertices into the native buffer here....
    ///
    ///        // Copy everything.
    ///        _vertexBuffer.SetData(vertices);
    /// 
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer.
    ///        _vertexBuffer.SetData(vertices, 5, 25, 2 * Unsafe.SizeOf<MyVertex>());
    ///
    ///        // Copy our data to the vertex buffer, using the 5th index in the native buffer, 25 vertices, and storing at index 2 in the vertex buffer, using a copy mode.
    ///        _vertexBuffer.SetData(vertices, 5, 25, 2 * Unsafe.SizeOf<MyVertex>(), CopyMode.NoOverWrite);
    ///
    ///        // Get the data back out from the buffer, using index 5 and up to 10 vertices, storing at index 2 of the native buffer.
    ///        _vertexBuffer.GetData<MyVertex>(vertices, 5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>(), 2); 
    ///     }
    ///
    ///     // Get the data back out from the buffer.
    ///     MyVertex[] readBack = _vertexBuffer.GetData<MyVertex>();
    ///
    ///     // Get the data back out from the buffer, starting at index 5 and a count of 10 vertices.
    ///     readBack = _vertexBuffer.GetData<MyVertex>(5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>());
    ///
    ///     // Get the data back out from the buffer, using index 5 and up to 10 vertices, storing at index 2.
    ///     _vertexBuffer.GetData<MyVertex>(readBack, 5 * Unsafe.SizeOf<MyVertex>(), 10 * Unsafe.SizeOf<MyVertex>(), 2);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public void GetData<T>(out T value, int sourceOffset = 0)
        where T : unmanaged
    {
        // Actual byte offset in the buffer.
        int typeSize = Unsafe.SizeOf<T>();

#if DEBUG
        ValidateGetSetData(sourceOffset, 0, typeSize, SizeInBytes, typeSize);
#endif
        value = default;

        unsafe
        {
            fixed (T* valuePtr = &value)
            {
                GetDataPtr(valuePtr, sourceOffset, 1);
            }
        }
    }

    /// <summary>
    /// Function to copy the contents of this buffer into another buffer.
    /// </summary>
    /// <param name="destinationBuffer">The source buffer that will receive the data.</param>
    /// <param name="sourceOffset">[Optional] Starting byte index to start copying from.</param>
    /// <param name="byteCount">[Optional] The number of bytes to copy.</param>
    /// <param name="destOffset">[Optional] The offset, in bytes, within the this buffer to start writing into.</param>
    /// <param name="copyMode">[Optional] Defines how data should be copied into the texture.</param>
    /// <remarks>
    /// <para>
    /// Use this method to copy this <see cref="GorgonBufferCommon"/> to another <see cref="GorgonBufferCommon"/>.
    /// </para>
    /// <para>
    /// The <paramref name="sourceOffset"/>, <paramref name="byteCount"/>, and <paramref name="destOffset"/> parameters allow a portion of the buffer to be written to an offset within the destination.
    /// If the parameters exceed the size of the <paramref name="destinationBuffer"/>, or this buffer, then the values will be clipped to ensure that no overrun is possible.
    /// </para>
    /// <para>
    /// The destination buffer must not have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Immutable"/>.
    /// </para>
    /// <para>
    /// The <paramref name="copyMode"/> parameter defines how the copy will be performed. If the buffer has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Dynamic"/> or
    /// <see cref="ResourceUsage.Default"/> and the <paramref name="copyMode"/> is set to <see cref="CopyMode.Discard"/> then the contents of the buffer are discarded before updating, if it is set to
    /// <see cref="CopyMode.NoOverwrite"/>, then the data will be copied to the destination if we know the GPU is not using the portion being updated. If the <paramref name="copyMode"/> is set to
    /// <see cref="CopyMode.None"/>, then <see cref="CopyMode.Discard"/> is used. For buffers created with a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>, the
    /// <see cref="CopyMode"/> will be ignored and act as though <see cref="CopyMode.None"/> were passed.
    /// </para>
    /// <para>
    /// <note type="caution">
    /// <para>
    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public void CopyTo(GorgonBufferCommon destinationBuffer, int sourceOffset = 0, int byteCount = 0, int destOffset = 0, CopyMode copyMode = CopyMode.None)
    {
        destinationBuffer.ValidateObject(nameof(destinationBuffer));


#if DEBUG
        if (destinationBuffer.Native.Description.Usage == D3D11.ResourceUsage.Immutable)
        {
            throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_ERR_BUFFER_IS_IMMUTABLE);
        }
#endif
        if (byteCount <= 0)
        {
            byteCount = SizeInBytes;
        }

        if ((sourceOffset <= 0) && (destOffset <= 0) && (byteCount == SizeInBytes))
        {
            if (destinationBuffer != this)
            {
                Graphics.D3DDeviceContext.CopyResource(D3DResource, destinationBuffer.D3DResource);
            }
            return;
        }

        if ((sourceOffset > SizeInBytes)
            || (destOffset > destinationBuffer.SizeInBytes))
        {
            return;
        }

        int srcCount = byteCount;
        int destCount = byteCount;

        if ((sourceOffset + srcCount) > SizeInBytes)
        {
            srcCount = SizeInBytes - sourceOffset;
        }

        if ((destOffset + destCount) > destinationBuffer.SizeInBytes)
        {
            destCount = SizeInBytes - destOffset;
        }

        byteCount = srcCount.Min(destCount).Max(1);
        sourceOffset = sourceOffset.Max(0);
        destOffset = destOffset.Max(0);

        Graphics.D3DDeviceContext.CopySubresourceRegion1(destinationBuffer.Native,
                                                         0,
                                                         destOffset,
                                                         0,
                                                         0,
                                                         Native,
                                                         0,
                                                         new D3D11.ResourceRegion
                                                         {
                                                             Top = 0,
                                                             Bottom = 1,
                                                             Left = sourceOffset,
                                                             Right = sourceOffset + byteCount,
                                                             Front = 0,
                                                             Back = 1
                                                         }, (int)copyMode);
    }

    /// <summary>
    /// Function to retrieve a copy of this buffer as a staging resource.
    /// </summary>
    /// <returns>The staging buffer to retrieve.</returns>
    protected abstract GorgonBufferCommon GetStagingInternal();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
    /// </para>
    /// </remarks>
    public override void Dispose()
    {
        Dictionary<BufferShaderViewKey, GorgonShaderResourceView> shaderViews = Interlocked.Exchange(ref _shaderViews, null);
        Dictionary<BufferShaderViewKey, GorgonReadWriteView> unorderedViews = Interlocked.Exchange(ref _uavs, null);

        if (shaderViews is not null)
        {
            // Remove any cached views for this buffer.
            foreach (KeyValuePair<BufferShaderViewKey, GorgonShaderResourceView> view in shaderViews)
            {
                view.Value.Dispose();
            }
        }

        if (unorderedViews is not null)
        {
            foreach (KeyValuePair<BufferShaderViewKey, GorgonReadWriteView> view in unorderedViews)
            {
                view.Value.Dispose();
            }
        }

        Log.Print($"'{Name}': Destroying D3D11 Buffer.", LoggingLevel.Simple);
        Native = null;
        base.Dispose();
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBufferCommon" /> class.
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    protected GorgonBufferCommon(GorgonGraphics graphics)
        : base(graphics)
    {
        Log = graphics.Log;

        this.RegisterDisposable(graphics);
    }
    #endregion
}
