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
using System.Globalization;
using System.Threading;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A gaussian blur effect.
    /// </summary>
    public class Gorgon2DGaussBlurEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Variables.
        // The shader used for blurring.
        private GorgonPixelShader _blurShader;
        private Gorgon2DShaderState<GorgonPixelShader> _blurState;
        // The shader used for blurring (no alpha channel support).
        private GorgonPixelShader _blurShaderNoAlpha;
        private Gorgon2DShaderState<GorgonPixelShader> _blurStateNoAlpha;
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
        // The kernel data to upload into the constant buffer.
        private GorgonNativeBuffer<float> _blurKernelData;
        // Radius for the blur.
        private int _blurRadius;
        // The size of the render targets used to blur.
        private DX.Size2 _renderTargetSize = new DX.Size2(256, 256);

        // Flag to indicate that the kernel data needs updating.
        private bool _needKernelUpdate = true;
        // Flag to indicate that the offsets need updating.
        private bool _needOffsetUpdate = true;
        // The batch states to use when rendering this effect.
        private Gorgon2DBatchState _batchState;
        private Gorgon2DBatchState _batchStateNoAlpha;
        // The number of floats for the offset data.
        private readonly int _offsetSize;
        // The info for the blur render targets.
        private readonly GorgonTexture2DInfo _blurRtvInfo;
        #endregion

        #region Properties.
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
        /// <see cref="GorgonGraphics.FormatSupport"/> for determining an acceptable format).
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
            get;
            set;
        } = BufferFormat.R8G8B8A8_UNorm;

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
            _blurRtvInfo.Format = BlurTargetFormat;
            _blurRtvInfo.Width = _renderTargetSize.Width;
            _blurRtvInfo.Height = _renderTargetSize.Height;

            _hPass = Graphics.TemporaryTargets.Rent(_blurRtvInfo, "GorgonGaussBlur_HPass", clearOnRetrieve: false);

            _hPassView = _hPass.GetShaderResourceView();

            _vPass = Graphics.TemporaryTargets.Rent(_blurRtvInfo, "GorgonGaussBlur_VPass", clearOnRetrieve: false);
            _vPassView = _vPass.GetShaderResourceView();
        }

        /// <summary>
        /// Function to update the offsets for the shader.
        /// </summary>
        private void UpdateOffsets()
        {
            // This adjusts just how far from the texel the blurring can occur.
            var unitSize = new DX.Vector2(1.0f / BlurRenderTargetsSize.Width, 1.0f / BlurRenderTargetsSize.Height);

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
            if (_hPass != null)
            {
                Graphics.TemporaryTargets.Return(_hPass);
            }

            if (_vPass == null)
            {
                return;
            }

            Graphics.TemporaryTargets.Return(_vPass);
        }

        /// <summary>
        /// Function to build up the state for the effect.
        /// </summary>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        private void BuildState(IGorgon2DEffectBuilders builders)
        {
            if (_blurState == null)
            {
                _blurState = builders.PixelShaderBuilder.SamplerState(GorgonSamplerState.Default)
                                                        .ConstantBuffer(_blurBufferKernel, 1)
                                                        .ConstantBuffer(_blurBufferPass, 2)
                                                        .Shader(_blurShader)
                                                        .Build();
            }

            if (_blurStateNoAlpha == null)
            {
                _blurStateNoAlpha = builders.PixelShaderBuilder.Shader(_blurShaderNoAlpha)
                                                               .Build();
            }

            _batchState = builders.BatchBuilder.Clear()
                          .BlendState(GorgonBlendState.NoBlending)
                          .PixelShaderState(_blurState)
                          .Build(BatchStateAllocator);

            _batchStateNoAlpha = builders.BatchBuilder.Clear()
                                 .BlendState(GorgonBlendState.NoBlending)
                                 .PixelShaderState(_blurStateNoAlpha)
                                 .Build(BatchStateAllocator);
        }

        /// <summary>Function called to build a new (or return an existing) 2D batch state.</summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="statesChanged">
        ///   <b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
        {
            if ((_batchStateNoAlpha == null)
                || (_batchState == null))
            {
                BuildState(builders);
            }

            return PreserveAlpha? _batchStateNoAlpha : _batchState;
        }

        /// <summary>
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="texture">The texture to blur.</param>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <remarks>
        /// <para>
        /// Applications must implement this in order to see any results from the effect.
        /// </para>
        /// </remarks>
        protected void BlurTexture(GorgonTexture2DView texture, int passIndex)
        {
            switch (passIndex)
            {
                case 0:
                    Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _hPassView.Width, _hPassView.Height), GorgonColor.White, texture, new DX.RectangleF(0, 0, 1, 1));
                    break;
                case 1:
                    Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _vPassView.Width, _vPassView.Height), GorgonColor.White, _hPassView, new DX.RectangleF(0, 0, 1, 1));
                    break;
            }
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
                SizeInBytes = (((sizeof(float) * KernelSize) + 15) & ~15)
                                                                           + (((sizeof(float) * _offsetSize) + 15) & ~15)
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
            _blurShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderGaussBlur");

            _blurShaderNoAlpha = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderGaussBlurNoAlpha");

            UpdateKernelWeights();
            UpdateOffsets();
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
            if (_needKernelUpdate)
            {
                UpdateKernelWeights();
            }

            if (_needOffsetUpdate)
            {
                UpdateOffsets();
            }

            // Update the render target prior to rendering.
            UpdateRenderTarget();
        }

        /// <summary>
        /// Function called prior to rendering a pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <returns>A <see cref="PassContinuationState"/> to instruct the effect on how to proceed.</returns>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.
        /// </para>
        /// </remarks>
        /// <seealso cref="PassContinuationState"/>
        protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, IGorgon2DCamera camera)
        {
            if (_blurRadius == 0)
            {
                return PassContinuationState.Stop;
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

            return PassContinuationState.Continue;
        }

        /// <summary>
        /// Function called after rendering is complete.
        /// </summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to clean up and/or restore any states when rendering is finished. This is an ideal method to copy any rendering imagery to the final output render target.
        /// </para>
        /// </remarks>
        protected override void OnAfterRender(GorgonRenderTargetView output)
        {
            if ((_hPass == null) || (_vPass == null))
            {
                return;
            }

            Renderer.Begin(Gorgon2DBatchState.NoBlend);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height), GorgonColor.White, _vPassView, new DX.RectangleF(0, 0, 1, 1));
            Renderer.End();

            FreeTargets();
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

            GorgonPixelShader blurShader = Interlocked.Exchange(ref _blurShader, null);
            GorgonPixelShader blurShaderNoAlpha = Interlocked.Exchange(ref _blurShaderNoAlpha, null);
            GorgonNativeBuffer<float> kernelData = Interlocked.Exchange(ref _blurKernelData, null);
            GorgonConstantBufferView kernelBuffer = Interlocked.Exchange(ref _blurBufferKernel, null);
            GorgonConstantBufferView passBuffer = Interlocked.Exchange(ref _blurBufferPass, null);

            blurShader?.Dispose();
            blurShaderNoAlpha?.Dispose();
            kernelData?.Dispose();
            kernelBuffer?.Dispose();
            passBuffer?.Dispose();
        }

        /// <summary>
        /// Function to render the effect.
        /// </summary>
        /// <param name="texture">The texture to blur and render to the output.</param>
        /// <param name="output">The output render target.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="texture"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Render(GorgonTexture2DView texture, GorgonRenderTargetView output) 
        {
            BeginRender(output);

            for (int i = 0; i < PassCount; ++i)
            {
                switch (BeginPass(i, output))
                {
                    case PassContinuationState.Continue:
                        BlurTexture(texture, i);
                        break;
                    case PassContinuationState.Skip:
                        continue;
                    default:
                        EndRender(output);
                        return;
                }

                EndPass(i, output);
            }

            EndRender(output);
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

            // A macro used to define the size of the kernel weight data structure.
            KernelSize = kernelSize;
            MaximumBlurRadius = ((kernelSize - 1) / 2).Max(1);

            // Adjust offset size to start on a 4 float boundary.
            _offsetSize = ((2 * KernelSize) + 3) & ~3;
            _blurRadius = MaximumBlurRadius;

            _blurRtvInfo = new GorgonTexture2DInfo
            {
                Binding = TextureBinding.ShaderResource | TextureBinding.RenderTarget
            };

            Macros.Add(new GorgonShaderMacro("GAUSS_BLUR_EFFECT"));
            Macros.Add(new GorgonShaderMacro("MAX_KERNEL_SIZE", KernelSize.ToString(CultureInfo.InvariantCulture)));
        }
        #endregion
    }
}
