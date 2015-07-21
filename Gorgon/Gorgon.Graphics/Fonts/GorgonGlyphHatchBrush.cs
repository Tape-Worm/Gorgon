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

namespace Gorgon.Graphics
{
	/// <summary>
	/// A brush used to draw glyphs using a hatching patterns.
	/// </summary>
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
		public HatchStyle HatchStyle
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
			return new HatchBrush(HatchStyle, ForegroundColor, BackgroundColor);
		}
		#endregion
	}
}
