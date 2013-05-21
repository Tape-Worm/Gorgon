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
// Created: Thursday, April 05, 2012 8:23:51 AM
// 
#endregion

using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders a posterized image.
	/// </summary>
	public class Gorgon2DPosterizedEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed;										// Flag to indicate that the object was disposed.
		private readonly GorgonConstantBuffer _posterizeBuffer;		// Buffer for the posterize effect.
		private readonly GorgonDataStream _posterizeStream;			// Stream for the posterize effect.
		private bool _posterizeAlpha;								// Flag to posterize the alpha channel.
		private float _posterizeExponent = 1.0f;					// Posterize exponent.
		private int _posterizeBits = 8;								// Posterize bit count.
		private bool _isUpdated = true;								// Flag to indicate that the parameters have been updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to posterize the alpha channel.
		/// </summary>
		public bool UseAlpha
		{
			get
			{
				return _posterizeAlpha;
			}
			set
			{
				if (_posterizeAlpha != value)
				{
					_posterizeAlpha = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the exponent power for the effect.
		/// </summary>
		public float Power
		{
			get
			{
				return _posterizeExponent;
			}
			set
			{
				if (value < 1e-6f)
					value = 1e-6f;

				if (_posterizeExponent != value)
				{
					_posterizeExponent = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the number of bits to reduce down to for the effect.
		/// </summary>
		public int Bits
		{
			get
			{
				return _posterizeBits;
			}
			set
			{
				if (value < 1)
					value = 1;

				if (_posterizeBits != value)
				{
					_posterizeBits = value;
					_isUpdated = true;
				}
			}
		}
		#endregion

		#region Methods.
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
			{
				_posterizeStream.Position = 0;
				_posterizeStream.Write(_posterizeBits);
				_posterizeStream.Write(_posterizeExponent);
				_posterizeStream.Write(_posterizeAlpha);
				_posterizeStream.Write<byte>(0);
				_posterizeStream.Write<byte>(0);
				_posterizeStream.Write<byte>(0);
				_posterizeStream.Position = 0;

				_posterizeBuffer.Update(_posterizeStream);

				_isUpdated = false;
			}

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _posterizeBuffer)
			    Gorgon2D.PixelShader.ConstantBuffers[2] = _posterizeBuffer;
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
					if (_posterizeBuffer != null)
						_posterizeBuffer.Dispose();
					if (_posterizeStream != null)
						_posterizeStream.Dispose();
					if (PixelShader != null)
						PixelShader.Dispose();
				}

				PixelShader = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DPosterizedEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal Gorgon2DPosterizedEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.GrayScale", 1)
		{
			
#if DEBUG
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Posterized.PS", "GorgonPixelShaderPosterize", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Posterized.PS", "GorgonPixelShaderPosterize", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif
            _posterizeBuffer = Graphics.Shaders.CreateConstantBuffer("Gorgon2DPosterizedEffect Constant Buffer",
			                                                    new GorgonConstantBufferSettings
				                                                    {
					                                                    SizeInBytes = 16
				                                                    });
			_posterizeStream = new GorgonDataStream(16);
		}
		#endregion
	}
}
