#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 4, 2016 1:05:13 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Collections;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A list of shader resource views to apply to the pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The shader resource view list is used to bind resources like textures and structured buffers to the GPU pipeline so that shaders can make use of them.
    /// </para>
    /// <para>
    /// If a resource being bound is bound to the <see cref="GorgonGraphics.RenderTargets"/> list, then the render target view will be unbound from the pipeline and rebound as a shader resource. This is
    /// because the render target cannot be used as a shader resource and a render target at the same time.
    /// </para>
    /// </remarks>
    public sealed class GorgonShaderResourceViews
        : GorgonArray<GorgonShaderResourceView>
    {
        #region Constants.
        /// <summary>
        /// The maximum size for a shader resource view binding list.
        /// </summary>
        public const int MaximumShaderResourceViewCount = 64;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the native buffers.
        /// </summary>
        internal D3D11.ShaderResourceView[] Native
        {
            get;
        } = new D3D11.ShaderResourceView[MaximumShaderResourceViewCount];
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when a dirty item is found and added.
        /// </summary>
        /// <param name="dirtyIndex">The index that is considered dirty.</param>
        /// <param name="value">The dirty value.</param>
        protected override void OnAssignDirtyItem(int dirtyIndex, GorgonShaderResourceView value) => Native[dirtyIndex] = value?.Native;

        /// <summary>
        /// Function called when the array is cleared.
        /// </summary>
        protected override void OnClear() => Array.Clear(Native, 0, Native.Length);
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderResourceViews"/> class.
        /// </summary>
        /// <param name="bufferViews">[Optional] A list of buffer views to copy into the the list.</param>
        public GorgonShaderResourceViews(IReadOnlyList<GorgonShaderResourceView> bufferViews = null)
            : base(MaximumShaderResourceViewCount)
        {
            if (bufferViews is null)
            {
                return;
            }

            for (int i = 0; i < bufferViews.Count.Min(Length); ++i)
            {
                this[i] = bufferViews[i];
            }
        }
        #endregion
    }
}
