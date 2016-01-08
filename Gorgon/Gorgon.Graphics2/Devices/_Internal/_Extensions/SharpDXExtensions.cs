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
using System.Text;
using System.Threading.Tasks;
using Gorgon.Math;
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
		public static DXGI.ModeDescription1 ToModeDesc(this GorgonVideoMode mode)
		{
			return new DXGI.ModeDescription1
			       {
				       Format = (DXGI.Format)mode.Format,
				       RefreshRate = mode.RefreshRate.ToRational(),
				       Scaling = (DXGI.DisplayModeScaling)mode.Scaling,
				       Width = mode.Width,
				       Height = mode.Height,
				       ScanlineOrdering = (DXGI.DisplayModeScanlineOrder)mode.ScanlineOrdering,
				       Stereo = mode.Stereo
			       };
		}
	}
}
