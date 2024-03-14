
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: April 27, 2019 11:35:57 AM
// 


using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.Services;

/// <summary>
/// Data used to extract sprites from a texture using a grid
/// </summary>
/// <remarks>
/// <para>
/// Developers can use this to extract sprite information using a fixed size grid to retrieve texture coordinates from a texture passed to the service
/// </para>
/// </remarks>
/// <seealso cref="ISpriteExtractorService"/>
public class SpriteExtractionData
{

    // A weak reference to the texture so we don't hang onto it for eternity.
    private WeakReference<GorgonTexture2DView> _textureRef;



    /// <summary>
    /// Property to set or return whether to skip empty sprites.
    /// </summary>
    public bool SkipEmpty
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return the color to use when skipping empty sprites.
    /// </summary>
    public GorgonColor SkipColor
    {
        get;
        set;
    } = GorgonColor.BlackTransparent;

    /// <summary>
    /// Property to set or return the texture that is to be rendered.
    /// </summary>
    public GorgonTexture2DView Texture
    {
        get => !_textureRef.TryGetTarget(out GorgonTexture2DView result) ? null : result;
        set
        {
            if (value is null)
            {
                _textureRef = null;
                return;
            }

            _textureRef = new WeakReference<GorgonTexture2DView>(value);
        }
    }

    /// <summary>
    /// Property to set or return the offset of the grid, in pixels.
    /// </summary>
    public DX.Point GridOffset
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the size of a grid cell.
    /// </summary>
    public DX.Size2 CellSize
    {
        get;
        set;
    } = new DX.Size2(32, 32);

    /// <summary>
    /// Property to set or return the number of columns/rows in the grid.
    /// </summary>
    public DX.Size2 GridSize
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of array indices used.
    /// </summary>
    public int ArrayCount
    {
        get;
        set;
    } = 1;

    /// <summary>
    /// Property to set or return the starting array index to use.
    /// </summary>
    public int StartArrayIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the maximum columns and rows allowed in the grid.
    /// </summary>
    public DX.Size2 MaxGridSize => ((!_textureRef.TryGetTarget(out GorgonTexture2DView texture))
                || (CellSize.Width == 0) || (CellSize.Height == 0))
                ? new DX.Size2(1, 1)
                : new DX.Size2((texture.Width - GridOffset.X) / CellSize.Width, (texture.Height - GridOffset.Y) / CellSize.Height);

    /// <summary>
    /// Property to return the number of sprites that will be extracted.
    /// </summary>
    public int SpriteCount => _textureRef is null ? 0 : (GridSize.Width.Min(MaxGridSize.Width).Max(1) * GridSize.Height.Min(MaxGridSize.Height).Max(1)) * ArrayCount;

}
