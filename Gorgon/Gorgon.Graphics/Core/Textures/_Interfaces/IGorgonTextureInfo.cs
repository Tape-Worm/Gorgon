// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: January 11, 2026 12:06:54 PM
//

using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The available texture types.
/// </summary>
public enum TextureType
{
    /// <summary>
    /// Unknown texture data.
    /// </summary>
    Unknown = ImageDataType.Unknown,
    /// <summary>
    /// 
    /// </summary>
    Texture1D = ImageDataType.Image1D,
    /// <summary>
    /// 
    /// </summary>
    Texture2D = ImageDataType.Image2D,
    /// <summary>
    /// 
    /// </summary>
    Texture3D = ImageDataType.Image3D
}


/// <summary>
/// Information that was used to build a <see cref="GorgonTexture"/>.
/// </summary>
public interface IGorgonTextureInfo
{
    /// <summary>
    /// Property to return the number of array elements in the texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies to <see cref="TextureType.Texture1D"/> and <see cref="TextureType.Texture2D"/> textures.
    /// </para>
    /// </remarks>
    short ArrayCount
    {
        get;        
    }

    /// <summary>
    /// Property to return the depth of the texture, in depth slices.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to <see cref="TextureType.Texture3D"/> textures.
    /// </para>
    /// </remarks>
    short Depth
    {
        get;        
    }

    /// <summary>
    /// Property to return the format of a texel within the texture.
    /// </summary>
    BufferFormat Format
    {
        get;        
    }

    /// <summary>
    /// Property to return the height of the texture, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is only available to <see cref="TextureType.Texture2D"/> and <see cref="TextureType.Texture3D"/> textures.
    /// </para>
    /// </remarks>
    int Height
    {
        get;        
    }

    /// <summary>
    /// Property to return whether this texure is a cube map.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value only applies to <see cref="TextureType.Texture2D"/> textures with an <see cref="ArrayCount"/> that is a multiple of 6.
    /// </para>
    /// </remarks>
    bool IsCube
    {
        get;        
    }

    /// <summary>
    /// Property to return whether this texture is used as a depth/stencil buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For best performance, it is recommended that the application set <see cref="IsShaderResource"/> to <b>false</b> if the texture is never read from.
    /// </para>
    /// </remarks>
    bool IsDepthStencil
    {
        get;        
    }

    /// <summary>
    /// Property to return whether this texture is used as a render target.
    /// </summary>
    bool IsRenderTarget
    {
        get;        
    }

    /// <summary>
    /// Property to return whether this texture is used as a shader resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When set to <b>false</b>, some adapter architectures gain bandwidth capacity If a texture is rarely used as a shader resource, it may be better to have two textures, one with this value set to 
    /// <b>false</b>, and another set to <b>true</b> and copy between them.
    /// </para>
    /// <para>
    /// If the texture has <see cref="IsDepthStencil"/> set to <b>true</b>, and the depth texture is never read in a shader, then performance is improved when this value is set to <b>false</b>.
    /// </para>
    /// </remarks>
    bool IsShaderResource
    {
        get;        
    }

    /// <summary>
    /// Property to return whether this texture is used as an unordered access resource.
    /// </summary>
    bool IsUnorderedAccess
    {
        get;        
    }

    /// <summary>
    /// Property to return the number of mip map levels in the texture.
    /// </summary>
    short MipCount
    {
        get;        
    }

    /// <summary>
    /// Property to return the multisample information for the texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies to <see cref="TextureType.Texture2D"/> textures and is ignored on other types.
    /// </para>
    /// </remarks>
    GorgonMultisampleInfo MultisampleInfo
    {
        get;        
    }

    /// <summary>
    /// Property to return the type of texture to create.
    /// </summary>
    TextureType Type
    {
        get;        
    }

    /// <summary>
    /// Property to return the width of the texture, in pixels.
    /// </summary>
    int Width
    {
        get;        
    }

    /// <summary>
    /// Property to return the bounds of the texture at mip level 0.
    /// </summary>
    GorgonRectangle Bounds
    {
        get;
    }
}