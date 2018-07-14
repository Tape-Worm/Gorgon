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

using System.Threading;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders an inverted image.
	/// </summary>
	public class Gorgon2DInvertEffect
		: Gorgon2DEffect, IGorgon2DTextureDrawEffect
	{
		#region Variables.
	    // Buffer for the inversion effect.
		private GorgonConstantBufferView _invertBuffer;
	    // Flag to invert the alpha channel.
        private bool _invertAlpha;
        // The pixel shader for the effect.
	    private Gorgon2DShader<GorgonPixelShader> _shader;
        // The batch render state.
	    private Gorgon2DBatchState _batchState;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to invert the alpha channel.
		/// </summary>
		public bool InvertAlpha
		{
			get => _invertAlpha;
		    set
			{
				if (_invertAlpha == value)
				{
					return;
				}
				
				_invertAlpha = value;
				_invertBuffer.Buffer.SetData(ref _invertAlpha);
			}
		}
		#endregion

		#region Methods.
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
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// <para>When creating a custom effect, use this method to initialize the effect.  Do not put initialization code in the effect constructor.</para>
        /// </remarks>
	    protected override void OnInitialize()
        {
            _invertBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _invertAlpha, "Gorgon2DInvertEffect Constant Buffer");

            _shader = PixelShaderBuilder
                      .ConstantBuffer(_invertBuffer, 1)
                      .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderInvert"))
                      .Build();

            _batchState = BatchStateBuilder
                          .PixelShader(_shader)
                          .Build();
        }

        /// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
	    {
	        GorgonConstantBufferView buffer = Interlocked.Exchange(ref _invertBuffer, null);
	        Gorgon2DShader<GorgonPixelShader> pixelShader = Interlocked.Exchange(ref _shader, null);

            buffer?.Dispose();
            pixelShader?.Dispose();
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
        /// <remarks><para>
        /// Renders the specified <paramref name="texture" /> using 1 bit color.
        /// </para>
        /// <para>
        /// If the <paramref name="region" /> parameter is omitted, then the texture will be rendered to the full size of the current render target.  If it is provided, then texture will be rendered to the
        /// location specified, and with the width and height specified.
        /// </para>
        /// <para>
        /// If the <paramref name="textureCoordinates" /> parameter is omitted, then the full size of the texture is rendered.
        /// </para>
        /// <para>
        /// If the <paramref name="samplerStateOverride" /> parameter is omitted, then the <see cref="GorgonSamplerState.Default" /> is used.  When provided, this will alter how the pixel shader samples our
        /// texture in slot 0.
        /// </para>
        /// <para>
        /// If the <paramref name="blendStateOverride" />, parameter is omitted, then the <see cref="GorgonBlendState.Default" /> is used.
        /// </para>
        /// <para>
        /// The <paramref name="camera" /> parameter is used to render the texture using a different view, and optionally, a different coordinate set.
        /// </para>
        /// <para>
        ///   <note type="important">
        ///     <para>
        /// For performance reasons, any exceptions thrown by this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        ///   </note>
        /// </para></remarks>
        public void RenderEffect(GorgonTexture2DView texture, DX.RectangleF? region = null, DX.RectangleF? textureCoordinates = null, GorgonSamplerState samplerStateOverride = null, GorgonBlendState blendStateOverride = null, Gorgon2DCamera camera = null)
        {
            RenderTexture(texture, region, textureCoordinates, samplerStateOverride, camera: camera);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DInvertEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw with this effect.</param>
        public Gorgon2DInvertEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_INVERT, Resources.GOR2D_EFFECT_INVERT_DESC, 1)
		{
            Macros.Add(new GorgonShaderMacro("INVERSE_EFFECT"));
		}
		#endregion
	}
}
