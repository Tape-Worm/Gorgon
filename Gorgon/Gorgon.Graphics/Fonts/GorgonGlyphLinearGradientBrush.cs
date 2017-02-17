﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, October 12, 2013 10:08:08 PM
// 
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using DX = SharpDX;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A brush used to draw glyphs using a linear gradient value.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This allows glyphs to be drawn using multiple colors that fade into one another.
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonGlyphSolidBrush"/>
	/// <seealso cref="GorgonGlyphHatchBrush"/>
	/// <seealso cref="GorgonGlyphPathGradientBrush"/>
	/// <seealso cref="GorgonGlyphTextureBrush"/>
	public class GorgonGlyphLinearGradientBrush
		: GorgonGlyphBrush
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the region for the gradient.
		/// </summary>
		internal DX.Rectangle GradientRegion
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the type of brush.
		/// </summary>
		public override GlyphBrushType BrushType => GlyphBrushType.LinearGradient;

		/// <summary>
		/// Property to set or return the starting color to use for the gradient.
		/// </summary>
		public GorgonColor StartColor
		{
			get
			{
				// ReSharper disable once InvertIf
				if (Interpolation.Count == 0)
				{
					Interpolation.Add(new GorgonGlyphBrushInterpolator(0, GorgonColor.Black));
					Interpolation.Add(new GorgonGlyphBrushInterpolator(0, GorgonColor.White));
				}

				return Interpolation[0].Color;
			}
			set
			{
				var newValue = new GorgonGlyphBrushInterpolator(0, value);

				if (Interpolation.Count == 0)
				{
					Interpolation.Add(newValue);
					Interpolation.Add(new GorgonGlyphBrushInterpolator(1, GorgonColor.White));
				}
				else
				{
					Interpolation[0] = newValue;
				}
			}
		}

		/// <summary>
		/// Property to set or return the ending color to use for the gradient.
		/// </summary>
		public GorgonColor EndColor
		{
			get
			{
				if (Interpolation.Count == 0)
				{
					Interpolation.Add(new GorgonGlyphBrushInterpolator(0, GorgonColor.Black));
				}

				if (Interpolation.Count == 1)
				{
					Interpolation.Add(new GorgonGlyphBrushInterpolator(1, GorgonColor.White));
				}

				return Interpolation[Interpolation.Count - 1].Color;
			}
			set
			{
				var newValue = new GorgonGlyphBrushInterpolator(1, value);

				switch (Interpolation.Count)
				{
					case 0:
						Interpolation.Add(new GorgonGlyphBrushInterpolator(0, GorgonColor.Black));
						Interpolation.Add(newValue);
						break;
					case 1:
						Interpolation.Add(newValue);
						break;
					default:
						Interpolation[Interpolation.Count - 1] = newValue;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return the angle, in degrees, for the gradient.
		/// </summary>
		public float Angle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to scale the angle.
		/// </summary>
		public bool ScaleAngle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to enable or disable gamma correction.
		/// </summary>
		public bool GammaCorrection
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the interpolation colors for the gradient.
		/// </summary>
		public IList<GorgonGlyphBrushInterpolator> Interpolation
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert this brush to the equivalent GDI+ brush type.
		/// </summary>
		/// <returns>
		/// The GDI+ brush type for this object.
		/// </returns>
		internal override Brush ToGDIBrush()
		{
			var result = new LinearGradientBrush(new Rectangle(GradientRegion.X, GradientRegion.Y, GradientRegion.Width, GradientRegion.Height),
			                                     StartColor,
			                                     EndColor,
			                                     Angle,
			                                     ScaleAngle)
			             {
				             GammaCorrection = GammaCorrection
			             };

			var interpolationColors = new ColorBlend(Interpolation.Count);

			for (int i = 0; i < Interpolation.Count; i++)
			{
				interpolationColors.Colors[i] = Interpolation[i].Color;
				interpolationColors.Positions[i] = Interpolation[i].Weight;
			}

			result.InterpolationColors = interpolationColors;

			return result;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyphPathGradientBrush"/> class.
		/// </summary>
		public GorgonGlyphLinearGradientBrush()
		{
			Interpolation = new List<GorgonGlyphBrushInterpolator>
			                {
				                new GorgonGlyphBrushInterpolator(0, GorgonColor.Black),
				                new GorgonGlyphBrushInterpolator(1, GorgonColor.White)
			                };
		}
		#endregion
	}
}
