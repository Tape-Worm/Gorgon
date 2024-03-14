
// 
// Gorgon
// Copyright (C) 2016 Michael Winsor
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
// Created: July 28, 2016 12:20:33 AM
// 


using Gorgon.Core;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines the flags that describe how the texture should be used
/// </summary>
/// <remarks>
/// <para>
/// This values can be OR'd together for use in different stages of the pipeline. For example, <c>ShaderResource | RenderTarget</c> allows the texture to be used as a render target and a shader input 
/// (although, not at the same time) through either a render target view, or shader resource view
/// </para>
/// </remarks>
[Flags]
public enum TextureBinding
{
    /// <summary>
    /// <para>
    /// No binding will be done with this texture.
    /// </para>
    /// <para>
    /// This flag is mutually exclusive, and supercedes any other flags.
    /// </para>
    /// <para>
    /// If this flag is set, then this texture cannot be bound with the pipeline. And this is the only binding flag allowed when the texture has a <see cref="IGorgonTexture2DInfo.Usage"/> 
    /// of <see cref="ResourceUsage.Staging"/>.
    /// </para>
    /// </summary>
    None = D3D11.BindFlags.None,
    /// <summary>
    /// The texture is meant to be bound as an input to a shader.
    /// </summary>
    ShaderResource = D3D11.BindFlags.ShaderResource,
    /// <summary>
    /// <para>
    /// The texture is meant to be used as a render target.
    /// </para>
    /// <para>
    /// <note type="important">
    /// <see cref="GorgonTexture1D"/> types cannot have this as a binding. This restriction is imposed by Gorgon and may be lifted in the future.
    /// </note>
    /// </para>
    /// </summary>
    RenderTarget = D3D11.BindFlags.RenderTarget,
    /// <summary>
    /// <para>
    /// The texture is meant to be used as a depth stencil buffer.
    /// </para>
    /// <para>
    /// To use a depth/stencil buffer as a shader input, the <see cref="IGorgonTexture2DInfo.Format"/> must be set to an typeless appropriate format. Failure to do so when specifying this flag 
    /// will result in an exception.
    /// </para>
    /// <para>
    /// The following table lists the acceptable typeless formats to use with a depth/stencil format:
    /// <list type="table">
    ///		<listheader>
    ///			<term>Depth/Stencil Format</term>
    ///			<term>Typeless Format</term>
    ///		</listheader>
    ///		<item>
    ///			<term>D16_UNorm</term>
    ///			<term>R16_Typeless</term>
    ///		</item>
    ///		<item>
    ///			<term>D32_Float</term>
    ///			<term>R32_Typeless</term>
    ///		</item>
    ///		<item>
    ///			<term>D24_UNorm_S8_UInt</term>
    ///			<term>R24G8_Typeless</term>
    ///		</item>
    ///		<item>
    ///			<term>D32_Float_S8X24_UInt</term>
    ///			<term>R32G8X24_Typeless</term>
    ///		</item>
    /// </list>
    /// </para>
    /// <para>
    /// <note type="important">
    /// <see cref="GorgonTexture1D"/> and <see cref="GorgonTexture3D"/> types cannot have this as a binding.
    /// </note>
    /// </para>
    /// </summary>
    DepthStencil = D3D11.BindFlags.DepthStencil,
    /// <summary>
    /// <para>
    /// The texture is meant to be used with a read/write (unordered access) view.
    /// </para>
    /// <para>
    /// Textures that are multisampled cannot use this flag.
    /// </para>
    /// </summary>
    ReadWriteView = D3D11.BindFlags.UnorderedAccess
}

/// <summary>
/// Values to indicate how texture resources should be shared
/// </summary>
public enum TextureSharingOptions
{
    /// <summary>
    /// No sharing.
    /// </summary>
    None = 0,
    /// <summary>
    /// Enables resource data sharing between two or more Direct3D devices. The only resources that can be shared are 2D non-mipmapped textures.
    /// </summary>
    Shared = 1,
    /// <summary>
    /// Enables the resource to be synchronized by using the IDXGIKeyedMutex::AcquireSync and IDXGIKeyedMutex::ReleaseSync APIs. 
    /// </summary>
    SharedKeyedMutex = 2
}

/// <summary>
/// Information used to create a 2D texture object
/// </summary>
/// <remarks>
/// <para>
/// This provides an immutable view of the texture information so that it cannot be modified after the texture is created
/// </para>
/// </remarks>
public interface IGorgonTexture2DInfo
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
    /// Property to return the number of array levels for a texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is greater than 0, the texture will be used as a texture array. If the texture is supposed to be a cube map, then this value should be a multiple of 6 (1 for each face in the cube).
    /// </para>
    /// <para>
    /// This value is defaulted to 1.
    /// </para>
    /// </remarks>
    int ArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to return whether this 2D texture is a cube map.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is set to <b>true</b>, then the texture is defined as a cube map using the <see cref="ArrayCount"/> as the number of faces. Because of this, the <see cref="ArrayCount"/> value 
    /// must be a multiple of 6. If it is not, then the array count will be adjusted to meet the requirement.
    /// </para>
    /// <para>
    /// This value is defaulted to <b>false</b>.
    /// </para>
    /// </remarks>
    bool IsCubeMap
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
    /// Property to return the multisample quality and count for this texture.
    /// </summary>
    /// <remarks>
    /// This value is defaulted to <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.
    /// </remarks>
    GorgonMultisampleInfo MultisampleInfo
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

    /// <summary>
    /// Property to return whether this texture can be shared with other graphics interfaces.
    /// </summary>
    /// <remarks>
    /// Settings this flag to <b>true</b> allows the texture to be used with external graphics interfaces such as a Direct3D device. This is useful for providing interoperation between systems.
    /// </remarks>
    TextureSharingOptions Shared
    {
        get;
    }
}
