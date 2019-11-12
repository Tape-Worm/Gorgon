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
// Created: July 9, 2016 3:47:30 PM
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
    /// A list of constant buffers used for shaders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is used to pass a set of <see cref="GorgonConstantBuffer"/> objects to a <see cref="GorgonDrawCallCommon"/> object. This allows an application to set a single or multiple constant buffers at 
    /// the same time and thus provides a performance boost over setting them individually. 
    /// </para>
    /// </remarks>
    public sealed class GorgonConstantBuffers
        : GorgonArray<GorgonConstantBufferView>
    {
        #region Constants.
        /// <summary>
        /// The maximum size for a constant buffer binding list.
        /// </summary>
        public const int MaximumConstantBufferCount = D3D11.CommonShaderStage.ConstantBufferApiSlotCount;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the native buffers.
        /// </summary>
        internal D3D11.Buffer[] Native
        {
            get;
        } = new D3D11.Buffer[MaximumConstantBufferCount];

        /// <summary>
        /// Property to return the start of the view.
        /// </summary>
	    internal int[] ViewStart
        {
            get;
        } = new int[MaximumConstantBufferCount];

        /// <summary>
        /// Property to return the number of elements in the view.
        /// </summary>
        internal int[] ViewCount
        {
            get;
        } = new int[MaximumConstantBufferCount];
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when a dirty item is found and added.
        /// </summary>
        /// <param name="dirtyIndex">The index that is considered dirty.</param>
        /// <param name="value">The dirty value.</param>
        protected override void OnAssignDirtyItem(int dirtyIndex, GorgonConstantBufferView value)
        {
            Native[dirtyIndex] = value?.Buffer.Native;
            ViewStart[dirtyIndex] = value?.StartElement * 16 ?? 0;
            ViewCount[dirtyIndex] = value?.ElementCount * 16 ?? 0;
        }

        /// <summary>
        /// Function called when the array is cleared.
        /// </summary>
        protected override void OnClear()
        {
            Array.Clear(Native, 0, Native.Length);
            Array.Clear(ViewStart, 0, ViewStart.Length);
            Array.Clear(ViewCount, 0, ViewCount.Length);
        }

        /// <summary>
        /// Function to find the index of the resource in the array.
        /// </summary>
        /// <param name="resource">The resource to look up.</param>
        /// <returns>The index, if found. -1 if not.</returns>
	    internal int IndexOf(GorgonGraphicsResource resource)
        {
            (int start, int count) = GetDirtyItems(true);

            for (int i = 0; i < count; ++i)
            {
                GorgonConstantBuffer buffer = BackingArray[i + start]?.Buffer;

                if (buffer == resource)
                {
                    return i + start;
                }
            }

            return -1;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonConstantBuffers"/> class.
        /// </summary>
        /// <param name="bufferViews">[Optional] A list of buffer views to copy into the the list.</param>
        public GorgonConstantBuffers(IReadOnlyList<GorgonConstantBufferView> bufferViews = null)
            : base(MaximumConstantBufferCount)
        {
            if (bufferViews == null)
            {
                return;
            }

            for (int i = 0; i < bufferViews.Count.Min(Length); ++i)
            {
                this[i] = bufferViews[i];
            }
        }
        #endregion
    }
}
