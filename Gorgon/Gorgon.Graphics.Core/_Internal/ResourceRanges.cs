#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 1, 2021 1:18:53 PM
// 
#endregion


namespace Gorgon.Graphics.Core;

/// <summary>
/// The list of ranges indicating what indices have changed for resource arrays.
/// </summary>
internal class ResourceRanges
{
    /// <summary>
    /// The resources that have changed.
    /// </summary>
    public ResourceStateChanges Changes;
    /// <summary>
    /// The begin/end index for vertex buffers.
    /// </summary>
    public (int Start, int Count) VertexBuffers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for stream out buffers.
    /// </summary>
    public (int Start, int Count) StreamOutBuffers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for vertex shader constants.
    /// </summary>
    public (int Start, int Count) VertexShaderConstants = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for vertex shader resource views.
    /// </summary>
    public (int Start, int Count) VertexShaderResources = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for vertex shader samplers.
    /// </summary>
    public (int Start, int Count) VertexShaderSamplers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for pixel shader constants.
    /// </summary>
    public (int Start, int Count) PixelShaderConstants = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for pixel shader resource views.
    /// </summary>
    public (int Start, int Count) PixelShaderResources = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for pixel shader samplers.
    /// </summary>
    public (int Start, int Count) PixelShaderSamplers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for geometry shader constants.
    /// </summary>
    public (int Start, int Count) GeometryShaderConstants = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for geometry shader resource views.
    /// </summary>
    public (int Start, int Count) GeometryShaderResources = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for geometry shader samplers.
    /// </summary>
    public (int Start, int Count) GeometryShaderSamplers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for hull shader constants.
    /// </summary>
    public (int Start, int Count) HullShaderConstants = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for hull shader resource views.
    /// </summary>
    public (int Start, int Count) HullShaderResources = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for hull shader samplers.
    /// </summary>
    public (int Start, int Count) HullShaderSamplers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for domain shader constants.
    /// </summary>
    public (int Start, int Count) DomainShaderConstants = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for domain shader resource views.
    /// </summary>
    public (int Start, int Count) DomainShaderResources = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for domain shader samplers.
    /// </summary>
    public (int Start, int Count) DomainShaderSamplers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for compute shader constants.
    /// </summary>
    public (int Start, int Count) ComputeShaderConstants = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for compute shader resource views.
    /// </summary>
    public (int Start, int Count) ComputeShaderResources = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for compute shader samplers.
    /// </summary>
    public (int Start, int Count) ComputeShaderSamplers = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for compute shader unordered access views.
    /// </summary>
    public (int Start, int Count) ComputeShaderUavs = (int.MaxValue, int.MinValue);
    /// <summary>
    /// The begin/end index for unordered access views.
    /// </summary>
    public (int Start, int Count) Uavs = (int.MaxValue, int.MinValue);
}
