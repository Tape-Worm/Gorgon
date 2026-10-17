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
// Created: October 17, 2026 12:35:49 AM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines how the value in a predication buffer decides whether commands on a command list are skipped.
/// </summary>
/// <remarks>
/// <para>
/// The value used for predication is a 64-bit unsigned integer, read from a buffer passed to <see cref="GorgonCommandList.SetPredication"/>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonCommandList.SetPredication"/>
public enum PredicationOperation
{
    /// <summary>
    /// Commands are skipped when the value is 0.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this with the results of a <see cref="QueryType.Occlusion"/> or <see cref="QueryType.BinaryOcclusion"/> query to skip the commands when nothing was visible.
    /// </para>
    /// </remarks>
    SkipIfZero = D3D12_PREDICATION_OP.D3D12_PREDICATION_OP_EQUAL_ZERO,
    /// <summary>
    /// Commands are skipped when the value is not 0.
    /// </summary>
    SkipIfNotZero = D3D12_PREDICATION_OP.D3D12_PREDICATION_OP_NOT_EQUAL_ZERO
}
