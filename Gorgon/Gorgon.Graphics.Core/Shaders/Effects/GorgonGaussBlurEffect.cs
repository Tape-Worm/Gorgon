#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 17, 2017 8:21:31 PM
// 
#endregion

using System;
using System.Globalization;
using Gorgon.Core;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Direct3D11;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// An effect that performs a gaussian blur render target textures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will take the image from a render target and blur it using gaussian blurring. The blur effect is done in two passes, one for horizontal and one for vertical and renders each pass to an internal 
    /// set of render targets, which are then output to another shader at the end of the render (this, however, is optional).
    /// </para>
    /// <para>
    /// Effects such as these are derived using the <see cref="GorgonShaderEffect"/> base class which can be extended by users to provide visual effects using familiar programming interfaces.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShaderEffect"/>
    public class GorgonGaussBlurEffect
        : GorgonShaderEffect
    {
        #region Variables.
        // The pixel shader used for blurring.
        private GorgonPixelShader _blurShader;
        // The pixel shader used for blurring with alpha preservation.
        private GorgonPixelShader _blurShaderNoAlpha;
        // The number of bytes for the offset data (aligned to 16 bytes).
        private readonly int _offsetSizeBytes;
        // The number of bytes for the kernel data (aligned to 16 bytes).
        private readonly int _kernelSizeBytes; 
        // The radius of the blur.
        private int _blurRadius;
        // Buffer for blur kernel data.
        private GorgonConstantBuffer _blurBufferKernel;
        // Buffer for buffer settings.
        private GorgonConstantBuffer _blurBufferPass;
        // Horizontal blur render target.
        private GorgonTexture _hTarget;
        // Vertical blur render target.
        private GorgonTexture _vTarget;
        // Format of the blur render targets.
        private DXGI.Format _blurTargetFormat = DXGI.Format.R8G8B8A8_UNorm;
        // Size of the render targets used for blurring.
        private DX.Size2 _blurTargetSize = new DX.Size2(256, 256);
        // A pointer to an in-memory buffer for kernel data.
        private GorgonPointer _blurKernelData;
        // Flag to indicate that the render target has been updated.
        private bool _renderTargetUpdated = true;
        // Flag to indicate that the offsets have been updated.
        private bool _offsetsNeedUpdate = true;
        // Flag to indicate that the kernel data has been updated.
        private bool _kernelNeedUpdate = true;
        // The blitter used to pass data from one render target to another and apply the blur shader(s).
        private GorgonTextureBlitter _blitter;
        // The pipeline state for blurring each pass.
        private GorgonPipelineState _blurState;
        // The pipeline state for blurring each pass with alpha preservation.
        private GorgonPipelineState _blurStateNoAlpha;
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
        /// Higher values will increase the quality of the blur, but will cause the shader to run slower because more samples will need to be taken.
        /// </para>
        /// <para>
        /// The valid range of values are within the range of 0 (no blurring) to <see cref="MaximumBlurRadius"/>.
        /// </para>
        /// <para>
        /// The default value is 1.
        /// </para>
        /// </remarks>
        /// <seealso cref="MaximumBlurRadius"/>
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
                _kernelNeedUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the format of the internal render targets used for blurring.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the default texture surface format for the internal render targets used for blurring. This value may be any type of format supported by a render target (see 
        /// <see cref="IGorgonVideoDevice.GetBufferFormatSupport"/> for determing an acceptable format).
        /// </para>
        /// <para>
        /// If this value is set to an unacceptable format, then the effect will throw an exception during rendering.
        /// </para>
        /// <para>
        /// The default value is <c>R8G8B8A8_UNorm</c>.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonVideoDevice.GetBufferFormatSupport"/>
        public DXGI.Format BlurTargetFormat
        {
            get => _blurTargetFormat;
            set
            {
                if (_blurTargetFormat == value)
                {
                    return;
                }

                _blurTargetFormat = value;
                _renderTargetUpdated = true;
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
        /// This value is limited to <c>3x3</c> up to the maximum width and height specified by <see cref="IGorgonVideoDevice.MaxTextureWidth"/> and <see cref="IGorgonVideoDevice.MaxTextureHeight"/>.
        /// </para>
        /// <para>
        /// The default size is <c>256x256</c>.
        /// </para>
        /// </remarks>
        public DX.Size2 BlurRenderTargetsSize
        {
            get => _blurTargetSize;
            set
            {
                if (_blurTargetSize == value)
                {
                    return;
                }

                // Constrain the size.
                value.Width = value.Width.Max(3).Min(Graphics.VideoDevice.MaxTextureWidth);
                value.Height = value.Height.Max(3).Min(Graphics.VideoDevice.MaxTextureHeight);

                _blurTargetSize = value;
                _renderTargetUpdated = true;
                _offsetsNeedUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the texture to pass in to the effect to be blurred.
        /// </summary>
        /// ??
        public GorgonTexture InputTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the target that will receive the blurred image.
        /// </summary>
        /// ??
        public GorgonRenderTargetView BlurredTarget
        {
            get;
            set;
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
            _hTarget = new GorgonTexture("Effect.Gauss_BlurHPass",
                                         Graphics,
                                         new GorgonTextureInfo
                                         {
                                             Format = _blurTargetFormat,
                                             Usage = ResourceUsage.Default,
                                             Width = _blurTargetSize.Width,
                                             Height = _blurTargetSize.Height,
                                             Binding = TextureBinding.RenderTarget | TextureBinding.ShaderResource,
                                             TextureType = TextureType.Texture2D
                                         });

            _vTarget = new GorgonTexture("Effect.Gauss_BlurVPass",
                                         Graphics,
                                         new GorgonTextureInfo
                                         {
                                             Format = _blurTargetFormat,
                                             Usage = ResourceUsage.Default,
                                             Width = _blurTargetSize.Width,
                                             Height = _blurTargetSize.Height,
                                             Binding = TextureBinding.RenderTarget | TextureBinding.ShaderResource,
                                             TextureType = TextureType.Texture2D
                                         });

            _renderTargetUpdated = false;
        }

        /// <summary>
        /// Function to update the offsets for the shader.
        /// </summary>
        private void UpdateOffsets()
        {
            // This adjusts just how far from the texel the blurring can occur.
            var unitSize = new DX.Vector2(1.0f / BlurRenderTargetsSize.Width, 1.0f / BlurRenderTargetsSize.Height);

            int pointerOffset = 0;
            int byteBoundary = (((_blurRadius) * 2) + 1) * sizeof(float);

            // Store the offsets like this: xxxxx yyyyy, where x is the horizontal offset and y is the vertical offset.
            // This way our shader can read it linearly per pass.
            for (int i = -_blurRadius; i <= _blurRadius; i++)
            {
                _blurKernelData.Write(pointerOffset, unitSize.X * i);
                _blurKernelData.Write(pointerOffset + byteBoundary, unitSize.Y * i);
                pointerOffset += sizeof(float);
            }

            _blurBufferKernel.UpdateFromPointer(_blurKernelData);

            _offsetsNeedUpdate = false;
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
            int pointerOffset = _offsetSizeBytes;
            int firstPassOffset = pointerOffset;

            // Write kernel coefficients.
            for (int i = -_blurRadius; i <= _blurRadius; ++i)
            {
                float distance = i * i;
                float kernelValue = (-distance / sqSigmaDouble).Exp() / sigmaRoot;

                _blurKernelData.Write(firstPassOffset, kernelValue);
                
                total += kernelValue;
                firstPassOffset += sizeof(float);
            }
            
            // We need to read back the memory and average the kernel weights.
            for (int i = 0; i < blurKernelSize; ++i)
            {
                _blurKernelData.Write(pointerOffset, _blurKernelData.Read<float>(pointerOffset) / total);
                pointerOffset += sizeof(float);
            }

            // Write out the current blur radius.
            pointerOffset = _kernelSizeBytes + _offsetSizeBytes;
            _blurKernelData.Write(pointerOffset, _blurRadius);

            _kernelNeedUpdate = false;
            _offsetsNeedUpdate = true;
        }

        /// <summary>
        /// Function to free any render targets allocated by the effect.
        /// </summary>
        private void FreeTargets()
        {
            _hTarget?.Dispose();
            _vTarget?.Dispose();

            _hTarget = null;
            _vTarget = null;

            _renderTargetUpdated = true;
        }

        /// <summary>
        /// Function called when a pass is rendered.
        /// </summary>
        /// <param name="passIndex">The current index of the pass being rendered.</param>
        protected override void OnRenderPass(int passIndex)
        {
            switch (passIndex)
            {
                case 0:
                    _blitter.Blit(InputTexture, 0, 0, _hTarget.Info.Width, _hTarget.Info.Height);
                    break;
                case 1:
                    _blitter.Blit(_hTarget, 0, 0, _hTarget.Info.Width, _hTarget.Info.Height);
                    break;
                case 2:
                    _blitter.Blit(_blurRadius != 0 ? _vTarget : InputTexture, 0, 0, BlurredTarget.Width, BlurredTarget.Height);
                    break;
            }
        }

        /// <summary>
        /// Function called when the effect is initialized for the first time.
        /// </summary>
        protected override void OnInitialize()
        {
            // Align sizes to 16 byte boundaries.
            _blurBufferKernel = new GorgonConstantBuffer("Effect.GorgonGaussKernelData",
                                                         Graphics,
                                                         new GorgonConstantBufferInfo
                                                         {
                                                             Usage = ResourceUsage.Default,
                                                             SizeInBytes = _kernelSizeBytes + _offsetSizeBytes + sizeof(int)
                                                         });
            _blurKernelData = new GorgonPointer(_blurBufferKernel.SizeInBytes);
            _blurKernelData.Zero();

            _blurBufferPass = new GorgonConstantBuffer("Effect.GorgonGaussPassSettings",
                                                       Graphics,
                                                       new GorgonConstantBufferInfo
                                                       {
                                                           Usage = ResourceUsage.Dynamic,
                                                           SizeInBytes = 16
                                                       });


            // Set the appropriate pipeline state.
            _blitter = new GorgonTextureBlitter(Graphics)
                       {
                           PixelShaderConstants =
                           {
                               [0] = _blurBufferKernel,
                               [1] = _blurBufferPass
                           },
                           ScaleBlitter = true
                       };

            // A macro used to define the size of the kernel weight data structure.
            var weightsMacro = new[]
                               {
                                   new GorgonShaderMacro("GAUSS_BLUR_EFFECT"),
                                   new GorgonShaderMacro("MAX_KERNEL_SIZE", KernelSize.ToString(CultureInfo.InvariantCulture))
                               };

            // Compile our blur shaders.
            _blurShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics.VideoDevice,
                                                                         Resources.GraphicsShaders,
                                                                         "GorgonPixelShaderGaussBlur",
                                                                         GorgonGraphics.IsDebugEnabled,
                                                                         weightsMacro);

            _blurShaderNoAlpha = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics.VideoDevice,
                                                                                Resources.GraphicsShaders,
                                                                                "GorgonPixelShaderGaussBlurNoAlpha",
                                                                                GorgonGraphics.IsDebugEnabled,
                                                                                weightsMacro);

            // Build up our pipeline state containing our pixel shader used to blur.
            _blurState = Graphics.GetPipelineState(new GorgonPipelineStateInfo
                                                       {
                                                           PixelShader = _blurShader,
                                                           VertexShader = _blitter.VertexShader,
                                                           RasterState = GorgonRasterState.Default,
                                                           DepthStencilState = GorgonDepthStencilState.Default,
                                                           BlendStates = new[]
                                                                         {
                                                                             GorgonBlendState.NoBlending
                                                                         }
                                                       });

            _blurStateNoAlpha = Graphics.GetPipelineState(new GorgonPipelineStateInfo(_blurState.Info)
                                                          {
                                                              PixelShader = _blurShaderNoAlpha
                                                          });


            _blitter.PipelineState = !PreserveAlpha ? _blurState : _blurStateNoAlpha;

            UpdateRenderTarget();
            UpdateKernelWeights();
            UpdateOffsets();

            // Upload to the GPU.
            _blurBufferKernel.UpdateFromPointer(_blurKernelData);
        }

        /// <summary>
        /// Function called before rendering begins.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        /// <exception cref="GorgonException">Thrown if the render target could not be created due to an incompatible <see cref="BlurTargetFormat"/>.
        /// </exception>
        protected override bool OnBeforeRender()
        {
#if DEBUG
            if ((_renderTargetUpdated) && ((Graphics.VideoDevice.GetBufferFormatSupport(BlurTargetFormat) & FormatSupport.RenderTarget) !=
                                           FormatSupport.RenderTarget))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, BlurTargetFormat));
            }
#endif

            // If we didn't supply an input, then we don't need to continue.
            if ((InputTexture == null)
                || (BlurredTarget == null))
            {
                return false;
            }

            if (_kernelNeedUpdate)
            {
                UpdateKernelWeights();
            }

            if (_offsetsNeedUpdate)
            {
                UpdateOffsets();
            }

            if (!_renderTargetUpdated)
            {
                return true;
            }

            // Update the render target prior to rendering.
            UpdateRenderTarget();
            return true;
        }

        /// <summary>
        /// Function called before a rendering pass begins.
        /// </summary>
        /// <param name="passIndex">The current pass index.</param>
        /// <returns><b>true</b> to continue rendering, or <b>false</b> to skip this pass and move to the next.</returns>
        protected override bool OnBeforePass(int passIndex)
        {
            GorgonPointerAlias data;

            switch (passIndex)
            {
                case 0:
                    if (_blurRadius == 0)
                    {
                        return false;
                    }

                    data = _blurBufferPass.Lock(MapMode.WriteDiscard);
                    data.Write(passIndex);
                    _blurBufferPass.Unlock(ref data);

                    _blitter.PipelineState = !PreserveAlpha ? _blurState : _blurStateNoAlpha;
                    _blitter.PixelShaderConstants[0] = _blurBufferKernel;
                    _blitter.PixelShaderConstants[1] = _blurBufferPass;

                    Graphics.SetRenderTarget(_hTarget.DefaultRenderTargetView);
                    break;
                case 1:
                    if (_blurRadius == 0)
                    {
                        return false;
                    }
                    
                    data = _blurBufferPass.Lock(MapMode.WriteDiscard);
                    data.Write(passIndex);
                    _blurBufferPass.Unlock(ref data);

                    Graphics.SetRenderTarget(_vTarget.DefaultRenderTargetView);
                    break;
                case 2:
                    _blitter.PipelineState = null;
                    _blitter.PixelShaderConstants[0] = null;
                    _blitter.PixelShaderConstants[1] = null;
                    Graphics.SetRenderTarget(BlurredTarget);
                    break;
            }

            return true;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public override void Dispose()
        {
            FreeTargets();
            _blurShader?.Dispose();
            _blurShaderNoAlpha?.Dispose();
            _blurKernelData?.Dispose();
            _blurBufferKernel?.Dispose();
            _blurBufferPass?.Dispose();
            _blitter?.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGaussBlurEffect"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this effect.</param>
        /// <param name="kernelSize">[Optional] The size of the convolution kernel.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="kernelSize"/> is less than 1 or greater than 64.</exception>
        /// <remarks>
        /// <para>
        /// If the <paramref name="kernelSize"/> is specified, it will define the number of taps used to sample around a texel for blurring. This value should be a multiple of 3 and will be adjusted 
        /// accordingly to the nearest power of 3 if it is not. If this parameter is omitted, then a 9 tap blur will be created.
        /// </para>
        /// </remarks>
        public GorgonGaussBlurEffect(GorgonGraphics graphics, int kernelSize = 9)
            : base(graphics, Resources.GORGFX_NAME_GAUSS_BLUR_EFFECT, 3)
        {
            if ((kernelSize < 1) 
                || (kernelSize > 64))
            {
                throw new ArgumentOutOfRangeException(nameof(kernelSize), Resources.GORGFX_ERR_GAUSS_BLUR_KERNEL_SIZE_INVALID);
            }

            KernelSize = (kernelSize + 2) & ~2;
            MaximumBlurRadius = ((kernelSize - 1) / 2).Max(1);
            _offsetSizeBytes = (((sizeof(float) * 2) * KernelSize) + 15) & (~15); 
            _kernelSizeBytes = ((sizeof(float) * KernelSize) + 15) & (~15);
            _blurRadius = 1;
        }
        #endregion
    }
}
