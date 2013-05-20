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
// Created: Sunday, April 08, 2012 2:36:05 PM
// 
#endregion

using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders images burn/dodge effect.
	/// </summary>
	public class Gorgon2DBurnDodgeEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed;											// Flag to indicate that the object was disposed.
		private readonly GorgonConstantBuffer _burnDodgeBuffer;			// Burn/dodge buffer.
		private readonly GorgonDataStream _burnDodgeStream;				// Burn/dodge stream.
		private bool _useDodge;											// Flag to indicate that we want a color dodge effect.
		private bool _useLinear;										// Flag to use linear versions of the burn/dodge effects.
		private bool _isUpdated = true;									// Flag to indicate that the effect parameters are updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use a burn or dodge effect.
		/// </summary>
		public bool UseDodge
		{
			get
			{
				return _useDodge;
			}
			set
			{
				if (_useDodge != value)
				{
					_useDodge = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether to use a linear burn/dodge.
		/// </summary>
		public bool UseLinear
		{
			get
			{
				return _useLinear;
			}
			set
			{
				if (_useLinear != value)
				{
					_useLinear = value;
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
				_burnDodgeStream.Position = 0;
				_burnDodgeStream.Write(_useDodge);
				_burnDodgeStream.Write<byte>(0);
				_burnDodgeStream.Write<byte>(0);
				_burnDodgeStream.Write<byte>(0);
				_burnDodgeStream.Write(_useLinear);
				_burnDodgeStream.Write<byte>(0);
				_burnDodgeStream.Write<byte>(0);
				_burnDodgeStream.Write<byte>(0);
				_burnDodgeStream.Position = 0;
				_burnDodgeBuffer.Update(_burnDodgeStream);
				_isUpdated = false;
			}

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _burnDodgeBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[2] = _burnDodgeBuffer;
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
				}

				PixelShader = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DBurnDodgeEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal Gorgon2DBurnDodgeEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.BurnDodge", 1)
		{
			
#if DEBUG
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.BurnDodge.PS", "GorgonPixelShaderBurnDodge", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.BurnDodge.PS", "GorgonPixelShaderBurnDodge", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif
            _burnDodgeBuffer = Graphics.Shaders.CreateConstantBuffer(16, "Gorgon2DBurnDodgeEffect Constant Buffer");
			_burnDodgeStream = new GorgonDataStream(16);
		}
		#endregion
	}
}
