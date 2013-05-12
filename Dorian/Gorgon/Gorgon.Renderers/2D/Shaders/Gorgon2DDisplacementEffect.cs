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
// Created: Monday, April 09, 2012 7:14:08 AM
// 
#endregion

using System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders a displacement effect.
	/// </summary>
	public class Gorgon2DDisplacementEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed;													// Flag to indicate that the object was disposed.
		private GorgonRenderTarget _displacementTarget;							// Displacement buffer target.
		private Size _targetSize = new Size(512, 512);							// Displacement target size.
		private BufferFormat _targetFormat = BufferFormat.R8G8B8A8_UIntNormal;	// Format for the displacement target.
		private readonly GorgonConstantBuffer _displacementBuffer;				// Buffer used to send displacement data.
		private readonly GorgonDataStream _displacementStream;					// Stream used to update the displacement buffer.
		private bool _isUpdated = true;											// Flag to indicate that the parameters have been updated.
		private float _displacementStrength = 1.0f;								// Strength of the displacement map.
		private SmoothingMode _lastSmoothMode = SmoothingMode.None;				// Last global smoothing state.
		private GorgonRenderTarget _currentTarget;								// Current render target.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the strength of the displacement map.
		/// </summary>
		public float Strength
		{
			get
			{
				return _displacementStrength;
			}
			set
			{
				if (value < 0.0f)
					value = 0.0f;

				if (_displacementStrength != value)
				{
					_displacementStrength = value;
					_isUpdated = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the displacement render target size.
		/// </summary>
		public Size DisplacementRenderTargetSize
		{
			get
			{
				return _targetSize;
			}
			set
			{
				if (_targetSize != value)
					UpdateDisplacementMap(value, _targetFormat);
			}
		}

		/// <summary>
		/// Property to set or return the format of the displacement render target.
		/// </summary>
		public BufferFormat Format
		{
			get
			{
				return _targetFormat;
			}
			set
			{
				if (_targetFormat != value)
					UpdateDisplacementMap(_targetSize, value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the displacement map render target.
		/// </summary>
		/// <param name="newSize">New size for the target.</param>
		/// <param name="format">Format to use for the displacement target.</param>
		private void UpdateDisplacementMap(Size newSize, BufferFormat format)
		{
			if (_displacementTarget != null)
				_displacementTarget.Dispose();

			_displacementTarget = null;

			if ((newSize.Width <= 0) || (newSize.Height <= 0))
				return;

			_displacementTarget = Graphics.Output.CreateRenderTarget("Effect.Displacement.RT", new GorgonRenderTargetSettings()
			{
				Width = newSize.Width,
				Height = newSize.Height,
				DepthStencilFormat = BufferFormat.Unknown,
				Format = format,
				MultiSample = new GorgonMultisampling(1, 0)				
			});

			_targetFormat = format;
			_targetSize = newSize;
			_isUpdated = true;
		}

		/// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public override void FreeResources()
		{
			if (_displacementTarget != null)
				_displacementTarget.Dispose();
			_displacementTarget = null;
		}

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// TRUE to continue rendering, FALSE to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			if (_displacementTarget == null)
				UpdateDisplacementMap(_targetSize, _targetFormat);

#if DEBUG
			if (_displacementTarget == null)
				throw new GorgonException(GorgonResult.CannotWrite, "No displacement target size was set.");
#endif

			_currentTarget = Gorgon2D.Target;

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _displacementBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[2] = _displacementBuffer;

			return base.OnBeforeRender();
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
				_displacementStream.Position = 0;
				_displacementStream.Write(new Vector4(1.0f / _targetSize.Width, 1.0f / _targetSize.Height, _displacementStrength, 0));
				_displacementStream.Position = 0;
				_displacementBuffer.Update(_displacementStream);
				_isUpdated = false;
			}

			if (passIndex == 0)
			{
				_displacementTarget.Clear(GorgonColor.Transparent);
				Gorgon2D.PixelShader.Current = null;
				Gorgon2D.Target = _displacementTarget;
			}
			else
			{
				_lastSmoothMode = Gorgon2D.Drawing.SmoothingMode;
				Gorgon2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
				Gorgon2D.PixelShader.Current = PixelShader;
				Gorgon2D.Target = _currentTarget;
				if (Gorgon2D.PixelShader.Resources[1] != _displacementTarget.Texture.View)
					Gorgon2D.PixelShader.Resources[1] = _displacementTarget.Texture.View;
			}				
		}

		/// <summary>
		/// Function called after a pass has rendered.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnAfterRenderPass(int passIndex)
		{
			base.OnAfterRenderPass(passIndex);
			if ((passIndex == 1) && (Gorgon2D.Drawing.SmoothingMode != _lastSmoothMode))
				Gorgon2D.Drawing.SmoothingMode = _lastSmoothMode;
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
		/// Initializes a new instance of the <see cref="Gorgon2DDisplacementEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that created this object.</param>
		internal Gorgon2DDisplacementEffect(Gorgon2D gorgon2D)
			: base(gorgon2D, "Effect.2D.Displacement", 2)
		{
			
#if DEBUG
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.DisplacementDecoder.PS", "GorgonPixelShaderDisplacementDecoder", "#GorgonInclude \"Gorgon2DShaders\"", true);
#else
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.DisplacementDecoder.PS", "GorgonPixelShaderDisplacementDecoder", "#GorgonInclude \"Gorgon2DShaders\"", false);
#endif

			_displacementBuffer = Graphics.Shaders.CreateConstantBuffer(16, false);
			_displacementStream = new GorgonDataStream(16);
		}
		#endregion
	}
}
