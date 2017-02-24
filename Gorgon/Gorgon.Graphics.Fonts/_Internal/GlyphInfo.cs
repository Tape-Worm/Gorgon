﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 15, 2017 9:07:03 PM
// 
#endregion
using Gorgon.Graphics.Core;
using Drawing = System.Drawing;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// Information about a glyph.
	/// </summary>
	class GlyphInfo
	{
		#region Properties.
		/// <summary>
		/// Property to return the packed bitmap associated with the glyph.
		/// </summary>
		public Drawing.Bitmap GlyphBitmap
		{
			get;
		}

		/// <summary>
		/// Property to return the region for the glyph.
		/// </summary>
		public DX.Rectangle Region
		{
			get;
		}

		/// <summary>
		/// Property to return the region for the glyph outline (if applicable).
		/// </summary>
		public DX.Rectangle OutlineRegion
		{
			get;
		}

		/// <summary>
		/// Property to return the offset used to adjust the glyph when rendering an outline.
		/// </summary>
		public DX.Point OutlineOffset
		{
			get;
		}

		/// <summary>
		/// Property to return the offset used to adjust the glyph when rendering a string.
		/// </summary>
		public DX.Point Offset
		{
			get;
		}

		/// <summary>
		/// Property to set or return the texture assigned to the glyph.
		/// </summary>
		public GorgonTexture Texture
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the texture array index assigned to the glyph.
		/// </summary>
		public int TextureArrayIndex
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GlyphInfo"/> class.
		/// </summary>
		/// <param name="glyphBitmap">The glyph bitmap.</param>
		/// <param name="region">The region.</param>
		/// <param name="offset">The offset.</param>
		/// <param name="outlineRegion">The outline region.</param>
		/// <param name="outlineOffset">The outline offset.</param>
		public GlyphInfo(Drawing.Bitmap glyphBitmap, DX.Rectangle region, DX.Point offset, DX.Rectangle outlineRegion, DX.Point outlineOffset)
		{
			GlyphBitmap = glyphBitmap;
			Region = region;
			Offset = offset;
			OutlineRegion = outlineRegion;
			OutlineOffset = outlineOffset;
		}
		#endregion
	}
}
