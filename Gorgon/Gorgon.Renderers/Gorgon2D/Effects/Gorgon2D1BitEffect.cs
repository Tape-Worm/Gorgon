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
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders an image as if it were 1 bit image.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This effect renders a 1-bit color image by using a <see cref="Threshold"/> to determine which bit is on, and which is off.  If a color value falls within the <see cref="Threshold"/>, then a bit is 
	/// set as on, otherwise it will be set as off.
	/// </para>
	/// </remarks>
	public class Gorgon2D1BitEffect
		: Gorgon2DEffect, IGorgon2DTextureDrawEffect
	{
		#region Value Types.
		/// <summary>
		/// Settings for the effect shader.
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 32)]
		private struct Settings
		{
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
	    // Constant buffer for the 1 bit information.
		private GorgonConstantBufferView _1BitBuffer;						
	    // Settings for the effect.
		private Settings _settings;											
        // Flag to indicate that the parameters were updated.
		private bool _isUpdated = true;
        // The shader used to render the image.
	    private Gorgon2DShader<GorgonPixelShader> _shader;
        // The batch state to render.
	    private Gorgon2DBatchState _batchState;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use an average of the texel colors or to use a grayscale calculation.
		/// </summary>
		public bool UseAverage
		{
			get => _settings.UseAverage;
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
			get => _settings.UseAlpha;
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
			get => _settings.Invert;
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
			get => _settings.WhiteRange;
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
		#endregion

		#region Methods.
	    /// <summary>
	    /// Function called to initialize the effect.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// Applications must implement this method to ensure that any required resources are created, and configured for the effect.
	    /// </para>
	    /// </remarks>
	    protected override void OnInitialize()
	    {
	        _1BitBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _settings, "Gorgon2D1BitEffect Constant Buffer");

	        _shader = PixelShaderBuilder.ConstantBuffer(_1BitBuffer, 1)
	                                    .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShader1Bit"))
	                                    .Build();

	        _batchState = BatchStateBuilder.PixelShader(_shader)
	                                       .Build();
	    }

	    /// <summary>
	    /// Function called prior to rendering.
	    /// </summary>
	    /// <returns><b>true</b> if rendering should continue, or <b>false</b> if not.</returns>
	    /// <remarks>
	    /// <para>
	    /// Applications can use this to set up common states and other configuration settings prior to executing the render passes.
	    /// </para>
	    /// </remarks>
	    protected override bool OnBeforeRender()
		{
		    if (!_isUpdated)
		    {
		        return true;
		    }

		    _1BitBuffer.Buffer.SetData(ref _settings);
		    _isUpdated = false;

		    return true;
		}

	    /// <summary>
	    /// Function called to build a new (or return an existing) 2D batch state.
	    /// </summary>
	    /// <param name="passIndex">The index of the current rendering pass.</param>
	    /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
	    /// <returns>The 2D batch state.</returns>
	    protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
	    {
	        if (statesChanged)
	        {
	            _batchState = BatchStateBuilder.Build();
	        }

	        return _batchState;
	    }
        
	    /// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
	    {
	        if (!disposing)
	        {
	            return;
	        }

	        GorgonConstantBufferView buffer = Interlocked.Exchange(ref _1BitBuffer, null);
	        Gorgon2DShader<GorgonPixelShader> shader = Interlocked.Exchange(ref _shader, null);
	        
            buffer?.Dispose();
            shader?.Dispose();
		}

	    /// <summary>
	    /// Function to render the effect.
	    /// </summary>
	    /// <param name="texture">The texture containing the image to burn or dodge.</param>
	    /// <param name="region">[Optional] The region to draw the texture info.</param>
	    /// <param name="textureCoordinates">[Optional] The texture coordinates, in texels, to use when drawing the texture.</param>
	    /// <param name="samplerStateOverride">[Optional] An override for the current texture sampler.</param>
	    /// <param name="blendStateOverride">[Optional] The blend state to use when rendering.</param>
	    /// <param name="camera">[Optional] The camera used to render the image.</param>
	    /// <remarks>
	    /// <para>
	    /// Renders the specified <paramref name="texture"/> using 1 bit color.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="region"/> parameter is omitted, then the texture will be rendered to the full size of the current render target.  If it is provided, then texture will be rendered to the
	    /// location specified, and with the width and height specified.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="textureCoordinates"/> parameter is omitted, then the full size of the texture is rendered.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="samplerStateOverride"/> parameter is omitted, then the <see cref="GorgonSamplerState.Default"/> is used.  When provided, this will alter how the pixel shader samples our
	    /// texture in slot 0.
	    /// </para>
	    /// <para>
	    /// If the <paramref name="blendStateOverride"/>, parameter is omitted, then the <see cref="GorgonBlendState.Default"/> is used. 
	    /// </para>
	    /// <para>
	    /// The <paramref name="camera"/> parameter is used to render the texture using a different view, and optionally, a different coordinate set.  
	    /// </para>
	    /// <para>
	    /// <note type="important">
	    /// <para>
	    /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
	    /// </para>
	    /// </note>
	    /// </para>
	    /// </remarks>
	    public void RenderEffect(GorgonTexture2DView texture,
	                             DX.RectangleF? region = null,
	                             DX.RectangleF? textureCoordinates = null,
	                             GorgonSamplerState samplerStateOverride = null,
	                             GorgonBlendState blendStateOverride = null,
	                             Gorgon2DCamera camera = null)
	    {
	        RenderTexture(texture, region, textureCoordinates, samplerStateOverride, blendStateOverride, camera: camera);
	    }
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2D1BitEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw the effect.</param>
        public Gorgon2D1BitEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_1BIT, Resources.GOR2D_EFFECT_1BIT_DESC, 1)
		{
		    _settings = new Settings(new GorgonRangeF(0.5f, 1.0f), false, false, true);
		    Macros.Add(new GorgonShaderMacro("GRAYSCALE_EFFECT"));
		    Macros.Add(new GorgonShaderMacro("ONEBIT_EFFECT"));
		}
		#endregion
	}
}
