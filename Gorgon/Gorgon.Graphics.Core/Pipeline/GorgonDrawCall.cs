﻿#region MIT
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
// Created: May 23, 2018 12:19:14 PM
// 
#endregion

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A draw call that draws only using a set of vertices.
    /// </summary>
    public class GorgonDrawCall
        : GorgonDrawCallCommon
    {
        /// <summary>
        /// Property to set or return the vertex index to start at within the vertex buffer.
        /// </summary>
        public int VertexStartIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the number of vertices to draw.
        /// </summary>
        public int VertexCount
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonDrawCall"/> class.
        /// </summary>
        internal GorgonDrawCall()
        {
            // We need to create this through a builder.
        }
    }
}
