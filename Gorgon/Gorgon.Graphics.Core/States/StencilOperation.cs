
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 29, 2018 8:51:25 AM
// 



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
    Keep = SharpDX.Direct3D11.StencilOperation.Keep,
    /// <summary>
    /// <para>
    /// Set the stencil data to 0.
    /// </para>
    /// </summary>
    Zero = SharpDX.Direct3D11.StencilOperation.Zero,
    /// <summary>
    /// <para>
    /// Set the stencil data to the reference value set by calling ID3D11DeviceContext::OMSetDepthStencilState.
    /// </para>
    /// </summary>
    Replace = SharpDX.Direct3D11.StencilOperation.Replace,
    /// <summary>
    /// <para>
    /// Increment the stencil value by 1, and clamp the result.
    /// </para>
    /// </summary>
    IncrementClamp = SharpDX.Direct3D11.StencilOperation.IncrementAndClamp,
    /// <summary>
    /// <para>
    /// Decrement the stencil value by 1, and clamp the result.
    /// </para>
    /// </summary>
    DecrementClamp = SharpDX.Direct3D11.StencilOperation.DecrementAndClamp,
    /// <summary>
    /// <para>
    /// Invert the stencil data.
    /// </para>
    /// </summary>
    Invert = SharpDX.Direct3D11.StencilOperation.Invert,
    /// <summary>
    /// <para>
    /// Increment the stencil value by 1, and wrap the result if necessary.
    /// </para>
    /// </summary>
    Increment = SharpDX.Direct3D11.StencilOperation.Increment,
    /// <summary>
    /// <para>
    /// Decrement the stencil value by 1, and wrap the result if necessary.
    /// </para>
    /// </summary>
    Decrement = SharpDX.Direct3D11.StencilOperation.Decrement
}
