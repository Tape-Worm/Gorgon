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
// Created: April 21, 2026 12:37:56 AM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Represents an allocation of a region from a <see cref="GorgonVirtualTexture"/>.
/// </summary>
/// <seealso cref="GorgonVirtualTexture"/>
internal readonly struct VirtualTextureAllocation    
{
    /// <summary>
    /// The region that covers the tiles that were allocated on the texture.
    /// </summary>
    public readonly GorgonBox TileRegion;

    /// <summary>
    /// The array index for the sub resource that was allocated.
    /// </summary>
    public readonly short ArrayIndex;

    /// <summary>
    /// The mip level for the sub resource that was allocated.
    /// </summary>
    /// <remarks>
    /// This is the mip level on the actual texture, which ranges from 0 to <see cref="GorgonTextureCommon.MipCount"/><c>-1</c>.
    /// </remarks>
    public readonly short MipLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTextureAllocation"/> value type.
    /// </summary>
    /// <param name="tileRegion"><inheritdoc cref="TileRegion" path="/summary"/></param>
    /// <param name="arrayIndex"><inheritdoc cref="ArrayIndex" path="/summary"/></param>
    /// <param name="mipLevel"><inheritdoc cref="MipLevel" path="/summary"/></param>
    internal VirtualTextureAllocation(GorgonBox tileRegion, short mipLevel, short arrayIndex)
    {
        TileRegion = tileRegion;
        ArrayIndex = arrayIndex;
        MipLevel = mipLevel;
    }
}
