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
        // The constant buffer for displacement settings.
        private GorgonConstantBufferView _displacementSettingsBuffer;
        // Flag to indicate that the parameters have been updated.
        private bool _isUpdated = true;
        // Strength of the displacement map.
        private float _displacementStrength = 0.25f;
        // The batch states.
        private Gorgon2DBatchState _batchState;
        // Flag to indicate that chromatic aberration should be applied.
        private bool _chromatic = true;
        // The scale of the chromatic aberration color channel separation.
        private Vector2 _chromaAbScale = new(0.5f, 0);
        // The currently active render target view.
        private GorgonRenderTargetView _currentRtv;
        // The texture that will be displaced by the objects being rendered.
        private GorgonTexture2DView _displaceTexture;
        // The output render target.
        private GorgonRenderTarget2DView _output;
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
        protected override void OnAfterRender(GorgonRenderTargetView output) =>_currentRtv = null; 

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
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
        {
            if ((statesChanged) || (_displacementState is null) || (_batchState is null))
            {
                if (_displacementState is null)
                {
                    _displacementState = builders.PixelShaderBuilder.Clear()
                                          .Shader(_displacementShader)
                                          .ConstantBuffer(_displacementSettingsBuffer, 1)
                                          .ShaderResource(_displaceTexture, 1)
                                          .Build(PixelShaderAllocator);
                }

                _batchState = builders.BatchBuilder
                                .PixelShaderState(_displacementState)
                                .Build(BatchStateAllocator);
            }

            return _batchState;
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

            GorgonConstantBufferView displacementBuffer = Interlocked.Exchange(ref _displacementSettingsBuffer, null);
            GorgonPixelShader shader = Interlocked.Exchange(ref _displacementShader, null);

            displacementBuffer?.Dispose();
            shader?.Dispose();
        }        

        /// <summary>
        /// Function to begin a batch for rendering the objects used to displace the target.
        /// </summary>
        /// <param name="backgroundTexture">The texture that will be displaced.</param>
        /// <param name="output">The output render target that will receive the displaced rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <returns><b>true</b> if the pass can continue, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// This is the first pass in the effect. It will receive rendering using objects like sprites, primitives, etc... that will be used to displace the pixels on an image in the next pass.
        /// </para>
        /// <para>
        /// When this method is called, the <see cref="EndDisplacementBatch"/> method <b>must</b> be called prior to moving to the next pass. If this is not done, an exception will be thrown. If this method 
        /// returns <b>false</b>, the <see cref="EndDisplacementBatch"/> still must be called.
        /// </para>
        /// </remarks>
        public bool BeginDisplacementBatch(GorgonTexture2DView backgroundTexture, GorgonRenderTarget2DView output, GorgonDepthStencilState depthStencilState = null, GorgonCameraCommon camera = null)
        {
            _currentRtv = Graphics.RenderTargets[0];
            _displaceTexture = backgroundTexture;
            _output = output;

            // No texture? Nothing to displace.
            if ((_displaceTexture is null) || (output is null))
            {
                return false;
            }

            BeginRender(_currentRtv, depthStencilState: depthStencilState);

            if (BeginPass(0, output, camera) != PassContinuationState.Continue)
            {
                EndRender(_currentRtv);                
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Function to end the batch.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method must be called after the <see cref="BeginDisplacementBatch"/>, and any custom rendering is called. 
        /// </para>
        /// </remarks>
        public void EndDisplacementBatch()
        {
            EndPass(0, _output);
            EndRender(_currentRtv);

            _currentRtv = null;
            _displaceTexture = null;
            _output = null;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DDisplacementEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DDisplacementEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_DISPLACEMENT, Resources.GOR2D_EFFECT_DISPLACEMENT_DESC, 1) => Macros.Add(new GorgonShaderMacro("DISPLACEMENT_EFFECT"));
        #endregion
    }
}
