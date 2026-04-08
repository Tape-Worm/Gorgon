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
/// Provides a view that interprets the data in a <see cref="GorgonGpuBuffer"/> as raw byte data.
/// </summary>
/// <remarks>
/// <para>
/// Applications can use a raw buffer to allow a shader to interpret buffer data as blob of byte data. This allows shaders to read data at a byte level, and cast it however they choose.
/// </para>
/// <para>
/// Raw views require that the underlying buffer be aligned to the <see cref="AlignmentRequirement"/> (16 bytes), and that the buffer size be at least <see cref="MinimumElementSize"/> (4 bytes). The 
/// data in the buffer is accessed 4 bytes at a time in the shader.
/// </para>
/// <para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="GorgonShaderBufferView.GetViewHandle"/>
public sealed class GorgonRawBufferView
    : GorgonShaderBufferView
{
    /// <summary>
    /// The alignment, in bytes, required for raw buffer data.
    /// </summary>
    public const int AlignmentRequirement = D3D12.D3D12_RAW_UAV_SRV_BYTE_ALIGNMENT;
    /// <summary>
    /// The minimum size, in bytes, for a single element in the raw buffer.
    /// </summary>
    public const int MinimumElementSize = 4;

    /// <summary>
    /// Function to validate the view settings.
    /// </summary>
    /// <param name="name">The name of the buffer.</param>
    /// <param name="bufferSize">The total size of the buffer.</param>
    /// <param name="resourceOffset"><inheritdoc cref="GorgonConstantBufferView.ValidateConstantView(string, int, long, ulong)" path="/param[@name='resourceOffset']"/></param>
    /// <exception cref="GorgonException"><para>Thrown if the size of the buffer is less than the <see cref="MinimumElementSize"/> (4 bytes).</para>
    /// <para>Throw if the buffer was not aligned to the <see cref="AlignmentRequirement"/> (16 bytes) upon creation.</para>
    /// </exception>
    internal static void ValidateRawView(string name, long bufferSize, ulong resourceOffset)
    {
        if (bufferSize < MinimumElementSize)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_VIEW, name, bufferSize, nameof(GorgonRawBufferView), MinimumElementSize));
        }

        if ((resourceOffset % AlignmentRequirement) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_ALIGNMENT_INCORRECT_FOR_VIEW, name, AlignmentRequirement, nameof(GorgonRawBufferView)));
        }
    }

    /// <inheritdoc/>
    private protected override D3D12_SHADER_RESOURCE_VIEW_DESC GetDesc()
    {
        D3D12_SHADER_RESOURCE_VIEW_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_R32_TYPELESS,
            ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_BUFFER,
            Shader4ComponentMapping = D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING,            
        };

        desc.Buffer.StructureByteStride = 0;
        desc.Buffer.FirstElement = (Buffer.ResourceOffset / MinimumElementSize) + (ulong)StartElementIndex;
        desc.Buffer.NumElements = (uint)ElementCount;
        desc.Buffer.Flags = D3D12_BUFFER_SRV_FLAGS.D3D12_BUFFER_SRV_FLAG_RAW;

        return desc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonRawBufferView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of 4 byte elements in the buffer to view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonRawBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, long startIndex, int elementCount, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonRawBufferView))} - Raw Buffer View", buffer, startIndex, elementCount, MinimumElementSize, owned) => AllocateDescriptors();
}
