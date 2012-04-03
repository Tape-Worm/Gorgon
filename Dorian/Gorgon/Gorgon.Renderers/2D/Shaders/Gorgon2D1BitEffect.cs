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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders an image as if it were 1 bit image.
	/// </summary>
	public class Gorgon2D1BitEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed = false;												// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _1bitBuffer = null;							// Constant buffer for the 1 bit information.
		private GorgonDataStream _1bitStream = null;								// Stream used to write to the buffer.
		private GorgonMinMaxF _whiteRange = new GorgonMinMaxF(0.5f, 1.0f);			// Range of values that are considered "on".
		private bool _useAverage = false;											// Flag to calculate using an average of the texel colors.
		private bool _invert = false;												// Flag to invert the texel colors.
		private bool _isUpdated = true;												// Flag to indicate that the parameters were updated.
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
		public GorgonMinMaxF OnBitRange
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
			_1bitStream.Position = 0;
			_1bitStream.Write(_whiteRange.Minimum);
			_1bitStream.Write(_whiteRange.Maximum);

			// .NET uses 1 byte for BOOL, HLSL uses 4 bytes.
			_1bitStream.Write(_useAverage);
			_1bitStream.Write<byte>(0);
			_1bitStream.Write<byte>(0);
			_1bitStream.Write<byte>(0);
			_1bitStream.Write(_invert);
			_1bitStream.Write<byte>(0);
			_1bitStream.Write<byte>(0);
			_1bitStream.Write<byte>(0);
			_1bitStream.Position = 0;
			_1bitBuffer.Update(_1bitStream);
			_isUpdated = false;
		}

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// TRUE to continue rendering, FALSE to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			base.OnBeforeRender();

			if (_isUpdated)
				SetValues();

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _1bitBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[2] = _1bitBuffer;

			return true;
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
					if (_1bitBuffer != null)
						_1bitBuffer.Dispose();
					if (_1bitStream != null)
						_1bitStream.Dispose();
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
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.1Bit.PS", "GorgonPixelShader1Bit", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.1Bit.PS", "GorgonPixelShader1Bit", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif

			_1bitStream = new GorgonDataStream(16);
			_1bitBuffer = Graphics.Shaders.CreateConstantBuffer(16, false);
		}
		#endregion
	}
}
