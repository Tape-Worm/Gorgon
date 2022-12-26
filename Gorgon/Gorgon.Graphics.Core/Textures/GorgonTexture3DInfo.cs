#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 17, 2018 12:40:18 PM
// 
#endregion

using System;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

#if NET6_0_OR_GREATER
/// <summary>
/// Information used to create a texture object.
/// </summary>
/// <param name="Width">The width of the texture, in pixels.</param>
/// <param name="Height">The height of the texture, in pixels.</param>
/// <param name="Depth">The depth of the texture, in pixels.</param>
/// <param name="Format">The format of the texel data.</param>
public record GorgonTexture3DInfo(int Width, int Height, int Depth, BufferFormat Format)
    : IGorgonTexture3DInfo, IGorgonImageInfo
{
#region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture3DInfo"/> class.
    /// </summary>
    /// <param name="info">A <see cref="IGorgonTexture3DInfo"/> to copy settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonTexture3DInfo(IGorgonTexture3DInfo info)
        : this(info?.Width ?? throw new ArgumentNullException(nameof(info)), info.Height, info.Depth, info.Format)
    {
        Name = info.Name;
        Binding = info.Binding;
        MipLevels = info.MipLevels;
        Usage = info.Usage;
    }
#endregion

#region Properties.
    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    ImageType IGorgonImageInfo.ImageType => ImageType.Image3D;

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
    /// Property to return the total number of images there are in an image array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
    /// </para>
    /// </remarks>
    int IGorgonImageInfo.ArrayCount => 1;

    /// <summary>
    /// Property to return whether the size of the texture is a power of 2 or not.
    /// </summary>
    bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                          && ((Height == 0) || (Height & (Height - 1)) == 0)
                                          && ((Depth == 0) || (Depth & (Depth - 1)) == 0);

    /// <summary>
    /// Property to return the name of the texture.
    /// </summary>
    public string Name
    {
        get;
        init;
    } = GorgonGraphicsResource.GenerateName(nameof(GorgonTexture3D));

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
#endregion
}
#else
/// <summary>
/// Information used to create a texture object.
/// </summary>
public class GorgonTexture3DInfo
    : IGorgonTexture3DInfo, IGorgonImageInfo
{
    #region Properties.
    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    ImageType IGorgonImageInfo.ImageType => ImageType.Image3D;

    /// <summary>
    /// Property to return whether the image data is using premultiplied alpha.
    /// </summary>
    bool IGorgonImageInfo.HasPreMultipliedAlpha => false;

    /// <summary>
    /// Property to return the number of mip map levels in the image.
    /// </summary>
    int IGorgonImageInfo.MipCount => MipLevels;

    /// <summary>
    /// Property to return the total number of images there are in an image array.
    /// </summary>
    int IGorgonImageInfo.ArrayCount => 1;

    /// <summary>
    /// Property to return whether the size of the texture is a power of 2 or not.
    /// </summary>
    bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                          && ((Height == 0) || (Height & (Height - 1)) == 0)
                                          && ((Depth == 0) || (Depth & (Depth - 1)) == 0);

    /// <summary>
    /// Property to return the name of the texture.
    /// </summary>
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the width of the texture, in pixels.
    /// </summary>
    public int Width
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the height of the texture, in pixels.
    /// </summary>
    public int Height
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the depth of the texture, in slices.
    /// </summary>
    public int Depth
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the format of the texture.
    /// </summary>
    public BufferFormat Format
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the number of mip-map levels for the texture.
    /// </summary>
    public int MipLevels
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the intended usage flags for this texture.
    /// </summary>
    public ResourceUsage Usage
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
    /// </summary>
    public TextureBinding Binding
    {
        get;
        set;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture3DInfo"/> class.
    /// </summary>
    /// <param name="info">A <see cref="IGorgonTexture3DInfo"/> to copy settings from.</param>
    /// <param name="newName">[Optional] The new name for the texture.</param>
    public GorgonTexture3DInfo(IGorgonTexture3DInfo info, string newName = null)
    {
        Name = newName;
        Format = info.Format;
        Depth = info.Depth;
        Binding = info.Binding;
        Height = info.Height;
        MipLevels = info.MipLevels;
        Usage = info.Usage;
        Width = info.Width;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTexture3DInfo"/> class.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="depth"></param>
    /// <param name="format"></param>
    public GorgonTexture3DInfo(int width, int height, int depth, BufferFormat format)
    {
        Width = width;
        Height = height;
        Depth = depth;
        Format = format;
    }
    #endregion
}
#endif
