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
// Created: January 11, 2026 1:11:36 PM
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Internal conversion methods.
/// </summary>
internal static class GorgonConversionExtensions
{
    extension(TextureType textureType)
    {
        /// <summary>
        /// Function to convert a <see cref="TextureType"/> to a <see cref="ImageDataType"/>.
        /// </summary>
        /// <param name="isCube"><b>true</b> if the texture is a cube map, <b>false</b> if not.</param>
        /// <returns>The texture type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImageDataType ToImageDataType(bool isCube) => textureType switch
        {
            TextureType.Texture1D => ImageDataType.Image1D,
            TextureType.Texture2D => isCube ? ImageDataType.ImageCube : ImageDataType.Image2D,
            TextureType.Texture3D => ImageDataType.Image3D,
            _ => ImageDataType.Unknown
        };
    }

    extension(GraphicsResourceType resourceType)
    {
        /// <summary>
        /// Function to convert a <see cref="TextureType"/> to a D3D12_RESOURCE_DIMENSION.
        /// </summary>
        /// <returns>The texture type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextureType ToTextureType() => resourceType switch
        {
            GraphicsResourceType.Texture1D => TextureType.Texture1D,
            GraphicsResourceType.Texture2D => TextureType.Texture2D,
            GraphicsResourceType.Texture3D => TextureType.Texture3D,
            _ => TextureType.Unknown
        };
    }
}
