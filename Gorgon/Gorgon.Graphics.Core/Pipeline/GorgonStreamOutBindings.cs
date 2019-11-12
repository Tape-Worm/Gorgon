#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 25, 2017 9:28:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Collections;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A list of <see cref="GorgonStreamOutBinding"/> values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonStreamOutBinding"/> is used to bind a vertex buffer to the GPU pipeline so that it may be used for rendering.
    /// </para>
    /// </remarks>
    public sealed class GorgonStreamOutBindings
        : GorgonArray<GorgonStreamOutBinding>
    {
        #region Constants.
        /// <summary>
        /// The maximum number of vertex buffers allow to be bound at the same time.
        /// </summary>
        public const int MaximumStreamOutCount = 4;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the native items wrapped by this list.
        /// </summary>
        internal D3D11.StreamOutputBufferBinding[] Native
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when a dirty item is found and added.
        /// </summary>
        /// <param name="dirtyIndex">The index that is considered dirty.</param>
        /// <param name="value">The dirty value.</param>
        protected override void OnAssignDirtyItem(int dirtyIndex, GorgonStreamOutBinding value)
        {
            D3D11.StreamOutputBufferBinding binding = default;

            if (value.Buffer != null)
            {
                binding = new D3D11.StreamOutputBufferBinding(value.Buffer?.Native, value.Offset);
            }

            Native[dirtyIndex] = binding;
        }

        /// <summary>
        /// Function called when the array is cleared.
        /// </summary>
        protected override void OnClear() => Array.Clear(Native, 0, Native.Length);

        /// <summary>
        /// Function to find the index of a <see cref="GorgonStreamOutBinding"/> with the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to look up.</param>
        /// <returns>The index of the <see cref="GorgonStreamOutBinding"/>, or -1 if not found.</returns>
        /// <remarks>
        /// <para>
        /// For the sake of efficiency, this checks the dirty items in the list only.
        /// </para>
        /// </remarks>
	    internal int IndexOf(GorgonGraphicsResource buffer)
        {
            (int start, int count) = GetDirtyItems(true);

            for (int i = 0; i < count; ++i)
            {
                GorgonStreamOutBinding binding = BackingArray[i + start];

                if (binding.Buffer != buffer)
                {
                    continue;
                }

                return i + start;
            }

            return -1;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStreamOutBindings"/> class.
        /// </summary>
        /// <param name="bindings">[Optional] The list of bindings to copy.</param>
        public GorgonStreamOutBindings(IReadOnlyList<GorgonStreamOutBinding> bindings = null)
            : base(MaximumStreamOutCount)
        {
            Native = new D3D11.StreamOutputBufferBinding[MaximumStreamOutCount];

            if (bindings == null)
            {
                return;
            }

            for (int i = 0; i < bindings.Count.Min(Length); ++i)
            {
                this[i] = bindings[i];
            }
        }
        #endregion
    }
}
