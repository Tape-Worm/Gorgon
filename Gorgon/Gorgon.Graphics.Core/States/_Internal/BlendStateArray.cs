#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 4, 2021 2:30:19 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;
using Gorgon.Collections;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The array for holding blend state information.
    /// </summary>
    internal class BlendStateArray
        : GorgonArray<GorgonBlendState>
    {
        #region Methods.
        /// <summary>
        /// Function to build the D3D11 blend state.
        /// </summary>
        /// <param name="device">The device used to create the blend state.</param>
        /// <param name="isAlphaToCoverageEnabled">Flag to enable/disable alpha to coverage.</param>
        /// <param name="isIndependentBlendEnabled">Flag to enable/disable independent blend targets.</param>
        public D3D11.BlendState1 BuildD3D11BlendState(D3D11.Device5 device, bool isAlphaToCoverageEnabled, bool isIndependentBlendEnabled)
        {
            ref readonly (int start, int count) indices = ref GetDirtyItems();
            var desc = new D3D11.BlendStateDescription1
            {
                AlphaToCoverageEnable = isAlphaToCoverageEnabled,
                IndependentBlendEnable = isIndependentBlendEnabled
            };

            for (int i = 0; i < indices.count; ++i)
            {
                GorgonBlendState state = this[indices.start + i];

                if (state == null)
                {
                    continue;
                }

                desc.RenderTarget[i] = new D3D11.RenderTargetBlendDescription1
                {
                    AlphaBlendOperation = (D3D11.BlendOperation)state.AlphaBlendOperation,
                    BlendOperation = (D3D11.BlendOperation)state.ColorBlendOperation,
                    IsLogicOperationEnabled = state.LogicOperation != LogicOperation.Noop,
                    IsBlendEnabled = state.IsBlendingEnabled,
                    RenderTargetWriteMask = (D3D11.ColorWriteMaskFlags)state.WriteMask,
                    LogicOperation = (D3D11.LogicOperation)state.LogicOperation,
                    SourceAlphaBlend = (D3D11.BlendOption)state.SourceAlphaBlend,
                    SourceBlend = (D3D11.BlendOption)state.SourceColorBlend,
                    DestinationAlphaBlend = (D3D11.BlendOption)state.DestinationAlphaBlend,
                    DestinationBlend = (D3D11.BlendOption)state.DestinationColorBlend
                };
            }

            return new D3D11.BlendState1(device, desc)
            {
                DebugName = nameof(GorgonBlendState)
            };
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="BlendStateArray" /> class.</summary>
        internal BlendStateArray()
            : base(D3D11.OutputMergerStage.SimultaneousRenderTargetCount)
        {
        }
        #endregion        
    }
}
