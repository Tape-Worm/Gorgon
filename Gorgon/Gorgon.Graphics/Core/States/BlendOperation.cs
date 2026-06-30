
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 29, 2018 8:51:04 AM
// 

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of operation to perform while blending colors
/// </summary>
public enum BlendOperation
{
    /// <summary>
    /// <para>
    /// Add source 1 and source 2.
    /// </para>
    /// </summary>
    Add = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD,
    /// <summary>
    /// <para>
    /// Subtract source 1 from source 2.
    /// </para>
    /// </summary>
    Subtract = D3D12_BLEND_OP.D3D12_BLEND_OP_SUBTRACT,
    /// <summary>
    /// <para>
    /// Subtract source 2 from source 1.
    /// </para>
    /// </summary>
    ReverseSubtract = D3D12_BLEND_OP.D3D12_BLEND_OP_REV_SUBTRACT,
    /// <summary>
    /// <para>
    /// Find the minimum of source 1 and source 2.
    /// </para>
    /// </summary>
    Minimum = D3D12_BLEND_OP.D3D12_BLEND_OP_MIN,
    /// <summary>
    /// <para>
    /// Find the maximum of source 1 and source 2.
    /// </para>
    /// </summary>
    Maximum = D3D12_BLEND_OP.D3D12_BLEND_OP_MAX
}
