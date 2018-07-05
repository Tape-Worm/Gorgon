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
// Created: Monday, April 02, 2012 1:41:47 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Type of wave effect.
    /// </summary>
    public enum WaveType
	{
		/// <summary>
		/// A horizontal wave.
		/// </summary>
		Horizontal = 0,
		/// <summary>
		/// A vertical wave.
		/// </summary>
		Vertical = 1,
		/// <summary>
		/// Both horizontal and vertical.
		/// </summary>
		Both = 2
	}

	/// <summary>
	/// An effect that renders a wavy image.
	/// </summary>
	public class Gorgon2DWaveEffect
		: Gorgon2DEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for the effect shader.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Size = 32)]
		private struct Settings
		{
			/// <summary>
			/// Amplitude for the wave.
			/// </summary>
			public readonly float Amplitude;						
			/// <summary>
			/// Length of the wave.
			/// </summary>
			public readonly float Length;							
			/// <summary>
			/// Period for the wave.
			/// </summary>
			public readonly float Period;
			/// <summary>
			/// Scale for the wave length.
			/// </summary>
			public readonly float LengthScale;

		    // Wave type.
		    private readonly int _waveType;							

			/// <summary>
			/// Property to return the type of wave.
			/// </summary>
			public WaveType WaveType => (WaveType)_waveType;

			/// <summary>
			/// Initializes a new instance of the <see cref="Settings"/> struct.
			/// </summary>
			/// <param name="amplitude">The amplitude.</param>
			/// <param name="length">The length.</param>
			/// <param name="period">The period.</param>
			/// <param name="scale">Scale for the length.</param>
			/// <param name="waveType">Type of the wave.</param>
			public Settings(float amplitude, float length, float period, float scale, WaveType waveType)
			{
				Amplitude = amplitude;
				Length = length;
				Period = period;
				LengthScale = scale.Max(1.0f);
				_waveType = (int)waveType;
			}
		}
		#endregion

		#region Variables.
	    // Constant buffer for the wave information.
		private GorgonConstantBufferView _waveBuffer;		    
	    // Settings for the effect shader.
		private Settings _settings;								
	    // Flag to indicate that the parameters were updated.
		private bool _isUpdated = true;
        // The shader used to render the wave effect.
	    private Gorgon2DShader<GorgonPixelShader> _waveShader;
        // The batch state for rendering.
	    private Gorgon2DBatchState _batchState;
	    // A method used to render data while the effect is active.
	    private Action _renderCallback;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the wave type.
		/// </summary>
		public WaveType WaveType
        {
            get => _settings.WaveType;
            set
            {
                if (_settings.WaveType == value)
                {
                    return;
                }

                _settings = new Settings(_settings.Amplitude, _settings.Length, _settings.Period, _settings.LengthScale, value);
                _isUpdated = true;
            }
        }

        /// <summary>
        /// Property to set or return the amplitude for the wave.
        /// </summary>
        public float Amplitude
		{
			get => _settings.Amplitude;
            set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_settings.Amplitude == value)
				{
					return;
				}

				_settings = new Settings(value, _settings.Length, _settings.Period, _settings.LengthScale, _settings.WaveType);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the period for the wave.
		/// </summary>
		public float Period
		{
			get => _settings.Period;
		    set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_settings.Period == value)
				{
					return;
				}

				_settings = new Settings(_settings.Amplitude, _settings.Length, value, _settings.LengthScale, _settings.WaveType);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the length of the wave.
		/// </summary>
		public float Length
		{
			get => _settings.Length;
		    set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_settings.Length == value)
				{
					return;
				}

				_settings = new Settings(_settings.Amplitude, value, _settings.Period, _settings.LengthScale, _settings.WaveType);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the scale for the wave length.
		/// </summary>
		public float LengthScale
		{
			get => _settings.LengthScale;
		    set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_settings.LengthScale == value)
				{
					return;
				}

				_settings = new Settings(_settings.Amplitude, _settings.Length, _settings.Period, value, _settings.WaveType);
				_isUpdated = true;
			}
		}
        #endregion

        #region Methods.
        /// <summary>
        /// Function called to initialize the effect.
        /// </summary>
        /// <remarks>Applications must implement this method to ensure that any required resources are created, and configured for the effect.</remarks>
        protected override void OnInitialize()
        {
            _settings = new Settings(10.0f, 50.0f, 0.0f, 100.0f, WaveType.Horizontal);
            _waveBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _settings, "Gorgon2DWaveEffect Constant Buffer", ResourceUsage.Dynamic);

            var macros = new []
                         {
                             new GorgonShaderMacro("WAVE_EFFECT"),
                         };
            
            var shaderBuilder = new Gorgon2DShaderBuilder<GorgonPixelShader>();
            _waveShader = shaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
                                                                                              Resources.BasicSprite,
                                                                                              "GorgonPixelShaderWaveEffect",
                                                                                              GorgonGraphics.IsDebugEnabled,
                                                                                              macros))
                                       .ConstantBuffer(_waveBuffer, 1)
                                       .Build();

            _batchState = BatchStateBuilder
                          .PixelShader(_waveShader)
                          .Build();
        }

	    /// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
		    if (!_isUpdated)
		    {
		        return true;
		    }

		    _waveBuffer.Buffer.SetData(ref _settings);
		    _isUpdated = false;

		    return true;
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

		    GorgonConstantBufferView waveBuffer = Interlocked.Exchange(ref _waveBuffer, null);
		    Gorgon2DShader<GorgonPixelShader> shader = Interlocked.Exchange(ref _waveShader, null);

            waveBuffer?.Dispose();
            shader?.Shader.Dispose();
		}

	    /// <summary>
	    /// Function called to build a new (or return an existing) 2D batch state.
	    /// </summary>
	    /// <param name="passIndex">The index of the current rendering pass.</param>
	    /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
	    /// <returns>The 2D batch state.</returns>
	    protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
	    {
	        if (statesChanged)
	        {
	            _batchState = BatchStateBuilder.Build();
	        }

	        return _batchState;
	    }

	    /// <summary>
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="batchState">The current batch state for the pass.</param>
        /// <param name="camera">The current camera to use when rendering.</param>
        /// <remarks>Applications must implement this in order to see any results from the effect.</remarks>
        protected override void OnRenderPass(int passIndex, Gorgon2DBatchState batchState, Gorgon2DCamera camera)
	    {
	        Renderer.Begin(batchState, camera);
	        _renderCallback();
            Renderer.End();
	    }

	    /// <summary>
        /// Function to render the specified texture using waves.
        /// </summary>
	    /// <param name="renderCallback">A method that is executed to render image data as gray scale.</param>
	    /// <param name="blendState">[Optional] A blending state used to override the default blending state.</param>
	    /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderCallback"/> parameter is <b>null</b>.</exception>
	    /// <remarks>
	    /// <para>
	    /// The <paramref name="renderCallback"/> is a method that users can define to draw whatever is needed as grayscale.  When this method is called, the <see cref="Gorgon2D.Begin"/> and
	    /// <see cref="Gorgon2D.End"/> methods are already taken care of by the effect and will not need to be called during the callback.
	    /// </para>
	    /// <para>
	    /// <note type="warning">
	    /// <para>
	    /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled in <b>DEBUG</b> mode.
	    /// </para>
	    /// </note>
	    /// </para>
	    /// </remarks>
	    public void Wave(Action renderCallback, GorgonBlendState blendState = null, Gorgon2DCamera camera = null)
	    {
            renderCallback.ValidateObject(nameof(renderCallback));
	        _renderCallback = renderCallback;
            Render(blendState, null, null, camera);
	    }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DWaveEffect"/> class.
		/// </summary>
		/// <param name="renderer">The renderer used to render the effect.</param>
		public Gorgon2DWaveEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_WAVE, Resources.GOR2D_EFFECT_WAVE_DESC, 1)
		{
		}
		#endregion
	}
}
