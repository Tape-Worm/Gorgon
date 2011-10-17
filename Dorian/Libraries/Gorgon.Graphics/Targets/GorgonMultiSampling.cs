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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Predefined sampling quality for multisampling.
	/// </summary>
	public enum PredefinedSamplingQuality
	{
		/// <summary>
		/// Standard multisample pattern required by DX 11 and 10.1 video devices.
		/// </summary>
		StandardPattern = 0,
		/// <summary>
		/// Samples will be located at the pixel center.
		/// </summary>
		CenteredPattern = 1
	}

	/// <summary>
	/// Values to define the number and quality of multisampling.
	/// </summary>
	/// <remarks>Setting the <see cref="GorgonLibrary.Graphics.GorgonMultiSampling.Count">count</see> and <see cref="GorgonLibrary.Graphics.GorgonMultiSampling.Quality">quality</see> values to 1 and 0 respectively, will disable multisampling.</remarks>
	public struct GorgonMultiSampling
	{
		#region Variables.
		/// <summary>
		/// Number of multisamples per pixel.
		/// </summary>
		public int Count;
		/// <summary>
		/// Image quality.
		/// </summary>
		/// <remarks>There is a performance penalty for setting this value to higher levels.</remarks>
		public int Quality;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert Gorgon predefined sampling quality values to D3D standard multisample quality values.
		/// </summary>
		/// <param name="quality">Quality value to convert.</param>
		/// <returns>The D3D standard multisample quality value.</returns>
		internal static int Convert(PredefinedSamplingQuality quality)
		{
			// These values are no defined in SlimDX, so they may not be correct if the API changes.
			if (quality == PredefinedSamplingQuality.StandardPattern)
				return -1;
			else
				return -2;
		}

		/// <summary>
		/// Function to convert a Gorgon multisampling value to a D3D sample description.
		/// </summary>
		/// <param name="sampling">Sampling value to convert.</param>
		/// <returns>The D3D sample description.</returns>
		internal static GI.SampleDescription Convert(GorgonMultiSampling sampling)
		{
			return new GI.SampleDescription(sampling.Count, sampling.Quality);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonMultiSampling"/> struct.
		/// </summary>
		/// <param name="count">The number of multisamples per pixel.</param>
		/// <param name="quality">Image quality.</param>
		public GorgonMultiSampling(int count, int quality)
		{
			Count = count;
			Quality = quality;
		}
		#endregion
	}
}
