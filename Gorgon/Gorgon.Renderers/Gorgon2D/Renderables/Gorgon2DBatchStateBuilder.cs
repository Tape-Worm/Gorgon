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
// Created: June 7, 2018 3:41:52 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A builder which will buld <see cref="Gorgon2DBatchState"/> objects to pass to the <see cref="Gorgon2D.Begin"/> method.
    /// </summary>
    /// <seealso cref="Gorgon2DBatchState"/>
    /// <seealso cref="Gorgon2D"/>
    public class Gorgon2DBatchStateBuilder
        : IGorgonFluentBuilder<Gorgon2DBatchStateBuilder, Gorgon2DBatchState>
    {
        #region Variables.
        // The state that will be edited.
        private readonly Gorgon2DBatchState _worker = new Gorgon2DBatchState();
        #endregion

        #region Properties.
        /// <summary>
        /// Function to assign the blend state to the batch state.
        /// </summary>
        /// <param name="blendState">The blend state to assign, or <b>null</b> for a default state.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder BlendState(GorgonBlendState blendState)
        {
            _worker.BlendState = blendState;
            return this;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the object.
        /// </summary>
        /// <returns>The object created or updated by this builder.</returns>
        public Gorgon2DBatchState Build()
        {
            var result = new Gorgon2DBatchState
                         {
                             PixelShader = _worker.PixelShader,
                             VertexShader = _worker.VertexShader,
                             BlendState = _worker.BlendState,
                             DepthStencilState = _worker.DepthStencilState,
                             RasterState = _worker.RasterState
                         };

            return result;
        }

        /// <summary>
        /// Function to clear the builder to a default state.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder Clear()
        {
            _worker.PixelShader = null;
            _worker.VertexShader = null;
            _worker.BlendState = null;
            _worker.DepthStencilState = null;
            _worker.RasterState = null;
            return this;
        }

        /// <summary>
        /// Function to reset the builder to the specified object state.
        /// </summary>
        /// <param name="builderObject">[Optional] The specified object state to copy.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder ResetTo(Gorgon2DBatchState builderObject = null)
        {
            if (builderObject == null)
            {
                return Clear();
            }

            _worker.PixelShader = builderObject.PixelShader;
            _worker.VertexShader = builderObject.VertexShader;
            _worker.BlendState = builderObject.BlendState;
            _worker.DepthStencilState = builderObject.DepthStencilState;
            _worker.RasterState = builderObject.RasterState;

            return this;
        }
        #endregion
    }
}
