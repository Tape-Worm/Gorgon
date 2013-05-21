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

using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Renderers
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
		#region Variables.
		private bool _disposed;									// Flag to indicate that the object was disposed.
		private readonly GorgonConstantBuffer _waveBuffer;		// Constant buffer for the wave information.
		private readonly GorgonDataStream _waveStream;			// Stream used to write to the buffer.
		private float _amplitude = 0.01f;						// Amplitude for the wave.
		private float _length = 50.0f;							// Length of the wave.
		private float _period;									// Period for the wave.
		private bool _isUpdated = true;							// Flag to indicate that the parameters were updated.
		private WaveType _waveType = WaveType.Horizontal;		// Wave type.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the wave type.
		/// </summary>
		public WaveType WaveType
		{
			get
			{
				return _waveType;
			}
			set
			{
				if (_waveType != value)
				{
					_waveType = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the amplitude for the wave.
		/// </summary>
		public float Amplitude
		{
			get
			{
				return _amplitude;
			}
			set
			{
				if (_amplitude != value)
				{
					_amplitude = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the period for the wave.
		/// </summary>
		public float Period
		{
			get
			{
				return _period;
			}
			set
			{
				if (_period != value)
				{
					_period = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the length of the wave.
		/// </summary>
		public float Length
		{
			get
			{
				return _length;
			}
			set
			{
				if (_length != value)
				{
					_length = value;
					_isUpdated = true;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the shader values.
		/// </summary>
		private void SetValues()
		{
			_waveStream.Position = 0;
			_waveStream.Write(_amplitude);
			_waveStream.Write(_length);
			_waveStream.Write(_period);
			_waveStream.Write(_waveType);
			_waveStream.Position = 0;
			_waveBuffer.Update(_waveStream);
			_isUpdated = false;
		}

		/// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public override void FreeResources()
		{			
		}

		/// <summary>
		/// Function called when a pass is about to start rendering.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnBeforeRenderPass(int passIndex)
		{
			base.OnBeforeRenderPass(passIndex);

			if (_isUpdated)
				SetValues();

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _waveBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[2] = _waveBuffer;

		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (PixelShader != null)
						PixelShader.Dispose();

					if (_waveBuffer != null)
						_waveBuffer.Dispose();
					if (_waveStream != null)
						_waveStream.Dispose();
				}

				PixelShader = null; 
				_disposed = true;
			}
			
			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DWaveEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal Gorgon2DWaveEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.Wave", 1)
		{
			
#if DEBUG
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Wave.PS", "GorgonPixelShaderWaveEffect", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Wave.PS", "GorgonPixelShaderWaveEffect", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif

			_waveStream = new GorgonDataStream(16);
			_waveBuffer = Graphics.Shaders.CreateConstantBuffer("Gorgon2DWaveEffect Constant Buffer",
																new GorgonConstantBufferSettings
																{
																	SizeInBytes = 16
																});
		}
		#endregion
	}
}
