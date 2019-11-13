#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, April 09, 2012 7:14:08 AM
// 
#endregion

using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that displaces the pixels on an image using the pixels from another image for weighting.
    /// </summary>
    public class Gorgon2DDisplacementEffect
        : Gorgon2DEffect
    {
        #region Variables.
        // The shader used for displacement.
        private GorgonPixelShader _displacementShader;
        private Gorgon2DShaderState<GorgonPixelShader> _displacementState;
        // The displacement render target.
        private GorgonRenderTarget2DView _displacementTarget;
        // The displacement texture view.
        private GorgonTexture2DView _displacementView;
        // The background texture view to distort.
        private GorgonTexture2DView _backgroundView;
        // The constant buffer for displacement settings.
        private GorgonConstantBufferView _displacementSettingsBuffer;
        // Flag to indicate that the parameters have been updated.
        private bool _isUpdated = true;
        // Strength of the displacement map.
        private float _displacementStrength = 0.25f;
        // The batch state.
        private Gorgon2DBatchState _batchState;
        // The original render target prior to rendering.
        private GorgonRenderTargetView _originalTarget;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the strength of the displacement map.
        /// </summary>
        public float Strength
        {
            get => _displacementStrength;
            set
            {
                if (value < 0.0f)
                {
                    value = 0.0f;
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_displacementStrength == value)
                {
                    return;
                }

                _displacementStrength = value;
                _isUpdated = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the displacement map render target.
        /// </summary>
        /// <param name="output">The final output render target.</param>
        private void UpdateDisplacementMap(GorgonRenderTargetView output)
        {
            _displacementView?.Dispose();
            _displacementTarget?.Dispose();
            _displacementTarget = null;
            _displacementView = null;

#if DEBUG
            if (!Graphics.FormatSupport[output.Format].IsRenderTargetFormat)
            {
                throw new GorgonException(GorgonResult.CannotWrite,
                                          string.Format(Resources.GOR2D_ERR_EFFECT_DISPLACEMENT_UNSUPPORTED_FORMAT, output.Format));
            }
#endif

            _displacementTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics,
                                                                              new GorgonTexture2DInfo("Effect.Displacement.RT")
                                                                              {
                                                                                  Width = output.Width,
                                                                                  Height = output.Height,
                                                                                  Format = output.Format,
                                                                                  Binding = TextureBinding.ShaderResource
                                                                              });
            _displacementView = _displacementTarget.GetShaderResourceView();

            _displacementState = null;
            _batchState = null;

            _isUpdated = true;
        }

        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// </remarks>
	    protected override void OnInitialize()
        {
            _displacementSettingsBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                                        new
                                                                                        GorgonConstantBufferInfo("Gorgon2DDisplacementEffect Constant Buffer")
                                                                                        {
                                                                                            Usage = ResourceUsage.Dynamic,
                                                                                            SizeInBytes = DX.Vector4.SizeInBytes
                                                                                        });

            _displacementShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderDisplacementDecoder");
        }

        /// <summary>
        /// Function to free any resources allocated by the effect.
        /// </summary>
        public void FreeResources()
        {
            GorgonTexture2DView view = Interlocked.Exchange(ref _displacementView, null);
            GorgonRenderTarget2DView rtv = Interlocked.Exchange(ref _displacementTarget, null);
            view?.Dispose();
            rtv?.Dispose();
        }

        /// <summary>
        /// Function called prior to rendering.
        /// </summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
        /// targets (if applicable).
        /// </para>
        /// </remarks>
        protected override void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
        {
            if ((_displacementView == null) || (sizeChanged))
            {
                UpdateDisplacementMap(output);
            }

            if (!_isUpdated)
            {
                return;
            }

            var settings = new DX.Vector4(1.0f / output.Width, 1.0f / output.Height, _displacementStrength * 100, 0);
            _displacementSettingsBuffer.Buffer.SetData(ref settings);
            _isUpdated = false;
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
        protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, IGorgon2DCamera camera)
        {
            if (passIndex == 0)
            {
                _displacementTarget.Clear(GorgonColor.BlackTransparent);
            }

            Graphics.SetRenderTarget(output, Graphics.DepthStencilView);

            return PassContinuationState.Continue;
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
        {
            if ((_batchState == null) || (statesChanged))
            {
                if (_displacementState == null)
                {
                    _displacementState = builders.PixelShaderBuilder.Clear()
                                          .Shader(_displacementShader)
                                          .ConstantBuffer(_displacementSettingsBuffer, 1)
                                          .ShaderResource(_displacementView, 1)
                                          .Build(PixelShaderAllocator);
                }

                _batchState = builders.BatchBuilder.Clear()
                              .PixelShaderState(_displacementState)
                              .BlendState(GorgonBlendState.NoBlending)
                              .Build(BatchStateAllocator);
            }

            return passIndex == 0 ? null : _batchState;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            FreeResources();

            GorgonConstantBufferView displacementBuffer = Interlocked.Exchange(ref _displacementSettingsBuffer, null);
            GorgonPixelShader shader = Interlocked.Exchange(ref _displacementShader, null);

            displacementBuffer?.Dispose();
            shader?.Dispose();
        }

        /// <summary>
        /// Function to begin rendering the effect.
        /// </summary>
        /// <param name="backgroundTexture">The texture that will be distorted by the displacement.</param>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        public void Begin(GorgonTexture2DView backgroundTexture, GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, IGorgon2DCamera camera = null)
        {
            backgroundTexture.ValidateObject(nameof(backgroundTexture));

            _originalTarget = Graphics.RenderTargets[0];

            if (_originalTarget == null)
            {
                return;
            }

            _backgroundView = backgroundTexture;

            BeginRender(_originalTarget, blendState, depthStencilState, rasterState);
            BeginPass(0, _displacementTarget, camera);
        }

        /// <summary>
        /// Function to end the effect rendering.
        /// </summary>
        public void End()
        {
            EndPass(0, _displacementTarget);

            BeginPass(1, _originalTarget);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _originalTarget.Width, _originalTarget.Height), GorgonColor.White, _backgroundView, new DX.RectangleF(0, 0, 1, 1));
            EndPass(1, _originalTarget);

            EndRender(_originalTarget);
            _originalTarget = null;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DDisplacementEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DDisplacementEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_DISPLACEMENT, Resources.GOR2D_EFFECT_DISPLACEMENT_DESC, 2) => Macros.Add(new GorgonShaderMacro("DISPLACEMENT_EFFECT"));
        #endregion
    }
}
