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
/// Defines parameters for drawing indexed primitives.
/// </summary>
/// <param name="indexCount">The number of indices to draw.</param>
/// <param name="indexBuffer">The index buffer to use when drawing.</param>
/// <param name="pso">The graphics pipeline state object to use when drawing.</param>
/// <remarks>
/// <para>
/// This is used by the <see cref="GorgonCommandList.Draw(GorgonIndexedDrawCall)"/> method.
/// </para>
/// <inheritdoc cref="GorgonDrawCall" path="/remarks/para[@type='vertices']"/>
/// </remarks>
/// <seealso cref="GorgonCommandList.Draw(GorgonIndexedDrawCall)"/>
public sealed class GorgonIndexedDrawCall(int indexCount, GorgonIndexBuffer indexBuffer, GorgonGraphicsPso pso)
        : GorgonDrawCallCommon(pso)
{
    /// <summary>
    /// Property to set or return the index buffer used to draw.
    /// </summary>
    public GorgonIndexBuffer IndexBuffer
    {
        get;
        set;
    } = indexBuffer;

    /// <summary>
    /// Property to set or return the location of the first index read from the index buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Negative values are treated as 0.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int StartIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the base vertex for the draw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_VertexID</a> value in the vertex shader is the index read from 
    /// the index buffer, and does not include this value. A vertex shader that needs the base vertex reads it through the 
    /// <a href="https://microsoft.github.io/hlsl-specs/proposals/0015-extended-command-info/" target="_blank">SV_StartVertexLocation</a> system value (shader model 6.8, see 
    /// <see cref="GorgonVideoAdapterInfo.SupportsExtendedCommandInfo"/>), or receives it through a constant, and adds it to the index itself.
    /// </para>
    /// <para>
    /// This value can be negative, as long as the index plus the base vertex is not less than 0.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int BaseVertex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of indices to read from the index buffer for each instance.
    /// </summary>
    /// <seealso cref="GorgonCommandList"/>
    /// <seealso cref="GorgonIndexBuffer"/>
    public int IndexCount
    {
        get;
        set;
    } = indexCount;
}
