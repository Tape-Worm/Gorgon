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

using System;
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An effect that renders as gray scale.
	/// </summary>
	public class Gorgon2DGrayScaleEffect
		: Gorgon2DEffect
	{
        #region Variables.
        // The batch state to use when rendering.
	    private Gorgon2DBatchState _batchState;
        // The shader used to render grayscale images.
	    private Gorgon2DShader<GorgonPixelShader> _grayScaleShader;
        // A method used to render data while the effect is active.
	    private Action _renderCallback;
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
	        // A macro used to define the size of the kernel weight data structure.
	        GorgonShaderMacro[] weightsMacro = {
	                                               new GorgonShaderMacro("GRAYSCALE_EFFECT")
	                                           };

	        // Compile our blur shader.
            var shaderBuilder = new Gorgon2DShaderBuilder<GorgonPixelShader>();
	        _grayScaleShader = shaderBuilder.Shader(GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics,
	                                                                                               Resources.BasicSprite,
	                                                                                               "GorgonPixelShaderGrayScale",
	                                                                                               GorgonGraphics.IsDebugEnabled,
	                                                                                               weightsMacro))
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
		    shader?.Shader.Dispose();
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
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="batchState">The current batch state for the pass.</param>
        /// <param name="camera">The current camera to use when rendering.</param>
        /// <remarks>Applications must implement this in order to see any results from the effect.</remarks>
        protected override void OnRenderPass(int passIndex, Gorgon2DBatchState batchState, Gorgon2DCamera camera)
	    {
	        Renderer.Begin(batchState, camera);
	        _renderCallback();
            Renderer.End();
	    }

	    /// <summary>
        /// Funcion to render the specified texture as grayscale.
        /// </summary>
        /// <param name="renderCallback">A method that is executed to render image data as gray scale.</param>
        /// <param name="blendState">[Optional] A blending state used to override the default blending state.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderCallback"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="renderCallback"/> is a method that users can define to draw whatever is needed as grayscale.  When this method is called, the <see cref="Gorgon2D.Begin"/> and
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
	    public void GrayScale(Action renderCallback, GorgonBlendState blendState = null, Gorgon2DCamera camera = null)
	    {
            renderCallback.ValidateObject(nameof(renderCallback));
	        _renderCallback = renderCallback;
            
            Render(blendState, null, null, camera);

	        _renderCallback = null;
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
		}
		#endregion
	}
}
