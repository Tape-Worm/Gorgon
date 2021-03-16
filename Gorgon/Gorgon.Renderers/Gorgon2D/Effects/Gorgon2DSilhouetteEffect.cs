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
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that renders the silhouette of an image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This effect renders a silhouette image by using the vertex colors instead of the texture colors, while respecting the alpha of the texture (and vertex).
    /// </para>
    /// </remarks>
    public class Gorgon2DSilhouetteEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Variables.
        // The pixel shader for the effect.
        private GorgonPixelShader _shader;
        // The shader used to render the image.
        private Gorgon2DShaderState<GorgonPixelShader> _shaderState;
        // The batch state to render.
        private Gorgon2DBatchState _batchState;
        #endregion

        #region Methods.
        /// <summary>
        /// Function called to initialize the effect.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Applications must implement this method to ensure that any required resources are created, and configured for the effect.
        /// </para>
        /// </remarks>
        protected override void OnInitialize() => _shader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderSilhouettePixelShader");

        /// <summary>Function called to build a new (or return an existing) 2D batch state.</summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="statesChanged">
        ///   <b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
        {
            if ((statesChanged) || (_batchState is null))
            {
                if (_shaderState is null)
                {
                    _shaderState = builders.PixelShaderBuilder
                                                .Shader(_shader)
                                                .Build();
                }

                _batchState = builders.BatchBuilder.PixelShaderState(_shaderState)
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

            GorgonPixelShader shader = Interlocked.Exchange(ref _shader, null);

            shader?.Dispose();
        }

        /// <summary>
        /// Function to begin rendering the effect.
        /// </summary>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        public void Begin(GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, GorgonCameraCommon camera = null)
        {
            GorgonRenderTargetView target = Graphics.RenderTargets[0];

            if (target is null)
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

            if (target is null)
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
            if ((texture is null) || (output is null))
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
        /// Initializes a new instance of the <see cref="Gorgon2DSilhouetteEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw the effect.</param>
        public Gorgon2DSilhouetteEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_SILHOUETTE, Resources.GOR2D_EFFECT_SILHOUETTE_DESC, 1) => Macros.Add(new GorgonShaderMacro("SILHOUETTE_EFFECT"));
        #endregion
    }
}
