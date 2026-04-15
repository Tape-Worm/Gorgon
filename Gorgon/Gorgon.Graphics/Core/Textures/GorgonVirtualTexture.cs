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
// Created: April 13, 2026 4:55:42 PM
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Gorgon.Graphics.Core;

public sealed class GorgonVirtualTexture
    : GorgonGpuResource
{
    private readonly GorgonTextureInfo _info;

    internal override bool IsMegaBufferResource => throw new NotImplementedException();

    private protected override void OnGetResourceInfo(out GpuResourceInfo resourceInfo) => throw new NotImplementedException();

    public GorgonVirtualTexture(GorgonGraphics graphics, string name, GorgonTextureInfo info)
        : base(graphics, name)
    {

    }
}
