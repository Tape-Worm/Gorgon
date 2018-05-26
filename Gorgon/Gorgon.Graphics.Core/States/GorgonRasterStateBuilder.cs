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
    public class GorgonRasterStateBuilder
        : GorgonStateBuilderCommon<GorgonRasterStateBuilder, GorgonRasterState>
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
            dest.ForcedUavSampleCount = src.ForcedUavSampleCount;
            dest.IsFrontCounterClockwise = src.IsFrontCounterClockwise;
            dest.IsMultisamplingEnabled = src.IsMultisamplingEnabled;
            dest.IsScissorClippingEnabled = src.IsScissorClippingEnabled;
            dest.SlopeScaledDepthBias = src.SlopeScaledDepthBias;
            dest.UseConservativeRasterization = src.UseConservativeRasterization;
        }

        /// <summary>
        /// Function to update the properties of the state from the working copy to the final copy.
        /// </summary>
        protected override GorgonRasterState OnUpdate()
        {
            return new GorgonRasterState(WorkingState);
        }

        /// <summary>
        /// Function to reset the builder to the specified state.
        /// </summary>
        /// <param name="state">The state to copy from.</param>
        protected override GorgonRasterStateBuilder OnResetState(GorgonRasterState state)
        {
            CopyState(WorkingState, state);
            return this;
        }

        /// <summary>
        /// Function to clear the working state for the builder.
        /// </summary>
        protected override GorgonRasterStateBuilder OnClearState()
        {
            CopyState(WorkingState, GorgonRasterState.Default);
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
            if (isFrontCounterClockwise != null)
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
        public GorgonRasterStateBuilder ForcedUavSampleCount(int sampleCount)
        {
            WorkingState.ForcedUavSampleCount = sampleCount;
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
            if (lineAntiAlias != null)
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
