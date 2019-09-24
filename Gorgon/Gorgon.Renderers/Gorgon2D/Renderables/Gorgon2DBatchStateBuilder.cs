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
        : IGorgonFluentBuilderAllocator<Gorgon2DBatchStateBuilder, Gorgon2DBatchState, Gorgon2DBatchStatePoolAllocator>
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

        /// <summary>
        /// Function to assign the blend state to the batch state.
        /// </summary>
        /// <param name="blendState">The blend state to assign, or <b>null</b> for a default state.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder BlendState(GorgonBlendStateBuilder blendState)
        {
            _worker.BlendState = blendState?.Build();
            return this;
        }

        /// <summary>
        /// Function to assign a raster state to the batch state.
        /// </summary>
        /// <param name="rasterState">The raster state to assign, or <b>null</b> for a default state.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder RasterState(GorgonRasterState rasterState)
        {
            _worker.RasterState = rasterState;
            return this;
        }

        /// <summary>
        /// Function to assign a raster state to the batch state.
        /// </summary>
        /// <param name="rasterState">The raster state to assign, or <b>null</b> for a default state.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder RasterState(GorgonRasterStateBuilder rasterState)
        {
            _worker.RasterState = rasterState?.Build();
            return this;
        }

        /// <summary>
        /// Function to assign a depth/stencil state to the batch state.
        /// </summary>
        /// <param name="depthStencilState">The depth/stencil state to assign, or <b>null</b> for a default state.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder DepthStencilState(GorgonDepthStencilState depthStencilState)
        {
            _worker.DepthStencilState = depthStencilState;
            return this;
        }

        /// <summary>
        /// Function to assign a depth/stencil state to the batch state.
        /// </summary>
        /// <param name="depthStencilState">The depth/stencil state to assign, or <b>null</b> for a default state.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder DepthStencilState(GorgonDepthStencilStateBuilder depthStencilState)
        {
            _worker.DepthStencilState = depthStencilState?.Build();
            return this;
        }

        /// <summary>
        /// Function to assign a pixel shader state to the batch state.
        /// </summary>
        /// <param name="shader">The pixel shader and resources to assign, or <b>null</b> for a default pixel shader and states.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder PixelShaderState(Gorgon2DShaderStateBuilder<GorgonPixelShader> shader)
        {
            _worker.PixelShaderState = shader?.Build();
            return this;
        }

        /// <summary>
        /// Function to assign a vertex shader state to the batch state.
        /// </summary>
        /// <param name="shader">The vertex shader and resources to assign, or <b>null</b> for a default vertex shader and states.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder VertexShaderState(Gorgon2DShaderStateBuilder<GorgonVertexShader> shader)
        {
            _worker.VertexShaderState = shader?.Build();
            return this;
        }

        /// <summary>
        /// Function to assign a pixel shader state to the batch state.
        /// </summary>
        /// <param name="shader">The pixel shader and resources to assign, or <b>null</b> for a default pixel shader and states.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder PixelShaderState(Gorgon2DShaderState<GorgonPixelShader> shader)
        {
            _worker.PixelShaderState = shader;
            return this;
        }

        /// <summary>
        /// Function to assign a vertex shader state to the batch state.
        /// </summary>
        /// <param name="shader">The vertex shader and resources to assign, or <b>null</b> for a default vertex shader and states.</param>
        /// <returns>The fluent builder interface.</returns>
        public Gorgon2DBatchStateBuilder VertexShaderState(Gorgon2DShaderState<GorgonVertexShader> shader)
        {
            _worker.VertexShaderState = shader;
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
                PixelShaderState = _worker.PixelShaderState,
                VertexShaderState = _worker.VertexShaderState,
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
            _worker.PixelShaderState = null;
            _worker.VertexShaderState = null;
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

            _worker.PixelShaderState = builderObject.PixelShaderState;
            _worker.VertexShaderState = builderObject.VertexShaderState;
            _worker.BlendState = builderObject.BlendState;
            _worker.DepthStencilState = builderObject.DepthStencilState;
            _worker.RasterState = builderObject.RasterState;

            return this;
        }

        /// <summary>Function to return the object.</summary>
        /// <param name="allocator">The allocator used to create an instance of the object</param>
        /// <returns>The object created or updated by this builder.</returns>
        /// <remarks>
        ///   <para>
        /// Using an <paramref name="allocator" /> can provide different strategies when building objects.  If omitted, the object will be created using the standard <span class="keyword">new</span> keyword.
        /// </para>
        ///   <para>
        /// A custom allocator can be beneficial because it allows us to use a pool for allocating the objects, and thus allows for recycling of objects. This keeps the garbage collector happy by keeping objects
        /// around for as long as we need them, instead of creating objects that can potentially end up in the large object heap or in Gen 2.
        /// </para>
        ///   <para>
        /// A object requires that at least a vertex shader be bound. If none is present, then the method will throw an exception.
        /// </para>
        /// </remarks>
        public Gorgon2DBatchState Build(Gorgon2DBatchStatePoolAllocator allocator)
        {
            if (allocator == null)
            {
                return Build();
            }

            Gorgon2DBatchState state = allocator.Allocate();

            state.PixelShaderState = _worker.PixelShaderState;
            state.VertexShaderState = _worker.VertexShaderState;
            state.BlendState = _worker.BlendState;
            state.DepthStencilState = _worker.DepthStencilState;
            state.RasterState = _worker.RasterState;

            return state;
        }
        #endregion
    }
}
