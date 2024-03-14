#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 20, 2016 9:22:50 PM
// 
#endregion

using Gorgon.Math;

namespace Gorgon.Graphics.Imaging;

#if NET6_0_OR_GREATER
/// <summary>
/// A record used to define the properties of a <see cref="IGorgonImage"/>.
/// </summary>
/// <param name="ImageType">The type of image to build.</param>
/// <param name="Format">The pixel format layout of the data in the image.</param>
public record GorgonImageInfo(ImageType ImageType, BufferFormat Format)
    : IGorgonImageInfo
{
#region Constructor.
    /// <summary>
    /// A copy constructor for an <see cref="IGorgonImageInfo"/>.
    /// </summary>
    /// <param name="info">The information to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonImageInfo(IGorgonImageInfo info)
        : this(info?.ImageType ?? throw new ArgumentNullException(nameof(info)), info.Format)
    {
        ArrayCount = info.ArrayCount.Max(1);
        Depth = info.Depth;
        Height = info.Height;
        Width = info.Width;
        MipCount = info.MipCount.Max(1);
        HasPreMultipliedAlpha = info.HasPreMultipliedAlpha;
    }
#endregion

#region Properties.
    /// <summary>
    /// Property to return the total number of images there are in an image array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
    /// </para>
    /// <para>
    /// If the <see cref="IGorgonImageInfo.ImageType"/> is <see cref="ImageType.ImageCube"/>, then this value should be set to a multiple of 6. If it is not, then Gorgon will adjust this value to be a multiple of 
    /// 6 if this image is to be used as a cube map.
    /// </para>
    /// <para>
    /// This value will be reset to 1 if the value supplied is less than 1.
    /// </para>
    /// </remarks>
    public int ArrayCount
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the depth of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 3D images only.  This parameter will be set to a value of 1 for a 1D or 2D image.
    /// </para>
    /// <para>
    /// For applicable image types, a value of less than 1 will throw an exception upon image creation.
    /// </para>
    /// </remarks>
    public int Depth
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the height of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 2D and 3D images only.  This parameter will be set to a value of 1 for a 1D image.
    /// </para>
    /// <para>
    /// For applicable image types, a value of less than 1 will throw an exception upon image creation.
    /// </para>
    /// </remarks>
    public int Height
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return whether the size of the texture is a power of 2 or not.
    /// </summary>
    public bool IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0) &&
                                ((Height == 0) || (Height & (Height - 1)) == 0) &&
                                ((Depth == 0) || (Depth & (Depth - 1)) == 0);

    /// <summary>
    /// Property to return the number of mip map levels in the image.
    /// </summary>
    /// <remarks>
    /// If this value is set to 0, or less, then a full mip-map chain will be generated for the image.
    /// </remarks>
    public int MipCount
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the width of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// A value of less than 1 will throw an exception upon image creation.
    /// </remarks>
    public int Width
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return whether the image data is using premultiplied alpha.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Premultiplied alpha is used to display correct alpha blending. This flag indicates that the data in the image has already been transformed to use premultiplied alpha.
    /// </para>
    /// <para>
    /// For more information see: <a href="https://blogs.msdn.microsoft.com/shawnhar/2009/11/06/premultiplied-alpha/">Shawn Hargreaves Blog</a>
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    public bool HasPreMultipliedAlpha
    {
        get;
        internal init;
    }
#endregion
}
#else
#region Compatibility
/// <summary>
/// Provides information describing how to create an image.
/// </summary>
public class GorgonImageInfo
    : IGorgonImageInfo
{
    #region Properties.
    /// <summary>
    /// Property to return the total number of images there are in an image array.
    /// </summary>
    public int ArrayCount
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the depth of an image, in pixels.
    /// </summary>
    public int Depth
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the pixel format for an image.
    /// </summary>
    public BufferFormat Format
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the height of an image, in pixels.
    /// </summary>
    public int Height
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    public ImageType ImageType
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return whether the size of the texture is a power of 2 or not.
    /// </summary>
    public bool IsPowerOfTwo => false;

    /// <summary>
    /// Property to return the number of mip map levels in the image.
    /// </summary>
    public int MipCount
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the width of an image, in pixels.
    /// </summary>
    public int Width
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return whether the image data is using premultiplied alpha.
    /// </summary>
    public bool HasPreMultipliedAlpha
    {
        get;
        internal set;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImageInfo"/> class.
    /// </summary>
    /// <param name="imageType">The type of the image to create.</param>
    /// <param name="format">The format describing how a pixel is laid out in memory.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public GorgonImageInfo(ImageType imageType, BufferFormat format)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImageInfo"/> class.
    /// </summary>
    /// <param name="info">The initial image information to copy into this instance.</param>
    /// <param name="imageType">[Optional] An updated image type.</param>
    /// <param name="format">[Optional] An updated image pixel format.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public GorgonImageInfo(IGorgonImageInfo info, ImageType? imageType = null, BufferFormat? format = null)
    {
    }
    #endregion

}
#endregion
#endif
