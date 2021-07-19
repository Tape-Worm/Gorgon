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
using System.Numerics;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines the offsets for each corner of a rectangle.
    /// </summary>
    public class GorgonRectangleOffsets
        : IReadOnlyList<Vector3>
    {
        #region Variables.
        // The renderable object to update.
        private readonly BatchRenderable _renderable;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the corner offset value by index.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is not between 0 and 3.</exception>
        /// <remarks>
        /// The ordering of the indices is as follows: 0 - Upper left, 1 - Upper right, 2 - Lower right, 3 - Lower left.
        /// </remarks>
        public Vector3 this[int index]
        {
            get => index switch
            {
                0 => _renderable.UpperLeftOffset,
                1 => _renderable.UpperRightOffset,
                2 => _renderable.LowerRightOffset,
                3 => _renderable.LowerLeftOffset,
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
        /// Property to set or return the offset of the upper left corner.
        /// </summary>
        public Vector3 UpperLeft
        {
            get => _renderable.UpperLeftOffset;
            set
            {
                if (_renderable.UpperLeftOffset == value)
                {
                    return;
                }

                _renderable.UpperLeftOffset = value;
                _renderable.HasTextureChanges = _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the offset of the upper right corner.
        /// </summary>
        public Vector3 UpperRight
        {
            get => _renderable.UpperRightOffset;
            set
            {
                if (_renderable.UpperRightOffset == value)
                {
                    return;
                }

                _renderable.UpperRightOffset = value;
                _renderable.HasTextureChanges = _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the offset of the lower left corner.
        /// </summary>
        public Vector3 LowerLeft
        {
            get => _renderable.LowerLeftOffset;
            set
            {
                if (_renderable.LowerLeftOffset == value)
                {
                    return;
                }

                _renderable.LowerLeftOffset = value;
                _renderable.HasTextureChanges = _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>
        /// Property to set or return the offset of the lower right corner.
        /// </summary>
        public Vector3 LowerRight
        {
            get => _renderable.LowerRightOffset;
            set
            {
                if (_renderable.LowerRightOffset == value)
                {
                    return;
                }

                _renderable.LowerRightOffset = value;
                _renderable.HasTextureChanges = _renderable.HasTransformChanges = true;
            }
        }

        /// <summary>Gets the number of elements in the collection.</summary>
        public int Count => 4;

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Vector3> GetEnumerator()
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

        #region Methods.
        /// <summary>
        /// Function to assign a single offset to all corners.
        /// </summary>
        /// <param name="offset">The offset to assign.</param>
        public void SetAll(Vector3 offset)
        {
            if ((offset == _renderable.LowerLeftOffset)
                && (offset == _renderable.LowerRightOffset)
                && (offset == _renderable.UpperRightOffset)
                && (offset == _renderable.UpperLeftOffset))
            {
                return;
            }

            _renderable.LowerLeftOffset = _renderable.LowerRightOffset = _renderable.UpperRightOffset = _renderable.UpperLeftOffset;
            _renderable.HasTextureChanges = _renderable.HasTransformChanges = true;
        }

        /// <summary>
        /// Function to copy the offsets into the specified destination.
        /// </summary>
        /// <param name="destination">The destination that will receive the copy of the offsets.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/> parameter is <b>null</b>.</exception>
        public void CopyTo(GorgonRectangleOffsets destination)
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
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRectangleOffsets"/> class.
        /// </summary>
        /// <param name="renderable">The renderable to update.</param>
        internal GorgonRectangleOffsets(BatchRenderable renderable) => _renderable = renderable;
        #endregion
    }
}
