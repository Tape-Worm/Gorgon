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
// Created: Monday, April 02, 2012 1:41:47 PM
// 
#endregion

using System.Threading;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that renders as gray scale.
    /// </summary>
    public class Gorgon2DGrayScaleEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Variables.
        // The batch state to use when rendering.
        private Gorgon2DBatchState _batchState;
        // The shader used to render grayscale images.
        private GorgonPixelShader _grayScaleShader;
        private Gorgon2DShaderState<GorgonPixelShader> _grayScaleState;
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
            // Compile our blur shader.
            _grayScaleShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderGrayScale");
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

            GorgonPixelShader shader = Interlocked.Exchange(ref _grayScaleShader, null);
            shader?.Dispose();
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
                if (_grayScaleState == null)
                {
                    _grayScaleState = builders.PixelShaderBuilder
                                       .Shader(_grayScaleShader)
                                       .Build();
                }

                _batchState = builders.BatchBuilder
                              .PixelShaderState(_grayScaleState)
                              .Build(BatchStateAllocator);
            }

            return _batchState;
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
        /// Initializes a new instance of the <see cref="Gorgon2DGrayScaleEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DGrayScaleEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_GRAYSCALE, Resources.GOR2D_EFFECT_GRAYSCALE_DESC, 1) =>
            // A macro used to define the size of the kernel weight data structure.
            Macros.Add(new GorgonShaderMacro("GRAYSCALE_EFFECT"));
        #endregion
    }
}
