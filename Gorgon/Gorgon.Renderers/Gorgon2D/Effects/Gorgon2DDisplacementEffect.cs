﻿#region MIT.
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

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        : Gorgon2DEffect
    {
        #region Value Type.
        /// <summary>
        /// The constant buffer GPU data.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        private struct GpuData
        {
            /// <summary>
            /// The number of bytes for the structure.
            /// </summary>
            public static readonly int SizeInBytes = Unsafe.SizeOf<GpuData>();

            /// <summary>
            /// The size of a texel.
            /// </summary>
            [FieldOffset(0)]
            public Vector2 TexelSize;
            /// <summary>
            /// The strength of displacement.
            /// </summary>
            [FieldOffset(8)]
            public float Strength;
            /// <summary>
            /// Flag to enable or disable chromatic aberration.
            /// </summary>
            [FieldOffset(12)]
            public float ChromAbEnable;
            /// <summary>
            /// The scale between color channels when chromatic aberration is activated.
            /// </summary>
            [FieldOffset(16)]
            public Vector2 ChromaAbScale;
        }
        #endregion

        #region Variables.
        // The shader used for displacement.
        private GorgonPixelShader _displacementShader;
        private Gorgon2DShaderState<GorgonPixelShader> _displacementState;
        // Information for building render targets.
        private GorgonTexture2DInfo _targetInfo;
        // The displacement render targets.
        private GorgonRenderTarget2DView _displacementTarget;
        // The displacement texture view.
        private GorgonTexture2DView _displacementView;
        // The constant buffer for displacement settings.
        private GorgonConstantBufferView _displacementSettingsBuffer;
        // Flag to indicate that the parameters have been updated.
        private bool _isUpdated = true;
        // Strength of the displacement map.
        private float _displacementStrength = 0.25f;
        // The batch state.
        private Gorgon2DBatchState _batchState;
        // Flag to indicate that chromatic aberration should be applied.
        private bool _chromatic = true;
        // The scale of the chromatic aberration color channel separation.
        private Vector2 _chromaAbScale = new(0.5f, 0);
        // The currently active render target view.
        private GorgonRenderTargetView _currentRtv;
        // The current pass.
        private int _currentPass = -1;
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
        /// Property to set or return the scaling for the color channel separation when chromatic aberration is applied.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This scales the distance between color channels (along with <see cref="Strength"/>). 
        /// </para>
        /// <para>
        /// If the <see cref="ChromaticAberration"/> value is <b>false</b>, the this value is ignored.
        /// </para>
        /// <para>
        /// The default value is <c>(0.5f, 0.0f)</c>.
        /// </para>
        /// </remarks>
        public Vector2 ChromaticAberrationScale
        {
            get => _chromaAbScale;
            set
            {
                if (_chromaAbScale == value)
                {
                    return;
                }

                _chromaAbScale = value;
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to turn chromatic aberration on or off.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this value to <b>false</b> may improve performance.
        /// </para>
        /// <para>
        /// The default value is <b>true</b>.
        /// </para>
        /// </remarks>
        public bool ChromaticAberration
        {
            get => _chromatic;
            set
            {
                if (_chromatic == value)
                {
                    return;
                }

                _chromatic = value;
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
                                                                                        GorgonConstantBufferInfo(GpuData.SizeInBytes)
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

            _displacementState = null;
            _batchState = null;
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
            if (output is null)
            {
                return;
            }

            UpdateDisplacementMap(output, sizeChanged);

            if (!_isUpdated)
            {
                return;
            }

            GpuData data = new()
            {
                TexelSize = new Vector2(1.0f / output.Width, 1.0f / output.Height),
                Strength = _displacementStrength * 100,
                ChromAbEnable = _chromatic ? 1 : 0,
                ChromaAbScale = _chromaAbScale
            };

            _displacementSettingsBuffer.Buffer.SetData(in data);            
            _isUpdated = false;
        }

        /// <summary>Function called after rendering is complete.</summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks>
        /// This method is called after all passes are finished and the effect is ready to complete its rendering. Developers should override this method to finalize any custom rendering. For example
        /// an effect author can use this method to render the final output of an effect to the final render target.
        /// </remarks>
        protected override void OnAfterRender(GorgonRenderTargetView output)
        {
            _currentPass = -1;
            _currentRtv = null;

            FreeResources();
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
                output.Clear(GorgonColor.BlackTransparent);                
            }

            Graphics.SetRenderTarget(output, Graphics.DepthStencilView);

            return PassContinuationState.Continue;
        }

        /// <summary>
        /// Function called after a pass is finished rendering.
        /// </summary>
        /// <param name="passIndex">The index of the pass that was rendered.</param>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks><para>
        /// This method is called after rendering a single pass. Developers may use this to perform any clean up required, or any last minute rendering at the end of a pass.
        /// </para>
        /// <para>
        ///   <note type="important">
        ///     <para>
        /// The <see cref="Gorgon2D" />.<see cref="Gorgon2D.End()" /> method is already called prior to this method. Developers are free to use <see cref="Gorgon2D.Begin" /> and
        /// <see cref="Gorgon2D.End" /> to perform any last minute rendering (e.g. blending multiple targets together into the <paramref name="output" />).
        /// </para>
        ///   </note>
        /// </para></remarks>
        protected override void OnAfterRenderPass(int passIndex, GorgonRenderTargetView output) => _currentPass = passIndex;

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
        /// Function to begin a batch for rendering the objects used to displace the target.
        /// </summary>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <exception cref="GorgonException">Thrown if this method is called without calling <see cref="EndDisplacementBatch"/>, or <see cref="CancelDisplacementBatch"/>.</exception>
        /// <returns><b>true</b> if the pass can continue, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This is the first pass in the effect. It will receive rendering using objects like sprites, primitives, etc... that will be used to displace the pixels on an image in the next pass.
        /// </para>
        /// <para>
        /// When this method is called, the <see cref="EndDisplacementBatch"/> method <b>must</b> be called prior to moving to the next pass. If this is not done, an exception will be thrown. If this method 
        /// returns <b>false</b>, the <see cref="EndDisplacementBatch"/> still must be called.
        /// </para>
        /// <para>
        /// Users may call <see cref="CancelDisplacementBatch"/> if they wish to cancel a current batch.
        /// </para>
        /// </remarks>
        public bool BeginDisplacementBatch(GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, GorgonCameraCommon camera = null)
        {
            // Get the current render target, this will receive output.
            if (_currentPass != -1)
            {
                throw new GorgonException(GorgonResult.CannotWrite, Resources.GOR2D_ERR_DISPLACEMENT_PASS_ALREADY_CALLED);
            }

            _currentRtv = Graphics.RenderTargets[0];

            BeginRender(_currentRtv, blendState, depthStencilState, rasterState);

            if (BeginPass(0, _displacementTarget, camera) != PassContinuationState.Continue)
            {
                EndRender(_currentRtv);                
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Function to cancel the displacement batch.
        /// </summary>
        public void CancelDisplacementBatch()
        {
            if (_currentPass == 0)
            {
                EndDisplacementBatch();
            }
            
            EndRender(_currentRtv);
        }

        /// <summary>
        /// Function to end the batch.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must be called when <see cref="BeginDisplacementBatch"/> is called. The will mark the beginning of the next pass, <see cref="Render"/>, which will render the actual displacement.
        /// </para>
        /// </remarks>
        public void EndDisplacementBatch() => EndPass(0, _displacementTarget);

        /// <summary>
        /// Function to render the displacement effect.
        /// </summary>
        /// <param name="backgroundTexture">The texture to displace.</param>
        /// <param name="target">The render target that will receive the effect rendering.</param>
        /// <exception cref="GorgonException">Thrown if the current pass is not the second pass.</exception>
        /// <remarks>
        /// <para>
        /// This method must be called after <see cref="EndDisplacementBatch"/>
        /// </para>
        /// </remarks>
        public void Render(GorgonTexture2DView backgroundTexture, GorgonRenderTargetView target)
        {
            if (_currentPass == -1)
            {
                throw new GorgonException(GorgonResult.CannotWrite, string.Format(Resources.GOR2D_ERR_INCORRECT_DISPLACEMENT_PASS, _currentPass, 1));
            }

            BeginPass(1, target);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, target.Width, target.Height), GorgonColor.White, backgroundTexture, new DX.RectangleF(0, 0, 1, 1));
            EndPass(1, target);

            EndRender(_currentRtv);
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
