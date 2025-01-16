
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: August 28, 2020 7:19:52 PM
// 

using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.IO;

/// <summary>
/// Extensions for the <see cref="GorgonGraphics"/> object
/// </summary>
internal static class GorgonGraphicExtensions
{
    /// <summary>
    /// Function to locate a 2D texture resource by its name.
    /// </summary>
    /// <param name="graphics">The graphics interface that is holding the resources.</param>
    /// <param name="textureName">The name of the texture to locate.</param>
    /// <param name="width">The width of the texture to locate.</param>
    /// <param name="height">The height of the texture to locate.</param>
    /// <param name="format">The format of the texture to locate.</param>
    /// <param name="arrayCount">The number of array indices in the texture to locate. Pass -1 to ignore.</param>
    /// <param name="mipCount">The number of mip map levels in the texture to locate. Pass -1 to ignore.</param>
    /// <returns>The texture resource if found, or <b>null</b> if not.</returns>
    public static GorgonTexture2D Locate2DTextureByName(this GorgonGraphics graphics, string textureName, int width, int height, BufferFormat format, int arrayCount, int mipCount)
    {
        if ((graphics is null) || (string.IsNullOrWhiteSpace(textureName)))
        {
            return null;
        }

        IEnumerable<GorgonTexture2D> textureResources = graphics.LocateResourcesByName<GorgonTexture2D>(textureName).Where(item => item is not null);
        int resourceCount = textureResources.Count();

        // Nothing found, leave.
        if (resourceCount == 0)
        {
            return null;
        }

        // There's only 1 texture found, so this must be it. Right?
        if (resourceCount == 1)
        {
            return textureResources.First();
        }

        // If there's more than 1, then we have 
        foreach (GorgonTexture2D texture in textureResources)
        {
            if ((texture.Width == width)
                && (texture.Height == height)
                && (texture.Format == format)
                && ((arrayCount < 1) || (texture.ArrayCount == arrayCount))
                && ((mipCount < 1) || (texture.MipLevels == mipCount)))
            {
                return texture;
            }
        }

        return null;
    }
}
