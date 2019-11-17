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
    /// <remarks>
    /// <para>
    /// This class allows developers to build special effects utilizing shaders and various states by wrapping them in a custom object to simplify rendering with the aforementioned shaders and states. 
    /// </para>
    /// <para>
    /// Effects can operate in any way the developer deems appropriate as long as the internal code executes in the correct order. In Gorgon, the predefined effects such as Guassian Blur operate in 
    /// one of 3 modes: Immediate, Render texture, or both. The immediate mode works in the same way as the <see cref="Gorgon2D"/> renderer, by calling a Begin and End method and rendering the various 
    /// graphics types in between the Begin/End calls. The Render Texture mode will take a <see cref="GorgonTexture2DView"/> (often the output of a render target) and render it into a 
    /// <see cref="GorgonRenderTargetView"/>. This mode would usually be used in post processing.
    /// </para>
    /// <para>
    /// Rendering in an effect is usually done in 1 or more passes. With each pass usually building on the result of the previous pass. Developers are free to expose each pass to the end user for 
    /// custom rendering at each stage, wrap up the passes into a single method, or whatever they may deem appropriate. 
    /// </para>
    /// <para>
    /// To create a custom effect, applications should inherit this class and override the appropriate methods to set up state, and perform rendering pass(es). Gorgon has multiple predefined effects 
    /// that developers can examine for reference.
    /// </para>
    /// </remarks>
    /// <seealso cref="Gorgon2D"/>
    /// <seealso cref="GorgonTexture2DView"/>
    /// <seealso cref="GorgonRenderTargetView"/>
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
        // The state used to override the default blend state for the effect.
        private GorgonBlendState _blendStateOverride;
        // The state used to override the default depth/stencil state for the effect.
        private GorgonDepthStencilState _depthStencilStateOverride;
        // The state used to override the default raster state for the effect.
        private GorgonRasterState _rasterStateOverride;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the allocator to use with batch states.
        /// </summary>
        /// <remarks>
        /// Allocators are used to reuse objects to limit garbage collection pressure. These should be used in cases where there's states are being updated every frame, or even multiple times 
        /// per frame.
        /// </remarks>
        protected Gorgon2DBatchStatePoolAllocator BatchStateAllocator
        {
            get;
        } = new Gorgon2DBatchStatePoolAllocator(64);

        /// <summary>
        /// Property to return the allocator to use with pixel shader states.
        /// </summary>
        /// <remarks>
        /// Allocators are used to reuse objects to limit garbage collection pressure. These should be used in cases where there's states are being updated every frame, or even multiple times 
        /// per frame.
        /// </remarks>
        protected Gorgon2DShaderStatePoolAllocator<GorgonPixelShader> PixelShaderAllocator
        {
            get;
        } = new Gorgon2DShaderStatePoolAllocator<GorgonPixelShader>(64);

        /// <summary>
        /// Property to return the allocator to use with vertex shader states.
        /// </summary>
        /// <remarks>
        /// Allocators are used to reuse objects to limit garbage collection pressure. These should be used in cases where there's states are being updated every frame, or even multiple times 
        /// per frame.
        /// </remarks>
        protected Gorgon2DShaderStatePoolAllocator<GorgonVertexShader> VertexShaderAllocator
        {
            get;
        } = new Gorgon2DShaderStatePoolAllocator<GorgonVertexShader>(64);

        /// <summary>
        /// Property to return the macros to apply to the shader.
        /// </summary>
        /// <remarks>
        /// A list of <see cref="GorgonShaderMacro"/> values to send to any pixel/vertex shaders used by the effect. These can be used to turn on/off pieces of the shader code when compiling.
        /// </remarks>
        protected List<GorgonShaderMacro> Macros
        {
            get;
        } = new List<GorgonShaderMacro>();

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
        /// <remarks>
        /// This is merely for information, passes may or may not be exposed to the end user by the effect author.
        /// </remarks>
        public virtual int PassCount
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

            if ((blendStateOverride == _blendStateOverride)
                && (depthStencilStateOverride == _depthStencilStateOverride)
                && (rasterStateOverride == _rasterStateOverride))
            {
                return false;
            }

            _effectBuilders.BatchBuilder.BlendState(blendStateOverride ?? GorgonBlendState.Default)
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
        /// <returns>A new shader of type <typeparamref name="T"/>.</returns>        
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="shaderSource"/>, or the <paramref name="entryPoint"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="shaderSource"/>, or the <paramref name="entryPoint"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the shader fails to compile.</exception>
        /// <remarks>
        /// <para>
        /// This is convenience method used to compile a shader from source code. Users may wish to use the <see cref="GorgonShaderFactory"/> class for more robust functionality.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonShaderFactory"/>
        protected T CompileShader<T>(string shaderSource, string entryPoint)
            where T : GorgonShader => GorgonShaderFactory.Compile<T>(Graphics, shaderSource, entryPoint, GorgonGraphics.IsDebugEnabled, Macros);

        /// <summary>
        /// Function called to initialize the effect.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called to allow the effect to initialize. Developers must override this method to compile shaders, constant buffers, etc... and set up any initial values for the effect.
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
        /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// <para>
        /// This method is called prior to rendering the effect (before any passes). It allows an effect to do any required effect set up that's not specific to a pass. For example, this method could 
        /// be used to send a view matrix to a shader via a constant buffer since the view matrix will unlikely change between passes.
        /// </para>
        /// <para>
        /// Developers should override this method to perform any required setup prior to rendering a pass.
        /// </para>
        /// </remarks>
        protected virtual void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
        {
        }

        /// <summary>
        /// Function called after rendering is complete.
        /// </summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks>
        /// <para>
        /// This method is called after all passes are finished and the effect is ready to complete its rendering. Developers should override this method to finalize any custom rendering. For example 
        /// an effect author can use this method to render the final output of an effect to the final render target.
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
        /// This method is called prior to rendering a single pass. It allows per-pass configuration to be done prior to the actual rendering. Developers should override this method if shader data 
        /// needs to be updated on every pass.
        /// </para>
        /// <para>
        /// The return value is a <see cref="PassContinuationState"/> which allows developers to skip specific passes if criteria isn't met, or stop rendering altogether.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// The <see cref="Gorgon2D"/>.<see cref="Gorgon2D.Begin(Gorgon2DBatchState, IGorgon2DCamera)"/> method is already called prior to this method. Developers must not call 
        /// <see cref="Gorgon2D.Begin(Gorgon2DBatchState, IGorgon2DCamera)"/> while rendering a pass, or else an exception will be thrown. 
        /// </para>
        /// </note>
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
        /// This method is called after rendering a single pass. Developers may use this to perform any clean up required, or any last minute rendering at the end of a pass.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// The <see cref="Gorgon2D"/>.<see cref="Gorgon2D.End()"/> method is already called prior to this method. Developers are free to use <see cref="Gorgon2D.Begin"/> and 
        /// <see cref="Gorgon2D.End"/> to perform any last minute rendering (e.g. blending multiple targets together into the <paramref name="output"/>).
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        protected virtual void OnAfterRenderPass(int passIndex, GorgonRenderTargetView output)
        {
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="defaultStatesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        /// <remarks>
        /// <para>
        /// This method is responsible for initializing and setting up a rendering state for a pass. State changes (e.g. blend states change, additonal textures needed, etc...) are also handled 
        /// by this method. Note that the <paramref name="defaultStatesChanged"/> parameter indicates that the user has changed the default effect states when initially rendering (i.e. not per pass).
        /// </para>
        /// <para>
        /// Developers must take care with this method when creating state objects. Constant discarding and creating of states can get expensive as the garbage collector needs to kick in and release 
        /// the memory occupied by the states. To help alleviate constant state changes between passes, of the allocator properties in this class may be used to reuse state objects. The rule of thumb 
        /// however is to create a state once, and then just return that state and only recreate when absolutely necessary. Sometimes that's never, other times it's when the swap chain resizes, and 
        /// other times it may be on every pass. These conditions depend on the requirements of the effect.
        /// </para>
        /// </remarks>
        protected abstract Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool defaultStatesChanged);

        /// <summary>
        /// Function to begin the rendering for the effect.
        /// </summary>
        /// <param name="output">The render target that will receive the rendering.</param>
        /// <param name="blendState">[Optional] The blend state to apply.</param>
        /// <param name="depthStencilState">[Optional] The depth stencil state to apply.</param>
        /// <param name="rasterState">[Optional] The raster state to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if this method is has been called once before and called again without calling <see cref="EndRender"/>.</exception>
        /// <remarks>
        /// <para>
        /// When the effect is ready to begin rendering, developers must call this method <b>first</b> and ensure there's a matching call to <see cref="EndRender"/>. This informs the effect that 
        /// its default states need to be initialized (if not done so prior to calling), handle any render target output size changes, and call any required setup from the effect via the 
        /// <see cref="OnBeforeRender(GorgonRenderTargetView, bool)"/> method.
        /// </para>
        /// <para>
        /// The optional parameters allow effect authors to pass in user defined states from outside of the effect so that end users may customize rendering. In Gorgon's predefined effects, the 
        /// immediate mode (Begin/End) effects take these parameters, while render target rendering does not.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// Nesting BeginRender calls is <b>not</b> supported. 
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, some exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        protected void BeginRender(GorgonRenderTargetView output, GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null)
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

            OnBeforeRender(output, outputSizeChanged);

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
        ///     <para>
        ///     When the effect is ready to begin rendering a single pass, developers <b>must</b> call this method and ensure there's a matching call to <see cref="EndPass"/>. This method will begin by 
        ///     configuring the states via <see cref="OnGetBatchState(int, IGorgon2DEffectBuilders, bool)"/> for rendering the current effect pass, and by initializing batch rendering via the 
        ///     <see cref="Gorgon2D.Begin(Gorgon2DBatchState, IGorgon2DCamera)"/> method. It will also call the <see cref="OnBeforeRenderPass(int, GorgonRenderTargetView, IGorgon2DCamera)"/> method so 
        ///     effects can do per-pass configuration as needed.
        ///     </para>
        ///     
        ///     <para>
        ///     The return value indicates whether the pass may continue on to the next pass, skip the next pass, or stop rendering altogether. These are usually in response to invalid input, or to flags 
        ///     turning off specific functionality. The exact operation is left up to the effect author.
        ///     </para>
        ///     
        ///     <para>
        ///         <note type="warning">
        ///             <para>
        ///             Nesting BeginPass calls is <b>not</b> supported. 
        ///             </para>
        ///         </note>
        ///     </para>
        ///     
        ///     <para>
        ///         <note type="important">
        ///             <para>
        ///             For performance reasons, some exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        ///             </para>
        ///         </note>
        ///     </para>
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
        /// <remarks>
        /// <para>
        /// This is called to notify the effect that the pass is now finished rendering and that we can move on to a new pass (or finish rendering completely if that's what's required). This method is 
        /// responsible for calling the <see cref="Gorgon2D.End"/> method to finish 2D rendering, and will call the <see cref="OnAfterRender(GorgonRenderTargetView)"/> to perform any last minute clean 
        /// up, or last minute rendering for the pass.
        /// </para>
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

            if ((!_isRenderingPass) || (_currentEffect == null))
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
        ///     <para>
        ///     This informs the effect that rendering is done. It will trigger the <see cref="OnAfterRender(GorgonRenderTargetView)"/> method so that effect authors can perform any last minute rendering, 
        ///     or clean up. The method <b>must</b> be called whenever <see cref="BeginRender(GorgonRenderTargetView, GorgonBlendState, GorgonDepthStencilState, GorgonRasterState)"/> is called or else an 
        ///     exception will be thrown.
        ///     </para>
        ///     
        ///     <para>
        ///     <b>Null</b> can be passed to the <paramref name="output"/> method to indicate that the <see cref="OnAfterRender(GorgonRenderTargetView)"/> method should not be called, this is usually done 
        ///     when a pass is stopped prematurely via one of the <see cref="PassContinuationState"/> values.
        ///     </para>
        ///     
        ///     <para>
        ///         <note type="important">
        ///             <para>
        ///             For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        ///             </para>
        ///         </note>
        ///     </para>
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
