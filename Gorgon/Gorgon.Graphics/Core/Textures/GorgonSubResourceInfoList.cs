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
    private readonly GorgonTexture? _owner;

    /// <inheritdoc/>
    public GorgonSubResourceInfo this[int index] => _list[index];

    /// <summary>
    /// Property to return the sub resource by its mip level, array index, and optionally, format plane.
    /// </summary>
    public GorgonSubResourceInfo this[int mipLevel, int arrayIndex, int plane = 0] => _list[_owner?.GetSubResourceIndex(mipLevel, arrayIndex, plane) ?? 0];

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
    internal GorgonSubResourceInfoList(GorgonTexture owner, List<GorgonSubResourceInfo> list)
    {
        _owner = owner;
        _list = list;
    }    
}
