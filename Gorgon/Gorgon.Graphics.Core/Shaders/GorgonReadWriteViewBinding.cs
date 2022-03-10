#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 30, 2018 10:25:39 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A binding for a <see cref="GorgonReadWriteView"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows a <see cref="GorgonReadWriteView"/> to be bound to the GPU pipeline for access by a <see cref="GorgonPixelShader"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonReadWriteView"/>
    /// <seealso cref="GorgonPixelShader"/>
    public readonly struct GorgonReadWriteViewBinding
        : IGorgonEquatableByRef<GorgonReadWriteViewBinding>
    {
        #region Variables.
        // A structured buffer unordered access view for quick access.
        private readonly GorgonStructuredReadWriteView _structuredUav;

        /// <summary>
        /// An empty UAV binding.
        /// </summary>
        public static readonly GorgonReadWriteViewBinding Empty = new(null);

        /// <summary>
        /// An initial count for a <see cref="GorgonStructuredReadWriteView"/>.
        /// </summary>
        /// <remarks> 
        /// This provides an offset for an append/consume buffer. Setting this value to -1 will indicate that the current offset should be kept. This only applies to an unordered access view that was 
        /// created with the <see cref="StructuredBufferReadWriteViewType.Append"/> flag set.
        /// </remarks>
        public readonly int InitialCount;

        /// <summary>
        /// The unordered access view to bind.
        /// </summary>
        public readonly GorgonReadWriteView ReadWriteView;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of UAV if the UAV is for a structured buffer.
        /// </summary>
        public StructuredBufferReadWriteViewType ReadWriteViewType => _structuredUav?.ReadWriteViewType ?? StructuredBufferReadWriteViewType.None;
        #endregion

        #region Methods.
        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString() => string.Format(Resources.GORGFX_TOSTR_UAV_BINDING, InitialCount, (ReadWriteView is null ? "(NULL)" : ReadWriteView.Resource.Name));

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode() => HashCode.Combine(InitialCount, ReadWriteView);

        /// <summary>
        /// Function to determine if two instances are equal.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(in GorgonReadWriteViewBinding left, in GorgonReadWriteViewBinding right) => (left.InitialCount == right.InitialCount) && (left.ReadWriteView == right.ReadWriteView);

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance. </param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
        public override bool Equals(object obj) => obj is GorgonReadWriteViewBinding uav ? uav.Equals(in this) : base.Equals(obj);

        /// <summary>
        /// Function to compare this instance with another.
        /// </summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(in GorgonReadWriteViewBinding other) => Equals(in this, in other);

        /// <summary>
        /// Function to compare this instance with another.
        /// </summary>
        /// <param name="other">The other instance to use for comparison.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(GorgonReadWriteViewBinding other) => Equals(in this, in other);

        /// <summary>
        /// Operator to determine equality between two instances.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(in GorgonReadWriteViewBinding left, in GorgonReadWriteViewBinding right) => Equals(in left, in right);

        /// <summary>
        /// Operator to determine inequality between two instances.
        /// </summary>
        /// <param name="left">The left instance to compare.</param>
        /// <param name="right">The right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(in GorgonReadWriteViewBinding left, in GorgonReadWriteViewBinding right) => !Equals(in left, in right);
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonReadWriteViewBinding"/> struct.
        /// </summary>
        /// <param name="readWriteView">The read/write view.</param>
        public GorgonReadWriteViewBinding(GorgonReadWriteView readWriteView)
        {
            ReadWriteView = readWriteView;
            InitialCount = -1;
            _structuredUav = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonReadWriteViewBinding"/> struct.
        /// </summary>
        /// <param name="readWriteView">The read/write view.</param>
        /// <param name="initialCount">The inital count.</param>
        public GorgonReadWriteViewBinding(GorgonStructuredReadWriteView readWriteView, int initialCount)
        {
            ReadWriteView = readWriteView;
            _structuredUav = readWriteView;
            InitialCount = initialCount;
        }
        #endregion
    }
}
