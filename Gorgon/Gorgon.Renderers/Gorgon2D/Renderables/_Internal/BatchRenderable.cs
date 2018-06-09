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
// Created: June 9, 2018 12:42:34 AM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A base class for a 2D renderable object.
    /// </summary>
    internal class BatchRenderable
    {
        #region Variables.
        // The texture for the renderable.
        private GorgonTexture2DView _texture;
        // The sampler for the renderable.
        private GorgonSamplerState _textureSampler;
        // Alpha test data.
        private AlphaTestData _alphaTest = new AlphaTestData(true, GorgonRangeF.Empty);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether or not the sprite properties have been changed.
        /// </summary>
        public bool Changed
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Property to set or return the reference to the alpha test data.
        /// </summary>
        /// <remarks>
        /// This is used internally to write data into a buffer. Use the <see cref="AlphaTestData"/> property for general use.
        /// </remarks>
        public ref AlphaTestData AlphaTestDataRef => ref _alphaTest;

        /// <summary>
        /// Property to set or return the alpha test data.
        /// </summary>
        public AlphaTestData AlphaTestData
        {
            get => _alphaTest;
            set
            {
                if (_alphaTest.Equals(value))
                {
                    return;
                }

                _alphaTest = value;
                Changed = true;
            }
        }

        /// <summary>
        /// Property to set or return the vertices for this renderable.
        /// </summary>
        public Gorgon2DVertex[] Vertices
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the texture to render.
        /// </summary>
        public GorgonTexture2DView Texture
        {
            get => _texture;
            set
            {
                if (_texture == value)
                {
                    return;
                }

                _texture = value;
                Changed = true;
            }
        }

        /// <summary>
        /// Property to set or return the texture sampler to use when rendering.
        /// </summary>
        public GorgonSamplerState TextureSampler
        {
            get => _textureSampler;
            set
            {
                if (_textureSampler == value)
                {
                    return;
                }

                _textureSampler = value;
                Changed = true;
            }
        }
        #endregion
    }
}
