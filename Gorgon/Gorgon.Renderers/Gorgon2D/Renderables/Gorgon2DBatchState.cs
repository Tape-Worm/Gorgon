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
// Created: June 13, 2018 4:59:08 PM
// 
#endregion

using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines the state to pass to a call to the <see cref="Gorgon2D.Begin"/> method.
    /// </summary>
    public sealed class Gorgon2DBatchState
    {
        /// <summary>
        /// A default batch state that turns off blending.
        /// </summary>
        public static readonly Gorgon2DBatchState NoBlend = new Gorgon2DBatchState
                                                            {
                                                                BlendState = GorgonBlendState.NoBlending
                                                            };

        /// <summary>
        /// A default batch state that sets up additive blending.
        /// </summary>
        public static readonly Gorgon2DBatchState AdditiveBlend = new Gorgon2DBatchState
                                                                  {
                                                                      BlendState = GorgonBlendState.Additive
                                                                  };

        /// <summary>
        /// A default batch state that sets up premultiplied blending.
        /// </summary>
        public static readonly Gorgon2DBatchState PremultipliedBlend = new Gorgon2DBatchState
                                                                       {
                                                                           BlendState = GorgonBlendState.Premultiplied
                                                                       };

        /// <summary>
        /// A default batch state that sets up wirefame mode with no culling.
        /// </summary>
        public static readonly Gorgon2DBatchState WireFrameNoCulling = new Gorgon2DBatchState
                                                                       {
                                                                           RasterState = GorgonRasterState.WireFrameNoCulling
                                                                       };

        /// <summary>
        /// A default batch state that enabled depth testing/writing and no blending.
        /// </summary>
        public static readonly Gorgon2DBatchState DepthEnabledNoBlend = new Gorgon2DBatchState
        {
            BlendState = GorgonBlendState.NoBlending,
            DepthStencilState = GorgonDepthStencilState.DepthEnabled
        };

        /// <summary>
        /// A default batch state that enabled depth testing/writing and modulated blending.
        /// </summary>
        public static readonly Gorgon2DBatchState DepthEnabled = new Gorgon2DBatchState
        {
			BlendState = GorgonBlendState.Default,
            DepthStencilState = GorgonDepthStencilState.DepthEnabled
        };

        /// <summary>
        /// A default batch state that enabled depth testing/writing and additive blending.
        /// </summary>
        public static readonly Gorgon2DBatchState DepthEnabledAdditiveBlend = new Gorgon2DBatchState
        {
            BlendState = GorgonBlendState.Additive,
            DepthStencilState = GorgonDepthStencilState.DepthEnabled
        };

        /// <summary>
        /// A default batch state that enabled depth testing/writing and premultiplied blending.
        /// </summary>
        public static readonly Gorgon2DBatchState DepthEnabledPremultipliedBlend = new Gorgon2DBatchState
        {
            BlendState = GorgonBlendState.Premultiplied,
            DepthStencilState = GorgonDepthStencilState.DepthEnabled
        };

        /// <summary>
        /// Property to return the current blending state to apply.
        /// </summary>
        public GorgonBlendState BlendState
        {
            get;
            internal set;
        } = GorgonBlendState.Default;

        /// <summary>
        /// Property to return the current raster state to apply.
        /// </summary>
        public GorgonRasterState RasterState
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the current depth/stencil state to apply.
        /// </summary>
        public GorgonDepthStencilState DepthStencilState
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the current pixel shader to use.
        /// </summary>
        public Gorgon2DShader<GorgonPixelShader> PixelShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the current vertex shader to use.
        /// </summary>
        public Gorgon2DShader<GorgonVertexShader> VertexShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DBatchState"/> class.
        /// </summary>
        internal Gorgon2DBatchState()
        {
            // We should not be able to create this outside of the assembly.
        }
    }
}
