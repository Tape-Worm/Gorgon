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
// Created: Monday, April 09, 2012 7:14:08 AM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that displaces the pixels on an image using the pixels from another image for weighting.
	/// </summary>
	public class Gorgon2DDisplacementEffect
		: Gorgon2DEffect
	{
		#region Variables.
        // The shader used for displacement.
	    private Gorgon2DShader<GorgonPixelShader> _displacementShader;
	    // The displacement render target.
		private GorgonRenderTarget2DView _displacementTarget;
        // The displacement texture view.
	    private GorgonTexture2DView _displacementView;
        // The texture that contains the pixels to displace.
		private GorgonTexture2DView _backgroundTarget;								
        // The constant buffer for displacement settings.
		private GorgonConstantBufferView _displacementSettingsBuffer;				
	    // Flag to indicate that the parameters have been updated.
		private bool _isUpdated = true;											
	    // Strength of the displacement map.
		private float _displacementStrength = 1.0f;
        // Method called to render the displacement effect.
	    private Action _displacementRender;
        // The previously active render target view.
	    private GorgonRenderTargetView _prevRtv;
        // The previously active depth/stencil view.
	    private GorgonDepthStencil2DView _prevDsv;
        // The batch state.
	    private Gorgon2DBatchState _batchState;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the strength of the displacement map.
		/// </summary>
		public float Strength
		{
			get => _displacementStrength;
		    set
			{
				if (value < 0.0f)
				{
					value = 0.0f;
				}
				
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_displacementStrength == value)
				{
					return;
				}

				_displacementStrength = value;
				_isUpdated = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the displacement map render target.
		/// </summary>
		private void UpdateDisplacementMap()
		{
		    if ((_displacementView != null) 
		        && (_backgroundTarget != null) 
		        && (_displacementView.Width == _backgroundTarget.Width) 
		        && (_displacementView.Height == _backgroundTarget.Height) 
		        && (_displacementView.Format == _backgroundTarget.Format))
		    {
		        return;
		    }

            _displacementView?.Dispose();
			_displacementTarget?.Dispose();
			_displacementTarget = null;
		    _displacementView = null;

			if (_backgroundTarget == null)
			{
				return;
			}

#if DEBUG
		    if (!Graphics.FormatSupport[_backgroundTarget.Format].IsRenderTargetFormat)
		    {
		        throw new GorgonException(GorgonResult.CannotWrite,
		                                  string.Format(Resources.GOR2D_ERR_EFFECT_DISPLACEMENT_UNSUPPORTED_FORMAT, _backgroundTarget.Format));
		    }
#endif

		    _displacementTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics,
		                                                                      new GorgonTexture2DInfo("Effect.Displacement.RT")
		                                                                      {
		                                                                          Width = _backgroundTarget.Width,
		                                                                          Height = _backgroundTarget.Height,
		                                                                          Format = _backgroundTarget.Format,
		                                                                          Binding = TextureBinding.ShaderResource
		                                                                      });
		    _displacementView = _displacementTarget.Texture.GetShaderResourceView();

            // We store this in the 1st slot so we can read back from it when necessary.
		    _displacementShader = PixelShaderBuilder
		                          .ShaderResource(_displacementView, 1)
		                          .Build();

		    _batchState = BatchStateBuilder
		                  .PixelShader(_displacementShader)
		                  .Build();

			_isUpdated = true;
		}

        /// <summary>
        /// Function called when the effect is being initialized.
        /// </summary>
        /// <remarks>
        /// Use this method to set up the effect upon its creation.  For example, this method could be used to create the required shaders for the effect.
        /// </remarks>
	    protected override void OnInitialize()
        {
            _displacementSettingsBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                                        new
                                                                                        GorgonConstantBufferInfo("Gorgon2DDisplacementEffect Constant Buffer")
                                                                                        {
                                                                                            Usage = ResourceUsage.Dynamic,
                                                                                            SizeInBytes = DX.Vector4.SizeInBytes
                                                                                        });
	        _displacementShader = PixelShaderBuilder
	                              .Shader(CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderDisplacementDecoder"))
	                              .ConstantBuffer(_displacementSettingsBuffer, 1)
	                              .Build();

            _batchState = BatchStateBuilder
                          .PixelShader(_displacementShader)
                          .BlendState(GorgonBlendState.NoBlending)
                          .Build();
        }

	    /// <summary>
		/// Function to free any resources allocated by the effect.
		/// </summary>
		public void FreeResources()
	    {
	        Interlocked.Exchange(ref _backgroundTarget, null);
	        GorgonTexture2DView view = Interlocked.Exchange(ref _displacementView, null);
	        GorgonRenderTarget2DView rtv = Interlocked.Exchange(ref _displacementTarget, null);
            view?.Dispose();
            rtv?.Dispose();
	    }

		/// <summary>
		/// Function called before rendering begins.
		/// </summary>
		/// <returns>
		/// <b>true</b> to continue rendering, <b>false</b> to exit.
		/// </returns>
		protected override bool OnBeforeRender()
		{
		    _prevRtv = Graphics.RenderTargets[0];
		    _prevDsv = Graphics.DepthStencilView;

			UpdateDisplacementMap();

			if (!_isUpdated)
			{
				return base.OnBeforeRender();
			}

			var settings = new DX.Vector4(1.0f / _backgroundTarget.Width, 1.0f / _backgroundTarget.Height, _displacementStrength, 0);
			_displacementSettingsBuffer.Buffer.SetData(ref settings);
			_isUpdated = false;

			return base.OnBeforeRender();
		}

        /// <summary>
        /// Function called after rendering ends.
        /// </summary>
	    protected override void OnAfterRender()
	    {
	        if ((_prevRtv == Graphics.RenderTargets[0])
	            && (_prevDsv == Graphics.DepthStencilView))
	        {
	            return;
	        }

            Graphics.SetRenderTarget(_prevRtv, _prevDsv);
	    }

	    /// <summary>
	    /// Function called prior to rendering a pass.
	    /// </summary>
	    /// <param name="passIndex">The index of the pass to render.</param>
	    /// <returns><b>true</b> if rendering the current pass should continue, or <b>false</b> if not.</returns>
	    /// <remarks>
	    /// <para>
	    /// Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.
	    /// </para>
	    /// </remarks>
	    protected override bool OnBeforeRenderPass(int passIndex)
	    {
			if ((_displacementTarget == null) || (_backgroundTarget == null))
			{
				return false;
			}

	        switch (passIndex)
	        {
	            case 0:
                    _displacementTarget.Clear(GorgonColor.BlackTransparent);
                    Graphics.SetRenderTarget(_displacementTarget, _prevDsv);
	                break;
                case 1:
                    Graphics.SetRenderTarget(_prevRtv, _prevDsv);
                    break;
	        }

			return true;
		}

	    /// <summary>
	    /// Function called to render a single effect pass.
	    /// </summary>
	    /// <param name="passIndex">The index of the pass being rendered.</param>
	    /// <param name="batchState">The current batch state for the pass.</param>
	    /// <param name="camera">The current camera to use when rendering.</param>
	    /// <remarks>
	    /// <para>
	    /// Applications must implement this in order to see any results from the effect.
	    /// </para>
	    /// </remarks>
	    protected override void OnRenderPass(int passIndex, Gorgon2DBatchState batchState, Gorgon2DCamera camera)
	    {
            Renderer.Begin(batchState, camera);

	        switch (passIndex)
	        {
	            case 0:
	                _displacementRender();
	                break;
                case 1:
                    Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _backgroundTarget.Width, _backgroundTarget.Height), GorgonColor.White, _backgroundTarget);
                    break;
	        }

            Renderer.End();
	    }

	    /// <summary>
	    /// Function called to build a new (or return an existing) 2D batch state.
	    /// </summary>
	    /// <param name="passIndex">The index of the current rendering pass.</param>
	    /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
	    /// <returns>The 2D batch state.</returns>
	    protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
	    {
            return passIndex == 0 ? null : _batchState;
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

            FreeResources();

		    GorgonConstantBufferView displacementBuffer = Interlocked.Exchange(ref _displacementSettingsBuffer, null);
		    Gorgon2DShader<GorgonPixelShader> shader = Interlocked.Exchange(ref _displacementShader, null);

            displacementBuffer?.Dispose();
		    shader?.Dispose();
		}

        /// <summary>
        /// Function to displace the pixels on an image by using another image as a displacement map.
        /// </summary>
        /// <param name="displacementRender">The method used to render into the displacement map.</param>
        /// <param name="backgroundImage">The image that will be distorted by the displacement.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="displacementRender"/> is a method that users can define to draw whatever is needed as grayscale.  When this method is called, the <see cref="Gorgon2D.Begin"/> and
        /// <see cref="Gorgon2D.End"/> methods are already taken care of by the effect and will not need to be called during the callback.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled in <b>DEBUG</b> mode.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
	    public void Displace(Action displacementRender, GorgonTexture2DView backgroundImage, Gorgon2DCamera camera = null)
	    {
            displacementRender.ValidateObject(nameof(displacementRender));
            backgroundImage.ValidateObject(nameof(backgroundImage));

	        _backgroundTarget = backgroundImage;
	        _displacementRender = displacementRender;

	        Render(camera: camera);
	    }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DDisplacementEffect"/> class.
		/// </summary>
		/// <param name="renderer">The renderer used to render this effect.</param>
		public Gorgon2DDisplacementEffect(Gorgon2D renderer)
			: base(renderer, Resources.GOR2D_EFFECT_DISPLACEMENT, Resources.GOR2D_EFFECT_DISPLACEMENT_DESC, 2)
		{
		    Macros.Add(new GorgonShaderMacro("DISPLACEMENT_EFFECT"));
		}
		#endregion
	}
}
