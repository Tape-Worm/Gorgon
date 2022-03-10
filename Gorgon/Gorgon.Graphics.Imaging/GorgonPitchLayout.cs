#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Wednesday, December 9, 2015 8:53:48 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;

namespace Gorgon.Graphics.Imaging
{
    /// <summary>
    /// Information about the pitch layout for buffer data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pitch layout indicates how data is organized within in a buffer. For most buffer types, this is simply the size of the buffer from 0 to <c>n - 1</c>. But for other buffer types, such as images 
    /// then extra dimensions are used to define the layout of the data. 
    /// </para>
    /// <para>
    /// In a 2D image, there is a width and a height. Or for a depth (3D) image, there is a width, height and depth. The number of bytes between each dimension must be used to determine where to start 
    /// reading/writing data in the memory occupied by the image. 
    /// </para>
    /// <para>
    /// In many cases, the "width" of an image is not necessarily the number of pixels across, it may also include extra padding information for alignment purposes. This information is returned in the 
    /// <see cref="RowPitch"/> for the pitch layout. This value is typically the number of <c>pixels * bytes per pixel + padding bytes</c>. Like a 2D image, a depth (3D) image must use this information to 
    /// determine where to start reading/writing data in the memory occupied by the image.
    /// </para>
    /// <para>
    /// For depth (3D) images, there is a slice value. This indicates the total size of one element along the Z-axis and typically represents the <c>bytes in the width * height</c> of the image. The 
    /// <see cref="SlicePitch"/> returns the total size of a single slice. The slice is calculated by <c>bytes in the width * height * slice index (z element)</c>). For a 2D image, this value represents just 
    /// a single slice (i.e. <c>bytes in the width * height</c>).
    /// </para>
    /// </remarks>
    public readonly struct GorgonPitchLayout
        : IEquatable<GorgonPitchLayout>
    {
        #region Variables.
        /// <summary>
        /// The number of bytes per line of data.
        /// </summary>
        /// <remarks>
        /// In an image, this indicates the number of bytes necessary for one row of pixel data, for other buffer types, this would indicate the size of the buffer, in bytes.
        /// </remarks>
        public readonly int RowPitch;

        /// <summary>
        /// The number of bytes per slice of data.
        /// </summary>
        /// <remarks>
        /// For a 2D image, this value indicates the total size of the image, in bytes (typically <see cref="RowPitch"/> * height). In a depth image (3D), this indicates the size, in bytes, of a single slice 
        /// along the Z-axis of the image. For other buffer types, this will be the same as the <see cref="RowPitch"/> value.
        /// </remarks>
        public readonly int SlicePitch;

        /// <summary>
        /// The number of horizontal blocks in a compressed format.
        /// </summary>
        /// <remarks>
        /// In the block compressed formats (e.g. <see cref="BufferFormat.BC3_UNorm"/>), the dimensions of the images are broken up into 4x4 blocks. This value indicates the total number of 4x4 blocks across 
        /// the width of the image. For example, if an image has a width of 256 pixels, it would be 64 blocks in the horizontal direction.
        /// </remarks>
        public readonly int HorizontalBlockCount;

        /// <summary>
        /// The number of vertical blocks in a compressed format.
        /// </summary>
        /// <remarks>
        /// In the block compressed formats (e.g. <see cref="BufferFormat.BC3_UNorm"/>), the dimensions of the images are broken up into 4x4 blocks. This value indicates the total number of 4x4 blocks across 
        /// the height of the image. For example, if an image has a height of 256 pixels, it would be 64 blocks in the vertical direction.
        /// </remarks>
        public readonly int VerticalBlockCount;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to compare two instances for equality.
        /// </summary>
        /// <param name="other">The other instance to compare to this one.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(GorgonPitchLayout other) => Equals(this, other);

        /// <summary>
        /// Function to compare two instances for equality.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(GorgonPitchLayout left, GorgonPitchLayout right) => ((left.RowPitch == right.RowPitch) && (left.SlicePitch == right.SlicePitch)
                    && (left.HorizontalBlockCount == right.HorizontalBlockCount) && (left.VerticalBlockCount == right.VerticalBlockCount));

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj) => obj is GorgonPitchLayout pitchLayout ? pitchLayout.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => HashCode.Combine(RowPitch, SlicePitch, HorizontalBlockCount, VerticalBlockCount);

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => ((HorizontalBlockCount == 0) && (VerticalBlockCount == 0))
                       ? string.Format(Resources.GORIMG_TOSTR_FMTPITCH, RowPitch, SlicePitch)
                       : string.Format(Resources.GORIMG_TOSTR_FMTPITCH_COMPRESSED, RowPitch, SlicePitch, HorizontalBlockCount, VerticalBlockCount);

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not</returns>
        public static bool operator ==(GorgonPitchLayout left, GorgonPitchLayout right) => Equals(left, right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if they are.</returns>
        public static bool operator !=(GorgonPitchLayout left, GorgonPitchLayout right) => !Equals(left, right);
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPitchLayout" /> struct.
        /// </summary>
        /// <param name="rowPitch">The number of bytes for a row in an image, or the total bytes for other buffer types.</param>
        /// <param name="slicePitch">The number of bytes between each slice in a depth (3D) image, or, for other image types, this indicates the total size of the image in bytes.</param>
        /// <param name="horizontalBlockCount">[Optional] The number of horizontal blocks in a block compressed format.</param>
        /// <param name="verticalBlockCount">[Optional] The number of vertical blocks in a block compressed format.</param>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="horizontalBlockCount"/> is greater than 0, and the <paramref name="verticalBlockCount"/> is 0, or the <paramref name="verticalBlockCount"/> is greater than 0, and the <paramref name="horizontalBlockCount"/> is 0.</exception>
        /// <remarks>
        /// <para>
        /// For a 2D image, the <paramref name="slicePitch"/> indicates the total size of the image, in bytes (typically <paramref name="rowPitch"/> * height). In a depth image (3D), this indicates the size, in 
        /// bytes, of a single slice along the Z-axis of the image. For other buffer types, this will be the same as the <paramref name="rowPitch"/> parameter.
        /// </para>
        /// <para>
        /// If the format is a block compressed image (e.g. <see cref="BufferFormat.BC3_UNorm"/>), then both the <paramref name="horizontalBlockCount"/>, and the <paramref name="verticalBlockCount"/> must be 
        /// larger than 0. Otherwise an exception will be thrown.
        /// </para>
        /// </remarks>
        public GorgonPitchLayout(int rowPitch, int slicePitch, int horizontalBlockCount = 0, int verticalBlockCount = 0)
        {
            RowPitch = rowPitch;
            SlicePitch = slicePitch;

            if ((verticalBlockCount <= 0) && (horizontalBlockCount > 0))
            {
                throw new ArgumentException(Resources.GORIMG_ERR_MISSING_BLOCK_COUNT, nameof(verticalBlockCount));
            }

            if ((horizontalBlockCount <= 0) && (verticalBlockCount > 0))
            {
                throw new ArgumentException(Resources.GORIMG_ERR_MISSING_BLOCK_COUNT, nameof(horizontalBlockCount));
            }

            HorizontalBlockCount = horizontalBlockCount;
            VerticalBlockCount = verticalBlockCount;
        }
        #endregion
    }
}