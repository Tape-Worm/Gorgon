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
// Created: July 4, 2018 9:54:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A base class used to implement special effects for 2D rendering.
    /// </summary>
    public abstract class Gorgon2DEffect
        : GorgonNamedObject, IDisposable, IGorgonGraphicsObject
    {
        #region Variables.
        // Flag to indicate that the effect is initialized.
        private bool _isInitialized;
        // The camera to use when rendering.
        private Gorgon2DCamera _camera;
        // The currently overridden blend state.
        private GorgonBlendState _blendStateOverride;
        // The currently overridden raster state.
        private GorgonRasterState _rasterStateOverride;
        // The currently overridden depth/stencil state.
        private GorgonDepthStencilState _depthStencilStateOverride;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the batch state builder used to create custom batch states.
        /// </summary>
        protected Gorgon2DBatchStateBuilder BatchStateBuilder
        {
            get;
        } = new Gorgon2DBatchStateBuilder();

        /// <summary>
        /// Property to return the pixel shader builder used to create our pixel shader(s) for the effect.
        /// </summary>
        protected Gorgon2DShaderBuilder<GorgonPixelShader> PixelShaderBuilder
        {
            get;
        } = new Gorgon2DShaderBuilder<GorgonPixelShader>();

        /// <summary>
        /// Property to return the macros to apply to the shader.
        /// </summary>
        protected List<GorgonShaderMacro> Macros
        {
            get;
        } = new List<GorgonShaderMacro>();

        /// <summary>
        /// Property to return the vertex shader builder used to create our vertex shader(s) for the effect.
        /// </summary>
        protected Gorgon2DShaderBuilder<GorgonVertexShader> VertexShaderBuilder
        {
            get;
        } = new Gorgon2DShaderBuilder<GorgonVertexShader>();

        /// <summary>
        /// Property to return the width and height of the current render target.
        /// </summary>
        protected DX.Size2 CurrentTargetSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the renderer used to render the effect.
        /// </summary>
        public Gorgon2D Renderer
        {
            get;
        }

        /// <summary>
        /// Property to return the graphics object used for rendering.
        /// </summary>
        public GorgonGraphics Graphics => Renderer?.Graphics;

        /// <summary>
        /// Property to return the number of passes required to render the effect.
        /// </summary>
        public int PassCount
        {
            get;
        }

        /// <summary>
        /// Property to return a friendly description of the effect.
        /// </summary>
        public string Description
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a shader for use with the effect.
        /// </summary>
        /// <typeparam name="T">The type of shader. Must inherit from <see cref="GorgonShader"/>.</typeparam>
        /// <param name="shaderSource">The source code for the shader.</param>
        /// <param name="entryPoint">The entry point function in the shader to execute.</param>
        /// <returns>A new shader.</returns>
        protected T CompileShader<T>(string shaderSource, string entryPoint)
            where T : GorgonShader
        {
            return GorgonShaderFactory.Compile<T>(Graphics, shaderSource, entryPoint, GorgonGraphics.IsDebugEnabled, Macros);
        }

        /// <summary>
        /// Function called to initialize the effect.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Applications must implement this method to ensure that any required resources are created, and configured for the effect.
        /// </para>
        /// </remarks>
        protected abstract void OnInitialize();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Function called prior to rendering.
        /// </summary>
        /// <returns><b>true</b> if rendering should continue, or <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes.
        /// </para>
        /// </remarks>
        protected virtual bool OnBeforeRender()
        {
            return true;
        }

        /// <summary>
        /// Function called after rendering is complete.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Applications can use this to clean up and/or restore any states when rendering is finished.
        /// </para>
        /// </remarks>
        protected virtual void OnAfterRender()
        {
        }

        /// <summary>
        /// Function called prior to rendering a pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <returns><b>true</b> if rendering the current pass should continue, or <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.
        /// </para>
        /// </remarks>
        protected virtual bool OnBeforeRenderPass(int passIndex)
        {
            return true;
        }

        /// <summary>
        /// Function called after a pass is finished rendering.
        /// </summary>
        /// <param name="passIndex">The index of the pass that was rendered.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to clean up and/or restore any states after the pass completes.
        /// </para>
        /// </remarks>
        protected virtual void OnAfterRenderPass(int passIndex)
        {
        }

        /// <summary>
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="batchState">The current batch state for the pass.</param>
        /// <param name="camera">The current camera to use when rendering.</param>
        /// <remarks>
        /// <para>
        /// Applications must implement this in order to see any results from the effect.
        /// </para>
        /// </remarks>
        protected abstract void OnRenderPass(int passIndex, Gorgon2DBatchState batchState, Gorgon2DCamera camera);

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected abstract  Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged);

        /// <summary>
        /// Function called to render the effect.
        /// </summary>
        /// <param name="blendStateOverride">[Optional] An override for the current blending state.</param>
        /// <param name="depthStencilStateOverride">[Optional] An override for the current depth/stencil state.</param>
        /// <param name="rasterStateOverride">[Optional] An override for the current raster state.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        protected void Render(GorgonBlendState blendStateOverride = null, GorgonDepthStencilState depthStencilStateOverride = null, GorgonRasterState rasterStateOverride = null, Gorgon2DCamera camera = null)
        {
            if (!_isInitialized)
            {
                OnInitialize();
                _isInitialized = true;
            }

            if (!OnBeforeRender())
            {
                return;
            }

            GorgonRenderTargetView firstTarget = Graphics.RenderTargets[0];

            if (firstTarget == null)
            {
                return;
            }

            CurrentTargetSize = new DX.Size2(firstTarget.Width, firstTarget.Height);

            bool stateChanged = false;
            _camera = camera;

            if ((blendStateOverride != _blendStateOverride)
                || (depthStencilStateOverride != _depthStencilStateOverride)
                || (rasterStateOverride != _rasterStateOverride))
            {
                BatchStateBuilder.BlendState(blendStateOverride ?? GorgonBlendState.Default)
                                 .DepthStencilState(depthStencilStateOverride ?? GorgonDepthStencilState.Default)
                                 .RasterState(rasterStateOverride ?? GorgonRasterState.Default);

                _blendStateOverride = blendStateOverride;
                _depthStencilStateOverride = depthStencilStateOverride;
                _rasterStateOverride = rasterStateOverride;
                    
                stateChanged = true;
            }

            for (int i = 0; i < PassCount; ++i)
            {
                Gorgon2DBatchState batchState = OnGetBatchState(i, stateChanged);

                OnBeforeRenderPass(i);
                OnRenderPass(i, batchState, _camera);
                OnAfterRenderPass(i);
            }

            OnAfterRender();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DEffect"/> class.
        /// </summary>
        /// <param name="renderer">The 2D renderer used to render the effect.</param>
        /// <param name="effectName">Name of the effect.</param>
        /// <param name="effectDescription">The effect description.</param>
        /// <param name="passCount">The number of passes required to render the effect.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/>, or the <paramref name="effectName"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="effectName"/> parameter is empty.</exception>
        protected Gorgon2DEffect(Gorgon2D renderer, string effectName, string effectDescription, int passCount)
            : base(effectName)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            // Ensure no less than 1 pass.
            PassCount = passCount.Max(1);
            Description = effectDescription ?? string.Empty;
        }
        #endregion
    }
}
