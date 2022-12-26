#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 8, 2017 7:22:29 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Gorgon.Graphics;

namespace Gorgon.Renderers;

/// <summary>
/// Defines the colors for each corner of a sprite rectangle.
/// </summary>
public class GorgonSpriteColors
    : IReadOnlyList<GorgonColor>
{
    #region Variables.
    // The renderable that owns this object.
    private readonly BatchRenderable _renderable;
    #endregion

    #region Properties.
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
            0 => _renderable.Vertices[0].Color,
            1 => _renderable.Vertices[1].Color,
            2 => _renderable.Vertices[3].Color,
            3 => _renderable.Vertices[2].Color,
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
        get => _renderable.Vertices[0].Color;
        set
        {
            ref GorgonColor color = ref _renderable.Vertices[0].Color;

            if (GorgonColor.Equals(in value, in color))
            {
                return;
            }

            color = value;
        }
    }

    /// <summary>
    /// Property to set or return the color of the upper right corner.
    /// </summary>
    public GorgonColor UpperRight
    {
        get => _renderable.Vertices[1].Color;
        set
        {
            ref GorgonColor color = ref _renderable.Vertices[1].Color;

            if (GorgonColor.Equals(in value, in color))
            {
                return;
            }

            color = value;
        }
    }

    /// <summary>
    /// Property to set or return the color of the lower left corner.
    /// </summary>
    public GorgonColor LowerLeft
    {
        get => _renderable.Vertices[2].Color;
        set
        {
            ref GorgonColor color = ref _renderable.Vertices[2].Color;

            if (GorgonColor.Equals(in value, in color))
            {
                return;
            }

            color = value;
        }
    }

    /// <summary>
    /// Property to set or return the color of the lower right corner.
    /// </summary>
    public GorgonColor LowerRight
    {
        get => _renderable.Vertices[3].Color;
        set
        {
            ref GorgonColor color = ref _renderable.Vertices[3].Color;

            if (GorgonColor.Equals(in value, in color))
            {
                return;
            }

            color = value;
        }
    }

    /// <summary>Gets the number of elements in the collection.</summary>
    public int Count => 4;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to assign a single color to all corners.
    /// </summary>
    /// <param name="color">The color to assign.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAll(in GorgonColor color)
    {
        ref GorgonColor llColor = ref _renderable.Vertices[2].Color;
        ref GorgonColor lrColor = ref _renderable.Vertices[3].Color;
        ref GorgonColor ulColor = ref _renderable.Vertices[0].Color;
        ref GorgonColor urColor = ref _renderable.Vertices[1].Color;

        if ((GorgonColor.Equals(in color, in llColor))
            && (GorgonColor.Equals(in color, in lrColor))
            && (GorgonColor.Equals(in color, in ulColor))
            && (GorgonColor.Equals(in color, in urColor)))
        {
            return;
        }

       ulColor = urColor = llColor = lrColor = color;
    }

    /// <summary>
    /// Function to copy the colors into the specified destination.
    /// </summary>
    /// <param name="destination">The destination that will receive the copy of the colors.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
    public void CopyTo(GorgonSpriteColors destination)
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
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSpriteColors"/> class.
    /// </summary>
    /// <param name="defaultColor">The default color for the corners.</param>
    /// <param name="renderable">The renderable that owns this object.</param>
    internal GorgonSpriteColors(GorgonColor defaultColor, BatchRenderable renderable)
    {
        _renderable = renderable;
        SetAll(defaultColor);
    }
    #endregion
}
