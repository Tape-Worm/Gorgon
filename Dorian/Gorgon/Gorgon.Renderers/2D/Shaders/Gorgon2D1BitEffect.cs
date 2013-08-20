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
// Created: Monday, April 02, 2012 2:59:16 PM
// 
#endregion

using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders an image as if it were 1 bit image.
	/// </summary>
	public class Gorgon2D1BitEffect
		: Gorgon2DEffect_GOINGBYEBYE2
	{
		#region Variables.
		private bool _disposed;												// Flag to indicate that the object was disposed.
		private readonly GorgonConstantBuffer _1BitBuffer;					// Constant buffer for the 1 bit information.
		private readonly GorgonDataStream _1BitStream;						// Stream used to write to the buffer.
		private GorgonRangeF _whiteRange = new GorgonRangeF(0.5f, 1.0f);	// Range of values that are considered "on".
		private bool _useAverage;											// Flag to calculate using an average of the texel colors.
		private bool _invert;												// Flag to invert the texel colors.
		private bool _useAlpha = true;										// Flag to indicate that the alpha channel should be included.
		private bool _isUpdated = true;										// Flag to indicate that the parameters were updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use an average of the texel colors or to use a grayscale calculation.
		/// </summary>
		public bool UseAverage
		{
			get
			{
				return _useAverage;
			}
			set
			{
				if (_useAverage != value)
				{
					_useAverage = value;
					_isUpdated = true;
				}
			}
		}


		/// <summary>
		/// Property to set or return whether the alpha channel should be included in the conversion.
		/// </summary>
		public bool ConvertAlphaChannel
		{
			get
			{
				return _useAlpha;
			}
			set
			{
				if (_useAlpha != value)
				{
					_useAlpha = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether to invert the texel colors.
		/// </summary>
		public bool Invert
		{
			get
			{
				return _invert;
			}
			set
			{
				if (_invert != value)
				{
					_invert = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the range of values that are considered to be "on".
		/// </summary>
		public GorgonRangeF Threshold
		{
			get
			{
				return _whiteRange;
			}
			set
			{
				if (_whiteRange != value)
				{
					_whiteRange = value;
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
			_1BitStream.Position = 0;
			_1BitStream.Write(_whiteRange.Minimum);
			_1BitStream.Write(_whiteRange.Maximum);

			// .NET uses 1 byte for BOOL, HLSL uses 4 bytes.
			_1BitStream.Write(_useAverage);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write(_invert);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write(_useAlpha);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write<byte>(0);
			_1BitStream.Write<byte>(0);
			_1BitStream.Position = 0;
			_1BitBuffer.Update(_1BitStream);
			_isUpdated = false;
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

			if (Gorgon2D.PixelShader.ConstantBuffers[1] != _1BitBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[1] = _1BitBuffer;
		}

		/// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public override void FreeResources()
		{			
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
					if (disposing)
					{
						if (PixelShader != null)
							PixelShader.Dispose();
					}

					PixelShader = null;

					if (_1BitBuffer != null)
						_1BitBuffer.Dispose();
					if (_1BitStream != null)
						_1BitStream.Dispose();
				}

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
		internal Gorgon2D1BitEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.1Bit", 1)
		{
			
#if DEBUG
			PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.1Bit.PS", "GorgonPixelShader1Bit", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.1Bit.PS", "GorgonPixelShader1Bit", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif

			_1BitStream = new GorgonDataStream(32);
			_1BitBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2D1BitEffect Constant Buffer",
																new GorgonConstantBufferSettings
																{
																	SizeInBytes = 32
																});
		}
		#endregion
	}
}
