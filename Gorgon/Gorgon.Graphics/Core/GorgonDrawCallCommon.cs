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

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common values for draw calls that draw instances of primitives.
/// </summary>
/// <param name="pso">The graphics pipeline state object to use for the call.</param>
/// <remarks>
/// <para>
/// This holds the instancing values shared by <see cref="GorgonDrawCall"/> and <see cref="GorgonIndexedDrawCall"/>. Execute calls do not use these values, because each command in the 
/// <see cref="GorgonExecuteCallCommon.Commands"/> buffer carries its own instance values.
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList.Draw(GorgonDrawCall)"/>
/// <seealso cref="GorgonCommandList.Draw(GorgonIndexedDrawCall)"/>
public abstract class GorgonDrawCallCommon(GorgonGraphicsPso pso)
        : GorgonGraphicsCallCommon(pso)
{
    /// <summary>
    /// Property to set or return the location of the first instance to draw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <a href="https://learn.microsoft.com/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics#system-value-semantics" target="_blank">SV_InstanceID</a> value in the vertex shader starts at 0, and does 
    /// not include this value. A vertex shader that needs this value reads it through the 
    /// <a href="https://microsoft.github.io/hlsl-specs/proposals/0015-extended-command-info/" target="_blank">SV_StartInstanceLocation</a> system value (shader model 6.8, see 
    /// <see cref="GorgonVideoAdapterInfo.SupportsExtendedCommandInfo"/>), or receives it through a constant.
    /// </para>
    /// <para>
    /// Negative values are treated as 0.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int StartInstance
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the number of instances to draw.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this value is less than 1, nothing is drawn.
    /// </para>
    /// <para>
    /// The default value is 1.
    /// </para>
    /// </remarks>
    public int InstanceCount
    {
        get;
        set;
    } = 1;
}
