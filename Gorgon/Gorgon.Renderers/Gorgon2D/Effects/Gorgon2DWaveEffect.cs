﻿#region MIT.
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
using DX = SharpDX;
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
                             new GorgonShaderMacro("WAVE_EFFECT")
                         };

            _waveShader = PixelShaderBuilder
                          .Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
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
		    if (!_isUpdated)
		    {
		        return;
		    }

		    _waveBuffer.Buffer.SetData(ref _settings);
		    _isUpdated = false;
		}

	    /// <summary>
	    /// Function called prior to rendering a pass.
	    /// </summary>
	    /// <param name="passIndex">The index of the pass to render.</param>
	    /// <param name="output">The final render target that will receive the rendering from the effect.</param>
	    /// <returns>A <see cref="PassContinuationState"/> to instruct the effect on how to proceed.</returns>
	    /// <remarks>
	    /// <para>
	    /// Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.
	    /// </para>
	    /// </remarks>
	    /// <seealso cref="PassContinuationState"/>
	    protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output)
	    {
	        if (Graphics.RenderTargets[0] != output)
	        {
                Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
	        }

	        return PassContinuationState.Continue;
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
            shader?.Dispose();
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
	    /// <param name="renderMethod">The method used to render a scene for the effect.</param>
	    /// <param name="output">The render target that will receive the final render data.</param>
	    /// <remarks>
	    /// <para>
	    /// Applications must implement this in order to see any results from the effect.
	    /// </para>
	    /// </remarks>
	    protected override void OnRenderPass(int passIndex, Action<int, int, DX.Size2> renderMethod, GorgonRenderTargetView output)
	    {
            renderMethod(passIndex, PassCount, new DX.Size2(output.Width, output.Height));
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
