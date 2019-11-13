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
// Created: Wednesday, April 04, 2012 12:35:23 PM
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
    /// An effect that renders an inverted image.
    /// </summary>
    public class Gorgon2DInvertEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Variables.
        // Buffer for the inversion effect.
        private GorgonConstantBufferView _invertBuffer;
        // Flag to invert the alpha channel.
        private bool _invertAlpha;
        // The pixel shader for the effect.
        private GorgonPixelShader _invertShader;
        private Gorgon2DShaderState<GorgonPixelShader> _invertState;
        // The batch render state.
        private Gorgon2DBatchState _batchState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to invert the alpha channel.
        /// </summary>
        public bool InvertAlpha
        {
            get => _invertAlpha;
            set
            {
                if (_invertAlpha == value)
                {
                    return;
                }

                _invertAlpha = value;
                _invertBuffer?.Buffer.SetData(ref _invertAlpha);
            }
        }
        #endregion

        #region Methods.
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
                if (_invertState == null)
                {
                    _invertState = builders.PixelShaderBuilder
                              .ConstantBuffer(_invertBuffer, 1)
                              .Shader(_invertShader)
                              .Build();
                }

                _batchState = builders.BatchBuilder
                              .PixelShaderState(_invertState)
                              .Build(BatchStateAllocator);
            }

            return _batchState;
        }

        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
        /// </remarks>
        protected override void OnInitialize()
        {
            _invertBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _invertAlpha, "Gorgon2DInvertEffect Constant Buffer");
            _invertShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderInvert");
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
            if (Graphics.RenderTargets[0] != output)
            {
                Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            GorgonConstantBufferView buffer = Interlocked.Exchange(ref _invertBuffer, null);
            GorgonPixelShader pixelShader = Interlocked.Exchange(ref _invertShader, null);

            buffer?.Dispose();
            pixelShader?.Dispose();
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

        /// <summary>Function to render an effect under the <see cref="T:Gorgon.Renderers.Gorgon2DCompositor"/>.</summary>
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
        /// Initializes a new instance of the <see cref="Gorgon2DInvertEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw with this effect.</param>
        public Gorgon2DInvertEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_INVERT, Resources.GOR2D_EFFECT_INVERT_DESC, 1) => Macros.Add(new GorgonShaderMacro("INVERSE_EFFECT"));
        #endregion
    }
}
