#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, August 19, 2013 11:37:22 PM
// 
#endregion

using System;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A rendering pass for an effect.
    /// </summary>
    /// <remarks>A pass is used to render a scene once using the supplied rendering states and shaders.</remarks>
    public class GorgonEffectPass
    {
        #region Properties.
        /// <summary>
        /// Property to return the effect that owns this pass.
        /// </summary>
        public GorgonEffect Effect
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the pixel shader for this pass.
        /// </summary>
        public GorgonPixelShader PixelShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the vertex shader for this pass.
        /// </summary>
        public GorgonVertexShader VertexShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the geometry shader for this pass.
        /// </summary>
        public GorgonGeometryShader GeometryShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the compute shader for this pass.
        /// </summary>
        public GorgonComputeShader ComputeShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the hull shader for this pass.
        /// </summary>
        public GorgonHullShader HullShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the domain shader for this pass.
        /// </summary>
        public GorgonDomainShader DomainShader
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the index of this pass.
        /// </summary>
        public int PassIndex
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to set or return the action to perform when this pass is being rendered.
        /// </summary>
        /// <remarks>This is the method that is typically used to draw the scene.</remarks>
        public Action<GorgonEffectPass> RenderAction
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonEffectPass"/> class.
        /// </summary>
        /// <param name="effect">Effect that owns the pass.</param>
        /// <param name="passIndex">Index of the pass.</param>
        internal GorgonEffectPass(GorgonEffect effect, int passIndex)
        {
            Effect = effect;
            PassIndex = passIndex;
        }
        #endregion
    }
}
