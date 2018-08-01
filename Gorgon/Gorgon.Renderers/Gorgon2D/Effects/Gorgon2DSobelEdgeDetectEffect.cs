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

using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

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
		    // Texel size and threshold.
			private readonly DX.Vector4 _texelThreshold;			

			/// <summary>
			/// Line color.
			/// </summary>
			public readonly GorgonColor LineColor;

			/// <summary>
			/// Property to return the size of a texel.
			/// </summary>
			public DX.Vector2 TexelSize => (DX.Vector2)_texelThreshold;

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
			public Settings(GorgonColor linecolor, DX.Vector2 texelSize, float threshold)
			{
				LineColor = linecolor;
				_texelThreshold = new DX.Vector4(texelSize, threshold, 0);
			}
		}
		#endregion

		#region Variables.
	    // Buffer for the sobel edge detection.
		private GorgonConstantBufferView _sobelBuffer;
        // The pixel shader for the effect.
	    private Gorgon2DShader<GorgonPixelShader> _shader;
        // The batch state used for the effect.
	    private Gorgon2DBatchState _batchState;
	    // Settings for the effect.
		private Settings _settings;								
	    // Flag to indicate that the parameters have been updated.
		private bool _isUpdated = true;
        // The thickness of the lines.
	    private float _lineThickness = 1.0f;
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the relative thickness of the line.
        /// </summary>
	    public float LineThickness
	    {
	        get => _lineThickness;
	        set
	        {
	            if (_lineThickness == value)
	            {
	                return;
	            }

	            _settings = new Settings(_settings.LineColor,
	                                     new DX.Vector2(_settings.TexelSize.X * value, _settings.TexelSize.Y * value),
	                                     _settings.Threshold);
	            _lineThickness = value;
	            _isUpdated = true;
	        }
	    }

		/// <summary>
		/// Property to set or return the threshold value for the edges.
		/// </summary>
		public float EdgeThreshold
		{
			get => _settings.Threshold;
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
		/// Property to set or return the color for the edges.
		/// </summary>
		public GorgonColor LineColor
		{
			get => _settings.LineColor;
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
	        _sobelBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _settings, "Gogron 2D Sobel Edge Detect Filter Effect Constant Buffer");

	        _shader = PixelShaderBuilder
	                  .ConstantBuffer(_sobelBuffer, 1)
	                  .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderSobelEdge"))
	                  .Build();


	        _batchState = BatchStateBuilder
	                      .PixelShader(_shader)
	                      .Build();
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
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override void OnBeforeRender()
		{
		    if (!_isUpdated)
		    {
		        return;
		    }


		    _sobelBuffer.Buffer.SetData(ref _settings);
			_isUpdated = false;
		}

	    /// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
	    {
	        GorgonConstantBufferView buffer = Interlocked.Exchange(ref _sobelBuffer, null);
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
            texture.ValidateObject(nameof(texture));
            DX.Vector2 texelSize = new DX.Vector2((1.0f / texture.Width) * _lineThickness, (1.0f / texture.Height) * _lineThickness);

	        if (texelSize != _settings.TexelSize)
	        {
	            _settings = new Settings(LineColor, texelSize, EdgeThreshold);
	            _isUpdated = true;
	        }

	        RenderTexture(texture, region, textureCoordinates, samplerStateOverride, blendStateOverride, camera: camera);
	    }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DSobelEdgeDetectEffect"/> class.
		/// </summary>
		/// <param name="renderer">The renderer used to render this effect.</param>
		public Gorgon2DSobelEdgeDetectEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_SHARPEMBOSS, Resources.GOR2D_EFFECT_SHARPEMBOSS_DESC, 1)
		{
		    _settings = new Settings(Color.Black, DX.Vector2.Zero, 0.75f);
            Macros.Add(new GorgonShaderMacro("SOBEL_EDGE_EFFECT"));
		}
		#endregion
	}
}
