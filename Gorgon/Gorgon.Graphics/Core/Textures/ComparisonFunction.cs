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
// Created: April 14, 2026 9:37:21 PM
//

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines a type of comparison to perform for a comparison operation (e.g. depth compare)
/// </summary>
public enum ComparisonFunction
{
    /// <summary>
    /// <para>
    /// No comparison function.
    /// </para>
    /// </summary>
    None = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_NONE,
    /// <summary>
    /// <para>
    /// Never pass the comparison.
    /// </para>
    /// </summary>
    Never = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_NEVER,
    /// <summary>
    /// <para>
    /// If the source data is less than the destination data, the comparison passes.
    /// </para>
    /// </summary>
    Less = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_LESS,
    /// <summary>
    /// <para>
    /// If the source data is equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    Equal = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_EQUAL,
    /// <summary>
    /// <para>
    /// If the source data is less than or equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    LessEqual = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_LESS_EQUAL,
    /// <summary>
    /// <para>
    /// If the source data is greater than the destination data, the comparison passes.
    /// </para>
    /// </summary>
    Greater = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_GREATER,
    /// <summary>
    /// <para>
    /// If the source data is not equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    NotEqual = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_NOT_EQUAL,
    /// <summary>
    /// <para>
    /// If the source data is greater than or equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    GreaterEqual = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_GREATER_EQUAL,
    /// <summary>
    /// <para>
    /// Always pass the comparison.
    /// </para>
    /// </summary>
    Always = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_ALWAYS
}
