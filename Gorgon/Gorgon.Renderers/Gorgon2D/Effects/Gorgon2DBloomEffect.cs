#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 15, 2019 10:54:42 PM
// 
#endregion

using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>An effect that renders a bloom (glow) effect for a scene.</summary>
    /// <remarks>
    ///   <para>
    /// Bloom is an effect that helps make light sources or bright areas look more intense and vibrant by surrounding them in a glow.
    /// </para>
    ///   <para>
    /// It does this by finding the bright areas above a certain threshold in a render target, and then blurring that image while down sampling and up sampling and applying the result to the original
    /// scene using an additive blend. Depending on the settings your scene could be given a dream-like hazy look, or an overbright image for very intense light sources (e.g. a star).
    /// </para>
    ///   <para>
    /// To facilitate the quality of the resulting bloom image, rendering will be performed on a mini HDR (High Dynamic Range) pipeline, and all render targets will use a floating point buffer format
    /// (<see cref="BufferFormat.R16G16B16A16_Float"/>). This means that this effect will use a lot of memory, and consume a fair bit of bandwidth. This means that developers should take target hardware
    /// capabilities into considering before using this effect.
    /// </para>
    ///   <para>
    /// While this effect employs HDR, it does not provide a means of performing Tone Mapping (a means of converting the large range of color values from floating point into LDR values that map from
    /// 0..1/0..255 per channel), so the resulting image may not appear as desired. Tone mapping is a very subjective process, and as such, there is no "right" tone mapping.
    /// </para>
    ///   <para>
    /// When using this effect, be aware that all blending is turned off except on the first pass. The best practice in this case would be to render the contents of your original render target (or swap 
    /// chain) using a call to <see cref="Gorgon2D.DrawFilledRectangle"/> or 
    /// <see cref="Gorgon2D.DrawSprite(GorgonSprite)"/>.
    /// </para>
    ///   <para>The rendering is split out over multiple passes:</para>
    ///   <list type="number">
    ///     <item>
    ///       Copy pass - This copies the data into an internal render target for processing.
    ///     </item>
    ///     <item>
    ///       Filter down/up sample pass - This isolates all bright areas above a certain <see cref="Threshold"/> and performs the down/up sampling of the image to blur it.
    ///     </item>
    ///     <item>
    ///       Combine pass - Combines the resulting blurred image with the original source image using an additive function.
    ///     </item>
    ///     <item>
    ///       [Optional] Dirt - Applies a lens dirt texture. 
    ///     </item>
    ///     <item>
    ///       Tone mapping - <em><strong>Not available as of this writing. May come in a future version.</strong></em>
    ///     </item>
    ///   </list>
    /// <para>
    /// This effect is based on the work of Jorge Jimenez from his SIGGRAPH 2014 presentation on advances in realtime rendering.<br/>
    /// The website for this presentation is located <a target="_blank" href="http://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare">here</a>.
    /// </para>
    /// <para>
    /// The PowerPoint presentation is <a target="_blank" href="http://www.iryoku.com/downloads/Next-Generation-Post-Processing-in-Call-of-Duty-Advanced-Warfare-v18.pptx">here</a> (it's a big'un).
    /// </para>
    /// </remarks>
    /// <seealso cref="Gorgon2D.DrawFilledRectangle(DX.RectangleF, GorgonColor, GorgonTexture2DView, DX.RectangleF?, int, GorgonSamplerState, float)"/>
    /// <seealso cref="Gorgon2D.DrawSprite"/>
    /// <seealso cref="BufferFormat"/>
    /// <seealso cref="Threshold"/>
    public class Gorgon2DBloomEffect
        : Gorgon2DEffect, IGorgon2DCompositorEffect
    {
        #region Value Types.
        /// <summary>
        /// Settings to apply when bright pass filtering.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct BloomSettings
        {
            /// <summary>
            /// The threshold and knee curve values used to filter the image.
            /// </summary>
            public Vector4 FilterValues;
            /// <summary>
            /// The color to apply to the bloom.
            /// </summary>
            public Vector4 BloomColor;
            /// <summary>
            /// The amount of blurring and bloom intensity/dirt intensity to apply (w is unused).
            /// </summary>
            public Vector4 BlurAndIntensity;
            /// <summary>
            /// Transformation parameters for the dirt texture.
            /// </summary>
            public Vector4 DirtTransform;
        }
        #endregion

        #region Constants.
        // The maximum number of iterations for sampling.
        private const int MaxIterations = 16;
        #endregion

        #region Variables.
        // Macro used to define low quality bloom code.
        private readonly GorgonShaderMacro _lowQualityMacro = new("LOW_QUALITY_BLOOM");
        // The shader that applies bright pass filtering.
        private GorgonPixelShader _filterShader;
        private Gorgon2DBatchState _filterBatchState;
        // The shader that performs down sampling.
        private GorgonPixelShader _downSampleShader;
        private Gorgon2DBatchState _downSampleBatchState;
        // The shader that performs up sampling.
        private GorgonPixelShader _upSampleShader;
        // The shader that performs the final pass.
        private GorgonPixelShader _finalPassShader;
        private Gorgon2DBatchState _finalPassBatchState;
        // The render target for the scene. This is the the first pass, and will receive the original rendering.
        private GorgonRenderTarget2DView _sceneRtv;
        private GorgonRenderTarget2DView _blurRtv;
        private GorgonTexture2DView _blurSrv;
        // The shader resource for the scene.
        private GorgonTexture2DView _sceneSrv;
        // A builder used to create shader states.
        private readonly Gorgon2DShaderStateBuilder<GorgonPixelShader> _shaderBuilder = new();
        // The bloom intensity level.
        private float _intensity = 1;
        // The amount of blur to apply.
        private float _blurAmount = 7;
        // The settings for the shaders.
        private BloomSettings _settings;
        private GorgonConstantBufferView _settingsBuffer;
        private GorgonConstantBufferView _textureSettingsBuffer;
        // The state for the first pass.
        private Gorgon2DBatchState _pass0State;
        // The intensity of the color to apply to the bloom.
        private float _colorIntensity;
        // The sampler targets.
        private (GorgonRenderTarget2DView up, GorgonRenderTarget2DView down)[] _sampleTargets;
        // The states that apply to each down sample target.
        private Gorgon2DBatchState[] _sampleTargetStates;
        // The allocators used to creating states for final pass.
        private readonly Gorgon2DBatchStatePoolAllocator _finalPassBatchAllocator = new(64);
        private readonly Gorgon2DShaderStatePoolAllocator<GorgonPixelShader> _finalPassPixelShaderAllocator = new(64);
        // The batch state builder for downsampling.
        private readonly Gorgon2DBatchStateBuilder _downSampleStateBuilder = new();
        // The texture settings for the bloom render targets.
        private readonly BloomTextureInfo _targetInfo;
        private readonly BloomTextureInfo _sceneTargetInfo;
        private readonly BloomTextureInfo _blurTargetInfo;
        // Flag to use lower quality functions to improve performance.
        private bool _lowQuality;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether to use lower quality rendering to improve performance.
        /// </summary>
        public bool LowQuality
        {
            get => _lowQuality;
            set
            {
                if (_lowQuality == value)
                {
                    return;
                }

                _lowQuality = value;
            }
        }

        /// <summary>
        /// Property to set or return the blurring level for the bloom effect.
        /// </summary>
        /// <remarks>
        /// Higher values generally apply more blurring when down/up sampling, while lower values will lessen the blur. Note that a value of 0 will disable the bloom effect.
        /// </remarks>
        public float BlurAmount
        {
            get => _blurAmount;
            set => _blurAmount = value.Min(10).Max(0);
        }

        /// <summary>
        /// Property to set or return the knee value to apply to the curve when filtering for a bright pass.
        /// </summary>
        public float BrightPassCurveKnee
        {
            get;
            set;
        } = 0.5f;

        /// <summary>
        /// Property to set or return the intensity of the bloom effect.
        /// </summary>
        /// <remarks>
        /// An intensity of 0 will disable the effect.
        /// </remarks>
        public float BloomIntensity
        {
            get => _intensity;
            set => _intensity = value.Min(10).Max(0);
        }

        /// <summary>
        /// Property to set or return the level of color value that will be included in the bright filter pass .
        /// </summary>
        /// <remarks>
        /// Values higher than or equal to 1 will only pick up data that has an R, G or B value larger than 1.0 and is only useful for HDR imagery.  Values lower than 1 will skip LDR colors.
        /// </remarks>
        public float Threshold
        {
            get;
            set;
        } = 1;

        /// <summary>
        /// Property to set or return the color to apply to the bloom.
        /// </summary>
        public GorgonColor Color
        {
            get;
            set;
        } = GorgonColor.White;

        /// <summary>
        /// Property to set or return the intensity of the color.
        /// </summary>
        /// <remarks>
        /// This value has a range of -6 to 6.
        /// </remarks>
        public float ColorIntensity
        {
            get => _colorIntensity;
            set => _colorIntensity = value.Max(-6).Min(6);
        }

        /// <summary>
        /// Property to set or return the texture used simluate lens dirt.
        /// </summary>
        /// <remarks>
        /// Setting this value to <b>null</b> will disable the dirt.
        /// </remarks>
        public GorgonTexture2DView DirtTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the intensity for the dirt.
        /// </summary>
        /// <remarks>
        /// Setting this value to 0.0f will disable the dirt.
        /// </remarks>
        public float DirtIntensity
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the down/upsampling targets to the pool.
        /// </summary>
        private void ReturnSampleTargets()
        {
            if (_sampleTargets is null)
            {
                return;
            }

            foreach ((GorgonRenderTarget2DView up, GorgonRenderTarget2DView down) in _sampleTargets)
            {
                if (up != _blurRtv)
                {
                    Graphics.TemporaryTargets.Return(up);
                }

                Graphics.TemporaryTargets.Return(down);
            }
        }

        /// <summary>
        /// Function to set up for the filtering/blurring pass.
        /// </summary>
        /// <param name="outputSize">The width and height of the final output.</param>
        private void SetupFilterAndBlur(DX.Size2 outputSize)
        {
            ref BloomSettings settings = ref _settings;

            float linearThreshold = Threshold.Pow(2.2f);
            int maxSize = (outputSize.Width / 2).Max(outputSize.Height / 2);

            float logSize = ((float)maxSize).Log(2) + BlurAmount - 10;
            float floorLog = logSize.FastFloor();

            int sampleIterations = (int)floorLog.Min(MaxIterations).Max(1);            
            float knee = linearThreshold * BrightPassCurveKnee + 1e-5f;

            settings.FilterValues = new Vector4(linearThreshold, linearThreshold - knee, knee * 2, 0.25f / knee);
            settings.BloomColor = Color.ApplyGamma(ColorIntensity).ToLinear();
            settings.BlurAndIntensity = new Vector4(0.5f + logSize - floorLog, (2.0f.Pow(BloomIntensity / 10.0f)) - 1.0f, DirtIntensity, 0);

            GorgonTexture2DView dirtTexture = DirtTexture ?? Renderer.EmptyBlackTexture;

            float screenAspect = outputSize.Width / (float)outputSize.Height;
            float dirtAspect = dirtTexture.Width / (float)dirtTexture.Height;
            ref Vector4 dirtTransform = ref _settings.DirtTransform;
            dirtTransform = new Vector4(0, 0, 1, 1);

            if (screenAspect > dirtAspect)
            {
                dirtTransform.W = dirtAspect / screenAspect;
                dirtTransform.Y = (1 - dirtTransform.W) * 0.5f;
            }
            else if (dirtAspect > screenAspect)
            {
                dirtTransform.Z = screenAspect / dirtAspect;
                dirtTransform.X = (1 - dirtTransform.Z) * 0.5f;
            }

            _settingsBuffer.Buffer.SetData(in settings);

            // Check target and state arrays for changes.
            if ((_sampleTargets is null) || (_sampleTargets.Length != sampleIterations))
            {
                _sampleTargets = new (GorgonRenderTarget2DView up, GorgonRenderTarget2DView down)[sampleIterations];
                _sampleTargetStates = new Gorgon2DBatchState[sampleIterations];
            }
        }

        /// <summary>
        /// Function to perform bright pass filtering and down sample blurring on the bloom texture.
        /// </summary>
        private void FilterAndDownSample()
        {
            int w = _blurRtv.Width;
            int h = _blurRtv.Height;

            GorgonTexture2DView src = _sceneSrv;
            for (int i = 0; i < _sampleTargets.Length; ++i)
            {
                _targetInfo.Width = w;
                _targetInfo.Height = h;

                (GorgonRenderTarget2DView up, GorgonRenderTarget2DView down) targets = (i == 0 ? _blurRtv : Graphics.TemporaryTargets.Rent(_targetInfo, $"UpSample_{i}", false),
                                                                                                            Graphics.TemporaryTargets.Rent(_targetInfo, $"DownSample_{i}", false));

                var texelSize = new Vector2(1.0f / src.Width, 1.0f / src.Height);
                _textureSettingsBuffer.Buffer.SetData(in texelSize);

                Graphics.SetRenderTarget(targets.down);

                Renderer.Begin(i == 0 ? _filterBatchState : _downSampleBatchState);
                Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, targets.down.Width, targets.down.Height),
                                            GorgonColor.White,
                                            src,
                                            new DX.RectangleF(0, 0, 1, 1),
                                            textureSampler: GorgonSamplerState.Default);
                Renderer.End();

                src = targets.down.GetShaderResourceView();

                // Update our batch state if our target has changed.
                if (_sampleTargets[i].down != targets.down)
                {
                    _sampleTargetStates[i] = _downSampleStateBuilder.Clear()
                                                                    .BlendState(GorgonBlendState.NoBlending)
                                                                    .PixelShaderState(_shaderBuilder.Clear()
                                                                                                    .Shader(_upSampleShader)
                                                                                                    .SamplerState(GorgonSamplerState.Default, 1)
                                                                                                    .ShaderResource(src, 1)
                                                                                                    .ConstantBuffer(_settingsBuffer, 1)
                                                                                                    .ConstantBuffer(_textureSettingsBuffer, 2)
                                                                                                    .Build(PixelShaderAllocator))
                                                                    .Build(BatchStateAllocator);
                }

                _sampleTargets[i] = targets;

                w = (w >> 1).Max(1);
                h = (h >> 1).Max(1);
            }
        }

        /// <summary>
        /// Function to perform upsampling on the bloom texture.
        /// </summary>
        private void UpSample()
        {
            GorgonTexture2DView src = _sampleTargets[^1].down.GetShaderResourceView();

            for (int i = _sampleTargets.Length - 2; i >= 0; --i)
            {
                (GorgonRenderTarget2DView up, GorgonRenderTarget2DView _) = _sampleTargets[i];

                var texelSize = new Vector2(1.0f / src.Width, 1.0f / src.Height);
                _textureSettingsBuffer.Buffer.SetData(in texelSize);

                Graphics.SetRenderTarget(up);

                Renderer.Begin(_sampleTargetStates[i]);
                Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, up.Width, up.Height),
                    GorgonColor.White,
                    src,
                    new DX.RectangleF(0, 0, 1, 1),
                    textureSampler: GorgonSamplerState.Default);
                Renderer.End();

                src = up.GetShaderResourceView();
            }
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _textureSettingsBuffer?.Dispose();
            _settingsBuffer?.Dispose();
            _finalPassShader?.Dispose();
            _downSampleShader?.Dispose();
            _upSampleShader?.Dispose();
            _filterShader?.Dispose();

            _blurSrv?.Dispose();
            _blurRtv?.Dispose();
            _sceneSrv?.Dispose();
            _sceneRtv?.Dispose();
        }

        /// <summary>
        /// Function to compile the shaders needed for the effect.
        /// </summary>
        private void CompileShaders()
        {
            _finalPassShader?.Dispose();
            _downSampleShader?.Dispose();
            _upSampleShader?.Dispose();
            _filterShader?.Dispose();

            if (_lowQuality)
            {
                Macros.Add(_lowQualityMacro);
            }
            else
            {
                Macros.Remove(_lowQualityMacro);
            }

            _filterShader = CompileShader<GorgonPixelShader>(Resources.HdrBloom, "FilterBrightPass");
            _downSampleShader = CompileShader<GorgonPixelShader>(Resources.HdrBloom, "DownSampleBlur");
            _upSampleShader = CompileShader<GorgonPixelShader>(Resources.HdrBloom, "UpSampleBlur");
            _finalPassShader = CompileShader<GorgonPixelShader>(Resources.HdrBloom, "FinalPass");

            _filterBatchState = null;
            _downSampleBatchState = null;
            _pass0State = null;
            _finalPassBatchState = null;
            _sceneTargetInfo.Height = _sceneTargetInfo.Width = 0;
            _sampleTargets = null;
        }

        /// <summary>Function called to build a new (or return an existing) 2D batch state.</summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="statesChanged">
        ///   <b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
        {

            if (_filterBatchState is null)
            {
                _filterBatchState = builders.BatchBuilder
                                                .Clear()
                                                .PixelShaderState(_shaderBuilder.Clear()
                                                                                .Shader(_filterShader)
                                                                                .ConstantBuffer(_settingsBuffer, 1)
                                                                                .ConstantBuffer(_textureSettingsBuffer, 2)
                                                                                .Build())
                                                .BlendState(GorgonBlendState.NoBlending)
                                                .Build();
            }

            if (_downSampleBatchState is null)
            {
                _downSampleBatchState = builders.BatchBuilder
                                                .Clear()
                                                .PixelShaderState(_shaderBuilder.Clear()
                                                                                .Shader(_downSampleShader)
                                                                                .ConstantBuffer(_textureSettingsBuffer, 2)
                                                                                .Build())
                                                .BlendState(GorgonBlendState.NoBlending)
                                                .Build();
            }

            switch (passIndex)
            {
                case 0:
                    // This state shouldn't change that often in typical use cases, so we'll not bother with an allocator here.
                    if ((statesChanged) || (_pass0State is null))
                    {
                        _pass0State = builders.BatchBuilder
                                                .Clear()
                                                .ResetShader(ShaderType.Pixel)
                                                .Build();
                    }

                    return _pass0State;
                case 1:
                    if ((_finalPassBatchState is not null) 
                        && (_finalPassBatchState.PixelShaderState.ShaderResources[1] == _blurSrv)
                        && (_finalPassBatchState.PixelShaderState.ShaderResources[2] == DirtTexture))
                    {
                        return _finalPassBatchState;
                    }

                    // This guy changes frequently, so we'll use an allocator to ensure our GCs are kept minimal.
                    _finalPassBatchState = builders.BatchBuilder
                                                    .Clear()
                                                    .BlendState(GorgonBlendState.NoBlending)
                                                    .PixelShaderState(_shaderBuilder.Clear()
                                                                                    .Shader(_finalPassShader)
                                                                                    .SamplerState(GorgonSamplerState.Default, 1)
                                                                                    .ShaderResource(_blurSrv, 1)
                                                                                    .SamplerState(GorgonSamplerState.Default, 2)
                                                                                    .ShaderResource(DirtTexture ?? Renderer.EmptyBlackTexture, 2)
                                                                                    .ConstantBuffer(_settingsBuffer, 1)
                                                                                    .ConstantBuffer(_textureSettingsBuffer, 2)
                                                                                    .Build(_finalPassPixelShaderAllocator))
                                                    .Build(_finalPassBatchAllocator);
                    return _finalPassBatchState;
            }

            return Gorgon2DBatchState.NoBlend;
        }

        /// <summary>Function called prior to rendering.</summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="sizeChanged">
        ///   <b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
        /// targets (if applicable).
        /// </remarks>
        protected override void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
        {
            // Check to see if we need to rebuild our shaders.
            if (((Macros.Contains(_lowQualityMacro)) && (!_lowQuality))
                || (!Macros.Contains(_lowQualityMacro) && (_lowQuality)))
            {
                CompileShaders();
            }

            // Allocate a render target that matches the size of our output.
            // We will render our data into this.
            if ((_sceneTargetInfo.Width != output.Width) || (_sceneTargetInfo.Height != output.Height))
            {
                _sceneSrv?.Dispose();
                _blurSrv?.Dispose();
                _sceneRtv?.Dispose();
                _blurRtv?.Dispose();

                _blurTargetInfo.Width = _sceneTargetInfo.Width = output.Width;
                _blurTargetInfo.Height = _sceneTargetInfo.Height = output.Height;

                _sceneRtv = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, _sceneTargetInfo);
                _blurRtv = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, _blurTargetInfo);

                _sceneSrv = _sceneRtv.GetShaderResourceView();
                _blurSrv = _blurRtv.GetShaderResourceView();
            }

            _sceneRtv.Clear(GorgonColor.BlackTransparent);

            _targetInfo.Width = (_sceneSrv.Width >> 1).Max(1);
            _targetInfo.Height = (_sceneSrv.Height >> 1).Max(1);
        }

        /// <summary>Function called prior to rendering a pass.</summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <returns>A <see cref="PassContinuationState"/> to instruct the effect on how to proceed.</returns>
        /// <remarks>Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.</remarks>
        protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, GorgonCameraCommon camera)
        {
            if ((_blurAmount.EqualsEpsilon(0)) || (_intensity.EqualsEpsilon(0)))
            {
                return PassContinuationState.Stop;
            }

            switch (passIndex)
            {
                case 0:
                    Graphics.SetRenderTarget(_sceneRtv);
                    break;
                case 1:
                    var texelSize = new Vector2(1.0f / _blurRtv.Width, 1.0f / _blurRtv.Height);
                    _textureSettingsBuffer.Buffer.SetData(in texelSize);

                    Graphics.SetRenderTarget(output);
                    break;
            }

            return PassContinuationState.Continue;
        }

        /// <summary>
        /// Function to render the effect to a render target.
        /// </summary>
        /// <param name="texture">The texture to render.</param>
        /// <param name="output">The final render target output.</param>
        public void Render(GorgonTexture2DView texture, GorgonRenderTargetView output)
        {
            BeginRender(output, null, null, null);

            // Filter down and up prior to sending out the effect.
            if (BeginPass(0, _sceneRtv) == PassContinuationState.Continue)
            {
                Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _sceneRtv.Width, _sceneRtv.Height),
                                             GorgonColor.White,
                                             texture,
                                             new DX.RectangleF(0, 0, 1, 1));
                EndPass(0, _sceneRtv);
            }
            else
            {
                EndRender(null);
                return;
            }

            SetupFilterAndBlur(new DX.Size2(output.Width, output.Height));
            FilterAndDownSample();
            UpSample();
            ReturnSampleTargets();

            if (BeginPass(1, output) == PassContinuationState.Continue)
            {
                Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height),
                                             GorgonColor.White,
                                             _sceneSrv,
                                             new DX.RectangleF(0, 0, 1, 1),
                                             textureSampler: GorgonSamplerState.Default);
                EndPass(1, output);
                EndRender(output);
            }
            else
            {
                EndRender(null);
            }            
        }

        /// <summary>Function called to initialize the effect.</summary>
        /// <remarks>Applications must implement this method to ensure that any required resources are created, and configured for the effect.</remarks>
        protected override void OnInitialize()
        {
            _settingsBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, in _settings, "Bloom Settings Buffer");
            _textureSettingsBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo(Unsafe.SizeOf<Vector2>())
            {
                Name = "Texture Settings Buffer",                
                Usage = ResourceUsage.Dynamic
            });

            CompileShaders();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DBloomEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DBloomEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_BLOOM, Resources.GOR2D_EFFECT_BLOOM_DESC, 2)
        {
            // Set up common properties for our targets.
            _targetInfo = new BloomTextureInfo
            {
                Name = "Bloom Downsample Target"
            };

            _sceneTargetInfo = new BloomTextureInfo(_targetInfo, "Bloom Source Image");
            _blurTargetInfo = new BloomTextureInfo(_targetInfo, "Blurred/Filtered Target");
        }    
        #endregion
    }
}
