using Gorgon.Core.Memory;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// An allocator for creating pipeline state information objects.
    /// </summary>
    public class GorgonPipelineStateInfoAllocator
        : GorgonLinearAllocatorPool<GorgonPipelineStateInfo>
    {
        #region Methods.
        /// <summary>
        /// Function to allocate a new <see cref="GorgonPipelineStateInfo"/>.
        /// </summary>
        /// <param name="existingState">[Optional] The existing state to copy into the new pipeline state info.</param>
        /// <returns>A new <see cref="GorgonPipelineStateInfo"/>.</returns>
        public GorgonPipelineStateInfo AllocatePipelineStateInfo(IGorgonPipelineStateInfo existingState = null)
        {
            int index = Allocate();

            if (index == -1)
            {
                Reset();
                index = Allocate();
            }

            GorgonPipelineStateInfo result = Items[index];

            if (result == null)
            {
                Items[index] = result = new GorgonPipelineStateInfo
                                        {
                                            RasterState = new GorgonRasterState(),
                                            RenderTargetBlendState = new []
                                                                     {
                                                                         new GorgonRenderTargetBlendStateInfo()
                                                                     },
                                            DepthStencilState = new GorgonDepthStencilStateInfo()
                                        };
            }

            if (existingState == null)
            {
                return result;
            }

            result.IsAlphaToCoverageEnabled = existingState.IsAlphaToCoverageEnabled;
            result.IsIndependentBlendingEnabled = existingState.IsIndependentBlendingEnabled;
            result.VertexShader = existingState.VertexShader;
            result.PixelShader = existingState.PixelShader;
            result.RasterState = existingState.RasterState;

            if (existingState.DepthStencilState == null)
            {
                result.DepthStencilState = null;
            }
            else
            {
                if (result.DepthStencilState == null)
                {
                    result.DepthStencilState = new GorgonDepthStencilStateInfo(existingState.DepthStencilState);
                }
                else
                {
                    result.DepthStencilState.CopyFrom(existingState.DepthStencilState);
                }
            }

            if (existingState.RenderTargetBlendState == null)
            {
                result.RenderTargetBlendState = null;
                return result;
            }

            if ((result.RenderTargetBlendState == null) || (result.RenderTargetBlendState.Length != existingState.RenderTargetBlendState.Count))
            {
                result.RenderTargetBlendState = new GorgonRenderTargetBlendStateInfo[existingState.RenderTargetBlendState.Count];
            }

            for (int i = 0; i < result.RenderTargetBlendState.Length; ++i)
            {
                IGorgonRenderTargetBlendStateInfo existingBlend = existingState.RenderTargetBlendState[i];

                if (existingBlend == null)
                {
                    result.RenderTargetBlendState[i] = null;
                    continue;
                }
                    
                GorgonRenderTargetBlendStateInfo blendState = result.RenderTargetBlendState[i];

                if (blendState == null)
                {
                    result.RenderTargetBlendState[i] = new GorgonRenderTargetBlendStateInfo(existingBlend);
                    continue;
                }

                blendState.CopyFrom(existingBlend);
            }

            return result;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPipelineStateInfoAllocator"/> class.
        /// </summary>
        public GorgonPipelineStateInfoAllocator()
            : base(4096)
        {
            
        }
        #endregion
    }
}
