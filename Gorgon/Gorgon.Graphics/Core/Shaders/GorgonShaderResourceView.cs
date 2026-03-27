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
// Created: January 3, 2026 1:54:46 PM
//


using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A generic view for a shader resource.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Write something here.
/// </para>
/// </remarks>
public abstract unsafe class GorgonShaderResourceView
    : GorgonResourceView
{
    /// <summary>
    /// Property to return the size of a shader resource view, in bytes.
    /// </summary>
    protected private uint SrvSize
    {
        get;
    }

    /// <summary>
    /// Property to return the format of view data.
    /// </summary>
    public BufferFormat Format
    {
        get;
    } = BufferFormat.Unknown;

    /// <summary>
    /// Property to return the information about the buffer <see cref="Format"/>.
    /// </summary>
    public GorgonFormatInfo FormatInfo
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonResourceView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="resource"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="format">The format of the view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    private protected GorgonShaderResourceView(GorgonGraphics graphics, string name, GorgonGpuResource resource, BufferFormat format, bool owned)
        : base(graphics, name, resource, owned)
    {
        Format = format;
        FormatInfo = new(format);        
        SrvSize = graphics.D3DDevice.Get()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);        
    }
}
