
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 15, 2016 9:33:57 PM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A buffer for indices used to look up vertices within a <see cref="GorgonVertexBuffer"/>
/// </summary>
/// <remarks>
/// <para>
/// This buffer allows the use of indices to allow for smaller vertex buffers and providing a faster means of finding vertices to draw on the GPU
/// </para>
/// <para>
/// To send indices to the GPU using a index buffer, an application can upload a value type values, representing the indices, to the buffer using one of the 
/// <see cref="GorgonBufferCommon.SetData{T}(ReadOnlySpan{T}, int, CopyMode)"/> overloads. For best performance, it is recommended to upload index data only once, or rarely. However, in 
/// some scenarios, and with the correct <see cref="IGorgonIndexBufferInfo.Usage"/> flag, indices can be updated regularly for things like dynamic tesselation of surface
/// </para>
/// <para> 
/// <example language="csharp">
/// For example, to send a list of indices to a index buffer:
/// <code language="csharp">
/// <![CDATA[
/// GorgonGraphics graphics;
/// ushort[] _indices = new ushort[100];
/// GorgonIndexBuffer _indexBuffer;
/// 
/// void InitializeIndexBuffer()
/// {
///		_indices = ... // Fill your index array here
/// 
///		// Create the index buffer large enough so that it'll hold all 100 indices
///     // Unlike other buffers, we're passing the number of indices instead of bytes. 
///     // This is because we can determine the number of bytes by whether we're using 
///     // 16 bit indices (we are) and the index count
///		_indexBuffer = new GorgonIndexBuffer("MyIB", graphics, new GorgonIndexBufferInfo
///	                                                               {
///		                                                              IndexCount = _indices.Length
///                                                                });
/// 
///		// Copy our data to the index buffer
///     graphics.SetData<ushort>(_indices);
/// }
/// ]]>
/// </code>
/// </example>
/// </para>
/// </remarks>
public sealed class GorgonIndexBuffer
    : GorgonBufferCommon, IGorgonIndexBufferInfo
{
    /// <summary>
    /// The prefix to assign to a default name.
    /// </summary>
    internal const string NamePrefix = nameof(GorgonIndexBuffer);

    // The information used to create the buffer.
    private readonly GorgonIndexBufferInfo _info;

    /// <summary>
    /// Property to return the bind flags used for the D3D 11 resource.
    /// </summary>
    internal override D3D11.BindFlags BindFlags => Native?.Description.BindFlags ?? D3D11.BindFlags.None;

    /// <summary>
    /// Property to return whether or not the buffer is directly readable by the CPU via one of the <see cref="GorgonBufferCommon.GetData{T}(Span{T}, int, int?)"/> methods.
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
    /// <seealso cref="GorgonBufferCommon.GetData{T}(Span{T}, int, int?)"/>
    /// <seealso cref="GorgonBufferCommon.GetData{T}(out T, int)"/>
    /// <seealso cref="GorgonBufferCommon.GetData{T}(int, int?)"/>
    public override bool IsCpuReadable => Usage == ResourceUsage.Staging;

    /// <summary>
    /// Property to return the binding used to bind this buffer to the GPU.
    /// </summary>
    public VertexIndexBufferBinding Binding => _info.Binding;

    /// <summary>
    /// Property to return the number of indices to store.
    /// </summary>
    public int IndexCount => _info.IndexCount;

    /// <summary>
    /// Property to return whether to use 16 bit values for indices.
    /// </summary>
    public bool Use16BitIndices => _info.Use16BitIndices;

    /// <summary>
    /// Property to return the usage for the resource.
    /// </summary>
    public override ResourceUsage Usage => _info.Usage;

    /// <summary>
    /// Property to return the size, in bytes, of the resource.
    /// </summary>
    public override int SizeInBytes => IndexCount * (Use16BitIndices ? sizeof(short) : sizeof(int));

    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    public override string Name => _info.Name;

    /// <summary>
    /// Function to initialize the buffer data.
    /// </summary>
    /// <param name="initialData">The initial data used to populate the buffer.</param>
    private void Initialize<T>(ReadOnlySpan<T> initialData)
        where T : unmanaged
    {
        D3D11.CpuAccessFlags cpuFlags = GetCpuFlags(false, D3D11.BindFlags.IndexBuffer);

        Log.Print($"{Name} Index Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

        GorgonVertexBuffer.ValidateBufferBindings(_info.Usage, 0);

        D3D11.BindFlags bindFlags = D3D11.BindFlags.IndexBuffer;

        if ((_info.Binding & VertexIndexBufferBinding.StreamOut) == VertexIndexBufferBinding.StreamOut)
        {
            bindFlags |= D3D11.BindFlags.StreamOutput;
        }

        if ((_info.Binding & VertexIndexBufferBinding.UnorderedAccess) == VertexIndexBufferBinding.UnorderedAccess)
        {
            bindFlags |= D3D11.BindFlags.UnorderedAccess;
        }

        D3D11.BufferDescription desc = new()
        {
            SizeInBytes = SizeInBytes,
            Usage = (D3D11.ResourceUsage)_info.Usage,
            BindFlags = bindFlags,
            OptionFlags = D3D11.ResourceOptionFlags.None,
            CpuAccessFlags = cpuFlags,
            StructureByteStride = 0
        };

        D3DResource = Native = ResourceFactory.Create(Graphics.D3DDevice, Name, in desc, initialData);
    }

    /// <summary>
    /// Function to retrieve a copy of this buffer as a staging resource.
    /// </summary>
    /// <returns>The staging buffer to retrieve.</returns>
    protected override GorgonBufferCommon GetStagingInternal() => GetStaging();

    /// <summary>
    /// Function to retrieve a copy of this buffer as a staging resource.
    /// </summary>
    /// <returns>The staging buffer to retrieve.</returns>
    public GorgonIndexBuffer GetStaging()
    {
        GorgonIndexBuffer buffer = new(Graphics,
                                                         new GorgonIndexBufferInfo(_info)
                                                         {
                                                             Name = $"{Name} Staging",
                                                             Binding = VertexIndexBufferBinding.None,
                                                             Usage = ResourceUsage.Staging
                                                         });

        CopyTo(buffer);

        return buffer;
    }

    /// <summary>
    /// Function to create a new <see cref="GorgonIndexBufferReadWriteView"/> for this buffer.
    /// </summary>
    /// <param name="startElement">[Optional] The first element to start viewing from.</param>
    /// <param name="elementCount">[Optional] The number of elements to view.</param>
    /// <returns>A <see cref="GorgonIndexBufferReadWriteView"/> used to bind the buffer to a shader.</returns>
    /// <exception cref="GorgonException">Thrown when this buffer does not have a <see cref="Binding"/> of <see cref="VertexIndexBufferBinding.UnorderedAccess"/>.
    /// <para>-or-</para>
    /// <para>Thrown when this buffer has a usage of <see cref="ResourceUsage.Staging"/>.</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This will create an unordered access view that makes a buffer accessible to shaders using unordered access to the data. This allows viewing of the buffer data in a 
    /// different format, or even a subsection of the buffer from within the shader.
    /// </para>
    /// <para>
    /// The format of the view is based on whether the buffer uses 16 bit indices or 32 bit indices.
    /// </para>
    /// <para>
    /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
    /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed.
    /// </para>
    /// <para>
    /// To determine how many elements are in a buffer, use the <see cref="IndexCount"/> property.
    /// </para>
    /// <para>
    /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
    /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
    /// </para>
    /// </remarks>
    public GorgonIndexBufferReadWriteView GetReadWriteView(int startElement = 0, int elementCount = 0)
    {
        if ((Usage == ResourceUsage.Staging)
            || ((Binding & VertexIndexBufferBinding.UnorderedAccess) != VertexIndexBufferBinding.UnorderedAccess))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
        }

        BufferFormat format = Use16BitIndices ? BufferFormat.R16_UInt : BufferFormat.R32_UInt;

        if (!Graphics.FormatSupport.TryGetValue(format, out IGorgonFormatSupportInfo support))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, format));
        }

        if ((support.FormatSupport & BufferFormatSupport.TypedUnorderedAccessView) != BufferFormatSupport.TypedUnorderedAccessView)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, format));
        }

        // Ensure the size of the data type fits the requested format.
        GorgonFormatInfo info = new(format);

        int totalElementCount = GetTotalElementCount(info);

        startElement = startElement.Min(totalElementCount - 1).Max(0);

        if (elementCount <= 0)
        {
            elementCount = totalElementCount - startElement;
        }

        elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

        BufferShaderViewKey key = new(startElement, elementCount, format);
        GorgonIndexBufferReadWriteView result = GetReadWriteView<GorgonIndexBufferReadWriteView>(key);

        if (result is not null)
        {
            return result;
        }

        result = new GorgonIndexBufferReadWriteView(this, format, info, startElement, elementCount, totalElementCount);
        result.CreateNativeView();
        RegisterReadWriteView(key, result);

        return result;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexBuffer" /> class.
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="info"/> parameters are <b>null</b>.</exception>
    public GorgonIndexBuffer(GorgonGraphics graphics, GorgonIndexBufferInfo info)
        : base(graphics)
    {
        _info = new GorgonIndexBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));
        Initialize<byte>(null);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexBuffer" /> class, initialized with <see cref="byte"/> values. 
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <param name="initialData">The initial data used to populate the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="info"/> or the <paramref name="initialData"/> parameters are <b>null</b>.</exception>
    public GorgonIndexBuffer(GorgonGraphics graphics, GorgonIndexBufferInfo info, ReadOnlySpan<byte> initialData)
        : base(graphics)
    {
        _info = new GorgonIndexBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));
        Initialize(initialData.IsEmpty ? throw new ArgumentNullException(nameof(initialData)) : initialData);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexBuffer" /> class, initialized with <see cref="ushort"/> values for 16 bit index buffers. 
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <param name="initialData">The initial data used to populate the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="info"/> or the <paramref name="initialData"/> parameters are <b>null</b>.</exception>
    public GorgonIndexBuffer(GorgonGraphics graphics, GorgonIndexBufferInfo info, ReadOnlySpan<ushort> initialData)
        : base(graphics)
    {
        _info = new GorgonIndexBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));
        Initialize(initialData.IsEmpty ? throw new ArgumentNullException(nameof(initialData)) : initialData);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexBuffer" /> class, initialized with <see cref="short"/> values for 16 bit index buffers.
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <param name="initialData">The initial data used to populate the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="info"/> or the <paramref name="initialData"/> parameters are <b>null</b>.</exception>
    public GorgonIndexBuffer(GorgonGraphics graphics, GorgonIndexBufferInfo info, ReadOnlySpan<short> initialData)
        : base(graphics)
    {
        _info = new GorgonIndexBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));
        Initialize(initialData.IsEmpty ? throw new ArgumentNullException(nameof(initialData)) : initialData);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexBuffer" /> class, initialized with <see cref="uint"/> values for 32 bit index buffers.
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <param name="initialData">The initial data used to populate the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="info"/> or the <paramref name="initialData"/> parameters are <b>null</b>.</exception>
    public GorgonIndexBuffer(GorgonGraphics graphics, GorgonIndexBufferInfo info, ReadOnlySpan<uint> initialData)
        : base(graphics)
    {
        _info = new GorgonIndexBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));
        Initialize(initialData.IsEmpty ? throw new ArgumentNullException(nameof(initialData)) : initialData);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexBuffer" /> class, initialized with <see cref="int"/> values for 32 bit index buffers.
    /// </summary>
    /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <param name="initialData">The initial data used to populate the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="info"/> or the <paramref name="initialData"/> parameters are <b>null</b>.</exception>
    public GorgonIndexBuffer(GorgonGraphics graphics, GorgonIndexBufferInfo info, ReadOnlySpan<int> initialData)
        : base(graphics)
    {
        _info = new GorgonIndexBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));
        Initialize(initialData.IsEmpty ? throw new ArgumentNullException(nameof(initialData)) : initialData);
    }
}
