#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 9, 2018 8:35:23 AM
// 
#endregion

using Gorgon.Graphics;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Represents an immutable vertex for a <see cref="GorgonPolySprite"/>.
    /// </summary>
    public class GorgonPolySpriteVertex
    {
        #region Variables.
        // The actual vertex for the sprite.
        internal Gorgon2DVertex Vertex;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the position of the vertex in space.
        /// </summary>
        public DX.Vector2 Position => (DX.Vector2)Vertex.Position;

        /// <summary>
        /// Property to return the color of the vertex.
        /// </summary>
        public GorgonColor Color
        {
            get => Vertex.Color;
            internal set => Vertex.Color = value;
        }

        /// <summary>
        /// Property to return the texture coordinate (in texel space).
        /// </summary>
        public DX.Vector2 TextureCoordinate
        {
            get => (DX.Vector2)Vertex.UV;
            internal set => Vertex.UV = new DX.Vector4(value, Vertex.UV.Z, 0);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPolySpriteVertex" /> class.
        /// </summary>
        /// <param name="position">The position of the vertex.</param>
        /// <param name="color">The color for the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinates.</param>
        public GorgonPolySpriteVertex(DX.Vector2 position, GorgonColor color, DX.Vector2 textureCoordinate) => Vertex = new Gorgon2DVertex
        {
            Position = new DX.Vector4(position, 0, 1.0f),
            Color = color,
            UV = new DX.Vector4(textureCoordinate, 0, 0)
        };
        #endregion
    }
}
