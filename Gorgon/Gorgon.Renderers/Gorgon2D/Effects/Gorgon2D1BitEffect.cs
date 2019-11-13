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
using System.Runtime.InteropServices;
using System.Threading;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that renders an image as if it were 1 bit image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This effect renders a 1-bit color image by using a <see cref="Threshold"/> to determine which bit is on, and which is off.  If a color value falls within the <see cref="Threshold"/>, then a bit is 
    /// set as on, otherwise it will be set as off.
    /// </para>
    /// </remarks>
    public class Gorgon2D1BitEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Value Types.
        /// <summary>
        /// Settings for the effect shader.
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        private struct Settings
        {
            [FieldOffset(0)]
            private readonly int _useAverage;           // Flag to indicate that the average of the texel colors should be used.
            [FieldOffset(4)]
            private readonly int _invert;               // Flag to invert the texel colors.
            [FieldOffset(8)]
            private readonly int _useAlpha;             // Flag to indicate that the alpha channel should be included.

            /// <summary>
            /// Range of values that are considered "on".
            /// </summary>
            [FieldOffset(16)]
            public readonly GorgonRangeF WhiteRange;

            /// <summary>
            /// Flag to indicate that the average of the texel colors should be used.
            /// </summary>
            public bool UseAverage => _useAverage != 0;

            /// <summary>
            /// Flag to invert the texel colors.
            /// </summary>
            public bool Invert => _invert != 0;

            /// <summary>
            /// Flag to indicate that the alpha channel should be included.
            /// </summary>
            public bool UseAlpha => _useAlpha != 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="Settings"/> struct.
            /// </summary>
            /// <param name="range">The range.</param>
            /// <param name="average">if set to <b>true</b> [average].</param>
            /// <param name="invert">if set to <b>true</b> [invert].</param>
            /// <param name="useAlpha">if set to <b>true</b> [use alpha].</param>
            public Settings(GorgonRangeF range, bool average, bool invert, bool useAlpha)
            {
                WhiteRange = range;
                _useAverage = Convert.ToInt32(average);
                _invert = Convert.ToInt32(invert);
                _useAlpha = Convert.ToInt32(useAlpha);
            }
        }
        #endregion

        #region Variables.
        // Constant buffer for the 1 bit information.
        private GorgonConstantBufferView _1BitBuffer;
        // Settings for the effect.
        private Settings _settings;
        // Flag to indicate that the parameters were updated.
        private bool _isUpdated = true;
        // The pixel shader for the effect.
        private GorgonPixelShader _shader;
        // The shader used to render the image.
        private Gorgon2DShaderState<GorgonPixelShader> _shaderState;
        // The batch state to render.
        private Gorgon2DBatchState _batchState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to use an average of the texel colors or to use a grayscale calculation.
        /// </summary>
        public bool UseAverage
        {
            get => _settings.UseAverage;
            set
            {
                if (_settings.UseAverage == value)
                {
                    return;
                }

                _settings = new Settings(_settings.WhiteRange, value, _settings.Invert, _settings.UseAlpha);
                _isUpdated = true;
            }
        }


        /// <summary>
        /// Property to set or return whether the alpha channel should be included in the conversion.
        /// </summary>
        public bool ConvertAlphaChannel
        {
            get => _settings.UseAlpha;
            set
            {
                if (_settings.UseAlpha == value)
                {
                    return;
                }

                _settings = new Settings(_settings.WhiteRange, _settings.UseAverage, _settings.Invert, value);
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return whether to invert the texel colors.
        /// </summary>
        public bool Invert
        {
            get => _settings.Invert;
            set
            {
                if (_settings.Invert == value)
                {
                    return;
                }

                _settings = new Settings(_settings.WhiteRange, _settings.UseAverage, value, _settings.UseAlpha);
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return the range of values that are considered to be "on".
        /// </summary>
        public GorgonRangeF Threshold
        {
            get => _settings.WhiteRange;
            set
            {
                if (_settings.WhiteRange.Equals(value))
                {
                    return;
                }

                _settings = new Settings(value, _settings.UseAverage, _settings.Invert, _settings.UseAlpha);
                _isUpdated = true;
            }
        }
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
        protected override void OnInitialize()
        {
            _1BitBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _settings, "Gorgon2D1BitEffect Constant Buffer");
            _shader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShader1Bit");
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

            _1BitBuffer.Buffer.SetData(ref _settings);
            _isUpdated = false;
        }

        /// <summary>Function called to build a new (or return an existing) 2D batch state.</summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="statesChanged">
        ///   <b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
        {
            if ((statesChanged) || (_batchState == null))
            {
                if (_shaderState == null)
                {
                    _shaderState = builders.PixelShaderBuilder.ConstantBuffer(_1BitBuffer, 1)
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

            GorgonConstantBufferView buffer = Interlocked.Exchange(ref _1BitBuffer, null);
            GorgonPixelShader shader = Interlocked.Exchange(ref _shader, null);

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
        /// Initializes a new instance of the <see cref="Gorgon2D1BitEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw the effect.</param>
        public Gorgon2D1BitEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_1BIT, Resources.GOR2D_EFFECT_1BIT_DESC, 1)
        {
            _settings = new Settings(new GorgonRangeF(0.5f, 1.0f), false, false, true);
            Macros.Add(new GorgonShaderMacro("GRAYSCALE_EFFECT"));
            Macros.Add(new GorgonShaderMacro("ONEBIT_EFFECT"));
        }
        #endregion
    }
}
