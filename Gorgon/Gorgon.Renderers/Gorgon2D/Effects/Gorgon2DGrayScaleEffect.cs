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
// Created: Monday, April 02, 2012 1:41:47 PM
// 
#endregion

using System.Threading;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders as gray scale.
	/// </summary>
	public class Gorgon2DGrayScaleEffect
		: Gorgon2DEffect, IGorgon2DTextureDrawEffect
	{
        #region Variables.
        // The batch state to use when rendering.
	    private Gorgon2DBatchState _batchState;
        // The shader used to render grayscale images.
	    private Gorgon2DShader<GorgonPixelShader> _grayScaleShader;
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
	        // Compile our blur shader.
	        _grayScaleShader = PixelShaderBuilder
	                           .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderGrayScale"))
	                           .Build();

	        _batchState = BatchStateBuilder
	                      .PixelShader(_grayScaleShader)
	                      .Build();
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

		    Gorgon2DShader<GorgonPixelShader> shader = Interlocked.Exchange(ref _grayScaleShader, null);
		    shader?.Dispose();
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
	    public void RenderEffect(GorgonTexture2DView texture,
	                             RectangleF? region = null,
	                             RectangleF? textureCoordinates = null,
	                             GorgonSamplerState samplerStateOverride = null,
	                             GorgonBlendState blendStateOverride = null,
	                             Gorgon2DCamera camera = null)
	    {
	        RenderTexture(texture, region, textureCoordinates, samplerStateOverride, blendStateOverride, camera: camera);
	    }
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DGrayScaleEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DGrayScaleEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_GRAYSCALE, Resources.GOR2D_EFFECT_GRAYSCALE_DESC, 1)
		{
		    // A macro used to define the size of the kernel weight data structure.
            Macros.Add(new GorgonShaderMacro("GRAYSCALE_EFFECT"));
		}
		#endregion
	}
}
