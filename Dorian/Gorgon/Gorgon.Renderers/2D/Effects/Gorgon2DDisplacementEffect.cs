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
using System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers.Properties;
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
		private GorgonRenderTarget2D _displacementTarget;						// Displacement buffer target.
		// TODO: I don't like this.  The developer should be able to assign their own target.  Fix tomorrow or sometime this week (08202013).
		private GorgonRenderTarget2D _backgroundTarget;							// Background target.
		private Size _targetSize = new Size(512, 512);							// Displacement target size.
		private BufferFormat _targetFormat = BufferFormat.R16G16B16A16_Float;	// Format for the displacement target.
		private readonly GorgonConstantBuffer _displacementBuffer;				// Buffer used to send displacement data.
		private bool _isUpdated = true;											// Flag to indicate that the parameters have been updated.
		private float _displacementStrength = 1.0f;								// Strength of the displacement map.
		private readonly GorgonSprite _displacementSprite;						// Sprite used to draw the displacement map.
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
				{
					value = 0.0f;
				}

				if (_displacementStrength.EqualsEpsilon(value))
				{
					return;
				}

				_displacementStrength = value;
				_isUpdated = true;
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
				{
					UpdateDisplacementMap(value, _targetFormat);
				}
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
				{
					UpdateDisplacementMap(_targetSize, value);
				}
			}
		}

		/// <summary>
		/// Property to set or return the method used to render the objects for the background.
		/// </summary>
		/// <remarks>Use this method to render items to be displaced by the displacement map.  The graphics rendered here will be displaced by the graphics rendered by <see cref="DisplacementRender"/>.</remarks>
		public Action<GorgonEffectPass> BackgroundRender
		{
			get
			{
				return Passes[1].RenderAction;
			}
			set
			{
				Passes[1].RenderAction = value;
			}
		}

		/// <summary>
		/// Property to set or return the method used to render the objects used to displace the background graphics.
		/// </summary>
		/// <remarks>Use this method to render items to displace the items rendered by <see cref="BackgroundRender"/>.</remarks>
		public Action<GorgonEffectPass> DisplacementRender
		{
			get
			{
				return Passes[0].RenderAction;
			}
			set
			{
				Passes[0].RenderAction = value;
			}
		}

		/// <summary>
		/// Property to return the output from the effect.
		/// </summary>
		public GorgonTexture Output
		{
			get
			{
				return _backgroundTarget;
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
			{
				_displacementTarget.Dispose();
			}

			if (_backgroundTarget != null)
			{
				_backgroundTarget.Dispose();
			}

			_displacementTarget = null;
			_backgroundTarget = null;

			if ((newSize.Width <= 0) || (newSize.Height <= 0))
			{
				return;
			}

			_displacementTarget = Graphics.Output.CreateRenderTarget("Effect.Displacement.RT", new GorgonRenderTarget2DSettings
			{
				Width = newSize.Width,
				Height = newSize.Height,
				DepthStencilFormat = BufferFormat.Unknown,
				Format = format,
				Multisampling = GorgonMultisampling.NoMultiSampling
			});

			_backgroundTarget = Graphics.Output.CreateRenderTarget("Effect.Background.RT", new GorgonRenderTarget2DSettings
			{
				Width = newSize.Width,
				Height = newSize.Height,
				DepthStencilFormat = BufferFormat.Unknown,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				Multisampling = GorgonMultisampling.NoMultiSampling
			});

			_displacementSprite.Texture = _backgroundTarget;
			_displacementSprite.Size = newSize;

			_targetFormat = format;
			_targetSize = newSize;
			_isUpdated = true;
		}

		/// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public void FreeResources()
		{
			if (_backgroundTarget != null)
			{
				_backgroundTarget.Dispose();
			}
			if (_displacementTarget != null)
			{
				_displacementTarget.Dispose();
			}
			_backgroundTarget = null;
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
			if ((_displacementTarget == null) || (_backgroundTarget == null))
			{
				UpdateDisplacementMap(_targetSize, _targetFormat);
			}

#if DEBUG
			if ((_displacementTarget == null) || (_backgroundTarget == null))
			{
				throw new GorgonException(GorgonResult.CannotWrite, Resources.GOR2D_EFFECT_NO_DISPLACEMENT_TARGET);
			}
#endif

			Gorgon2D.PixelShader.ConstantBuffers[1] = _displacementBuffer;

			if (!_isUpdated)
			{
				return base.OnBeforeRender();
			}

			var settings = new Vector4(1.0f / _targetSize.Width, 1.0f / _targetSize.Height, _displacementStrength, 0);

			_displacementBuffer.Update(ref settings);
			_isUpdated = false;

			return base.OnBeforeRender();
		}

		/// <summary>
		/// Function called before a pass is rendered.
		/// </summary>
		/// <param name="pass">Pass to render.</param>
		/// <returns>
		/// TRUE to continue rendering, FALSE to stop.
		/// </returns>
		protected override bool OnBeforePassRender(GorgonEffectPass pass)
		{
			if ((_displacementTarget == null) || (_backgroundTarget == null))
			{
				return false;
			}

			switch (pass.PassIndex)
			{
				case 0:
					StoredShaders.PixelShader = Gorgon2D.PixelShader.Current;
					StoredShaders.VertexShader = Gorgon2D.VertexShader.Current;

					Gorgon2D.PixelShader.Current = pass.PixelShader;
					Gorgon2D.VertexShader.Current = pass.VertexShader;
					Gorgon2D.PixelShader.Resources[1] = null;

					_displacementTarget.Clear(GorgonColor.Transparent);
					Gorgon2D.Target = _displacementTarget;
					break;
				case 2:
					Gorgon2D.PixelShader.Current = pass.PixelShader;
					Gorgon2D.VertexShader.Current = pass.VertexShader;
					Gorgon2D.PixelShader.Resources[1] = _displacementTarget;
					break;
			}

			return true;
		}

		/// <summary>
		/// Function called after a pass has been rendered.
		/// </summary>
		/// <param name="pass">Pass that was rendered.</param>
		protected override void OnAfterPassRender(GorgonEffectPass pass)
		{
			switch (pass.PassIndex)
			{
				case 0:
					Gorgon2D.Target = _backgroundTarget;
					break;
				case 1:
					Gorgon2D.Target = CurrentTarget;
					break;
				case 2:
					Gorgon2D.PixelShader.Current = StoredShaders.PixelShader;
					Gorgon2D.VertexShader.Current = StoredShaders.VertexShader;
					break;
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
					if (Passes[1].PixelShader != null)
					{
						Passes[1].PixelShader.Dispose();
					}

					FreeResources();
				}

				Passes[1].PixelShader = null;
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
			: base(gorgon2D, "Effect.2D.Displacement", 3)
		{
			
			Passes[2].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.DisplacementDecoder.PS", "GorgonPixelShaderDisplacementDecoder", "#GorgonInclude \"Gorgon2DShaders\"");

			_displacementBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DDisplacementEffect Constant Buffer",
																									new GorgonConstantBufferSettings
																									{
																										SizeInBytes = 16
																									});
			_displacementSprite = gorgon2D.Renderables.CreateSprite("Gorgon2DDisplacementEffect Sprite", new GorgonSpriteSettings
			{
				Size = DisplacementRenderTargetSize
			});
			_displacementSprite.BlendingMode = BlendingMode.None;
			_displacementSprite.SmoothingMode = SmoothingMode.Smooth;

			// Set the drawing for rendering the displacement map.
			Passes[2].RenderAction = pass => _displacementSprite.Draw();
		}
		#endregion
	}
}
