#region MIT.
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
// Created: Saturday, October 12, 2013 9:10:02 PM
// 
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DX = SharpDX;
using Gorgon.Math;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A brush that paints the font glyphs using a gradient that follows a specific path.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This allows glyphs to be drawn using multiple colors that fade into one another.
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonGlyphSolidBrush"/>
	/// <seealso cref="GorgonGlyphHatchBrush"/>
	/// <seealso cref="GorgonGlyphLinearGradientBrush"/>
	/// <seealso cref="GorgonGlyphTextureBrush"/>
	public class GorgonGlyphPathGradientBrush
		: GorgonGlyphBrush
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of brush.
		/// </summary>
		public override GlyphBrushType BrushType => GlyphBrushType.PathGradient;

		/// <summary>
		/// Property to set or return the wrapping mode for the gradient fill.
		/// </summary>
		public GlyphBrushWrapMode WrapMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the points for the path to follow in the gradient fill.
		/// </summary>
		public IList<DX.Vector2> Points
		{
			get;
		}

		/// <summary>
		/// Property to return the list of blending factors for the gradient falloff.
		/// </summary>
		public IList<float> BlendFactors
		{
			get;
		}

		/// <summary>
		/// Property to return the list of blending positions for the gradient falloff.
		/// </summary>
		public IList<float> BlendPositions
		{
			get;
		}

		/// <summary>
		/// Property to set or return the color of the center point in the gradient.
		/// </summary>
		public GorgonColor CenterColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the center point position in the gradient.
		/// </summary>
		public DX.Vector2 CenterPoint
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the focus point for the gradient falloff.
		/// </summary>
		public DX.Vector2 FocusScales
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the interpolation values for the gradient.
		/// </summary>
		public IList<GorgonGlyphBrushInterpolator> Interpolation
		{
			get;
		}

		/// <summary>
		/// Property to set or return the surrounding colors for the gradient path.
		/// </summary>
		public IList<GorgonColor> SurroundColors
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
			var result = new PathGradientBrush(Points.Select(item => new PointF(item.X, item.Y)).ToArray(), (WrapMode)WrapMode);

			var blend = new Blend(BlendFactors.Count.Max(BlendPositions.Count).Max(1));
			
			if (Interpolation.Count > 2)
			{
				var interpolationColors = new ColorBlend(Interpolation.Count);

				for (int i = 0; i < Interpolation.Count; i++)
				{
					interpolationColors.Colors[i] = Interpolation[i].Color;
					interpolationColors.Positions[i] = Interpolation[i].Weight;
				}

				result.InterpolationColors = interpolationColors;
			}

			for (int i = 0; i < blend.Factors.Length; i++)
			{
				if (i < BlendFactors.Count)
				{
					blend.Factors[i] = BlendFactors[i];
				}

				if (i < BlendPositions.Count)
				{
					blend.Positions[i] = BlendPositions[i];
				}
			}
			
			result.Blend = blend;
			result.CenterColor = CenterColor;
			result.CenterPoint = new PointF(CenterPoint.X, CenterPoint.Y);
			result.FocusScales = new PointF(FocusScales.X, FocusScales.Y);
			
			result.SurroundColors = SurroundColors.Select(item => item.ToColor()).ToArray();

			return result;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyphPathGradientBrush"/> class.
		/// </summary>
		public GorgonGlyphPathGradientBrush()
		{
			Points = new List<DX.Vector2>();
			BlendFactors = new List<float>();
			BlendPositions = new List<float>();
			Interpolation = new List<GorgonGlyphBrushInterpolator>();
			SurroundColors = new List<GorgonColor>();
		}
		#endregion
	}
}
