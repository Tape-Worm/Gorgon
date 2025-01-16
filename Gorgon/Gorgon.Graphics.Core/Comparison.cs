
// 
// Gorgon
// Copyright (C) 2017 Michael Winsor
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
// Created: November 20, 2017 12:48:08 PM
// 

// ReSharper disable InconsistentNaming
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines a type of comparison to perform for a comparison operation (e.g. depth compare)
/// </summary>
public enum Comparison
{
    /// <summary>
    /// <para>
    /// Never pass the comparison.
    /// </para>
    /// </summary>
    Never = D3D11.Comparison.Never,
    /// <summary>
    /// <para>
    /// If the source data is less than the destination data, the comparison passes.
    /// </para>
    /// </summary>
    Less = D3D11.Comparison.Less,
    /// <summary>
    /// <para>
    /// If the source data is equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    Equal = D3D11.Comparison.Equal,
    /// <summary>
    /// <para>
    /// If the source data is less than or equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    LessEqual = D3D11.Comparison.LessEqual,
    /// <summary>
    /// <para>
    /// If the source data is greater than the destination data, the comparison passes.
    /// </para>
    /// </summary>
    Greater = D3D11.Comparison.Greater,
    /// <summary>
    /// <para>
    /// If the source data is not equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    NotEqual = D3D11.Comparison.NotEqual,
    /// <summary>
    /// <para>
    /// If the source data is greater than or equal to the destination data, the comparison passes.
    /// </para>
    /// </summary>
    GreaterEqual = D3D11.Comparison.GreaterEqual,
    /// <summary>
    /// <para>
    /// Always pass the comparison.
    /// </para>
    /// </summary>
    Always = D3D11.Comparison.Always
}
