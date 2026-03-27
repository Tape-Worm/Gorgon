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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Base object for shader resource based views.
/// </summary>
public abstract class GorgonShaderBufferView
    : GorgonResourceView
{
    /// <summary>
    /// Property to return the buffer used by this view.
    /// </summary>
    public GorgonGpuBuffer Buffer
    {
        get;
    }

    /// <summary>
    /// Property to return the size, in bytes, of the buffer to view.
    /// </summary>
    public long Size
    {
        get;
    }

    /// <summary>
    /// Property to return the offset, in bytes, within the buffer that the view starts at.
    /// </summary>
    public long Offset
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShaderBufferView"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
    /// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
    /// <param name="offset">The offset, in bytes, within the buffer to start viewing at.</param>
    /// <param name="size">The size, in bytes, of the view.</param>
    /// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
    internal GorgonShaderBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, long offset, int size, bool owned)
        : base(graphics, name, buffer, owned)
    {
        Buffer = buffer;                
        Offset = offset;
        Size = size;
    }
}
