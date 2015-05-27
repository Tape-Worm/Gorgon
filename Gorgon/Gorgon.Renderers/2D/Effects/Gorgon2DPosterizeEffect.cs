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

using System;
using System.Runtime.InteropServices;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders a posterized image.
	/// </summary>
	public class Gorgon2DPosterizedEffect
		: Gorgon2DEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for the effect shader.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct Settings
		{
			private readonly int _posterizeAlpha;								// Flag to posterize the alpha channel.

			/// <summary>
			/// Exponent power for the posterization.
			/// </summary>
			public readonly float PosterizeExponent;
			/// <summary>
			/// Number of bits to reduce down to.
			/// </summary>
			public readonly int PosterizeBits;
			
			/// <summary>
			/// Property to return whether to posterize the alpha channel.
			/// </summary>
			public bool PosterizeAlpha
			{
				get
				{
					return _posterizeAlpha != 0;
				}
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Settings"/> struct.
			/// </summary>
			/// <param name="useAlpha">if set to <c>true</c> [use alpha].</param>
			/// <param name="power">The power.</param>
			/// <param name="bits">The bits.</param>
			public Settings(bool useAlpha, float power, int bits)
			{
				_posterizeAlpha = Convert.ToInt32(useAlpha);
				PosterizeExponent = power;
				PosterizeBits = bits;
			}
		}
		#endregion

		#region Variables.
		private bool _disposed;										// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _posterizeBuffer;		        // Buffer for the posterize effect.
		private Settings _settings;									// Settings for the effect shader.
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
				return _settings.PosterizeAlpha;
			}
			set
			{
				if (_settings.PosterizeAlpha == value)
				{
					return;
				}

				_settings = new Settings(value, _settings.PosterizeExponent, _settings.PosterizeBits);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the exponent power for the effect.
		/// </summary>
		public float Power
		{
			get
			{
				return _settings.PosterizeExponent;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_settings.PosterizeExponent == value)
				{
					return;
				}

				if (value < 1e-6f)
				{
					value = 1e-6f;
				}

				_settings = new Settings(_settings.PosterizeAlpha, value, _settings.PosterizeBits);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the number of bits to reduce down to for the effect.
		/// </summary>
		public int Bits
		{
			get
			{
				return _settings.PosterizeBits;
			}
			set
			{
				if (_settings.PosterizeBits == value)
				{
					return;
				}

				if (value < 1)
				{
					value = 1;
				}

				_settings = new Settings(_settings.PosterizeAlpha, _settings.PosterizeExponent, value);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the function used to render the scene when posterizing.
		/// </summary>
		/// <remarks>Use this to render the image to be blurred.</remarks>
		public Action<GorgonEffectPass> RenderScene
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

		#endregion

		#region Methods.
        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
        /// </remarks>
	    protected override void OnInitialize()
	    {
	        base.OnInitialize();
            Passes[0].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Posterized.PS", "GorgonPixelShaderPosterize", "#GorgonInclude \"Gorgon2DShaders\"");

            _posterizeBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DPosterizedEffect Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = 16
                                                                });

            _settings = new Settings(false, 1.0f, 8);
        }

	    /// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <c>true</c> to continue rendering, <c>false</c> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			if (_isUpdated)
			{
				_posterizeBuffer.Update(ref _settings);
				_isUpdated = false;
			}

            RememberConstantBuffer(ShaderType.Pixel , 1);
			Gorgon2D.PixelShader.ConstantBuffers[1] = _posterizeBuffer;
			return base.OnBeforeRender();
		}

        /// <summary>
        /// Function called after rendering ends.
        /// </summary>
	    protected override void OnAfterRender()
	    {
            RestoreConstantBuffer(ShaderType.Pixel, 1);
	        base.OnAfterRender();
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
					{
						_posterizeBuffer.Dispose();
					}

					if (Passes[0].PixelShader != null)
					{
						Passes[0].PixelShader.Dispose();
					}
				}

				Passes[0].PixelShader = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DPosterizedEffect" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns the effect.</param>
        /// <param name="name">The name of the effect.</param>
		internal Gorgon2DPosterizedEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}
