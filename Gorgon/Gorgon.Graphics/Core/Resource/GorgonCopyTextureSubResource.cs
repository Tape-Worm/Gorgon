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
// Created: January 16, 2026 2:28:28 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Parameters used to copy a texture sub resource to another texture sub resource.
/// </summary>
/// <seealso cref="IGorgonResourceWriter.CopyTexture(GorgonTexture, GorgonTexture, in GorgonCopyTextureSubResource)"/>
public readonly ref struct GorgonCopyTextureSubResource
{
    /// <summary>
    /// An empty texture sub resource parameter.
    /// </summary>
    public static GorgonCopyTextureSubResource Empty => default;

    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => (SourceRegion.Equals(in GorgonBox.Empty)) && (SourceMipLevel == 0) && (SourcePlane == 0)
                && (DestinationX == 0) && (DestinationY == 0) && (DestinationZOrArrayIndex == 0) && (DestinationMipLevel == 0) && (DestinationPlane == 0);

    /// <summary>
    /// Property to return the region on the source texture to copy. 
    /// </summary>
    /// <remarks>
    /// This region will contain either be the depth range for a 3D texture, or the range of array indices for a 1D or 2D texture array.
    /// </remarks>
    public readonly GorgonBox SourceRegion
    {
        get;
        init;
    } = GorgonBox.Empty;

    /// <summary>
    /// Property to return the mip level on the source texture to copy from.
    /// </summary>
    public readonly short SourceMipLevel
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the source format plane index on the source texture to copy from.
    /// </summary>
    public readonly byte SourcePlane
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the horizontal destination position in the destination texture.
    /// </summary>
    public readonly int DestinationX
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the vertical destination position in the destination texture.
    /// </summary>
    /// <remarks>
    /// This only applies to textures with a texture type of <see cref="TextureType.Texture2D"/>, or <see cref="TextureType.Texture3D"/>.
    /// </remarks>
    public readonly int DestinationY
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the depth destination position in the texture or the array index.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the destination texture has a texture type of <see cref="TextureType.Texture1D"/>, or <see cref="TextureType.Texture2D"/>, then this value represents the array index for the texture array.
    /// </para>
    /// <para>
    /// If the destination texture has a texture type of <see cref="TextureType.Texture3D"/>, then this value represents the depth slice in the depth texture.
    /// </para>
    /// </remarks>
    public readonly short DestinationZOrArrayIndex
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the mip level on the destination texture to copy into.
    /// </summary>
    public readonly short DestinationMipLevel
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Property to return the destination format plane index on the destination texture to copy into.
    /// </summary>
    public readonly byte DestinationPlane
    {
        get;
        init;
    } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCopyTextureSubResource"/> value type.
    /// </summary>
    public GorgonCopyTextureSubResource()
    {
    }
}
