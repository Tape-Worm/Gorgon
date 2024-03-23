
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
// Created: March 24, 2019 10:22:58 AM
// 


using System.Numerics;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Defines what data to use as a mask when determining clip boundaries
/// </summary>
internal enum ClipMask
{
    /// <summary>
    /// Use the alpha channel of the image.
    /// </summary>
    Alpha = 0,
    /// <summary>
    /// Use the color channels of the image.
    /// </summary>
    Color = 1
}

/// <summary>
/// A clipper that automatically defines the bounds of the sprite based on pixel data
/// </summary>
internal class PickClipperService
{

    /// <summary>
    /// Stores a span of lines that form the boundaries of where to clip.
    /// </summary>
    private struct ClipSpan
    {
        /// <summary>
        /// Starting point for the clip span.
        /// </summary>
        public int Start;
        /// <summary>
        /// End point for the clip span.
        /// </summary>
        public int End;
        /// <summary>
        /// Vertical position for the clip span.
        /// </summary>
        public int Y;
    }



    // A list of pixels that have been checked by the clipper.
    private bool[] _pixels = [];
    // The image data.
    private IGorgonImageBuffer _imageData;
    // The padding for the rectangle.
    private int _padding;



    /// <summary>
    /// Property to set or return the padding for the clipping rectangle.
    /// </summary>
    public int Padding
    {
        get => _padding;
        set => _padding = value.Max(0).Min(16);
    }

    /// <summary>
    /// Property to set or return the pixel data for the texture.
    /// </summary>
    public IGorgonImageBuffer ImageData
    {
        get => _imageData;
        set
        {
            if (_imageData == value)
            {
                return;
            }

            _imageData = value;

            if (value is null)
            {
                _pixels = [];
                return;
            }

            _pixels = new bool[_imageData.Width * _imageData.Height];
        }
    }



    /// <summary>
    /// Function to determine if the pixel at the specified location is a value that falls within the mask.
    /// </summary>
    /// <param name="position">The position of the pixel.</param>
    /// <param name="clipMask">Determines how to use the clip color when detecting rectangular boundaries.</param>
    /// <param name="clipColor">The color used to determine the boundaries of the rectangular region.</param>
    /// <returns><b>true</b> if the pixel is a masked value, or <b>false</b> if not.</returns>
    private bool IsMaskValue(GorgonPoint position, ClipMask clipMask, GorgonColor clipColor)
    {
        int color = clipMask == ClipMask.Alpha ? (int)(clipColor.Alpha * 255) : GorgonColor.ToABGR(clipColor);

        // The linear memory address.
        int location = (position.X * ImageData.FormatInformation.SizeInBytes) + (position.Y * ImageData.PitchInformation.RowPitch);

        // Grab the pixel at the location.
        int pixel = ImageData.Data.AsRef<int>(location);

        // Images should be in RGBA 32 bit format.  If they're not, then we'll get bad data.
        if (clipMask == ClipMask.Alpha)
        {
            pixel = ((pixel >> 24) & 0xff);
        }

        return pixel == color;
    }

    /// <summary>
    /// Function to record a span used to determine the size of the sprite extents.
    /// </summary>
    /// <param name="position">The pixel position to read.</param>
    /// <param name="clipMask">Determines how to use the clip color when detecting rectangular boundaries.</param>
    /// <param name="clipColor">The color used to determine the boundaries of the rectangular region.</param>
    /// <returns>A new span value.</returns>
    private ClipSpan GetSpan(GorgonPoint position, ClipMask clipMask, GorgonColor clipColor)
    {
        int left = position.X;
        int right = position.X;
        int index = (position.Y * ImageData.Width) + position.X;

        // Get starting point of the span by seeking the left most edge.
        while (true)
        {
            left--;
            _pixels[index] = true;

            index--;

            if ((left < 0) || (IsMaskValue(new GorgonPoint(left, position.Y), clipMask, clipColor)) || (_pixels[index]))
            {
                break;
            }
        }
        left++;

        // Reset the index into the pixel buffer.
        index = (position.Y * ImageData.Width) + position.X;

        // Get the ending point of the span by seeking the right most edge.
        while (true)
        {
            right++;
            _pixels[index] = true;

            index++;

            if ((right >= ImageData.Width) || (IsMaskValue(new GorgonPoint(right, position.Y), clipMask, clipColor)) || (_pixels[index]))
            {
                break;
            }
        }

        right--;

        return new ClipSpan
        {
            Start = left,
            End = right,
            Y = position.Y
        };
    }

    /// <summary>
    /// Function to scan the <see cref="ImageData"/> and find a rectangle region from the point specified.
    /// </summary>
    /// <param name="imagePosition">The position on the image to start sampling from.</param>
    /// <param name="maskColor">The color/alpha value used to determine the rectangular boundaries.</param>
    /// <param name="clipMask">[Optional] Determines how the mask color is used to identify the rectangular boundaries.</param>
    /// <returns>A new rectangle for clipping, or <b>null</b> if one could not be generated.</returns>
    /// <remarks>
    /// <para>
    /// This will scan the image until it finds the specified <paramref name="maskColor"/> in the image. It does this until it cannot scan anywhere else and return the rectangular region that is 
    /// surrounded by the <paramref name="maskColor"/>. Selecting a pixel that has the same value as <paramref name="maskColor"/>, then <b>null</b> will returned, signifying that no rectangluar 
    /// region could be found.
    /// </para>
    /// <para>
    /// The <paramref name="clipMask"/> value determines how the <paramref name="maskColor"/> is used to determine the boundaries. If the <paramref name="clipMask"/> is set to 
    /// <see cref="ClipMask.Alpha"/>, then only the <see cref="GorgonColor.Alpha"/> component of the <paramref name="maskColor"/> is used, otherwise the <see cref="GorgonColor.Red"/>, 
    /// <see cref="GorgonColor.Green"/>, <see cref="GorgonColor.Blue"/> and <see cref="GorgonColor.Alpha"/> values are used.
    /// </para>
    /// <para>
    /// The <paramref name="imagePosition"/> must be in the coordinate space of the <see cref="ImageData"/>, that is it should be contained in a region of <c>0x0 - Image WidthxImage Height</c>. 
    /// If the position is not contained within this region, <b>null</b> will be returned signifying that no rectangular region could be found.
    /// </para>
    /// </remarks>
    public GorgonRectangleF? Pick(Vector2 imagePosition, GorgonColor maskColor, ClipMask clipMask = ClipMask.Alpha)
    {
        if (ImageData is null)
        {
            return null;
        }

        GorgonRectangleF imageBounds = new(0, 0, ImageData.Width, ImageData.Height);

        // If we clicked outside of the image, then there's nothing to click.
        if (!imageBounds.Contains(imagePosition.X, imagePosition.Y))
        {
            return null;
        }

        GorgonPoint imagePoint = (GorgonPoint)imagePosition;

        // We clicked on an area that has the masking value under our cursor, so we can't build anything.
        // In this case, do not change the current rectangle.
        if (IsMaskValue(imagePoint, clipMask, maskColor))
        {
            return null;
        }

        Queue<ClipSpan> spanQueue = new();
        GorgonRectangleF clipRegion = new()
        {
            Left = imagePoint.X,
            Top = imagePoint.Y,
            Right = imagePoint.X,
            Bottom = imagePoint.Y
        };

        Array.Clear(_pixels, 0, _pixels.Length);
        ClipSpan span = GetSpan(new GorgonPoint((int)clipRegion.Left, (int)clipRegion.Top), clipMask, maskColor);

        // If we have an empty span, then leave.
        if (((span.End - span.Start) + 1) <= 0)
        {
            return null;
        }

        spanQueue.Enqueue(span);

        while (spanQueue.Count > 0)
        {
            span = spanQueue.Dequeue();

            GorgonRectangle spanRegion = new()
            {
                Left = span.Start,
                Top = span.Y - 1,
                Right = span.End,
                Bottom = span.Y + 1
            };

            // Check each pixel between the start and end of the upper and lower spans.
            for (int x = spanRegion.Left; x <= spanRegion.Right; ++x)
            {
                int pixelindex = ImageData.Width * spanRegion.Top + x;
                GorgonPoint topSpan = new(x, spanRegion.Top);
                GorgonPoint bottomSpan = new(x, spanRegion.Bottom);
                ClipSpan vertSpan;

                if ((span.Y >= 0) && (!IsMaskValue(topSpan, clipMask, maskColor)) && (!_pixels[pixelindex]))
                {
                    vertSpan = GetSpan(topSpan, clipMask, maskColor);

                    if (((vertSpan.End - vertSpan.Start) + 1) <= 0)
                    {
                        return null;
                    }

                    spanQueue.Enqueue(vertSpan);
                }

                pixelindex = ImageData.Width * spanRegion.Bottom + x;

                if ((span.Y >= ImageData.Height - 1) || (IsMaskValue(bottomSpan, clipMask, maskColor)) || (_pixels[pixelindex]))
                {
                    continue;
                }

                vertSpan = GetSpan(bottomSpan, clipMask, maskColor);

                if (((vertSpan.End - vertSpan.Start) + 1) <= 0)
                {
                    return null;
                }

                spanQueue.Enqueue(vertSpan);
            }

            // Update the boundaries.
            clipRegion = GorgonRectangleF.FromLTRB(spanRegion.Left.Min((int)clipRegion.Left), (spanRegion.Top + 1).Min((int)clipRegion.Top), 
                                                   (spanRegion.Right + 1).Max((int)clipRegion.Right), spanRegion.Bottom.Max((int)clipRegion.Bottom));
        }

        clipRegion = GorgonRectangleF.Expand(clipRegion, Padding);

        if (clipRegion.Left < 0)
        {
            clipRegion.Left = 0;
        }

        if (clipRegion.Top < 0)
        {
            clipRegion.Top = 0;
        }

        if (clipRegion.Right > ImageData.Width)
        {
            clipRegion.Right = ImageData.Width;
        }

        if (clipRegion.Bottom > ImageData.Height)
        {
            clipRegion.Bottom = ImageData.Height;
        }

        // Ensure that we don't get a degenerate rectangle.
        return ((clipRegion.IsEmpty) || (clipRegion.Width < 0) || (clipRegion.Height < 0)) ? null : clipRegion;
    }

}
