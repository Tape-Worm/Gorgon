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

using System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders the edges of an image with Sobel edge detection.
	/// </summary>
	public class Gorgon2DSobelEdgeDetectEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed;									// Flag to indicate that the object was disposed.
		private readonly GorgonConstantBuffer _sobelBuffer;		// Buffer for the sobel edge detection.
		private readonly GorgonDataStream _sobelStream;			// Stream for the sobel edge detection.
		private Vector2 _sobelTexelSize = Vector2.Zero;			// Size of a texel.
		private GorgonColor _sobelLineColor = Color.Black;		// Line color for the edges.
		private float _sobelThreshold = 0.75f;					// Threshhold for edges.
		private bool _isUpdated = true;							// Flag to indicate that the parameters have been updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the threshold value for the edges.
		/// </summary>
		public float EdgeThreshold
		{
			get
			{
				return _sobelThreshold;
			}
			set
			{
				if (value < 0)
					value = 0.0f;
				if (value > 1.0f)
					value = 1.0f;

				if (value != _sobelThreshold)
				{
					_sobelThreshold = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the line thickness.
		/// </summary>
		public Vector2 LineThickness
		{
			get
			{
				return _sobelTexelSize;
			}
			set
			{
				if (_sobelTexelSize != value)
				{
					_sobelTexelSize = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the color for the edges.
		/// </summary>
		public GorgonColor LineColor
		{
			get
			{
				return _sobelLineColor;
			}
			set
			{
				if (_sobelLineColor != value)
				{
					_sobelLineColor = value;
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
				_sobelStream.Position = 0;
				_sobelStream.Write(_sobelLineColor);
				_sobelStream.Write(_sobelTexelSize);
				_sobelStream.Write(_sobelThreshold);
				_sobelStream.Write<byte>(0);
				_sobelStream.Write<byte>(0);
				_sobelStream.Write<byte>(0);
				_sobelStream.Position = 0;

				_sobelBuffer.Update(_sobelStream);

				_isUpdated = false;
			}

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _sobelBuffer)
			    Gorgon2D.PixelShader.ConstantBuffers[2] = _sobelBuffer;
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
					if (_sobelBuffer != null)
						_sobelBuffer.Dispose();
					if (_sobelStream != null)
						_sobelStream.Dispose();

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
		/// Initializes a new instance of the <see cref="Gorgon2DSobelEdgeDetectEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal Gorgon2DSobelEdgeDetectEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.GrayScale", 1)
		{
			
#if DEBUG
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.SobelEdgeDetect.PS", "GorgonPixelShaderSobelEdge", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.SobelEdgeDetect.PS", "GorgonPixelShaderSobelEdge", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif
			_sobelBuffer = Graphics.Shaders.CreateConstantBuffer(32, false);
			_sobelStream = new GorgonDataStream(32);
		}
		#endregion
	}
}
