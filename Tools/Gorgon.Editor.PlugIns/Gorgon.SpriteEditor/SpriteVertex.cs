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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Renderers;
using SlimMath;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn
{
    /// <summary>
    /// Vertex information for a sprite.
    /// </summary>
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
        /// Property to set or return the position of the sprite vertex.
        /// </summary>
        [TypeConverter(typeof(PointConverter))]
        public Point Position
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
