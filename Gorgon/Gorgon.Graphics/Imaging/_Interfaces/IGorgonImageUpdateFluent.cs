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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
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
/// Functionality for editing an image using a fluent interface.
/// </summary>
/// <remarks>
/// <para>
/// This interface is returned by <see cref="IGorgonImage.BeginUpdate"/>. Calling an operation records it, and does not change the image. The recorded operations are applied to the image, in the order that 
/// they were called, when <see cref="EndUpdate"/> is called, or they are discarded when <see cref="EndUpdate"/> is called with its <c>cancel</c> parameter set to <b>true</b>.
/// </para>
/// <para>
/// The parameters for an operation are validated when the operation is called. Errors that happen while an operation is applied (e.g. a failure in the Windows Imaging Component) are thrown from 
/// <see cref="EndUpdate"/>.
/// </para>
/// </remarks>
/// <seealso cref="IGorgonImage.BeginUpdate"/>
public interface IGorgonImageUpdateFluent
{
    /// <summary>
    /// Function to generate a new mip map chain.
    /// </summary>
    /// <param name="mipCount">The number of mip map levels.</param>
    /// <param name="filter">[Optional] The filter to apply when copying the data from one mip level to another.</param>
    /// <returns>The fluent interface for the image update.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <exception cref="Core.GorgonException">Thrown when the image update has already ended.</exception>
    /// <remarks>
    /// <para>
    /// This method will generate a new mip map chain from the first mip map level of the image, and replaces any existing mip map levels. If the <paramref name="mipCount"/> is 0 or less, or larger than the 
    /// number of mip map levels that the width, height and depth of the image can hold, then a full mip map chain is generated.
    /// </para>
    /// <para>
    /// If the <paramref name="mipCount"/> is 1, then the mip map chain is trimmed down to the first mip map level. Check the <see cref="IGorgonImageInfo.MipCount"/> property of the image after calling 
    /// <see cref="EndUpdate"/> to determine how many mip levels were actually generated.
    /// </para>
    /// <para>
    /// The default value for the <paramref name="filter"/> is <see cref="ImageFilter.Point"/>.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent GenerateMipMaps(int mipCount, ImageFilter filter = ImageFilter.Point);

    /// <summary>
    /// Function to crop the image to the rectangle passed to the parameters.
    /// </summary>
    /// <param name="cropRect">The rectangle that will be used to crop the image.</param>
    /// <param name="newDepth">[Optional] The new depth for the image (for <see cref="ImageDataType.Image3D"/> images).</param>
    /// <returns>The fluent interface for the image update.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="newDepth"/> is less than 1 for a <see cref="ImageDataType.Image3D"/> image.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <exception cref="Core.GorgonException">Thrown when the image update has already ended.</exception>
    /// <remarks>
    /// <para>
    /// This method will crop the existing image to a smaller version of itself. The <paramref name="cropRect"/> is clipped to the boundaries of the image, so a rectangle that extends past the edges of the 
    /// image only keeps the part that overlaps the image. For a <see cref="ImageDataType.Image1D"/> image, only the horizontal position and width of the <paramref name="cropRect"/> are used.
    /// </para>
    /// <para>
    /// If the <paramref name="cropRect"/> covers the entire image (and the depth is unchanged), or does not overlap the image at all, then no changes will be made.
    /// </para>
    /// <para>
    /// The default value for the <paramref name="newDepth"/> is the current depth of the image.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent Crop(GorgonRectangle cropRect, int? newDepth = null);

    /// <summary>
    /// Function to expand an image width, height, and/or depth.
    /// </summary>
    /// <param name="newWidth">The new width of the image.</param>
    /// <param name="newHeight">The new height of the image (ignored for <see cref="ImageDataType.Image1D"/> images).</param>
    /// <param name="newDepth">[Optional] The new depth of the image (for <see cref="ImageDataType.Image3D"/> images).</param>
    /// <param name="offset">[Optional] The position of the original image within the expanded image.</param>
    /// <returns>The fluent interface for the image update.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <exception cref="Core.GorgonException">Thrown when the image update has already ended.</exception>
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
    /// If a user wishes to resize the image, then call the <see cref="Resize"/> method, or if they wish to crop an image, use the <see cref="Crop"/> method.
    /// </para>
    /// <para>
    /// The default values are the current depth of the image for the <paramref name="newDepth"/>, and (0, 0) for the <paramref name="offset"/>.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent Expand(int newWidth, int newHeight, int? newDepth = null, GorgonPoint? offset = null);

    /// <summary>
    /// Function to resize the image to a new width, height and/or depth.
    /// </summary>
    /// <param name="newWidth">The new width for the image.</param>
    /// <param name="newHeight">The new height for the image (ignored for <see cref="ImageDataType.Image1D"/> images).</param>
    /// <param name="newDepth">[Optional] The new depth for the image (for <see cref="ImageDataType.Image3D"/> images).</param>
    /// <param name="filter">[Optional] The type of filtering to apply to the scaled image to help smooth larger and smaller images.</param>
    /// <returns>The fluent interface for the image update.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><para>
    /// Thrown when the <paramref name="newWidth"/> is less than 1.
    /// </para>
    /// <para>
    /// Thrown when the <paramref name="newHeight"/> is less than 1 for a <see cref="ImageDataType.Image2D"/>, <see cref="ImageDataType.ImageCube"/> or <see cref="ImageDataType.Image3D"/> image.
    /// </para>
    /// <para>
    /// Thrown when the <paramref name="newDepth"/> is less than 1 for a <see cref="ImageDataType.Image3D"/> image.
    /// </para>
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <exception cref="Core.GorgonException">Thrown when the image update has already ended.</exception>
    /// <remarks>
    /// <para>
    /// This method will change the size of the image, and scale the image data to fit the new size using the <paramref name="filter"/>. If the new size cannot hold the current number of mip map levels, then 
    /// the number of mip map levels is reduced.
    /// </para>
    /// <para>
    /// The default values are the current depth of the image for the <paramref name="newDepth"/>, and <see cref="ImageFilter.Point"/> for the <paramref name="filter"/>.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent Resize(int newWidth, int newHeight, int? newDepth = null, ImageFilter filter = ImageFilter.Point);

    /// <summary>
    /// Function to convert the pixel format of an image into another pixel format.
    /// </summary>
    /// <param name="format">The new pixel format for the image.</param>
    /// <param name="dithering">[Optional] Flag to indicate the type of dithering to perform when the bit depth for the <paramref name="format"/> is lower than the original bit depth.</param>
    /// <returns>The fluent interface for the image update.</returns>
    /// <exception cref="Core.GorgonException"><para>
    /// Thrown when the <paramref name="format"/> is <see cref="BufferFormat.Unknown"/>, or the image cannot be converted to the <paramref name="format"/>.
    /// </para>
    /// <para>
    /// Thrown when the image update has already ended.
    /// </para>
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <remarks>
    /// <para>
    /// Use this to convert an image format from one to another. The conversion functionality uses Windows Imaging Components (WIC) to perform the conversion.
    /// </para>
    /// <para>
    /// Because this method uses WIC, not all formats will be convertible. To determine if a format can be converted, use the <see cref="IGorgonImage.CanConvertToFormat"/> method.
    /// </para>
    /// <para>
    /// For the <see cref="BufferFormat.B4G4R4A4_UNorm"/>, or the <see cref="BufferFormat.A4B4G4R4_UNorm"/> format, Gorgon has to perform a manual conversion since those formats are not supported by WIC. 
    /// Because of this, the <paramref name="dithering"/> flag will be ignored when downsampling to those formats.
    /// </para>
    /// <para>
    /// The default value for the <paramref name="dithering"/> is <see cref="ImageDithering.None"/>.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent ConvertToFormat(BufferFormat format, ImageDithering dithering = ImageDithering.None);

    /// <summary>
    /// Function to convert the image data from a premultiplied format.
    /// </summary>
    /// <returns>The fluent interface for the image update.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <exception cref="Core.GorgonException">Thrown when the image update has already ended.</exception>
    /// <remarks>
    /// <para>
    /// Use this to convert an image from a premultiplied format. This takes each Red, Green and Blue element and divides them by the Alpha element.
    /// </para>
    /// <para>
    /// The conversion is applied whether or not the <see cref="IGorgonImageInfo.HasPremultipliedAlpha"/> property of the image is <b>true</b>, and that property is set to <b>false</b> afterward.
    /// </para>
    /// <para>
    /// If the image does not contain alpha then no alterations to the image will be performed.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent ConvertFromPremultipliedAlpha();

    /// <summary>
    /// Function to convert the image data into a premultiplied format.
    /// </summary>
    /// <returns>The fluent interface for the image update.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <exception cref="Core.GorgonException">Thrown when the image update has already ended.</exception>
    /// <remarks>
    /// <para>
    /// Use this to convert an image to a premultiplied format. This takes each Red, Green and Blue element and multiplies them by the Alpha element.
    /// </para>
    /// <para>
    /// The conversion is applied whether or not the <see cref="IGorgonImageInfo.HasPremultipliedAlpha"/> property of the image is <b>false</b>, and that property is set to <b>true</b> afterward.
    /// </para>
    /// <para>
    /// If the image does not contain alpha then no alterations to the image will be performed.
    /// </para>
    /// </remarks>
    IGorgonImageUpdateFluent ConvertToPremultipliedAlpha();

    /// <summary>
    /// Function to finalize the update of the image and apply all changes.
    /// </summary>
    /// <param name="cancel">[Optional] <b>true</b> to cancel the operations, or <b>false</b> to commit them.</param>
    /// <returns>The image that was updated.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    /// <remarks>
    /// <para>
    /// This applies the recorded operations to the image in the order that they were called, or discards them if <paramref name="cancel"/> is <b>true</b>. Any exception thrown while an operation is applied is 
    /// thrown from this method, and the operations queued after it are discarded.
    /// </para>
    /// <para>
    /// The update ends when this method returns, even if an operation throws an exception, and <see cref="IGorgonImage.BeginUpdate"/> may be called again.
    /// </para>
    /// <para>
    /// The default value for the <paramref name="cancel"/> is <b>false</b>.
    /// </para>
    /// </remarks>
    IGorgonImage EndUpdate(bool cancel = false);
}
