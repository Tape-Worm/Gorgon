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
// Created: Sunday, April 08, 2012 2:36:05 PM
// 
#endregion

using System.Threading;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that renders images burn/dodge effect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides an image post processing filter akin to the dodge and burn effect filters in Photoshop.
    /// </para>
    /// </remarks>
    public class Gorgon2DBurnDodgeEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Variables.
        // Burn/dodge buffer.
        private GorgonConstantBufferView _burnDodgeBuffer;
        // Dodge/burn shader.
        private GorgonPixelShader _dodgeBurnShader;
        private Gorgon2DShaderState<GorgonPixelShader> _dodgeBurn;
        // Linear dodge/burn shader.
        private GorgonPixelShader _linearDodgeBurnShader;
        private Gorgon2DShaderState<GorgonPixelShader> _linearDodgeBurn;
        // Flag to indicate that the effect parameters are updated.
        private bool _isUpdated = true;
        // Flag to indicate whether to use dodging or burning.
        private bool _useDodge;
        // The batch render state for a linear burn/dodge.
        private Gorgon2DBatchState _batchStateLinearDodgeBurn;
        // The batch render state.
        private Gorgon2DBatchState _batchStateDodgeBurn;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to use a burn or dodge effect.
        /// </summary>
        public bool UseDodge
        {
            get => _useDodge;
            set
            {
                if (_useDodge == value)
                {
                    return;
                }

                _useDodge = value;
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return whether to use a linear burn/dodge.
        /// </summary>
        public bool UseLinear
        {
            get;
            set;
        }
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
            _burnDodgeBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                             new GorgonConstantBufferInfo("Gorgon 2D Burn/Dodge Effect Constant Buffer")
                                                                             {
                                                                                 Usage = ResourceUsage.Default,
                                                                                 SizeInBytes = 16
                                                                             });

            _dodgeBurnShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderBurnDodge");
            _linearDodgeBurnShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderLinearBurnDodge");
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
            if ((_batchStateDodgeBurn == null) || (_batchStateLinearDodgeBurn == null) || (statesChanged))
            {

                if (_linearDodgeBurn == null)
                {
                    _linearDodgeBurn = builders.PixelShaderBuilder.Clear()
                                       .Shader(_linearDodgeBurnShader)
                                       .ConstantBuffer(_burnDodgeBuffer, 1)
                                       .Build();
                }

                if (_dodgeBurn == null)
                {
                    _dodgeBurn = builders.PixelShaderBuilder.Clear()
                                 .Shader(_dodgeBurnShader)
                                 .ConstantBuffer(_burnDodgeBuffer, 1)
                                 .Build();
                }

                _batchStateLinearDodgeBurn = builders.BatchBuilder
                                                    .PixelShaderState(_linearDodgeBurn)
                                                    .Build(BatchStateAllocator);
                _batchStateDodgeBurn = builders.BatchBuilder
                                                    .PixelShaderState(_dodgeBurn)
                                                    .Build(BatchStateAllocator);
            }

            return UseLinear ? _batchStateLinearDodgeBurn : _batchStateDodgeBurn;
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

            int value = _useDodge ? 1 : 0;
            _burnDodgeBuffer.Buffer.SetData(ref value);
            _isUpdated = false;
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

            GorgonConstantBufferView buffer = Interlocked.Exchange(ref _burnDodgeBuffer, null);
            GorgonPixelShader shader1 = Interlocked.Exchange(ref _linearDodgeBurnShader, null);
            GorgonPixelShader shader2 = Interlocked.Exchange(ref _dodgeBurnShader, null);

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
        /// Initializes a new instance of the <see cref="Gorgon2DBurnDodgeEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw this effect.</param>
        public Gorgon2DBurnDodgeEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_BURN_DODGE, Resources.GOR2D_EFFECT_BURN_DODGE_DESC, 1) => Macros.Add(new GorgonShaderMacro("BURN_DODGE_EFFECT"));
        #endregion
    }
}
