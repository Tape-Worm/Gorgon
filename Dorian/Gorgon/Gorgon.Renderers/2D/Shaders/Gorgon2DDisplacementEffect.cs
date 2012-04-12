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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// An effect that renders a displacement effect.
	/// </summary>
	public class Gorgon2DDisplacementEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the object was disposed.
		private GorgonRenderTarget _displacementTarget = null;		// Displacement buffer target.
		private Size _targetSize = Size.Empty;						// Displacement target size.
		private GorgonConstantBuffer _displacementBuffer = null;	// Buffer used to send displacement data.
		private GorgonDataStream _displacementStream = null;		// Stream used to update the displacement buffer.
		private bool _isUpdated = true;								// Flag to indicate that the parameters have been updated.
		private float _displacementStrength = 1.0f;					// Strength of the displacement map.
		private SmoothingMode _lastSmoothMode = SmoothingMode.None;	// Last global smoothing state.
		private GorgonRenderTarget _currentTarget = null;			// Current render target.
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the displacement map render target.
		/// </summary>
		/// <param name="newSize">New size for the target.</param>
		private void UpdateDisplacementMap(Size newSize)
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
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				MultiSample = new GorgonMultisampling(1, 0)				
			});

			_targetSize = newSize;
		}

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// TRUE to continue rendering, FALSE to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			_currentTarget = Gorgon2D.Target;

			if (Gorgon2D.PixelShader.ConstantBuffers[2] != _displacementBuffer)
				Gorgon2D.PixelShader.ConstantBuffers[2] = _displacementBuffer;

			// TODO: This is not good, allow the displacement map size and format to be set manually or something.
			if ((_displacementTarget == null) || (_currentTarget.Settings.Size != _displacementTarget.Settings.Size))
			{
				UpdateDisplacementMap(_currentTarget.Settings.Size);
				_isUpdated = true;
			}

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
				if (Gorgon2D.PixelShader.Textures[1] != _displacementTarget.Texture)
					Gorgon2D.PixelShader.Textures[1] = _displacementTarget.Texture;
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
						if (_displacementTarget != null)
							_displacementTarget.Dispose();

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
