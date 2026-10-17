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
/// Provides a read/write view that interprets the data in a <see cref="GorgonGpuBuffer"/> or <see cref="GorgonIndexBuffer"/> as a type specified by a <see cref="BufferFormat"/>.
/// </summary>
/// <remarks>
/// <para>
/// Applications can use a typed read/write buffer to allow a shader to read and write buffer data as a type represented by a <see cref="BufferFormat"/>. This can allow shader intrinsics to do format 
/// conversions and other operations.
/// </para>
/// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
/// <para>
/// Typed views require that the underlying buffer be aligned to the size of the <see cref="BufferFormat"/> used by the view, and that the buffer size be at least the size of a <see cref="BufferFormat"/>. This 
/// information can be determined by using the <see cref="GorgonFormatInfo"/> object and reading the <see cref="GorgonFormatInfo.SizeInBytes"/> property.
/// </para>
/// <para type="format_support">
/// The format must be usable as a typed read/write view format. This can be determined by reading the <see cref="GorgonBufferFormatSupport.IsTypedReadWriteViewFormat"/> property on the 
/// <see cref="GorgonBufferFormatSupport"/> object returned by the <see cref="GorgonGraphics.FormatSupport"/> property.
/// </para>
/// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
/// </remarks>
/// <seealso cref="GorgonCommandList.WriteConstant{T}(int, in T)"/>
/// <seealso cref="GorgonShaderBufferRwView.GetViewHandle"/>
/// <seealso cref="GorgonFormatInfo"/>
/// <seealso cref="BufferFormat"/>
public sealed class GorgonTypedBufferRwView
    : GorgonShaderBufferRwView
{
    /// <summary>
    /// Property to return the format information for the <see cref="Format"/>.
    /// </summary>
    public GorgonFormatInfo FormatInfo
    {
        get;
    }

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
    /// <param name="resourceOffset"><inheritdoc cref="GorgonConstantBufferView.ValidateConstantView(string, int, ulong, ulong)" path="/param[@name='resourceOffset']"/></param>
    /// <param name="isRwResource"><b>true</b> if the buffer was created with read/write access, <b>false</b> if not.</param>
    /// <exception cref="GorgonException"><para type="norw">
    /// Thrown if the buffer was not created with <see cref="GorgonCommonBufferInfo.HasReadWriteAccess">read/write access</see>.
    /// </para>
    /// <para type="support">Thrown if the format cannot be used with buffers, or cannot be used for a typed read/write view.</para>
    /// <para type="format">Thrown if the format is <see cref="BufferFormat.Unknown"/>, typeless, compressed, or a depth/stencil format.</para>
    /// <para type="size">Thrown if the size of the buffer is less than the <see cref="GorgonFormatInfo.SizeInBytes">format size</see>, in bytes.</para>
    /// <para type="alignment">Thrown if the buffer was not aligned to the <see cref="GorgonFormatInfo.SizeInBytes">format size</see> upon creation.</para>
    /// </exception>
    internal static void ValidateTypedView(string name, GorgonFormatInfo formatInfo, GorgonBufferFormatSupport formatSupport, long bufferSize, ulong resourceOffset, bool isRwResource)
    {
        if (!isRwResource)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_RESOURCE_NO_READ_WRITE_ACCESS, name));
        }

        if ((!formatSupport.IsBufferFormat) || (!formatSupport.IsTypedReadWriteViewFormat))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_COMPATIBLE_WITH_BUFFER, formatInfo.Format));
        }

        if ((formatInfo.Format == BufferFormat.Unknown) || (formatInfo.IsTypeless) || (formatInfo.IsCompressed) || (formatInfo.HasDepth) || (formatInfo.HasStencil))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_INVALID, formatInfo.Format));
        }

        if (bufferSize < formatInfo.SizeInBytes)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_TOO_SMALL_FOR_VIEW, name, bufferSize, nameof(GorgonTypedBufferRwView), formatInfo.SizeInBytes));
        }

        if ((resourceOffset % (ulong)formatInfo.SizeInBytes) != 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_ALIGNMENT_INCORRECT_FOR_VIEW, name, formatInfo.SizeInBytes, nameof(GorgonTypedBufferRwView)));
        }
    }

    /// <inheritdoc/>
    private protected override D3D12_UNORDERED_ACCESS_VIEW_DESC GetDesc()
    {
        D3D12_UNORDERED_ACCESS_VIEW_DESC desc = new()
        {
            Format = (DXGI_FORMAT)Format,
            ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_BUFFER
        };

        desc.Buffer.StructureByteStride = 0;
        desc.Buffer.FirstElement = (Buffer.ResourceOffset / (ulong)ElementSize) + (ulong)StartElementIndex;
        desc.Buffer.NumElements = (uint)ElementCount;
        desc.Buffer.CounterOffsetInBytes = 0;
        desc.Buffer.Flags = D3D12_BUFFER_UAV_FLAGS.D3D12_BUFFER_UAV_FLAG_NONE;

        return desc;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTypedBufferRwView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="formatInfo">The information about the view format type.</param>
    /// <param name="startIndex">The element index within the buffer the view starts at.</param>
    /// <param name="elementCount">The number of elements of the format type in the buffer.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonTypedBufferRwView(GorgonGraphics graphics, string name, GorgonGpuBufferCommon buffer, GorgonFormatInfo formatInfo, long startIndex, int elementCount, bool owned)
        : base(graphics, $"{GorgonGraphicsFactory.GenerateName(name, nameof(GorgonTypedBufferRwView))} - Typed Buffer Read Write (UAV) View ({formatInfo.Format})", buffer, startIndex, elementCount, formatInfo.SizeInBytes, owned)
    {
        Format = formatInfo.Format;
        FormatInfo = formatInfo;
        AllocateDescriptors();
    }
}