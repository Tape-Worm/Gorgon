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
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Native;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders an image as if it were 1 bit image.
	/// </summary>
	public class Gorgon2D1BitEffect
		: Gorgon2DEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for the effect shader.
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 32)]
		private struct Settings
		{
			/// <summary>
			/// The size of the structure, in bytes.
			/// </summary>
			public static readonly int SizeInBytes = DirectAccess.SizeOf<Settings>();

			[FieldOffset(0)]
			private readonly int _useAverage;			// Flag to indicate that the average of the texel colors should be used.
			[FieldOffset(4)]
			private readonly int _invert;				// Flag to invert the texel colors.
			[FieldOffset(8)]
			private readonly int _useAlpha;				// Flag to indicate that the alpha channel should be included.

			/// <summary>
			/// Range of values that are considered "on".
			/// </summary>
			[FieldOffset(16)]
			public readonly GorgonRangeF WhiteRange;
			
			/// <summary>
			/// Flag to indicate that the average of the texel colors should be used.
			/// </summary>
			public bool UseAverage => _useAverage != 0;

			/// <summary>
			/// Flag to invert the texel colors.
			/// </summary>
			public bool Invert => _invert != 0;

			/// <summary>
			/// Flag to indicate that the alpha channel should be included.
			/// </summary>
			public bool UseAlpha => _useAlpha != 0;

			/// <summary>
			/// Initializes a new instance of the <see cref="Settings"/> struct.
			/// </summary>
			/// <param name="range">The range.</param>
			/// <param name="average">if set to <b>true</b> [average].</param>
			/// <param name="invert">if set to <b>true</b> [invert].</param>
			/// <param name="useAlpha">if set to <b>true</b> [use alpha].</param>
			public Settings(GorgonRangeF range, bool average, bool invert, bool useAlpha)
			{
				WhiteRange = range;
				_useAverage = Convert.ToInt32(average);
				_invert = Convert.ToInt32(invert);
				_useAlpha = Convert.ToInt32(useAlpha);
			}
		}
		#endregion

		#region Variables.
		private bool _disposed;												// Flag to indicate that the object was disposed.
		private GorgonConstantBuffer _1BitBuffer;							// Constant buffer for the 1 bit information.
		private Settings _settings;											// Settings for the effect.
		private bool _isUpdated = true;										// Flag to indicate that the parameters were updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use an average of the texel colors or to use a grayscale calculation.
		/// </summary>
		public bool UseAverage
		{
			get
			{
				return _settings.UseAverage;
			}
			set
			{
				if (_settings.UseAverage == value)
				{
					return;
				}

				_settings = new Settings(_settings.WhiteRange, value, _settings.Invert, _settings.UseAlpha);
				_isUpdated = true;
			}
		}


		/// <summary>
		/// Property to set or return whether the alpha channel should be included in the conversion.
		/// </summary>
		public bool ConvertAlphaChannel
		{
			get
			{
				return _settings.UseAlpha;
			}
			set
			{
				if (_settings.UseAlpha == value)
				{
					return;
				}

				_settings = new Settings(_settings.WhiteRange, _settings.UseAverage, _settings.Invert, value);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return whether to invert the texel colors.
		/// </summary>
		public bool Invert
		{
			get
			{
				return _settings.Invert;
			}
			set
			{
				if (_settings.Invert == value)
				{
					return;
				}

				_settings = new Settings(_settings.WhiteRange, _settings.UseAverage, value, _settings.UseAlpha);
				_isUpdated = true;
			}
		}

		/// <summary>
		/// Property to set or return the range of values that are considered to be "on".
		/// </summary>
		public GorgonRangeF Threshold
		{
			get
			{
				return _settings.WhiteRange;
			}
			set
			{
				if (_settings.WhiteRange.Equals(value))
				{
					return;
				}

				_settings = new Settings(value, _settings.UseAverage, _settings.Invert, _settings.UseAlpha);
				_isUpdated = true;
			}
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

            Passes[0].PixelShader = Graphics.ImmediateContext.Shaders.CreateShader<GorgonPixelShader>("Effect.2D.1Bit.PS", "GorgonPixelShader1Bit", "#GorgonInclude \"Gorgon2DShaders\"");

            _1BitBuffer = Graphics.ImmediateContext.Buffers.CreateConstantBuffer("Gorgon2D1BitEffect Constant Buffer",
                                                                new GorgonConstantBufferSettings
                                                                {
                                                                    SizeInBytes = Settings.SizeInBytes
                                                                });

            _settings = new Settings(new GorgonRangeF(0.5f, 1.0f), false, false, true);
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
				_1BitBuffer.Update(ref _settings);
				_isUpdated = false;
			}

            RememberConstantBuffer(ShaderType.Pixel, 1);
			Gorgon2D.PixelShader.ConstantBuffers[1] = _1BitBuffer;
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
					if (Passes[0].PixelShader != null)
					{
						Passes[0].PixelShader.Dispose();
					}

					Passes[0].PixelShader = null;

					if (_1BitBuffer != null)
					{
						_1BitBuffer.Dispose();
						_1BitBuffer = null;
					}
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DWaveEffect"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
        /// <param name="name">Name of the effect.</param>
        internal Gorgon2D1BitEffect(GorgonGraphics graphics, string name)
			: base(graphics, name, 1)
		{
		}
		#endregion
	}
}
