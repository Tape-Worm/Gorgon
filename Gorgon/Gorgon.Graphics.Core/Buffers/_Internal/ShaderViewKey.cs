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
    internal readonly struct BufferShaderViewKey
        : IEquatable<BufferShaderViewKey>
    {
        /// <summary>
        /// The starting element.
        /// </summary>
        public readonly int Start;
        /// <summary>
        /// The number of elements.
        /// </summary>
        public readonly int Count;
        /// <summary>
        /// The type of data or size of data.
        /// </summary>
        public readonly int DataType;

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => obj is BufferShaderViewKey key ? key.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => HashCode.Combine(Start, Count, DataType);

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferShaderViewKey"/> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <param name="format">The format.</param>
        public BufferShaderViewKey(int start, int count, BufferFormat format)
            : this(start, count, (int)format)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferShaderViewKey"/> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <param name="elementType">Type of the element.</param>
        public BufferShaderViewKey(int start, int count, RawBufferElementType elementType)
            : this(start, count, (int)elementType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferShaderViewKey"/> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="count">The count.</param>
        /// <param name="dataType">Type of the data.</param>
        public BufferShaderViewKey(int start, int count, int dataType)
        {
            Start = start;
            Count = count;
            DataType = dataType;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(BufferShaderViewKey other) => Start == other.Start && Count == other.Count && DataType == other.DataType;
    }
}
