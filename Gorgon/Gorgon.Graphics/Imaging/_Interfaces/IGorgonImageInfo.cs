
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 20, 2016 9:23:44 PM
// 

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// Type of image data.
/// </summary>
public enum ImageDataType
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Image is a 1 dimensional image.
    /// </summary>
    Image1D = 2,
    /// <summary>
    /// Image is a 2 dimensional image.
    /// </summary>
    Image2D = 3,
    /// <summary>
    /// Image is a 3 dimensional image.
    /// </summary>
    Image3D = 4,
    /// <summary>
    /// Image is a cube map.
    /// </summary>
    ImageCube = 0xFF
}

/// <summary>
/// Provides information about an image.
/// </summary>
public interface IGorgonImageInfo
{
    /// <summary>
    /// Property to return the type of image data.
    /// </summary>
    ImageDataType ImageType
    {
        get;
    }

    /// <summary>
    /// Property to return the width of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of less than 1 will throw an exception upon creation.
    /// </para>
    /// </remarks>
    int Width
    {
        get;
    }

    /// <summary>
    /// Property to return the height of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 2D, cube and 3D images only. This value will be set to 1 for a 1D image.
    /// </para>
    /// <para>
    /// For applicable image types, a value of less than 1 will throw an exception upon creation.
    /// </para>
    /// </remarks>
    int Height
    {
        get;
    }

    /// <summary>
    /// Property to return the depth of an image, in pixels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This applies to 3D images only. This value will be set to 1 for a 1D, 2D or cube image.
    /// </para>
    /// <para>
    /// For applicable image types, a value of less than 1 will throw an exception upon creation.
    /// </para>
    /// </remarks>
    int Depth
    {
        get;
    }

    /// <summary>
    /// Property to return the pixel format for an image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the value is set to <see cref="BufferFormat.Unknown"/>, then an exception will be thrown upon image creation.
    /// </para>
    /// </remarks>
    BufferFormat Format
    {
        get;
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
    bool HasPremultipliedAlpha
    {
        get;
    }

    /// <summary>
    /// Property to return the number of mip map levels in the image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of less than 1 will throw an exception upon creation. A value larger than the number of mip map levels that the width, height and depth of the image can hold is reduced to 
    /// that number (see <see cref="GorgonImageInfo.GetMaximumMipCount"/>).
    /// </para>
    /// </remarks>
    int MipCount
    {
        get;
    }

    /// <summary>
    /// Property to return whether the width, height and depth of the image are all powers of 2.
    /// </summary>
    bool IsPowerOfTwo
    {
        get;
    }

    /// <summary>
    /// Property to return the total number of images there are in an image array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to 1D, 2D and cube images. This value will be set to 1 for a 3D image.
    /// </para>
    /// <para>
    /// If the <see cref="ImageType"/> is <see cref="ImageDataType.ImageCube"/>, then this value must be a multiple of 6 (one array index per cube face). 
    /// <see cref="GorgonImageInfo.CreateCubeImageInfo"/> takes the number of cubes, and multiplies it by 6.
    /// </para>
    /// <para>
    /// A value of less than 1 will throw an exception upon creation.
    /// </para>
    /// </remarks>
    int ArrayCount
    {
        get;
    }
}
