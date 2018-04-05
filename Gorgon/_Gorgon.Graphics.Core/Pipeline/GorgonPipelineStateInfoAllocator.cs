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
                                            BlendStates = new []
                                                                     {
                                                                         new GorgonBlendState()
                                                                     },
                                            DepthStencilState = new GorgonDepthStencilState()
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
            result.DepthStencilState = existingState.DepthStencilState;
            
            if (existingState.BlendStates == null)
            {
                result.BlendStates = null;
                return result;
            }

            if ((result.BlendStates == null) || (result.BlendStates.Length != existingState.BlendStates.Count))
            {
                result.BlendStates = new GorgonBlendState[existingState.BlendStates.Count];
            }

            for (int i = 0; i < result.BlendStates.Length; ++i)
            {
                GorgonBlendState existingBlend = existingState.BlendStates[i];

                if (existingBlend == null)
                {
                    result.BlendStates[i] = null;
                    continue;
                }
                    
                result.BlendStates[i] = existingBlend;
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
