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

using System;
using System.Threading;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that sharpens (and optionally embosses) an image.
    /// </summary>
    public class Gorgon2DSharpenEmbossEffect
        : Gorgon2DEffect
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
        #endregion

        #region Properties.
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

            _sharpenState = PixelShaderBuilder
                             .ConstantBuffer(_sharpenEmbossBuffer, 1)
                             .Shader(_sharpenShader)
                             .Build();

            _embossState = PixelShaderBuilder
                            .Shader(_embossShader)
                            .Build();

        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
        {
            // ReSharper disable once InvertIf
            if (statesChanged)
            {
                _sharpenBatchState = BatchStateBuilder
                                     .PixelShaderState(_sharpenState)
                                     .Build();
                _embossBatchState = BatchStateBuilder
                                    .PixelShaderState(_embossState)
                                    .Build();
            }

            return UseEmbossing ? _embossBatchState : _sharpenBatchState;
        }

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
        protected override void OnBeforeRender(GorgonRenderTargetView output, IGorgon2DCamera camera, bool sizeChanged)
        {
            if (Graphics.RenderTargets[0] != output)
            {
                Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
                _isUpdated = true;
            }

            if ((!_isUpdated) && (!sizeChanged))
            {
                return;
            }

            var settings = new DX.Vector3(1.0f / output.Width, 1.0f / output.Height, _amount);

            _sharpenEmbossBuffer.Buffer.SetData(ref settings);
            _isUpdated = false;
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
        protected override void OnRenderPass(int passIndex, Action<int, int, DX.Size2> renderMethod, GorgonRenderTargetView output) => renderMethod(passIndex, PassCount, new DX.Size2(output.Width, output.Height));

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
