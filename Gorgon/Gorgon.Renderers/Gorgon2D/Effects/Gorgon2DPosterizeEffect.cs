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
// Created: Thursday, April 05, 2012 8:23:51 AM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that renders a posterized image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will perform a posterize operation, which will reduce the number of colors in the image.
    /// </para>
    /// </remarks>
    public class Gorgon2DPosterizedEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Value Types.
        /// <summary>
        /// Settings for the effect shader.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 16)]
        private struct Settings
        {
            // Flag to posterize the alpha channel.
            private readonly int _posterizeAlpha;                               

            /// <summary>
            /// Gamma for the posterization.
            /// </summary>
            public readonly float PosterizeGamma;
            /// <summary>
            /// Number of colors to reduce down to.
            /// </summary>
            public readonly int PosterizeColorCount;

            /// <summary>
            /// Property to return whether to posterize the alpha channel.
            /// </summary>
            public bool PosterizeAlpha => _posterizeAlpha != 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="Settings"/> struct.
            /// </summary>
            /// <param name="useAlpha">if set to <b>true</b> [use alpha].</param>
            /// <param name="power">The power.</param>
            /// <param name="count">The number of colors.</param>
            public Settings(bool useAlpha, float power, int count)
            {
                _posterizeAlpha = Convert.ToInt32(useAlpha);
                PosterizeGamma = power;
                PosterizeColorCount = count;
            }
        }
        #endregion

        #region Variables.
        // Buffer for the posterize effect.
        private GorgonConstantBufferView _posterizeBuffer;
        // The shader used to render the effect.
        private GorgonPixelShader _posterizeShader;
        private Gorgon2DShaderState<GorgonPixelShader> _posterizeState;
        // The renderer batch state.
        private Gorgon2DBatchState _batchState;
        // Settings for the effect shader.
        private Settings _settings;
        // Flag to indicate that the parameters have been updated.
        private bool _isUpdated = true;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to posterize the alpha channel.
        /// </summary>
        public bool UseAlpha
        {
            get => _settings.PosterizeAlpha;
            set
            {
                if (_settings.PosterizeAlpha == value)
                {
                    return;
                }

                _settings = new Settings(value, _settings.PosterizeGamma, _settings.PosterizeColorCount);
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return the gamma value for the effect.
        /// </summary>
        public float Gamma
        {
            get => _settings.PosterizeGamma;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_settings.PosterizeGamma.EqualsEpsilon(value))
                {
                    return;
                }

                value = value.Max(0.1f).Min(10.0f);

                _settings = new Settings(_settings.PosterizeAlpha, value, _settings.PosterizeColorCount);
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return the number of colors to reduce down to for the effect.
        /// </summary>
        public int ColorCount
        {
            get => _settings.PosterizeColorCount;
            set
            {
                if (_settings.PosterizeColorCount == value)
                {
                    return;
                }

                value = value.Min(255).Max(2);

                _settings = new Settings(_settings.PosterizeAlpha, _settings.PosterizeGamma, value);
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
            _posterizeBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, in _settings, "Gorgon 2D Posterize Effect Constant Buffer");
            _posterizeShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderPosterize");
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
                if (_posterizeState is null)
                {
                    _posterizeState = builders.PixelShaderBuilder
                              .ConstantBuffer(_posterizeBuffer, 1)
                              .Shader(_posterizeShader)
                              .Build();
                }

                _batchState = builders.BatchBuilder
                              .PixelShaderState(_posterizeState)
                              .Build();
            }

            return _batchState;
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

            _posterizeBuffer.Buffer.SetData(in _settings);
            _isUpdated = false;
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            GorgonConstantBufferView buffer = Interlocked.Exchange(ref _posterizeBuffer, null);
            GorgonPixelShader shader = Interlocked.Exchange(ref _posterizeShader, null);

            buffer?.Dispose();
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
        /// Initializes a new instance of the <see cref="Gorgon2DPosterizedEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DPosterizedEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_POSTERIZE, Resources.GOR2D_EFFECT_POSTERIZE_DESC, 1)
        {
            _settings = new Settings(false, 1.0f, 4);
            Macros.Add(new GorgonShaderMacro("POSTERIZE_EFFECT"));
        }
        #endregion
    }
}
