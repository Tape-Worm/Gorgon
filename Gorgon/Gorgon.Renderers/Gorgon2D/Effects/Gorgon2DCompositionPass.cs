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
// Created: August 2, 2018 3:38:43 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Defines the a composition pass for the <see cref="Gorgon2DCompositor"/>.
    /// </summary>
    public class Gorgon2DCompositionPass
        : GorgonNamedObject
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the color to use when clearing the active render target.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  If this value is set to <b>null</b>, then the current target will not be cleared.
        /// </para>
        /// <para>
        /// The default value is <b>null</b>.
        /// </para>
        /// </remarks>
        public GorgonColor? ClearColor
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the effect for the pass.
        /// </summary>
        public Gorgon2DEffect Effect
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the effect is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Property to set or return a rendering method that is executed when rendering the effect.
        /// </summary>
        /// <remarks>
        /// This allows an application to define custom rendering when the effect is being rendered. The method assigned to this property has 4 parameters:
        /// <list type="number">
        ///     <item>
        ///         <description>The last texture that was processed by a previous effect.</description>
        ///     </item>
        ///     <item>
        ///         <description>The current pass index being rendered.</description>
        ///     </item>
        ///     <item>
        ///         <description>Total number of passes in the effect.</description>
        ///     </item>
        ///     <item>
        ///         <description>The size of the current render target.</description>
        ///     </item>
        /// </list>
        /// </remarks>
        public Action<GorgonTexture2DView, int, int, DX.Size2> RenderMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the blending state override for the effect.
        /// </summary>
        public GorgonBlendState BlendOverride
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the raster state override for the effect.
        /// </summary>
        public GorgonRasterState RasterOverride
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the depth/stencil state override for the effect.
        /// </summary>
        public GorgonDepthStencilState DepthStencilOverride
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the camera to use for the effect.
        /// </summary>
        public IGorgon2DCamera Camera
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return a destination region when blitting.
        /// </summary>
        /// <remarks>
        /// When no <see cref="RenderMethod"/> is assigned, the default behavior is to blit the previous render target to a new one.  This property allows an application to assign the destination region 
        /// to copy the texture into.
        /// </remarks>
        public DX.RectangleF? DestinationRegion
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return source texture coordinates when blitting.
        /// </summary>
        /// <remarks>
        /// When no <see cref="RenderMethod"/> is assigned, the default behavior is to blit the previous render target to a new one.  This property allows an application to assign the texture coordinates
        /// when copying the texture. 
        /// </remarks>
        public DX.RectangleF? SourceCoordinates
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DCompositionPass"/> class.
        /// </summary>
        /// <param name="name">The name of this object.</param>
        /// <param name="effect">[Optional] The effect to apply to the pass.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="effect"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public Gorgon2DCompositionPass(string name, Gorgon2DEffect effect = null)
            : base(name) => Effect = effect;
        #endregion
    }
}
