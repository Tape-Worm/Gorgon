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
// Created: December 27, 2020 4:23:52 PM
// 

namespace Gorgon.Graphics.Imaging;

/// <summary>
/// Functionality for editing the image using a fluent interface
/// </summary>
public interface IGorgonImageUpdateFluent
{
    /// <summary>
    /// Function to generate a new mip map chain.
    /// </summary>
    /// <param name="mipCount">The number of mip map levels.</param>
    /// <param name="filter">[Optional] The filter to apply when copying the data from one mip level to another.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the updated mip map data.</returns>
    /// <remarks>
    /// <para>
    /// This method will generate a new mip map chain for the <paramref name="mipCount"/>. If the current number of mip maps is not the same as the requested number, then the image buffer will be 
    /// adjusted to use the requested number of mip maps. If 0 is passed to <paramref name="mipCount"/>, then a full mip map chain is generated.
    /// </para>
    /// <para>
    /// Note that the <paramref name="mipCount"/> may not be honored depending on the current width, height, and depth of the image. Check the width, height and/or depth property on the returned 
    /// <see cref="IGorgonImage"/> to determine how many mip levels were actually generated.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent GenerateMipMaps(int mipCount, ImageFilter filter = ImageFilter.Point);

    /// <summary>
    /// Function to crop the image to the rectangle passed to the parameters.
    /// </summary>
    /// <param name="cropRect">The rectangle that will be used to crop the image.</param>
    /// <param name="newDepth">[Optional] The new depth for the image (for <see cref="ImageDataType.Image3D"/> images).</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the resized image.</returns>
    /// <remarks>
    /// <para>
    /// This method will crop the existing image a smaller version of itself as a new <see cref="IGorgonImage"/>. If the sizes are the same, or the <paramref name="cropRect"/> is larger than the size 
    /// of the original image, then no changes will be made.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent Crop(GorgonRectangle cropRect, int? newDepth = null);

    /// <summary>
    /// Function to expand an image width, height, and/or depth.
    /// </summary>
    /// <param name="newWidth">The new width of the image.</param>
    /// <param name="newHeight">The new height of the image.</param>
    /// <param name="newDepth">[Optional] The new depth of the image.</param>
    /// <param name="offset">The offset within the newly resized image that the original image be located at.</param>
    /// <returns>The expanded image.</returns>
    /// <remarks>
    /// <para>
    /// This will expand the size of an image, but not stretch the actual image data. This will leave a padding around the original image area filled with transparent pixels. 
    /// </para>
    /// <para>
    /// The image data can be repositioned in the new image by specifying an <paramref name="offset"/> point.
    /// </para>
    /// <para>
    /// If the new size of the image is smaller than that of this image, then the new size is constrained to the old size. Cropping is not supported by this method. 
    /// </para>
    /// <para>
    /// If a user wishes to resize the image, then call the <see cref="Resize"/> method, of if they wish to crop an image, use the <see cref="Crop"/> method.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent Expand(int newWidth, int newHeight, int? newDepth = null, GorgonPoint? offset = null);

    /// <summary>
    /// Function to resize the image to a new width, height and/or depth.
    /// </summary>
    /// <param name="newWidth">The new width for the image.</param>
    /// <param name="newHeight">The new height for the image (for <see cref="ImageDataType.Image2D"/> and <see cref="ImageDataType.ImageCube"/> images).</param>
    /// <param name="newDepth">[Optional] The new depth for the image (for <see cref="ImageDataType.Image3D"/> images).</param>
    /// <param name="filter">[Optional] The type of filtering to apply to the scaled image to help smooth larger and smaller images.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the resized image.</returns>
    /// <remarks>
    /// <para>
    /// This method will change the size of an existing image and return a larger or smaller version of itself as a new <see cref="IGorgonImage"/>. 
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent Resize(int newWidth, int newHeight, int? newDepth = null, ImageFilter filter = ImageFilter.Point);

    /// <summary>
    /// Function to convert the pixel format of an image into another pixel format.
    /// </summary>
    /// <param name="format">The new pixel format for the image.</param>
    /// <param name="dithering">[Optional] Flag to indicate the type of dithering to perform when the bit depth for the <paramref name="format"/> is lower than the original bit depth.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data with the converted pixel format.</returns>
    /// <remarks>
    /// <para>
    /// Use this to convert an image format from one to another. The conversion functionality uses Windows Imaging Components (WIC) to perform the conversion.
    /// </para>
    /// <para>
    /// Because this method uses WIC, not all formats will be convertible. To determine if a format can be converted, use the <see cref="GorgonImage.CanConvertToFormat"/> method. 
    /// </para>
    /// <para>
    /// For the <see cref="BufferFormat.B4G4R4A4_UNorm"/> format, Gorgon has to perform a manual conversion since that format is not supported by WIC. Because of this, the 
    /// <paramref name="dithering"/> flag will be ignored when downsampling to that format.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent ConvertToFormat(BufferFormat format, ImageDithering dithering = ImageDithering.None);

    /// <summary>
    /// Function to convert the image data from a premultiplied format.
    /// </summary>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data with the premultiplied alpha removed from the pixel data.</returns>
    /// <remarks>
    /// <para>
    /// Use this to convert an image from a premultiplied format. This takes each Red, Green and Blue element and divides them by the Alpha element.
    /// </para>
    /// <para>
    /// If the image does not contain alpha then the method will return right away and no alterations to the image will be performed.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent ConvertFromPremultipedAlpha();

    /// <summary>
    /// Function to convert the image data into a premultiplied format.
    /// </summary>
    /// <returns>A <see cref="IGorgonImage"/> containing the image data with the premultiplied alpha pixel data.</returns>
    /// <remarks>
    /// <para>
    /// Use this to convert an image to a premultiplied format. This takes each Red, Green and Blue element and multiplies them by the Alpha element.
    /// </para>
    /// <para>
    /// If the image does not contain alpha then the method will return right away and no alterations to the image will be performed.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent ConvertToPremultipliedAlpha();

    /// <summary>
    /// Function to finalize the update of the image and apply all changes.
    /// </summary>
    /// <param name="cancel">[Optional] <b>true</b> to cancel the operations, or <b>false</b> to commit them.</param>
    /// <returns>The original interface for the image that was updated.</returns>
    IGorgonImage EndUpdate(bool cancel = false);
}
