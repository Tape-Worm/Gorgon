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
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the size of the displacement map target.
		/// </summary>
		public Size DisplacementTargetSize
		{
			get
			{
				return _displacementTarget.Settings.Size;
			}
			set
			{
				if (_targetSize != value)
				{
					_targetSize = value;
					UpdateDisplacementMap();
				}
			}
		}

		/// <summary>
		/// Property to set or return the background image to displace.
		/// </summary>
		public GorgonTexture2D Background
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the displacement map render target.
		/// </summary>
		private void UpdateDisplacementMap()
		{
			if (_displacementTarget != null)
				_displacementTarget.Dispose();

			_displacementTarget = null;

			if ((_targetSize.Width <= 0) || (_targetSize.Height <= 0))
				return;

			_displacementTarget = Graphics.Output.CreateRenderTarget("Effect.Displacement.RT", new GorgonRenderTargetSettings()
			{
				Width = _targetSize.Width,
				Height = _targetSize.Height,
				DepthStencilFormat = BufferFormat.Unknown,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				MultiSample = new GorgonMultisampling(1, 0)				
			});
		}

		/// <summary>
		/// Function called when a pass is about to start rendering.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnBeforeRenderPass(int passIndex)
		{
			base.OnBeforeRenderPass(passIndex);

			if (passIndex == 0)
			{
				_displacementTarget.Clear(new GorgonColor(0, 0, 0, 0));
				Gorgon2D.PixelShader.Current = null;
				Gorgon2D.Target = _displacementTarget;
			}
			else
			{
				Gorgon2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
				Gorgon2D.Target = null;
				Gorgon2D.PixelShader.Current = null;
				Gorgon2D.Drawing.Blit(_displacementTarget, new Vector2(0, 400));
				Gorgon2D.PixelShader.Current = PixelShader;
				if (Gorgon2D.PixelShader.Textures[1] != _displacementTarget.Texture)
					Gorgon2D.PixelShader.Textures[1] = _displacementTarget.Texture;
			}				
		}

		/// <summary>
		/// Function to render a specific pass while using this effect.
		/// </summary>
		/// <param name="renderMethod">Method to use to render the data.</param>
		/// <param name="passIndex">Index of the pass to render.</param>
		/// <remarks>The <paramref name="renderMethod"/> is an action delegate that must be defined with an integer value.  The parameter indicates which pass the rendering is currently on.</remarks>
		protected override void RenderImpl(Action<int> renderMethod, int passIndex)
		{
			if (passIndex == 0)
				base.RenderImpl(renderMethod, passIndex);
			else
			{
				Gorgon2D.Drawing.Blit(Background, Vector2.Zero);
			}
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
		}
		#endregion
	}
}
