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
// Created: Monday, April 02, 2012 2:59:16 PM
// 
#endregion

using System.Threading;
using DX = SharpDX;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that sharpens (and optionally embosses) an image.
    /// </summary>
    public class Gorgon2DSharpenEmbossEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Variables.
        // Constant buffer for the sharpen/emboss information.
        private GorgonConstantBufferView _sharpenEmbossBuffer;
        // Pixel shader used to sharpen an image.
        private GorgonPixelShader _sharpenShader;
        private Gorgon2DShaderState<GorgonPixelShader> _sharpenState;
        // Pixel shader used to emboss an image.
        private GorgonPixelShader _embossShader;
        private Gorgon2DShaderState<GorgonPixelShader> _embossState;
        // The batch render state for sharpening.
        private Gorgon2DBatchState _sharpenBatchState;
        // The batch render state for embossing.
        private Gorgon2DBatchState _embossBatchState;
        // Amount to sharpen/emboss
        private float _amount = 0.5f;
        // Flag to indicate that the parameters were updated.
        private bool _isUpdated = true;
        // The texture size used to calculate the emboss/sharpen edges.
        private DX.Size2F _textureSize = new DX.Size2F(512.0f, 512.0f);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the offset of the shapren/embossing edges.
        /// </summary>
        public DX.Size2F TextureSize
        {
            get => _textureSize;
            set
            {
                if (_textureSize == value)
                {
                    return;
                }

                _textureSize = value;
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return whether to use embossing instead of sharpening.
        /// </summary>
        public bool UseEmbossing
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the amount to sharpen/emboss.
        /// </summary>
        public float Amount
        {
            get => _amount;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_amount == value)
                {
                    return;
                }

                _amount = value;
                _isUpdated = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
        /// </remarks>
        protected override void OnInitialize()
        {
            _sharpenEmbossBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Gorgon 2D Sharpen/Emboss Constant Buffer")
            {
                SizeInBytes = 16
            });
            _sharpenShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderSharpen");
            _embossShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderEmboss");
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
            // ReSharper disable once InvertIf
            if ((_sharpenBatchState == null) || (_embossBatchState == null) || (statesChanged))
            {
                if (_sharpenState == null)
                {
                    _sharpenState = builders.PixelShaderBuilder
                                     .ConstantBuffer(_sharpenEmbossBuffer, 1)
                                     .Shader(_sharpenShader)
                                     .Build();
                }

                if (_embossState == null)
                {
                    _embossState = builders.PixelShaderBuilder
                                    .ConstantBuffer(_sharpenEmbossBuffer, 1)
                                    .Shader(_embossShader)
                                    .Build();
                }


                _sharpenBatchState = builders.BatchBuilder
                                     .PixelShaderState(_sharpenState)
                                     .Build(BatchStateAllocator);
                _embossBatchState = builders.BatchBuilder
                                    .PixelShaderState(_embossState)
                                    .Build(BatchStateAllocator);
            }

            return UseEmbossing ? _embossBatchState : _sharpenBatchState;
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

            var settings = new DX.Vector3(1.0f / _textureSize.Width, 1.0f / _textureSize.Height, _amount);

            _sharpenEmbossBuffer.Buffer.SetData(ref settings);
            _isUpdated = false;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            GorgonConstantBufferView buffer = Interlocked.Exchange(ref _sharpenEmbossBuffer, null);
            GorgonPixelShader shader1 = Interlocked.Exchange(ref _sharpenShader, null);
            GorgonPixelShader shader2 = Interlocked.Exchange(ref _embossShader, null);

            buffer?.Dispose();
            shader1?.Dispose();
            shader2?.Dispose();
        }

        /// <summary>
        /// Function to begin rendering the effect.
        /// </summary>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        public void Begin(GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, IGorgon2DCamera camera = null)
        {
            GorgonRenderTargetView target = Graphics.RenderTargets[0];

            if (target == null)
            {
                return;
            }

            BeginRender(target, blendState, depthStencilState, rasterState);
            BeginPass(0, target, camera);
        }

        /// <summary>
        /// Function to end the effect rendering.
        /// </summary>
        public void End()
        {
            GorgonRenderTargetView target = Graphics.RenderTargets[0];

            if (target == null)
            {
                return;
            }

            EndPass(0, target);
            EndRender(target);
        }

        /// <summary>
        /// Function to render an effect under the <see cref="Gorgon2DCompositor"/>.
        /// </summary>
        /// <param name="texture">The texture to render into the next target.</param>
        /// <param name="output">The render target that will receive the final output.</param>
        public void Render(GorgonTexture2DView texture, GorgonRenderTargetView output)
        {
            if ((texture == null) || (output == null))
            {
                return;
            }

            Graphics.SetRenderTarget(output);

            Begin(GorgonBlendState.Default, GorgonDepthStencilState.Default, GorgonRasterState.Default, null);

            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height),
                                            GorgonColor.White,
                                            texture,
                                            new DX.RectangleF(0, 0, 1, 1));
            End();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DSharpenEmbossEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DSharpenEmbossEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_SHARPEMBOSS, Resources.GOR2D_EFFECT_SHARPEMBOSS_DESC, 1) => Macros.Add(new GorgonShaderMacro("SHARPEN_EMBOSS_EFFECT"));
        #endregion
    }
}
