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
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Values to define the number and quality of multisampling.
	/// </summary>
	/// <remarks>Setting the <see cref="GorgonLibrary.Graphics.GorgonMultiSampling.Count">count</see> and <see cref="GorgonLibrary.Graphics.GorgonMultiSampling.Quality">quality</see> values to 1 and 0 respectively, will disable multisampling.</remarks>
	public struct GorgonMultiSampling
	{
		#region Variables.
		private int _count;

		/// <summary>
		/// Image quality.
		/// </summary>
		/// <remarks>There is a performance penalty for setting this value to higher levels.
		/// <para>This value must be 0 or less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.  Failure to do so will cause the anything using the value to throw an exception.</para>
		/// </remarks>
		public int Quality;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the number of samples per pixel.
		/// </summary>
		/// <remarks>This value is limited to a range of 1 to 32.</remarks>
		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				if (value < 1)
					value = 1;
				if (value > 32)
					value = 32;

				_count = value;
			}
		}
		#endregion

		#region Methods.
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
			if (count < 1)
				count = 1;
			if (count > 32)
				count = 32;

			_count = count;
			Quality = quality;
		}
		#endregion
	}
}
