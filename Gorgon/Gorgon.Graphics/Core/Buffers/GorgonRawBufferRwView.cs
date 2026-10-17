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
/// Provides a read/write view that interprets the data in a <see cref="GorgonGpuBuffer"/> or <see cref="GorgonIndexBuffer"/> as raw byte data.
/// </summary>
/// <remarks>
/// <para>
/// Applications can use a raw read/write buffer to allow a shader to read and write buffer data as a blob of byte data. This allows shaders to access data at a byte level, and cast it however they choose.
/// </para>
/// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
/// <para>
/// Raw views require that the underlying buffer be aligned to the <see cref="AlignmentRequirement"/> (16 bytes), and that the buffer size be at least <see cref="MinimumElementSize"/> (4 bytes). The data in 
/// the buffer is accessed 4 bytes at a time in the shader.
/// </para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
/// </remarks>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="GorgonShaderBufferRwView.GetViewHandle"/>
public sealed class GorgonRawBufferRwView
    : GorgonShaderBufferRwView
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
    /// <param name="resourceOffset"><inheritdoc cref="GorgonConstantBufferView.ValidateConstantView(string, int, ulong, ulong)" path="/param[@name='resourceOffset']"/></param>
    /// <param name="isRwResource"><b>true</b> if the buffer was created with read/write access, <b>false</b> if not.</param>
    /// <exception cref="GorgonException"><para type="norw">
    /// Thrown if the buffer was not created with <see cref="GorgonCommonBufferInfo.HasReadWriteAccess">read/write access</see>.
    /// </para>
    /// <para type="size">Thrown if the size of the buffer is less than the <see cref="MinimumElementSize"/> (4 bytes).</para>
    /// <para type="alignment">Thrown if the buffer was not aligned to the <see cref="AlignmentRequirement"/> (16 bytes) upon creation.</para>
    /// </exception>
    internal static void ValidateRawView(string name, long bufferSize, ulong resourceOffset, bool isRwResource)
    {
        if (!isRwResource)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RESOURCE_NO_READ_WRITE_ACCESS, name));
        }

        if (bufferSize < MinimumElementSize)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_VIEW, name, bufferSize, nameof(GorgonRawBufferRwView), MinimumElementSize));
        }

        if ((resourceOffset % AlignmentRequirement) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_ALIGNMENT_INCORRECT_FOR_VIEW, name, AlignmentRequirement, nameof(GorgonRawBufferRwView)));
        }
    }

    /// <inheritdoc/>
    private protected override D3D12_UNORDERED_ACCESS_VIEW_DESC GetDesc()
    {
        D3D12_UNORDERED_ACCESS_VIEW_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_R32_TYPELESS,
            ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_BUFFER
        };

        desc.Buffer.StructureByteStride = 0;
        desc.Buffer.FirstElement = (Buffer.ResourceOffset / MinimumElementSize) + (ulong)StartElementIndex;
        desc.Buffer.NumElements = (uint)ElementCount;
        desc.Buffer.CounterOffsetInBytes = 0;
        desc.Buffer.Flags = D3D12_BUFFER_UAV_FLAGS.D3D12_BUFFER_UAV_FLAG_RAW;

        return desc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonRawBufferRwView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of 4 byte elements in the buffer to view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonRawBufferRwView(GorgonGraphics graphics, string name, GorgonGpuBufferCommon buffer, long startIndex, int elementCount, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonRawBufferRwView))} - Raw Buffer Read Write (UAV) View", buffer, startIndex, elementCount, MinimumElementSize, owned) => AllocateDescriptors();
}
