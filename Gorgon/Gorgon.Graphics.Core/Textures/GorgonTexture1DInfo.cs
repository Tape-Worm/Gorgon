
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
// Created: June 13, 2016 8:45:14 PM
// 

using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information used to create a 1D texture object
/// </summary>
/// <param name="Width">The width of the texture, in pixels.</param>
/// <param name="Format">The texel format for the texture.</param>
public record GorgonTexture1DInfo(int Width, BufferFormat Format)
    : IGorgonTexture1DInfo, IGorgonImageInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture1DInfo"/> class.
    /// </summary>
    /// <param name="info">A <see cref="IGorgonTexture1DInfo"/> to copy settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonTexture1DInfo(IGorgonTexture1DInfo info)
        : this(info?.Width ?? throw new ArgumentNullException(nameof(info)), info.Format)
    {
        Name = info.Name;
        ArrayCount = info.ArrayCount;
        Binding = info.Binding;
        MipLevels = info.MipLevels;
        Usage = info.Usage;
    }

    /// <summary>
    /// Property to return the name of the texture.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = GorgonGraphicsResource.GenerateName(GorgonTexture1D.NamePrefix);

    /// <summary>
    /// Property to return the number of array levels for a texture.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is greater than 0, the texture will be used as a texture array. 
    /// </para>
    /// <para>
    /// This value is defaulted to 1.
    /// </para>
    /// </remarks>
    public int ArrayCount
    {
        get;
        init;
    } = 1;

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
    public int MipLevels
    {
        get;
        init;
    } = 1;

    /// <summary>
    /// Property to return the intended usage flags for this texture.
    /// </summary>
    /// <remarks>
    /// This value is defaulted to <c>Default</c>.
    /// </remarks>
    public ResourceUsage Usage
    {
        get;
        init;
    } = ResourceUsage.Default;

    /// <summary>
    /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <see cref="Usage"/> property is set to <c>Staging</c>, then the texture must be created with a value of <see cref="TextureBinding.None"/> as staging textures do not 
    /// support bindings of any kind. If this value is set to anything other than <see cref="TextureBinding.None"/>, an exception will be thrown.
    /// </para>
    /// <para>
    /// This value is defaulted to <see cref="TextureBinding.ShaderResource"/>. 
    /// </para>
    /// </remarks>
    public TextureBinding Binding
    {
        get;
        init;
    } = TextureBinding.ShaderResource;

    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    ImageDataType IGorgonImageInfo.ImageType => ImageDataType.Image1D;

    /// <summary>
    /// Property to return the height of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 2D and 3D images only.  This parameter will be set to a value of 1 for a 1D image.
    /// </para>
    /// </remarks>
    int IGorgonImageInfo.Height => 1;

    /// <summary>
    /// Property to return the depth of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 3D images only.  This parameter will be set to a value of 1 for a 1D or 2D image.
    /// </para>
    /// </remarks>
    int IGorgonImageInfo.Depth => 1;

    /// <summary>
    /// Property to return whether the image data is using premultiplied alpha.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value has no meaning for this type.
    /// </para>
    /// </remarks>
    bool IGorgonImageInfo.HasPreMultipliedAlpha => false;

    /// <summary>
    /// Property to return the number of mip map levels in the image.
    /// </summary>
    int IGorgonImageInfo.MipCount => MipLevels;

    /// <summary>
    /// Property to return whether the size of the texture is a power of 2 or not.
    /// </summary>
    bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0);

}