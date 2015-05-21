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

using System.Drawing;
using Gorgon.Graphics;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// Settings for a sprite object.
	/// </summary>
	public class GorgonSpriteSettings
    {
        #region Variables.

	    private Vector2 _size = Vector2.Zero;
        #endregion

        #region Properties.
        /// <summary>
		/// Property to set or return the initial anchor for pivoting the sprite.
		/// </summary>
        public Vector2 Anchor
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the initial color of the sprite.
		/// </summary>
        /// <remarks>The default value is White (ARGB = 1,1,1,1).</remarks>
        public GorgonColor Color
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the initial size of the sprite.
		/// </summary>
        public Vector2 Size
        {
            get
            {
                return _size;
            }
            set
            {
                if (value.X < 0)
                {
                    value.X = 0;
                }
                if (value.Y < 0)
                {
                    value.Y = 0;
                }

                _size = value;
            }
        }

		/// <summary>
		/// Property to set or return the texture to apply to the sprite.
		/// </summary>
        public GorgonTexture2D Texture
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the texture region to map to the sprite.
		/// </summary>
		/// <remarks>This value is in texture space (0..1).</remarks>
        public RectangleF TextureRegion
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the initial position of the sprite.
		/// </summary>
        public Vector2 InitialPosition
        {
            get;
            set;
        }

	    /// <summary>
	    /// Property to set or return the initial scale of the sprite.
	    /// </summary>
	    public Vector2 InitialScale
	    {
	        get;
	        set;
	    }

	    /// <summary>
		/// Property to set or return the initial angle of the sprite.
		/// </summary>
        public float InitialAngle
        {
            get;
            set;
        }
		#endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSpriteSettings"/> class.
        /// </summary>
        public GorgonSpriteSettings()
        {
            InitialScale = new Vector2(1);
            Color = GorgonColor.White;
            InitialPosition = Vector2.Zero;
            InitialAngle = 0.0f;
            TextureRegion = new RectangleF(0, 0, 1.0f, 1.0f);
            Texture = null;
            Anchor = Vector2.Zero;
        }
        #endregion
    }
}
