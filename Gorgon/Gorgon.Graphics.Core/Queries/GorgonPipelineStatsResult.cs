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
// Created: January 16, 2021 9:12:53 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Statistics for the pipeline from a query.
    /// </summary>
    public readonly struct GorgonPipelineStatsResult
    {
        /// <summary>
        /// Number of vertices read by input assembler.
        /// </summary>
        public readonly long InputAssemblerVertexCount;
        /// <summary>
        /// Number of primitives read by the input assembler. This number can be different depending on the primitive topology used. For example, a triangle strip with 6 vertices will produce 4 
        /// triangles, however a triangle list with 6 vertices will produce 2 triangles.
        /// </summary>
        public readonly long InputAssemblerPrimitiveCount;
        /// <summary>
        /// Number of times a vertex shader was invoked. 
        /// </summary>
        public readonly long VertexShaderInvocations;
        /// <summary>
        /// Number of times a geometry shader was invoked. When the geometry shader is set to <b>null</b>, this statistic may or may not increment depending on the hardware manufacturer.
        /// </summary>
        public readonly long GeometryShaderInvocations;
        /// <summary>
        /// Number of primitives output by a geometry shader.
        /// </summary>
        public readonly long GeometryShaderPrimitiveCount;
        /// <summary>
        /// Number of primitives that were sent to the rasterizer. When the rasterizer is disabled, this will not increment.
        /// </summary>
        public readonly long PrimitivesRasterized;
        /// <summary>
        /// Number of primitives that were rendered. This may be larger or smaller than <see cref="PrimitivesRasterized"/> because after a primitive is clipped sometimes it is either broken up into 
        /// more than one primitive or completely culled.
        /// </summary>
        public readonly long PrimitivesRendered;
        /// <summary>
        /// Number of times a pixel shader was invoked.
        /// </summary>
        public readonly long PixelShaderInvocations;
        /// <summary>
        /// Number of times a hull shader was invoked.
        /// </summary>
        public readonly long HullShaderInvocations;
        /// <summary>
        /// Number of times a domain shader was invoked.
        /// </summary>
        public readonly long DomainShaderInvocations;
        /// <summary>
        /// Number of times a compute shader was invoked.
        /// </summary>
        public readonly long ComputeShaderInvocations;

        /// <summary>Initializes a new instance of the <see cref="GorgonPipelineStatsResult" /> struct.</summary>
        /// <param name="stats">The stats.</param>
        internal GorgonPipelineStatsResult(ref D3D11.QueryDataPipelineStatistics stats)
        {
            InputAssemblerVertexCount = stats.IAVerticeCount;
            InputAssemblerPrimitiveCount = stats.IAPrimitiveCount;
            VertexShaderInvocations = stats.VSInvocationCount;
            GeometryShaderInvocations = stats.GSInvocationCount;
            GeometryShaderPrimitiveCount = stats.GSPrimitiveCount;
            PrimitivesRasterized = stats.CInvocationCount;
            PrimitivesRendered = stats.CPrimitiveCount;
            PixelShaderInvocations = stats.PSInvocationCount;
            HullShaderInvocations = stats.HSInvocationCount;
            DomainShaderInvocations = stats.DSInvocationCount;
            ComputeShaderInvocations = stats.CSInvocationCount;
        }
    }
}
