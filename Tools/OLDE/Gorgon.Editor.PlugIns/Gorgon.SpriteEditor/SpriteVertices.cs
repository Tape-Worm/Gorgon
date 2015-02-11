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
// Created: Wednesday, November 26, 2014 8:43:38 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Properties;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn
{
    /// <summary>
    /// A list of sprite vertices.
    /// </summary>
    class SpriteVertices
    {
        #region Properties.
        /// <summary>
        /// Property to return the upper left corner of the sprite rectangle.
        /// </summary>
		[LocalDisplayName(typeof(Resources), "PROP_SPRITE_UL_VERTEX_NAME"), LocalDescription(typeof(Resources), "PROP_SPRITE_UL_VERTEX_DESC")]
        public SpriteVertex UpperLeft
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the upper right corner of the sprite rectangle.
        /// </summary>
		[LocalDisplayName(typeof(Resources), "PROP_SPRITE_UR_VERTEX_NAME"), LocalDescription(typeof(Resources), "PROP_SPRITE_UR_VERTEX_DESC")]
        public SpriteVertex UpperRight
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the lower left corner of the sprite rectangle.
        /// </summary>
		[LocalDisplayName(typeof(Resources), "PROP_SPRITE_LL_VERTEX_NAME"), LocalDescription(typeof(Resources), "PROP_SPRITE_LL_VERTEX_DESC")]
        public SpriteVertex LowerLeft
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the lower right corner of the sprite rectangle.
        /// </summary>
		[LocalDisplayName(typeof(Resources), "PROP_SPRITE_LR_VERTEX_NAME"), LocalDescription(typeof(Resources), "PROP_SPRITE_LR_VERTEX_DESC")]
        public SpriteVertex LowerRight
        {
            get;
            private set;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="SpriteVertices"/> class.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        public SpriteVertices(GorgonSpriteContent sprite)
        {
            UpperLeft = new SpriteVertex(sprite, RectangleCorner.UpperLeft);
            UpperRight = new SpriteVertex(sprite, RectangleCorner.UpperRight);
            LowerLeft = new SpriteVertex(sprite, RectangleCorner.LowerLeft);
            LowerRight = new SpriteVertex(sprite, RectangleCorner.LowerRight);
        }
        #endregion
    }
}
