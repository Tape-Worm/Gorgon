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
		private GorgonTexture2DView _backgroundView;
        // The constant buffer for displacement settings.
		private GorgonConstantBufferView _displacementSettingsBuffer;				
	    // Flag to indicate that the parameters have been updated.
		private bool _isUpdated = true;											
	    // Strength of the displacement map.
		private float _displacementStrength = 0.25f;
        // Method called to render the displacement effect.
	    private Action _displacementRender;
        // The final output render target.
	    private GorgonRenderTargetView _outputRtv;
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
		        && (_backgroundView != null) 
		        && (_displacementView.Width == _backgroundView.Width) 
		        && (_displacementView.Height == _backgroundView.Height) 
		        && (_displacementView.Format == _backgroundView.Format))
		    {
		        return;
		    }

            _displacementView?.Dispose();
			_displacementTarget?.Dispose();
			_displacementTarget = null;
		    _displacementView = null;

			if (_backgroundView == null)
			{
				return;
			}

#if DEBUG
		    if (!Graphics.FormatSupport[_backgroundView.Format].IsRenderTargetFormat)
		    {
		        throw new GorgonException(GorgonResult.CannotWrite,
		                                  string.Format(Resources.GOR2D_ERR_EFFECT_DISPLACEMENT_UNSUPPORTED_FORMAT, _backgroundView.Format));
		    }
#endif

		    _displacementTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics,
		                                                                      new GorgonTexture2DInfo("Effect.Displacement.RT")
		                                                                      {
		                                                                          Width = _backgroundView.Width,
		                                                                          Height = _backgroundView.Height,
		                                                                          Format = _backgroundView.Format,
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
			UpdateDisplacementMap();

			if (!_isUpdated)
			{
				return base.OnBeforeRender();
			}

			var settings = new DX.Vector4(1.0f / _backgroundView.Width, 1.0f / _backgroundView.Height, _displacementStrength * 100, 0);
			_displacementSettingsBuffer.Buffer.SetData(ref settings);
			_isUpdated = false;

			return base.OnBeforeRender();
		}

        /// <summary>
        /// Function called prior to rendering a pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <returns>A <see cref="PassContinuationState" /> to instruct the effect on how to proceed.</returns>
        /// <seealso cref="PassContinuationState" />
        /// <remarks>Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.</remarks>
        protected override PassContinuationState OnBeforeRenderPass(int passIndex)
	    {
			if ((_displacementTarget == null) || (_backgroundView == null))
			{
				return PassContinuationState.Stop;
			}

	        switch (passIndex)
	        {
	            case 0:
                    _displacementTarget.Clear(GorgonColor.BlackTransparent);
                    Graphics.SetRenderTarget(_displacementTarget, Graphics.DepthStencilView);
	                break;
                case 1:
                    Graphics.SetRenderTarget(_outputRtv, Graphics.DepthStencilView);
                    break;
	        }

			return PassContinuationState.Continue;
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
                    Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _backgroundView.Width, _backgroundView.Height), GorgonColor.White, _backgroundView);
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
        /// <param name="outputTarget">The render target that will receive the displaced image.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="displacementRender"/>, <paramref name="backgroundImage"/>, or the <paramref name="outputTarget"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="displacementRender"/> is a method that users can define to draw whatever will displace the underlying <paramref name="backgroundImage"/>.  When this method is called, the
        /// <see cref="Gorgon2D.Begin"/> and <see cref="Gorgon2D.End"/> methods are already taken care of by the effect and will not need to be called during the callback.
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled in <b>DEBUG</b> mode.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
	    public void RenderEffect(Action displacementRender, GorgonTexture2DView backgroundImage, GorgonRenderTargetView outputTarget, Gorgon2DCamera camera = null)
	    {
            displacementRender.ValidateObject(nameof(displacementRender));
            backgroundImage.ValidateObject(nameof(backgroundImage));
            outputTarget.ValidateObject(nameof(outputTarget));

	        _outputRtv = outputTarget;
	        _backgroundView = backgroundImage;
	        _displacementRender = displacementRender;

	        Render(camera: camera);

	        _backgroundView = null;
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
