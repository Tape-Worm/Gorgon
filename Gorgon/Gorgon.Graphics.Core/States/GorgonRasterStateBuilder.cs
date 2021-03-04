#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 24, 2018 4:10:30 PM
// 
#endregion


namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A builder for a <see cref="GorgonRasterState"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this builder to create a new immutable <see cref="GorgonRasterState"/> to pass to a <see cref="GorgonPipelineState"/>. This object provides a fluent interface to help build up a 
    /// raster state.
    /// </para>
    /// <para>
    /// This will define how a triangle, line, point, etc... is rasterized by the GPU when rendering. Clipping, vertex ordering, culling, etc... are all affected by this state.
    /// </para>
    /// <para>
    /// A raster state is an immutable object, and as such can only be created by using this object.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonRasterState"/>
    public class GorgonRasterStateBuilder
        : GorgonStateBuilderAllocator<GorgonRasterStateBuilder, GorgonRasterState>
    {
        #region Methods.
        /// <summary>
        /// Function to copy the state settings from the source state into the destination.
        /// </summary>
        /// <param name="dest">The destination state.</param>
        /// <param name="src">The state to copy.</param>
        private static void CopyState(GorgonRasterState dest, GorgonRasterState src)
        {
            dest.IsAntialiasedLineEnabled = src.IsAntialiasedLineEnabled;
            dest.CullMode = src.CullMode;
            dest.DepthBias = src.DepthBias;
            dest.DepthBiasClamp = src.DepthBiasClamp;
            dest.IsDepthClippingEnabled = src.IsDepthClippingEnabled;
            dest.FillMode = src.FillMode;
            dest.ForcedReadWriteViewSampleCount = src.ForcedReadWriteViewSampleCount;
            dest.IsFrontCounterClockwise = src.IsFrontCounterClockwise;
            dest.IsMultisamplingEnabled = src.IsMultisamplingEnabled;
            dest.ScissorRectsEnabled = src.ScissorRectsEnabled;
            dest.SlopeScaledDepthBias = src.SlopeScaledDepthBias;
            dest.UseConservativeRasterization = src.UseConservativeRasterization;
        }

        /// <summary>
        /// Function to update the properties of the state from the working copy to the final copy.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonRasterState OnCreateState() => new(WorkingState);
        
        /// <summary>Function to update the properties of the state, allocated from an allocator, from the working copy.</summary>
        /// <param name="state">The state to update.</param>
        protected override void OnUpdate(GorgonRasterState state) => CopyState(WorkingState, state);

        /// <summary>
        /// Function to reset the builder to the specified state.
        /// </summary>
        /// <param name="state">The state to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonRasterStateBuilder OnResetTo(GorgonRasterState state)
        {
            CopyState(WorkingState, state);
            return this;
        }

        /// <summary>
        /// Function to clear the working state for the builder.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonRasterStateBuilder OnClearState()
        {
            CopyState(WorkingState, GorgonRasterState.Default);
            return this;
        }

        /// <summary>
        /// Function to enable scissor cipping rectangles.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        /// <remarks>
        /// <para>
        /// This must be called if the <see cref="GorgonGraphics.SetScissorRect"/>, or <see cref="GorgonGraphics.SetScissorRects"/> methods are to be used. 
        /// </para>
        /// <para>
        /// <note type="warning">
        /// <para>
        /// If no scissor rectangle is set on the <see cref="GorgonGraphics"/> interface, and this is set to true, nothing will be rendered.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonGraphics"/>
        public GorgonRasterStateBuilder EnableScissorRects()
        {
            WorkingState.ScissorRectsEnabled = true;
            return this;
        }

        /// <summary>
        /// Function to disable scissor cipping rectangles.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        /// <remarks>
        /// <para>
        /// This will disable scissor rectangle clipping entirely. Any rectangles assigned via the <see cref="GorgonGraphics.SetScissorRect"/>, or 
        /// <see cref="GorgonGraphics.SetScissorRects"/> will remain assigned.
        /// </para>
        /// </remarks>
        public GorgonRasterStateBuilder DisableScissorRects()
        {
            WorkingState.ScissorRectsEnabled = false;
            return this;
        }

        /// <summary>
        /// Function to turn on conservative rasterization.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonRasterStateBuilder EnableConservativeRasterization()
        {
            WorkingState.UseConservativeRasterization = true;
            return this;
        }


        /// <summary>
        /// Function to turn off conservative rasterization.
        /// </summary>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonRasterStateBuilder DisableConservativeRasterization()
        {
            WorkingState.UseConservativeRasterization = false;
            return this;
        }

        /// <summary>
        /// Function to set the culling mode.
        /// </summary>
        /// <param name="cullMode">The current culling mode.</param>
        /// <param name="isFrontCounterClockwise">[Optional] <b>true</b> if vertices that are ordered counter-clockwise are considered front facing, <b>false</b> if not.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonRasterStateBuilder CullMode(CullingMode cullMode, bool? isFrontCounterClockwise = null)
        {
            WorkingState.CullMode = cullMode;
            if (isFrontCounterClockwise is not null)
            {
                WorkingState.IsFrontCounterClockwise = isFrontCounterClockwise.Value;
            }

            return this;
        }

        /// <summary>
        /// Function to set the primitive fill mode.
        /// </summary>
        /// <param name="fillMode">The current primitive fil mode.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonRasterStateBuilder FillMode(FillMode fillMode)
        {
            WorkingState.FillMode = fillMode;
            return this;
        }

        /// <summary>
        /// Function to set the depth bias parameters.
        /// </summary>
        /// <param name="depthBias">The depth bias.</param>
        /// <param name="depthBiasClamp">The depth bias clamping value.</param>
        /// <param name="slopeScaledDepthBias">The slope scaled depth bias value.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonRasterStateBuilder DepthBias(int depthBias, float depthBiasClamp, float slopeScaledDepthBias)
        {
            WorkingState.DepthBias = depthBias;
            WorkingState.DepthBiasClamp = depthBiasClamp;
            WorkingState.SlopeScaledDepthBias = slopeScaledDepthBias;
            return this;
        }

        /// <summary>
        /// Function to set the forced unordered access view count.
        /// </summary>
        /// <param name="sampleCount">The sample count to set.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonRasterStateBuilder ForcedReadWriteViewSampleCount(int sampleCount)
        {
            WorkingState.ForcedReadWriteViewSampleCount = sampleCount;
            return this;
        }

        /// <summary>
        /// Function to set the antialiasing flags for MSAA and line anti aliasing.
        /// </summary>
        /// <param name="msaa"><b>true</b> to enable multisampling, <b>false</b> to disable.</param>
        /// <param name="lineAntiAlias">[Optional] <b>true</b> to enable line antialiasing, <b>false</b> to disable.</param>
        /// <returns>The fluent interface for this builder.</returns>
        public GorgonRasterStateBuilder Antialiasing(bool msaa, bool? lineAntiAlias = null)
        {
            WorkingState.IsMultisamplingEnabled = msaa;
            if (lineAntiAlias is not null)
            {
                WorkingState.IsAntialiasedLineEnabled = lineAntiAlias.Value;
            }
            return this;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRasterStateBuilder"/> class.
        /// </summary>
        public GorgonRasterStateBuilder()
            : base(new GorgonRasterState())
        {
        }
        #endregion
    }
}
