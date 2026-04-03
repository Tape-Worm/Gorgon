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
/// Provides a view that interprets the data in a <see cref="GorgonGpuBuffer"/> as a type specified by a <see cref="BufferFormat"/>.
/// </summary>
/// <remarks>
/// <para>
/// Applications can use a typed buffer to allow a shader to interpret buffer data as a type represented by a <see cref="BufferFormat"/>. This can allow shader intrinsics to do format conversions and other 
/// operations.
/// </para>
/// <para>
/// Typed views require that the underlying buffer be aligned to the size of the <see cref="BufferFormat"/> used by the view, and that the buffer size be at least the size of a <see cref="BufferFormat"/>. 
/// This information can be determined by using the <see cref="GorgonFormatInfo"/> object and reading the <see cref="GorgonFormatInfo.SizeInBytes"/> property.  
/// </para>
/// <para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="GorgonShaderBufferView.GetViewHandle"/>
/// <seealso cref="GorgonFormatInfo"/>
/// <seealso cref="BufferFormat"/>
public sealed class GorgonTypedBufferView
    : GorgonShaderBufferView
{
    /// <summary>
    /// Property to return the format type for the view.
    /// </summary>
    public BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Function to validate the view settings.
    /// </summary>
    /// <param name="name">The name of the buffer.</param>
    /// <param name="formatInfo">The information about the format for the view.</param>
    /// <param name="formatSupport">The adapter support for formats.</param>
    /// <param name="bufferSize">The total size of the buffer.</param>
    /// <param name="resourceOffset"><inheritdoc cref="GorgonConstantBufferView.ValidateConstantView(string, int, long, ulong)" path="/param[@name='resourceOffset']"/></param>
    /// <exception cref="GorgonException"><para>Thrown if the buffer does not support the format supplied.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the format is <see cref="BufferFormat.Unknown"/>, typeless, compressed, or a depth/stencil format.</para>
    /// <para>-or-</para>
    /// <para>Thrown if the size of the buffer is less than the <see cref="GorgonFormatInfo.SizeInBytes">format size</see>, in bytes.</para>
    /// <para>-or-</para>
    /// <para>Throw if the buffer was not aligned to the <see cref="GorgonFormatInfo.SizeInBytes">format size</see> upon creation.</para>
    /// </exception>
    internal static void ValidateTypedView(string name, GorgonFormatInfo formatInfo, GorgonBufferFormatSupport formatSupport, long bufferSize, ulong resourceOffset)
    {
        if (!formatSupport.IsBufferFormat)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_COMPATIBLE_WITH_BUFFER, formatInfo.Format));
        }

        if ((formatInfo.Format == BufferFormat.Unknown) || (formatInfo.IsTypeless) || (formatInfo.IsCompressed) || (formatInfo.HasDepth) || (formatInfo.HasStencil))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_INVALID, formatInfo.Format));
        }

        if (bufferSize < formatInfo.SizeInBytes)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_VIEW, name, bufferSize, nameof(GorgonStructuredBufferView), formatInfo.SizeInBytes));
        }

        if ((resourceOffset % (ulong)formatInfo.SizeInBytes) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_ALIGNMENT_INCORRECT_FOR_VIEW, name, formatInfo.SizeInBytes, nameof(GorgonStructuredBufferView)));
        }
    }

    /// <inheritdoc/>
    private protected override D3D12_SHADER_RESOURCE_VIEW_DESC GetDesc()
    {
        D3D12_SHADER_RESOURCE_VIEW_DESC desc = new()
        {
            Format = (DXGI_FORMAT)Format,
            ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_BUFFER,
            Shader4ComponentMapping = D3D12.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING,            
        };

        desc.Buffer.StructureByteStride = 0;
        desc.Buffer.FirstElement = (Buffer.ResourceOffset / (ulong)ElementSize) + (ulong)StartElementIndex;
        desc.Buffer.NumElements = (uint)ElementCount;
        desc.Buffer.Flags = D3D12_BUFFER_SRV_FLAGS.D3D12_BUFFER_SRV_FLAG_NONE;

        return desc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTypedBufferView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="format">The format type for the view.</param>
    /// <param name="formatSize">The size, in bytes, of the format type.</param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of elements of the format type in the buffer.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonTypedBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, BufferFormat format, int formatSize, long startIndex, int elementCount, bool owned)
        : base(graphics, $"{name} - Typed Buffer View ({format})", buffer, startIndex, elementCount, formatSize, owned)
    {
        Format = format;
        AllocateDescriptors();
    }
}
