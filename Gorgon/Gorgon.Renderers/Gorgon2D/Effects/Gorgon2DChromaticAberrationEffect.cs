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
// Created: May 18, 2019 9:24:40 AM
// 
#endregion

using System;
using System.Threading;
using DX = SharpDX;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect that simulates lens imperfection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A chromatic aberration is the failure of a lens to focus all wavelengths of light to the same point.  This results in a prism-like effect around the edges of an image.
    /// </para>
    /// <para>
    /// This implementation of chromatic aberration simulates the effect around the corners of the screen to give the appearance of an actual lens, but when setting its <see cref="FullScreen"/> 
    /// property to <b>true</b>, the effect is applied evenly across the screen (and is also cheaper for performance).
    /// </para>
    /// <para>
    /// If you wish to modify the color spectrum used, setting a new <see cref="GorgonTexture1DView"/> on the <see cref="LookupTexture"/> property will let the effect use your custom texture when 
    /// processing the image.
    /// </para>
    /// <para>
    /// This effect is based on the presentation by Mikkel Gjoel 
    /// (<a target="_blank" href="https://github.com/playdeadgames/publications/tree/master/INSIDE">https://github.com/playdeadgames/publications/tree/master/INSIDE</a>) and the code of Erik Faye Lund 
    /// (@kusma) (<a target="_blank" href="https://github.com/kusma/vlee/blob/master/data/postprocess.fx">https://github.com/kusma/vlee/blob/master/data/postprocess.fx</a>).
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonTexture1DView"/>
    /// <seealso cref="LookupTexture"/>
    /// <seealso cref="FullScreen"/>
    public class Gorgon2DChromaticAberrationEffect
		: Gorgon2DEffect
    {
        #region Variables.
		// The default texture to use for look up.
        private GorgonTexture1DView _defaultLut;
        // The default texture to use for look up.
        private GorgonTexture1DView _currentLut;
        // Settings for the effect shader.
        private GorgonConstantBufferView _settings;
		// The pixel shader for the effect.
        private GorgonPixelShader _chromeAbShader;
		// A simplified version of the effect.
        private GorgonPixelShader _simpleChromeAbShader;
		// The batch state for the effect.
        private Gorgon2DBatchState _chromeAbBatchState;
		// Flag to indicate that the simple shader is in use.
        private bool _useSimple;
		// The builder used to create shader states.
        private readonly Gorgon2DShaderStateBuilder<GorgonPixelShader> _shaderStateBuilder = new Gorgon2DShaderStateBuilder<GorgonPixelShader>();		
        #endregion

        #region Properties.
		/// <summary>
        /// Property to set or return the texture for look up.
        /// </summary>
        /// <remarks>
        /// If this property is set to <b>null</b>, then an internal default look up will be used.
        /// </remarks>
		public GorgonTexture1DView LookupTexture
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the intensity of the effect.
        /// </summary>
        /// <remarks>
        /// For best results, set this to a value between 0 and 1. Larger values give more exaggerated effects, which may or may not be desirable depending on your needs.
        /// </remarks>
        public float Intensity
        {
            get;
            set;
        } = 0.45f;

		/// <summary>
        /// Property to return whether or not the effect should cover the render target, or only be used around the corners.
        /// </summary>
        /// <remarks>
        /// Setting this value to <b>true</b> will utilize the entire render target when applying the effect, not just the corners. This will increase the performance of the effect because this method is 
        /// much more simple than the corner aberration state (<c>FullScreen=</c><b>false</b>).
        /// </remarks>
		public bool FullScreen
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            GorgonTexture1DView texture = Interlocked.Exchange(ref _defaultLut, null);
            GorgonPixelShader shader1 = Interlocked.Exchange(ref _chromeAbShader, null);
            GorgonPixelShader shader2 = Interlocked.Exchange(ref _simpleChromeAbShader, null);
            GorgonConstantBufferView cbv = Interlocked.Exchange(ref _settings, null);

            shader1?.Dispose();
            shader2?.Dispose();
            texture?.Dispose();
            cbv?.Dispose();
        }

        /// <summary>Function called to build a new (or return an existing) 2D batch state.</summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="statesChanged">
        ///   <b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
        {
            if ((_useSimple != FullScreen) 
				|| ((LookupTexture == null) && (_currentLut != _defaultLut))
				|| ((LookupTexture != null) && (LookupTexture != _currentLut)))
            {
                GorgonPixelShader current = FullScreen ? _simpleChromeAbShader : _chromeAbShader;
                _currentLut = LookupTexture ?? _defaultLut;

                _chromeAbBatchState = BatchStateBuilder.Clear()
														.BlendState(GorgonBlendState.NoBlending)
														.PixelShaderState(_shaderStateBuilder
																						.Clear()
																						.Shader(current)
																						.ShaderResource(!FullScreen ? _currentLut : null, 1)
																						.SamplerState(!FullScreen ? GorgonSamplerState.Default : null, 1)
																						.ConstantBuffer(_settings, 1))
														.Build();

                _useSimple = FullScreen;
            }
			
            return _chromeAbBatchState;
        }

        /// <summary>Function called prior to rendering a pass.</summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <returns>A <see cref="PassContinuationState"/> to instruct the effect on how to proceed.</returns>
        /// <remarks>Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.</remarks>
        /// <seealso cref="PassContinuationState" />
        protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, IGorgon2DCamera camera)
        {
            DX.Vector2 intensity = FullScreen ? new DX.Vector2((Intensity * 16) * (1.0f / output.Width), (Intensity * 16) * (1.0f / output.Height))
                : new DX.Vector2(Intensity * 0.05f, 0);
            var settings = new DX.Vector4(intensity, output.Width, output.Height);
            _settings.Buffer.SetData(ref settings);

            Graphics.SetRenderTarget(output);

            return PassContinuationState.Continue;
        }

        /// <summary>Function called to render a single effect pass.</summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="renderMethod">The method used to render a scene for the effect.</param>
        /// <param name="output">The render target that will receive the final render data.</param>
        /// <remarks>Applications must implement this in order to see any results from the effect.</remarks>
        protected override void OnRenderPass(int passIndex, Action<int, int, DX.Size2> renderMethod, GorgonRenderTargetView output) => renderMethod(passIndex, PassCount, new DX.Size2(output.Width, output.Height));        

        /// <summary>Function called to initialize the effect.</summary>
        /// <remarks>Applications must implement this method to ensure that any required resources are created, and configured for the effect.</remarks>
        protected override void OnInitialize()
        {
			// Initialize the default look up table.
            using (IGorgonImage image = new GorgonImage(new GorgonImageInfo(ImageType.Image1D, BufferFormat.R8G8B8A8_UNorm)
            {
				Width = 3
            }))
            {
                image.ImageData.ReadAs<int>(0) = GorgonColor.RedPure.ToABGR();
                image.ImageData.ReadAs<int>(4) = GorgonColor.BluePure.ToABGR();
                image.ImageData.ReadAs<int>(8) = GorgonColor.GreenPure.ToABGR();

                _defaultLut = GorgonTexture1DView.CreateTexture(Graphics, new GorgonTexture1DInfo("Default Spectral LUT")
                {
                    Binding = TextureBinding.ShaderResource,
                    Usage = ResourceUsage.Immutable,
                    Width = 3
                }, image);
            }

            _chromeAbShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.ChromaticAberration, "ChromaticAberration", GorgonGraphics.IsDebugEnabled);
			_simpleChromeAbShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.ChromaticAberration, "ChromaticAberrationSimple", GorgonGraphics.IsDebugEnabled);

            _settings = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Chromatic Aberration Settings Buffer")
            {
                SizeInBytes = DX.Vector4.SizeInBytes,
                Usage = ResourceUsage.Default
            });
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Gorgon2DChromaticAberrationEffect"/> class.</summary>
        /// <param name="renderer">The 2D renderer used to render the effect.</param>
        public Gorgon2DChromaticAberrationEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_CHROMATIC_ABBERATION, Resources.GOR2D_EFFECT_CHROMATIC_ABBERATION_DESC, 1)
        {
        }
        #endregion
    }
}
