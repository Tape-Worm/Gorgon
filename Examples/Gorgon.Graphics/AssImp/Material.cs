
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
// Created: March 1, 2017 12:52:33 PM
// 

using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Examples;

/// <summary>
/// Represents a material applied to a model
/// </summary>
public class Material
{
    /// <summary>
    /// Property to set or return the diffuse color for the model.
    /// </summary>
    public GorgonColor Diffuse
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the emissive color.
    /// </summary>
    public GorgonColor Emissive
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the specular color.
    /// </summary>
    public GorgonColor Specular
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the specular power.
    /// </summary>
    public float SpecularPower
    {
        get;
        set;
    } = 1.0f;

    /// <summary>
    /// Property to set or return the texture for the model.
    /// </summary>
    public GorgonTexture2DView Texture
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the sampler used to sample the texture in the shader.
    /// </summary>
    public GorgonSamplerState TextureSampler
    {
        get;
        set;
    }
}
