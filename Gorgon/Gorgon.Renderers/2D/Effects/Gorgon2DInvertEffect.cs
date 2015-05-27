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
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders an inverted image.
	/// </summary>
	public class Gorgon2DInvertEffect
		: Gorgon2DEffect
	{
		#region Variables.
		private bool _disposed;										// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _invertBuffer;		            // Buffer for the inversion effect.
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
				if (_invertAlpha == value)
				{
					return;
				}
				
				_invertAlpha = value;
				_invertBuffer.Update(ref _invertAlpha);
			}
		}

		/// <summary>
		/// Property to set or return the function used to render the scene when inverting.
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

            Passes[0].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.Invert.PS", "GorgonPixelShaderInvert", "#GorgonInclude \"Gorgon2DShaders\"");

            _invertBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DInvertEffect Constant Buffer",
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
            RememberConstantBuffer(ShaderType.Pixel, 1);
			Gorgon2D.PixelShader.ConstantBuffers[1] = _invertBuffer;

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
					if (_invertBuffer != null)
					{
						_invertBuffer.Dispose();
					}

					if (Passes[0].PixelShader != null)
					{
						Passes[0].PixelShader.Dispose();
					}
				}

			    _invertBuffer = null;
				Passes[0].PixelShader = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DInvertEffect" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that owns this effect.</param>
        /// <param name="name">The name of the effect.</param>
		internal Gorgon2DInvertEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}
