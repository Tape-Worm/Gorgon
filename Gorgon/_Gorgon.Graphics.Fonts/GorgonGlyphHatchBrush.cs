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
// Created: Saturday, October 12, 2013 11:22:36 PM
// 
#endregion

using System.Drawing;
using System.Drawing.Drawing2D;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// The patterns used to draw the glyphs.
	/// </summary>
	public enum GlyphBrushHatchStyle
	{
		/// <summary>
		/// 
		/// </summary>
		Horizontal = HatchStyle.Horizontal,

		/// <summary>
		/// 
		/// </summary>
		Min = HatchStyle.Min,

		/// <summary>
		/// 
		/// </summary>
		Vertical = HatchStyle.Vertical,

		/// <summary>
		/// 
		/// </summary>
		ForwardDiagonal = HatchStyle.ForwardDiagonal,

		/// <summary>
		/// 
		/// </summary>
		BackwardDiagonal = HatchStyle.BackwardDiagonal,

		/// <summary>
		/// 
		/// </summary>
		Cross = HatchStyle.Cross,

		/// <summary>
		/// 
		/// </summary>
		LargeGrid = HatchStyle.LargeGrid,

		/// <summary>
		/// 
		/// </summary>
		Max = HatchStyle.Max,

		/// <summary>
		/// 
		/// </summary>
		DiagonalCross = HatchStyle.DiagonalCross,

		/// <summary> 
		/// 
		/// </summary> 
		Percent05 = HatchStyle.Percent05,

		/// <summary> 
		/// 
		/// </summary> 
		Percent10 = HatchStyle.Percent10,

		/// <summary> 
		/// 
		/// </summary> 
		Percent20 = HatchStyle.Percent20,

		/// <summary> 
		/// 
		/// </summary> 
		Percent25 = HatchStyle.Percent25,

		/// <summary> 
		/// 
		/// </summary> 
		Percent30 = HatchStyle.Percent30,

		/// <summary> 
		/// 
		/// </summary> 
		Percent40 = HatchStyle.Percent40,

		/// <summary> 
		/// 
		/// </summary> 
		Percent50 = HatchStyle.Percent50,

		/// <summary> 
		/// 
		/// </summary> 
		Percent60 = HatchStyle.Percent60,

		/// <summary> 
		/// 
		/// </summary> 
		Percent70 = HatchStyle.Percent70,

		/// <summary> 
		/// 
		/// </summary> 
		Percent75 = HatchStyle.Percent75,

		/// <summary> 
		/// 
		/// </summary> 
		Percent80 = HatchStyle.Percent80,

		/// <summary> 
		/// 
		/// </summary> 
		Percent90 = HatchStyle.Percent90,

		/// <summary> 
		/// 
		/// </summary> 
		LightDownwardDiagonal = HatchStyle.LightDownwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		LightUpwardDiagonal = HatchStyle.LightUpwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		DarkDownwardDiagonal = HatchStyle.DarkDownwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		DarkUpwardDiagonal = HatchStyle.DarkUpwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		WideDownwardDiagonal = HatchStyle.WideDownwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		WideUpwardDiagonal = HatchStyle.WideUpwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		LightVertical = HatchStyle.LightVertical,

		/// <summary> 
		/// 
		/// </summary> 
		LightHorizontal = HatchStyle.LightHorizontal,

		/// <summary> 
		/// 
		/// </summary> 
		NarrowVertical = HatchStyle.NarrowVertical,

		/// <summary> 
		/// 
		/// </summary> 
		NarrowHorizontal = HatchStyle.NarrowHorizontal,

		/// <summary> 
		/// 
		/// </summary> 
		DarkVertical = HatchStyle.DarkVertical,

		/// <summary> 
		/// 
		/// </summary> 
		DarkHorizontal = HatchStyle.DarkHorizontal,

		/// <summary> 
		/// 
		/// </summary> 
		DashedDownwardDiagonal = HatchStyle.DashedDownwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		DashedUpwardDiagonal = HatchStyle.DashedUpwardDiagonal,

		/// <summary> 
		/// 
		/// </summary> 
		DashedHorizontal = HatchStyle.DashedHorizontal,

		/// <summary> 
		/// 
		/// </summary> 
		DashedVertical = HatchStyle.DashedVertical,

		/// <summary> 
		/// 
		/// </summary> 
		SmallConfetti = HatchStyle.SmallConfetti,

		/// <summary> 
		/// 
		/// </summary> 
		LargeConfetti = HatchStyle.LargeConfetti,

		/// <summary> 
		/// 
		/// </summary> 
		ZigZag = HatchStyle.ZigZag,

		/// <summary> 
		/// 
		/// </summary> 
		Wave = HatchStyle.Wave,

		/// <summary> 
		/// 
		/// </summary> 
		DiagonalBrick = HatchStyle.DiagonalBrick,

		/// <summary> 
		/// 
		/// </summary> 
		HorizontalBrick = HatchStyle.HorizontalBrick,

		/// <summary> 
		/// 
		/// </summary> 
		Weave = HatchStyle.Weave,

		/// <summary> 
		/// 
		/// </summary> 
		Plaid = HatchStyle.Plaid,

		/// <summary> 
		/// 
		/// </summary> 
		Divot = HatchStyle.Divot,

		/// <summary> 
		/// 
		/// </summary> 
		DottedGrid = HatchStyle.DottedGrid,

		/// <summary> 
		/// 
		/// </summary> 
		DottedDiamond = HatchStyle.DottedDiamond,

		/// <summary> 
		/// 
		/// </summary> 
		Shingle = HatchStyle.Shingle,

		/// <summary> 
		/// 
		/// </summary> 
		Trellis = HatchStyle.Trellis,

		/// <summary> 
		/// 
		/// </summary> 
		Sphere = HatchStyle.Sphere,

		/// <summary> 
		/// 
		/// </summary> 
		SmallGrid = HatchStyle.SmallGrid,

		/// <summary> 
		/// 
		/// </summary> 
		SmallCheckerBoard = HatchStyle.SmallCheckerBoard,

		/// <summary> 
		/// 
		/// </summary> 
		LargeCheckerBoard = HatchStyle.LargeCheckerBoard,

		/// <summary> 
		/// 
		/// </summary> 
		OutlinedDiamond = HatchStyle.OutlinedDiamond,

		/// <summary> 
		/// 
		/// </summary> 
		SolidDiamond = HatchStyle.SolidDiamond
	}

	/// <summary>
	/// A brush used to draw glyphs using a hatching patterns.
	/// </summary>
	/// <seealso cref="GorgonGlyphSolidBrush"/>
	/// <seealso cref="GorgonGlyphLinearGradientBrush"/>
	/// <seealso cref="GorgonGlyphPathGradientBrush"/>
	/// <seealso cref="GorgonGlyphTextureBrush"/>
	public class GorgonGlyphHatchBrush
		: GorgonGlyphBrush
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of brush.
		/// </summary>
		public override GlyphBrushType BrushType => GlyphBrushType.Hatched;

		/// <summary>
		/// Property to set or return the style to use for the hatching pattern.
		/// </summary>
		public GlyphBrushHatchStyle HatchStyle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the foreground color for the hatching pattern.
		/// </summary>
		public GorgonColor ForegroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the background color for the hatching pattern.
		/// </summary>
		public GorgonColor BackgroundColor
		{
			get;
			set;
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
			return new HatchBrush((HatchStyle)HatchStyle, ForegroundColor, BackgroundColor);
		}
		#endregion
	}
}
