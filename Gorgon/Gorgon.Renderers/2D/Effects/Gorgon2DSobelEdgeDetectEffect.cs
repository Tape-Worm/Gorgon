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
using System.Drawing;
using System.Runtime.InteropServices;
using Gorgon.Graphics;
using Gorgon.Native;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders the edges of an image with Sobel edge detection.
	/// </summary>
	public class Gorgon2DSobelEdgeDetectEffect
		: Gorgon2DEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for the effect shader.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
		private struct Settings
		{
			private readonly Vector4 _texelThreshold;			// Texel size and threshold.

			/// <summary>
			/// Line color.
			/// </summary>
			public readonly GorgonColor LineColor;

			/// <summary>
			/// Property to return the size of a texel.
			/// </summary>
			public Vector2 TexelSize => (Vector2)_texelThreshold;

			/// <summary>
			/// Property to return the threshold for the effect.
			/// </summary>
			public float Threshold => _texelThreshold.Z;

			/// <summary>
			/// Initializes a new instance of the <see cref="Settings"/> struct.
			/// </summary>
			/// <param name="linecolor">The linecolor.</param>
			/// <param name="texelSize">Size of the texel.</param>
			/// <param name="threshold">The threshold.</param>
			public Settings(GorgonColor linecolor, Vector2 texelSize, float threshold)
			{
				LineColor = linecolor;
				_texelThreshold = new Vector4(texelSize, threshold, 0);
			}
		}
		#endregion

		#region Variables.
		private bool _disposed;									// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _sobelBuffer;		        // Buffer for the sobel edge detection.
		private Settings _settings;								// Settings for the effect.
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
				return _settings.Threshold;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_settings.Threshold == value)
				{
					return;
				}

				if (value < 0)
				{
					value = 0.0f;
				}
				if (value > 1.0f)
				{
					value = 1.0f;
				}

				_settings = new Settings(_settings.LineColor, _settings.TexelSize, value);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the line thickness.
		/// </summary>
		public Vector2 LineThickness
		{
			get
			{
				return _settings.TexelSize;
			}
			set
			{
				if (_settings.TexelSize.Equals(value))
				{
					return;
				}

				_settings = new Settings(_settings.LineColor, value, _settings.Threshold);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the color for the edges.
		/// </summary>
		public GorgonColor LineColor
		{
			get
			{
				return _settings.LineColor;
			}
			set
			{
				if (_settings.LineColor.Equals(value))
				{
					return;
				}

				_settings = new Settings(value, _settings.TexelSize, _settings.Threshold);
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
            Passes[0].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.SobelEdgeDetect.PS", "GorgonPixelShaderSobelEdge", "#GorgonInclude \"Gorgon2DShaders\"");

            _sobelBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2DSobelEdgeDetectEffect Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = DirectAccess.SizeOf<Settings>()
                                                                });
            _settings = new Settings(Color.Black, Vector2.Zero, 0.75f);
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
				_sobelBuffer.Update(ref _settings);
				_isUpdated = false;
			}

            RememberConstantBuffer(ShaderType.Pixel, 1);

			Gorgon2D.PixelShader.ConstantBuffers[1] = _sobelBuffer;
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
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_sobelBuffer != null)
					{
						_sobelBuffer.Dispose();
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
		/// Initializes a new instance of the <see cref="Gorgon2DSobelEdgeDetectEffect"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this effect.</param>
		/// <param name="name">The name of the effect.</param>
		internal Gorgon2DSobelEdgeDetectEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}
