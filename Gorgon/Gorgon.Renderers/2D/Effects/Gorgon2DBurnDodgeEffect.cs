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

using System;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders images burn/dodge effect.
	/// </summary>
	public class Gorgon2DBurnDodgeEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private GorgonConstantBuffer _burnDodgeBuffer;			        // Burn/dodge buffer.
		private GorgonPixelShader _dodgeBurn;							// Dodge/burn shader.
		private GorgonPixelShader _linearDodgeBurn;						// Linear dodge/burn shader.
		private bool _disposed;											// Flag to indicate that the object was disposed.
		private bool _isUpdated = true;									// Flag to indicate that the effect parameters are updated.
		private bool _useDodge;											// Flag to indicate whether to use dodging or burning.
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
				if (_useDodge == value)
				{
					return;
				}

				_useDodge = value;
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return whether to use a linear burn/dodge.
		/// </summary>
		public bool UseLinear
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the function used to render the scene when blurring.
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
        /// </remarks>
	    protected override void OnInitialize()
	    {
            base.OnInitialize();

            _linearDodgeBurn = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.BurnDodge.PS", "GorgonPixelShaderLinearBurnDodge", "#GorgonInclude \"Gorgon2DShaders\"");
            _dodgeBurn = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.BurnDodge.PS", "GorgonPixelShaderBurnDodge", "#GorgonInclude \"Gorgon2DShaders\"");

            _burnDodgeBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DBurnDodgeEffect Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = 16
                                                                });
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
				_burnDodgeBuffer.Update(ref _useDodge);
				_isUpdated = false;
			}

            RememberConstantBuffer(ShaderType.Pixel, 1);
			Gorgon2D.PixelShader.ConstantBuffers[1] = _burnDodgeBuffer;

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
		/// <c>true</c> to continue rendering, <c>false</c> to stop.
		/// </returns>
		protected override bool OnBeforePassRender(GorgonEffectPass pass)
		{
			Passes[0].PixelShader = UseLinear ? _linearDodgeBurn : _dodgeBurn;
			return base.OnBeforePassRender(pass);
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
					if (_linearDodgeBurn != null)
					{
						_linearDodgeBurn.Dispose();
					}

					if (_dodgeBurn != null)
					{
						_dodgeBurn.Dispose();
					}

				    if (_burnDodgeBuffer != null)
				    {
				        _burnDodgeBuffer.Dispose();
				    }
				}

			    _burnDodgeBuffer = null;
				_linearDodgeBurn = null;
				_dodgeBurn = null;

				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DBurnDodgeEffect"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns the effect.</param>
		/// <param name="name">The name of the effect.</param>
		internal Gorgon2DBurnDodgeEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}
