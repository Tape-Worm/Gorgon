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
// Created: July 4, 2016 9:16:56 PM
// 
#endregion

using System.Diagnostics;
using Gorgon.Collections;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A pipeline state object used to set up the complete graphics pipeline for Gorgon.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A pipeline state object is used to define the state of the pipeline prior to drawing anything. It can be used to assign shaders, and various other states that will affect how data is rasterized on 
    /// the GPU.
    /// </para>
    /// <para>
    /// The pipeline state object is immutable, and as such cannot be changed directly. To create a new pipeline state object, a <see cref="GorgonPipelineStateBuilder"/> must be used.
    /// </para>
    /// <para>
    /// Pipeline states are assigned to a draw call via one of the draw call builders.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPipelineStateBuilder"/>
    /// <seealso cref="GorgonDrawCall"/>
    /// <seealso cref="GorgonDrawIndexCall"/>
    /// <seealso cref="GorgonInstancedCall"/>
    /// <seealso cref="GorgonInstancedIndexCall"/>
    /// <seealso cref="GorgonStreamOutCall"/>
    public class GorgonPipelineState
    {
        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D 11 raster state.
        /// </summary>
        internal D3D11.RasterizerState1 D3DRasterState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the Direct 3D 11 depth/stencil state.
        /// </summary>
        internal D3D11.DepthStencilState D3DDepthStencilState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the Direct 3D 11 blend state.
        /// </summary>
        internal D3D11.BlendState1 D3DBlendState
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the readable/writable list of blending states for each render target.
        /// </summary>
	    internal GorgonArray<GorgonBlendState> RwBlendStates
        {
            get;
        } = new GorgonArray<GorgonBlendState>(D3D11.OutputMergerStage.SimultaneousRenderTargetCount);

        /// <summary>
        /// Property to return the ID of the pipeline state.
        /// </summary>
        /// <remarks>
        /// This is used to store a globally cached version of the pipeline state.
        /// </remarks>
        public int ID
        {
            get;
            internal set;
        } = int.MinValue;

        /// <summary>
        /// Property to return the pixel shader.
        /// </summary>
	    public GorgonPixelShader PixelShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the vertex shader.
        /// </summary>
        public GorgonVertexShader VertexShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the geometry shader.
        /// </summary>
        public GorgonGeometryShader GeometryShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the domain shader.
        /// </summary>
        public GorgonDomainShader DomainShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the hull shader.
        /// </summary>
        public GorgonHullShader HullShader
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the rasterizer state for the pipeline.
        /// </summary>
        public GorgonRasterState RasterState
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the depth/stencil state for the pipeline.
        /// </summary>
        public GorgonDepthStencilState DepthStencilState
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the topology for a primitive.
        /// </summary>
        public PrimitiveType PrimitiveType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return whether alpha to coverage is enabled or not for blending.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will use alpha to coverage as a multisampling technique when writing a pixel to a render target. Alpha to coverage is useful in situations where there are multiple overlapping polygons 
        /// that use transparency to define edges.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IsAlphaToCoverageEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return whether independent render target blending is enabled or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will specify whether to use different blending states for each render target. When this value is set to <b>true</b>, each render target blend state will be independent of other render 
        /// target blend states. When this value is set to <b>false</b>, then only the blend state of the first render target is used.
        /// </para>
        /// <para>
        /// The default value is <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IsIndependentBlendingEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the list of blending states for each render target.
        /// </summary>
        public IGorgonReadOnlyArray<GorgonBlendState> BlendStates => RwBlendStates;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build the D3D11 blend state.
        /// </summary>
        /// <param name="device">The device used to create the blend state.</param>
	    internal void BuildD3D11BlendState(D3D11.Device5 device)
        {
            Debug.Assert(D3DBlendState == null, "D3D Blend state already assigned to this pipeline state.");

            (int start, int count) = RwBlendStates.GetDirtyItems();
            var desc = new D3D11.BlendStateDescription1
            {
                AlphaToCoverageEnable = IsAlphaToCoverageEnabled,
                IndependentBlendEnable = IsIndependentBlendingEnabled
            };

            for (int i = 0; i < count; ++i)
            {
                GorgonBlendState state = RwBlendStates[start + i];

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

            for (int i = count; i < D3D11.OutputMergerStage.SimultaneousRenderTargetCount; ++i)
            {
                desc.RenderTarget[i] = new D3D11.RenderTargetBlendDescription1
                {
                    AlphaBlendOperation = D3D11.BlendOperation.Add,
                    BlendOperation = D3D11.BlendOperation.Add,
                    IsLogicOperationEnabled = false,
                    IsBlendEnabled = false,
                    RenderTargetWriteMask = D3D11.ColorWriteMaskFlags.All,
                    LogicOperation = D3D11.LogicOperation.Noop,
                    SourceAlphaBlend = D3D11.BlendOption.One,
                    SourceBlend = D3D11.BlendOption.One,
                    DestinationAlphaBlend = D3D11.BlendOption.Zero,
                    DestinationBlend = D3D11.BlendOption.Zero
                };
            }

            D3DBlendState = new D3D11.BlendState1(device, desc)
            {
                DebugName = nameof(GorgonBlendState)
            };
        }

        /// <summary>
        /// Function to copy the contents of a pipeline state to another pipeline state.
        /// </summary>
        /// <param name="pipelineState">The pipeline state to copy.</param>
        internal void CopyTo(GorgonPipelineState pipelineState)
        {
            if (RasterState != null)
            {
                if (pipelineState.RasterState == null)
                {
                    pipelineState.RasterState = new GorgonRasterState();
                }

                pipelineState.RasterState.IsAntialiasedLineEnabled = RasterState.IsAntialiasedLineEnabled;
                pipelineState.RasterState.CullMode = RasterState.CullMode;
                pipelineState.RasterState.DepthBias = RasterState.DepthBias;
                pipelineState.RasterState.DepthBiasClamp = RasterState.DepthBiasClamp;
                pipelineState.RasterState.IsDepthClippingEnabled = RasterState.IsDepthClippingEnabled;
                pipelineState.RasterState.FillMode = RasterState.FillMode;
                pipelineState.RasterState.ForcedReadWriteViewSampleCount = RasterState.ForcedReadWriteViewSampleCount;
                pipelineState.RasterState.IsFrontCounterClockwise = RasterState.IsFrontCounterClockwise;
                pipelineState.RasterState.IsMultisamplingEnabled = RasterState.IsMultisamplingEnabled;
                pipelineState.RasterState.ScissorRectsEnabled = RasterState.ScissorRectsEnabled;
                pipelineState.RasterState.SlopeScaledDepthBias = RasterState.SlopeScaledDepthBias;
                pipelineState.RasterState.UseConservativeRasterization = RasterState.UseConservativeRasterization;
            }

            if (DepthStencilState != null)
            {
                if (pipelineState.DepthStencilState == null)
                {
                    pipelineState.DepthStencilState = new GorgonDepthStencilState();
                }

                pipelineState.DepthStencilState.BackFaceStencilOp.Comparison = DepthStencilState.BackFaceStencilOp.Comparison;
                pipelineState.DepthStencilState.BackFaceStencilOp.DepthFailOperation = DepthStencilState.BackFaceStencilOp.DepthFailOperation;
                pipelineState.DepthStencilState.BackFaceStencilOp.FailOperation = DepthStencilState.BackFaceStencilOp.FailOperation;
                pipelineState.DepthStencilState.BackFaceStencilOp.PassOperation = DepthStencilState.BackFaceStencilOp.PassOperation;

                pipelineState.DepthStencilState.FrontFaceStencilOp.Comparison = DepthStencilState.FrontFaceStencilOp.Comparison;
                pipelineState.DepthStencilState.FrontFaceStencilOp.DepthFailOperation = DepthStencilState.FrontFaceStencilOp.DepthFailOperation;
                pipelineState.DepthStencilState.FrontFaceStencilOp.FailOperation = DepthStencilState.FrontFaceStencilOp.FailOperation;
                pipelineState.DepthStencilState.FrontFaceStencilOp.PassOperation = DepthStencilState.FrontFaceStencilOp.PassOperation;

                pipelineState.DepthStencilState.DepthComparison = DepthStencilState.DepthComparison;
                pipelineState.DepthStencilState.IsDepthWriteEnabled = DepthStencilState.IsDepthWriteEnabled;
                pipelineState.DepthStencilState.IsDepthEnabled = DepthStencilState.IsDepthEnabled;
                pipelineState.DepthStencilState.IsStencilEnabled = DepthStencilState.IsStencilEnabled;
                pipelineState.DepthStencilState.StencilReadMask = DepthStencilState.StencilReadMask;
                pipelineState.DepthStencilState.StencilWriteMask = DepthStencilState.StencilWriteMask;
            }

            pipelineState.IsIndependentBlendingEnabled = IsIndependentBlendingEnabled;
            pipelineState.IsAlphaToCoverageEnabled = IsAlphaToCoverageEnabled;
            pipelineState.PixelShader = PixelShader;
            pipelineState.VertexShader = VertexShader;
            pipelineState.GeometryShader = GeometryShader;
            pipelineState.DomainShader = DomainShader;
            pipelineState.HullShader = HullShader;
            pipelineState.PrimitiveType = PrimitiveType;
            RwBlendStates.CopyTo(pipelineState.RwBlendStates);
        }

        /// <summary>
        /// Function to clear the pipeline state.
        /// </summary>
	    internal void Clear()
        {
            D3DBlendState = null;
            D3DDepthStencilState = null;
            D3DRasterState = null;

            RasterState = null;
            DepthStencilState = null;
            IsIndependentBlendingEnabled = false;
            IsAlphaToCoverageEnabled = false;
            PixelShader = null;
            VertexShader = null;
            GeometryShader = null;
            DomainShader = null;
            HullShader = null;
            PrimitiveType = PrimitiveType.None;
            RwBlendStates.Clear();
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPipelineState"/> class.
        /// </summary>
        /// <param name="state">The state copy.</param>
        internal GorgonPipelineState(GorgonPipelineState state)
        {
            RasterState = state.RasterState;
            DepthStencilState = state.DepthStencilState;
            IsIndependentBlendingEnabled = state.IsIndependentBlendingEnabled;
            IsAlphaToCoverageEnabled = state.IsAlphaToCoverageEnabled;
            PixelShader = state.PixelShader;
            VertexShader = state.VertexShader;
            GeometryShader = state.GeometryShader;
            DomainShader = state.DomainShader;
            HullShader = state.HullShader;
            PrimitiveType = state.PrimitiveType;
            state.RwBlendStates.CopyTo(RwBlendStates);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPipelineState" /> class.
        /// </summary>
        internal GorgonPipelineState()
        {
        }
        #endregion
    }
}
