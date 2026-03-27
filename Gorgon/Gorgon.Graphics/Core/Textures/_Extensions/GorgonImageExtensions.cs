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
// Created: January 21, 2026 8:09:15 PM
//

using System;
using System.Collections.Generic;
using System.Text;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for the <see cref="IGorgonImage"/> and <see cref="IGorgonImageInfo"/> types.
/// </summary>
public static class GorgonImageExtensions
{
    extension(ImageDataType imageDataType)
    {
        /// <summary>
        /// Function to convert a <see cref="ImageDataType"/> value to a <see cref="TextureType"/> value.
        /// </summary>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">Thrown if the image type is not a valid type.</exception>
        public TextureType ToTextureType() =>
            imageDataType switch
            {
                ImageDataType.Image1D => TextureType.Texture1D,
                ImageDataType.Image2D or ImageDataType.ImageCube => TextureType.Texture2D,
                ImageDataType.Image3D => TextureType.Texture3D,
                _ => throw new InvalidCastException()
            };
    }
}
