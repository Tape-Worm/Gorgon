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
// Created: May 13, 2026 10:21:25 PM
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Gorgon.Graphics.Core.Textures;

/// <summary>
/// Extension methods for the <see cref="IGorgonTextureView{T}"/> type.
/// </summary>
public static class GorgonVirtualTextureViewExtensions
{
    extension(IGorgonTextureView<GorgonVirtualTexture>)
    {
        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="name"></param>
        /// <param name="format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipCount"></param>
        /// <param name="arrayCount"></param>
        /// <returns></returns>
        public static IGorgonTextureView<GorgonVirtualTexture> Create2DTexture(GorgonGraphics graphics, string name, BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1)
        {
            // TODO:
            return null!;
        }
    }
}
