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
// Created: July 2, 2026 12:39:37 AM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common properties for buffer views.
/// </summary>
/// <param name="graphics"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='graphics']"/></param>
/// <param name="name"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='name']"/></param>
/// <param name="buffer"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='resource']"/></param>
/// <param name="owned"><inheritdoc cref="GorgonResourceView(GorgonGraphics, string, GorgonGpuResource, bool)" path="/param[@name='owned']"/></param>
public class GorgonGpuBufferView(GorgonGraphics graphics, string name, GorgonGpuBuffer buffer, bool owned)
    : GorgonResourceView(graphics, name, buffer, owned)
{
    /// <summary>
    /// Property to return the buffer used by this view.
    /// </summary>
    public GorgonGpuBuffer Buffer
    {
        get;
    } = buffer;
}
