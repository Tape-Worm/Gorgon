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
// Created: Monday, April 02, 2012 2:59:16 PM
// 
#endregion

using System;
using Gorgon.Graphics;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that sharpens (and optionally embosses) an image.
	/// </summary>
	public class Gorgon2DSharpenEmbossEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed;												// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _sharpenEmbossBuffer;			        // Constant buffer for the sharpen/emboss information.
		private GorgonPixelShader _sharpenShader;							// Pixel shader used to sharpen an image.
		private GorgonPixelShader _embossShader;							// Pixel shader used to emboss an image.
		private float _amount = 1.0f;										// Amount to sharpen/emboss
		private bool _isUpdated = true;										// Flag to indicate that the parameters were updated.
		private Vector2 _size = Vector2.Zero;								// Area to emboss.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use embossing instead of sharpening.
		/// </summary>
		public bool UseEmbossing
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the amount to sharpen/emboss.
		/// </summary>
		public float Amount
		{
			get
			{
				return _amount;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_amount == value)
				{
					return;
				}

				_amount = value;
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the area (width/height) that will receive the sharpen/emboss.
		/// </summary>
		/// <remarks>Ideally this should be set to the size of the object being rendered, otherwise shimmering artifacts may occour.</remarks>
		public Vector2 Area
		{
			get
			{
				return _size;
			}
			set
			{
				if (_size == value)
				{
					return;
				}

				_size = value;
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
            _sharpenShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.SharpenEmboss.PS", "GorgonPixelShaderSharpen", "#GorgonInclude \"Gorgon2DShaders\"");
            _embossShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.SharpenEmboss.PS", "GorgonPixelShaderEmboss", "#GorgonInclude \"Gorgon2DShaders\"");

            _sharpenEmbossBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DSharpenEmbossEffect Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = 16
                                                                });
        }

	    /// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
			if (_isUpdated)
			{
				var settings = new Vector3(1.0f / Area.X, 1.0f / Area.Y, _amount);

				_sharpenEmbossBuffer.Update(ref settings);
				_isUpdated = false;
			}

            RememberConstantBuffer(ShaderType.Pixel, 1);

			Gorgon2D.PixelShader.ConstantBuffers[1] = _sharpenEmbossBuffer;
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
		/// Function called before a pass is rendered.
		/// </summary>
		/// <param name="pass">Pass to render.</param>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to stop.
		/// </returns>
		protected override bool OnBeforePassRender(GorgonEffectPass pass)
		{
			pass.PixelShader = UseEmbossing ? _embossShader : _sharpenShader;
			return base.OnBeforePassRender(pass);
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
					if (_embossShader != null)
					{
						_embossShader.Dispose();
					}

					if (_sharpenShader != null)
					{
						_sharpenShader.Dispose();
					}

					if (_sharpenEmbossBuffer != null)
					{
						_sharpenEmbossBuffer.Dispose();
					}
				}

				_embossShader = null;
				_sharpenShader = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DWaveEffect" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this effect.</param>
        /// <param name="name">The name of the effect.</param>
		internal Gorgon2DSharpenEmbossEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
			
		}
		#endregion
	}
}
