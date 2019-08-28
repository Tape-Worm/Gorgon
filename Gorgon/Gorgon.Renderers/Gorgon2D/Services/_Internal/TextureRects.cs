#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 2, 2019 10:46:41 AM
// 
#endregion

using System.Collections.Generic;
using DX = SharpDX;

namespace Gorgon.Renderers.Services
{
    /// <summary>
    /// A list of rectangles for a sprite texture.
    /// </summary>
    internal class TextureRects
    {
        /// <summary>
        /// Property to return the boundaries of the texture.
        /// </summary>
        public DX.Rectangle Bounds
        {
            get;
        }

        /// <summary>
        /// Property to return the list of occupied sprite regions on this texture.
        /// </summary>
        public Dictionary<GorgonSprite, DX.Rectangle> SpriteRegion
        {
            get;
        } = new Dictionary<GorgonSprite, DX.Rectangle>();

        /// <summary>
        /// Property to set or return the array index for the texture.
        /// </summary>
        public int ArrayIndex
        {
            get;
            set;
        }

        /// <summary>Initializes a new instance of the <see cref="TextureRects"/> class.</summary>
        /// <param name="textureBounds">The texture bounds.</param>
        /// <param name="arrayIndex">Index of the texture array to use.</param>
        public TextureRects(DX.Rectangle textureBounds, int arrayIndex)
        {
            Bounds = textureBounds;
            ArrayIndex = arrayIndex;
        }
    }
}
