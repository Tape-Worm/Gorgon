#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, January 31, 2013 8:29:10 AM
// 

// This code was adapted from the SharpDX ToolKit by Alexandre Mutel.
#region SharpDX License.
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

// SharpDX is available from http://sharpdx.org
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.UI;
using SharpDX.WIC;
using Bitmap = SharpDX.WIC.Bitmap;
using DX = SharpDX;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A container for raw image data that can be sent to or read from a image.
    /// </summary>
    /// <remarks>This object will allow pixel manipulation of image data.  It will break a image into buffers, such as a series of buffers 
    /// for arrays (for 1D and 2D images only), mip-map levels and depth slices (for 3D images only).
	/// <para>The object takes its settings from an object that implements <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see>.  All texture settings objects such as 
	/// <see cref="Gorgon.Graphics.GorgonTexture1DSettings">GorgonTexture1DSettings</see>, <see cref="Gorgon.Graphics.GorgonTexture2DSettings">GorgonTexture2DSettings</see> and 
	/// <see cref="Gorgon.Graphics.GorgonTexture3DSettings">GorgonTexture3DSettings</see> implement the IImageSettings interface and can be used with this object.</para>
	/// </remarks>
    public class GorgonImageData
        : IDisposable
	{
		#region Variables.
		private bool _disposed;									// Flag to indicate whether the object was disposed.
        private GorgonDataStream _imageData;					// Base image data buffer.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the pointer to the beginning of the internal buffer.
        /// </summary>
        public IntPtr BufferPointer
        {
            get
            {
                return _imageData.BaseIntPtr;
            }            
        }

        /// <summary>
        /// Property to return an unsafe pointer to the beginning of the internal buffer.
        /// </summary>
        public unsafe void *UnsafePointer
        {
            get
            {
                return _imageData.BasePointer;
            }
        }

        /// <summary>
        /// Property to return the settings for the image.
        /// </summary>
        public IImageSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of bytes, in total, that this image occupies.
        /// </summary>
        public int SizeInBytes
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the list of image buffers for this image.
		/// </summary>
	    public GorgonImageBufferList Buffers
	    {
		    get;
		    private set;
	    }
		#endregion

        #region Methods.
		/// <summary>
		/// Function to initialize the image data.
		/// </summary>
		/// <param name="data">Pre-existing data to use.</param>
		/// <param name="copy"><b>true</b> to copy the data, <b>false</b> to take ownership of the pointer.  Only applies when data is non-null.</param>
		private unsafe void Initialize(void* data, bool copy)
        {
            // Create a buffer large enough to hold our data.
            if (data == null)
            {
                _imageData = new GorgonDataStream(SizeInBytes);
				DirectAccess.ZeroMemory(_imageData.BasePointer, SizeInBytes);
            }
            else
            {
	            if (!copy)
	            {
		            _imageData = new GorgonDataStream(data, SizeInBytes);
	            }
	            else
	            {
		            _imageData = new GorgonDataStream(SizeInBytes);
					DirectAccess.MemoryCopy(_imageData.BasePointer, data, SizeInBytes);
	            }
            }

			Buffers = new GorgonImageBufferList(this);
			Buffers.CreateBuffers((byte *)_imageData.BasePointer);
        }

		/// <summary>
		/// Function to sanitize the image settings.
		/// </summary>
		private void SanitizeSettings()
        {
		    if (Settings.ImageType != ImageType.Image3D)
		    {
		        Settings.ArrayCount = 1.Max(Settings.ArrayCount);
		    }

		    Settings.Width = 1.Max(Settings.Width);
		    if ((Settings.ImageType == ImageType.Image2D)
		        || (Settings.ImageType == ImageType.ImageCube))
		    {
		        Settings.Height = 1.Max(Settings.Height);
		    }
		    if (Settings.ImageType == ImageType.Image3D)
		    {
		        Settings.Depth = 1.Max(Settings.Depth);
		    }

		    // Ensure mip values do not exceed more than what's available based on width, height and/or depth.
			if (Settings.MipCount > 1)
			{
				Settings.MipCount = Settings.MipCount.Min(GetMaxMipCount(Settings));
			}

			// Create mip levels if we didn't specify any.
			if (Settings.MipCount == 0)
			{
				Settings.MipCount = GetMaxMipCount(Settings);
			}

			// If we're an image cube, and we don't have an array count that's a multiple of 6, then up size until we do.
			if ((Settings.ImageType != ImageType.ImageCube) || ((Settings.ArrayCount % 6) == 0))
			{
				return;
			}

			while ((Settings.ArrayCount % 6) != 0)
			{
				Settings.ArrayCount++;
			}
        }

		/// <summary>
		/// Function to copy the image data to an existing texture.
		/// </summary>
		/// <param name="texture">Texture to copy into.</param>
		/// <remarks>This method will do a straight copy of the data into the specified texture.  This method will not perform format conversions, stretching/shrinking of the image data 
		/// to match the texture.  Clipping will occour if the texture width/height is not the same as the image data width/height.
		/// <para>This overload will copy all mip levels and array indices (if applicable).  If the texture has fewer array indices or mip levels than the image data, then 
		/// the data will be clipped to the lower mip/array count.</para>
		/// <para>Images and textures must be the same format, and share the same number of dimensions (i.e. 2D texture needs 2D image data, etc...).</para>
		/// <para>The texture to update must not have usage type of Immutable.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the texture and the image data do not have the same format.
		/// <para>-or-</para>
		/// <para>Thrown when the texture and the image data do not share the same number dimensions.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture passed in is immutable.</para>
		/// </exception>
		public void CopyToTexture(GorgonTexture texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}

			int arrayCount = texture.Settings.ArrayCount.Min(Settings.ArrayCount);
			int mipCount = texture.Settings.MipCount.Min(Settings.MipCount);

			// Copy to the texture.
			for (int array = 0; array < arrayCount; array++)
			{
				for (int mip = 0; mip < mipCount; mip++)
				{
					CopyToTexture(texture, array, mip);
				}
			}
		}

		/// <summary>
		/// Function to copy the image data to an existing texture.
		/// </summary>
		/// <param name="texture">Texture to copy into.</param>
		/// <param name="arrayIndex">Array index to copy.</param>
		/// <param name="mipLevel">Mip map level to copy.</param>
		/// <remarks>This method will do a straight copy of the data into the specified texture.  This method will not perform format conversions, stretching/shrinking of the image data 
		/// to match the texture.  Clipping will occour if the texture width/height is not the same as the image data width/height.
		/// <para>Images and textures must be the same format, and share the same number of dimensions (i.e. 2D texture needs 2D image data, etc...).</para>
		/// <para>The texture to update must not have usage type of Immutable.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the texture and the image data do not have the same format.
		/// <para>-or-</para>
		/// <para>Thrown when the texture and the image data do not share the same number dimensions.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture passed in is immutable.</para>
		/// </exception>
		public void CopyToTexture(GorgonTexture texture, int arrayIndex, int mipLevel)
		{
			int depth = 1;
			var flags = BufferLockFlags.Write;

		    if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}

            if (texture.Settings.Format != Settings.Format)
            {
                throw new ArgumentException(
                    string.Format(Resources.GORGFX_IMAGE_FORMAT_MISMATCH, texture.Settings.Format, Settings.Format),
                    "texture");
            }

            if (texture.Settings.ImageType != Settings.ImageType)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, texture.Settings.ImageType),
                    "texture");
            }

            if (texture.Settings.Usage == BufferUsage.Immutable)
            {
                throw new ArgumentException(Resources.GORGFX_TEXTURE_IMMUTABLE, "texture");
            }

			int arrayCount = Settings.ArrayCount.Min(texture.Settings.ArrayCount);
			int mipCount = Settings.MipCount.Min(texture.Settings.MipCount);

			// Ensure our mip levels and array indices are within the bounds.
			if (mipLevel >= mipCount)
			{
				mipLevel = mipCount - 1;
			}

			if (arrayIndex >= arrayCount)
			{
				arrayIndex = arrayCount - 1;
			}

			// Get the buffer to copy.
			var buffer = Buffers[mipLevel, arrayIndex];

		    int height = texture.Settings.Height.Min(Settings.Height);

			// Calculate depth for the texture.
			for (int mip = 1; mip <= mipLevel; mip++)
			{
				if (height > 1)
				{
					height >>= 1;
				}

				if (depth > 1)
				{
					depth >>= 1;
				}
			}

			// Copy manually.
			if (texture.Settings.Usage != BufferUsage.Default)
			{
			    GorgonTextureLockData textureData;

				if (texture.Settings.Usage == BufferUsage.Dynamic)
				{
					flags |= BufferLockFlags.Discard;
				}

			    switch (Settings.ImageType)
			    {
			        case ImageType.Image1D:
			            textureData = ((GorgonTexture1D)texture).Lock(flags,
			                arrayIndex,
			                mipLevel);
			            break;
                    case ImageType.ImageCube:
                    case ImageType.Image2D:
			            textureData = ((GorgonTexture2D)texture).Lock(flags,
			                arrayIndex,
			                mipLevel);
			            break;
                    case ImageType.Image3D:
			            textureData = ((GorgonTexture3D)texture).Lock(flags,
			                mipLevel);
			            break;
                    default:
			            throw new NotSupportedException(string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, Settings.ImageType));
			    }

				unsafe
				{
					using (textureData)
					{
						if ((textureData.PitchInformation.RowPitch == buffer.PitchInformation.RowPitch)
						    && (textureData.PitchInformation.SlicePitch == buffer.PitchInformation.SlicePitch)
						    && (depth == buffer.Depth))
						{
							DirectAccess.MemoryCopy(textureData.Data.BasePointer,
							                        buffer.Data.BasePointer,
							                        buffer.PitchInformation.SlicePitch * buffer.Depth);
						}
						else
						{
							int clipRowPitch = textureData.PitchInformation.RowPitch.Min(buffer.PitchInformation.RowPitch);
							var destDepthPtr = (byte*)textureData.Data.BasePointer;
							var srcPtr = (byte*)textureData.Data.BasePointer;

							// Copy all depth information.
							for (int i = 0; i < depth; i++)
							{
								var destPtr = destDepthPtr;

								for (int y = 0; y < height; y++)
								{
									DirectAccess.MemoryCopy(destPtr, srcPtr, clipRowPitch);
									destPtr += textureData.PitchInformation.RowPitch;
									srcPtr += buffer.PitchInformation.RowPitch;
								}

								destDepthPtr += textureData.PitchInformation.SlicePitch;
							}
						}
					}
				}
			}
			else
			{
			    switch (texture.ResourceType)
			    {
			        case ResourceType.Texture1D:
			            ((GorgonTexture1D)texture).UpdateSubResource(buffer,
			                new GorgonRange(0, buffer.Width),
			                arrayIndex,
			                mipLevel);
			            break;
                    case ResourceType.Texture2D:
			            ((GorgonTexture2D)texture).UpdateSubResource(buffer,
			                new Rectangle(0, 0, buffer.Width, buffer.Height),
			                arrayIndex,
			                mipLevel);
			            break;
                    case ResourceType.Texture3D:
			            ((GorgonTexture3D)texture).UpdateSubResource(buffer,
			                new GorgonBox
			                {
			                    Front = 0,
                                Left = 0,
                                Top = 0,
                                Depth = buffer.Depth,
                                Width = buffer.Width,
                                Height = buffer.Height
			                },
			                arrayIndex,
			                mipLevel);
			            break;
                    default:
			            throw new NotSupportedException(string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, Settings.ImageType));
			    }
			}				
		}

        /// <summary>
        /// Function to create a GorgonImageData object from an array of System.Drawing.Image objects.
        /// </summary>
        /// <param name="images">Image objects to retrieve image data from.</param>
        /// <param name="imageType">Type of image to build.</param>
        /// <param name="options">[Optional] Options used to create the image data.</param>
        /// <returns>A new image data object containing the image data from the image array.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="images"/> parameter is NULL.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="images"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown when the image could not be created.</exception>
        /// <remarks>
        /// This method will create a new <see cref="Gorgon.Graphics.GorgonImageData">GorgonImageData</see> object from an array of <see cref="System.Drawing.Image">System.Drawing.Images</see>. 
        /// The method will copy the image information and do a best fit conversion.
        /// <para>If <paramref name="imageType"/> is set to Image1D/2D, then the array can be laid out as mip slices and array indices, if it is a 3D image, then it will be laid out as mip slices and depth slices. 
        /// If the MipCount is set to 1, then only the first image will be processed IF there is only one image in the list.</para>
        /// <para><strong>For 1D and 2D images:</strong> If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
        /// of the list will be taken as the array size. The layout of the image list is processed in the following order (assuming the MipCount = 2, and the array count is = 4):</para>
        /// <para>
        /// <code>
        /// images[0]: Array Index 0, Mip Level 0<br/>
        /// images[1]: Array Index 0, Mip Level 1<br/>
        /// images[2]: Array Index 0, Mip Level 2<br/>
        /// images[3]: Array Index 0, Mip Level 3<br/>
        /// images[4]: Array Index 1, Mip Level 0<br/>
        /// images[5]: Array Index 1, Mip Level 1<br/>
        /// images[6]: Array Index 1, Mip Level 2<br/>
        /// images[7]: Array Index 1, Mip Level 3<br/>
        /// </code>
        /// </para>
        /// <para><strong>For 3D images:</strong> If there is more than 1 image in the list, and the mip count is set to 1, then the element count 
        /// of the list will be taken as the depth size. The layout of the image list is processed in the following order (assuming the MipCount = 2, and the depth is = 4):</para>
        /// <para>
        /// <code>
        /// images[0]: Mip Level 0, Depth slice 0<br/>
        /// images[1]: Mip Level 0, Depth slice 1<br/>
        /// images[2]: Mip Level 0, Depth slice 2<br/>
        /// images[3]: Mip Level 0, Depth slice 3<br/>
        /// images[4]: Mip Level 1, Depth slice 4<br/>
        /// images[5]: Mip Level 1, Depth slice 5<br/>
        /// </code>
        /// The depth is shrunk by a power of 2 for each mip level.  So, at mip level 0 we have 4 depth slices, and at mip level 1 we have 2.  If we had a third mip level, then 
        /// the depth would be 1 at that mip level.
        /// </para>
        /// <para>3D images MUST have a width, height and depth that is a power of 2 if mip maps are to be used.  If the image does not meet the criteria, then an exception will be thrown.
        /// </para>
        /// <para>The <paramref name="options" /> parameter controls how the <paramref name="images" /> are converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>Width</term><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <term>Height</term><description>For 2D and 3D images only. The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <term>Depth</term><description>For 3D images only. This sets the depth for the image.  The default value is set to 1.  If there are no mip-maps (i.e. MipCount = 1), then the number of elements in the list will be used as the depth size.</description>
        /// </item>
        /// <item>
        /// <term>Format</term><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <term>MipCount</term><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <term>ArrayCount</term><description>For 1D/2D images only.  Gorgon will generate the requested number of image arrays from the source image.  Set to 1 if no image arrays are required.</description>
        /// </item>
        /// <item>
        /// <term>Dither</term><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <term>Filter</term><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <term>UseClipping</term><description>Set to <b>true</b> to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is <b>false</b>.</description>
        /// </item>
        /// <item>
        /// <term>ViewFormat</term><description>This value is ignored for image data.</description>
        /// </item>
        /// <item>
        /// <term>AllowUnorderedAccess</term><description>This value is ignored for image data.</description>
        /// </item>
        /// <item>
        /// <term>Multisampling</term><description>This value is ignored for image data.</description>
        /// </item>
        /// </list>
        /// <para>The list of images must be large enough to accomodate the number of mip map levels and the depth at each mip level, must not contain any NULL (<i>Nothing</i> in VB.Net) elements and all images must use
        /// the same pixel format.  If the list is larger than the requested mip/array count, then only the first elements up until mip count and each depth for each mip level are used.  Unlike other overloads, 
        /// this method will NOT auto-generate mip-maps and will only use the images provided.</para>
        /// <para>Images in the list to be used as mip-map levels do not need to be resized because the method will automatically resize based on mip-map level.</para>
        /// </remarks>
        public static GorgonImageData CreateFromGDIImage(IList<Image> images, ImageType imageType, GorgonGDIOptions options = null)
        {
            if (images == null)
            {
                throw new ArgumentNullException("images");
            }

            if (images.Count == 0)
            {
                throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "images");
            }

            if (options == null)
            {
                options = new GorgonGDIOptions();
            }

            using (var wic = new GorgonWICImage())
            {
                switch (imageType)
                {
                    case ImageType.Image1D:
                        return GorgonGDIImageConverter.Create1DImageDataFromImages(wic, images, options);
                    case ImageType.Image2D:
                    case ImageType.ImageCube:
                        return GorgonGDIImageConverter.Create2DImageDataFromImages(wic, images, options);
                    case ImageType.Image3D:
                        return GorgonGDIImageConverter.Create3DImageDataFromImages(wic, images, options);
                }

                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_IMAGE_TYPE_INVALID);
            }
        }


        /// <summary>
        /// Function to create a new image data object from a System.Drawing.Image object.
        /// </summary>
        /// <param name="image">An image object to use as the source image data.</param>
        /// <param name="imageType">Type of image to create.</param>
        /// <param name="options">[Optional] Options for image conversion.</param>
        /// <returns>
        /// A new image data object containing a copy of the System.Drawing.Image data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the image parameter is NULL (<i>Nothing</i> in VB.Net)</exception>
        /// <exception cref="GorgonException">Thrown when image could not be created.</exception>
        /// <remarks>
        /// This method will create a new <see cref="Gorgon.Graphics.GorgonImageData">GorgonImageData</see> object from a <see cref="System.Drawing.Image"/>. 
        /// The method will copy the image information and do a best fit conversion.
        /// <para>This overload will only support 1D and 2D images (non-cubemap, since they require 6 array indices), other image types are not supported for this overload.</para>
        /// <para>The <paramref name="options"/> parameter controls how the <paramref name="image" /> is converted.  Here is a list of available conversion options:</para>
        /// <list type="table">
        /// <listheader>
        /// <term>Setting</term><term>Description</term>
        /// </listheader>
        /// <item>
        /// <term>Width</term><description>The image will be resized to match the width specified.  Set to 0 to use the original image width.</description>
        /// </item>
        /// <item>
        /// <term>Height</term><description>For 2d images only. The image will be resized to match the height specified.  Set to 0 to use the original image height.</description>
        /// </item>
        /// <item>
        /// <term>Depth</term><description>This is ignored for 1D and 2D images.</description>
        /// </item>
        /// <item>
        /// <term>Format</term><description>The image will be converted to the format specified.  Set to Unknown to map to the closest available format.</description>
        /// </item>
        /// <item>
        /// <term>MipCount</term><description>Gorgon will generate the requested number of mip-maps from the source image.  Set to 0 to generate a full mip-map chain, or set to 1 if no mip-maps are required.</description>
        /// </item>
        /// <item>
        /// <term>ArrayCount</term><description>This is ignored for this overload.</description>
        /// </item>
        /// <item>
        /// <term>Dither</term><description>Dithering to apply to images with a higher bit depth than the specified format.  The default value is None.</description>
        /// </item>
        /// <item>
        /// <term>Filter</term><description>Filtering to apply to images that are scaled to the width/height specified.  The default value is Point.</description>
        /// </item>
        /// <item>
        /// <term>UseClipping</term><description>Set to <b>true</b> to clip the image instead of scaling when the width/height is smaller than the image width/height.  The default value is <b>false</b>.</description>
        /// </item>
        /// <item>
        /// <term>ViewFormat</term><description>This value is ignored for image data.</description>
        /// </item>
        /// <item>
        /// <term>AllowUnorderedAccess</term><description>This value is ignored for image data.</description>
        /// </item>
        /// <item>
        /// <term>Multisampling</term><description>This value is ignored for image data.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static GorgonImageData CreateFromGDIImage(Image image, ImageType imageType, GorgonGDIOptions options = null)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }

            if (options == null)
            {
                options = new GorgonGDIOptions();
            }

            // Default these values to 1 for 2D images.
            options.ArrayCount = 1;
            options.Depth = 1;

            using (var wic = new GorgonWICImage())
            {
                switch (imageType)
                {
                    case ImageType.Image1D:
                        return GorgonGDIImageConverter.Create1DImageDataFromImage(wic, image, options);
                    case ImageType.Image2D:
                        return GorgonGDIImageConverter.Create2DImageDataFromImage(wic, image, options);
                }

                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_IMAGE_TYPE_INVALID);
            }
        }

        /// <summary>
        /// Function to copy texture data into a buffer.
        /// </summary>
        /// <param name="stagingTexture">Texture to copy.</param>
        /// <param name="arrayIndex">Index of the array to copy.</param>
        /// <param name="mipLevel">Mip level to copy.</param>
		/// <param name="destImageBuffer">Image buffer to copy into.</param>
        private static void GetTextureData(GorgonTexture stagingTexture, int arrayIndex, int mipLevel, GorgonImageBuffer destImageBuffer)
        {
	        int depthCount = 1.Max(destImageBuffer.Depth);
	        int height = 1.Max(destImageBuffer.Height);
	        int rowStride = destImageBuffer.PitchInformation.RowPitch;
	        int sliceStride = destImageBuffer.PitchInformation.SlicePitch;
	        var flags = BufferLockFlags.Write;

	        if (stagingTexture.Settings.Usage == BufferUsage.Dynamic)
	        {
		        flags |= BufferLockFlags.Discard;
	        }

			// If this image is compressed, then use the block height information.
	        if (destImageBuffer.PitchInformation.BlockCount.Height > 0)
	        {
		        height = destImageBuffer.PitchInformation.BlockCount.Height;
	        }

            // Copy the texture data into the buffer.
            GorgonTextureLockData textureLock;
            switch (stagingTexture.Settings.ImageType)
            {
                case ImageType.Image1D:
                    textureLock = ((GorgonTexture1D)stagingTexture).Lock(flags,
                        arrayIndex,
                        mipLevel);
                    break;
                case ImageType.ImageCube:
                case ImageType.Image2D:
                    textureLock = ((GorgonTexture2D)stagingTexture).Lock(flags,
                        arrayIndex,
                        mipLevel);
                    break;
                case ImageType.Image3D:
                    textureLock = ((GorgonTexture3D)stagingTexture).Lock(flags,
                        mipLevel);
                    break;
                default:
                    throw new GorgonException(GorgonResult.NotInitialized,
                        string.Format(Resources.GORGFX_IMAGE_TYPE_INVALID, stagingTexture.Settings.ImageType));
            }

	        unsafe
	        {
				var buffer = (byte*)destImageBuffer.Data.BasePointer;

		        using (textureLock)
		        {
			        // If the strides don't match, then the texture is using padding, so copy one scanline at a time for each depth index.
			        if ((textureLock.PitchInformation.RowPitch != rowStride)
			            || (textureLock.PitchInformation.SlicePitch != sliceStride))
			        {
				        var destData = buffer;
				        var sourceData = (byte*)textureLock.Data.BasePointer;

				        for (int depth = 0; depth < depthCount; depth++)
				        {
					        // Restart at the padded slice size.
					        byte* sourceStart = sourceData;

					        for (int row = 0; row < height; row++)
					        {
						        DirectAccess.MemoryCopy(destData, sourceStart, rowStride);
						        sourceStart += textureLock.PitchInformation.RowPitch;
						        destData += rowStride;
					        }

					        sourceData += textureLock.PitchInformation.SlicePitch;
				        }
			        }
			        else
			        {
				        // Since we have the same row and slice stride, copy everything in one shot.
				        DirectAccess.MemoryCopy(buffer, textureLock.Data.BasePointer, sliceStride);
			        }
		        }
	        }
        }

        /// <summary>
        /// Function to create an image data object from a texture.
        /// </summary>
        /// <param name="texture">Texture used to create the image data.</param>
        /// <param name="mipLevel">Mip level to copy.</param>
        /// <param name="arrayIndex">[Optional] Index of the texture array to copy.</param>
        /// <returns>A new image data object containing the data from the texture.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the texture parameter has a usage of Immutable.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="arrayIndex"/> or <paramref name="mipLevel"/> parameters are greater than or equal to the number of array indices or mip map levels.</exception>
        /// <remarks>This will create a system memory clone of the <see cref="Gorgon.Graphics.GorgonTexture">texture</see>.  Only textures that do not have the Immutable usage can be converted, if an attempt to 
        /// convert a texture with a usage of Immutable is made, then an exception will be thrown.</remarks>
        public static GorgonImageData CreateFromTexture(GorgonTexture texture, int mipLevel, int arrayIndex = 0)
        {
            GorgonTexture staging = texture;

            if (texture == null)
            {
                throw new ArgumentException("texture");
            }

            if (texture.Settings.Usage == BufferUsage.Immutable)
            {
                throw new ArgumentException(Resources.GORGFX_TEXTURE_IMMUTABLE, "texture");
            }

            // If the texture is a volume texture, then set the array index to 0.
            if ((arrayIndex < 0) || (texture.Settings.ImageType == ImageType.Image3D))
            {
                arrayIndex = 0;
            }

            if (mipLevel < 0)
            {
                mipLevel = 0;
            }

            if (arrayIndex >= texture.Settings.ArrayCount)
            {
                throw new ArgumentOutOfRangeException("arrayIndex",
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, arrayIndex, 0, texture.Settings.ArrayCount));
            }

            if (mipLevel >= texture.Settings.MipCount)
            {
                throw new ArgumentOutOfRangeException("mipLevel",
                    string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, mipLevel, 0, texture.Settings.MipCount));
            }

            if (texture.Settings.Usage != BufferUsage.Staging)
            {
                staging = texture.GetStagingTexture<GorgonTexture>();
            }

            try
            {
                // Build our structure.
                var result = new GorgonImageData(texture.Settings);

                // Get the buffer for the array and mip level.
                var buffer = result.Buffers[mipLevel, arrayIndex];

                // Copy the data from the texture.
                GetTextureData(staging, arrayIndex, mipLevel, buffer);

                return result;
            }
            finally
            {
                if ((staging != null) && (staging != texture))
                {
                    staging.Dispose();
                }
            }            
        }

        /// <summary>
        /// Function to create an image data object from a texture.
        /// </summary>
        /// <param name="texture">Texture used to create the image data.</param>
        /// <returns>A new image data object containing the data from the texture.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the texture parameter has a usage of Immutable.</exception>
        /// <remarks>This will create a system memory clone of the <see cref="Gorgon.Graphics.GorgonTexture">texture</see>.  Only textures that do not have the Immutable usage can be converted, if an attempt to 
        /// convert a texture with a usage of Immutable is made, then an exception will be thrown.</remarks>
        public static GorgonImageData CreateFromTexture(GorgonTexture texture)
        {
            GorgonTexture staging = texture;

            if (texture == null)
            {
                throw new ArgumentException("texture");
            }

            if (texture.Settings.Usage == BufferUsage.Immutable)
            {
                throw new ArgumentException(Resources.GORGFX_TEXTURE_IMMUTABLE, "texture");
            }

            if (texture.Settings.Usage != BufferUsage.Staging)
            {
                staging = texture.GetStagingTexture<GorgonTexture>();
            }

            try
            {
                // Build our structure.
                var result = new GorgonImageData(texture.Settings);

                for (int array = 0; array < texture.Settings.ArrayCount; array++)
                {    
                    for (int mipLevel = 0; mipLevel < texture.Settings.MipCount; mipLevel++)
                    {
                        // Get the buffer for the array and mip level.
                        var buffer = result.Buffers[mipLevel, array];

                        // Copy the data from the texture.
                        GetTextureData(staging, array, mipLevel, buffer);
                    }
                }

                return result;
            }
            finally
            {
                if ((staging != null) && (staging != texture))
                {
                    staging.Dispose();
                }
            }
        }

		/// <summary>
		/// Function to return the size of a 1D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 1D image.</param>
		/// <param name="format">Format of the 1D image.</param>
		/// <param name="arrayCount">[Optional] Number of array indices.</param>
		/// <param name="mipCount">[Optional] Number of mip-map levels in the 1D image.</param>
		/// <returns>The number of bytes for the 1D image.</returns>
        /// <exception cref="GorgonException">Thrown when the <paramref name="format"/> is not supported.</exception>
		public static int GetSizeInBytes(int width, BufferFormat format, int arrayCount = 1, int mipCount = 1)
		{
			return GetSizeInBytes(width, 1, format, arrayCount, mipCount);
		}

		/// <summary>
		/// Function to return the size of a 2D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 2D image.</param>
		/// <param name="height">Height of the 2D image.</param>
		/// <param name="format">Format of the 2D image.</param>
		/// <param name="arrayCount">[Optional] Number of array indices.</param>
		/// <param name="mipCount">[Optional] Number of mip-map levels in the 2D image.</param>
		/// <param name="pitchFlags">[Optional] Flags used to influence the row pitch size.</param>
		/// <returns>The number of bytes for the 2D image.</returns>
        /// <exception cref="GorgonException">Thrown when the <paramref name="format"/> is not supported.</exception>
		public static int GetSizeInBytes(int width, int height, BufferFormat format, int arrayCount = 1, int mipCount = 1, PitchFlags pitchFlags = PitchFlags.None)
		{
			int result = 0;

			arrayCount = 1.Max(arrayCount);

			for (int i = 0; i < arrayCount; i++)
			{
				result += GetSizeInBytes(width, height, 1, format, mipCount, pitchFlags);
			}

			return result;
		}

		/// <summary>
		/// Function to return the size of a 3D image in bytes.
		/// </summary>
		/// <param name="width">Width of the 3D image.</param>
		/// <param name="height">Height of the 3D image.</param>
		/// <param name="depth">Depth of the 3D image.</param>
		/// <param name="format">Format of the 3D image.</param>
		/// <param name="mipCount">[Optional] Number of mip-map levels in the 3D image.</param>
		/// <param name="pitchFlags">[Optional] Flags used to influence the row pitch of the image.</param>
		/// <returns>The number of bytes for the 3D image.</returns>
		/// <exception cref="GorgonException">Thrown when the <paramref name="format"/> is not supported.</exception>
		public static int GetSizeInBytes(int width, int height, int depth, BufferFormat format, int mipCount = 1, PitchFlags pitchFlags = PitchFlags.None)
		{
			if (format == BufferFormat.Unknown)
			{
			    throw new GorgonException(GorgonResult.FormatNotSupported,
			        string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, BufferFormat.Unknown));
			}

			width = 1.Max(width);
			height = 1.Max(height);
			depth = 1.Max(depth);
			mipCount = 1.Max(mipCount);
			var formatInfo = GorgonBufferFormatInfo.GetInfo(format);
			int result = 0;

			if (formatInfo.SizeInBytes == 0)
			{
			    throw new GorgonException(GorgonResult.FormatNotSupported,
			        string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, format));
			}

			int mipWidth = width;
			int mipHeight = height;

			for (int mip = 0; mip < mipCount; mip++)
			{
				var pitchInfo = formatInfo.GetPitch(mipWidth, mipHeight, pitchFlags);
				result += pitchInfo.SlicePitch * depth;

				if (mipWidth > 1)
				{
					mipWidth >>= 1;
				}
				if (mipHeight > 1)
				{
					mipHeight >>= 1;
				}
				if (depth > 1)
				{
					depth >>= 1;
				}
			}

			return result;
		}

		/// <summary>
		/// Function to return the size, in bytes, of an image with the given settings.
		/// </summary>
		/// <param name="settings">Settings to describe the image.</param>
		/// <returns>The number of bytes for the image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the format value of the <paramref name="settings"/> parameter is not supported.</exception>
		public static int GetSizeInBytes(IImageSettings settings)
		{
			return GetSizeInBytes(settings, PitchFlags.None);
		}

		/// <summary>
		/// Function to return the size, in bytes, of an image with the given settings.
		/// </summary>
		/// <param name="settings">Settings to describe the image.</param>
		/// <param name="pitchFlags">Flags to influence the size of the row pitch.</param>
		/// <returns>The number of bytes for the image.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the format value of the <paramref name="settings"/> parameter is not supported.</exception>
		public static int GetSizeInBytes(IImageSettings settings, PitchFlags pitchFlags)
		{
			return settings.ImageType == ImageType.Image3D
				       ? GetSizeInBytes(settings.Width, settings.Height, settings.Depth, settings.Format, settings.MipCount,
				                        pitchFlags)
				       : GetSizeInBytes(settings.Width, settings.Height, settings.Format, settings.ArrayCount, settings.MipCount,
				                        pitchFlags);
		}

	    /// <summary>
		/// Function to return the maximum number of mip levels supported given the specified settings.
		/// </summary>
		/// <param name="settings">Settings to evaluate.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int GetMaxMipCount(IImageSettings settings)
	    {
		    return settings == null ? 0 : GetMaxMipCount(settings.Width, settings.Height, settings.Depth);
	    }

		/// <summary>
		/// Function to return the maximum number of mip levels supported in an image.
		/// </summary>
		/// <param name="width">Width of the proposed image.</param>
		/// <param name="height">[Optional] Height of the proposed image.</param>
		/// <param name="depth">[Optional] Depth of the proposed image.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int GetMaxMipCount(int width, int height = 1, int depth = 1)
		{
			int result = 1;
			width = 1.Max(width);
			height = 1.Max(height);
			depth = 1.Max(depth);			

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
		/// Function to return the number of depth slices for an image with the given number of mip maps.
		/// </summary>
		/// <param name="slices">Slices requested.</param>
		/// <param name="mipCount">Mip map count.</param>
		/// <returns>The number of depth slices.</returns>
		public static int GetDepthSliceCount(int slices, int mipCount)
		{
			if (mipCount < 2)
			{
				return slices;
			}

			int bufferCount = 0;
			int depth = slices;

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
		/// Function to return the number of depth slices for a given mip map slice.
		/// </summary>
		/// <param name="mipLevel">The mip map level to look up.</param>
		/// <returns>The number of depth slices for the given mip map level.</returns>
		/// <remarks>For 1D and 2D images, the mip level will always return 1.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter exceeds the number of mip maps for the image or is less than 0.</exception>
		public int GetDepthCount(int mipLevel)
		{
		    if ((mipLevel < 0)
		        || (mipLevel >= Settings.MipCount))
		    {
		        throw new ArgumentOutOfRangeException("mipLevel",
		            string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, mipLevel, 0, Settings.MipCount));
		    }

			return Settings.Depth <= 1 ? 1 : Buffers.MipOffsetSize[mipLevel].Item2;
		}

		/// <summary>
		/// Function to save the image data to a file with the specified codec.
		/// </summary>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="codec">Codec used to encode the file data.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> or the <paramref name="codec"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the filePath parameter is empty.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.
		/// <para>-or-</para>
		/// <para>Thrown when there is an error when attempting to encode the image data.</para>
		/// </exception>
		/// <remarks>This will persist the contents of the image data object into a stream.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
		/// number of built-in codecs.  Currently, Gorgon supports the following formats:
		/// <list type="bullet">
		///		<item>
		///			<description>DDS</description>
		///		</item>
		///		<item>
		///			<description>TGA</description>
		///		</item>
		///		<item>
		///			<description>PNG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>BMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>JPG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>WMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>TIF (WIC)</description>
		///		</item>
		/// </list>
		/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
		/// then it is required in order to read/save the files in those formats.</para>
		/// </remarks>
		public void Save(string filePath, GorgonImageCodec codec)
		{
			if (filePath == null)
			{
				throw new ArgumentNullException("filePath");
			}

			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "filePath");
			}

			using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				Save(file, codec);
			}
		}

		/// <summary>
		/// Function to save the image data to a stream with the specified codec.
		/// </summary>
		/// <param name="stream">Stream that will contain the image information.</param>
		/// <param name="codec">Codec used to encode the stream data.</param>        
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> or the <paramref name="codec"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>		
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.
		/// <para>-or-</para>
		/// <para>Thrown when there is an error when attempting to encode the image data.</para>
		/// </exception>
		/// <remarks>This will persist the contents of the image data object into a stream.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
		/// number of built-in codecs.  Currently, Gorgon supports the following formats:
		/// <list type="bullet">
		///		<item>
		///			<description>DDS</description>
		///		</item>
		///		<item>
		///			<description>TGA</description>
		///		</item>
		///		<item>
		///			<description>PNG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>BMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>JPG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>WMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>TIF (WIC)</description>
		///		</item>
		/// </list>
		/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
		/// then it is required in order to read/save the files in those formats.</para>
		/// </remarks>
		public void Save(Stream stream, GorgonImageCodec codec)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (codec == null)
			{
				throw new ArgumentNullException("codec");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GORGFX_STREAM_READ_ONLY);
			}

			codec.SaveToStream(this, stream);
		}

		/// <summary>
		/// Function to save the image data to a byte array.
		/// </summary>
		/// <param name="codec">Codec used to encode the stream data.</param>        
		/// <returns>A byte array containing the encoded image data.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="codec"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>		
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.
		/// <para>-or-</para>
		/// <para>Thrown when there is an error when attempting to encode the image data.</para>
		/// </exception>
		/// <remarks>This will persist the contents of the image data object into a byte array.  The data is encoded into various formats via the codec parameter.  Gorgon contains a 
		/// number of built-in codecs.  Currently, Gorgon supports the following formats:
		/// <list type="bullet">
		///		<item>
		///			<description>DDS</description>
		///		</item>
		///		<item>
		///			<description>TGA</description>
		///		</item>
		///		<item>
		///			<description>PNG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>BMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>JPG (WIC)</description>
		///		</item>
		///		<item>
		///			<description>WMP (WIC)</description>
		///		</item>
		///		<item>
		///			<description>TIF (WIC)</description>
		///		</item>
		/// </list>
		/// <para>The items with (WIC) indicate that the codec support is supplied by the Windows Imaging Component.  This component should be installed on most systems, but if it is not 
		/// then it is required in order to read/save the files in those formats.</para>
		/// </remarks>
		public byte[] Save(GorgonImageCodec codec)
		{
			using (var memoryStream = new MemoryStream())
			{
				Save(memoryStream, codec);
				memoryStream.Position = 0;

				return memoryStream.ToArray();
			}
		}

		/// <summary>
		/// Function to save the raw image data to a byte array.
		/// </summary>
		/// <returns>A byte array containing the raw image data.</returns>
		/// <remarks>This will write the contents of this image into a byte array with no encoding.  Use this when there is a need to transfer the raw image data from one 
		/// data structure to another.</remarks>
		public unsafe byte[] SaveRaw()
		{
			var result = new byte[SizeInBytes];
			
			fixed (byte* element = &result[0])
			{
				DirectAccess.MemoryCopy(element, _imageData.BasePointer, result.Length);
			}

			return result;
		}

		/// <summary>
		/// Function to save the raw image data to a stream.
		/// </summary>
		/// <param name="stream">Stream that will contain the raw image data.</param>
		/// <remarks>This will write the contents of this image into a stream with no encoding.  Use this when there is a need to transfer the raw image data from one 
		/// data structure to another.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream parameter is read only.</exception>
		public unsafe void SaveRaw(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}

			if (!stream.CanWrite)
			{
				throw new IOException(Resources.GORGFX_STREAM_READ_ONLY);
			}

			using (var writer = new GorgonBinaryWriter(stream, true))
			{
				writer.Write(_imageData.BasePointer, SizeInBytes);
			}
		}

		/// <summary>
		/// Function to read image data from a file.
		/// </summary>
		/// <param name="filePath">Path to the file that contains the image data.</param>
		/// <param name="codec">The codec that will read the file.</param>
		/// <returns>The image data from the stream.</returns>
		/// <remarks>This will load image data from a file.  The file must have been encoded by a supported image codec.  
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
        /// codecs may be defined and used to load a texture.
        /// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is NULL (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>The <paramref name="codec"/> parameter is NULL.</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the filePath parameter is empty.
		/// <para>-or-</para>
		/// <para>Thrown when the data in the stream cannot be read by the image codec.</para></exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The image file is corrupted or unable to be read by a codec.</para>
		/// </exception>
		public static GorgonImageData FromFile(string filePath, GorgonImageCodec codec)
		{
			GorgonApplication.Log.Print("GorgonImageData : Loading image data from '{0}'...", LoggingLevel.Verbose, filePath);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return FromStream(stream, (int)stream.Length, codec);
            }
		}

		/// <summary>
		/// Function to read image data from a stream.
		/// </summary>
		/// <param name="stream">Stream that contains the image data.</param>
		/// <param name="size">The size of the image, in bytes.</param>
		/// <param name="codec">The codec that will read the file.</param>
		/// <returns>The image data from the stream.</returns>
		/// <remarks>This will load image data from a stream.  The image data in the stream must have been encoded by a supported image codec.  
        /// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
        /// codecs may be defined and used to load a texture.
        /// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).
		/// <para>-or-</para>
		/// <para>The <paramref name="codec"/> parameter is NULL.</para>
		/// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than or equal to 0 or larger than the length of the <paramref name="stream"/>.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the data in the stream cannot be read by the image codec.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		/// <para>-or-</para>
		/// <para>The image file is corrupted or unable to be read by a codec.</para>
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">Thrown if an attempt to read beyond the end of the stream is made.</exception>
		public static GorgonImageData FromStream(Stream stream, int size, GorgonImageCodec codec)
		{
			GorgonImageData result = null;

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            if (codec == null)
			{
				throw new ArgumentNullException("codec");
			}

            if ((size <= 0) || (size > stream.Length))
            {
                throw new ArgumentOutOfRangeException("size", string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, size, 1, stream.Length));
            }

			if (!stream.CanRead)
			{
			    throw new IOException(Resources.GORGFX_STREAM_WRITE_ONLY);
			}

			// Check to see if the decoder can actually read the data in the stream.
			if (!codec.IsReadable(stream))
			{
				throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, codec.Codec));
			}

			// Just apply directly if we're already using a data stream.
			var gorgonDataStream = stream as GorgonDataStream;

            // Load the data into unmanaged memory.
			try
			{
				// Use a memory stream for ease of access.
				if (gorgonDataStream == null)
				{
					gorgonDataStream = new GorgonDataStream(size);
					stream.CopyToStream(gorgonDataStream, size);
					gorgonDataStream.Position = 0;
				}

				result = codec.LoadFromStream(gorgonDataStream, size);

				// Post process our data.
				if ((codec.Width != 0) || (codec.Height != 0) || (codec.MipCount != 0) || (codec.Format != BufferFormat.Unknown))
				{
					codec.PostProcess(result);
				}
			}
			catch
			{
				if (result != null)
				{
                    result.Dispose();
				}

				throw;
			}
            finally
            {
                // If we haven't co-opted the pointer, then free the memory we've allocated.
                if ((stream != gorgonDataStream)
					&& (gorgonDataStream != null))
                {
                    gorgonDataStream.Dispose();
                }
            }

            return result;
		}

        /// <summary>
        /// Function to determine if this image format can be converted to another format.
        /// </summary>
        /// <param name="format">Format to convert to.</param>
        /// <returns><b>true</b> if the the current format and the requested format can be converted, <b>false</b> if not.</returns>
        public bool CanConvert(BufferFormat format)
        {
	        return CanConvert(Settings.Format, format);
        }

		/// <summary>
		/// Function to determine if the source format can be converted to the destination format.
		/// </summary>
		/// <param name="sourceFormat">The source format to compare.</param>
		/// <param name="destFormat">The destination format to compare.</param>
		/// <returns><b>true</b> if the format can be converted, <b>false</b> if not.</returns>
	    public static bool CanConvert(BufferFormat sourceFormat, BufferFormat destFormat)
	    {
			using (var wic = new GorgonWICImage())
			{
				if ((sourceFormat == BufferFormat.Unknown)
					|| (destFormat == BufferFormat.Unknown))
				{
					return false;
				}

				if (destFormat == sourceFormat)
				{
					return true;
				}

				Guid sourcePixelFormat = wic.GetGUID(sourceFormat);
				Guid destPixelFormat = wic.GetGUID(destFormat);

				if ((sourcePixelFormat == Guid.Empty)
					|| (destPixelFormat == Guid.Empty))
				{
					return false;
				}

				// Ensure WIC can convert between the two formats.
				using (var converter = new FormatConverter(wic.Factory))
				{
					return converter.CanConvert(sourcePixelFormat, destPixelFormat);
				}
			}
		}

		/// <summary>
		/// Function to determine if the source format can convert to any of the formats in the destination list.
		/// </summary>
		/// <param name="sourceFormat">The source format to compaare.</param>
		/// <param name="destFormat">List of destination formats to compare.</param>
		/// <returns>An array of formats that the source format can be converted into, or an empty array if no conversion is possible.</returns>
	    public static BufferFormat[] CanConvertToAny(BufferFormat sourceFormat, IEnumerable<BufferFormat> destFormat)
	    {
			if ((sourceFormat == BufferFormat.Unknown)
				|| (destFormat == null))
			{
				return new BufferFormat[0];
			}

			// ReSharper disable PossibleMultipleEnumeration
			if (destFormat.All(item => item == sourceFormat))
			{
				return destFormat.ToArray();
			}

			using (var wic = new GorgonWICImage())
			{
				Guid sourcePixelFormat = wic.GetGUID(sourceFormat);

				if (sourcePixelFormat == Guid.Empty)
				{
					return new BufferFormat[0];
				}

				// Ensure WIC can convert between the two formats.
				using (var converter = new FormatConverter(wic.Factory))
				{
					return (from dest in destFormat
					        let destPixelFormat = wic.GetGUID(dest)
					        where destPixelFormat != Guid.Empty && converter.CanConvert(sourcePixelFormat, destPixelFormat)
                            orderby dest
					        select dest).ToArray();
				}
			}
			// ReSharper restore PossibleMultipleEnumeration
		}

	    /// <summary>
	    /// Function to determine if one format can be converted to all of the formats listed in the destination list.
	    /// </summary>
	    /// <param name="sourceFormat">Source format to compare.</param>
	    /// <param name="destFormat">List of destination formats to compare.</param>
	    /// <returns><b>true</b> if the format can be converted, <b>false</b> if not.</returns>
	    public static bool CanConvertToAll(BufferFormat sourceFormat, IEnumerable<BufferFormat> destFormat)
	    {
			if ((sourceFormat == BufferFormat.Unknown)
				|| (destFormat == null))
			{
				return false;
			}

			// ReSharper disable PossibleMultipleEnumeration
			if (destFormat.All(item => item == sourceFormat))
			{
				return true;
			}

			using (var wic = new GorgonWICImage())
			{
				Guid sourcePixelFormat = wic.GetGUID(sourceFormat);

				if (sourcePixelFormat == Guid.Empty)
				{
					return false;
				}

				// Ensure WIC can convert between the two formats.
				using (var converter = new FormatConverter(wic.Factory))
				{
					return destFormat
						.Select(wic.GetGUID)
						.All(destPixelFormat => (destPixelFormat != Guid.Empty) && (converter.CanConvert(sourcePixelFormat, destPixelFormat)));
				}
			}
			// ReSharper restore PossibleMultipleEnumeration
	    }

		/// <summary>
		/// Function to take ownership of the data belonging to another image object.
		/// </summary>
		/// <param name="data">Data to consume.</param>
		internal void TakeOwnership(GorgonImageData data)
		{
			// Clean up this image.
			Buffers.ClearBuffers(false);
			_imageData.Dispose();

			// Import the data and settings from the other image.
			Settings = data.Settings;
			SizeInBytes = data.SizeInBytes;
			_imageData = data._imageData;
			Buffers = data.Buffers;

			// Take ownership away from the old image.
			data.SizeInBytes = 0;
			data.Buffers = new GorgonImageBufferList(data);
			data._imageData = null;
		}

		/// <summary>
		/// Function to copy this image data into another image.
		/// </summary>
		/// <param name="dest">The image data that will receive the copy of this image.</param>
		/// <remarks>This method requires that the image format, width, and height be the same as the destination.  If these conditions are not met, then an exception will be thrown.</remarks>
		public void CopyTo(GorgonImageData dest)
	    {
			if (dest == null)
			{
				throw new ArgumentNullException("dest");
			}

			if ((dest.Settings.Format != Settings.Format)
			    || (dest.Settings.Width != Settings.Width)
			    || (dest.Settings.Height != Settings.Height))
			{
				throw new ArgumentException(Resources.GORGFX_IMAGE_CANNOT_COPY_IMAGE_DATA_MISMATCH, "dest");
			}

			for (int array = 0; array < Settings.ArrayCount.Min(dest.Settings.ArrayCount); array++)
			{
				int mipDepth = Settings.Depth.Min(dest.Settings.Depth);

				// Start at 1 because we've either already copied the first levels, or we're using the source data.
				for (int mipLevel = 0; mipLevel < Settings.MipCount.Min(dest.Settings.MipCount); mipLevel++)
				{
					for (int depth = 0; depth < mipDepth; depth++)
					{
						var sourceBuffer = Buffers[mipLevel, Settings.ImageType == ImageType.Image3D ? depth : array];
						var destBuffer = dest.Buffers[mipLevel, Settings.ImageType == ImageType.Image3D ? depth : array];

						sourceBuffer.CopyTo(destBuffer);
					}

					// Scale the depth.
					if (mipDepth > 1)
					{
						mipDepth >>= 1;
					}
				}
			}
	    }

		/// <summary>
		/// Function to generate a new mip map chain.
		/// </summary>
		/// <param name="mipCount">Number of mip map levels.</param>
		/// <param name="filter">Filter to apply.</param>
		/// <returns>The actual number of mip maps generated.</returns>
		/// <remarks>This method will generate a new mip map chain for the image data.  If the current number of mip maps is not the same as the requested number, then the image 
		/// buffer will be adjusted to use the requested number of mip maps.  If 0 is passed to <paramref name="mipCount"/>, then a full mip map chain is generated.
		/// <para>Note that the number of requested mip maps may not be honored depending on the current width, height, and depth of the image.  Check the return value to get the 
		/// actual number of mip maps generated.</para>
		/// </remarks>
		public int GenerateMipMaps(int mipCount, ImageFilter filter)
		{
			int maxMips = GetMaxMipCount(Settings);

			// If we specify 0, then generate a full chain.
			if ((mipCount <= 0) || (mipCount > maxMips))
			{
				mipCount = maxMips;
			}

			IImageSettings destSettings = Settings.Clone();
			destSettings.MipCount = mipCount;

			using (var destData = new GorgonImageData(destSettings))
			{
				unsafe
				{
					// Copy the first buffer from the source image to the dest image.
					for (int array = 0; array < Settings.ArrayCount; array++)
					{
						DirectAccess.MemoryCopy(destData.Buffers[0, array].Data.BasePointer,
						                        Buffers[0, array].Data.BasePointer,
						                        Buffers[0, array].PitchInformation.SlicePitch * Settings.Depth);
					}
				}

				if (mipCount > 1)
				{
					using (var wic = new GorgonWICImage())
					{
						Guid format = wic.GetGUID(Settings.Format);

						if (format == Guid.Empty)
						{
							throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, Settings.Format));
						}

						// Begin scaling.
						for (int array = 0; array < Settings.ArrayCount; array++)
						{
							int mipDepth = Settings.Depth;

							// Start at 1 because we've either already copied the first levels, or we're using the source data.
							for (int mipLevel = 1; mipLevel < mipCount; mipLevel++)
							{
								for (int depth = 0; depth < mipDepth; depth++)
								{
									var sourceBuffer = destData.Buffers[0, Settings.ImageType == ImageType.Image3D ? (Settings.Depth / mipDepth) * depth : array];
									var depthBuffer = destData.Buffers[mipLevel, Settings.ImageType == ImageType.Image3D ? depth : array];

									var dataPtr = new DX.DataRectangle(sourceBuffer.Data.BaseIntPtr, sourceBuffer.PitchInformation.RowPitch);
									// Create a temporary bitmap and resize it.
									using (var bitmap = new Bitmap(wic.Factory, sourceBuffer.Width, sourceBuffer.Height, format, dataPtr, sourceBuffer.PitchInformation.SlicePitch))
									{
										// Scale the image into the next buffer.
										wic.TransformImageData(bitmap,
										                       depthBuffer.Data.BaseIntPtr,
										                       depthBuffer.PitchInformation.RowPitch,
										                       depthBuffer.PitchInformation.SlicePitch,
										                       Guid.Empty,
															   false,
															   false,
										                       ImageDithering.None,
										                       new Rectangle(0, 0, depthBuffer.Width, depthBuffer.Height),
										                       false,
										                       filter);
									}
								}

								// Scale the depth.
								if (mipDepth > 1)
								{
									mipDepth >>= 1;
								}
							}
						}
					}
				}

				TakeOwnership(destData);
			}

			return mipCount;
		}

		/// <summary>
		/// Function to resize the image data.
		/// </summary>
		/// <param name="width">New width of the image data.</param>
		/// <param name="height">New height of the image data.</param>
		/// <param name="clip"><b>true</b> to clip the image data, or <b>false</b> to scale the image data to the new size.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="width"/> or the <paramref name="height"/> parameter is less than 1.</exception>
		/// <exception cref="GorgonException">Thrown when the format of the image could not be stretched.</exception>
		/// <remarks>
		/// This will stretch or shrink the image to a new size.  If the <paramref name="clip" /> parameter is set to <b>true</b>, then the image data will be clipped.  That is, it
		/// will not be stretched if the image bounds are larger than the previous image data, and it will not be shrunk if the image bounds were smaller.
		/// <para>Please note that if the image has existing mip-mips then they will be lost and only the first level will be retained. Resizing will only affect the width or height of an image,
		/// not the depth of a 3D image.  1D images can only be stretched horizontally.</para>
		/// <para>If the image is in a format that is not supported, an exception will be thrown.</para>
		/// <para>This overload only provides point filtering when the image is scaled.</para>
		/// </remarks>
		public void Resize(int width, int height, bool clip)
		{
			Resize(width, height, clip, ImageFilter.Point);
		}

		/// <summary>
		/// Function to resize the image data.
		/// </summary>
		/// <param name="width">New width of the image data.</param>
		/// <param name="height">New height of the image data.</param>
		/// <param name="filter">Filtering to apply to the image if it was upscaled or downscaled.</param>
		/// <param name="clip"><b>true</b> to clip the image data, or <b>false</b> to scale the image data to the new size.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="width"/> or the <paramref name="height"/> parameter is less than 1.</exception>
		/// <exception cref="GorgonException">Thrown when the format of the image could not be stretched.</exception>
		/// <remarks>
		/// This will stretch or shrink the image to a new size.  If the <paramref name="clip" /> parameter is set to <b>true</b>, then the image data will be clipped.  That is, it
		/// will not be stretched if the image bounds are larger than the previous image data, and it will not be shrunk if the image bounds were smaller.
		/// <para>Please note that if the image has existing mip-mips then they will be lost and only the first level will be retained. Resizing will only affect the width or height of an image,
		/// not the depth of a 3D image.  1D images can only be stretched horizontally.</para>
		/// <para>If the image is in a format that is not supported, an exception will be thrown.</para>
		/// <para>The <paramref name="filter"/> parameter will only be applied with <paramref name="clip"/> is set to <b>false</b>.  Otherwise it has no effect.</para>
		/// </remarks>
		public void Resize(int width, int height, bool clip, ImageFilter filter)
		{
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException("width");
			}

			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException("height");
			}

			// Well, that was easy...
			if ((width == Settings.Width) && (height == Settings.Height))
			{
				return;
			}

			using (var wic = new GorgonWICImage())
			{
				Guid sourceFormat = wic.GetGUID(Settings.Format);

				if (sourceFormat == Guid.Empty)
				{
					throw new GorgonException(GorgonResult.FormatNotSupported,
					                          string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, Settings.Format));
				}

				// Create our destination buffer to hold the converted data.
				IImageSettings destSettings = Settings.Clone();
				destSettings.Width = width;

				// Don't allow a 1D image to be stretched vertically.
				if (Settings.ImageType != ImageType.Image1D)
				{
					destSettings.Height = height;
				}

				int calcMipLevels = GetMaxMipCount(destSettings.Width, destSettings.Height, destSettings.Depth);

				if (calcMipLevels < destSettings.MipCount)
				{
					destSettings.MipCount = calcMipLevels;
				}

				using (var destData = new GorgonImageData(destSettings))
				{
					for (int array = 0; array < Settings.ArrayCount; array++)
					{
						for (int mip = 0; mip < destSettings.MipCount; mip++)
						{
							int mipDepth = GetDepthCount(mip);

							// Keep the buffer empty if the number of source mips are less than the current mip level.
							if (mip >= Settings.MipCount)
							{
								break;
							}

							for (int depth = 0; depth < mipDepth; depth++)
							{
								// Get the array/mip/depth buffer.
								var destBuffer = destData.Buffers[mip, Settings.ImageType == ImageType.Image3D ? depth : array];
								var srcBuffer = Buffers[mip, Settings.ImageType == ImageType.Image3D ? depth : array];
								var rect = new DX.DataRectangle(srcBuffer.Data.BaseIntPtr, srcBuffer.PitchInformation.RowPitch);

								// Create a WIC bitmap so we have a source for conversion.
								using (var wicBmp = new Bitmap(wic.Factory, srcBuffer.Width, srcBuffer.Height, sourceFormat, rect, srcBuffer.PitchInformation.SlicePitch))
								{
									wic.TransformImageData(wicBmp,
									                       destBuffer.Data.BaseIntPtr,
									                       destBuffer.PitchInformation.RowPitch,
									                       destBuffer.PitchInformation.SlicePitch,
									                       Guid.Empty,
														   false,
														   false,
									                       ImageDithering.None,
									                       new Rectangle(0, 0, destBuffer.Width, destBuffer.Height),
									                       clip,
									                       filter);
								}
							}
						}
					}

					// Import the data into our current image.
					TakeOwnership(destData);
				}
			}
		}

        /// <summary>
        /// Function to convert the format of this image to the requested format.
        /// </summary>
        /// <param name="format">New format for the image.</param>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> is set to Unknown.</exception>
        /// <exception cref="GorgonException">Thrown when the format of this image data or the format parameter is not supported for conversion.</exception>
        /// <remarks>This will convert the current image data to another buffer format.  If a format is unable to be converted then an exception will be thrown.</remarks>
		public void ConvertFormat(BufferFormat format)
		{
			ConvertFormat(format, ImageDithering.None);
		}

        /// <summary>
        /// Function to convert the format of this image to the requested format.
        /// </summary>
        /// <param name="format">New format for the image.</param>
        /// <param name="ditherMode">Dithering to apply to images that have to be downsampled.</param>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> is set to Unknown.</exception>
        /// <exception cref="GorgonException">Thrown when the format of this image data or the format parameter is not supported for conversion.</exception>
        /// <remarks>This will convert the current image data to another buffer format.  If a format is unable to be converted then an exception will be thrown.</remarks>
        public void ConvertFormat(BufferFormat format, ImageDithering ditherMode)
        {
            if (format == BufferFormat.Unknown)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, format), "format");
            }

            // We have the same format, why convert?
            if (format == Settings.Format)
            {
                return;
            }

	        using (var wic = new GorgonWICImage())
	        {
		        var sourceInfo = GorgonBufferFormatInfo.GetInfo(Settings.Format);
		        var destInfo = GorgonBufferFormatInfo.GetInfo(format);
		        Guid sourceFormat = wic.GetGUID(Settings.Format);
		        Guid destFormat = wic.GetGUID(format);

		        if (sourceFormat == Guid.Empty)
		        {
			        throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, Settings.Format));
		        }

		        if (destFormat == Guid.Empty)
		        {
			        throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, format));
		        }

		        // Well, that was easy...
		        if ((sourceFormat == destFormat)
					&& (sourceInfo.IssRGB == destInfo.IssRGB))
		        {
			        return;
		        }

		        // Create our destination buffer to hold the converted data.
		        IImageSettings destSettings = Settings.Clone();
		        destSettings.Format = format;

		        using (var destData = new GorgonImageData(destSettings))
		        {
			        for (int array = 0; array < Settings.ArrayCount; array++)
			        {
				        for (int mip = 0; mip < Settings.MipCount; mip++)
				        {
					        int depthCount = destData.GetDepthCount(mip);

					        for (int depth = 0; depth < depthCount; depth++)
					        {
						        // Get the array/mip/depth buffer.
						        var destBuffer = destData.Buffers[mip, Settings.ImageType == ImageType.Image3D ? depth : array];
						        var srcBuffer = Buffers[mip, Settings.ImageType == ImageType.Image3D ? depth : array];
						        var rect = new DX.DataRectangle(srcBuffer.Data.BaseIntPtr, srcBuffer.PitchInformation.RowPitch);

						        // Create a WIC bitmap so we have a source for conversion.
						        using (var wicBmp = new Bitmap(wic.Factory, srcBuffer.Width, srcBuffer.Height, sourceFormat, rect, srcBuffer.PitchInformation.SlicePitch))
						        {
							        wic.TransformImageData(wicBmp,
							                               destBuffer.Data.BaseIntPtr,
							                               destBuffer.PitchInformation.RowPitch,
							                               destBuffer.PitchInformation.SlicePitch,
							                               destFormat,
														   sourceInfo.IssRGB,
														   destInfo.IssRGB,
							                               ditherMode,
							                               Rectangle.Empty,
							                               true,
							                               ImageFilter.Point);
						        }
					        }
				        }
			        }

			        // Import the data into our current image.
			        TakeOwnership(destData);
		        }
	        }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageData" /> class.
        /// </summary>
        /// <param name="settings">The settings to describe an image.</param>
        /// <param name="data">Pointer to pre-existing image data.</param>
        /// <param name="dataSize">Size of the data, in bytes.  This parameter is ignored if the data parameter is NULL.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		/// <remarks>This overload takes a <paramref name="settings"/> value that implements the <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> interface.  
		/// The GorgonTexture[n]DSettings (where n = <see cref="Gorgon.Graphics.GorgonTexture1DSettings">1</see>, <see cref="Gorgon.Graphics.GorgonTexture2DSettings">2</see>, 
		/// or <see cref="Gorgon.Graphics.GorgonTexture3DSettings">3</see>) types all implement IImageSettings.
		/// <para>If the <paramref name="data"/> pointer is NULL, then a buffer will be created, otherwise the buffer that the pointer is pointing 
        /// at must be large enough to accommodate the size of the image described in the settings parameter and will be validated against the 
        /// <paramref name="dataSize"/> parameter.</para>
        /// <para>If the user passes NULL to the data parameter, then the dataSize parameter is ignored.</para>
        /// <para>If the buffer is passed in via the pointer, then the user is responsible for freeing that memory 
        /// once they are done with it.</para></remarks>
        public unsafe GorgonImageData(IImageSettings settings, void *data, int dataSize)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, settings.Format));
            }

            Settings = settings;            
            SanitizeSettings();
			SizeInBytes = GetSizeInBytes(settings);

            // Validate the image size.
            if ((data != null) && (SizeInBytes > dataSize))
            {
                throw new ArgumentException(Resources.GORGFX_IMAGE_BUFFER_SIZE_MISMATCH, "dataSize");
            }

            Initialize(data, false);
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageData" /> class.
        /// </summary>
        /// <param name="settings">The settings to describe an image.</param>
        /// <param name="data">Pointer to pre-existing image data.</param>
        /// <param name="dataSize">Size of the data, in bytes.  This parameter is ignored if the data parameter is NULL.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		/// <remarks>This overload takes a <paramref name="settings"/> value that implements the <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> interface.  
		/// The GorgonTexture[n]DSettings (where n = <see cref="Gorgon.Graphics.GorgonTexture1DSettings">1</see>, <see cref="Gorgon.Graphics.GorgonTexture2DSettings">2</see>, 
		/// or <see cref="Gorgon.Graphics.GorgonTexture3DSettings">3</see>) types all implement IImageSettings.
		/// <para>If the <paramref name="data"/> pointer is NULL, then a buffer will be created, otherwise the buffer that the pointer is pointing 
        /// at must be large enough to accomodate the size of the image described in the settings parameter and will be validated against the 
		/// <paramref name="dataSize"/> parameter.</para>
        /// <para>If the user passes NULL to the data parameter, then the dataSize parameter is ignored.</para>
        /// <para>If the buffer is passed in via the pointer, then the user is responsible for freeing that memory 
        /// once they are done with it.</para></remarks>
		public unsafe GorgonImageData(IImageSettings settings, IntPtr data, int dataSize)
			: this(settings, data == IntPtr.Zero ? null : data.ToPointer(), dataSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageData" /> class.
		/// </summary>
		/// <param name="settings">The settings to describe an image.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		/// <remarks>This overload takes a <paramref name="settings"/> value that implements the <see cref="Gorgon.Graphics.IImageSettings">IImageSettings</see> interface.  
		/// The GorgonTexture[n]DSettings (where n = <see cref="Gorgon.Graphics.GorgonTexture1DSettings">1</see>, <see cref="Gorgon.Graphics.GorgonTexture2DSettings">2</see>, 
		/// or <see cref="Gorgon.Graphics.GorgonTexture3DSettings">3</see>) types all implement IImageSettings.</remarks>
		public unsafe GorgonImageData(IImageSettings settings)
			: this(settings, null, 0)
		{
		}
		#endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
	        if (_disposed)
	        {
		        return;
	        }

	        if (disposing)
	        {
		        if (_imageData != null)
		        {
			        _imageData.Dispose();
		        }

		        if (Buffers != null)
		        {
			        Buffers.ClearBuffers(true);
		        }
	        }

			_imageData = null;
	        _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
	}
}
