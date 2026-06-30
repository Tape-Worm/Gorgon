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
// Created: May 29, 2018 8:51:17 AM
// 

using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of logical operations to perform while blending a render target
/// </summary>
public enum LogicOperation
{
    /// <summary>
    /// <para>
    /// Clears the render target.
    /// </para>
    /// </summary>
    Clear = D3D12_LOGIC_OP.D3D12_LOGIC_OP_CLEAR,
    /// <summary>
    /// <para>
    /// Sets the render target.
    /// </para>
    /// </summary>
    Set = D3D12_LOGIC_OP.D3D12_LOGIC_OP_SET,
    /// <summary>
    /// <para>
    /// Copies the render target.
    /// </para>
    /// </summary>
    Copy = D3D12_LOGIC_OP.D3D12_LOGIC_OP_COPY,
    /// <summary>
    /// <para>
    /// Performs an inverted-copy of the render target.
    /// </para>
    /// </summary>
    CopyInverted = D3D12_LOGIC_OP.D3D12_LOGIC_OP_COPY_INVERTED,
    /// <summary>
    /// <para>
    /// No operation is performed on the render target.
    /// </para>
    /// </summary>
    Noop = D3D12_LOGIC_OP.D3D12_LOGIC_OP_NOOP,
    /// <summary>
    /// <para>
    /// Inverts the render target.
    /// </para>
    /// </summary>
    Invert = D3D12_LOGIC_OP.D3D12_LOGIC_OP_INVERT,
    /// <summary>
    /// <para>
    /// Performs a logical AND operation on the render target.
    /// </para>
    /// </summary>
    And = D3D12_LOGIC_OP.D3D12_LOGIC_OP_AND,
    /// <summary>
    /// <para>
    /// Performs a logical NAND operation on the render target.
    /// </para>
    /// </summary>
    Nand = D3D12_LOGIC_OP.D3D12_LOGIC_OP_NAND,
    /// <summary>
    /// <para>
    /// Performs a logical OR operation on the render target.
    /// </para>
    /// </summary>
    Or = D3D12_LOGIC_OP.D3D12_LOGIC_OP_OR,
    /// <summary>
    /// <para>
    /// Performs a logical NOR operation on the render target.
    /// </para>
    /// </summary>
    Nor = D3D12_LOGIC_OP.D3D12_LOGIC_OP_NOR,
    /// <summary>
    /// <para>
    /// Performs a logical XOR operation on the render target.
    /// </para>
    /// </summary>
    Xor = D3D12_LOGIC_OP.D3D12_LOGIC_OP_XOR,
    /// <summary>
    /// <para>
    /// Performs a logical equal operation on the render target.
    /// </para>
    /// </summary>
    Equiv = D3D12_LOGIC_OP.D3D12_LOGIC_OP_EQUIV,
    /// <summary>
    /// <para>
    /// Performs a logical AND and reverse operation on the render target.
    /// </para>
    /// </summary>
    AndReverse = D3D12_LOGIC_OP.D3D12_LOGIC_OP_AND_REVERSE,
    /// <summary>
    /// <para>
    /// Performs a logical AND and invert operation on the render target.
    /// </para>
    /// </summary>
    AndInverted = D3D12_LOGIC_OP.D3D12_LOGIC_OP_AND_INVERTED,
    /// <summary>
    /// <para>
    /// Performs a logical OR and reverse operation on the render target.
    /// </para>
    /// </summary>
    OrReverse = D3D12_LOGIC_OP.D3D12_LOGIC_OP_OR_REVERSE,
    /// <summary>
    /// <para>
    /// Performs a logical OR and invert operation on the render target.
    /// </para>
    /// </summary>
    OrInverted = D3D12_LOGIC_OP.D3D12_LOGIC_OP_OR_INVERTED
}
