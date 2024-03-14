#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 29, 2018 8:51:04 AM
// 
#endregion


namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the type of operation to perform while blending colors.
/// </summary>
public enum BlendOperation
{
    /// <summary>
    /// <para>
    /// Add source 1 and source 2.
    /// </para>
    /// </summary>
    Add = SharpDX.Direct3D11.BlendOperation.Add,
    /// <summary>
    /// <para>
    /// Subtract source 1 from source 2.
    /// </para>
    /// </summary>
    Subtract = SharpDX.Direct3D11.BlendOperation.Subtract,
    /// <summary>
    /// <para>
    /// Subtract source 2 from source 1.
    /// </para>
    /// </summary>
    ReverseSubtract = SharpDX.Direct3D11.BlendOperation.ReverseSubtract,
    /// <summary>
    /// <para>
    /// Find the minimum of source 1 and source 2.
    /// </para>
    /// </summary>
    Minimum = SharpDX.Direct3D11.BlendOperation.Minimum,
    /// <summary>
    /// <para>
    /// Find the maximum of source 1 and source 2.
    /// </para>
    /// </summary>
    Maximum = SharpDX.Direct3D11.BlendOperation.Maximum
}
