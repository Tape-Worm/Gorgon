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
/// Parameters used to copy a texture sub resource to a destination virtual texture.
/// </summary>
/// <param name="sourceRegion">The region on the source texture to copy.</param>
/// <param name="destinationHandle">The handle to the allocation on the destination texture.</param>
/// <seealso cref="IGorgonCopyMethodsFluent{T}.CopyTextureToVirtual(GorgonTexture, GorgonVirtualTexture, ref readonly GorgonCopyTextureToVirtual)"/>
[method: SetsRequiredMembers]
public readonly ref struct GorgonCopyTextureToVirtual(GorgonBox sourceRegion, GorgonVirtualTextureHandle destinationHandle)
{
    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => (SourceRegion.Equals(in GorgonBox.Empty)) && (DestinationHandle.Equals(GorgonVirtualTextureHandle.Null));

    /// <summary>
    /// <inheritdoc cref="GorgonCopyTextureToVirtual(GorgonBox, GorgonVirtualTextureHandle)" path="/param[@name='sourceRegion']"/>
    /// </summary>
    /// <remarks>
    /// This region will contain either the depth range for a <see cref="TextureType.Texture3D"/>, or the range of array indices for a <see cref="TextureType.Texture1D"/> or 
    /// <see cref="TextureType.Texture2D"/> texture array.
    /// </remarks>
    public readonly GorgonBox SourceRegion = sourceRegion;

    /// <summary>
    /// <inheritdoc cref="GorgonCopyTextureToVirtual(GorgonBox, GorgonVirtualTextureHandle)" path="/param[@name='destinationHandle']"/>
    /// </summary>
    public readonly GorgonVirtualTextureHandle DestinationHandle = destinationHandle;

    /// <summary>
    /// Property to return the mip level on the source texture to copy from.
    /// </summary>
    public readonly short SourceMipLevel
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
    /// If the destination texture has a texture type of <see cref="TextureType.Texture1D"/>, or <see cref="TextureType.Texture2D"/>, then this value is ignored and treated as 0.
    /// </para>
    /// </remarks>
    public readonly short DestinationZ
    {
        get;
        init;
    } = 0;
}
