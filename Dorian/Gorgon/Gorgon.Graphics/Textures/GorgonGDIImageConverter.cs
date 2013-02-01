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
// Created: Friday, February 1, 2013 12:51:14 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using WIC = SharpDX.WIC;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A utility class to convert a GDI+ image object into a Gorgon 2D image object.
	/// </summary>
	static class GorgonGDIImageConverter
	{
		// Formats that can be converted.
		private static Tuple<PixelFormat, BufferFormat>[] _formatConversion = new[] 
		{
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format64bppArgb, BufferFormat.R16G16B16A16_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format32bppArgb, BufferFormat.R8G8B8A8_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppGrayScale, BufferFormat.R16_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppArgb1555, BufferFormat.B5G5R5A1_UIntNormal),
			new Tuple<PixelFormat, BufferFormat>(PixelFormat.Format16bppRgb565, BufferFormat.B5G6R5_UIntNormal),
		};

		// Best fit format mapping.
		private static Tuple<PixelFormat, PixelFormat>[] _bestFit = new[]
		{
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format1bppIndexed, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format4bppIndexed, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format8bppIndexed, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format16bppRgb555, PixelFormat.Format16bppArgb1555),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format24bppRgb, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format32bppPArgb, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format32bppRgb, PixelFormat.Format32bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format48bppRgb, PixelFormat.Format64bppArgb),
			new Tuple<PixelFormat, PixelFormat>(PixelFormat.Format64bppPArgb, PixelFormat.Format64bppArgb)
		};

        /// <summary>
        /// Function to retrieve the corresponding buffer format for a given pixel format.
        /// </summary>
        /// <param name="format">Pixel format to look up.</param>
        /// <returns>The corresponding buffer format if found, Unknown if not.</returns>
        private static BufferFormat GetBufferFormat(PixelFormat format)
        {
            PixelFormat bestFormat = format;
            BufferFormat result = BufferFormat.Unknown;

            // Get the best fit for the pixel format.
            for (int i = 0; i < _bestFit.Length; i++)
            {
                if (format == _bestFit[i].Item1)
                {
                    bestFormat = _bestFit[i].Item2;
                }
            }

            // Find the conversion format.
            for (int i = 0; i < _formatConversion.Length; i++)
            {
                if (bestFormat == _formatConversion[i].Item1)
                {
                    result = _formatConversion[i].Item2;
                }
            }

            return result;
        }

        /// <summary>
        /// Function to return the corresponding pixel format for a given buffer format.
        /// </summary>
        /// <param name="format">Format to look up.</param>
        /// <returns>The corresponding pixel format if found, NULL if not.</returns>
        private static PixelFormat? GetPixelFormat(BufferFormat format)
        {
            PixelFormat? result = null;

            for (int i = 0; i < _formatConversion.Length; i++)
            {
                if (format == _formatConversion[i].Item2)
                {
                    result = _formatConversion[i].Item1;
                }
            }

            switch (format)
            {
                case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
                case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
                case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
                    return PixelFormat.Format32bppArgb;
            }

            return result;
        }

        /// <summary>
        /// Function to create 2D Gorgon image data from an array of System.Drawing.Image objects.
        /// </summary>
        /// <param name="image">A list of images to convert.</param>
        /// <param name="arrayCount">Number of array indices to convert.</param>
        /// <param name="mipCount">Number of mip maps to convert.</param>
        /// <returns>The converted image data.</returns>
        /// <remarks>The image array should be laid out with a sequence of mip maps for every array index.  For example, to create image data with 2 array items and 3 mip 
        /// map levels, you would format the array as like this:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Array Index</term>
        ///         <term>Mip Map</term>
        ///     </listheader>
        ///     <item>
        ///         <description>Index 0</description><description>Mip 0</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Mip 1</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Mip 2</description>
        ///     </item>
        ///     <item>
        ///         <description>Index 1</description><description>Mip 0</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Mip 1</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Mip 2</description>
        ///     </item>
        /// </list>
        /// <para>The <paramref name="image"/> parameter must contain images that use the same pixel format.  Multiple formats are not supported.  If the image array contains 
        /// images that have different formats, then an exception will be thrown.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="image"/> parameter is NULL or empty.
        /// <para>-or-</para>
        /// <para>Thrown when the image parameter contains images that use different pixel formats.</para>
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the image array length is smaller than array element count * mip map level count.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown if the image pixel format cannot be converted to a Gorgon <see cref="GorgonLibrary.Graphics.BufferFormat">BufferFormat</see>.</exception>
        public static GorgonImageData<GorgonTexture2DSettings> Create2DImageDataFromImage(Image[] image, int arrayCount, int mipCount)
        {
            BufferFormat format = BufferFormat.Unknown;
            GorgonImageData<GorgonTexture2DSettings> data = null;

            if ((image == null) || (image.Length == 0))
            {
                throw new ArgumentException("The parameter must not be NULL or empty.", "image");
            }

            // Ensure that all formats are the same.
            if (image.Any(item => item.PixelFormat != image[0].PixelFormat))
            {
                throw new ArgumentException("The images must use the same pixel format.", "image");
            }

            format = GetBufferFormat(image[0].PixelFormat);
            if (format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "The pixel format '" + image[0].PixelFormat.ToString() + "' is not supported.");
            }            

            if (mipCount < 1)
            {
                mipCount = 1;
            }

            if (arrayCount < 1)
            {
                arrayCount = 1;
            }

            if (image.Length < mipCount * arrayCount)
            {
                throw new ArgumentOutOfRangeException("image", "There are not enough images [" + image.Length.ToString() + "] to support " + arrayCount.ToString() + " array indices and " + mipCount.ToString() + " mip levels.");
            }

            // Create our settings.
            GorgonTexture2DSettings settings = new GorgonTexture2DSettings()
            {
                Width = image.Max(item => item.Width),
                Height = image.Max(item => item.Height),
                MipCount = mipCount,
                ArrayCount = arrayCount,
                Format = format
            };

            // Create our image data.
            data = new GorgonImageData<GorgonTexture2DSettings>(settings);

            using (GorgonWICImage wic = new GorgonWICImage())
            {
                for (int array = 0; array < arrayCount; array++)
                {
                    for (int mipLevel = 0; mipLevel < mipCount; mipLevel++)
                    {
                        Image currentImage = image[array * mipCount + mipLevel];

                        // Using the image, convert to a WIC bitmap object.
                        using (WIC.Bitmap bitmap = wic.CreateWICImageFromImage(currentImage))                        
                        {
                            // Convert to a GorgonImageData
                            wic.AddWICBitmapToImageData<GorgonTexture2DSettings>(bitmap, WIC.BitmapDitherType.None, data, array, mipLevel, 1);
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Function to create 3D Gorgon image data from an array of System.Drawing.Image objects.
        /// </summary>
        /// <param name="image">A list of images to convert.</param>
        /// <param name="mipCount">Number of mip maps to convert.</param>
        /// <param name="sliceCount">Number of depth slices.</param>
        /// <returns>The converted image data.</returns>
        /// <remarks>The image array should be laid out with a sequence of depth slices for every mip map level, with the number of depth slices being divided by 2 for 
        /// each mip map level.  For example, to create image data with 2 mip map levels and  starting with 4 depth slices, you would format the array as like this:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Mip Map</term><term>Depth Slice</term>
        ///     </listheader>
        ///     <item>
        ///         <description>Mip 0</description><description>Slice 0</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Slice 1</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Slice 2</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Slice 3</description>
        ///     </item>
        ///     <item>
        ///         <description>Mip 1</description><description>Slice 0</description>
        ///     </item>
        ///     <item>
        ///         <description></description><description>Slice 1</description>
        ///     </item>
        /// </list>
        /// <para>The <paramref name="image"/> parameter must contain images that use the same pixel format.  Multiple formats are not supported.  If the image array contains 
        /// images that have different formats, then an exception will be thrown.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="image"/> parameter is NULL or empty.
        /// <para>-or-</para>
        /// <para>Thrown when the image parameter contains images that use different pixel formats.</para>
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the image array length is smaller than mip map level count * depth slice count.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown if the image pixel format cannot be converted to a Gorgon <see cref="GorgonLibrary.Graphics.BufferFormat">BufferFormat</see>.</exception>
        public static GorgonImageData<GorgonTexture3DSettings> Create3DImageDataFromImage(Image[] image, int mipCount, int sliceCount)
        {
            BufferFormat format = BufferFormat.Unknown;
            GorgonImageData<GorgonTexture3DSettings> data = null;

            if ((image == null) || (image.Length == 0))
            {
                throw new ArgumentException("The parameter must not be NULL or empty.", "image");
            }

            // Ensure that all formats are the same.
            if (image.Any(item => item.PixelFormat != image[0].PixelFormat))
            {
                throw new ArgumentException("The images must use the same pixel format.", "image");
            }

            format = GetBufferFormat(image[0].PixelFormat);
            if (format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "The pixel format '" + image[0].PixelFormat.ToString() + "' is not supported.");
            }

            if (mipCount < 1)
            {
                mipCount = 1;
            }

            if (sliceCount < 1)
            {
                sliceCount = 1;
            }

            // Create our settings.
            GorgonTexture3DSettings settings = new GorgonTexture3DSettings()
            {
                Width = image.Max(item => item.Width),
                Height = image.Max(item => item.Height),
                Depth = sliceCount,
                MipCount = mipCount,                
                Format = format
            };

            // Create our image data.
            data = new GorgonImageData<GorgonTexture3DSettings>(settings);

            using (GorgonWICImage wic = new GorgonWICImage())
            {
                int mipDepth = sliceCount;

                for (int mipLevel = 0; mipLevel < mipCount; mipLevel++)
                {
                    if (image.Length < mipCount * mipDepth)
                    {
                        throw new ArgumentOutOfRangeException("image", "There are not enough images [" + image.Length.ToString() + "] to support " + mipCount.ToString() + " mip levels and " + sliceCount.ToString() + " depth slices.");
                    }

                    for (int depth = 0; depth < mipDepth; depth++)
                    {
                        Image currentImage = image[mipLevel * mipDepth + mipLevel];

                        // Using the image, convert to a WIC bitmap object.
                        using (WIC.Bitmap bitmap = wic.CreateWICImageFromImage(currentImage))
                        {
                            // Convert to a GorgonImageData
                        }
                    }

                    if (mipDepth > 1)
                    {
                        mipDepth >>= 1;
                    }
                }
            }

            return data;
        }
    }
}
