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
using Gorgon.Graphics.Properties;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Values to define the number and quality of multisampling.
	/// </summary>
	/// <remarks>Setting the <see cref="Gorgon.Graphics.GorgonMultisampling.Count">count</see> and <see cref="Gorgon.Graphics.GorgonMultisampling.Quality">quality</see> values to 1 and 0 respectively, will disable multisampling.</remarks>
	public struct GorgonMultisampling
		: IEquatable<GorgonMultisampling>
	{
		#region Variables.
        /// <summary>
        /// The default multisampling value.
        /// </summary>
        public static readonly GorgonMultisampling NoMultiSampling = new GorgonMultisampling(1, 0);

        /// <summary>
        /// The number of samples per pixel.
        /// </summary>
		public readonly int Count;

		/// <summary>
		/// Image quality.
		/// </summary>
		/// <remarks>There is a performance penalty for setting this value to higher levels.
		/// <para>This value must be 0 or less than the value returned by <see cref="Gorgon.Graphics.VideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see>.  Failure to do so will cause the anything using the value to throw an exception.</para>
		/// </remarks>
		public readonly int Quality;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert a Gorgon multisampling value to a D3D sample description.
		/// </summary>
		/// <param name="sampling">Sampling value to convert.</param>
		/// <returns>The D3D sample description.</returns>
		internal static GI.SampleDescription Convert(GorgonMultisampling sampling)
		{
			return new GI.SampleDescription(sampling.Count, sampling.Quality);
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left value to compare.</param>
		/// <param name="right">Right value to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonMultisampling left, ref GorgonMultisampling right)
		{
			return left.Count == right.Count && left.Quality == right.Quality;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
		    if (obj is GorgonMultisampling)
		    {
		        return Equals((GorgonMultisampling)obj);
		    }

		    return base.Equals(obj);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static bool operator ==(GorgonMultisampling left, GorgonMultisampling right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns></returns>
		public static bool operator !=(GorgonMultisampling left, GorgonMultisampling right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Count).GenerateHash(Quality);
		}

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
		    return string.Format(Resources.GORGFX_TOSTR_MULTISAMPLEINFO, Count, Quality);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMultisampling"/> struct.
		/// </summary>
		/// <param name="count">The number of multisamples per pixel.</param>
		/// <param name="quality">Image quality.</param>
		public GorgonMultisampling(int count, int quality)
		{
		    if (count < 1)
		    {
		        count = 1;
		    }
		    if (count > 32)
		    {
		        count = 32;
		    }

		    Count = count;
			Quality = quality;
		}
		#endregion

		#region IEquatable<GorgonMultisampling> Members
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="other">Other instance for the equality test.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool Equals(GorgonMultisampling other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
