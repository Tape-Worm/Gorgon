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
    internal class CompositionPass
        : GorgonNamedObject, IGorgon2DCompositorPass
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the method to call when rendering without an effect.
        /// </summary>
        public Action<Gorgon2D, GorgonTexture2DView, GorgonRenderTargetView> NoEffectRenderMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the batch state for rendering without an effect.
        /// </summary>
        public Gorgon2DBatchState BatchState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the camera for rendering without an effect.
        /// </summary>
        public IGorgon2DCamera Camera
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the effect for the pass.
        /// </summary>
        public IGorgon2DCompositorEffect Effect
        {
            get;
            set;
        }

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
        /// Property to set or return whether the effect is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        } = true;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionPass"/> class.
        /// </summary>
        /// <param name="name">The name of this object.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public CompositionPass(string name)
            : base(name)
        {        
        }
        #endregion
    }
}
