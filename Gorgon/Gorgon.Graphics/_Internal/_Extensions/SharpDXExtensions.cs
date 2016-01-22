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
// Created: Monday, December 14, 2015 8:41:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Math;
using SharpDX.Mathematics.Interop;
using DXGI = SharpDX.DXGI;
using D3DCommon = SharpDX.Direct3D;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Extension methods for SharpDX object conversion.
	/// </summary>
	static class SharpDXExtensions
	{
		/// <summary>
		/// Function to convert a sharp dx raw color 4 type to a GorgonColor.
		/// </summary>
		/// <param name="color">The color type to convert.</param>
		/// <returns>The new color type.</returns>
		public static GorgonColor ToGorgonColor(this RawColor4 color)
		{
			return new GorgonColor(color.R, color.G, color.B, color.A);	
		}

		/// <summary>
		/// Function to convert a GorgonColor to a sharp dx raw color 4 type.
		/// </summary>
		/// <param name="color">The color type to convert.</param>
		/// <returns>The new color type.</returns>
		public static RawColor4 ToRawColor4(this GorgonColor color)
		{
			return new RawColor4(color.Red, color.Green, color.Blue, color.Alpha);
		}

		/// <summary>
		/// Function to convert a Gorgon multi-sampling value to a D3D sample description.
		/// </summary>
		/// <param name="sampleInfo">The multi-sample info to convert.</param>
		/// <returns>The D3D sample description.</returns>
		public static DXGI.SampleDescription Convert(this GorgonMultiSampleInfo sampleInfo)
		{
			return new DXGI.SampleDescription(sampleInfo.Count, sampleInfo.Quality);
		}


		/// <summary>
		/// Function to convert a DXGI rational number to a Gorgon rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>A Gorgon rational number.</returns>
		public static GorgonRationalNumber FromRational(this DXGI.Rational rational)
		{
			return new GorgonRationalNumber(rational.Numerator, rational.Denominator);
		}

		/// <summary>
		/// Function to convert a Gorgon rational number to a DXGI rational number.
		/// </summary>
		/// <param name="rational">The rational number to convert.</param>
		/// <returns>The DXGI ration number.</returns>
		public static DXGI.Rational ToRational(this GorgonRationalNumber rational)
		{
			return new DXGI.Rational(rational.Numerator, rational.Denominator);
		}

		/// <summary>
		/// Function to convert a GorgonVideoMode to a DXGI mode description.
		/// </summary>
		/// <param name="mode">The video mode to convert.</param>
		/// <returns>The DXGI mode description.</returns>
		public static DXGI.ModeDescription ToModeDesc(this GorgonVideoMode mode)
		{
			return new DXGI.ModeDescription
			       {
				       Format = (DXGI.Format)mode.Format,
				       RefreshRate = mode.RefreshRate.ToRational(),
				       Scaling = (DXGI.DisplayModeScaling)mode.Scaling,
				       Width = mode.Width,
				       Height = mode.Height,
				       ScanlineOrdering = (DXGI.DisplayModeScanlineOrder)mode.ScanlineOrdering
			       };
		}
	}
}
