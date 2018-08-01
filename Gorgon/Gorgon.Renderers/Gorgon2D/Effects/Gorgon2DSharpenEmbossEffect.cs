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

using System.Threading;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that sharpens (and optionally embosses) an image.
	/// </summary>
	public class Gorgon2DSharpenEmbossEffect
		: Gorgon2DEffect, IGorgon2DTextureDrawEffect
	{
		#region Variables.
	    // Constant buffer for the sharpen/emboss information.
		private GorgonConstantBufferView _sharpenEmbossBuffer;			        
	    // Pixel shader used to sharpen an image.
		private Gorgon2DShader<GorgonPixelShader> _sharpenShader;							
	    // Pixel shader used to emboss an image.
		private Gorgon2DShader<GorgonPixelShader> _embossShader;
        // The batch render state for sharpening.
	    private Gorgon2DBatchState _sharpenBatchState;
        // The batch render state for embossing.
	    private Gorgon2DBatchState _embossBatchState;
	    // Amount to sharpen/emboss
		private float _amount = 0.5f;										
	    // Flag to indicate that the parameters were updated.
		private bool _isUpdated = true;										
	    // Area to emboss.
		private DX.Size2F _size = DX.Size2F.Empty;								
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
			get => _amount;
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
	        _sharpenEmbossBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo("Gorgon 2D Sharpen/Emboss Constant Buffer")
	                                                                                       {
                                                                                               SizeInBytes = 16
	                                                                                       });

	        _sharpenShader = PixelShaderBuilder
	                         .ConstantBuffer(_sharpenEmbossBuffer, 1)
	                         .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderSharpen"))
	                         .Build();

	        _embossShader = PixelShaderBuilder
	                        .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderEmboss"))
	                        .Build();

	        _sharpenBatchState = BatchStateBuilder
	                             .PixelShader(_sharpenShader)
	                             .Build();

	        _embossBatchState = BatchStateBuilder
	                            .PixelShader(_embossShader)
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
	        // ReSharper disable once InvertIf
	        if (statesChanged)
	        {
	            _sharpenBatchState = BatchStateBuilder.Build();
	            _embossBatchState = BatchStateBuilder.Build();
	        }

	        return UseEmbossing ? _embossBatchState : _sharpenBatchState;
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

		    var settings = new DX.Vector3(1.0f / _size.Width, 1.0f / _size.Height, _amount);

		    _sharpenEmbossBuffer.Buffer.SetData(ref settings);
		    _isUpdated = false;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
		    GorgonConstantBufferView buffer = Interlocked.Exchange(ref _sharpenEmbossBuffer, null);
		    Gorgon2DShader<GorgonPixelShader> shader1 = Interlocked.Exchange(ref _sharpenShader, null);
		    Gorgon2DShader<GorgonPixelShader> shader2 = Interlocked.Exchange(ref _embossShader, null);

            buffer?.Dispose();
            shader1?.Dispose();
            shader2?.Dispose();
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
	        _size = region?.Size ?? new DX.Size2F(CurrentTargetSize.Width, CurrentTargetSize.Height);

	        RenderTexture(texture, region, textureCoordinates, samplerStateOverride, blendStateOverride, camera: camera);
	    }
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DSharpenEmbossEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DSharpenEmbossEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_SHARPEMBOSS, Resources.GOR2D_EFFECT_SHARPEMBOSS_DESC, 1)
		{
			Macros.Add(new GorgonShaderMacro("SHARPEN_EMBOSS_EFFECT"));
		}
		#endregion
	}
}
