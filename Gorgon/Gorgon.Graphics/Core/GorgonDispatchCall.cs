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
// Created: October 7, 2026 12:35:00 AM
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines parameters for dispatching thread groups to a compute shader.
/// </summary>
/// <param name="pso">The compute pipeline state object to use for the call.</param>
/// <param name="threadXGroupCount">The number of thread groups to dispatch in the X dimension.</param>
/// <param name="threadYGroupCount">The number of thread groups to dispatch in the Y dimension.</param>
/// <param name="threadZGroupCount">The number of thread groups to dispatch in the Z dimension.</param>
/// <remarks>
/// <para>
/// This is used by the <see cref="GorgonCommandList.Dispatch(GorgonDispatchCall)"/> and <see cref="GorgonComputeCommandList.Dispatch(GorgonDispatchCall)"/> methods.
/// </para>
/// <para>
/// The total number of thread groups dispatched is <see cref="ThreadXGroupCount"/> x <see cref="ThreadYGroupCount"/> x <see cref="ThreadZGroupCount"/>. The number of threads in each group is defined 
/// by the <c>numthreads</c> attribute on the compute shader.
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList.Dispatch(GorgonDispatchCall)"/>
/// <seealso cref="GorgonComputeCommandList.Dispatch(GorgonDispatchCall)"/>
public sealed class GorgonDispatchCall(GorgonComputePso pso, int threadXGroupCount, int threadYGroupCount, int threadZGroupCount)
    : GorgonComputeCallCommon(pso)
{
    /// <summary>
    /// Property to set or return the number of thread groups to dispatch in the X dimension.
    /// </summary>
    /// <remarks>
    /// <para type="limits">
    /// If this value is less than 1, nothing is dispatched. If this value is greater than 65535, an exception is thrown when the call is dispatched.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.Dispatch(GorgonDispatchCall)"/>
    public int ThreadXGroupCount
    {
        get;
        set;
    } = threadXGroupCount;

    /// <summary>
    /// Property to set or return the number of thread groups to dispatch in the Y dimension.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ThreadXGroupCount" path="/remarks/para[@type='limits']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.Dispatch(GorgonDispatchCall)"/>
    public int ThreadYGroupCount
    {
        get;
        set;
    } = threadYGroupCount;

    /// <summary>
    /// Property to set or return the number of thread groups to dispatch in the Z dimension.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ThreadXGroupCount" path="/remarks/para[@type='limits']"/>
    /// </remarks>
    /// <seealso cref="GorgonCommandList.Dispatch(GorgonDispatchCall)"/>
    public int ThreadZGroupCount
    {
        get;
        set;
    } = threadZGroupCount;
}
