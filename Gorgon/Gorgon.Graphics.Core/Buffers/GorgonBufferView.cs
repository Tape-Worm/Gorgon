
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: July 4, 2017 11:54:56 PM
// 


using Gorgon.Core;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A shader resource view for a <see cref="GorgonBuffer"/>
/// </summary>
/// <remarks>
/// <para>
/// This is a generic view to allow a <see cref="GorgonBuffer"/> to be bound to the GPU pipeline as a buffer resource
/// </para>
/// <para>
/// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
/// format within the same group.	
/// </para>
/// <para>
/// This type of view will allow shaders to access the data in the buffer by treating each data item as an "element".  The size of these elements depend on the view <see cref="Format"/>.  
/// </para>
/// </remarks>
/// <seealso cref="GorgonBuffer"/>
public sealed class GorgonBufferView
    : GorgonBufferViewCommon, IGorgonBufferInfo
{

    /// <summary>
    /// Property to return the format for the view.
    /// </summary>
    public BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return information about the <see cref="Format"/> used by this view.
    /// </summary>
    public GorgonFormatInfo FormatInformation
    {
        get;
    }

    /// <summary>
    /// Property to return the size of an element, in bytes.
    /// </summary>
    public override int ElementSize => FormatInformation.SizeInBytes;

    /// <summary>
    /// Property to set or return whether to allow the CPU read access to the buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value indicates whether or not the CPU can directly access the buffer for reading. If this value is <b>false</b>, the buffer still can be read, but will be done through an intermediate
    /// staging buffer, which is obviously less performant. 
    /// </para>
    /// <para>
    /// This value is treated as <b>false</b> if the buffer does not have a <see cref="IGorgonBufferInfo.Binding"/> containing the <see cref="BufferBinding.Shader"/> flag, and does not have a
    /// <see cref="IGorgonBufferInfo.Usage"/> of <see cref="ResourceUsage.Default"/>. This means any reads will be done through an intermediate staging buffer, impacting performance.
    /// </para>
    /// <para>
    /// If the <see cref="IGorgonBufferInfo.Usage"/> property is set to <see cref="ResourceUsage.Staging"/>, then this value is treated as <b>true</b> because staging buffers are CPU only and as such,
    /// can be read directly by the CPU regardless of this value.
    /// </para>
    /// </remarks>
    public bool AllowCpuRead => Buffer?.IsCpuReadable ?? false;

    /// <summary>
    /// Property to return the size, in bytes, of an individual structure in a structured buffer.
    /// </summary>
    /// <remarks>
    /// This value is always 0 for this type of view.
    /// </remarks>
    int IGorgonBufferInfo.StructureSize => 0;

    /// <summary>
    /// Property to return whether to allow raw unordered views of the buffer.
    /// </summary>
    /// <remarks>
    /// This value is always <b>false</b> for this type of view.
    /// </remarks>
    bool IGorgonBufferInfo.AllowRawView => false;

    /// <summary>
    /// Property to return the type of binding for the GPU.
    /// </summary>
    public BufferBinding Binding => Buffer?.Binding ?? BufferBinding.None;

    /// <summary>
    /// Property to return whether the buffer will contain indirect argument data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This flag only applies to buffers with a <see cref="IGorgonBufferInfo.Binding"/> of <see cref="BufferBinding.ReadWrite"/>, and/or <see cref="BufferBinding.Shader"/>. If the binding is set
    /// to anything else, then this flag is treated as being set to <b>false</b>.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IndirectArgs => Buffer?.IndirectArgs ?? false;

    /// <summary>
    /// Property to return the size of the buffer, in bytes.
    /// </summary>
    int IGorgonBufferInfo.SizeInBytes => Buffer?.SizeInBytes ?? 0;

    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    string IGorgonNamedObject.Name => Buffer?.Name;



    /// <summary>Function to retrieve the necessary parameters to create the native view.</summary>
    /// <returns>A shader resource view descriptor.</returns>
    private protected override ref readonly D3D11.ShaderResourceViewDescription1 OnGetSrvParams()
    {
        SrvDesc = new D3D11.ShaderResourceViewDescription1
        {
            Format = (DXGI.Format)Format,
            Dimension = D3D.ShaderResourceViewDimension.ExtendedBuffer,
            BufferEx = new D3D11.ShaderResourceViewDescription.ExtendedBufferResource
            {
                FirstElement = StartElement,
                ElementCount = ElementCount,
                Flags = D3D11.ShaderResourceViewExtendedBufferFlags.None
            }
        };

        return ref SrvDesc;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonBufferView"/> class.
    /// </summary>
    /// <param name="buffer">The buffer to bind to the view.</param>
    /// <param name="format">The format of the view into the buffer.</param>
    /// <param name="formatInfo">The information about the format being used.</param>
    /// <param name="startingElement">The starting element in the buffer to view.</param>
    /// <param name="elementCount">The number of elements in the buffer to view.</param>
    /// <param name="totalElementCount">The total number of elements in the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="buffer"/>, or the <paramref name="formatInfo"/> parameter is <b>null</b>.</exception>
    internal GorgonBufferView(GorgonBuffer buffer,
                                  BufferFormat format,
                                  GorgonFormatInfo formatInfo,
                                  int startingElement,
                                  int elementCount,
                                  int totalElementCount)
        : base(buffer, startingElement, elementCount, totalElementCount)
    {
        FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
        Format = format;
    }

}
