﻿
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: February 8, 2017 7:22:29 PM
// 

using System.Collections;
using System.Runtime.CompilerServices;
using Gorgon.Graphics;

namespace Gorgon.Renderers;

/// <summary>
/// Defines the colors for each corner of a text sprite glyph rectangle
/// </summary>
public class GorgonGlyphColors
    : IReadOnlyList<GorgonColor>
{

    // The renderable that owns this object.
    private readonly TextRenderable _renderable;

    /// <summary>
    /// Property to set or return the corner color by index.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is not between 0 and 3.</exception>
    /// <remarks>
    /// The ordering of the indices is as follows: 0 - Upper left, 1 - Upper right, 2 - Lower right, 3 - Lower left.
    /// </remarks>
    public GorgonColor this[int index]
    {
        get => index switch
        {
            0 => _renderable.UpperLeftColor,
            1 => _renderable.UpperRightColor,
            2 => _renderable.LowerRightColor,
            3 => _renderable.LowerLeftColor,
            _ => throw new ArgumentOutOfRangeException(),
        };
        set
        {
            switch (index)
            {
                case 0:
                    UpperLeft = value;
                    return;
                case 1:
                    UpperRight = value;
                    return;
                case 2:
                    LowerRight = value;
                    return;
                case 3:
                    LowerLeft = value;
                    return;
            }

            throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Property to set or return the color of the upper left corner.
    /// </summary>
    public GorgonColor UpperLeft
    {
        get => _renderable.UpperLeftColor;
        set
        {
            if (GorgonColor.Equals(value, _renderable.UpperLeftColor))
            {
                return;
            }

            _renderable.UpperLeftColor = value;
            _renderable.HasVertexColorChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the color of the upper right corner.
    /// </summary>
    public GorgonColor UpperRight
    {
        get => _renderable.UpperRightColor;
        set
        {
            if (GorgonColor.Equals(value, _renderable.UpperRightColor))
            {
                return;
            }

            _renderable.UpperRightColor = value;
            _renderable.HasVertexColorChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the color of the lower left corner.
    /// </summary>
    public GorgonColor LowerLeft
    {
        get => _renderable.LowerLeftColor;
        set
        {
            if (GorgonColor.Equals(value, _renderable.LowerLeftColor))
            {
                return;
            }

            _renderable.LowerLeftColor = value;
            _renderable.HasVertexColorChanges = true;
        }
    }

    /// <summary>
    /// Property to set or return the color of the lower right corner.
    /// </summary>
    public GorgonColor LowerRight
    {
        get => _renderable.LowerRightColor;
        set
        {
            if (GorgonColor.Equals(value, _renderable.LowerRightColor))
            {
                return;
            }

            _renderable.LowerRightColor = value;
            _renderable.HasVertexColorChanges = true;
        }
    }

    /// <summary>Gets the number of elements in the collection.</summary>
    public int Count => 4;

    /// <summary>
    /// Function to assign a single color to all corners.
    /// </summary>
    /// <param name="color">The color to assign.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAll(GorgonColor color)
    {
        if ((GorgonColor.Equals(_renderable.LowerLeftColor, color))
            && (GorgonColor.Equals(_renderable.LowerRightColor, color))
            && (GorgonColor.Equals(_renderable.UpperLeftColor, color))
            && (GorgonColor.Equals(_renderable.UpperRightColor, color)))
        {
            return;
        }

        _renderable.LowerLeftColor = _renderable.LowerRightColor = _renderable.UpperRightColor = _renderable.UpperLeftColor = color;
        _renderable.HasVertexColorChanges = true;
    }

    /// <summary>
    /// Function to copy the colors into the specified destination.
    /// </summary>
    /// <param name="destination">The destination that will receive the copy of the colors.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
    public void CopyTo(GorgonGlyphColors destination)
    {
        if (destination is null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        destination.LowerLeft = LowerLeft;
        destination.LowerRight = LowerRight;
        destination.UpperRight = UpperRight;
        destination.UpperLeft = UpperLeft;
    }

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<GorgonColor> GetEnumerator()
    {
        for (int i = 0; i < 4; ++i)
        {
            yield return this[i];
        }
    }

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < 4; ++i)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGlyphColors"/> class.
    /// </summary>
    /// <param name="defaultColor">The default color for the corners.</param>
    /// <param name="renderable">The renderable that owns this object.</param>
    internal GorgonGlyphColors(GorgonColor defaultColor, TextRenderable renderable)
    {
        _renderable = renderable;
        SetAll(defaultColor);
    }
}
