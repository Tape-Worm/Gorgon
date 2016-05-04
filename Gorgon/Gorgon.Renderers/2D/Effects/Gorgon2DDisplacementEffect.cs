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
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Renderers.Properties;
using SlimMath;

namespace Gorgon.Renderers
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
		private GorgonTexture2D _backgroundTarget;								// Background target.
		private BufferFormat _targetFormat = BufferFormat.Unknown;				// Format for the displacement target.
		private GorgonConstantBuffer _displacementBuffer;				        // Buffer used to send displacement data.
		private bool _isUpdated = true;											// Flag to indicate that the parameters have been updated.
		private float _displacementStrength = 1.0f;								// Strength of the displacement map.
		private GorgonSprite _displacementSprite;						        // Sprite used to draw the displacement map.
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
				
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_displacementStrength == value)
				{
					return;
				}

				_displacementStrength = value;
				_isUpdated = true;
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
					UpdateDisplacementMap(value);
				}
			}
		}

		/// <summary>
		/// Property to set or return the method used to render the objects used to displace the background graphics.
		/// </summary>
		/// <remarks>Use this method to render items to displace the texture specified by <see cref="BackgroundImage"/>.</remarks>
		public Action<GorgonEffectPass> RenderDisplacement
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
		/// Property to set or return the render target used as the background for the displacement.
		/// </summary>
		public GorgonTexture2D BackgroundImage
		{
			get
			{
				return _backgroundTarget;
			}
			set
			{
				if (_backgroundTarget == value)
				{
					return;
				}

				_backgroundTarget = value;

				if (_backgroundTarget != null)
				{
					UpdateDisplacementMap(_targetFormat);
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the displacement map render target.
		/// </summary>
		/// <param name="format">Format to use for the displacement target.</param>
		private void UpdateDisplacementMap(BufferFormat format)
		{
			if (_displacementTarget != null) 
			{
				_displacementTarget.Dispose();
			}

			_displacementTarget = null;

			if (_backgroundTarget == null)
			{
				return;
			}

			_displacementTarget = Graphics.Output.CreateRenderTarget("Effect.Displacement.RT", new GorgonRenderTarget2DSettings
			{
				Width = _backgroundTarget.Settings.Width,
				Height = _backgroundTarget.Settings.Height,
				DepthStencilFormat = BufferFormat.Unknown,
				Format = format == BufferFormat.Unknown ? _backgroundTarget.Settings.Format : format,
				Multisampling = GorgonMultiSampleInfo.NoMultiSampling
			});

			_displacementSprite.Texture = _backgroundTarget;
			_displacementSprite.Size = new Size(_backgroundTarget.Settings.Width, _backgroundTarget.Settings.Height);

			_targetFormat = format;
			_isUpdated = true;
		}

        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// </remarks>
	    protected override void OnInitialize()
	    {
            base.OnInitialize();

            Passes[1].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.DisplacementDecoder.PS", "GorgonPixelShaderDisplacementDecoder", "#GorgonInclude \"Gorgon2DShaders\"");

            _displacementBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DDisplacementEffect Constant Buffer",
                                                                                                    new GorgonConstantBufferSettings
                                                                                                    {
                                                                                                        SizeInBytes = 16
                                                                                                    });
            _displacementSprite = Gorgon2D.Renderables.CreateSprite("Gorgon2DDisplacementEffect Sprite", new GorgonSpriteSettings
            {
                Size = new Vector2(1)
            });
            _displacementSprite.BlendingMode = BlendingMode.None;
            _displacementSprite.SmoothingMode = SmoothingMode.Smooth;

            // Set the drawing for rendering the displacement map.
            Passes[1].RenderAction = pass => _displacementSprite.Draw();
        }

	    /// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public void FreeResources()
	    {
	        _backgroundTarget = null;
	        
            if (_displacementTarget == null)
	        {
	            return;
	        }

	        _displacementTarget.Dispose();
	        _displacementTarget = null;
	    }

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			if ((_displacementTarget == null) || (_backgroundTarget == null))
			{
				UpdateDisplacementMap(_targetFormat);
			}

#if DEBUG
			if ((_displacementTarget == null) || (_backgroundTarget == null))
			{
				throw new GorgonException(GorgonResult.CannotWrite, Resources.GOR2D_EFFECT_NO_DISPLACEMENT_TARGET);
			}
#endif
            RememberConstantBuffer(ShaderType.Pixel, 1);
            RememberShaderResource(ShaderType.Pixel, 1);

			Gorgon2D.PixelShader.ConstantBuffers[1] = _displacementBuffer;

			if (!_isUpdated)
			{
				return base.OnBeforeRender();
			}

			var settings = new Vector4(1.0f / _backgroundTarget.Settings.Width, 1.0f / _backgroundTarget.Settings.Height, _displacementStrength, 0);

			_displacementBuffer.Update(ref settings);
			_isUpdated = false;

			return base.OnBeforeRender();
		}

        /// <summary>
        /// Function called after rendering ends.
        /// </summary>
	    protected override void OnAfterRender()
	    {
            RestoreConstantBuffer(ShaderType.Pixel, 1);
            RestoreShaderResource(ShaderType.Pixel, 1);
	        base.OnAfterRender();
	    }

	    /// <summary>
		/// Function called before a pass is rendered.
		/// </summary>
		/// <param name="pass">Pass to render.</param>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to stop.
		/// </returns>
		protected override bool OnBeforePassRender(GorgonEffectPass pass)
		{
			if ((_displacementTarget == null) || (_backgroundTarget == null))
			{
				return false;
			}

			if (pass.PassIndex == 0)
			{
			    base.OnBeforePassRender(pass);

				Gorgon2D.PixelShader.Current = null;
				Gorgon2D.VertexShader.Current = null;
				Gorgon2D.PixelShader.Resources[1] = null;

				_displacementTarget.Clear(GorgonColor.Transparent);
				Gorgon2D.Target = _displacementTarget;
				return true;
			}
			
			Gorgon2D.PixelShader.Current = pass.PixelShader;
			Gorgon2D.VertexShader.Current = pass.VertexShader;
			Gorgon2D.PixelShader.Resources[1] = _displacementTarget;

			return true;
		}

		/// <summary>
		/// Function called after a pass has been rendered.
		/// </summary>
		/// <param name="pass">Pass that was rendered.</param>
		protected override void OnAfterPassRender(GorgonEffectPass pass)
		{
			if (pass.PassIndex == 0)
			{
				Gorgon2D.Target = CurrentTarget;
				return;
			}

			base.OnAfterPassRender(pass);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
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

				    if (_displacementBuffer != null)
				    {
				        _displacementBuffer.Dispose();
				    }
        
					FreeResources();
				}

			    _displacementBuffer = null;
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
		/// <param name="graphics">The graphics interface that owns this effect.</param>
		/// <param name="name">Name of the effect.</param>
		internal Gorgon2DDisplacementEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 2)
		{
		}
		#endregion
	}
}
