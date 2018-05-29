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
    /// A builder for a <see cref="GorgonBlendState"/> object.
    /// </summary>
    public class GorgonBlendStateBuilder
        : GorgonStateBuilderCommon<GorgonBlendStateBuilder, GorgonBlendState>
    {
        #region Methods.
        /// <summary>
        /// Function to copy the state settings from the source state into the destination.
        /// </summary>
        /// <param name="dest">The destination state.</param>
        /// <param name="src">The state to copy.</param>
        private static void CopyState(GorgonBlendState dest, GorgonBlendState src)
        {
            dest.AlphaBlendOperation = src.AlphaBlendOperation;
            dest.ColorBlendOperation = src.ColorBlendOperation;
            dest.DestinationAlphaBlend = src.DestinationAlphaBlend;
            dest.DestinationColorBlend = src.DestinationColorBlend;
            dest.IsBlendingEnabled = src.IsBlendingEnabled;
            dest.LogicOperation = src.LogicOperation;
            dest.SourceAlphaBlend = src.SourceAlphaBlend;
            dest.SourceColorBlend = src.SourceColorBlend;
            dest.WriteMask = src.WriteMask;
        }

        /// <summary>
        /// Function to update the properties of the state from the working copy to the final copy.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonBlendState OnUpdate()
        {
            return new GorgonBlendState(WorkingState);
        }

        /// <summary>
        /// Function to reset the builder to the specified state.
        /// </summary>
        /// <param name="state">The state to copy from.</param>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonBlendStateBuilder OnResetState(GorgonBlendState state)
        {
            CopyState(WorkingState, state);
            return this;
        }

        /// <summary>
        /// Function to clear the working state for the builder.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        protected override GorgonBlendStateBuilder OnClearState()
        {
            CopyState(WorkingState, GorgonBlendState.Default);
            return this;
        }

        /// <summary>
        /// Function to enable blending.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonBlendStateBuilder EnableBlending()
        {
            WorkingState.IsBlendingEnabled = true;
            return this;
        }

        /// <summary>
        /// Function to disable blending.
        /// </summary>
        /// <returns>The fluent builder interface.</returns>
        public GorgonBlendStateBuilder DisableBlending()
        {
            WorkingState.IsBlendingEnabled = false;
            return this;
        }

        /// <summary>
        /// Function to set the logical blending operation.
        /// </summary>
        /// <param name="logicOperation">The operation to perform.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonBlendStateBuilder LogicOperation(LogicOperation logicOperation)
        {
            WorkingState.LogicOperation = logicOperation;
            return this;
        }

        /// <summary>
        /// Function to set the write mask.
        /// </summary>
        /// <param name="writeMask">The write mask to apply.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonBlendStateBuilder WriteMask(WriteMask writeMask)
        {
            WorkingState.WriteMask = writeMask;
            return this;
        }

        /// <summary>
        /// Function to set the color, and alpha blend operations.
        /// </summary>
        /// <param name="color">[Optional] The color blend operation to assign.</param>
        /// <param name="alpha">[Optional] The alpha blend operation to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonBlendStateBuilder BlendOperation(BlendOperation color = Core.BlendOperation.Add, BlendOperation alpha = Core.BlendOperation.Add)
        {
            WorkingState.ColorBlendOperation = color;
            WorkingState.AlphaBlendOperation = alpha;
            return this;
        }

        /// <summary>
        /// Function to set the color, and alpha source blend operation.
        /// </summary>
        /// <param name="color">[Optional] The color blend operation to assign.</param>
        /// <param name="alpha">[Optional] The alpha blend operation to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonBlendStateBuilder SourceBlend(Blend color = Blend.SourceAlpha, Blend alpha = Blend.One)
        {
            WorkingState.SourceColorBlend = color;
            WorkingState.SourceAlphaBlend = alpha;
            return this;
        }

        /// <summary>
        /// Function to set the color, and alpha destination blend operation.
        /// </summary>
        /// <param name="color">[Optional] The color blend operation to assign.</param>
        /// <param name="alpha">[Optional] The alpha blend operation to assign.</param>
        /// <returns>The fluent builder interface.</returns>
        public GorgonBlendStateBuilder DestinationBlend(Blend color = Blend.InverseSourceAlpha, Blend alpha = Blend.Zero)
        {
            WorkingState.DestinationColorBlend = color;
            WorkingState.DestinationAlphaBlend = alpha;
            return this;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBlendStateBuilder"/> class.
        /// </summary>
        public GorgonBlendStateBuilder()
            : base(new GorgonBlendState())
        {
        }
        #endregion
    }
}
