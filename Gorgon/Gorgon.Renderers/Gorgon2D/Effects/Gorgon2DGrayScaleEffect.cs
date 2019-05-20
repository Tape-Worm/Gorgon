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
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Properties;
using SharpDX;

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
        private GorgonPixelShader _grayScaleShader;
	    private Gorgon2DShaderState<GorgonPixelShader> _grayScaleState;
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
            _grayScaleShader = CompileShader<GorgonPixelShader>(Resources.BasicSprite, "GorgonPixelShaderGrayScale");
            _grayScaleState = PixelShaderBuilder
	                           .Shader(_grayScaleShader)
	                           .Build();

	        _batchState = BatchStateBuilder
	                      .PixelShaderState(_grayScaleState)
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

		    GorgonPixelShader shader = Interlocked.Exchange(ref _grayScaleShader, null);
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
        /// Function called prior to rendering.
        /// </summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
        /// targets (if applicable).
        /// </para>
        /// </remarks>
        protected override void OnBeforeRender(GorgonRenderTargetView output, IGorgon2DCamera camera, bool sizeChanged)
	    {
	        if (Graphics.RenderTargets[0] != output)
	        {
                Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
	        }
	    }

        /// <summary>
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="renderMethod">The method used to render a scene for the effect.</param>
        /// <param name="output">The render target that will receive the final render data.</param>
        /// <remarks>
        /// <para>
        /// Applications must implement this in order to see any results from the effect.
        /// </para>
        /// </remarks>
        protected override void OnRenderPass(int passIndex, Action<int, int, Size2> renderMethod, GorgonRenderTargetView output) => renderMethod(passIndex, PassCount, new Size2(output.Width, output.Height));
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DGrayScaleEffect" /> class.
        /// </summary>
        /// <param name="renderer">The renderer used to render this effect.</param>
        public Gorgon2DGrayScaleEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_GRAYSCALE, Resources.GOR2D_EFFECT_GRAYSCALE_DESC, 1) =>
            // A macro used to define the size of the kernel weight data structure.
            Macros.Add(new GorgonShaderMacro("GRAYSCALE_EFFECT"));
        #endregion
    }
}
