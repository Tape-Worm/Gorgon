
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
// Created: April 17, 2018 12:40:25 PM
// 


using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information used to create a 3D texture object
/// </summary>
/// <remarks>
/// <para>
/// This provides an immutable view of the texture information so that it cannot be modified after the texture is created
/// </para>
/// </remarks>
public interface IGorgonTexture3DInfo
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the width of the texture, in pixels.
    /// </summary>
    int Width
    {
        get;
    }

    /// <summary>
    /// Property to return the height of the texture, in pixels.
    /// </summary>
    int Height
    {
        get;
    }

    /// <summary>
    /// Property to return the depth of the texture, in slices.
    /// </summary>
    int Depth
    {
        get;
    }

    /// <summary>
    /// Property to return the format of the texture.
    /// </summary>
    BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return the number of mip-map levels for the texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the texture is multisampled, this value must be set to 1.
    /// </para>
    /// <para>
    /// This value is defaulted to 1.
    /// </para>
    /// </remarks>
    int MipLevels
    {
        get;
    }

    /// <summary>
    /// Property to return the intended usage flags for this texture.
    /// </summary>
    /// <remarks>
    /// This value is defaulted to <see cref="ResourceUsage.Default"/>.
    /// </remarks>
    ResourceUsage Usage
    {
        get;
    }

    /// <summary>
    /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Usage"/> property is set to <see cref="ResourceUsage.Staging"/>, then the texture must be created with a value of <see cref="TextureBinding.None"/> as staging textures do not 
    /// support bindings of any kind. If this value is set to anything other than <see cref="TextureBinding.None"/>, an exception will be thrown.
    /// </para>
    /// <para>
    /// This value is defaulted to <see cref="TextureBinding.ShaderResource"/>. 
    /// </para>
    /// </remarks>
    TextureBinding Binding
    {
        get;
    }
}
