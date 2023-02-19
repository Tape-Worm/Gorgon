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
// Created: March 21, 2021 9:17:06 PM
// 
#endregion

using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers;

/// <summary>
/// Texture information for render targets used by the <see cref="Gorgon2DBloomEffect"/>.
/// </summary>
internal class BloomTextureInfo
    : IGorgonTexture2DInfo
{
    /// <summary>Property to set or return the width of the texture, in pixels.</summary>
    public int Width
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the height of the texture, in pixels.
    /// </summary>
    public int Height
    {
        get;
        set;
    }

    /// <summary>Property to return the number of array levels for a texture.</summary>
    /// <remarks>
    ///   <para>
    /// When this value is greater than 0, the texture will be used as a texture array. If the texture is supposed to be a cube map, then this value should be a multiple of 6 (1 for each face in the cube).
    /// </para>
    ///   <para>
    /// This value is defaulted to 1.
    /// </para>
    /// </remarks>
    int IGorgonTexture2DInfo.ArrayCount => 1;

    /// <summary>Property to return whether this 2D texture is a cube map.</summary>
    /// <remarks>
    ///   <para>
    /// When this value is set to <b>true</b>, then the texture is defined as a cube map using the <see cref="IGorgonTexture2DInfo.ArrayCount" /> as the number of faces. Because of this, the <see cref="IGorgonTexture2DInfo.ArrayCount" /> value
    /// must be a multiple of 6. If it is not, then the array count will be adjusted to meet the requirement.
    /// </para>
    ///   <para>
    /// This value is defaulted to <b>false</b>.
    /// </para>
    /// </remarks>
    bool IGorgonTexture2DInfo.IsCubeMap => false;

    /// <summary>Property to return the format of the texture.</summary>
    BufferFormat IGorgonTexture2DInfo.Format => BufferFormat.R16G16B16A16_Float;

    /// <summary>Property to return the number of mip-map levels for the texture.</summary>
    /// <remarks>
    ///   <para>
    /// If the texture is multisampled, this value must be set to 1.
    /// </para>
    ///   <para>
    /// This value is defaulted to 1.
    /// </para>
    /// </remarks>
    int IGorgonTexture2DInfo.MipLevels => 1;

    /// <summary>Property to return the multisample quality and count for this texture.</summary>
    /// <remarks>This value is defaulted to <see cref="GorgonMultisampleInfo.NoMultiSampling" />.</remarks>
    GorgonMultisampleInfo IGorgonTexture2DInfo.MultisampleInfo => GorgonMultisampleInfo.NoMultiSampling;

    /// <summary>Property to return the intended usage flags for this texture.</summary>
    /// <remarks>This value is defaulted to <see cref="ResourceUsage.Default" />.</remarks>
    ResourceUsage IGorgonTexture2DInfo.Usage => ResourceUsage.Default;

    /// <summary>Property to return the flags to determine how the texture will be bound with the pipeline when rendering.</summary>
    /// <remarks>
    ///   <para>
    /// If the <see cref="IGorgonTexture2DInfo.Usage" /> property is set to <see cref="ResourceUsage.Staging" />, then the texture must be created with a value of <see cref="TextureBinding.None" /> as staging textures do not
    /// support bindings of any kind. If this value is set to anything other than <see cref="TextureBinding.None" />, an exception will be thrown.
    /// </para>
    ///   <para>
    /// This value is defaulted to <see cref="TextureBinding.ShaderResource" />.
    /// </para>
    /// </remarks>
    TextureBinding IGorgonTexture2DInfo.Binding => TextureBinding.ShaderResource | TextureBinding.RenderTarget;

    /// <summary>Property to return whether this texture can be shared with other graphics interfaces.</summary>
    /// <remarks>
    /// Settings this flag to <b>true</b> allows the texture to be used with external graphics interfaces such as a Direct3D device. This is useful for providing interoperation between systems.
    /// </remarks>
    TextureSharingOptions IGorgonTexture2DInfo.Shared => TextureSharingOptions.None;

    /// <summary>Property to set or return the name of this object.</summary>
    public string Name
    {
        get;
        set;
    }

    /// <summary>Initializes a new instance of the <see cref="BloomTextureInfo" /> class.</summary>
    /// <param name="info">The information.</param>
    /// <param name="newName">The new name.</param>
    public BloomTextureInfo(IGorgonTexture2DInfo info, string newName = null)
    {
        Name = string.IsNullOrEmpty(newName) ? info.Name : newName;
        Width = info.Width;
        Height = info.Height;
    }

    /// <summary>Initializes a new instance of the <see cref="BloomTextureInfo" /> class.</summary>
    public BloomTextureInfo()
    {
    }
}
