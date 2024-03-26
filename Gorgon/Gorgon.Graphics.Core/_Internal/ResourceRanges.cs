
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 1, 2021 1:18:53 PM
// 

using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The list of ranges indicating what indices have changed for resource arrays
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
    public (int Start, int Count) VertexBuffers = (0, 0);
    /// <summary>
    /// The begin/end index for stream out buffers.
    /// </summary>
    public (int Start, int Count) StreamOutBuffers = (0, 0);
    /// <summary>
    /// The begin/end index for vertex shader constants.
    /// </summary>
    public (int Start, int Count) VertexShaderConstants = (0, 0);
    /// <summary>
    /// The begin/end index for vertex shader resource views.
    /// </summary>
    public (int Start, int Count) VertexShaderResources = (0, 0);
    /// <summary>
    /// The begin/end index for vertex shader samplers.
    /// </summary>
    public (int Start, int Count) VertexShaderSamplers = (0, 0);
    /// <summary>
    /// The begin/end index for pixel shader constants.
    /// </summary>
    public (int Start, int Count) PixelShaderConstants = (0, 0);
    /// <summary>
    /// The begin/end index for pixel shader resource views.
    /// </summary>
    public (int Start, int Count) PixelShaderResources = (0, 0);
    /// <summary>
    /// The begin/end index for pixel shader samplers.
    /// </summary>
    public (int Start, int Count) PixelShaderSamplers = (0, 0);
    /// <summary>
    /// The begin/end index for geometry shader constants.
    /// </summary>
    public (int Start, int Count) GeometryShaderConstants = (0, 0);
    /// <summary>
    /// The begin/end index for geometry shader resource views.
    /// </summary>
    public (int Start, int Count) GeometryShaderResources = (0, 0);
    /// <summary>
    /// The begin/end index for geometry shader samplers.
    /// </summary>
    public (int Start, int Count) GeometryShaderSamplers = (0, 0);
    /// <summary>
    /// The begin/end index for hull shader constants.
    /// </summary>
    public (int Start, int Count) HullShaderConstants = (0, 0);
    /// <summary>
    /// The begin/end index for hull shader resource views.
    /// </summary>
    public (int Start, int Count) HullShaderResources = (0, 0);
    /// <summary>
    /// The begin/end index for hull shader samplers.
    /// </summary>
    public (int Start, int Count) HullShaderSamplers = (0, 0);
    /// <summary>
    /// The begin/end index for domain shader constants.
    /// </summary>
    public (int Start, int Count) DomainShaderConstants = (0, 0);
    /// <summary>
    /// The begin/end index for domain shader resource views.
    /// </summary>
    public (int Start, int Count) DomainShaderResources = (0, 0);
    /// <summary>
    /// The begin/end index for domain shader samplers.
    /// </summary>
    public (int Start, int Count) DomainShaderSamplers = (0, 0);
    /// <summary>
    /// The begin/end index for compute shader constants.
    /// </summary>
    public (int Start, int Count) ComputeShaderConstants = (0, 0);
    /// <summary>
    /// The begin/end index for compute shader resource views.
    /// </summary>
    public (int Start, int Count) ComputeShaderResources = (0, 0);
    /// <summary>
    /// The begin/end index for compute shader samplers.
    /// </summary>
    public (int Start, int Count) ComputeShaderSamplers = (0, 0);
    /// <summary>
    /// The begin/end index for compute shader unordered access views.
    /// </summary>
    public (int Start, int Count) ComputeShaderUavs = (0, 0);
    /// <summary>
    /// The begin/end index for unordered access views.
    /// </summary>
    public (int Start, int Count) Uavs = (0, 0);

    /// <summary>
    /// Function to reset all values back to their original state.
    /// </summary>
    public void Reset()
    {
        Changes = ResourceStateChanges.None;
        VertexBuffers = (0, 0);
        StreamOutBuffers = (0, 0);
        VertexShaderConstants = (0, 0);
        VertexShaderResources = (0, 0);
        VertexShaderSamplers = (0, 0);
        PixelShaderConstants = (0, 0);
        PixelShaderResources = (0, 0);
        PixelShaderSamplers = (0, 0);
        GeometryShaderConstants = (0, 0);
        GeometryShaderResources = (0, 0);
        GeometryShaderSamplers = (0, 0);
        HullShaderConstants = (0, 0);
        HullShaderResources = (0, 0);
        HullShaderSamplers = (0, 0);
        DomainShaderConstants = (0, 0);
        DomainShaderResources = (0, 0);
        DomainShaderSamplers = (0, 0);
        ComputeShaderConstants = (0, 0);
        ComputeShaderResources = (0, 0);
        ComputeShaderSamplers = (0, 0);
        ComputeShaderUavs = (0, 0);
        Uavs = (0, 0);
    }
}
