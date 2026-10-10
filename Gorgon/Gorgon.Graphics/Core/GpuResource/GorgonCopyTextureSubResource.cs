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
// Created: January 16, 2026 2:28:28 PM
//

using System.Diagnostics.CodeAnalysis;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Parameters used to copy a texture sub resource to another texture sub resource.
/// </summary>
/// <param name="sourceRegion">The region on the source texture to copy.</param>
/// <remarks>
/// <para>
/// These parameters are used to copy a <see cref="GorgonTextureCommon"/> sub resource to another sub resource via the <see cref="GorgonResourceCopier"/> or <see cref="GorgonCommandList"/>.
/// </para>
/// </remarks>
/// <seealso cref="IGorgonCopyMethodsFluent{T}.CopyTexture(GorgonTexture, GorgonTexture, ref readonly GorgonCopyTextureSubResource)"/>
/// <seealso cref="GorgonTexture"/>
/// <seealso cref="GorgonVirtualTexture"/>
[method: SetsRequiredMembers]
public readonly ref struct GorgonCopyTextureSubResource(GorgonBox sourceRegion)
{
    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => SourceRegion.Equals(in GorgonBox.Empty);

    /// <summary>
    /// <inheritdoc cref="GorgonCopyTextureSubResource(GorgonBox)" path="/param[@name='sourceRegion']"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This region will contain either the depth range for a <see cref="TextureType.Texture3D"/>, or the array index, in <see cref="GorgonBox.Z"/>, for a <see cref="TextureType.Texture1D"/> or
    /// <see cref="TextureType.Texture2D"/> texture array. Only a single array index is copied, so the <see cref="GorgonBox.Depth"/> is ignored for these texture types.
    /// </para>
    /// </remarks>
    public readonly GorgonBox SourceRegion = sourceRegion;

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
    /// <para>
    /// This only applies to textures with a texture type of <see cref="TextureType.Texture2D"/>, or <see cref="TextureType.Texture3D"/>.
    /// </para>
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
}
