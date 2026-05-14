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
// Created: March 5, 2026 10:47:19 PM
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of sub resources in a <see cref="GorgonTexture"/>.
/// </summary>
public sealed class GorgonSubResourceInfoList
    : IReadOnlyList<GorgonSubResourceInfo>
{
    /// <summary>
    /// An empty list.
    /// </summary>
    internal static readonly GorgonSubResourceInfoList Empty = new();

    private readonly List<GorgonSubResourceInfo> _list = [];
    private readonly GorgonTextureCommon? _owner;

    /// <inheritdoc/>
    public GorgonSubResourceInfo this[int index] => _list[index];

    /// <summary>
    /// Property to return the sub resource by its mip level, array index, and optionally, format plane.
    /// </summary>
    public GorgonSubResourceInfo this[short mipLevel, short arrayIndex, byte plane = 0] => _list[_owner?.GetSubResourceIndex(mipLevel, arrayIndex, plane) ?? 0];

    /// <inheritdoc/>
    public int Count => _list.Count;

    /// <inheritdoc/>
    public IEnumerator<GorgonSubResourceInfo> GetEnumerator() => _list.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    /// <summary>
    /// Internal constructor for empty lists.
    /// </summary>    
    private GorgonSubResourceInfoList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSubResourceInfoList"/> class.
    /// </summary>
    /// <param name="owner">The texture that owns the sub resources.</param>
    /// <param name="list">The list of sub resources.</param>
    internal GorgonSubResourceInfoList(GorgonTextureCommon owner, List<GorgonSubResourceInfo> list)
    {
        _owner = owner;
        _list = list;
    }    
}

/// <summary>
/// A list of sub resource tile information data structures in a <see cref="GorgonVirtualTexture"/>.
/// </summary>
public sealed class GorgonSubResourceTileInfoList
    : IReadOnlyList<GorgonSubResourceTileInfo>
{
    /// <summary>
    /// An empty list.
    /// </summary>
    internal static readonly GorgonSubResourceTileInfoList Empty = new();

    private readonly List<GorgonSubResourceTileInfo> _list = [];
    private readonly GorgonVirtualTexture? _owner;

    /// <inheritdoc/>
    public GorgonSubResourceTileInfo this[int index] => _list[index];

    /// <summary>
    /// Property to return the sub resource by its mip level, array index, and optionally, format plane.
    /// </summary>
    public GorgonSubResourceTileInfo this[short mipLevel, short arrayIndex] => _list[_owner?.GetTileSubResourceIndex(mipLevel, arrayIndex) ?? 0];

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// This count is not necessarily the number of sub resources on the texture (see <see cref="GorgonTextureCommon.SubResources"/> for that). 
    /// </para>
    /// <para>
    /// If multiple mip levels can fit into a single tile, then the mip information will be collapsed down into a single sub resource entry (<see cref="GorgonSubResourceTileInfo.MipCount"/>).
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTextureCommon.SubResources"/>
    /// <seealso cref="GorgonSubResourceTileInfo"/>
    public int Count => _list.Count;

    /// <inheritdoc/>
    public IEnumerator<GorgonSubResourceTileInfo> GetEnumerator() => _list.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    /// <summary>
    /// Internal constructor for empty lists.
    /// </summary>    
    private GorgonSubResourceTileInfoList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSubResourceTileInfoList"/> class.
    /// </summary>
    /// <param name="owner">The texture that owns the sub resource tile information.</param>
    /// <param name="list">The list of sub resources.</param>
    internal GorgonSubResourceTileInfoList(GorgonVirtualTexture owner, List<GorgonSubResourceTileInfo> list)
    {
        _owner = owner;
        _list = list;
    }
}
