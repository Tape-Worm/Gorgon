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
// Created: January 3, 2021 11:59:00 PM
// 
#endregion

using System;
using System.Runtime.CompilerServices;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A cache for holding previously defined pipeline state objects.
    /// </summary>
    internal class PipelineStateCache
        : IDisposable
    {
        #region Constants.
        // The initial size for the cache.
        private const int InitialCacheSize = 16;
        #endregion

        #region Variables.
        // A syncrhonization lock for multiple thread when dealing with the pipeline state cache.
        private readonly object _stateLock = new object();
        // A list of cached pipeline states.
        private GorgonPipelineState[] _cachedPipelineStates = new GorgonPipelineState[InitialCacheSize];
        // The Direct 3D device.
        private readonly D3D11.Device5 _device;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to invalidate the cache data.
        /// </summary>
        private void InvalidateCache()
        {
            foreach (GorgonPipelineState state in _cachedPipelineStates)
            {
                if (state is null)
                {
                    break;
                }

                state.ID = int.MinValue;
                state.D3DRasterState?.Dispose();
                state.D3DRasterState = null;
                state.D3DDepthStencilState?.Dispose();
                state.D3DDepthStencilState = null;
                state.D3DBlendState?.Dispose();
                state.D3DBlendState = null;
            }

            Array.Clear(_cachedPipelineStates, 0, _cachedPipelineStates.Length);

            if (_cachedPipelineStates.Length != InitialCacheSize)
            {
                Array.Resize(ref _cachedPipelineStates, InitialCacheSize);
            }
        }

        /// <summary>
        /// Function to compare two states, and determine what values are the same.
        /// </summary>
        /// <param name="cachedState">The previously cached state.</param>
        /// <param name="newState">The new state.</param>
        /// <returns>The enumeration containing the flags of which states are the same.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PipelineStateChanges CompareState(GorgonPipelineState cachedState, GorgonPipelineState newState)
        {
            PipelineStateChanges inheritedState = (cachedState.PrimitiveType == newState.PrimitiveType) ? PipelineStateChanges.Topology : PipelineStateChanges.None;
            inheritedState |= (cachedState.VertexShader == newState.VertexShader) ? PipelineStateChanges.VertexShader : PipelineStateChanges.None;
            inheritedState |= (cachedState.PixelShader == newState.PixelShader) ? PipelineStateChanges.PixelShader : PipelineStateChanges.None;
            inheritedState |= (cachedState.GeometryShader == newState.GeometryShader) ? PipelineStateChanges.GeometryShader : PipelineStateChanges.None;                        
            inheritedState |= (cachedState.DomainShader == newState.DomainShader) ? PipelineStateChanges.DomainShader : PipelineStateChanges.None;
            inheritedState |= (cachedState.HullShader == newState.HullShader) ? PipelineStateChanges.HullShader : PipelineStateChanges.None;
            inheritedState |= (cachedState.RasterState.Equals(newState.RasterState)) ? PipelineStateChanges.RasterState : PipelineStateChanges.None;
            inheritedState |= ((cachedState.RwBlendStates.Equals(newState.RwBlendStates))
                                && (cachedState.IsAlphaToCoverageEnabled == newState.IsAlphaToCoverageEnabled)
                                && (cachedState.IsIndependentBlendingEnabled == newState.IsIndependentBlendingEnabled)) ? PipelineStateChanges.BlendState : PipelineStateChanges.None;
            inheritedState |= ((cachedState.DepthStencilState != null) 
                                && (cachedState.DepthStencilState.Equals(newState.DepthStencilState))) ? PipelineStateChanges.DepthStencilState : PipelineStateChanges.None;

            return inheritedState;
        }

        /// <summary>
        /// Function to initialize a <see cref="GorgonPipelineState" /> object with Direct 3D 11 state objects by creating new objects for the unassigned values.
        /// </summary>
        /// <param name="pipelineState">The pipeline state.</param>
        /// <param name="blendState">An existing blend state to use.</param>
        /// <param name="depthStencilState">An existing depth/stencil state to use.</param>
        /// <param name="rasterState">An existing rasterizer state to use.</param>
        /// <returns>A new <see cref="GorgonPipelineState"/>.</returns>
        private void InitializePipelineState(GorgonPipelineState pipelineState,
                                             D3D11.BlendState1 blendState,
                                             D3D11.DepthStencilState depthStencilState,
                                             D3D11.RasterizerState1 rasterState)
        {
            pipelineState.D3DRasterState = rasterState;
            pipelineState.D3DBlendState = blendState;
            pipelineState.D3DDepthStencilState = depthStencilState;

            if ((rasterState is null) && (pipelineState.RasterState != null))
            {
                pipelineState.D3DRasterState = pipelineState.RasterState.GetD3D11RasterState(_device);
            }

            if ((depthStencilState is null) && (pipelineState.DepthStencilState != null))
            {
                pipelineState.D3DDepthStencilState = pipelineState.DepthStencilState.GetD3D11DepthStencilState(_device);
            }

            if (blendState != null)
            {
                return;
            }

            pipelineState.D3DBlendState = pipelineState.RwBlendStates.BuildD3D11BlendState(_device, 
                                                                                            pipelineState.IsAlphaToCoverageEnabled, 
                                                                                            pipelineState.IsIndependentBlendingEnabled);
        }

        /// <summary>
        /// Function to cache a pipeline state, or return a previously cached pipeline state.
        /// </summary>
        /// <param name="newState">The state to cache.</param>
        /// <returns>The cached state.</returns>
        public GorgonPipelineState Cache(GorgonPipelineState newState)
        {
            // Existing states.
            D3D11.DepthStencilState depthStencilState = null;
            D3D11.BlendState1 blendState = null;
            D3D11.RasterizerState1 rasterState = null;

            lock (_stateLock)
            {
                if ((newState.ID >= 0) && (newState.ID < _cachedPipelineStates.Length) && (_cachedPipelineStates[newState.ID] != null))
                {
                    GorgonPipelineState state = _cachedPipelineStates[newState.ID];
                    depthStencilState = newState.D3DDepthStencilState;
                    rasterState = newState.D3DRasterState;
                    blendState = newState.D3DBlendState;

                    if ((depthStencilState == state.D3DDepthStencilState) && (depthStencilState != null)
                        && (rasterState == state.D3DRasterState)  && (rasterState != null)
                        && (blendState == state.D3DBlendState) && (blendState != null))
                    {
                        return state;
                    }

                    newState.ID = int.MinValue;
                }

                if (_cachedPipelineStates[_cachedPipelineStates.Length - 1] != null)
                {
                    Array.Resize(ref _cachedPipelineStates, _cachedPipelineStates.Length + InitialCacheSize);
                }

                int index = 0;

                while (index < _cachedPipelineStates.Length)
                {   
                    GorgonPipelineState cachedState = _cachedPipelineStates[index];

                    // We haven't taken this slot yet.
                    if (cachedState is null)
                    {
                        break;
                    }

                    PipelineStateChanges inheritedState = CompareState(cachedState, newState);
                    
                    if ((inheritedState & PipelineStateChanges.RasterState) == PipelineStateChanges.RasterState)
                    {
                        rasterState = cachedState.D3DRasterState;
                    }

                    if ((inheritedState & PipelineStateChanges.BlendState) == PipelineStateChanges.BlendState)
                    {
                        blendState = cachedState.D3DBlendState;
                    }

                    if ((inheritedState & PipelineStateChanges.DepthStencilState) == PipelineStateChanges.DepthStencilState)
                    {
                        depthStencilState = cachedState.D3DDepthStencilState;
                    }

                    // We've copied all the states, so just return the existing pipeline state.
                    // We exclude blend flags like BlendFactor and whatnot as they're handled elsewhere.
                    if (inheritedState == PipelineStateChanges.AllWithoutBlendFlags)
                    {
                        return cachedState;
                    }

                    ++index;
                }

                // Setup any uninitialized states.
                var resultState = new GorgonPipelineState(newState)
                {
                    ID = index
                };
                InitializePipelineState(resultState, blendState, depthStencilState, rasterState);                
                _cachedPipelineStates[index] = resultState;
                return resultState;
            }
        }

        /// <summary>
        /// Function to invalidate and clear the cache data.
        /// </summary>
        public void Clear()
        {
            lock (_stateLock)
            {
                InvalidateCache();
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => InvalidateCache();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="PipelineStateCache" /> class.</summary>
        /// <param name="device">The Direct3D device.</param>
        public PipelineStateCache(D3D11.Device5 device) => _device = device;
        #endregion
    }
}
