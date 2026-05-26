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
// Created: May 14, 2026 9:16:37 PM
//

using System.Diagnostics.CodeAnalysis;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Parameters used to copy a virtual texture to a destination texture sub resource.
/// </summary>
/// <param name="sourceRegion">The region on the source texture to copy.</param>
/// <param name="sourceHandle">The handle to the allocation on the source texture.</param>
/// <remarks>
/// <para>
/// The <paramref name="sourceRegion"/> is relative to the area in the <paramref name="sourceHandle"/> allocation. This means that specifying 0x0 in the region is the upper left corner of the allocation 
/// region, and <b>NOT</b> 0x0 in the actual texture. This is done because areas in the virtual texture can be unallocated when specifying absolute texture coordinates. This protects the user from specifying 
/// out of bounds regions.
/// </para>
/// </remarks>
/// <seealso cref="IGorgonCopyMethodsFluent{T}.CopyVirtualToTexture(GorgonVirtualTexture, GorgonTexture, ref readonly GorgonCopyVirtualToTexture)"/>
[method: SetsRequiredMembers]
public readonly ref struct GorgonCopyVirtualToTexture(GorgonBox sourceRegion, GorgonVirtualTextureHandle sourceHandle)
{
    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => (SourceRegion.Equals(in GorgonBox.Empty)) && (SourceHandle.Equals(GorgonVirtualTextureHandle.Null));

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToTexture(GorgonBox, GorgonVirtualTextureHandle)" path="/param[@name='sourceRegion']"/>
    /// </summary>
    /// <remarks>
    /// This region will contain either the depth range for a <see cref="TextureType.Texture3D"/>, or the range of array indices for a <see cref="TextureType.Texture1D"/> or 
    /// <see cref="TextureType.Texture2D"/> texture array.
    /// </remarks>
    public readonly GorgonBox SourceRegion = sourceRegion;

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToTexture(GorgonBox, GorgonVirtualTextureHandle)" path="/param[@name='sourceHandle']"/>
    /// </summary>
    public readonly GorgonVirtualTextureHandle SourceHandle = sourceHandle;

    /// <summary>
    /// Property to return the mip level on the source texture to copy from.
    /// </summary>
    public readonly short DestinationMipLevel
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
    /// Property to return the depth destination position in the texture.
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
}
