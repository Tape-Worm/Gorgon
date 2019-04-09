#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 17, 2019 1:12:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The service used to build a sampler.
    /// </summary>
    internal class SamplerBuildService
		: ISamplerBuildService
    {
        #region Variables.
		// The builder used to create the sampler state.
        private readonly GorgonSamplerStateBuilder _builder;
        #endregion

        #region Methods.
        /// <summary>Function to retrieve the sampler based on the specified states.</summary>
        /// <param name="filter">The texture filtering state to use.</param>
        /// <param name="wrapU">The horizontal wrapping state.</param>
        /// <param name="wrapV">The vertical wrapping state.</param>
        /// <param name="borderColor">The border color to use when one of the wrapping states is set to <see cref="TextureWrap.Border"/>.</param>
        /// <returns>The sampler state.</returns>
        public GorgonSamplerState GetSampler(SampleFilter filter, TextureWrap wrapU, TextureWrap wrapV, GorgonColor borderColor)
        {
            if ((filter == GorgonSamplerState.Default.Filter)
                && (wrapU == GorgonSamplerState.Default.WrapU)
                && (wrapV == GorgonSamplerState.Default.WrapV)
                && (borderColor.Equals(GorgonSamplerState.Default.BorderColor)))
            {
                return GorgonSamplerState.Default;
            }

            if ((filter == GorgonSamplerState.PointFiltering.Filter)
                && (wrapU == GorgonSamplerState.PointFiltering.WrapU)
                && (wrapV == GorgonSamplerState.PointFiltering.WrapV)
                && (borderColor.Equals(GorgonSamplerState.PointFiltering.BorderColor)))
            {
                return GorgonSamplerState.PointFiltering;
            }

            if ((filter == GorgonSamplerState.Wrapping.Filter)
                && (wrapU == GorgonSamplerState.Wrapping.WrapU)
                && (wrapV == GorgonSamplerState.Wrapping.WrapV)
                && (borderColor.Equals(GorgonSamplerState.Wrapping.BorderColor)))
            {
                return GorgonSamplerState.Wrapping;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((filter == GorgonSamplerState.PointFilteringWrapping.Filter)
                && (wrapU == GorgonSamplerState.PointFilteringWrapping.WrapU)
                && (wrapV == GorgonSamplerState.PointFilteringWrapping.WrapV)
                && (borderColor.Equals(GorgonSamplerState.PointFilteringWrapping.BorderColor)))
            {
                return GorgonSamplerState.PointFilteringWrapping;
            }

            return _builder.Clear()
                   .Filter(filter)
                   .Wrapping(wrapU, wrapV, borderColor: borderColor)
                   .Build();
#pragma warning restore IDE0046 // Convert to conditional expression
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SamplerBuildService"/> class.</summary>
        /// <param name="builder">The builder used to create the sampler state.</param>
        public SamplerBuildService(GorgonSamplerStateBuilder builder) => _builder = builder;        
        #endregion
    }
}
