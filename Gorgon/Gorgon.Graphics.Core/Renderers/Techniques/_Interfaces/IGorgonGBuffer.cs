
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
// Created: April 19, 2021 10:52:46 PM
// 

using Gorgon.Graphics.Core;

namespace Gorgon.Renderers.Techniques;

/// <summary>
/// Defines a gbuffer for use with deferred rendering scenarios
/// </summary>
public interface IGorgonGBuffer
{
    /// <summary>
    /// Property to return the position texture for the gbuffer.
    /// </summary>
    GorgonTexture2DView Position
    {
        get;
    }

    /// <summary>
    /// Property to return the position render target for the gbuffer.
    /// </summary>
    GorgonRenderTarget2DView PositionTarget
    {
        get;
    }

    /// <summary>
    /// Property to return the diffuse texture for the gbuffer.
    /// </summary>
    GorgonTexture2DView Diffuse
    {
        get;
    }

    /// <summary>
    /// Property to return the diffuse render target for the gbuffer.
    /// </summary>
    GorgonRenderTarget2DView DiffuseTarget
    {
        get;
    }

    /// <summary>
    /// Property to return the entire gbuffer texture (all array indices).
    /// </summary>
    GorgonTexture2DView GBufferTexture
    {
        get;
    }

    /// <summary>
    /// Property to return the normal map texture for the gbuffer.
    /// </summary>
    GorgonTexture2DView Normal
    {
        get;
    }

    /// <summary>
    /// Property to return the normal map render target for the gbuffer.
    /// </summary>
    GorgonRenderTarget2DView NormalTarget
    {
        get;
    }

    /// <summary>
    /// Property to return the specular texture for the gbuffer.
    /// </summary>
    GorgonTexture2DView Specular
    {
        get;
    }

    /// <summary>
    /// Property to return the specular render target for the gbuffer.
    /// </summary>
    GorgonRenderTarget2DView SpecularTarget
    {
        get;
    }

    /// <summary>
    /// Function to clear the GBuffer.
    /// </summary>
    void ClearGBuffer();

    /// <summary>
    /// Function to update the gbuffer to a new width and height.
    /// </summary>
    /// <param name="width">The width of the gbuffer texture.</param>
    /// <param name="height">The height of the gbuffer texture.</param>
    void Resize(int width, int height);

}
