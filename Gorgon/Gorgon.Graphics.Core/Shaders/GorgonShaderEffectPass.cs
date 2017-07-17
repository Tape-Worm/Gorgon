#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 10, 2017 12:52:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A single pass for a shader effect.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Passes are used to repeat rendering using different parameters or even shaders and composite the results.
    /// </para>
    /// </remarks>
    public class GorgonShaderEffectPass
    {
        #region Variables.
        // The effect that owns this pass.
        private readonly GorgonShaderEffect _effect;
        // The pipeline state to apply when rendering this pass.
        private readonly GorgonPipelineState _pipelineState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics interface that owns this pass.
        /// </summary>
        public GorgonGraphics Graphics => _effect.Graphics;

        /// <summary>
        /// Property to return the index of this pass.
        /// </summary>
        public int Index
        {
            get;
        }

        /// <summary>
        /// Property to return the action that will render data for this pass.
        /// </summary>
        public Action<GorgonPipelineState> RenderAction
        {
            get;
        }
        #endregion

        #region Methods.
        internal void Begin(GorgonRenderTargetView[] rtvs, GorgonDepthStencilView depthStencilView)
        {
            Graphics.SetRenderTargets(rtvs, depthStencilView);
        }

        internal void End()
        {
        }

        /// <summary>
        /// Function called to render data.
        /// </summary>
        internal void Render()
        {
            RenderAction?.Invoke(_pipelineState);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderEffectPass"/> class.
        /// </summary>
        /// <param name="shaderEffect">The shader effect that owns this pass.</param>
        /// <param name="renderAction">The action used to render data for the pass.</param>
        /// <param name="pipelineState">The current state of the pipeline.</param>
        /// <param name="passIndex">Index of the pass.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="shaderEffect"/>, <paramref name="renderAction"/> or the <paramref name="pipelineState"/> parameter is <b>null</b>.</exception>
        public GorgonShaderEffectPass(GorgonShaderEffect shaderEffect, Action<GorgonPipelineState> renderAction, GorgonPipelineState pipelineState, int passIndex)
        {
            _effect = shaderEffect ?? throw new ArgumentNullException(nameof(shaderEffect));
            RenderAction = renderAction ?? throw new ArgumentNullException(nameof(renderAction));
            _pipelineState = pipelineState ?? throw new ArgumentNullException(nameof(pipelineState));

            Index = passIndex;
        }
        #endregion
    }
}
