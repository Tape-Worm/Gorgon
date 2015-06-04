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
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Native;

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
			private readonly int _waveType;							// Wave type.

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

			/// <summary>
			/// Property to return the type of wave.
			/// </summary>
			public WaveType WaveType
			{
				get
				{
					return (WaveType)_waveType;
				}
			}

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
		private bool _disposed;									// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _waveBuffer;		        // Constant buffer for the wave information.
		private Settings _settings;								// Settings for the effect shader.
		private bool _isUpdated = true;							// Flag to indicate that the parameters were updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the wave type.
		/// </summary>
		public WaveType WaveType
		{
			get
			{
				return _settings.WaveType;
			}
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
			get
			{
				return _settings.Amplitude;
			}
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
			get
			{
				return _settings.Period;
			}
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
			get
			{
				return _settings.Length;
			}
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
			get
			{
				return _settings.LengthScale;
			}
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

		/// <summary>
		/// Property to set or return the function used to render the scene when posterizing.
		/// </summary>
		/// <remarks>Use this to render the image to be blurred.</remarks>
		public Action<GorgonEffectPass> RenderScene
		{
			get
			{
				return Passes[0].RenderAction;
			}
			set
			{
				Passes[0].RenderAction = value;
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
        /// </remarks>
	    protected override void OnInitialize()
	    {
	        base.OnInitialize();

            Passes[0].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Wave.PS", "GorgonPixelShaderWaveEffect", "#GorgonInclude \"Gorgon2DShaders\"");

            _waveBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DWaveEffect Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = DirectAccess.SizeOf<Settings>()
                                                                });

            _settings = new Settings(10.0f, 50.0f, 0.0f, 100.0f, WaveType.Horizontal);
        }

	    /// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			if (_isUpdated)
			{
				_waveBuffer.Update(ref _settings);
				_isUpdated = false;
			}

            RememberConstantBuffer(ShaderType.Pixel, 1);

			Gorgon2D.PixelShader.ConstantBuffers[1] = _waveBuffer;

			return base.OnBeforeRender();
		}

        /// <summary>
        /// Function called after rendering ends.
        /// </summary>
	    protected override void OnAfterRender()
	    {
            RestoreConstantBuffer(ShaderType.Pixel, 1);

	        base.OnAfterRender();
	    }

	    /// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (Passes[0].PixelShader != null)
					{
						Passes[0].PixelShader.Dispose();
					}

					if (_waveBuffer != null)
					{
						_waveBuffer.Dispose();
					}
				}

				Passes[0].PixelShader = null; 
				_disposed = true;
			}
			
			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DWaveEffect"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this effect.</param>
		/// <param name="name">The name of the effect.</param>
		internal Gorgon2DWaveEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}
