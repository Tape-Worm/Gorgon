
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
// Created: May 24, 2018 11:03:35 PM
// 

using Gorgon.Collections;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A grouping of resource states for a compute shader
/// </summary>
public sealed class GorgonComputeShaderResources
{
    /// <summary>
    /// Property to return the samplers for the shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonSamplerState> Samplers
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the constant buffers for the shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonConstantBufferView> ConstantBuffers
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the list of shader resources for the shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonShaderResourceView> ShaderResources
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the list of read/write (unordered access) views for the compute shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonReadWriteViewBinding> ReadWriteViews
    {
        get;
        internal set;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonComputeShaderResources"/> class.
    /// </summary>
    internal GorgonComputeShaderResources()
    {
    }
}
