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
// Created: Sunday, June 14, 2015 5:59:28 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// A chunk for the chunked file format.
    /// </summary>
    public readonly struct GorgonChunk
        : IGorgonEquatableByRef<GorgonChunk>
    {
        #region Variables.		
        /// <summary>
        /// An empty chunk.
        /// </summary>
        public static readonly GorgonChunk EmptyChunk = default;

        /// <summary>
        /// The ID for the chunk.
        /// </summary>
        public readonly ulong ID;

        /// <summary>
        /// The size of the chunk, in bytes.
        /// </summary>
        public readonly int Size;

        /// <summary>
        /// The offset, in bytes, of the chunk within the chunked file.
        /// </summary>
        /// <remarks>This is relative to the header of the file.</remarks>
        public readonly ulong FileOffset;
        #endregion

        #region Methods.
        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GOR_TOSTR_GORGONCHUNK, ID.FormatHex(), FileOffset.FormatHex(), Size.FormatHex());

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => 281.GenerateHash(ID);

        /// <summary>
        /// Function to compare two instances for equality.
        /// </summary>
        /// <param name="left">The first object of type <see cref="GorgonChunk"/> to compare.</param>
        /// <param name="right">The second object of type <see cref="GorgonChunk"/> to compare.</param>
        /// <returns><b>true</b> if the specified objects are equal; otherwise, <b>false</b> if not.</returns>
        public static bool Equals(in GorgonChunk left, in GorgonChunk right) => left.ID == right.ID;

        /// <summary>
        /// Function to compare two instances for equality.
        /// </summary>
        /// <param name="other">The object of type <see cref="GorgonChunk"/> to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> otherwise.</returns>
        public bool Equals(GorgonChunk other) => Equals(in this, in other);

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.</returns>
        public override bool Equals(object obj) => obj is GorgonChunk chunk ? chunk.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Function to compare this instance with another.
        /// </summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(in GorgonChunk other) => Equals(in this, in other);
        #endregion

        #region Operators.
        /// <summary>
        /// Operator used to compare two instances for equality.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> otherwise.</returns>
        public static bool operator ==(in GorgonChunk left, in GorgonChunk right) => Equals(in left, in right);

        /// <summary>
        /// Operator used to compare two instances for inequality.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> otherwise.</returns>
        public static bool operator !=(in GorgonChunk left, in GorgonChunk right) => !Equals(in left, in right);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonChunk"/> struct.
        /// </summary>
        /// <param name="id">The identifier for the chunk.</param>
        /// <param name="size">The size of the chunk, in bytes.</param>
        /// <param name="offset">The offset within the file, in bytes.</param>
        public GorgonChunk(ulong id, int size, ulong offset)
        {
            ID = id;
            Size = size;
            FileOffset = offset;
        }
        #endregion
    }
}
