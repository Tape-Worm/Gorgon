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

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that displaces the pixels on an image using the pixels from another image for weighting.
    /// </summary>
    public class Gorgon2DDisplacementEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Variables.
        // The shader used for displacement.
        private GorgonPixelShader _displacementShader;
        private Gorgon2DShaderState<GorgonPixelShader> _displacementState;
        // Information for building render targets.
        private GorgonTexture2DInfo _targetInfo;
        // The displacement render targets.
        private GorgonRenderTarget2DView _displacementTarget;
        private GorgonRenderTarget2DView _workerTarget;
        // The displacement texture view.
        private GorgonTexture2DView _displacementView;
        private GorgonTexture2DView _workerView;
        // The constant buffer for displacement settings.
        private GorgonConstantBufferView _displacementSettingsBuffer;
        // Flag to indicate that the parameters have been updated.
        private bool _isUpdated = true;
        // Strength of the displacement map.
        private float _displacementStrength = 0.25f;
        // The batch state.
        private Gorgon2DBatchState _batchState;
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

        /// <summary>
        /// Property to set or return the sampler state to use when displacing texture data.
        /// </summary>
        public GorgonSamplerState DisplacementSampler
        {
            get;
            set;
        } = GorgonSamplerState.Default;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the displacement map render target.
        /// </summary>
        /// <param name="output">The final output render target.</param>
        /// <param name="isSizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        private void UpdateDisplacementMap(GorgonRenderTargetView output, bool isSizeChanged)
        {
#if DEBUG
            if (!Graphics.FormatSupport[output.Format].IsRenderTargetFormat)
            {
                throw new GorgonException(GorgonResult.CannotWrite,
                                          string.Format(Resources.GOR2D_ERR_EFFECT_DISPLACEMENT_UNSUPPORTED_FORMAT, output.Format));
            }
#endif

            if (_targetInfo is null)
            {
                _targetInfo = new GorgonTexture2DInfo(output.Width, output.Height, output.Format)
                {
                    Name = "Effect.Displacement.RT",
                    Binding = TextureBinding.ShaderResource
                };
            }
            else if (isSizeChanged)
            {
#if NET5_0_OR_GREATER
                _targetInfo = _targetInfo with
                {
                    Width = output.Width,
                    Height = output.Height
                };
#endif
            }

            _displacementTarget = Graphics.TemporaryTargets.Rent(_targetInfo, "Effect.Displacement.RT");
            _displacementView = _displacementTarget.GetShaderResourceView();

            _workerTarget = Graphics.TemporaryTargets.Rent(_displacementTarget, "Effect.Displacement.Worker");
            _workerView = _workerTarget.GetShaderResourceView();

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
                                                                                        GorgonConstantBufferInfo(Unsafe.SizeOf<Vector4>())
                                                                                        {
                                                                                            Name = "Gorgon2DDisplacementEffect Constant Buffer",
                                                                                            Usage = ResourceUsage.Dynamic                                                                                            
                                                                                        });

            _displacementShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderDisplacementDecoder");
        }

        /// <summary>
        /// Function to free any resources allocated by the effect.
        /// </summary>
        public void FreeResources()
        {
            if (_displacementTarget is not null)
            {
                Graphics.TemporaryTargets.Return(_displacementTarget);
            }

            if (_workerTarget is not null)
            {
                Graphics.TemporaryTargets.Return(_workerTarget);
            }

            _displacementState = null;
            _batchState = null;
            _workerTarget = _displacementTarget = null;
            _workerView = _displacementView = null;            
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
            UpdateDisplacementMap(output, sizeChanged);

            if (!_isUpdated)
            {
                return;
            }

            var settings = new Vector4(1.0f / output.Width, 1.0f / output.Height, _displacementStrength * 100, 0);
            _displacementSettingsBuffer.Buffer.SetData(in settings);
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
        protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, GorgonCameraCommon camera)
        {
            if (passIndex == 0)
            {
                _workerTarget.Clear(GorgonColor.BlackTransparent);
                _displacementTarget.Clear(GorgonColor.BlackTransparent);                
            }

            Graphics.SetRenderTarget(output, Graphics.DepthStencilView);

            return PassContinuationState.Continue;
        }


        /// <summary>Function called after rendering is complete.</summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks>
        /// This method is called after all passes are finished and the effect is ready to complete its rendering. Developers should override this method to finalize any custom rendering. For example
        /// an effect author can use this method to render the final output of an effect to the final render target.
        /// </remarks>
        protected override void OnAfterRender(GorgonRenderTargetView output)
        {            
            Renderer.Begin(Gorgon2DBatchState.PremultipliedBlend);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height), GorgonColor.White, _workerView);
            Renderer.End();

            FreeResources();
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

            if ((_batchState is null) || (statesChanged))
            {
                if (_displacementState is null)
                {
                    _displacementState = builders.PixelShaderBuilder.Clear()
                                          .Shader(_displacementShader)
                                          .ConstantBuffer(_displacementSettingsBuffer, 1)
                                          .ShaderResource(_displacementView, 1)
                                          .Build(PixelShaderAllocator);
                }

                _batchState = builders.BatchBuilder.Clear()                                
                              .PixelShaderState(_displacementState)
                              .BlendState(GorgonBlendState.Premultiplied)
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
        /// Function to render the specified texture as a displacement for the target texture data.
        /// </summary>
        /// <param name="texture">The texture to use as a displacement for the specified render target.</param>
        /// <param name="target">The target whos image will be displaced by the texture.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="target"/> cannot be bound as a shader resource.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="target"/> must be created with a binding of <see cref="TextureBinding.ShaderResource"/>, otherwise this method will throw an exception.
        /// </para>
        /// </remarks>
        public void Render(GorgonTexture2DView texture, GorgonRenderTarget2DView target)
        {
            if ((target.Binding & TextureBinding.ShaderResource) != TextureBinding.ShaderResource)
            {
                throw new ArgumentException(Resources.GOR2D_ERR_INVALID_TARGET, nameof(target));
            }

            

            BeginRender(target, GorgonBlendState.Premultiplied);
            BeginPass(0, _displacementTarget);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, target.Width, target.Height), GorgonColor.White, texture, new DX.RectangleF(0, 0, 1, 1));
            EndPass(0, _displacementTarget);            

            BeginPass(1, _workerTarget);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, target.Width, target.Height), GorgonColor.White, target.GetShaderResourceView(), new DX.RectangleF(0, 0, 1, 1), textureSampler: DisplacementSampler);
            EndPass(1, _workerTarget);
            EndRender(target);
        }

        /// <summary>Function to render an effect under the <see cref="Gorgon2DCompositor" />.</summary>
        /// <param name="texture">The texture to render into the next target.</param>
        /// <param name="output">The render target that will receive the final output.</param>
        void IGorgon2DCompositorEffect.Render(GorgonTexture2DView texture, GorgonRenderTargetView output) => Render(texture, (GorgonRenderTarget2DView)output);
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
