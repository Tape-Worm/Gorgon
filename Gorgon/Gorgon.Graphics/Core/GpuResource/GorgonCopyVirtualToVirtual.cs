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
/// Parameters used to copy a virtual texture to a destination virtual texture.
/// </summary>
/// <param name="sourceRegion">The region on the source texture to copy.</param>
/// <param name="sourceHandle">The handle to the allocation on the source texture.</param>
/// <param name="destinationX">The horizontal position within the destination texture allocation.</param>
/// <param name="destinationY">The vertical position within the destination texture allocation.</param>
/// <param name="destinationZ">The depth position within the destination texture allocation. (For <see cref="TextureType.Texture3D"/> only).</param>
/// <param name="destinationHandle">The handle to the allocation on the destination texture.</param>
/// <remarks>
/// <para>
/// The <paramref name="sourceRegion"/> and <paramref name="destinationX"/>, <paramref name="destinationY"/>, and <paramref name="destinationZ"/> are relative to the area in the 
/// <paramref name="sourceHandle"/> and <paramref name="destinationHandle"/> allocations. This means that specifying 0x0 in the region is the upper left corner of the allocation region, and <b>NOT</b> 0x0 in 
/// the actual texture. This is done because areas in the virtual texture can be unallocated when specifying absolute texture coordinates. This protects the user from specifying out of bounds regions.
/// </para>
/// </remarks>
/// <seealso cref="IGorgonCopyMethodsFluent{T}.CopyVirtualToVirtual(GorgonVirtualTexture, GorgonVirtualTexture, ref readonly GorgonCopyVirtualToVirtual)"/>
[method: SetsRequiredMembers]
public readonly ref struct GorgonCopyVirtualToVirtual(GorgonBox sourceRegion, GorgonVirtualTextureHandle sourceHandle, int destinationX, int destinationY, short destinationZ, GorgonVirtualTextureHandle destinationHandle)
{
    /// <summary>
    /// Property to return whether the parameter is considered empty.
    /// </summary>
    public readonly bool IsEmpty => (SourceRegion.Equals(in GorgonBox.Empty)) || (SourceHandle.Equals(GorgonVirtualTextureHandle.Null)) || (DestinationHandle.Equals(GorgonVirtualTextureHandle.Null));

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToVirtual(GorgonBox, GorgonVirtualTextureHandle, int, int, short, GorgonVirtualTextureHandle)" path="/param[@name='sourceRegion']"/>
    /// </summary>
    /// <remarks>
    /// This region will contain either the depth range for a <see cref="TextureType.Texture3D"/>, or the range of array indices for a <see cref="TextureType.Texture1D"/> or 
    /// <see cref="TextureType.Texture2D"/> texture array.
    /// </remarks>
    public readonly GorgonBox SourceRegion = sourceRegion;

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToVirtual(GorgonBox, GorgonVirtualTextureHandle, int, int, short, GorgonVirtualTextureHandle)" path="/param[@name='sourceHandle']"/>
    /// </summary>
    public readonly GorgonVirtualTextureHandle SourceHandle = sourceHandle;

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToVirtual(GorgonBox, GorgonVirtualTextureHandle, int, int, short, GorgonVirtualTextureHandle)" path="/param[@name='destinationX']"/>
    /// </summary>
    public readonly int DestinationX = destinationX;

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToVirtual(GorgonBox, GorgonVirtualTextureHandle, int, int, short, GorgonVirtualTextureHandle)" path="/param[@name='destinationY']"/>
    /// </summary>
    public readonly int DestinationY = destinationY;

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToVirtual(GorgonBox, GorgonVirtualTextureHandle, int, int, short, GorgonVirtualTextureHandle)" path="/param[@name='destinationZ']"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// For <see cref="TextureType.Texture2D"/> textures, this value is ignored and treated as 0.
    /// </para>
    /// </remarks>
    public readonly short DestinationZ = destinationZ;

    /// <summary>
    /// <inheritdoc cref="GorgonCopyVirtualToVirtual(GorgonBox, GorgonVirtualTextureHandle, int, int, short, GorgonVirtualTextureHandle)" path="/param[@name='destinationHandle']"/>
    /// </summary>
    public readonly GorgonVirtualTextureHandle DestinationHandle = destinationHandle;
}
