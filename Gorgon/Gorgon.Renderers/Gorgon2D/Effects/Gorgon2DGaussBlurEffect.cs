#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: July 2, 2018 11:17:53 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Graphics;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A gaussian blur effect.
    /// </summary>
    public class Gorgon2DGaussBlurEffect
        : IDisposable
    {
        #region Variables.
        // The shader used for blurring.
        private Gorgon2DShader<GorgonPixelShader> _blurShader;
        // The builder used to create or update the batch state.
        private readonly Gorgon2DBatchStateBuilder _batchStateBuilder = new Gorgon2DBatchStateBuilder();
        // The offsets for blurring.
        private GorgonConstantBufferView _offsets;
        // The weights for blurring.
        private GorgonConstantBufferView _weights;
        // The render target to use for a horizontal pass.
        private GorgonRenderTarget2DView _hPass;
        // The render target to use for a vertical pass.
        private GorgonRenderTarget2DView _vPass;
        // The shader resource to use for a horizontal pass.
        private GorgonTexture2DView _hPassView;
        // The original render target that was applied prior to rendering.
        private GorgonRenderTargetView _prev;
        // The previously active depth/stencil.
        private GorgonDepthStencil2DView _prevDepthStencil;
        // The kernel data to upload into the constant buffer.
        private GorgonNativeBuffer<byte> _kernelData;
        // Calculated offsets.
        private DX.Vector4[] _xOffsets;										        
        private DX.Vector4[] _yOffsets;										        
        // Blur kernel.
        private float[] _kernel;											        
        // Radius for the blur.
        private int _blurRadius = 6;												
        // Amount to blur.
        private float _blurAmount = 3.0f;											
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the output of the effect as a texture.
        /// </summary>
        public GorgonTexture2DView Output
        {
            get;
        }

        /// <summary>
        /// Property to return the renderer used by this effect.
        /// </summary>
        public Gorgon2D Renderer
        {
            get;
        }

        /// <summary>
        /// Property to return the batch state to use when rendering this effect.
        /// </summary>
        public Gorgon2DBatchState BatchState
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to restore the original render target and depth/stencil that was in place prior to rendering.
        /// </summary>
        private void RestoreOriginalTarget()
        {
            if ((_prev != null) 
                && (Renderer.Graphics.RenderTargets[0] == _prev)
                && (Renderer.Graphics.DepthStencilView == _prevDepthStencil))
            {
                return;
            }

            Renderer.Graphics.SetRenderTarget(_prev, _prevDepthStencil);
        }

        /// <summary>
        /// Function to update the offsets for the shader.
        /// </summary>
        private void UpdateOffsets()
        {
            int index = 0;

            if (_vPass == null)
            {
                /*UpdateRenderTarget();

                Debug.Assert(_vTarget != null, "_vTarget != null");*/
            }

            var unitSize = new DX.Vector2(1.0f / _hPass.Width, 1.0f / _hPass.Height);

            for (int i = -_blurRadius; i <= _blurRadius; i++)
            {
                _xOffsets[index] = new DX.Vector4(unitSize.X * i, 0, 0, 0);
                _yOffsets[index] = new DX.Vector4(0, unitSize.Y * i, 0, 0);
                index++;
            }
        }

        /// <summary>
        /// Function to update the blur kernel.
        /// </summary>
        /// <remarks>This implementation is ported from the Java code appearing in "Filthy Rich Clients: Developing Animated and Graphical Effects for Desktop Java".</remarks>
        private void UpdateKernel()
        {
            float sigma = _blurRadius / _blurAmount;
            float sqSigmaDouble = 2.0f * sigma * sigma;
            float sigmaRoot = (sqSigmaDouble * (float)System.Math.PI).Sqrt();
            float total = 0.0f;
            int blurKernelSize = (_blurRadius * 2) + 1;

            for (int i = -_blurRadius, index = 0; i <= _blurRadius; ++i, ++index)
            {
                float distance = i * i;
                _kernel[index] = (-distance / sqSigmaDouble).Exp() / sigmaRoot;
                total += _kernel[index];
            }

            ref int radius = ref _kernelData.ReadAs<int>(0);
            radius = _blurRadius;
            
            for (int i = 0; i < blurKernelSize; i++)
            {
                ref DX.Vector4 value = ref _kernelData.ReadAs<DX.Vector4>((i * DX.Vector4.SizeInBytes) + sizeof(int));
                value = new DX.Vector4(0, 0, 0, _kernel[i] / total);
            }

            _weights.Buffer.SetData(_kernelData);
        }

        public void Render(Action<int, int> callback)
        {
            _hPass.Clear(GorgonColor.Transparent);
            
            _prev = Renderer.Graphics.RenderTargets[0];
            _prevDepthStencil = Renderer.Graphics.DepthStencilView;
            
            Renderer.Graphics.SetRenderTarget(_hPass);
            _offsets.Buffer.SetData(_xOffsets);

            Renderer.Begin(BatchState);
            callback(_hPass.Width, _hPass.Height);
            Renderer.End();

            Renderer.Graphics.SetRenderTarget(_vPass);
            _offsets.Buffer.SetData(_yOffsets);

            Renderer.Begin(BatchState);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _hPass.Width, _hPass.Height),
                                         GorgonColor.White,
                                         _hPassView,
                                         new DX.RectangleF(0, 0, 1, 1));
            Renderer.End();
            
            RestoreOriginalTarget();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            RestoreOriginalTarget();

            GorgonNativeBuffer<byte> kernelData = Interlocked.Exchange(ref _kernelData, null);
            Gorgon2DShader<GorgonPixelShader> blurShader = Interlocked.Exchange(ref _blurShader, null);
            GorgonTexture2DView hPassView = Interlocked.Exchange(ref _hPassView, null);
            GorgonConstantBufferView offsets = Interlocked.Exchange(ref _offsets, null);
            GorgonConstantBufferView weights = Interlocked.Exchange(ref _weights, null);
            GorgonRenderTarget2DView hPass = Interlocked.Exchange(ref _hPass, null);
            GorgonRenderTarget2DView vPass = Interlocked.Exchange(ref _vPass, null);
            
            kernelData?.Dispose();
            Output?.Dispose();
            hPassView?.Dispose();
            hPass?.Dispose();
            vPass?.Dispose();
            offsets?.Dispose();
            weights?.Dispose();
            blurShader?.Shader.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DGaussBlurEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <exception cref="System.ArgumentNullException">renderer</exception>
        public Gorgon2DGaussBlurEffect(Gorgon2D renderer, GorgonBlendState blendState, GorgonDepthStencilState depthstate, GorgonRasterState rasterState)
        {
            _xOffsets = new DX.Vector4[13];
            _yOffsets = new DX.Vector4[13];
            _kernel = new float[13];

            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            var shaderBuilder = new Gorgon2DShaderBuilder<GorgonPixelShader>();
            shaderBuilder.SamplerState(GorgonSamplerState.Default);
            shaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(renderer.Graphics,
                                                                                Resources.BasicSprite,
                                                                                "GorgonPixelShaderGaussBlur",
                                                                                GorgonGraphics.IsDebugEnabled));

            _offsets = GorgonConstantBufferView.CreateConstantBuffer(renderer.Graphics,
                                                                     new
                                                                     GorgonConstantBufferInfo("Gorgon2DGaussianBlurEffect Constant Buffer")
                                                                     {
                                                                         SizeInBytes = DX.Vector4.SizeInBytes * _xOffsets.Length
                                                                     });
            _weights = GorgonConstantBufferView.CreateConstantBuffer(renderer.Graphics,
                                                                     new
                                                                     GorgonConstantBufferInfo("Gorgon2DGaussianBlurEffect Static Constant Buffer")
                                                                     {
                                                                         SizeInBytes = DX.Vector4.SizeInBytes * (_kernel.Length + 1)
                                                                     });
            shaderBuilder.ConstantBuffers(new[]
                                          {
                                              null, // First slot belongs to alpha testing.
                                              _weights,
                                              _offsets
                                          });

            _hPass = GorgonRenderTarget2DView.CreateRenderTarget(renderer.Graphics,
                                                                 new GorgonTexture2DInfo("Effect.GaussBlur.Target_Horizontal")
                                                                 {
                                                                     Width = 512,
                                                                     Height = 512,
                                                                     Binding = TextureBinding.ShaderResource,
                                                                     Usage = ResourceUsage.Default,
                                                                     Format = BufferFormat.R8G8B8A8_UNorm
                                                                 });
            _hPassView = _hPass.Texture.GetShaderResourceView();
            _vPass = GorgonRenderTarget2DView.CreateRenderTarget(renderer.Graphics,
                                                                 new GorgonTexture2DInfo(_hPass, "Effect.GaussBlur.Target_Vertical"));
            Output = _vPass.Texture.GetShaderResourceView();

            BatchState = _batchStateBuilder.BlendState(blendState ?? GorgonBlendState.NoBlending)
                                           .DepthStencilState(depthstate ?? GorgonDepthStencilState.Default)
                                           .RasterState(rasterState ?? GorgonRasterState.Default)
                                           .PixelShader(shaderBuilder)
                                           .Build();
            _kernelData = new GorgonNativeBuffer<byte>(_weights.Buffer.SizeInBytes);
            UpdateOffsets();
            UpdateKernel();
        }
        #endregion
    }
}
