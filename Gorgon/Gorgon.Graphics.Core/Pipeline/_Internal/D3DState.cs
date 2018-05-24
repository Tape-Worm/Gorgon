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
// Created: May 23, 2018 10:33:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D = SharpDX.Direct3D;
using Gorgon.Collections;


namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines what has been changed on a draw call.
    /// </summary>
    [Flags]
    enum DrawCallChanges
        : ulong
    {
        /// <summary>
        /// No changes.
        /// </summary>
        None = 0,
        /// <summary>
        /// The list of vertex buffers used by a draw call.
        /// </summary>
        VertexBuffers = 0x1,
        /// <summary>
        /// The input layout for the vertex buffers.
        /// </summary>
        InputLayout = 0x2,
        /// <summary>
        /// The index buffer used by a draw call.
        /// </summary>
        IndexBuffer = 0x4,
        /// <summary>
        /// The primtive topology.
        /// </summary>
        Topology = 0x8,
        /// <summary>
        /// Everything changed.
        /// </summary>
        All = VertexBuffers | InputLayout | IndexBuffer | Topology
    }

    /// <summary>
    /// Defines a means to record and compare state.
    /// </summary>
    class D3DState
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the current list of vertex buffers.
        /// </summary>
        public GorgonVertexBufferBindings VertexBuffers
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the index buffer.
        /// </summary>
        public GorgonIndexBuffer IndexBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the current primitive topology.
        /// </summary>
        public D3D.PrimitiveTopology Topology
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the current input layout.
        /// </summary>
        public GorgonInputLayout InputLayout => VertexBuffers?.InputLayout;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine what the difference is between two sets of state.
        /// </summary>
        /// <param name="state">The state to compare.</param>
        /// <returns>A <see cref="DrawCallChanges"/> containing the states that have been changed</returns>
        public DrawCallChanges GetDifference(D3DState state)
        {
            if (state == null)
            {
                return DrawCallChanges.All;
            }

            DrawCallChanges changes = DrawCallChanges.None;

            if (Topology != state.Topology)
            {
                changes |= DrawCallChanges.Topology;
            }

            if (VertexBuffers != state.VertexBuffers)
            {
                changes |= DrawCallChanges.VertexBuffers;
            }

            if (InputLayout != state.InputLayout)
            {
                changes |= DrawCallChanges.InputLayout;
            }

            if (IndexBuffer != state.IndexBuffer)
            {
                changes |= DrawCallChanges.IndexBuffer;
            }

            return changes;
        }
        #endregion
    }
}
