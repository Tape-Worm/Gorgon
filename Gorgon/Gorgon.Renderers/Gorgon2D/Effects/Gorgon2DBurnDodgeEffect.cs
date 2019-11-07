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

using System;
using System.Threading;
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
        : Gorgon2DEffect
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

            _dodgeBurnShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderLinearBurnDodge");
            _linearDodgeBurnShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderBurnDodge");

            _linearDodgeBurn = PixelShaderBuilder
                               .Shader(_linearDodgeBurnShader)
                               .ConstantBuffer(_burnDodgeBuffer, 1)
                               .Build();

            _dodgeBurn = PixelShaderBuilder
                         .Shader(_dodgeBurnShader)
                         .Build();

            _batchStateLinearDodgeBurn = BatchStateBuilder.PixelShaderState(_linearDodgeBurn)
                                                          .Build();
            _batchStateDodgeBurn = BatchStateBuilder.PixelShaderState(_dodgeBurn)
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
                _batchStateLinearDodgeBurn = BatchStateBuilder.Build();
                _batchStateDodgeBurn = BatchStateBuilder.Build();
            }

            return UseLinear ? _batchStateLinearDodgeBurn : _batchStateDodgeBurn;
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
            }

            if (!_isUpdated)
            {
                return;
            }

            _burnDodgeBuffer.Buffer.SetData(ref _useDodge);
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
        protected override void OnRenderPass(int passIndex, Action<int, DX.Size2> renderMethod, GorgonRenderTargetView output) => renderMethod(passIndex, new DX.Size2(output.Width, output.Height));

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
