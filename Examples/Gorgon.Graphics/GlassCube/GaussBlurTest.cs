using System;
using System.Runtime.InteropServices;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Direct3D11;

namespace Gorgon.Graphics.Example
{
    public class GaussBlurTest
        : GorgonShaderEffect
    {
        #region Value Types.
        [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
        private struct PassData
        {
            public int PassIndex;
            public int PreserveAlpha;
        }
        #endregion

        #region Variables.
        // The size of the convolution kernel.
        private readonly int _kernelSize;
        // The number of bytes for the offset data (aligned to 16 bytes).
        private readonly int _offsetSizeBytes;
        // The number of bytes for the kernel data (aligned to 16 bytes).
        private readonly int _kernelSizeBytes; 
        // The radius of the blur.
        private int _blurRadius = 1;
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
        // Pass data.
        private PassData _passData = new PassData
                                     {
                                         PreserveAlpha = 1,
                                         PassIndex = 0
                                     };
        private GorgonTextureBlitter _blitter;
        private GorgonPipelineState _pipelineState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to blur the alpha component or not.
        /// </summary>
        public bool PreserveAlpha
        {
            get => _passData.PreserveAlpha != 0;
            set => _passData.PreserveAlpha = value ? 1 : 0;
        }

        /// <summary>
        /// Property to set or return the radius for the blur effect.
        /// </summary>
        /// <remarks>Higher values will increase the quality of the blur, but will cause the shader to run slower.
        /// <para>
        /// The valid range of values are within the range of 0 - kernel size.
        /// </para>
        /// </remarks>
        public int BlurRadius
        {
            get => _blurRadius;
            set
            {
                if (_blurRadius == value)
                {
                    return;
                }

                int maxValue = ((_kernelSize - 1) / 2).Max(1);
                if (value > maxValue)
                {
                    value = maxValue;
                }

                if (value < 0)
                {
                    value = 0;
                }

                _blurRadius = value;
                _kernelNeedUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the format of the internal render targets used for blurring.
        /// </summary>
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
                if (value.Width < 1)
                {
                    value.Width = 1;
                }

                if (value.Height < 1)
                {
                    value.Height = 1;
                }

                if (value.Width >= Graphics.VideoDevice.MaxTextureWidth)
                {
                    value.Width = Graphics.VideoDevice.MaxTextureWidth;
                }

                if (value.Height >= Graphics.VideoDevice.MaxTextureHeight)
                {
                    value.Height = Graphics.VideoDevice.MaxTextureHeight;
                }

                _blurTargetSize = value;
                _renderTargetUpdated = true;
                _offsetsNeedUpdate = true;
            }
        }

        /// <summary>
        /// Property to set or return the texture to pass in to the effect to be blurred.
        /// </summary>
        public GorgonTexture InputTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the target that will receive the blurred image.
        /// </summary>
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
            FreeTargets();

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
        /// Function to update the blur kernel.
        /// </summary>
        /// <remarks>This implementation is ported from the Java code appearing in "Filthy Rich Clients: Developing Animated and Graphical Effects for Desktop Java".</remarks>
        private void UpdateKernel()
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
        }
        
        /// <summary>
        /// Function execute the horizontal pass for the effect.
        /// </summary>
        private void HorizontalPass()
        {
            _blitter.Blit(InputTexture, 0, 0, _hTarget.Info.Width, _hTarget.Info.Height);
        }

        /// <summary>
        /// Function to execute the 2nd render pass.
        /// </summary>
        private void VerticalPass()
        {
            _blitter.Blit(_hTarget, 0, 0, _hTarget.Info.Width, _hTarget.Info.Height);
        }

        /// <summary>
        /// Function to output the internal blurred render target to our final target.
        /// </summary>
        public void OutputPass()
        {
            _blitter.Blit(_blurRadius != 0 ? _vTarget : InputTexture, 0, 0, BlurredTarget.Width, BlurredTarget.Height);
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

            _pipelineState = Graphics.GetPipelineState(new GorgonPipelineStateInfo
                                                       {
#if DEBUG
                                                           PixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics.VideoDevice,
                                                                                                                        Resources.TestGauss,
                                                                                                                        "GorgonPixelShaderGaussBlur",
                                                                                                                        true,
                                                                                                                        new[]
                                                                                                                        {
                                                                                                                            new
                                                                                                                                GorgonShaderMacro("MAX_KERNEL_SIZE",
                                                                                                                                                  _kernelSize
                                                                                                                                                      .ToString()),
                                                                                                                        }),
#else
                                                           PixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics.VideoDevice,
                                                                                                                        Resources.TestGauss,
                                                                                                                        "GorgonPixelShaderGaussBlur",
                                                                                                                        false,
                                                                                                                        new []
                                                                                                                        {
                                                                                                                            new GorgonShaderMacro("MAX_KERNEL_SIZE", _kernelSize.ToString()), 
                                                                                                                        }),
#endif
                                                           VertexShader = _blitter.VertexShader,
                                                           RasterState = GorgonRasterState.Default,
                                                           DepthStencilState = GorgonDepthStencilState.Default,
                                                           BlendStates = new[]
                                                                         {
                                                                             GorgonBlendState.NoBlending
                                                                         }
                                                       });

            _blitter.PipelineState = _pipelineState;

            UpdateRenderTarget();
            UpdateKernel();
            UpdateOffsets();

            // Upload to the GPU.
            _blurBufferKernel.UpdateFromPointer(_blurKernelData);
        }

        /// <summary>
        /// Function called before rendering begins.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        protected override bool OnBeforeRender()
        {
            // If we didn't supply an input, then we don't need to continue.
            if ((InputTexture == null)
                || (BlurredTarget == null))
            {
                // TODO: This should be an exception and the target format should be compatible.
                return false;
            }

            if (_kernelNeedUpdate)
            {
                UpdateKernel();
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

            _passData.PassIndex = passIndex;

            switch (passIndex)
            {
                case 0:
                    if (_blurRadius == 0)
                    {
                        return false;
                    }

                    data = _blurBufferPass.Lock(MapMode.WriteDiscard);
                    data.Write(ref _passData);
                    _blurBufferPass.Unlock(ref data);

                    _blitter.PipelineState = _pipelineState;
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
                    data.Write(ref _passData);
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
            _blurKernelData?.Dispose();
            _blurBufferKernel?.Dispose();
            _blurBufferPass?.Dispose();
            _pipelineState?.Info.PixelShader.Dispose();
            _blitter?.Dispose();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GaussBlurTest"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this effect.</param>
        /// <param name="kernelSize">The size of the convolution kernel.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="kernelSize"/> is less than 1 or greater than 128.</exception>
        internal GaussBlurTest(GorgonGraphics graphics, int kernelSize = 9)
            : base(graphics, "Gaussian Blur Effect", 3)
        {
            if (kernelSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(kernelSize), "Less than 1.");
            }

            if (kernelSize > 128)
            {
                throw new ArgumentOutOfRangeException(nameof(kernelSize), "Greater than 128");
            }

            _kernelSize = (kernelSize + 2) & ~2;
            _offsetSizeBytes = (((sizeof(float) * 2) * _kernelSize) + 15) & (~15); 
            _kernelSizeBytes = ((sizeof(float) * _kernelSize) + 15) & (~15);
            _blurRadius = 1;
            
            // Register our passes for this effect.
            RegisterPass(0, HorizontalPass);
            RegisterPass(1, VerticalPass);
            RegisterPass(2, OutputPass);
        }
        #endregion
    }
}
