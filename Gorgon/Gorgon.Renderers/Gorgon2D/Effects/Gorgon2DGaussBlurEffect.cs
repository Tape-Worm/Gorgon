﻿#region MIT
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
using System.Globalization;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
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
        : Gorgon2DEffect
    {
        #region Variables.
        // The shader used for blurring.
        private Gorgon2DShader<GorgonPixelShader> _blurShader;
        // The shader used for blurring (no alpha channel support).
        private Gorgon2DShader<GorgonPixelShader> _blurShaderNoAlpha;
        // Buffer for blur kernel data.
        private GorgonConstantBufferView _blurBufferKernel;
        // Buffer for buffer settings.
        private GorgonConstantBufferView _blurBufferPass;
        // The render target to use for a horizontal pass.
        private GorgonRenderTarget2DView _hPass;
        // The render target to use for a vertical pass.
        private GorgonRenderTarget2DView _vPass;
        // The shader resource to use for a horizontal pass.
        private GorgonTexture2DView _hPassView;
        // The shader resource to use for a horizontal pass.
        private GorgonTexture2DView _vPassView;
        // The original render target that was applied prior to rendering.
        private GorgonRenderTargetView _prev;
        // The previously active depth/stencil.
        private GorgonDepthStencil2DView _prevDepthStencil;
        // The kernel data to upload into the constant buffer.
        private GorgonNativeBuffer<float> _blurKernelData;
        // Radius for the blur.
        private int _blurRadius;												
        // The size of the render targets used to blur.
        private DX.Size2 _renderTargetSize = new DX.Size2(256, 256);
        // The format for the render targets.
        private BufferFormat _targetFormat = BufferFormat.R8G8B8A8_UNorm;
        // Flag to indicate that the kernel data needs updating.
        private bool _needKernelUpdate = true;
        // Flag to indicate that the render targets need updating.
        private bool _needTargetUpdate = true;
        // Flag to indicate that the offsets need updating.
        private bool _needOffsetUpdate = true;
        // The batch states to use when rendering this effect.
        private Gorgon2DBatchState _batchState;
        private Gorgon2DBatchState _batchStateNoAlpha;
        // The number of floats for the offset data.
        private readonly int _offsetSize;
        // The texture to blur.
        private GorgonTexture2DView _inputTexture;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the output of the effect as a texture.
        /// </summary>
        public GorgonTexture2DView Output => _vPassView;

        /// <summary>
        /// Property to return the size of the convolution kernel used to apply weighting against pixel samples when blurring.
        /// </summary>
        public int KernelSize
        {
            get;
        }

        /// <summary>
        /// Property to return the maximum size of the <see cref="BlurRadius"/> property.
        /// </summary>
        /// <remarks>
        /// This value is derived by using 1/2 of the <see cref="KernelSize"/> without the center texel.  For example, if the <see cref="KernelSize"/> is 7, then this value will be:
        /// <c>(7 - 1) / 2 = 3</c>.  We see here that the kernel size is decremented by 1, which is the equivalent of removing the center texel, and then divided by 2 giving a result of 3 texels on either 
        /// side of the center texel that can be sampled.
        /// </remarks>
        /// <seealso cref="KernelSize"/>
        public int MaximumBlurRadius
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether to blur the alpha channel or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this value is <b>true</b>, then the alpha channel will be excluded when performing the weighting when blurring. In this case, the alpha will be taken directly from the texture and applied 
        /// as-is. If it is <b>false</b>, then the alpha values have weighting applied from the blur kernel and as a result, will be blurred along with the color information.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool PreserveAlpha
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the radius of the number of pixels to sample when blurring.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property will adjust the amount of data for the blur kernel. Setting this value has the effect of increasing or lowering the amount of blur applied to the image. This value indicates how 
        /// many texels will be sampled on either side of the center texel that is being blurred.
        /// </para>
        /// <para>
        /// For example, setting the blur radius to 2 with a <see cref="KernelSize"/> of 7 will result in 2 texels being sampled on either side of the of the center texel, giving a maximum of a 5 tap 
        /// sampling:
        /// <pre>
        /// T = Center texel
        /// * = Samples
        /// 
        /// Max Kernel Size = 7, Max radius size = 3.
        /// 
        /// Radius = 3 -> ***T*** (7 taps)
        /// Radius = 2 -> **T** (5 taps)
        /// Radius = 1 -> *T* (3 taps)
        /// Radius = 0 -> Blur disabled.
        /// </pre>
        /// </para>
        /// <para>
        /// The valid range of values are within the range of 0 (no blurring) to <see cref="MaximumBlurRadius"/>.
        /// </para>
        /// <para>
        /// The default value is 1.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// Higher values will increase the quality of the blur, but will cause the shader to run slower because more samples will need to be taken. 
        /// </para>
        /// <para>
        /// For optimal performance, it is <b>not</b> recommended to adjust this value in a real time scenario as it will recalculate the weights and offsets and upload them to the shader. 
        /// </para>
        /// </note> 
        /// </para>
        /// </remarks>
        /// <seealso cref="MaximumBlurRadius"/>
        /// <seealso cref="KernelSize"/>
        public int BlurRadius
        {
            get => _blurRadius;
            set
            {
                if (_blurRadius == value)
                {
                    return;
                }

                value = value.Min(MaximumBlurRadius).Max(0);

                _blurRadius = value;
                _needKernelUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the format of the internal render targets used for blurring.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the default texture surface format for the internal render targets used for blurring. This value may be any type of format supported by a render target (see 
        /// <see cref="GorgonGraphics.FormatSupport"/> for determing an acceptable format).
        /// </para>
        /// <para>
        /// If this value is set to an unacceptable format, then the effect will throw an exception during rendering.
        /// </para>
        /// <para>
        /// The default value is <see cref="BufferFormat.R8G8B8A8_UNorm"/>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonGraphics"/>
        /// <seealso cref="BufferFormat"/>
        public BufferFormat BlurTargetFormat
        {
            get => _targetFormat;
            set
            {
                if (_targetFormat == value)
                {
                    return;
                }

                _targetFormat = value;
                _needTargetUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the size of the internal render targets used for blurring.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is used to adjust the size of a render target for more accurate blurring (at the expense of performance and video memory). 
        /// </para>
        /// <para>
        /// This value is limited to <c>3x3</c> up to the maximum width and height specified by <see cref="IGorgonVideoAdapterInfo.MaxTextureWidth"/> and <see cref="IGorgonVideoAdapterInfo.MaxTextureHeight"/>.
        /// </para>
        /// <para>
        /// The default size is <c>256x256</c>.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// Increasing the render target size will have a performance impact due to the number of texels being processed. It is recommended to scale this size based on your target video adapter(s) 
        /// capabilities.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public DX.Size2 BlurRenderTargetsSize
        {
            get => _renderTargetSize;
            set
            {
                if (_renderTargetSize == value)
                {
                    return;
                }

                // Constrain the size.
                value.Width = value.Width.Max(3).Min(Graphics.VideoAdapter.MaxTextureWidth);
                value.Height = value.Height.Max(3).Min(Graphics.VideoAdapter.MaxTextureHeight);

                _renderTargetSize = value;
                _needTargetUpdate = true;
                _needOffsetUpdate = true;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the render target.
        /// </summary>
        private void UpdateRenderTarget()
        {
            // Destroy the previous render targets.
            FreeTargets();

            // Now recreate with a new size and format (if applicable).
            _hPass = GorgonRenderTarget2DView.CreateRenderTarget(Graphics,
                                                                 new GorgonTexture2DInfo("Effect.Gauss_BlurHPass")
                                                                 {
                                                                     Format = _targetFormat,
                                                                     Width = _renderTargetSize.Width,
                                                                     Height = _renderTargetSize.Height,
                                                                     Binding = TextureBinding.ShaderResource
                                                                 });
            _hPassView = _hPass.Texture.GetShaderResourceView();

            _vPass = GorgonRenderTarget2DView.CreateRenderTarget(Graphics,
                                                                   new GorgonTexture2DInfo(_hPass, "Effect.Gauss_BlurVPass"));
            _vPassView = _vPass.Texture.GetShaderResourceView();

            _needTargetUpdate = false;
        }

        /// <summary>
        /// Function to update the offsets for the shader.
        /// </summary>
        private void UpdateOffsets()
        {
            // This adjusts just how far from the texel the blurring can occur.
            DX.Vector2 unitSize = new DX.Vector2(1.0f / BlurRenderTargetsSize.Width, 1.0f / BlurRenderTargetsSize.Height);

            int pointerOffset = 0;
            int yOffset = (((_blurRadius) * 2) + 1);

            // Store the offsets like this: xxxxx yyyyy, where x is the horizontal offset and y is the vertical offset.
            // This way our shader can read it linearly per pass.
            for (int i = -_blurRadius; i <= _blurRadius; i++)
            {
                _blurKernelData[pointerOffset] = unitSize.X * i;
                _blurKernelData[pointerOffset + yOffset] = unitSize.Y * i;
                pointerOffset++;
            }

            _blurBufferKernel.Buffer.SetData(_blurKernelData);
            _needOffsetUpdate = false;
        }

        /// <summary>
        /// Function to update the blur kernel weights.
        /// </summary>
        private void UpdateKernelWeights()
        {
            float sigma = _blurRadius;
            float sqSigmaDouble = 2.0f * sigma * sigma;
            float sigmaRoot = (sqSigmaDouble * (float)System.Math.PI).Sqrt();
            float total = 0.0f;
            int blurKernelSize = (_blurRadius * 2) + 1;
            int pointerOffset = _offsetSize;
            int firstPassOffset = pointerOffset;

            // Write kernel coefficients.
            for (int i = -_blurRadius; i <= _blurRadius; ++i)
            {
                float distance = i * i;
                float kernelValue = (-distance / sqSigmaDouble).Exp() / sigmaRoot;

                _blurKernelData[firstPassOffset++] = kernelValue;
                
                total += kernelValue;
            }
            
            // We need to read back the memory and average the kernel weights.
            for (int i = 0; i < blurKernelSize; ++i)
            {
                _blurKernelData[pointerOffset] = _blurKernelData[pointerOffset++] / total;
            }

            // Write out the current blur radius.
            // Store the blur radius in the last part of the buffer (minus 4 floats for float alignment rules on constant buffers).
            _blurKernelData[_blurKernelData.Length - 4] = _blurRadius;

            _needKernelUpdate = false;
            _needOffsetUpdate = true;
        }

        /// <summary>
        /// Function to free any render targets allocated by the effect.
        /// </summary>
        private void FreeTargets()
        {
            GorgonRenderTarget2DView hPass = Interlocked.Exchange(ref _hPass, null);
            GorgonRenderTarget2DView vPass = Interlocked.Exchange(ref _vPass, null);
            GorgonTexture2DView hView = Interlocked.Exchange(ref _hPassView, null);
            GorgonTexture2DView vView = Interlocked.Exchange(ref _vPassView, null);

            hView?.Dispose();
            vView?.Dispose();
            hPass?.Dispose();
            vPass?.Dispose();

            _needTargetUpdate = true;
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
        {
            return PreserveAlpha ? _batchStateNoAlpha : _batchState;
        }

        /// <summary>
        /// Function called when a pass is rendered.
        /// </summary>
        /// <param name="passIndex">The current index of the pass being rendered.</param>
        /// <param name="batchState">The current batch state for the pass.</param>
        /// <param name="camera">The current camera to use when rendering.</param>
        protected override void OnRenderPass(int passIndex, Gorgon2DBatchState batchState, Gorgon2DCamera camera)
        {
            Renderer.Begin(batchState, camera);

            switch (passIndex)
            {
                case 0:
                    Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _vPassView.Width, _vPassView.Height), GorgonColor.White, _inputTexture, new DX.RectangleF(0, 0, 1, 1));
                    break;
                case 1:
                    Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _hPassView.Width, _hPassView.Height), GorgonColor.White, _hPassView, new DX.RectangleF(0, 0, 1, 1));
                    break;
            }

            Renderer.End();
        }

        /// <summary>
        /// Function called when the effect is initialized for the first time.
        /// </summary>
        protected override void OnInitialize()
        {
            // Align sizes to 16 byte boundaries.
            _blurBufferKernel = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Effect.GorgonGaussKernelData")
                                                         {
                                                             Usage = ResourceUsage.Default,
                                                             SizeInBytes = ((sizeof(float) * KernelSize + 15) & ~15) 
                                                                           + ((sizeof(float) * _offsetSize + 15) & ~15)
                                                                           + sizeof(int)
                                                         });
            _blurKernelData = new GorgonNativeBuffer<float>(_blurBufferKernel.Buffer.SizeInBytes / sizeof(float));
            _blurKernelData.Fill(0);

            _blurBufferPass = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                            new GorgonConstantBufferInfo("Effect.GorgonGaussPassSettings")
                                                                            {
                                                                                Usage = ResourceUsage.Dynamic,
                                                                                SizeInBytes = 16
                                                                            });


            // Set up the constants used by our pixel shader.
            var shaderBuilder = new Gorgon2DShaderBuilder<GorgonPixelShader>();
            shaderBuilder.SamplerState(GorgonSamplerState.Default);
            shaderBuilder.ConstantBuffer(_blurBufferKernel, 1);
            shaderBuilder.ConstantBuffer(_blurBufferPass, 2);
            

            // A macro used to define the size of the kernel weight data structure.
            GorgonShaderMacro[] weightsMacro = {
                                   new GorgonShaderMacro("GAUSS_BLUR_EFFECT"),
                                   new GorgonShaderMacro("MAX_KERNEL_SIZE", KernelSize.ToString(CultureInfo.InvariantCulture))
                               };

            // Compile our blur shader.
            shaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
                                                                                Resources.BasicSprite,
                                                                                "GorgonPixelShaderGaussBlur",
                                                                                GorgonGraphics.IsDebugEnabled,
                                                                                weightsMacro));

            _blurShader = shaderBuilder.Build();

            shaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
                                                                                Resources.BasicSprite,
                                                                                "GorgonPixelShaderGaussBlurNoAlpha",
                                                                                GorgonGraphics.IsDebugEnabled,
                                                                                weightsMacro));

            _blurShaderNoAlpha = shaderBuilder.Build();
            
            UpdateRenderTarget();
            UpdateKernelWeights();
            UpdateOffsets();

            _batchState = BatchStateBuilder
                          .BlendState(GorgonBlendState.NoBlending)
                          .PixelShader(_blurShader)
                          .Build();

            _batchStateNoAlpha = BatchStateBuilder
                                 .BlendState(GorgonBlendState.NoBlending)
                                 .PixelShader(_blurShaderNoAlpha)
                                 .Build();
        }

        /// <summary>
        /// Function called before rendering begins.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        /// <exception cref="GorgonException">Thrown if the render target could not be created due to an incompatible <see cref="BlurTargetFormat"/>.</exception>
        protected override bool OnBeforeRender()
        {
#if DEBUG
            if ((_needTargetUpdate) && (!Graphics.FormatSupport[BlurTargetFormat].IsRenderTargetFormat))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOR2D_ERR_EFFECT_BLUR_RENDER_TARGET_FORMAT_NOT_SUPPORTED, BlurTargetFormat));
            }
#endif

            if (_needKernelUpdate)
            {
                UpdateKernelWeights();
            }

            if (_needOffsetUpdate)
            {
                UpdateOffsets();
            }

            if (!_needTargetUpdate)
            {
                _prev = Graphics.RenderTargets[0];
                _prevDepthStencil = Graphics.DepthStencilView;

                return true;
            }

            // Update the render target prior to rendering.
            UpdateRenderTarget();

            _prev = Graphics.RenderTargets[0];
            _prevDepthStencil = Graphics.DepthStencilView;
            return true;
        }

        /// <summary>
        /// Function called after rendering is complete.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Applications can use this to clean up and/or restore any states when rendering is finished.
        /// </para>
        /// </remarks>
        protected override void OnAfterRender()
        {
            // Restore the previous render target.
            _prev = Graphics.RenderTargets[0];
            _prevDepthStencil = Graphics.DepthStencilView;

            if ((_prev == Graphics.RenderTargets[0]) && (_prevDepthStencil == Graphics.DepthStencilView))
            {
                return;
            }

            Graphics.SetRenderTarget(_prev, _prevDepthStencil);
        }

        /// <summary>
        /// Function called before a rendering pass begins.
        /// </summary>
        /// <param name="passIndex">The current pass index.</param>
        /// <returns><b>true</b> to continue rendering, or <b>false</b> to skip this pass and move to the next.</returns>
        protected override bool OnBeforeRenderPass(int passIndex)
        {
            if (_blurRadius == 0)
            {
                return false;
            }

            switch (passIndex)
            {
                case 0:
                    _blurBufferPass.Buffer.SetData(ref passIndex, copyMode: CopyMode.Discard);
                    Graphics.SetRenderTarget(_hPass);
                    break;
                case 1:
                    _blurBufferPass.Buffer.SetData(ref passIndex, copyMode: CopyMode.Discard);
                    Graphics.SetRenderTarget(_vPass);
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to render this effect using the source <see cref="GorgonTexture2DView"/> and the output <see cref="GorgonRenderTargetView"/>.
        /// </summary>
        /// <param name="sourceTexture">The texture containing the image data to blur.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceTexture"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <paramref name="sourceTexture"/>, blur it, and then copy the blurred result into the texture <see cref="Output"/> property. 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled in <b>DEBUG</b> mode.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonRenderTarget2DView"/>
        /// <seealso cref="GorgonTexture2D"/>
        public void Blur(GorgonTexture2DView sourceTexture)
        {
            sourceTexture.ValidateObject(nameof(sourceTexture));

            _inputTexture = sourceTexture;
            Render(null, null, null);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            FreeTargets();

            Gorgon2DShader<GorgonPixelShader> blurShader = Interlocked.Exchange(ref _blurShader, null);
            Gorgon2DShader<GorgonPixelShader> blurShaderNoAlpha = Interlocked.Exchange(ref _blurShaderNoAlpha, null);
            GorgonNativeBuffer<float> kernelData = Interlocked.Exchange(ref _blurKernelData, null);
            GorgonConstantBufferView kernelBuffer = Interlocked.Exchange(ref _blurBufferKernel, null);
            GorgonConstantBufferView passBuffer = Interlocked.Exchange(ref _blurBufferPass, null);

            blurShader?.Shader.Dispose();
            blurShaderNoAlpha?.Shader.Dispose();
            kernelData?.Dispose();
            kernelBuffer?.Dispose();
            passBuffer?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DGaussBlurEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="kernelSize">[Optional] The size of the convolution kernel.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="kernelSize"/> is less than 3 or greater than 81.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="kernelSize"/> is specified, it will define the number of texels sampled (including the center) during a pass. The number of kernel taps should be an odd number for best 
        /// results. For example:
        /// <pre>
        /// * = Sample
        /// T = Center texel
        /// 
        /// 3 tap: *T*
        /// 
        /// 7 tap: ***T*** 
        /// 
        /// 9 tap: ****T****
        /// </pre>
        /// Keep in mind that the more taps requested, the more texel reads that are required and will have a negative performance impact.  
        /// </para>
        /// </remarks>
        public Gorgon2DGaussBlurEffect(Gorgon2D renderer, int kernelSize = 7)
            : base(renderer, Resources.GOR2D_EFFECT_GAUSS_BLUR, Resources.GOR2D_EFFECT_GAUSS_BLUT_DESC, 2)
        {
            if ((kernelSize < 3) 
                || (kernelSize > 81))
            {
                throw new ArgumentOutOfRangeException(nameof(kernelSize), Resources.GOR2D_ERR_EFFECT_BLUR_KERNEL_SIZE_INVALID);
            }

            KernelSize = kernelSize;
            MaximumBlurRadius = ((kernelSize - 1) / 2).Max(1);

            // Adjust offset size to start on a 4 float boundary.
            _offsetSize = ((2 * KernelSize) + 3) & ~3; 
            _blurRadius = MaximumBlurRadius;
        }
        #endregion
    }
}
