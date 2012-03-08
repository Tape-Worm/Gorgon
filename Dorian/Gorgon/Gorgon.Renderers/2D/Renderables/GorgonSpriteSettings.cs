#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, March 07, 2012 6:07:13 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// Settings for a sprite object.
	/// </summary>
	public struct GorgonSpriteSettings
	{
		#region Variables.
		/// <summary>
		/// Initial anchor for pivoting the sprite.
		/// </summary>
		public Vector2 Anchor;
		/// <summary>
		/// Initial color of the sprite.
		/// </summary>
		public GorgonColor Color;
		/// <summary>
		/// Initial size of the sprite.
		/// </summary>
		public Vector2 Size;
		/// <summary>
		/// Texture to apply to the sprite.
		/// </summary>
		public GorgonTexture2D Texture;
		/// <summary>
		/// Texture region to map to the sprite.
		/// </summary>
		public RectangleF TextureRegion;
		/// <summary>
		/// Initial position of the sprite.
		/// </summary>
		public Vector2 InitialPosition;
		/// <summary>
		/// Initial scale of the sprite.
		/// </summary>
		public Vector2 InitialScale;
		/// <summary>
		/// Initial angle of the sprite.
		/// </summary>
		public float InitialAngle;
		#endregion
	}
}
