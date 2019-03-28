#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 24, 2019 10:22:58 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A clipper that automatically defines the bounds of the sprite based on pixel data.
    /// </summary>
    internal class PickClipperService 
        : IPickClipperService
    {
        #region Value Types.
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
        #endregion

        #region Variables.
        // A list of pixels that have been checked by the clipper.
        private bool[] _pixels = Array.Empty<bool>();
        // The image data.
        private IGorgonImageBuffer _imageData;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the region that was clipped.
        /// </summary>
        public DX.RectangleF Rectangle
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the flag to determine which data to use as a mask when determining clip boundaries.
        /// </summary>
        public ClipMask ClipMask
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the masking value to determine which data to stop at when determining clip boundaries.
        /// </summary>
        /// <remarks>
        /// If the <see cref="ClipMask"/> value is set to <see cref="SpriteEditor.ClipMask.Alpha"/>, then only the <see cref="GorgonColor.Alpha"/> component of the value is used, otherwise the 
        /// <see cref="GorgonColor.Red"/>, <see cref="GorgonColor.Green"/>, <see cref="GorgonColor.Blue"/> and <see cref="GorgonColor.Alpha"/> values are used.
        /// </remarks>
        public GorgonColor ClipMaskValue
        {
            get;
            set;
        } = new GorgonColor(1, 0, 1, 0);

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

                if (value == null)
                {
                    _pixels = Array.Empty<bool>();
                    return;
                }

                _pixels = new bool[_imageData.Width * _imageData.Height];
            }
        }

        /// <summary>
        /// Property to set or return the function used to transform a point from window client space into local clip space.
        /// </summary>
        public Func<DX.Vector2, DX.Vector2> PointFromClient
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the pixel at the specified location is a value that falls within the mask.
        /// </summary>
        /// <param name="position">The position of the pixel.</param>
        /// <returns><b>true</b> if the pixel is a masked value, or <b>false</b> if not.</returns>
        private bool IsMaskValue(DX.Point position)
        {
            int color = ClipMask == ClipMask.Alpha ? (int)(ClipMaskValue.Alpha * 255) : ClipMaskValue.ToABGR();

            // The linear memory address.
            int location = (position.X * ImageData.FormatInformation.SizeInBytes) + (position.Y * ImageData.PitchInformation.RowPitch);

            // Grab the pixel at the location.
            int pixel = ImageData.Data.ReadAs<int>(location);

            // Images should be in RGBA 32 bit format.  If they're not, then we'll get bad data.
            if (ClipMask == ClipMask.Alpha)
            {
                pixel = ((pixel >> 24) & 0xff);
            }

            return pixel == color;
        }

        /// <summary>
        /// Function to record a span used to determine the size of the sprite extents.
        /// </summary>
        /// <param name="position">The pixel position to read.</param>
        /// <returns>A new span value.</returns>
        private ClipSpan GetSpan(DX.Point position)
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

                if ((left < 0) || (IsMaskValue(new DX.Point(left, position.Y))) || (_pixels[index]))
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

                if ((right >= ImageData.Width) || (IsMaskValue(new DX.Point(right, position.Y))) || (_pixels[index]))
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
        /// Function to handle the mouse up event.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse cursor, relative to the image.</param>
        /// <param name="mouseButton">The mouse button that was released.</param>
        /// <param name="modifierKeys">The modifier keys held down while the mouse button was released.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if not.</returns>
        public bool MouseUp(DX.Vector2 mousePosition, MouseButtons mouseButton, Keys modifierKeys)
        {
            if ((mouseButton != MouseButtons.Left) || (ImageData == null) || (modifierKeys != Keys.None))
            {
                return false;
            }

            var mouse = (PointFromClient == null ? mousePosition : PointFromClient(mousePosition)).ToPoint();
            var imageBounds = new DX.RectangleF(0, 0, ImageData.Width, ImageData.Height);

            // If we clicked outside of the image, then there's nothing to click.
            if (!imageBounds.Contains(mouse))
            {
                return false;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // We clicked on an area that has the masking value under our cursor, so we can't build anything.
                // In this case, do not change the current rectangle.
                if (IsMaskValue(mouse))
                {
                    return true;
                }

                var spanQueue = new Queue<ClipSpan>();
                var clipRegion = new DX.RectangleF
                {
                    Left = mouse.X,
                    Top = mouse.Y,
                    Right = mouse.X,
                    Bottom = mouse.Y
                };

                Array.Clear(_pixels, 0, _pixels.Length);
                ClipSpan span = GetSpan(new DX.Point((int)clipRegion.Left, (int)clipRegion.Top));

                // If we have an empty span, then leave.
                if (span.Start >= span.End)
                {
                    return true;
                }

                spanQueue.Enqueue(span);

                while (spanQueue.Count > 0)
                {
                    span = spanQueue.Dequeue();

                    var spanRegion = new DX.Rectangle
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
                        var topSpan = new DX.Point(x, spanRegion.Top);
                        var bottomSpan = new DX.Point(x, spanRegion.Bottom);
                        ClipSpan vertSpan;

                        if ((span.Y > 0) && (!IsMaskValue(topSpan)) && (!_pixels[pixelindex]))
                        {
                            vertSpan = GetSpan(topSpan);

                            if (vertSpan.Start >= vertSpan.End)
                            {
                                continue;
                            }

                            spanQueue.Enqueue(vertSpan);
                        }

                        pixelindex = ImageData.Width * spanRegion.Bottom + x;

                        if ((span.Y >= ImageData.Height - 1) || (IsMaskValue(bottomSpan)) || (_pixels[pixelindex]))
                        {
                            continue;
                        }

                        vertSpan = GetSpan(bottomSpan);

                        if (vertSpan.Start >= vertSpan.End)
                        {
                            continue;
                        }

                        spanQueue.Enqueue(vertSpan);
                    }

                    // Update the boundaries.
                    clipRegion.Left = spanRegion.Left.Min((int)clipRegion.Left);
                    clipRegion.Right = (spanRegion.Right + 1).Max((int)clipRegion.Right);
                    clipRegion.Top = (spanRegion.Top + 1).Min((int)clipRegion.Top);
                    clipRegion.Bottom = spanRegion.Bottom.Max((int)clipRegion.Bottom);
                }

                // Ensure that we don't get a degenerate rectangle.
                if ((clipRegion.IsEmpty) || (clipRegion.Width < 0) || (clipRegion.Height < 0))
                {
                    return true;
                }

                Rectangle = clipRegion;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            return true;
        }
        #endregion
    }
}
