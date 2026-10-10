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
// Created: March 22, 2026 3:02:19 PM
//

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides a read/write view that interprets the data in a <see cref="GorgonGpuBuffer"/> as a structured data type.
/// </summary>
/// <remarks>
/// <para>
/// Applications can use a structured read/write buffer to allow a shader to read and write buffer data as a custom type. This allows flexible data usage within a shader.
/// </para>
/// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
/// <para>
/// Structured views require that the underlying buffer be aligned to the size of a single structured view element, and that the buffer be at least the size of a single element. The structured data must have a 
/// size, in bytes, that is a multiple of <see cref="MinimumElementSize"/> (4 bytes).
/// </para>
/// <para type="counter">
/// A structured read/write view can be created with a counter. Shaders use the counter through the <c>IncrementCounter</c> and <c>DecrementCounter</c> methods of a <c>RWStructuredBuffer</c>, or through an 
/// <c>AppendStructuredBuffer</c> or <c>ConsumeStructuredBuffer</c>. The counter is set to 0 when the view is created, and is stored in the <see cref="CounterBuffer"/>.
/// </para>
/// <para type="counter_note">
/// <note type="important">
/// <para>
/// Shaders must not use counter operations on a view that was created without a counter.
/// </para>
/// </note>
/// </para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
/// </remarks>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="GorgonShaderBufferRwView.GetViewHandle"/>
public sealed class GorgonStructuredBufferRwView
    : GorgonShaderBufferRwView
{
    /// <summary>
    /// The minimum size, in bytes, for a single element in the structured buffer.
    /// </summary>
    public const int MinimumElementSize = 4;

    /// <summary>
    /// Property to return the buffer that holds the counter for the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the view was created with a counter, then this buffer holds the current value of the counter as a 32-bit unsigned integer. If the view was created without a counter, then this value is <b>null</b>.
    /// </para>
    /// <para>
    /// The counter buffer is owned by the view, and is disposed when the view is disposed.
    /// </para>
    /// </remarks>
    public GorgonGpuBuffer? CounterBuffer
    {
        get;
    }

    /// <inheritdoc/>
    private protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CounterBuffer?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    private protected override D3D12_UNORDERED_ACCESS_VIEW_DESC GetDesc()
    {
        D3D12_UNORDERED_ACCESS_VIEW_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,
            ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_BUFFER            
        };
               
        uint elementSize = (uint)ElementSize;

        desc.Buffer.StructureByteStride = elementSize;
        desc.Buffer.FirstElement = (Buffer.ResourceOffset / elementSize) + (ulong)StartElementIndex;
        desc.Buffer.NumElements = (uint)ElementCount;
        desc.Buffer.CounterOffsetInBytes = CounterBuffer is null ? 0 : CounterBuffer.ResourceOffset;
        desc.Buffer.Flags = D3D12_BUFFER_UAV_FLAGS.D3D12_BUFFER_UAV_FLAG_NONE;

        return desc;
    }

    /// <inheritdoc/>
    private protected override void OnAllocateDescriptors() => AllocateDescriptors(CounterBuffer);    

    /// <summary>
    /// Function to validate the view settings.
    /// </summary>
    /// <param name="name">The name of the buffer.</param>
    /// <param name="structSize">The size, in bytes, of a single element in the view.</param>
    /// <param name="bufferSize">The total size of the buffer.</param>
    /// <param name="resourceOffset"><inheritdoc cref="GorgonConstantBufferView.ValidateConstantView(string, int, ulong, ulong)" path="/param[@name='resourceOffset']"/></param>
    /// <param name="isRwResource"><b>true</b> if the buffer was created with read/write access, <b>false</b> if not.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="structSize"/> is less than the <see cref="MinimumElementSize"/> (4 bytes).</exception>
    /// <exception cref="GorgonException"><para type="norw">
    /// Thrown if the buffer was not created with <see cref="GorgonCommonBufferInfo.HasReadWriteAccess">read/write access</see>.
    /// </para>
    /// <para type="multiple">Thrown if the <paramref name="structSize"/> is not a multiple of the <see cref="MinimumElementSize"/> (4 bytes).</para>
    /// <para type="size">Thrown if the size of the buffer is less than the <paramref name="structSize"/>.</para>
    /// <para type="alignment">Thrown if the buffer was not aligned to the <paramref name="structSize"/> upon creation.</para>
    /// </exception>
    internal static void ValidateStructuredView(string name, int structSize, long bufferSize, ulong resourceOffset, bool isRwResource)
    {
        if (!isRwResource)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RESOURCE_NO_READ_WRITE_ACCESS, name));
        }

        // Only allow buffers that are 4 bytes in size at minimum.
        ArgumentOutOfRangeException.ThrowIfLessThan(structSize, MinimumElementSize);

        if ((structSize % MinimumElementSize) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_ELEMENT_SIZE_NOT_MULTIPLE_OF, MinimumElementSize));
        }

        if (bufferSize < structSize)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_VIEW, name, bufferSize, nameof(GorgonStructuredBufferRwView), structSize));
        }

        if ((resourceOffset % (ulong)structSize) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_ALIGNMENT_INCORRECT_FOR_VIEW, name, structSize, nameof(GorgonStructuredBufferRwView)));
        }
    }

    /// <summary>
    /// Function to create a structured buffer based on the type passed to the function, and its associated buffer.
    /// </summary>
    /// <typeparam name="T">The type of structured data. Must be an unmanaged type.</typeparam>
    /// <param name="graphics"><inheritdoc cref="GorgonConstantBufferView.CreateConstantBuffer(GorgonGraphics, string, long, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonConstantBufferView.CreateConstantBuffer(GorgonGraphics, string, long, bool)" path="/param[@name='name']"/></param>
    /// <param name="elementCount">The number of elements of type <typeparamref name="T"/> in the buffer.</param>
    /// <param name="includeCounter">[Optional] <b>true</b> to create the view with a counter, <b>false</b> to create it without one.</param>
    /// <returns>A new <see cref="GorgonStructuredBufferRwView"/> and the associated <see cref="GorgonGpuBuffer"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the size of the type <typeparamref name="T"/> is less than the <see cref="MinimumElementSize"/> (4 bytes).</exception>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the <paramref name="elementCount"/> is less than 1.
    /// </para>
    /// <para>Thrown if the size of the type <typeparamref name="T"/> is not a multiple of the <see cref="MinimumElementSize"/> (4 bytes).</para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This function is a convenience method that builds a buffer, with read/write access, and a default read/write view to pass to shaders. Buffers created with this method will be destroyed when the default 
    /// view returned is disposed.
    /// </para>
    /// <inheritdoc cref="GorgonStructuredBufferRwView" path="/remarks/para[@type='counter']"/>
    /// <inheritdoc cref="GorgonStructuredBufferRwView" path="/remarks/para[@type='counter_note']"/>
    /// <para>
    /// The default value for the <paramref name="includeCounter"/> parameter is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGpuBuffer"/>
    public static unsafe GorgonStructuredBufferRwView CreateStructuredBuffer<T>(GorgonGraphics graphics, string name, int elementCount, bool includeCounter = false)
        where T : unmanaged
    {
        int typeSize = sizeof(T);
        GorgonGpuBufferInfo bufferInfo = new(typeSize * elementCount)
        {
            Alignment = typeSize,
            HasReadWriteAccess = true
        };

        GorgonGpuBuffer buffer = new(graphics, name, bufferInfo);

        try
        {
            return buffer.GetStructuredBufferReadWriteView(sizeof(T), 0, null, includeCounter, true);
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStructuredBufferRwView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="elementSize">The size, in bytes, of a single element.</param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of elements of the structure type in the buffer.</param>
    /// <param name="includeCounter"><b>true</b> to create a counter for the view, <b>false</b> to create the view without one.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonStructuredBufferRwView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, long startIndex, int elementSize, int elementCount, bool includeCounter, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonStructuredBufferRwView))} - Structured Buffer Read Write (UAV) View", buffer, startIndex, elementCount, elementSize, owned)
    {
        if (includeCounter)
        {
            CounterBuffer = new GorgonGpuBuffer(graphics, $"{Name} - Counter buffer", new GorgonGpuBufferInfo(sizeof(uint))
            {
                HasReadWriteAccess = true,
                Alignment = D3D12.D3D12_UAV_COUNTER_PLACEMENT_ALIGNMENT,
            });

            graphics.Queues.GlobalCopier.BeginUpload()
                                        .CopyValue(0, CounterBuffer)
                                        .End();
        }

        AllocateDescriptors(CounterBuffer);
    }
}
