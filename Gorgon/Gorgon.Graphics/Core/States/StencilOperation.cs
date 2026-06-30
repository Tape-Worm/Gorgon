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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 17, 2026 2:53:46 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines a type of operation to perform when masking using the stencil buffer
/// </summary>
public enum StencilOperation
{
    /// <summary>
    /// <para>
    /// Keep the existing stencil data.
    /// </para>
    /// </summary>
    Keep = D3D12_STENCIL_OP.D3D12_STENCIL_OP_KEEP,
    /// <summary>
    /// <para>
    /// Set the stencil data to 0.
    /// </para>
    /// </summary>
    Zero = D3D12_STENCIL_OP.D3D12_STENCIL_OP_ZERO,
    /// <summary>
    /// <para>
    /// Set the stencil data to the reference value set on the <see cref="GorgonDrawCall.DepthStencilReplaceValue"/>.
    /// </para>
    /// </summary>
    Replace = D3D12_STENCIL_OP.D3D12_STENCIL_OP_REPLACE,
    /// <summary>
    /// <para>
    /// Increment the stencil value by 1, and clamp the result if necessary.
    /// </para>
    /// </summary>
    IncrementClmap = D3D12_STENCIL_OP.D3D12_STENCIL_OP_INCR_SAT,
    /// <summary>
    /// <para>
    /// Decrement the stencil value by 1, and clamp the result if necessary.
    /// </para>
    /// </summary>
    DecrementClamp = D3D12_STENCIL_OP.D3D12_STENCIL_OP_DECR_SAT,
    /// <summary>
    /// <para>
    /// Invert the stencil data.
    /// </para>
    /// </summary>
    Invert = D3D12_STENCIL_OP.D3D12_STENCIL_OP_INVERT,
    /// <summary>
    /// <para>
    /// Increment the stencil value by 1, and wrap the result if necessary.
    /// </para>
    /// </summary>
    Increment = D3D12_STENCIL_OP.D3D12_STENCIL_OP_INCR,
    /// <summary>
    /// <para>
    /// Decrement the stencil value by 1, and wrap the result if necessary.
    /// </para>
    /// </summary>
    Decrement = D3D12_STENCIL_OP.D3D12_STENCIL_OP_DECR
}
