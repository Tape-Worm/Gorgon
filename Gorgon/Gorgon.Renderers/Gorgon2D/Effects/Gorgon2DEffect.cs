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
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A state that tells the effect how to proceed prior to rendering a pass.
    /// </summary>
    public enum PassContinuationState
    {
        /// <summary>
        /// Continue to the next pass.
        /// </summary>
        Continue = 0,
        /// <summary>
        /// Skip this pass and move on to the next.
        /// </summary>
        Skip = 1,
        /// <summary>
        /// Stop rendering entirely.
        /// </summary>
        Stop = 2,
    }

    /// <summary>
    /// A base class used to implement special effects for 2D rendering.
    /// </summary>
    public abstract class Gorgon2DEffect
        : GorgonNamedObject, IDisposable, IGorgonGraphicsObject
    {
        #region Variables.
        // Flag to indicate that the effect is initialized.
        private bool _isInitialized;
        // The previous size of the output.
        private DX.Size2 _prevOutputSize;
        // The builders used to manage state for the effect.
        private readonly EffectBuilders _effectBuilders = new EffectBuilders();
        // Flag to indicate that the batch state for the effect pass needs updating.
        private bool _needsStateUpdate = true;
        // The state of the current rendering pass.
        private PassContinuationState _passResultState = PassContinuationState.Stop;
        // The currently rendering effect.
        private static string _currentEffect;
        // Flag to indicate that a pass render has started.
        private bool _isRenderingPass;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the allocator to use with batch states.
        /// </summary>
        protected Gorgon2DBatchStatePoolAllocator BatchStateAllocator
        {
            get;
        } = new Gorgon2DBatchStatePoolAllocator(64);

        /// <summary>
        /// Property to return a texture to pass to shaders when no texture is desired.
        /// </summary>
        /// <remarks>
        /// This will keep D3D from complaining about a missing texture.
        /// </remarks>
        protected GorgonTexture2DView EmptyTexture => Renderer.EmptyBlackTexture;

        /// <summary>
        /// Property to return the allocator to use with pixel shader states.
        /// </summary>
        protected Gorgon2DShaderStatePoolAllocator<GorgonPixelShader> PixelShaderAllocator
        {
            get;
        } = new Gorgon2DShaderStatePoolAllocator<GorgonPixelShader>(64);

        /// <summary>
        /// Property to return the allocator to use with vertex shader states.
        /// </summary>
        protected Gorgon2DShaderStatePoolAllocator<GorgonVertexShader> VertexShaderAllocator
        {
            get;
        } = new Gorgon2DShaderStatePoolAllocator<GorgonVertexShader>(64);

        /// <summary>
        /// Property to return the macros to apply to the shader.
        /// </summary>
        protected List<GorgonShaderMacro> Macros
        {
            get;
        } = new List<GorgonShaderMacro>();

        /// <summary>
        /// Property to return the vertex shader state builder used to create our vertex shader(s) for the effect.
        /// </summary>
        protected Gorgon2DShaderStateBuilder<GorgonVertexShader> VertexShaderBuilder
        {
            get;
        } = new Gorgon2DShaderStateBuilder<GorgonVertexShader>();

        /// <summary>
        /// Property to return the state used to override the default blend state for the effect.
        /// </summary>
        protected GorgonBlendState BlendStateOverride
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the state used to override the default depth/stencil state for the effect.
        /// </summary>
        protected GorgonDepthStencilState DepthStencilStateOverride
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the state used to override the default raster state for the effect.
        /// </summary>
        protected GorgonRasterState RasterStateOverride
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
        /// Function to set up state prior to rendering.
        /// </summary>
        /// <param name="blendStateOverride">An override for the current blending state.</param>
        /// <param name="depthStencilStateOverride">An override for the current depth/stencil state.</param>
        /// <param name="rasterStateOverride">An override for the current raster state.</param>
        /// <returns><b>true</b> if state was overridden, <b>false</b> if not or <b>null</b> if rendering is canceled.</returns>
        private bool SetupStates(GorgonBlendState blendStateOverride, GorgonDepthStencilState depthStencilStateOverride, GorgonRasterState rasterStateOverride)
        {

            if ((blendStateOverride == BlendStateOverride)
                && (depthStencilStateOverride == DepthStencilStateOverride)
                && (rasterStateOverride == RasterStateOverride))
            {
                return false;
            }

            _effectBuilders.BatchBuilder.BlendState(blendStateOverride ?? GorgonBlendState.Default)
                                        .DepthStencilState(depthStencilStateOverride ?? GorgonDepthStencilState.Default)
                                        .RasterState(rasterStateOverride ?? GorgonRasterState.Default);

            BlendStateOverride = blendStateOverride;
            DepthStencilStateOverride = depthStencilStateOverride;
            RasterStateOverride = rasterStateOverride;

            return true;
        }

        /// <summary>
        /// Function to create a shader for use with the effect.
        /// </summary>
        /// <typeparam name="T">The type of shader. Must inherit from <see cref="GorgonShader"/>.</typeparam>
        /// <param name="shaderSource">The source code for the shader.</param>
        /// <param name="entryPoint">The entry point function in the shader to execute.</param>
        /// <returns>A new shader.</returns>
        protected T CompileShader<T>(string shaderSource, string entryPoint)
            where T : GorgonShader => GorgonShaderFactory.Compile<T>(Graphics, shaderSource, entryPoint, GorgonGraphics.IsDebugEnabled, Macros);

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
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
        /// targets (if applicable).
        /// </para>
        /// </remarks>
        protected virtual void OnBeforeRender(GorgonRenderTargetView output, IGorgon2DCamera camera, bool sizeChanged)
        {
        }

        /// <summary>
        /// Function called after rendering is complete.
        /// </summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to clean up and/or restore any states when rendering is finished. This is an ideal method to copy any rendering imagery to the final output render target.
        /// </para>
        /// </remarks>
        protected virtual void OnAfterRender(GorgonRenderTargetView output)
        {
        }

        /// <summary>
        /// Function called prior to rendering a pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <returns>A <see cref="PassContinuationState"/> to instruct the effect on how to proceed.</returns>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.
        /// </para>
        /// </remarks>
        /// <seealso cref="PassContinuationState"/>

        protected virtual PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, IGorgon2DCamera camera) => PassContinuationState.Continue;

        /// <summary>
        /// Function called after a pass is finished rendering.
        /// </summary>
        /// <param name="passIndex">The index of the pass that was rendered.</param>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to clean up and/or restore any states after the pass completes.
        /// </para>
        /// </remarks>
        protected virtual void OnAfterRenderPass(int passIndex, GorgonRenderTargetView output)
        {
        }

        /// <summary>
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="output">The render target that will receive the final render data.</param>
        /// <remarks>
        /// <para>
        /// Applications must implement this in order to see any results from the effect.
        /// </para>
        /// </remarks>
        [Obsolete("Donut call")]
        protected virtual void OnRenderPass(int passIndex, GorgonRenderTargetView output)
        {
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="defaultStatesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected abstract Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool defaultStatesChanged);

        /// <summary>
        /// Function to begin the rendering for the effect.
        /// </summary>
        /// <param name="output">The render target that will receive the rendering.</param>
        /// <param name="blendState">[Optional] The blend state to apply.</param>
        /// <param name="depthStencilState">[Optional] The depth stencil state to apply.</param>
        /// <param name="rasterState">[Optional] The raster state to apply.</param>
        /// <param name="camera">[Optional] The camera to use while rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if this method is has been called once before and called again without calling <see cref="EndRender"/>.</exception>
        /// <remarks>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, some exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        protected void BeginRender(GorgonRenderTargetView output, GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, IGorgon2DCamera camera = null)
        {
            output.ValidateObject(nameof(output));

            if (_currentEffect != null)
            {
                throw new GorgonException(GorgonResult.AlreadyInitialized, string.Format(Resources.GOR2D_ERR_EFFECT_BEGIN_RENDER_CALLED, _currentEffect));
            }

            if (!_isInitialized)
            {
                OnInitialize();
                _isInitialized = true;
            }

            _currentEffect = Name;
            bool outputSizeChanged = false;

            if ((_prevOutputSize.Width != output.Width)
                || (_prevOutputSize.Height != output.Height))
            {
                _prevOutputSize = new DX.Size2(output.Width, output.Height);
                outputSizeChanged = true;
            }

            OnBeforeRender(output, camera, outputSizeChanged);

            // Setup user defined states passed in for rendering.
            _needsStateUpdate = SetupStates(blendState, depthStencilState, rasterState);
        }

        /// <summary>
        /// Function to render a single pass in the effect.
        /// </summary>
        /// <param name="index">The index of the pass.</param>
        /// <param name="output">The render target that will receive the rendering.</param>
        /// <param name="camera">[Optional] The camera to use while rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0, or not less than <see cref="PassCount"/>.</exception>
        /// <exception cref="GorgonException">Thrown if this method is has been called once before and called again without calling <see cref="EndPass"/>.
        /// <para>-or-</para>
        /// <para>Thrown if <see cref="BeginRender"/> was not called prior to calling this method.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, some exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="BeginRender"/>
        protected PassContinuationState BeginPass(int index, GorgonRenderTargetView output, IGorgon2DCamera camera = null)
        {
            output.ValidateObject(nameof(output));
            index.ValidateRange(nameof(index), 0, PassCount);

            if (_currentEffect == null)
            {
                throw new GorgonException(GorgonResult.NotInitialized, Resources.GOR2D_ERR_EFFECT_BEGIN_RENDER_NOT_CALLED);
            }

            if (_isRenderingPass)
            {
                throw new GorgonException(GorgonResult.AlreadyInitialized, string.Format(Resources.GOR2D_ERR_EFFECT_PASS_RENDER_CALLED, index, _currentEffect));
            }

            _isRenderingPass = true;

            // Batch state should be cached on the implementation side, otherwise the GC could be impacted by a lot of dead objects per frame.
            Gorgon2DBatchState batchState = OnGetBatchState(index, _effectBuilders, _needsStateUpdate);

            // The state should be updated by now.
            _needsStateUpdate = false;

            _passResultState = OnBeforeRenderPass(index, output, camera);

            if (_passResultState == PassContinuationState.Continue)
            {
                Renderer.Begin(batchState, camera);
            }

            return _passResultState;
        }

        /// <summary>
        /// Function to end the render pass.
        /// </summary>
        /// <param name="index">The index of the pass.</param>
        /// <param name="output">The final output target.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> parameter is less than 0, or not less than <see cref="PassCount"/>.</exception>
        /// <exception cref="GorgonException">Thrown if <see cref="BeginRender"/> was not called prior to calling this method.</exception>
        /// <remarks>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="BeginRender"/>
        protected void EndPass(int index, GorgonRenderTargetView output)
        {
            index.ValidateRange(nameof(index), 0, PassCount);

            if (_currentEffect == null)
            {
                throw new GorgonException(GorgonResult.NotInitialized, Resources.GOR2D_ERR_EFFECT_BEGIN_RENDER_NOT_CALLED);
            }

            if (!_isRenderingPass)
            {
                return;
            }

            try
            {
                if (output == null)
                {
                    return;
                }

                if (Renderer.IsRendering)
                {
                    Renderer.End();
                }

                if (_passResultState == PassContinuationState.Continue)
                {
                    OnAfterRenderPass(index, output);
                }

                _passResultState = PassContinuationState.Stop;                
            }
            finally
            {
                _isRenderingPass = false;
            }
        }

        /// <summary>
        /// Function to end rendering for the effect.
        /// </summary>
        /// <param name="output">The final output target.</param>
        /// <remarks>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        protected void EndRender(GorgonRenderTargetView output)
        {
            if (_currentEffect == null)
            {
                return;
            }

            try
            {
                // Force the pass to end.
                if (Renderer.IsRendering)
                {
                    EndPass(-1, output);
                }

                // Set the output render target so effects can just dump their contents to the output.
                if ((output != null) && (Graphics.RenderTargets[0] != output))
                {
                    Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
                }

                if (output == null)
                {
                    return;
                }

                OnAfterRender(output);
            }
            finally
            {
                _currentEffect = null;
            }
        }

        /// <summary>
        /// Function called to render the effect.
        /// </summary>
        /// <param name="renderMethod">The method used to render the scene that will be used in the effect.</param>
        /// <param name="output">The render target that will receive the results of rendering the effect.</param>
        /// <param name="blendStateOverride">[Optional] An override for the current blending state.</param>
        /// <param name="depthStencilStateOverride">[Optional] An override for the current depth/stencil state.</param>
        /// <param name="rasterStateOverride">[Optional] An override for the current raster state.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderMethod"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="renderMethod"/> is a callback to a method with 2 parameters:
        /// <list type="number">
        ///     <item>
        ///         <description>The current pass index.</description>
        ///     </item>
        ///     <item>
        ///         <description>The size of the current render target.</description>
        ///     </item>
        /// </list>
        /// Users should pass a method that will render the items they want to use with this effect.
        /// </para>
        /// <para>
        /// <para>
        /// If the <paramref name="blendStateOverride"/>, parameter is omitted, then the <see cref="GorgonBlendState.Default"/> is used. When provided, this will override the current blending state.
        /// </para>
        /// <para>
        /// If the <paramref name="depthStencilStateOverride"/> parameter is omitted, then the <see cref="GorgonDepthStencilState.Default"/> is used. When provided, this will override the current
        /// depth/stencil state.
        /// </para>
        /// <para>
        /// If the <paramref name="rasterStateOverride"/> parameter is omitted, then the <see cref="GorgonRasterState.Default"/> is used. When provided, this will override the current raster state.
        /// </para>
        /// <para>
        /// The <paramref name="camera"/> parameter is used to render the texture using a different view, and optionally, a different coordinate set.  
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </para>
        /// </remarks>
        [Obsolete("Donut use")]
        public void Render(Action<int, DX.Size2> renderMethod,
                           GorgonRenderTargetView output,
                           GorgonBlendState blendStateOverride = null,
                           GorgonDepthStencilState depthStencilStateOverride = null,
                           GorgonRasterState rasterStateOverride = null,
                           IGorgon2DCamera camera = null)
        {
            renderMethod.ValidateObject(nameof(renderMethod));
            output.ValidateObject(nameof(output));

            if (!_isInitialized)
            {
                OnInitialize();
                _isInitialized = true;
            }

            bool outputSizeChanged = false;

            if ((_prevOutputSize.Width != output.Width)
                || (_prevOutputSize.Height != output.Height))
            {
                _prevOutputSize = new DX.Size2(output.Width, output.Height);
                outputSizeChanged = true;
            }

            OnBeforeRender(output, camera, outputSizeChanged);

            // Setup user defined states passed in for rendering.
            bool stateChanged = SetupStates(blendStateOverride, depthStencilStateOverride, rasterStateOverride);

            for (int i = 0; i < PassCount; ++i)
            {
                // Batch state should be cached on the implementation side, otherwise the GC could be impacted by a lot of dead objects per frame.
                Gorgon2DBatchState batchState = OnGetBatchState(i, _effectBuilders, stateChanged);
                
                switch (OnBeforeRenderPass(i, output, camera))
                {
                    case PassContinuationState.Continue:
                        Renderer.Begin(batchState, camera);
                        //OnRenderPass(i, renderMethod, output);
                        Renderer.End();
                        
                        OnAfterRenderPass(i, output);
                        break;
                    case PassContinuationState.Skip:
                        continue;
                    default:
                        OnAfterRender(output);
                        return;
                }
            }

            OnAfterRender(output);
        }

        /// <summary>
        /// Function to precache resources and initlaize the effect prior to renderering.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For most effects, there's not a lot to set up or initialize when the rendering starts. But, in some cases, some effects have a considerable amount of work to do prior to rendering. This method
        /// allows an application to pre cache any initialization data after the effect is created, instead of on the 1st render frame (where it may introduce a stutter).
        /// </para>
        /// </remarks>
        public void Precache()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            OnInitialize();
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
