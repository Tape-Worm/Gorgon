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
// Created: May 23, 2018 12:19:14 PM
// 
#endregion

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A draw call that draws only using a set of vertices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A draw call is an immutable object that contains all of the state required to render mesh information. For each mesh an application needs to render, an single draw call should be issued via the
    /// <see cref="GorgonGraphics.Submit(GorgonDrawCall, GorgonColor?, int, int)"/> methods.  
    /// </para>
    /// <para>
    /// State management is handled internally by Gorgon so that duplicate states are not set and thus, performance is not impacted by redundant states.
    /// </para>
    /// <para>
    /// Because a draw call is immutable, it is not possible to modify a draw call after it's been created. However, a copy of a draw call can be created using the
    /// <see cref="GorgonDrawCallBuilderCommon{TB,TDc}.ResetTo"/> method on this object. Or, the builder can be modified after the creation of your draw call that needs to be updated and a new call may be 
    /// built then.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonDrawCallBuilder"/>
    public class GorgonDrawCall
        : GorgonDrawCallCommon
    {
        /// <summary>
        /// Property to return the vertex index to start at within the vertex buffer.
        /// </summary>
        public int VertexStartIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the number of vertices to draw.
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
