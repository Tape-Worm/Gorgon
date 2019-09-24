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
// Created: May 29, 2018 8:57:29 AM
// 
#endregion

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines which face to apply a stencil operation to.
    /// </summary>
    public enum StencilFace
    {
        /// <summary>
        /// Apply stencil operation to back faces.
        /// </summary>
        Back = 0,
        /// <summary>
        /// Apply stencil operation to front faces.
        /// </summary>
        Front = 1
    }

    /// <summary>
    /// A builder for a <see cref="GorgonDepthStencilState"/> object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this builder to create a new immutable <see cref="GorgonDepthStencilState"/> to pass to a <see cref="GorgonPipelineState"/>. This object provides a fluent interface to help build up a 
    /// depth/stencil state.
    /// </para>
    /// <para>
    /// This will define how rasterized primitive data is clipped against a depth/stencil buffer. Depth reading, writing, and stencil operations are affected by this state.
    /// </para>
    /// <para>
    /// A depth/stencil state is an immutable object, and as such can only be created by using this object.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonDepthStencilState"/>
    public class GorgonDepthStencilStateBuilder
        : GorgonStateBuilderAllocator<GorgonDepthStencilStateBuilder, GorgonDepthStencilState>
    {
        #region Methods.
        /// <summary>
        /// Function to copy the state settings from the source state into the destination.
        /// </summary>
        /// <param name="dest">The destination state.</param>
        /// <param name="src">The state to copy.</param>
        private static void CopyState(GorgonDepthStencilState dest, GorgonDepthStencilState src)
        {
            dest.BackFaceStencilOp.Comparison = src.BackFaceStencilOp.Comparison;
            dest.BackFaceStencilOp.DepthFailOperation = src.BackFaceStencilOp.DepthFailOperation;
            dest.BackFaceStencilOp.FailOperation = src.BackFaceStencilOp.FailOperation;
            dest.BackFaceStencilOp.PassOperation = src.BackFaceStencilOp.PassOperation;
            dest.FrontFaceStencilOp.Comparison = src.FrontFaceStencilOp.Comparison;
            dest.FrontFaceStencilOp.DepthFailOperation = src.FrontFaceStencilOp.DepthFailOperation;
            dest.FrontFaceStencilOp.FailOperation = src.FrontFaceStencilOp.FailOperation;
            dest.FrontFaceStencilOp.PassOperation = src.FrontFaceStencilOp.PassOperation;
            dest.DepthComparison = src.DepthComparison;
            dest.IsDepthEnabled = src.IsDepthEnabled;
            dest.IsDepthWriteEnabled = src.IsDepthWriteEnabled;
            dest.IsStencilEnabled = src.IsStencilEnabled;
            dest.StencilReadMask = src.StencilReadMask;
            dest.StencilWriteMask = src.StencilWriteMask;
        }

        /// <summary>
        /// Function to update the properties of the state from the working copy to the final copy.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonDepthStencilState OnCreateState() => new GorgonDepthStencilState(WorkingState);

        /// <summary>Function to update the properties of the state, allocated from an allocator, from the working copy.</summary>
        /// <param name="state">The state to update.</param>
        protected override void OnUpdate(GorgonDepthStencilState state) => CopyState(WorkingState, state);

        /// <summary>
        /// Function to reset the builder to the specified state.
        /// </summary>
        /// <param name="state">The state to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonDepthStencilStateBuilder OnResetTo(GorgonDepthStencilState state)
        {
            CopyState(WorkingState, state);
            return this;
        }

        /// <summary>
        /// Function to clear the working state for the builder.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonDepthStencilStateBuilder OnClearState()
        {
            CopyState(WorkingState, GorgonDepthStencilState.Default);
            return this;
        }

        /// <summary>
        /// Function to set the comparison type for a stencil operation.
        /// </summary>
        /// <param name="face">The face direction for the operation.</param>
        /// <param name="comparison">The comparison type.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder StencilComparison(StencilFace face, Comparison comparison)
        {
            switch (face)
            {
                case StencilFace.Back:
                    WorkingState.BackFaceStencilOp.Comparison = comparison;
                    break;
                case StencilFace.Front:
                    WorkingState.FrontFaceStencilOp.Comparison = comparison;
                    break;
            }

            return this;
        }

        /// <summary>
        /// Function to set the operation(s) for the stencil <see cref="StencilComparison"/> result.
        /// </summary>
        /// <param name="face">The face direction for the operation.</param>
        /// <param name="passStencilOp">[Optional] The stencil operation if the comparison passes.</param>
        /// <param name="failStencilOp">[Optional] The stencil operation if the comparison fails.</param>
        /// <param name="depthFailOp">[Optional] The stencil operation if the depth comparison fails.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder StencilOperation(StencilFace face, StencilOperation passStencilOp = Core.StencilOperation.Keep, StencilOperation failStencilOp = Core.StencilOperation.Keep, StencilOperation depthFailOp = Core.StencilOperation.Keep)
        {
            switch (face)
            {
                case StencilFace.Back:
                    WorkingState.BackFaceStencilOp.PassOperation = passStencilOp;
                    WorkingState.BackFaceStencilOp.FailOperation = failStencilOp;
                    WorkingState.BackFaceStencilOp.DepthFailOperation = depthFailOp;
                    break;
                case StencilFace.Front:
                    WorkingState.FrontFaceStencilOp.PassOperation = passStencilOp;
                    WorkingState.FrontFaceStencilOp.FailOperation = failStencilOp;
                    WorkingState.FrontFaceStencilOp.DepthFailOperation = depthFailOp;
                    break;
            }

            return this;
        }

        /// <summary>
        /// Function to set the depth comparison.
        /// </summary>
        /// <param name="depthCompare">The comparison to type.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder DepthComparison(Comparison depthCompare)
        {
            WorkingState.DepthComparison = depthCompare;
            return this;
        }

        /// <summary>
        /// Function to enable depth testing.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder DepthEnabled()
        {
            WorkingState.IsDepthEnabled = true;
            return this;
        }

        /// <summary>
        /// Function to disable depth testing.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder DepthDisabled()
        {
            WorkingState.IsDepthEnabled = false;
            return this;
        }

        /// <summary>
        /// Function to enable depth writing.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder DepthWriteEnabled()
        {
            WorkingState.IsDepthWriteEnabled = true;
            return this;
        }

        /// <summary>
        /// Function to disable depth writing.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder DepthWriteDisabled()
        {
            WorkingState.IsDepthWriteEnabled = false;
            return this;
        }

        /// <summary>
        /// Function to enable stencil testing.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder StencilEnabled()
        {
            WorkingState.IsStencilEnabled = true;
            return this;
        }

        /// <summary>
        /// Function to disable stencil testing.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder StencilDisabled()
        {
            WorkingState.IsStencilEnabled = false;
            return this;
        }

        /// <summary>
        /// Function to set the read/write mask for the stencil test.
        /// </summary>
        /// <param name="read">[Optional] The read mask.</param>
        /// <param name="write">[Optional] The write mask.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonDepthStencilStateBuilder StencilMask(byte read = 0xff, byte write = 0xff)
        {
            WorkingState.StencilReadMask = read;
            WorkingState.StencilWriteMask = write;
            return this;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDepthStencilStateBuilder"/> class.
        /// </summary>
        public GorgonDepthStencilStateBuilder()
            : base(new GorgonDepthStencilState())
        {
        }
        #endregion
    }
}
