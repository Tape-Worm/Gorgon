#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Sunday, October 16, 2011 2:10:22 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Values to define the number and quality of multisampling.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Setting the <see cref="Count"/> and <see cref="Quality"/> values to 1 and 0 respectively, will disable multisampling.
	/// </para>
	/// <para>
	/// If multisample anti-aliasing is being used, all bound render targets and depth buffers must have the same sample counts and quality levels.
	/// </para>
	/// </remarks>
	public readonly struct GorgonMultisampleInfo
		: IEquatable<GorgonMultisampleInfo>
	{
		#region Variables.
        /// <summary>
        /// The default multisampling value.
        /// </summary>
        public static readonly GorgonMultisampleInfo NoMultiSampling = new GorgonMultisampleInfo(1, 0);

		/// <summary>
		/// A quality level for standard multisample quality.
		/// </summary>
		/// <remarks>
		/// This value is always supported in Gorgon because it can only use Direct 3D 10 or better devices, and these devices are required to implement this pattern.
		/// </remarks>
		public static readonly int StandardMultisamplePatternQuality = unchecked((int)0xffffffff);

		/// <summary>
		/// A pattern where all of the samples are located at the pixel center.
		/// </summary>
		public static readonly int CenteredMultisamplePatternQuality = unchecked((int)0xfffffffe);

        /// <summary>
        /// The number of samples per pixel.
        /// </summary>
		public readonly int Count;

		/// <summary>
		/// The quality for a sample.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There is a performance penalty for setting this value to higher levels.
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// This value must be 0 or less than/equal to the quality returned by <see cref="IGorgonFormatSupportInfo.MaxMultisampleCountQuality"/>.  Failure to do so will cause an exception for objects that use this 
		/// type.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public readonly int Quality;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if two instances are equal.
        /// </summary>
        /// <param name="left">Left value to compare.</param>
        /// <param name="right">Right value to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(GorgonMultisampleInfo left, GorgonMultisampleInfo right) => left.Count == right.Count && left.Quality == right.Quality;

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
		{
		    if (obj is GorgonMultisampleInfo multiSample)
		    {
		        return multiSample.Equals(this);
		    }

		    return base.Equals(obj);
		}

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool operator ==(GorgonMultisampleInfo left, GorgonMultisampleInfo right) => Equals(left, right);

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns></returns>
        public static bool operator !=(GorgonMultisampleInfo left, GorgonMultisampleInfo right) => !Equals(left, right);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => 281.GenerateHash(Count).GenerateHash(Quality);

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format(Resources.GORGFX_TOSTR_MULTISAMPLEINFO, Count, Quality);

        /// <summary>
        /// Function to determine if two instances are equal.
        /// </summary>
        /// <param name="other">Other instance for the equality test.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public bool Equals(GorgonMultisampleInfo other) => Equals(this, other);
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonMultisampleInfo"/> struct.
        /// </summary>
        /// <param name="sampleDesc">The DXGI sample description.</param>
        internal GorgonMultisampleInfo(SampleDescription sampleDesc)
		{
			Count = sampleDesc.Count.Max(1).Min(D3D11.Device.MultisampleCountMaximum);
			Quality = sampleDesc.Quality;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMultisampleInfo"/> struct.
		/// </summary>
		/// <param name="count">The number of samples per pixel.</param>
		/// <param name="quality">Image quality.</param>
		public GorgonMultisampleInfo(int count, int quality)
		{
		    Count = count.Min(D3D11.Device.MultisampleCountMaximum).Max(1);
			Quality = quality;
		}
		#endregion
	}
}
