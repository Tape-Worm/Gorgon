// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 25, 2026 12:15:44 AM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines parameters for drawing primitives.
/// </summary>
/// <param name="vertexCount">The number of vertices to draw.</param>
/// <param name="pso">The graphics pipeline state object to use when drawing.</param>
/// <remarks>
/// <para>
/// This is used by the <see cref="GorgonCommandList.Draw(GorgonDrawCall)"/> method.
/// </para>
/// <para type="vertices">
/// Gorgon does not bind vertex buffers. The vertex shader reads its vertex data from buffers through their view handles, so any buffer the vertex shader reads must be assigned to the call with 
/// <see cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList.Draw(GorgonDrawCall)"/>
public sealed class GorgonDrawCall(int vertexCount, GorgonGraphicsPso pso)
        : GorgonDrawCallCommon(pso)
{
    /// <summary>
    /// Property to set or return the number of vertices to draw for each instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this value is less than 1, nothing is drawn.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.Draw(GorgonDrawCall)"/>
    public int VertexCount
    {
        get;
        set;
    } = vertexCount;

    /// <summary>
    /// Property to set or return the location of the first vertex to draw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_VertexID</a> value in the vertex shader starts at 0, and does 
    /// not include this value. A vertex shader that needs this value reads it through the 
    /// <a href="https://microsoft.github.io/hlsl-specs/proposals/0015-extended-command-info/" target="_blank">SV_StartVertexLocation</a> system value (shader model 6.8, see 
    /// <see cref="GorgonVideoAdapterInfo.SupportsExtendedCommandInfo"/>), or receives it through a constant.
    /// </para>
    /// <para>
    /// Negative values are treated as 0.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int StartVertex
    {
        get;
        set;
    }
}
