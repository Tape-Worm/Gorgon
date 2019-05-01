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
// Created: July 5, 2017 12:10:23 AM
// 
#endregion

using System;
using Gorgon.Core;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A key used to uniquely identify a shader view.
    /// </summary>
    internal readonly struct TextureViewKey
        : IEquatable<TextureViewKey>
    {
        /// <summary>
        /// The starting element.
        /// </summary>
        public readonly BufferFormat Format;
        /// <summary>
        /// The encoded range of mip maps to view.
        /// </summary>
        public readonly uint MipRange;
        /// <summary>
        /// The encoded range of array indices to view (for 1D/2D).
        /// </summary>
        public readonly uint ArrayRange;

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj is TextureViewKey key ? key.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => 281.GenerateHash(Format).GenerateHash(MipRange).GenerateHash(ArrayRange);

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureViewKey"/> struct.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <param name="mipStart">The starting mip map to view.</param>
        /// <param name="mipCount">The number of mip maps to view.</param>
        /// <param name="arrayStart">The starting array index to view.</param>
        /// <param name="arrayCount">The number of array indices to view.</param>
        public TextureViewKey(BufferFormat format, int mipStart, int mipCount, int arrayStart, int arrayCount)
        {
            Format = format;
            MipRange = (((uint)mipCount & 0xffff) << 16) | ((uint)mipStart & 0xffff);
            ArrayRange = (((uint)arrayStart & 0xffff) << 16) | ((uint)arrayCount & 0xffff);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(TextureViewKey other) => Format == other.Format && MipRange == other.MipRange && ArrayRange == other.ArrayRange;
    }
}
