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
// Created: Wednesday, April 04, 2012 12:35:23 PM
// 
#endregion

using System;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders an inverted image.
	/// </summary>
	public class Gorgon2DInvertEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed;										// Flag to indicate that the object was disposed.
		private readonly GorgonConstantBuffer _invertBuffer;		// Buffer for the inversion effect.
		private readonly GorgonDataStream _invertStream;			// Stream for the invert effect.
		private bool _invertAlpha;									// Flag to invert the alpha channel.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to invert the alpha channel.
		/// </summary>
		public bool InvertAlpha
		{
			get
			{
				return _invertAlpha;
			}
			set
			{
				if (_invertAlpha != value)
				{
					_invertAlpha = value;
					_invertStream.Position = 0;
					_invertStream.Write(value);
					_invertStream.Write<byte>(0);
					_invertStream.Write<byte>(0);
					_invertStream.Write<byte>(0);
					_invertStream.Position = 0;
					_invertBuffer.Update(_invertStream);
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to render a specific pass while using this effect.
		/// </summary>
		/// <param name="renderMethod">Method to use to render the data.</param>
		/// <param name="passIndex">Index of the pass to render.</param>
		/// <remarks>The <paramref name="renderMethod"/> is an action delegate that must be defined with an integer value.  The parameter indicates which pass the rendering is currently on.</remarks>
		protected override void RenderImpl(Action<int> renderMethod, int passIndex)
		{
			if (Gorgon2D.PixelShader.ConstantBuffers[1] != _invertBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[1] = _invertBuffer;

			base.RenderImpl(renderMethod, passIndex);
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
					if (_invertBuffer != null)
						_invertBuffer.Dispose();
					if (_invertStream != null)
						_invertStream.Dispose();

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
		/// Initializes a new instance of the <see cref="Gorgon2DInvertEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal Gorgon2DInvertEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.GrayScale", 1)
		{
			
#if DEBUG
			PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Invert.PS", "GorgonPixelShaderInvert", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Invert.PS", "GorgonPixelShaderInvert", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif
			_invertBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DInvertEffect Constant Buffer",
																new GorgonConstantBufferSettings
																{
																	SizeInBytes = 16
																});
			_invertStream = new GorgonDataStream(16);
		}
		#endregion
	}
}
