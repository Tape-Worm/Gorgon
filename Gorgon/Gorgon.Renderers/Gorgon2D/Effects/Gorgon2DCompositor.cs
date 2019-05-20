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
// Created: August 2, 2018 12:25:08 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using DX = SharpDX;
using Gorgon.Collections;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A compositor system used to chain multiple effects together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This processor will take a scene and composite it using a series of effects (or plain rendering without effects).  The results of these effects will be passed on to the next effect and rendered
    /// until all effects are processed. The final image is then output to a render target specified by the user.
    /// </para>
    /// </remarks>
    public class Gorgon2DCompositor
        : IDisposable, IGorgonGraphicsObject
    {
        #region Variables.
        // The final output for the scene.
        private GorgonRenderTargetView _final;
        // The original, unfiltered scene.
        private GorgonRenderTarget2DView _originalTarget;
        // The ping render target in the ping-pong target scheme.
        private GorgonRenderTarget2DView _pingTarget;
        // The pong render target in the ping-pong target scheme.
        private GorgonRenderTarget2DView _pongTarget;
        // The texture view for the original target.
        private GorgonTexture2DView _originalTexture;
        // The texture view for the ping target.
        private GorgonTexture2DView _pingTexture;
        // The texture view for the pong target.
        private GorgonTexture2DView _pongTexture;
        // The ordered list where the actual passes are stored.
        private readonly GorgonNamedObjectList<Gorgon2DCompositionPass> _effectList = new GorgonNamedObjectList<Gorgon2DCompositionPass>(false);
        // The unique list of effects
        private readonly Dictionary<string, int> _effects = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // The builder for the batch state.
        private readonly Gorgon2DBatchStateBuilder _batchStateBuilder = new Gorgon2DBatchStateBuilder();
        // The batch state for the initial scene.
        private Gorgon2DBatchState _batchState = Gorgon2DBatchState.NoBlend;
        // The builder for the final batch state.
        private readonly Gorgon2DBatchStateBuilder _finalBatchStateBuilder = new Gorgon2DBatchStateBuilder();
        // The batch state for the final scene.
        private Gorgon2DBatchState _finalBatchState = Gorgon2DBatchState.NoBlend;
        // The builder for the batch state.
        private readonly Gorgon2DBatchStateBuilder _noEffectBatchStateBuilder = new Gorgon2DBatchStateBuilder();
        // The batch state for the initial scene.
        private Gorgon2DBatchState _noEffectBatchState = Gorgon2DBatchState.NoBlend;
        // The camera to use with the initial scene render.
        private IGorgon2DCamera _camera;
        // Flag to indicate that the batch state has changed.
        private bool _hasBatchStateChanged;
        // Flag to indicate that the batch state has changed.
        private bool _hasFinalBatchStateChanged;
        // The color used to clear the initial render target.
        private GorgonColor? _initialClear = GorgonColor.BlackTransparent;
        // The color used to clear the final render target.
        private GorgonColor? _finalClear = GorgonColor.BlackTransparent;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the renderer to use when rendering the scene.
        /// </summary>
        public Gorgon2D Renderer
        {
            get;
        }

        /// <summary>
        /// Property to return the composition passes registered with the compositor.
        /// </summary>
        public IGorgonNamedObjectReadOnlyList<Gorgon2DCompositionPass> Passes => _effectList;

        /// <summary>
        /// Property to set or return the sampler state for the default action.
        /// </summary>
        public GorgonSamplerState DefaultActionSampler
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the resources need updating or not.
        /// </summary>
        /// <param name="outputTarget">The final render output target.</param>
        /// <returns><b>true</b> if the resources need updating, or <b>false</b> if not.</returns>
        private bool NeedsResourceUpdate(GorgonRenderTargetView outputTarget) => (_final == null)
                                                                                 || (_originalTarget == null)
                                                                                 || (_originalTarget.Width != outputTarget.Width)
                                                                                 || (_originalTarget.Height != outputTarget.Height)
                                                                                 || (_originalTarget.Format != outputTarget.Format);

        /// <summary>
        /// Function to free any resources allocated by the compositor.
        /// </summary>
        private void FreeResources()
        {
            GorgonTexture2DView originalTexture = Interlocked.Exchange(ref _originalTexture, null);
            GorgonTexture2DView pingTexture = Interlocked.Exchange(ref _pingTexture, null);
            GorgonTexture2DView pongTexture = Interlocked.Exchange(ref _pongTexture, null);
            GorgonRenderTarget2DView originalTarget = Interlocked.Exchange(ref _originalTarget, null);
            GorgonRenderTarget2DView pingTarget = Interlocked.Exchange(ref _pingTarget, null);
            GorgonRenderTarget2DView pongTarget = Interlocked.Exchange(ref _pongTarget, null);

            originalTexture?.Dispose();
            pingTexture?.Dispose();
            pongTexture?.Dispose();
            originalTarget?.Dispose();
            pingTarget?.Dispose();
            pongTarget?.Dispose();
        }

        /// <summary>
        /// Function to create the required resources for the processor.
        /// </summary>
        /// <param name="outputTarget">The final output target.</param>
        private void CreateResources(GorgonRenderTargetView outputTarget)
        {
            FreeResources();

            _final = outputTarget;

            _originalTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics,
                                                                          new GorgonTexture2DInfo("Gorgon 2D Post Process Original Image")
                                                                          {
                                                                              Format = outputTarget.Format,
                                                                              Width = outputTarget.Width,
                                                                              Height = outputTarget.Height,
                                                                              Binding = TextureBinding.ShaderResource
                                                                          });
            _originalTexture = _originalTarget.GetShaderResourceView();

            _pingTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo(_originalTarget, "Gorgon 2D Post Process Ping Render Target"));
            _pingTexture = _pingTarget.GetShaderResourceView();

            _pongTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo(_originalTarget, "Gorgon 2D Post Process Pong Render Target"));
            _pongTexture = _pongTarget.GetShaderResourceView();
        }

        /// <summary>
        /// Function to render the initial scene to the initial render target.
        /// </summary>
        /// <param name="renderMethod">The method to execute that will contain rendering commands.</param>
        private void RenderInitalScene(Action renderMethod)
        {
            if (Graphics.RenderTargets[0] != _originalTarget)
            {
                Graphics.SetRenderTarget(_originalTarget, Graphics.DepthStencilView);
            }

            if (_initialClear != null)
            {
                _originalTarget.Clear(_initialClear.Value);
            }

            Renderer.Begin(_batchState, _camera);
            renderMethod();
            Renderer.End();
        }

        /// <summary>
        /// Function to render the initial scene to the initial render target.
        /// </summary>
        /// <param name="lastTargetTexture">The last texture used as a target.</param>
        private void CopyToFinal(GorgonTexture2DView lastTargetTexture)
        {
            if (Graphics.RenderTargets[0] != _final)
            {
                Graphics.SetRenderTarget(_final);
            }

            if (_finalClear != null)
            {
                _final.Clear(_finalClear.Value);
            }

            // Copy the composited output into the final render target specified by the user.
            Renderer.Begin(_finalBatchState, _camera);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, lastTargetTexture.Width, lastTargetTexture.Height),
                                         GorgonColor.White,
                                         lastTargetTexture,
                                         new DX.RectangleF(0, 0, 1, 1));
            Renderer.End();
        }

        /// <summary>
        /// Function to render a scene that does use an effect.
        /// </summary>
        /// <param name="pass">The current pass.</param>
        /// <param name="currentTarget">The currently active render target.</param>
        /// <param name="currentTexture">The texture for the previously active render target.</param>
        private void RenderEffectPass(Gorgon2DCompositionPass pass, GorgonRenderTargetView currentTarget, GorgonTexture2DView currentTexture)
        {
            // The callback method for rendering the effect.
            void RenderAction(int passIndex, int passCount, DX.Size2 size)
            {
                if (pass.RenderMethod != null)
                {
                    pass.RenderMethod(currentTexture, passIndex, passCount, size);
                }
                else
                {
                    DX.RectangleF destRegion = pass.DestinationRegion ?? new DX.RectangleF(0, 0, size.Width, size.Height);
                    DX.RectangleF srcCoords = pass.SourceCoordinates ?? new DX.RectangleF(0, 0, 1, 1);

                    Renderer.DrawFilledRectangle(destRegion,
                                                 GorgonColor.White,
                                                 currentTexture,
                                                 srcCoords,
                                                 textureSampler: DefaultActionSampler);
                }
            }

            pass.Effect.Render(RenderAction,
                               currentTarget,
                               pass.BlendOverride ?? GorgonBlendState.NoBlending,
                               pass.DepthStencilOverride,
                               pass.RasterOverride,
                               pass.Camera);
        }

        /// <summary>
        /// Function to render a scene that does not use an effect.
        /// </summary>
        /// <param name="pass">The current pass.</param>
        /// <param name="currentTarget">The currently active render target.</param>
        /// <param name="currentTexture">The texture for the previously active render target.</param>
        private void RenderNoEffectPass(Gorgon2DCompositionPass pass, GorgonRenderTargetView currentTarget, GorgonTexture2DView currentTexture)
        {
            if (pass.RenderMethod == null)
            {
                return;
            }

            // If we changed the states, then apply them now.
            if ((_noEffectBatchState.BlendState != pass.BlendOverride)
                || (_noEffectBatchState.RasterState != pass.RasterOverride)
                || (_noEffectBatchState.DepthStencilState != pass.DepthStencilOverride))
            {
                _noEffectBatchState = _noEffectBatchStateBuilder.BlendState(pass.BlendOverride)
                                                                .DepthStencilState(pass.DepthStencilOverride)
                                                                .RasterState(pass.RasterOverride)
                                                                .Build();
            }

            if (Graphics.RenderTargets[0] != currentTarget)
            {
                Graphics.SetRenderTarget(currentTarget, Graphics.DepthStencilView);
            }

            Renderer.Begin(_noEffectBatchState, pass.Camera);
            pass.RenderMethod(currentTexture, 0, 0, new DX.Size2(currentTarget.Width, currentTarget.Height));
            Renderer.End();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _final, null);
            FreeResources();
        }

        /// <summary>
        /// Function to assign a camera for the initial and final scene rendering.
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor Camera(IGorgon2DCamera camera)
        {
            _camera = camera;
            return this;
        }

        /// <summary>
        /// Function to provide an override for the depth/stencil state when rendering the initial scene.
        /// </summary>
        /// <param name="depthStencilState">The depth/stencil state to use.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor InitialDepthStencilState(GorgonDepthStencilState depthStencilState)
        {
            _batchStateBuilder.DepthStencilState(depthStencilState);
            _hasBatchStateChanged = true;
            return this;
        }

        /// <summary>
        /// Function to provide an override for the raster state when rendering the initial scene.
        /// </summary>
        /// <param name="rasterState">The raster state to use.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor InitialRasterState(GorgonRasterState rasterState)
        {
            _batchStateBuilder.RasterState(rasterState);
            _hasBatchStateChanged = true;
            return this;
        }

        /// <summary>
        /// Function to provide an override for the blending state when rendering the initial scene.
        /// </summary>
        /// <param name="blendState">The blending state to use.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor InitialBlendState(GorgonBlendState blendState)
        {
            _batchStateBuilder.BlendState(blendState ?? GorgonBlendState.NoBlending);
            _hasBatchStateChanged = true;
            return this;
        }

        /// <summary>
        /// Function to assign the color to use when clearing the initial render target when rendering begins.
        /// </summary>
        /// <param name="value">The color to use when clearing.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <remarks>
        /// <para>
        /// When this value is non-<b>null</b>, the very first rendering of the scene will have its render target initialized to this color. If the value is <b>null</b>, then the render target will not be
        /// cleared at all.
        /// </para>
        /// <para>
        /// The default value is <see cref="GorgonColor.BlackTransparent"/>.
        /// </para>
        /// </remarks>
        public Gorgon2DCompositor InitialClearColor(GorgonColor? value)
        {
            _initialClear = value;
            return this;
        }

        /// <summary>
        /// Function to provide an override for the depth/stencil state when rendering the initial scene.
        /// </summary>
        /// <param name="depthStencilState">The depth/stencil state to use.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor FinalDepthStencilState(GorgonDepthStencilState depthStencilState)
        {
            _finalBatchStateBuilder.DepthStencilState(depthStencilState);
            _hasFinalBatchStateChanged = true;
            return this;
        }

        /// <summary>
        /// Function to provide an override for the raster state when rendering the initial scene.
        /// </summary>
        /// <param name="rasterState">The raster state to use.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor FinalRasterState(GorgonRasterState rasterState)
        {
            _finalBatchStateBuilder.RasterState(rasterState);
            _hasFinalBatchStateChanged = true;
            return this;
        }

        /// <summary>
        /// Function to provide an override for the blending state when rendering the initial scene.
        /// </summary>
        /// <param name="blendState">The blending state to use.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor FinalBlendState(GorgonBlendState blendState)
        {
            _finalBatchStateBuilder.BlendState(blendState ?? GorgonBlendState.NoBlending);
            _hasFinalBatchStateChanged = true;
            return this;
        }

        /// <summary>
        /// Function to assign the color to use when clearing the final render target when rendering completes.
        /// </summary>
        /// <param name="value">The color to use when clearing.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <remarks>
        /// <para>
        /// When this value is non-<b>null</b>, the very last rendering of the scene will clear the render target supplied by the user in the <see cref="Render"/> method to the <paramref name="value"/>
        /// specified. If the value is <b>null</b>, then the render target will not be cleared at all.
        /// </para>
        /// <para>
        /// The default value is <see cref="GorgonColor.BlackTransparent"/>.
        /// </para>
        /// </remarks>
        public Gorgon2DCompositor FinalClearColor(GorgonColor? value)
        {
            _finalClear = value;
            return this;
        }

        /// <summary>
        /// Function to clear the effects from the compositor.
        /// </summary>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor Clear()
        {
            _batchState = Gorgon2DBatchState.NoBlend;
            _batchStateBuilder.Clear();
            _batchStateBuilder.BlendState(GorgonBlendState.NoBlending);
            _effectList.Clear();
            _effects.Clear();
            _hasBatchStateChanged = false;
            return this;
        }

        /// <summary>
        /// Function to remove the first matching pass in the effect chain.
        /// </summary>
        /// <param name="index">The index of the pass to remove.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor RemovePass(int index)
        {
            if ((index < 0) || (index >= _effectList.Count))
            {
                return this;
            }

            Gorgon2DCompositionPass pass = _effectList[index];
            _effectList.Remove(index);
            _effects.Remove(pass.Name);
            return this;
        }

        /// <summary>
        /// Function to remove the first matching pass in the effect chain.
        /// </summary>
        /// <param name="effectName">The name of the pass to remove.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor RemovePass(string effectName)
        {
            if (string.IsNullOrWhiteSpace(effectName))
            {
                return this;
            }

            if (!_effects.TryGetValue(effectName, out int index))
            {
                return this;
            }

            RemovePass(_effectList[index]);
            return this;
        }

        /// <summary>
        /// Function to remove the first matching pass in the effect chain.
        /// </summary>
        /// <param name="pass">The pass to remove.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor RemovePass(Gorgon2DCompositionPass pass)
        {
            if (pass == null)
            {
                return this;
            }

            if (!_effects.TryGetValue(pass.Name, out int index))
            {
                return this;
            }

            _effectList.Remove(index);
            _effects.Remove(pass.Name);
            return this;
        }

        /// <summary>
        /// Function to add an effect, and an optional rendering action to the compositor queue.
        /// </summary>
        /// <param name="pass">The effect pass to add.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pass"/> parameter is <b>null</b>.</exception>
        /// <seealso cref="Gorgon2DCompositionPass"/>
        public Gorgon2DCompositor Pass(Gorgon2DCompositionPass pass)
        {
            if (pass == null)
            {
                throw new ArgumentNullException(nameof(pass));
            }

            if (_effects.TryGetValue(pass.Name, out int prevIndex))
            {
                _effectList[prevIndex] = pass;
            }
            else
            {
                _effects[pass.Name] = _effectList.Count;
                _effectList.Add(pass);
            }

            return this;
        }

        /// <summary>
        /// Function to add an effect, and an optional rendering action to the compositor queue.
        /// </summary>
        /// <param name="name">A name for the pass.</param>
        /// <param name="effect">The effect to add as a pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="effect"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public Gorgon2DCompositor EffectPass(string name, Gorgon2DEffect effect)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            return Pass(new Gorgon2DCompositionPass(name, effect));
        }

        /// <summary>
        /// Function to render a pass without applying any effect.
        /// </summary>
        /// <param name="name">A name for the pass.</param>
        /// <param name="renderMethod">The method used to render the scene.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="renderMethod"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This allows an application to render a scene without applying any kind of effect.  The <paramref name="renderMethod"/> is a method that will take the following parameters:
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
        /// </para>
        /// </remarks>
        public Gorgon2DCompositor RenderPass(string name, Action<GorgonTexture2DView, int, int, DX.Size2> renderMethod)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (renderMethod == null)
            {
                throw new ArgumentNullException(nameof(renderMethod));
            }

            if (_effects.TryGetValue(name, out int prevIndex))
            {
                _effectList[prevIndex] = new Gorgon2DCompositionPass(name)
                                         {
                                             RenderMethod = renderMethod
                                         };
            }
            else
            {
                _effects[name] = _effectList.Count;
                _effectList.Add(new Gorgon2DCompositionPass(name)
                                {
                                    RenderMethod = renderMethod
                                });
            }

            return this;
        }

        /// <summary>
        /// Function to move the pass with the specified name to a new location in the list.
        /// </summary>
        /// <param name="name">The name of the pass to move.</param>
        /// <param name="newPassIndex">The new index for the pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor MovePass(string name, int newPassIndex)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this;
            }

            return !_effects.TryGetValue(name, out int passIndex) ? this : MovePass(passIndex, newPassIndex);
        }

        /// <summary>
        /// Function to move the specified pass to a new location in the list.
        /// </summary>
        /// <param name="pass">The pass to move.</param>
        /// <param name="newPassIndex">The new index for the pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor MovePass(Gorgon2DCompositionPass pass, int newPassIndex)
        {
            if (pass == null)
            {
                return this;
            }

            return !_effects.TryGetValue(pass.Name, out int passIndex) ? this : MovePass(passIndex, newPassIndex);
        }

        /// <summary>
        /// Function to move the pass at the specified index to a new location in the list.
        /// </summary>
        /// <param name="passIndex">The index of the pass to move.</param>
        /// <param name="newPassIndex">The new index for the pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor MovePass(int passIndex, int newPassIndex)
        {
            passIndex = passIndex.Max(0).Min(_effects.Count - 1);
            newPassIndex = newPassIndex.Max(0).Min(_effects.Count);

            if (newPassIndex == passIndex)
            {
                return this;
            }

            Gorgon2DCompositionPass pass = _effectList[passIndex];

            _effectList[passIndex] = null;
            _effectList.Insert(newPassIndex, pass);
            _effectList.Remove((Gorgon2DCompositionPass)null);

            for (int i = 0; i < _effectList.Count; ++i)
            {
                pass = _effectList[i];
                _effects[pass.Name] = i;
            }

            return this;
        }

        /// <summary>
        /// Function to render the scene for the compositor effects.
        /// </summary>
        /// <param name="output">The final output render target for the compositor.</param>
        /// <param name="renderMethod">The method used to render the initial scene.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor Render(GorgonRenderTargetView output,
                                            Action renderMethod)
        {
            output.ValidateObject(nameof(output));
            renderMethod.ValidateObject(nameof(renderMethod));

            // Create or update our resources
            if ((_final != output) || (NeedsResourceUpdate(output)))
            {
                FreeResources();
                CreateResources(output);
            }

            // Create the batch state that we'll use for our initial scene.
            if (_hasBatchStateChanged)
            {
                _batchState = _batchStateBuilder.Build();
                _hasBatchStateChanged = false;
            }

            if (_hasFinalBatchStateChanged)
            {
                _finalBatchState = _finalBatchStateBuilder.Build();
                _hasFinalBatchStateChanged = false;
            }

            RenderInitalScene(renderMethod);

            // If we have no effects, then, just output the scene to the render target as-is.
            if (_effects.Count == 0)
            {
                CopyToFinal(_originalTexture);
                return this;
            }

            (GorgonRenderTargetView target, GorgonTexture2DView texture) current = (_pingTarget, _originalTexture);

            // Iterate through our items.
            for (int i = 0; i < _effectList.Count; ++i)
            {
                Gorgon2DCompositionPass pass = _effectList[i];

                if (!pass.Enabled)
                {
                    continue;
                }

                GorgonRenderTargetView currentTarget = current.target;
                GorgonTexture2DView currentTexture = current.texture;
                GorgonTexture2DView nextTexture = ((currentTexture == _originalTexture) || (currentTexture == _pongTexture)) ? _pingTexture : _pongTexture;
                GorgonRenderTarget2DView nextTarget = current.target == _pingTarget ? _pongTarget : _pingTarget;

                if (pass.ClearColor != null)
                {
                    currentTarget.Clear(pass.ClearColor.Value);
                }

                if (pass.Effect != null)
                {
                    RenderEffectPass(pass, currentTarget, currentTexture);
                }
                else
                {
                    RenderNoEffectPass(pass, currentTarget, currentTexture);
                }

                current = (nextTarget, nextTexture);
            }

            CopyToFinal(current.texture);

            return this;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DCompositor"/> class.
        /// </summary>
        /// <param name="renderer">The renderer to use when rendering the effects.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public Gorgon2DCompositor(Gorgon2D renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Graphics = renderer.Graphics;
            _batchStateBuilder.BlendState(GorgonBlendState.NoBlending);
        }
        #endregion

    }
}
