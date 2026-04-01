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
// Created: February 17, 2026 7:09:03 PM
//

using Gorgon.Graphics.Imaging;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information about each sub resource in the texture.
/// </summary>
/// <param name="SubResourceIndex">The index of the sub resource in the texture.</param>
/// <param name="Width">The width of the sub resource.</param>
/// <param name="Height">The height of the sub resource.</param>
/// <param name="Depth">The depth of the sub resource.</param>
/// <param name="ArrayIndex">The index of the array sub resource.</param>
/// <param name="MipLevel">The mip level for the sub resource.</param>
/// <param name="Plane">The format plane for the sub resource.</param>
/// <param name="RowPitch">The size of a row, in bytes, in the subresource. This value is a multiple of 256 bytes.</param>
/// <param name="RowSize">The size of a row, in bytes, in the subresource. This value is unaligned.</param>
/// <param name="RowCount">The number of rows in the sub resource, this is different from the <paramref name="Height"/> depending on the format of the resource.</param>
/// <param name="Offset">The offset, in bytes, of the sub resource within the containing resource.</param>
/// <remarks>
/// <para>
/// The <see cref="ArrayIndex"/> property is only for <see cref="TextureType.Texture1D"/> and <see cref="TextureType.Texture2D"/> textures. For <see cref="TextureType.Texture3D"/>, this wll always return 1.
/// </para>
/// </remarks>
public record class GorgonSubResourceInfo(int SubResourceIndex, int Width, int Height, int Depth, int ArrayIndex, int MipLevel, int Plane, int RowPitch, long RowSize, int RowCount, long Offset)
{
    /// <summary>
    /// Function to convert this information object into a Direct 3D sub resource foot print.
    /// </summary>
    /// <param name="format">The format of the texture.</param>
    /// <param name="offset">The offset, in bytes, to add to the footprint.</param>
    /// <returns>The D3D sub resource foot print.</returns>
    internal D3D12_PLACED_SUBRESOURCE_FOOTPRINT ToD3DPlacedSubResourceFootPrint(BufferFormat format, ulong offset) => new()
    {
        Offset = (ulong)Offset + offset,
        Footprint = new D3D12_SUBRESOURCE_FOOTPRINT()
        {
            Format = (DXGI_FORMAT)format,
            RowPitch = (uint)RowPitch,
            Width = (uint)Width,
            Height = (uint)Height,
            Depth = (uint)Depth
        }
    };

    /// <summary>
    /// Property to return the size, in bytes, of the sub resource.
    /// </summary>
    /// <remarks>
    /// This value uses the <see cref="RowPitch"/> to calculate the size. This means the size may be larger than expected due to alignment.
    /// </remarks>
    public long SizeInBytes => RowPitch * RowCount;
}
