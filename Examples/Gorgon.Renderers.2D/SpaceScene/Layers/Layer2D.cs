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
// Created: May 20, 2019 11:37:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Graphics;

namespace Gorgon.Examples
{
	/// <summary>
    /// Represents a layer used for 2D drawing.
    /// </summary>
    internal abstract class Layer2D
		: Layer
    {
        #region Properties.
		/// <summary>
        /// Property to return the graphics interface for the applicaton.
        /// </summary>
        protected GorgonGraphics Graphics => Renderer?.Graphics;

		/// <summary>
        /// Property to return the 2D renderer for the application.
        /// </summary>
		protected Gorgon2D Renderer
        {
            get;
        }
        #endregion

        #region Methods.
		/// <summary>
        /// Function to blit the specified texture into the current output target.
        /// </summary>
        /// <param name="texture">The texture to blit.</param>
        /// <param name="textureCoordinates">[Optional] The texture coordinates to use on the source texture.</param>
        /// <param name="samplerState">[Optional] The sampler state to apply.</param>
        protected void Blit(GorgonTexture2DView texture, DX.RectangleF? textureCoordinates = null, GorgonSamplerState samplerState = null) =>
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, OutputSize.Width, OutputSize.Height),
                GorgonColor.White,
                texture,
                textureCoordinates ?? new DX.RectangleF(0, 0, 1, 1),
                textureSampler: samplerState ?? GorgonSamplerState.Default,				
                depth: 0.1f);
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Layer2D"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        protected Layer2D(Gorgon2D renderer) => Renderer = renderer;
		#endregion

    }
}
