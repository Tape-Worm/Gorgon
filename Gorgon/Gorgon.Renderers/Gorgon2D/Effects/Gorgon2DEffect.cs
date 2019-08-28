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
    /// Flags to indicate which states are recorded by the effect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a bit flag enum, so multiple states can be recorded by ORing the values together.
    /// </para>
    /// </remarks>
    [Flags]
    public enum RecordedState
    {
        /// <summary>
        /// Do not record any states.
        /// <para>
        /// This flag is mutally exclusive.
        /// </para>
        /// </summary>
        None = 0,
        /// <summary>
        /// Record render target states.
        /// </summary>
        RenderTargets = 1,
        /// <summary>
        /// Record depth/stencil state.
        /// </summary>
        DepthStencil = 2,
        /// <summary>
        /// Record viewport state.
        /// </summary>
        Viewports = 4,
        /// <summary>
        /// Record all states.
        /// </summary>
        All = RenderTargets | DepthStencil | Viewports
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
        // The currently overridden blend state.
        private GorgonBlendState _blendStateOverride;
        // The currently overridden raster state.
        private GorgonRasterState _rasterStateOverride;
        // The currently overridden depth/stencil state.
        private GorgonDepthStencilState _depthStencilStateOverride;
        // The recall targets.
        private GorgonRenderTargetView[] _targets;
        // The recall viewports.
        private DX.ViewportF[] _viewports;
        // The enumerator for render targets.
        private readonly IEnumerable<GorgonRenderTargetView> _targetEnumerator;
        // The enumerator for viewports.
        private readonly IEnumerable<DX.ViewportF> _viewportEnumerator;
        // The previous size of the output.
        private DX.Size2 _prevOutputSize;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return a texture to pass to shaders when no texture is desired.
        /// </summary>
        /// <remarks>
        /// This will keep D3D from complaining about a missing texture.
        /// </remarks>
        protected GorgonTexture2DView EmptyTexture => Renderer.EmptyBlackTexture;

        /// <summary>
        /// Property to return the list of recorded render targets.
        /// </summary>
        protected IReadOnlyList<GorgonRenderTargetView> RecordedRenderTargets => _targets;

        /// <summary>
        /// Property to return the recorded depth/stencil.
        /// </summary>
        protected GorgonDepthStencil2DView RecordedDepthStencil
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the list of recorded viewports.
        /// </summary>
        protected IReadOnlyList<DX.ViewportF> RecordedViewports => _viewports;

        /// <summary>
        /// Property to return the currently recorded states.
        /// </summary>
        protected RecordedState RecordedStates
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the batch state builder used to create custom batch states.
        /// </summary>
        protected Gorgon2DBatchStateBuilder BatchStateBuilder
        {
            get;
        } = new Gorgon2DBatchStateBuilder();

        /// <summary>
        /// Property to return the pixel shader state builder used to create our pixel shader(s) for the effect.
        /// </summary>
        protected Gorgon2DShaderStateBuilder<GorgonPixelShader> PixelShaderBuilder
        {
            get;
        } = new Gorgon2DShaderStateBuilder<GorgonPixelShader>();

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
        protected GorgonBlendState BlendStateOverride => _blendStateOverride;

        /// <summary>
        /// Property to return the state used to override the default depth/stencil state for the effect.
        /// </summary>
        protected GorgonDepthStencilState DepthStencilStateOverride => _depthStencilStateOverride;

        /// <summary>
        /// Property to return the state used to override the default raster state for the effect.
        /// </summary>
        protected GorgonRasterState RasterStateOverride => _rasterStateOverride;

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
        /// <param name="output">The target used as the output.</param>
        /// <param name="camera">The active camera.</param>
        /// <returns><b>true</b> if state was overridden, <b>false</b> if not or <b>null</b> if rendering is canceled.</returns>
        private bool SetupStates(GorgonBlendState blendStateOverride, GorgonDepthStencilState depthStencilStateOverride, GorgonRasterState rasterStateOverride, GorgonRenderTargetView output, IGorgon2DCamera camera)
        {
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

            if ((blendStateOverride == _blendStateOverride)
                && (depthStencilStateOverride == _depthStencilStateOverride)
                && (rasterStateOverride == _rasterStateOverride))
            {
                return false;
            }

            BatchStateBuilder.BlendState(blendStateOverride ?? GorgonBlendState.Default)
                             .DepthStencilState(depthStencilStateOverride ?? GorgonDepthStencilState.Default)
                             .RasterState(rasterStateOverride ?? GorgonRasterState.Default);

            _blendStateOverride = blendStateOverride;
            _depthStencilStateOverride = depthStencilStateOverride;
            _rasterStateOverride = rasterStateOverride;

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
        /// <param name="renderMethod">The method used to render a scene for the effect.</param>
        /// <param name="output">The render target that will receive the final render data.</param>
        /// <remarks>
        /// <para>
        /// Applications must implement this in order to see any results from the effect.
        /// </para>
        /// </remarks>
        protected virtual void OnRenderPass(int passIndex, Action<int, int, DX.Size2> renderMethod, GorgonRenderTargetView output)
        {
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected abstract Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged);

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
        /// The <paramref name="renderMethod"/> is a callback to a method with 3 parameters:
        /// <list type="number">
        ///     <item>
        ///         <description>The current pass index.</description>
        ///     </item>
        ///     <item>
        ///         <description>The total number of passes.</description>
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
        public void Render(Action<int, int, DX.Size2> renderMethod,
                           GorgonRenderTargetView output,
                           GorgonBlendState blendStateOverride = null,
                           GorgonDepthStencilState depthStencilStateOverride = null,
                           GorgonRasterState rasterStateOverride = null,
                           IGorgon2DCamera camera = null)
        {
            renderMethod.ValidateObject(nameof(renderMethod));
            output.ValidateObject(nameof(output));

            bool stateChanged = SetupStates(blendStateOverride, depthStencilStateOverride, rasterStateOverride, output, camera);

            for (int i = 0; i < PassCount; ++i)
            {
                // Batch state should be cached on the implementation side, otherwise the GC could be impacted by a lot of dead objects per frame.
                Gorgon2DBatchState batchState = OnGetBatchState(i, stateChanged);

                switch (OnBeforeRenderPass(i, output, camera))
                {
                    case PassContinuationState.Continue:
                        Renderer.Begin(batchState, camera);
                        OnRenderPass(i, renderMethod, output);
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
        /// Function to render a texture to the current render target using the current effect.
        /// </summary>
        /// <param name="texture">The texture to render.</param>
        /// <param name="outputSize">The size of the output render target that the blit will render into.</param>
        /// <param name="destinationRegion">[Optional] The region that the texture will be drawn into.</param>
        /// <param name="textureCoordinates">[Optional] The texture coordinates to use with the texture.</param>
        /// <param name="samplerStateOverride">[Optional] An override for the current texture sampler.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This method is a convenience method that will render a texture to the current set render target in slot 0. 
        /// </para>
        /// <para>
        /// If the <paramref name="destinationRegion"/> parameter is omitted, then the texture will be rendered to the full size of the current render target.  If it is provided, then texture will be rendered to the
        /// location specified, and with the width and height specified.
        /// </para>
        /// <para>
        /// If the <paramref name="textureCoordinates"/> parameter is omitted, then the full size of the texture is rendered.
        /// </para>
        /// <para>
        /// If the <paramref name="samplerStateOverride"/> parameter is omitted, then the <see cref="GorgonSamplerState.Default"/> is used.  When provided, this will alter how the pixel shader samples our
        /// texture in slot 0.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonBlendState"/>
        /// <seealso cref="GorgonDepthStencilState"/>
        /// <seealso cref="GorgonRasterState"/>
        /// <seealso cref="Gorgon2DOrthoCamera"/>
        /// <seealso cref="Gorgon2DPerspectiveCamera"/>
        protected void BlitTexture(GorgonTexture2DView texture,
                                   DX.Size2 outputSize,
                                     DX.RectangleF? destinationRegion = null,
                                     DX.RectangleF? textureCoordinates = null,
                                     GorgonSamplerState samplerStateOverride = null)
        {
            texture.ValidateObject(nameof(texture));

            if (destinationRegion == null)
            {
                destinationRegion = new DX.RectangleF(0, 0, outputSize.Width, outputSize.Height);
            }

            if (textureCoordinates == null)
            {
                textureCoordinates = new DX.RectangleF(0, 0, 1, 1);
            }

            Renderer.DrawFilledRectangle(destinationRegion.Value, GorgonColor.White, texture, textureCoordinates, textureSampler: samplerStateOverride);
        }

        /// <summary>
        /// Function to remember the current state of the graphics interface so that it can be reset after rendering.
        /// </summary>
        /// <param name="states">The states to record.</param>
        /// <remarks>
        /// <para>
        /// Applications should use this in order to remember the current set of render targets, depth/stencil or set of view ports on the <see cref="GorgonGraphics"/> interface prior to rendering a scene.
        /// When this method is called, it should be paired with a call to <see cref="RecallState"/> in order to restore the state.
        /// </para>
        /// <para>
        /// The <paramref name="states"/> parameter indicates which state to record based on the value from <see cref="RecordedState"/> enumeration. This enumeration is a bit flag, and the values can be
        /// combined via OR so that multiple states are stored.
        /// </para>
        /// </remarks>
        /// <seealso cref="RecordedState"/>
        /// <seealso cref="GorgonGraphics"/>
        public void RecordState(RecordedState states)
        {
            if (states == RecordedState.None)
            {
                RecordedStates = RecordedState.None;
                RecordedDepthStencil = null;
                Array.Clear(_targets, 0, _targets.Length);
                Array.Clear(_viewports, 0, _viewports.Length);
                return;
            }

            if ((states & RecordedState.RenderTargets) == RecordedState.RenderTargets)
            {
                int targetCount = _targetEnumerator.Count();

                if (_targets.Length != targetCount)
                {
                    Array.Resize(ref _targets, targetCount);
                }

                for (int i = 0; i < targetCount; ++i)
                {
                    _targets[i] = Graphics.RenderTargets[i];
                }
            }

            if ((states & RecordedState.Viewports) == RecordedState.Viewports)
            {
                int viewportCount = _viewportEnumerator.Count();

                if (_viewports.Length != viewportCount)
                {
                    Array.Resize(ref _viewports, viewportCount);
                }

                for (int i = 0; i < viewportCount; i++)
                {
                    _viewports[i] = Graphics.Viewports[i];
                }
            }

            if ((states & RecordedState.DepthStencil) == RecordedState.DepthStencil)
            {
                RecordedDepthStencil = Graphics.DepthStencilView;
            }

            RecordedStates = states;
        }

        /// <summary>
        /// Function to recall the previous state of the graphics interface so that it can be reset after rendering.
        /// </summary>
        /// <param name="state">[Optional] The state(s) to recall.</param>
        public void RecallState(RecordedState state = RecordedState.All)
        {
            if (RecordedStates == RecordedState.None)
            {
                return;
            }

            // Restore our states.
            bool resetDepthStencil = (((RecordedStates & RecordedState.DepthStencil) == RecordedState.DepthStencil)
                                      && ((state & RecordedState.DepthStencil) == RecordedState.DepthStencil));

            if (((RecordedStates & RecordedState.RenderTargets) == RecordedState.RenderTargets)
                && ((state & RecordedState.RenderTargets) == RecordedState.RenderTargets))
            {
                Graphics.SetRenderTargets(_targets, resetDepthStencil ? RecordedDepthStencil : Graphics.DepthStencilView);
                RecordedStates &= ~RecordedState.RenderTargets;

                if (resetDepthStencil)
                {
                    RecordedStates &= ~RecordedState.DepthStencil;
                }
            }
            else if (((RecordedStates & RecordedState.DepthStencil) == RecordedState.DepthStencil)
                     && ((state & RecordedState.DepthStencil) == RecordedState.DepthStencil))
            {
                Graphics.SetDepthStencil(RecordedDepthStencil);
                RecordedStates &= ~RecordedState.DepthStencil;
            }

            if (((RecordedStates & RecordedState.Viewports) != RecordedState.Viewports)
                || ((state & RecordedState.Viewports) != RecordedState.Viewports))
            {
                return;
            }

            Graphics.SetViewports(_viewports);
            RecordedStates &= ~RecordedState.Viewports;
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

            _targets = new GorgonRenderTargetView[1];
            _viewports = new DX.ViewportF[1];

            // Ensure no less than 1 pass.
            PassCount = passCount.Max(1);
            Description = effectDescription ?? string.Empty;

            _targetEnumerator = Graphics.RenderTargets.TakeWhile(item => item != null);
            _viewportEnumerator = Graphics.Viewports.TakeWhile(item => (!item.Width.EqualsEpsilon(0)) && (!item.Height.EqualsEpsilon(0)));
        }
        #endregion
    }
}
