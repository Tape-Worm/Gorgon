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

using System.Threading;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;


namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders images burn/dodge effect.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This provides an image post processing filter akin to the dodge and burn effect filters in Photoshop.
	/// </para>
	/// </remarks>
	public class Gorgon2DBurnDodgeEffect
		: Gorgon2DEffect, IGorgon2DTextureDrawEffect
	{
		#region Variables.
	    // Burn/dodge buffer.
		private GorgonConstantBufferView _burnDodgeBuffer;			    
	    // Dodge/burn shader.
		private Gorgon2DShader<GorgonPixelShader> _dodgeBurn;							
	    // Linear dodge/burn shader.
		private Gorgon2DShader<GorgonPixelShader> _linearDodgeBurn;						
	    // Flag to indicate that the effect parameters are updated.
		private bool _isUpdated = true;									
	    // Flag to indicate whether to use dodging or burning.
		private bool _useDodge;
        // The batch render state for a linear burn/dodge.
	    private Gorgon2DBatchState _batchStateLinearDodgeBurn;
        // The batch render state.
	    private Gorgon2DBatchState _batchStateDodgeBurn;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to use a burn or dodge effect.
		/// </summary>
		public bool UseDodge
		{
			get => _useDodge;
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
	        _burnDodgeBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
	                                                                         new GorgonConstantBufferInfo("Gorgon 2D Burn/Dodge Effect Constant Buffer")
	                                                                         {
	                                                                             Usage = ResourceUsage.Default,
	                                                                             SizeInBytes = 16
	                                                                         });

	        _linearDodgeBurn = PixelShaderBuilder
	                           .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderLinearBurnDodge"))
	                           .ConstantBuffer(_burnDodgeBuffer, 1)
	                           .Build();

	        _dodgeBurn = PixelShaderBuilder
	                     .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderBurnDodge"))
	                     .Build();

	        _batchStateLinearDodgeBurn = BatchStateBuilder.PixelShader(_linearDodgeBurn)
	                                                      .Build();
	        _batchStateDodgeBurn = BatchStateBuilder.PixelShader(_dodgeBurn)
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
	            _batchStateLinearDodgeBurn = BatchStateBuilder.Build();
	            _batchStateDodgeBurn = BatchStateBuilder.Build();
	        }

	        return UseLinear ? _batchStateLinearDodgeBurn : _batchStateDodgeBurn;
	    }

	    /// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
		    GorgonRenderTargetView currentTarget = Graphics.RenderTargets[0];

		    if (currentTarget == null)
		    {
		        return false;
		    }

		    if (!_isUpdated)
		    {
		        return true;
		    }

		    _burnDodgeBuffer.Buffer.SetData(ref _useDodge);
			_isUpdated = false;
		    return true;
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

		    GorgonConstantBufferView buffer = Interlocked.Exchange(ref _burnDodgeBuffer, null);
		    Gorgon2DShader<GorgonPixelShader> shader1 = Interlocked.Exchange(ref _linearDodgeBurn, null);
		    Gorgon2DShader<GorgonPixelShader> shader2 = Interlocked.Exchange(ref _dodgeBurn, null);

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
	        RenderTexture(texture, region, textureCoordinates, samplerStateOverride, blendStateOverride, camera: camera);
	    }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DBurnDodgeEffect"/> class.
		/// </summary>
		/// <param name="renderer">The renderer used to draw this effect.</param>
		public Gorgon2DBurnDodgeEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_BURN_DODGE, Resources.GOR2D_EFFECT_BURN_DODGE_DESC, 1)
		{
		    Macros.Add(new GorgonShaderMacro("BURN_DODGE_EFFECT"));
		}
		#endregion
	}
}
