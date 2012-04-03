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
using SlimMath;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that sharpens (and optionally embosses) an image.
	/// </summary>
	public class Gorgon2DSharpenEmbossEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed = false;												// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _sharpenEmbossBuffer = null;					// Constant buffer for the sharpen/emboss information.
		private GorgonDataStream _sharpenEmbossStream = null;						// Stream used to write to the buffer.
		private float _amount = 1.0f;												// Amount to sharpen/emboss
		private bool _useEmboss = false;											// Flag to indicate that the image should be embossed or not.
		private bool _isUpdated = true;												// Flag to indicate that the parameters were updated.
		private Vector2 _size = Vector2.Zero;										// Area to emboss.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use embossing instead of sharpening.
		/// </summary>
		public bool UseEmbossing
		{
			get
			{
				return _useEmboss;
			}
			set
			{
				if (_useEmboss != value)
				{
					_useEmboss = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the amount to sharpen/emboss.
		/// </summary>
		public float Amount
		{
			get
			{
				return _amount;
			}
			set
			{
				if (_amount != value)
				{
					_amount = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the area (width/height) that will receive the sharpen/emboss.
		/// </summary>
		/// <remarks>Ideally this should be set to the size of the object being rendered, otherwise shimmering artifacts may occour.</remarks>
		public Vector2 Area
		{
			get
			{
				return _size;
			}
			set
			{
				if (_size != value)
				{
					_size = value;
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
			_sharpenEmbossStream.Position = 0;
			_sharpenEmbossStream.Write(1.0f / _size.X);
			_sharpenEmbossStream.Write(1.0f / _size.Y);
			_sharpenEmbossStream.Write(_amount);

			// .NET uses 1 byte for BOOL, HLSL uses 4 bytes.
			_sharpenEmbossStream.Write(_useEmboss);
			_sharpenEmbossStream.Write<byte>(0);
			_sharpenEmbossStream.Write<byte>(0);
			_sharpenEmbossStream.Write<byte>(0);
			_sharpenEmbossStream.Position = 0;
			_sharpenEmbossBuffer.Update(_sharpenEmbossStream);
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

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _sharpenEmbossBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[2] = _sharpenEmbossBuffer;

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
					if (_sharpenEmbossBuffer != null)
						_sharpenEmbossBuffer.Dispose();
					if (_sharpenEmbossStream != null)
						_sharpenEmbossStream.Dispose();
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
		internal Gorgon2DSharpenEmbossEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.SharpenEmboss", 1)
		{
			
#if DEBUG
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.SharpenEmboss.PS", "GorgonPixelShaderSharpenEmboss", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.SharpenEmboss.PS", "GorgonPixelShaderSharpenEmboss", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif

			_sharpenEmbossStream = new GorgonDataStream(16);
			_sharpenEmbossBuffer = Graphics.Shaders.CreateConstantBuffer(16, false);
		}
		#endregion
	}
}
