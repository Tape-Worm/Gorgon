// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: May 6, 2025 11:13:30 PM
//

using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// Defines the properties used to create a <see cref="IGorgonImage"/>.
/// </summary>
public sealed record class GorgonImageInfo
{
    /// <summary>
    /// An empty image information record.
    /// </summary>
    public static readonly GorgonImageInfo Empty = new(ImageDataType.Unknown, BufferFormat.Unknown, 0, 0, 0, 0, 0, false);

    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    public ImageDataType ImageType
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the pixel format for an image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the value is set to <see cref="BufferFormat.Unknown"/>, then an exception will be thrown upon image creation.
    /// </para>
    /// </remarks>
    public BufferFormat Format
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the width of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// A value of less than 1 will throw an exception upon creation.
    /// </remarks>
    public int Width
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the height of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 2D and 3D images only. This parameter will be set to a value of 1 for a 1D image.
    /// </para>
    /// <para>
    /// For applicable image types, a value of less than 1 will throw an exception upon creation.
    /// </para>
    /// </remarks>
    public int Height
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
    /// For applicable image types, a value of less than 1 will throw an exception upon creation.
    /// </para>
    /// </remarks>
    public int Depth
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the total number of images there are in an image array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
    /// </para>
    /// <para>
    /// If the <see cref="ImageType"/> is <see cref="ImageDataType.ImageCube"/>, then this value should be set to a multiple of 6. If it is not, then Gorgon will adjust this value to be a multiple of 
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
    /// Property to return whether the image data is using premultiplied alpha.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Premultiplied alpha is used to display correct alpha blending. This flag indicates that the data in the image has already been transformed to use premultiplied alpha.
    /// </para>
    /// <para>
    /// For more information see: <a href="https://blogs.msdn.microsoft.com/shawnhar/2009/11/06/premultiplied-alpha/">Shawn Hargreaves Blog</a>
    /// </para>
    /// </remarks>
    public bool IsPremultiplied
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
    /// Function to create a new <see cref="GorgonImageInfo"/> record for a 1D image.
    /// </summary>
    /// <param name="format">The pixel format of the image.</param>
    /// <param name="width">The width of the 1D image, in pixels.</param>
    /// <param name="arrayCount">[Optional] The number of array indices in the image.</param>
    /// <param name="mipCount">[Optional] The number of mip levels in the image.</param>
    /// <param name="isPremultiplied">[Optional] <b>true</b> if the image will contain premultiplied alpha, or <b>false</b> if it does not.</param>
    /// <returns>A new <see cref="GorgonImageInfo"/> containing the parameters to create a new 1D <see cref="IGorgonImage"/>.</returns>
    /// <remarks>
    /// <para>
    /// A 1D image only has a <paramref name="width"/> value, and contains a linear set of pixels across that width. Ideally these image types are used for look up data.
    /// </para>
    /// <para>
    /// Storage for mip map levels, and array indices in the image can be specified by the <paramref name="mipCount"/> and <paramref name="arrayCount"/> parameters. These are useful for for images that get 
    /// used as textures. If these values are omitted, only the top mip level and a single array index is allocated (a <see cref="MipCount"/> of 1 and <see cref="ArrayCount"/> of 1).
    /// </para>
    /// <para>
    /// The <paramref name="isPremultiplied"/> flag is used to notify that the image data has already had its colour components multiplied by the alpha value. This only applies on image data where an alpha 
    /// component is present (e.g. <see cref="BufferFormat.R8G8B8A8_UInt"/>).
    /// </para>
    /// <para>
    /// The following properties will throw an exception upon <see cref="IGorgonImage"/> creation (i.e. not on creation of this record):
    /// <list type="table">
    /// <item>
    /// <term><paramref name="format"/></term>
    /// <description>If the value is <see cref="BufferFormat.Unknown"/>.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="width"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="arrayCount"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="mipCount"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="MipCount"/>
    /// <seealso cref="ArrayCount"/>
    public static GorgonImageInfo Create1DImageInfo(BufferFormat format, int width, int arrayCount = 1, int mipCount = 1, bool isPremultiplied = false) => new(ImageDataType.Image1D, format, width, 1, 1, arrayCount, mipCount, isPremultiplied);

    /// <summary>
    /// Function to create a new <see cref="GorgonImageInfo"/> record for a 2D image.
    /// </summary>
    /// <param name="format">The pixel format of the image.</param>
    /// <param name="width">The width of the 2D image, in pixels.</param>
    /// <param name="height">The height of the 2D image, in pixels.</param>
    /// <param name="arrayCount">[Optional] The number of array indices in the image.</param>
    /// <param name="mipCount">[Optional] The number of mip levels in the image.</param>
    /// <param name="isPremultiplied">[Optional] <b>true</b> if the image will contain premultiplied alpha, or <b>false</b> if it does not.</param>
    /// <returns>A new <see cref="GorgonImageInfo"/> containing the parameters to create a new 2D <see cref="IGorgonImage"/>.</returns>
    /// <remarks>
    /// <para>
    /// A 2D image only has a <paramref name="width"/>, and <paramref name="height"/> value. This image type is the most commonly used one.
    /// </para>
    /// <para>
    /// Storage for mip map levels, and array indices in the image can be specified by the <paramref name="mipCount"/> and <paramref name="arrayCount"/> parameters. These are useful for for images that get 
    /// used as textures. If these values are omitted, only the top mip level and a single array index is allocated (a <see cref="MipCount"/> of 1 and <see cref="ArrayCount"/> of 1).
    /// </para>
    /// <para>
    /// The <paramref name="isPremultiplied"/> flag is used to notify that the image data has already had its colour components multiplied by the alpha value. This only applies on image data where an alpha 
    /// component is present (e.g. <see cref="BufferFormat.R8G8B8A8_UInt"/>).
    /// </para>
    /// <para>
    /// The following properties will throw an exception upon <see cref="IGorgonImage"/> creation (i.e. not on creation of this record):
    /// <list type="table">
    /// <item>
    /// <term><paramref name="format"/></term>
    /// <description>If the value is <see cref="BufferFormat.Unknown"/>.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="width"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="height"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="arrayCount"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="mipCount"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="MipCount"/>
    /// <seealso cref="ArrayCount"/>
    public static GorgonImageInfo Create2DImageInfo(BufferFormat format, int width, int height, int arrayCount = 1, int mipCount = 1, bool isPremultiplied = false) => new(ImageDataType.Image2D, format, width, height, 1, arrayCount, mipCount, isPremultiplied);

    /// <summary>
    /// Function to create a new <see cref="GorgonImageInfo"/> record for a cube image.
    /// </summary>
    /// <param name="format">The pixel format of the image.</param>
    /// <param name="width">The width of the 2D image, in pixels.</param>
    /// <param name="height">The height of the 2D image, in pixels.</param>
    /// <param name="cubeCount">The number of cubes stored in the image.</param>[
    /// <param name="mipCount">[Optional] The number of mip levels in the image.</param>
    /// <param name="isPremultiplied">[Optional] <b>true</b> if the image will contain premultiplied alpha, or <b>false</b> if it does not.</param>
    /// <returns>A new <see cref="GorgonImageInfo"/> containing the parameters to create a new 2D <see cref="IGorgonImage"/> cube.</returns>
    /// <remarks>
    /// <para>
    /// A cube image only has a <paramref name="width"/>, <paramref name="height"/>, and <paramref name="cubeCount"/> value. This image type is a 2D image type, with array count that is a multiple of 6. Each 
    /// array index represents a face on a cube and is often used for environment mapping techniques.
    /// </para>
    /// <para>
    /// The <paramref name="cubeCount"/> value represents how many cubes are in the image. This value is multiplied by 6 and returned on the <see cref="ArrayCount"/> value as the total number 
    /// of faces. For example, calling this method with a <paramref name="cubeCount"/> of 3 will result in an image with an <see cref="ArrayCount"/> of 18.
    /// </para>
    /// <para>
    /// Storage for mip map levels in the image can be specified by the <paramref name="mipCount"/> parameter. If this value is omitted, only the top mip level is allocated (a <see cref="MipCount"/> of 1).
    /// </para>
    /// <para>
    /// The <paramref name="isPremultiplied"/> flag is used to notify that the image data has already had its colour components multiplied by the alpha value. This only applies on image data where an alpha 
    /// component is present (e.g. <see cref="BufferFormat.R8G8B8A8_UInt"/>).
    /// </para>
    /// <para>
    /// The following properties will throw an exception upon <see cref="IGorgonImage"/> creation (i.e. not on creation of this record):
    /// <list type="table">
    /// <item>
    /// <term><paramref name="format"/></term>
    /// <description>If the value is <see cref="BufferFormat.Unknown"/>.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="width"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="height"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="cubeCount"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="mipCount"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="MipCount"/>
    /// <seealso cref="ArrayCount"/>
    public static GorgonImageInfo CreateCubeImageInfo(BufferFormat format, int width, int height, int cubeCount, int mipCount = 1, bool isPremultiplied = false) => new(ImageDataType.ImageCube, format, width, height, 1, cubeCount * 6, mipCount, isPremultiplied);

    /// <summary>
    /// Function to create a new <see cref="GorgonImageInfo"/> record for a 3D image.
    /// </summary>
    /// <param name="format">The pixel format of the image.</param>
    /// <param name="width">The width of the 3D image, in pixels.</param>
    /// <param name="height">The height of the 3D image, in pixels.</param>
    /// <param name="depth">The depth of the 3D image, in slices.</param>
    /// <param name="mipCount">[Optional] The number of mip levels in the image.</param>
    /// <returns>A new <see cref="GorgonImageInfo"/> containing the parameters to create a new 3D <see cref="IGorgonImage"/>.</returns>
    /// <remarks>
    /// <para>
    /// A 2D image only has a <paramref name="width"/>, <paramref name="height"/>, and <paramref name="depth"/> value. This image type is used in volumetric rendering (e.g. clouds).
    /// </para>
    /// <para>
    /// Storage for mip map levels in the image can be specified by the <paramref name="mipCount"/> parameter. These are useful for for images that get used as textures. If this value is omitted, only the 
    /// top mip level is allocated (a <see cref="MipCount"/> of 1).
    /// </para>
    /// <para>
    /// The following properties will throw an exception upon <see cref="IGorgonImage"/> creation (i.e. not on creation of this record):
    /// <list type="table">
    /// <item>
    /// <term><paramref name="format"/></term>
    /// <description>If the value is <see cref="BufferFormat.Unknown"/>.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="width"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="height"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="depth"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// <item>
    /// <term><paramref name="mipCount"/></term>
    /// <description>If the value is less than or equal to 0.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonImage"/>
    /// <seealso cref="MipCount"/>
    public static GorgonImageInfo Create3DImageInfo(BufferFormat format, int width, int height, int depth, int mipCount = 1) => new(ImageDataType.Image2D, format, width, height, depth, 1, mipCount, false);

    /// <summary>
    /// Function to calculate the maximum number of available mip map levels for this image.
    /// </summary>
    /// <param name="width">The width of the image, in pixels.</param>
    /// <param name="height">The height of the image, in pixels.</param>
    /// <param name="depth">The depth of the image, in depth slices. Leave at 1 for 1D or 2D images.</param>
    /// <returns>The maximum number of mip map levels for the image.</returns>
    public static int GetMaximumMipCount(int width, int height, int depth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(depth, 1);

        int result = 1;

        while ((width > 1) || (height > 1) || (depth > 1))
        {
            if (width > 1)
            {
                width >>= 1;
            }
            if (height > 1)
            {
                height >>= 1;
            }
            if (depth > 1)
            {
                depth >>= 1;
            }

            result++;
        }

        return result;
    }

    /// <summary>
    /// Function to return the size of an image in bytes.
    /// </summary>
    /// <param name="imageType">The type of image.</param>
    /// <param name="width">Width of the image.</param>
    /// <param name="height">Height of the image.</param>
    /// <param name="arrayCountOrDepth">The number of array indices for <see cref="ImageDataType.Image1D"/> or <see cref="ImageDataType.Image2D"/> images, or the number of depth slices for a <see cref="ImageDataType.Image3D"/>.</param>
    /// <param name="format">Format of the image.</param>
    /// <param name="mipCount">[Optional] Number of mip-map levels in the image.</param>
    /// <param name="pitchFlags">[Optional] Flags used to influence the row pitch of the image.</param>
    /// <returns>The number of bytes for the image.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the <paramref name="width"/>, <paramref name="height"/>, <paramref name="arrayCountOrDepth"/>, or <paramref name="mipCount"/> values 
    /// are less than 1.</exception>
    /// <exception cref="NotSupportedException">Thrown when the <paramref name="format"/> is not supported.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="pitchFlags"/> parameter is used to compensate in cases where the original image data is not laid out correctly (such as with older DirectDraw DDS images).
    /// </para>
    /// </remarks>
    public static long CalculateSizeInBytes(ImageDataType imageType, int width, int height, int arrayCountOrDepth, BufferFormat format, int mipCount = 1, PitchFlags pitchFlags = PitchFlags.None)
    {
        if (imageType is ImageDataType.Unknown)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_IMAGE_TYPE_UNKNOWN, imageType));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(arrayCountOrDepth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(mipCount, 1);

        GorgonFormatInfo formatInfo = new(format);
        long result = 0;

        if (formatInfo.SizeInBytes == 0)
        {
            throw new NotSupportedException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
        }

        int arrayCount = imageType == ImageDataType.Image3D ? 1 : arrayCountOrDepth;
        int depthCount = imageType == ImageDataType.Image3D ? arrayCountOrDepth : 1;
        int mipLevels = mipCount.Min(GetMaximumMipCount(width, height, depthCount));

        for (int array = 0; array < arrayCount; ++array)
        {
            int mipWidth = width;
            int mipHeight = height;

            for (int mip = 0; mip < mipLevels; mip++)
            {
                GorgonPitchLayout pitchInfo = formatInfo.GetPitchForFormat(mipWidth, mipHeight, pitchFlags);
                result += (long)pitchInfo.SlicePitch * depthCount;

                if (mipWidth > 1)
                {
                    mipWidth >>= 1;
                }

                if (mipHeight > 1)
                {
                    mipHeight >>= 1;
                }

                if (depthCount > 1)
                {
                    depthCount >>= 1;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Function to return the number of depth slices for a 3D image with the given number of mip maps.
    /// </summary>
    /// <param name="maximumSlices">The maximum desired number of depth slices to return.</param>
    /// <param name="mipCount">The number of mip maps to use.</param>
    /// <returns>The actual number of depth slices.</returns>
    public static int GetMaximumDepthSliceCount(int maximumSlices, int mipCount)
    {
        if (mipCount < 2)
        {
            return maximumSlices;
        }

        int bufferCount = 0;
        int depth = maximumSlices;

        for (int i = 0; i < mipCount; i++)
        {
            bufferCount += depth;

            if (depth > 1)
            {
                depth >>= 1;
            }
        }

        return bufferCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImageInfo"/> record.
    /// </summary>
    /// <param name="imageType">The type of image.</param>
    /// <param name="format">The pixel format of the image.</param>
    /// <param name="width">The width, in pixels, of the image.</param>
    /// <param name="height">The height, in pixels of the image.</param>
    /// <param name="depth">The depth, in slices, of the image.</param>
    /// <param name="arrayCount">The number of array indices in the image.</param>
    /// <param name="mipCount">The number of mip maps in the image.</param>
    /// <param name="premultiplied"><b>true</b> if the data in the image uses premultiplied alpha, or <b>false</b> if not.</param>
    private GorgonImageInfo(ImageDataType imageType, BufferFormat format, int width, int height, int depth, int arrayCount, int mipCount, bool premultiplied)
    {
        ImageType = imageType;
        Format = format;
        Width = width;
        Height = height;
        Depth = depth;
        ArrayCount = arrayCount;
        MipCount = mipCount;
        IsPremultiplied = premultiplied;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImageInfo"/> record.
    /// </summary>
    /// <param name="imageInfo">The source image info record to copy from.</param>
    public GorgonImageInfo(GorgonImageInfo imageInfo)
    {
        ImageType = imageInfo.ImageType;
        Format = imageInfo.Format;
        Width = imageInfo.Width;
        Height = imageInfo.Height;
        Depth = imageInfo.Depth;
        ArrayCount = imageInfo.ArrayCount;
        MipCount = imageInfo.MipCount;
        IsPremultiplied = imageInfo.IsPremultiplied;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonImageInfo"/> record.
    /// </summary>
    /// <param name="imageInfo">The source image info record to copy from.</param>
    public GorgonImageInfo(IGorgonImageInfo imageInfo)
    {
        ImageType = imageInfo.ImageType;
        Format = imageInfo.Format;
        Width = imageInfo.Width;
        Height = imageInfo.Height;
        Depth = imageInfo.Depth;
        ArrayCount = imageInfo.ArrayCount;
        MipCount = imageInfo.MipCount;
        IsPremultiplied = imageInfo.HasPremultipliedAlpha;
    }
}
