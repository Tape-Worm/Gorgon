#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Wednesday, November 26, 2014 8:36:33 PM
// 
#endregion

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Gorgon.Design;
using Gorgon.Editor.Design;
using Gorgon.Editor.SpriteEditorPlugIn.Design;
using Gorgon.Editor.SpriteEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditorPlugIn
{
    /// <summary>
    /// Vertex information for a sprite.
    /// </summary>
    [TypeConverter(typeof(SpriteVertexTypeConverter))]
    class SpriteVertex
    {
        #region Variables.
        // Sprite content connected to this vertex.
        private GorgonSpriteContent _sprite;
        // Corner of the sprite rectangle to affect.
        private RectangleCorner _corner;
        #endregion

        #region Properties.
		/// <summary>
		/// Property to set or return the color for the vertex.
		/// </summary>
		[TypeConverter(typeof(RGBATypeConverter)),
		Editor(typeof(RGBAEditor), typeof(UITypeEditor)),
		DefaultValue(typeof(Color), "255,255,255,255"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_VERTEX_COLOR_NAME"), LocalDescription(typeof(Resources), "PROP_SPRITE_VERTEX_COLOR_DESC")]
	    public Color Color
	    {
		    get
		    {
			    GorgonColor result = _sprite.Sprite.GetCornerColor(_corner);

			    if (result == GorgonColor.White)
			    {
					// This is a stupid hack to get around the DefaultValue using the Color type converter
					// which decides that a hex value of FFFFFFFF is White, while doing a ToArgb on a Color
					// is not even though the two values are the same...
				    return Color.White;
			    }

			    return result;
		    }
		    set
		    {
			    Color currentColor = _sprite.Sprite.GetCornerColor(_corner);

			    if (currentColor == value)
			    {
				    return;
			    }

				_sprite.Sprite.SetCornerColor(_corner, value);

				_sprite.OnVertexUpdated(this);
		    }
	    }

        /// <summary>
        /// Property to set or return the offset of the sprite vertex.
        /// </summary>
        [TypeConverter(typeof(PointConverter)), 
		DefaultValue(typeof(Point), "0, 0"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_VERTEX_OFFSET_NAME"), LocalDescription(typeof(Resources), "PROP_SPRITE_VERTEX_OFFSET_DESC")]
        public Point Offset
        {
            get
            {
                return (Point)_sprite.Sprite.GetCornerOffset(_corner);
            }
            set
            {
                var current = (Point)_sprite.Sprite.GetCornerOffset(_corner);

                if (current == value)
                {
                    return;
                }

                _sprite.Sprite.SetCornerOffset(_corner, new Vector2(value));

                _sprite.OnVertexUpdated(this);
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteVertex"/> struct.
        /// </summary>
        /// <param name="sprite">The sprite content to use.</param>
        /// <param name="corner">Corner for the sprite rectangle.</param>
        public SpriteVertex(GorgonSpriteContent sprite, RectangleCorner corner)
        {
            _sprite = sprite;
            _corner = corner;
        }
        #endregion
    }
}
