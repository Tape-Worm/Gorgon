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
// Created: March 22, 2026 3:02:19 PM
//

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides a view that interprets the data in a <see cref="GorgonGpuBuffer"/> as a structured data type.
/// </summary>
/// <remarks>
/// <para>
/// Applications can use a structured buffer to allow a shader to interpret buffer data as a custom type. This allows flexible data usage within a shader.
/// </para>
/// <para>
/// Structured views require that the underlying buffer be aligned to the size of a single structured view element, and that the buffer be at least 16 bytes. The structured data must have a size, in bytes, 
/// that is a multiple of <see cref="MinimumElementSize"/> (4 bytes).
/// </para>
/// <para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="GorgonShaderBufferView.GetViewHandle"/>
public sealed class GorgonStructuredBufferView
    : GorgonShaderBufferView
{
    /// <summary>
    /// The minimum size, in bytes, for a single element in the structured buffer.
    /// </summary>
    public const int MinimumElementSize = 4;

    /// <inheritdoc/>
    private protected override D3D12_SHADER_RESOURCE_VIEW_DESC GetDesc()
    {
        D3D12_SHADER_RESOURCE_VIEW_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,
            ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_BUFFER,
            Shader4ComponentMapping = D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING,            
        };

        uint elementSize = (uint)ElementSize;

        desc.Buffer.StructureByteStride = elementSize;
        desc.Buffer.FirstElement = (Buffer.ResourceOffset / elementSize) + (ulong)StartElementIndex;
        desc.Buffer.NumElements = (uint)ElementCount;
        desc.Buffer.Flags = D3D12_BUFFER_SRV_FLAGS.D3D12_BUFFER_SRV_FLAG_NONE;

        return desc;
    }

    /// <summary>
    /// Function to validate the view settings.
    /// </summary>
    /// <param name="name">The name of the buffer.</param>
    /// <param name="structSize">The size, in bytes, of a single element in the view.</param>
    /// <param name="bufferSize">The total size of the buffer.</param>
    /// <param name="resourceOffset"><inheritdoc cref="GorgonConstantBufferView.ValidateConstantView(string, int, long, ulong)" path="/param[@name='resourceOffset']"/></param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="structSize"/> is less than the <see cref="MinimumElementSize"/> (4 bytes).</exception>
    /// <exception cref="GorgonException"><para>Thrown if the <paramref name="structSize"/> is not a multiple of the <see cref="MinimumElementSize"/> (4 bytes).</para>
    /// <para>-or-</para>
    /// <para>Thrown if the size of the buffer is less than the <see cref="MinimumElementSize"/> (4 bytes).</para>
    /// <para>-or-</para>
    /// <para>Throw if the buffer was not aligned to the <paramref name="structSize"/> upon creation.</para>
    /// </exception>
    internal static void ValidateStructuredView(string name, int structSize, long bufferSize, ulong resourceOffset)
    {
        // Only allow buffers that are 16 bytes in size at minimum.
        ArgumentOutOfRangeException.ThrowIfLessThan(structSize, MinimumElementSize);

        if ((structSize % MinimumElementSize) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_ELEMENT_SIZE_NOT_MULTIPLE_OF, MinimumElementSize));
        }

        if (bufferSize < structSize)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_VIEW, name, bufferSize, nameof(GorgonStructuredBufferView), structSize));
        }

        if ((resourceOffset % (ulong)structSize) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_ALIGNMENT_INCORRECT_FOR_VIEW, name, structSize, nameof(GorgonStructuredBufferView)));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonStructuredBufferView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="elementSize">The size, in bytes, of a single element.</param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of elements of the structure type in the buffer.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonStructuredBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, long startIndex, int elementSize, int elementCount, bool owned)
        : base(graphics, $"{name} - Structured Buffer View", buffer, startIndex, elementCount, elementSize, owned) => AllocateDescriptors();
}
