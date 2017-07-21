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
using Gorgon.Diagnostics;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// An effect that performs a gaussian blur render target textures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will take the image from a <see cref="GorgonTexture"/> and blur it using gaussian blurring. The blur effect is done in two passes, one for horizontal and one for vertical and renders each pass 
    /// to an internal set of <see cref="GorgonRenderTargetView"/> objects, which are then output to a final <see cref="GorgonRenderTargetView"/> at the end of the render.
    /// </para>
    /// <para>
    /// Effects such as these are derived using the <see cref="GorgonShaderEffect"/> base class which can be extended by users to provide visual effects using familiar programming interfaces.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShaderEffect"/>
    /// <seealso cref="GorgonRenderTargetView"/>
    /// <seealso cref="GorgonTexture"/>
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
        // The texture to pass in to the effect to be blurred.
        private GorgonTexture _inputTexture;
        // The target that will receive the blurred image.
        private GorgonRenderTargetView _blurredTarget;
        // The number blur constant buffer parameters.
        private GorgonConstantBuffers _blurConstants = new GorgonConstantBuffers(2);
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
        /// <para>
        /// <note type="warning">
        /// <para>
        /// Increasing the render target size will have a performance impact due to the number of texels being processed. It is recommended to scale this size based on your target video device(s) 
        /// capabilities.
        /// </para>
        /// </note>
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
                                             Usage = D3D11.ResourceUsage.Default,
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
                                             Usage = D3D11.ResourceUsage.Default,
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
                    Graphics.DrawTexture(_inputTexture,
                                         _hTarget.DefaultRenderTargetView.Bounds,
                                         pixelShader: (PreserveAlpha ? _blurShaderNoAlpha : _blurShader),
                                         pixelShaderConstants: _blurConstants);
                    break;
                case 1:
                    Graphics.DrawTexture(_inputTexture,
                                         _vTarget.DefaultRenderTargetView.Bounds,
                                         pixelShader: (PreserveAlpha ? _blurShaderNoAlpha : _blurShader),
                                         pixelShaderConstants: _blurConstants);
                    break;
                case 2:
                    Graphics.DrawTexture(_blurRadius != 0 ? _vTarget : _inputTexture, _blurredTarget.Bounds);
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
                                                             Usage = D3D11.ResourceUsage.Default,
                                                             SizeInBytes = _kernelSizeBytes + _offsetSizeBytes + sizeof(int)
                                                         });
            _blurKernelData = new GorgonPointer(_blurBufferKernel.SizeInBytes);
            _blurKernelData.Zero();

            _blurBufferPass = new GorgonConstantBuffer("Effect.GorgonGaussPassSettings",
                                                       Graphics,
                                                       new GorgonConstantBufferInfo
                                                       {
                                                           Usage = D3D11.ResourceUsage.Dynamic,
                                                           SizeInBytes = 16
                                                       });


            // Set up the constants used by our pixel shader.
            _blurConstants[0] = _blurBufferKernel;
            _blurConstants[1] = _blurBufferPass;

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
            if ((_renderTargetUpdated) && ((Graphics.VideoDevice.GetBufferFormatSupport(BlurTargetFormat) & D3D11.FormatSupport.RenderTarget) !=
                                           D3D11.FormatSupport.RenderTarget))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, BlurTargetFormat));
            }
#endif

            // If we didn't supply an input, then we don't need to continue.
            if ((_inputTexture == null)
                || (_blurredTarget == null))
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

                    data = _blurBufferPass.Lock(D3D11.MapMode.WriteDiscard);
                    data.Write(passIndex);
                    _blurBufferPass.Unlock(ref data);

                    Graphics.SetRenderTarget(_hTarget.DefaultRenderTargetView);
                    break;
                case 1:
                    if (_blurRadius == 0)
                    {
                        return false;
                    }
                    
                    data = _blurBufferPass.Lock(D3D11.MapMode.WriteDiscard);
                    data.Write(passIndex);
                    _blurBufferPass.Unlock(ref data);

                    Graphics.SetRenderTarget(_vTarget.DefaultRenderTargetView);
                    break;
                case 2:
                    Graphics.SetRenderTarget(_blurredTarget);
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to render this effect using the source <see cref="GorgonTexture"/> and the output <see cref="GorgonRenderTargetView"/>.
        /// </summary>
        /// <param name="sourceTexture">The texture containing the image data to blur.</param>
        /// <param name="outputRenderTarget">The output render target that will receive the blurred image data.</param>
        /// <param name="recordStates">[Optional] Flags to record states on the owning <see cref="Graphics"/> interface prior to rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceTexture"/> or the <paramref name="outputRenderTarget"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <paramref name="sourceTexture"/>, blur it, and then copy the blurred result into <paramref name="outputRenderTarget"/>. Applications can then take the 
        /// <paramref name="outputRenderTarget"/> and use it in a post process chain, or however is appropriate for the application. 
        /// </para>
        /// <para>
        /// The <paramref name="sourceTexture"/>, and the <paramref name="outputRenderTarget"/> values must <b>not</b> be disposed while this method is active. Doing so will cause undefined behavior 
        /// as well as exceptions.
        /// </para>
        /// <para>
        /// <para>
        /// The optional <paramref name="recordStates"/> flags are a combination of flags to indicate which states on the owning <see cref="Graphics"/> interface to record and restore prior to and after 
        /// rendering. If this parameter is omitted, then no state recording will occur. In some cases, users may desire more control over when the recording happens and when the restoration happens. 
        /// This can be facilitated through the <see cref="GorgonShaderEffect.RecordStates"/> and <see cref="GorgonShaderEffect.RestoreStates"/> methods.
        /// </para>
        /// <para>
        /// <note type="note">
        /// <para>
        /// Recording the states will incur a slight performance penalty.
        /// </para>
        /// </note>
        /// </para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled in <b>DEBUG</b> mode.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonRenderTargetView"/>
        /// <seealso cref="GorgonTexture"/>
        /// <seealso cref="GorgonShaderEffect.RecordedStates"/>
        /// <seealso cref="GorgonShaderEffect.RecordStates"/>
        /// <seealso cref="GorgonShaderEffect.RestoreStates"/>
        public void Blur(GorgonTexture sourceTexture, GorgonRenderTargetView outputRenderTarget, RecordStates recordStates = Core.RecordStates.None)
        {
            sourceTexture.ValidateObject(nameof(sourceTexture));
            outputRenderTarget.ValidateObject(nameof(outputRenderTarget));

            _inputTexture = sourceTexture;
            _blurredTarget = outputRenderTarget;

            Render(recordStates);

            _inputTexture = null;
            _blurredTarget = null;
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
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGaussBlurEffect"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this effect.</param>
        /// <param name="kernelSize">[Optional] The size of the convolution kernel.</param>
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
        /// <seealso cref="KernelSize"/>
        /// <seealso cref="MaximumBlurRadius"/>
        public GorgonGaussBlurEffect(GorgonGraphics graphics, int kernelSize = 7)
            : base(graphics, Resources.GORGFX_NAME_GAUSS_BLUR_EFFECT, 3)
        {
            if ((kernelSize < 3) 
                || (kernelSize > 81))
            {
                throw new ArgumentOutOfRangeException(nameof(kernelSize), Resources.GORGFX_ERR_EFFECT_GAUSS_BLUR_KERNEL_SIZE_INVALID);
            }

            KernelSize = kernelSize;
            MaximumBlurRadius = ((kernelSize - 1) / 2).Max(1);

            // Adjust to the weights and offsets to the nearest 16 bytes.
            _offsetSizeBytes = (((sizeof(float) * 2) * KernelSize) + 15) & (~15); 
            _kernelSizeBytes = ((sizeof(float) * KernelSize) + 15) & (~15);
            _blurRadius = MaximumBlurRadius;
        }
        #endregion
    }
}
