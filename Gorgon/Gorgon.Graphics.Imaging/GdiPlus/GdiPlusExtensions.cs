#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 1, 2017 9:44:33 PM
// 
#endregion

using System;
using System.Drawing.Imaging;
using Drawing = System.Drawing;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Imaging.GdiPlus
{
	/// <summary>
	/// Extension methods to use GDI+ (<seealso cref="System.Drawing"/>) bitmaps with <see cref="IGorgonImage"/>
	/// </summary>
	public static class GdiPlusExtensions
	{
        /// <summary>
        /// Function to transfer a 32 bit rgba image into a <see cref="IGorgonImageBuffer"/>.
        /// </summary>
        /// <param name="bitmapLock">The lock on the bitmap to transfer from.</param>
        /// <param name="buffer">The buffer to transfer into.</param>
        /// <param name="destBufferSize">The size of a pixel, in bytes, in the destination.</param>
	    private static void Transfer32Argb(BitmapData bitmapLock, IGorgonImageBuffer buffer, int destBufferSize)
	    {
	        unsafe
	        {
	            var pixels = (int*)bitmapLock.Scan0.ToPointer();

	            for (int y = 0; y < bitmapLock.Height; y++)
	            {
	                // We only need the width here, as our pointer will handle the stride by virtue of being an int.
	                int* offset = pixels + (y * bitmapLock.Width);

	                int destOffset = y * buffer.PitchInformation.RowPitch;
	                for (int x = 0; x < bitmapLock.Width; x++)
	                {
	                    // The DXGI format nomenclature is a little confusing as we tend to think of the layout as being highest to 
	                    // lowest, but in fact, it is lowest to highest.
	                    // So, we must convert to ABGR even though the DXGI format is RGBA. The memory layout is from lowest 
	                    // (R at byte 0) to the highest byte (A at byte 3).
	                    // Thus, R is the lowest byte, and A is the highest: A(24), B(16), G(8), R(0).
	                    var color = new GorgonColor(*offset);
	                    buffer.Data.Write(destOffset, color.ToABGR());
	                    offset++;

	                    destOffset += destBufferSize;
                    }
	            }
	        }
        }

	    /// <summary>
	    /// Function to transfer a 24 bit rgb image into a <see cref="IGorgonImageBuffer"/>.
	    /// </summary>
	    /// <param name="bitmapLock">The lock on the bitmap to transfer from.</param>
	    /// <param name="buffer">The buffer to transfer into.</param>
	    /// <param name="destBufferSize">The size of a pixel, in bytes, in the destination.</param>
	    private static void Transfer24Rgb(BitmapData bitmapLock, IGorgonImageBuffer buffer, int destBufferSize)
	    {
	        unsafe
	        {
	            var pixels = (byte*)bitmapLock.Scan0.ToPointer();

	            for (int y = 0; y < bitmapLock.Height; y++)
	            {
	                // We only need the width here, as our pointer will handle the stride by virtue of being an int.
	                byte* offset = pixels + (y * bitmapLock.Stride);

	                int destOffset = y * buffer.PitchInformation.RowPitch;
	                for (int x = 0; x < bitmapLock.Width; x++)
	                {
	                    // The DXGI format nomenclature is a little confusing as we tend to think of the layout as being highest to 
	                    // lowest, but in fact, it is lowest to highest.
	                    // So, we must convert to ABGR even though the DXGI format is RGBA. The memory layout is from lowest 
	                    // (R at byte 0) to the highest byte (A at byte 3).
	                    // Thus, R is the lowest byte, and A is the highest: A(24), B(16), G(8), R(0).
	                    byte b = *offset++;
	                    byte g = *offset++;
	                    byte r = *offset++;

	                    var color = new GorgonColor(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
	                    buffer.Data.Write(destOffset, color.ToABGR());

	                    destOffset += destBufferSize;
	                }
	            }
	        }
	    }

        /// <summary>
        /// Function to convert a <see cref="Drawing.Bitmap"/> into a <seealso cref="IGorgonImage"/>.
        /// </summary>
        /// <param name="bitmap">The <seealso cref="Drawing.Bitmap"/> to convert.</param>
        /// <returns>A new <seealso cref="IGorgonImage"/> containing the data from the <seealso cref="Drawing.Bitmap"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bitmap"/> parameter is <b>null</b>.</exception>
        /// <exception cref="GorgonException">Thrown if the <paramref name="bitmap"/> is not <see cref="PixelFormat.Format32bppArgb"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method will take a 2D <see cref="Drawing.Bitmap"/> and copy its data into a new 2D <seealso cref="IGorgonImage"/>. The resulting <seealso cref="IGorgonImage"/> will only contain 1 array level, 
        /// and no mip map levels.
        /// </para>
        /// <para>
        /// Some format conversion is performed on the <paramref name="bitmap"/> when it is imported. The format conversion will always convert to the image format of <c>R8G8B8A8_UNorm</c>. Only the 
        /// following GDI+ pixel formats are supported for conversion:
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="PixelFormat.Format32bppArgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="PixelFormat.Format32bppPArgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="PixelFormat.Format32bppRgb"/></term>
        ///     </item>
        ///     <item>
        ///         <term><see cref="PixelFormat.Format24bppRgb"/></term>
        ///     </item>
        /// </list>
        /// If the source <paramref name="bitmap"/> does not support any of the formats on the list, then an exception will be thrown.
        /// </para>
        /// </remarks>
        public static IGorgonImage ToGorgonImage(this Drawing.Bitmap bitmap)
		{
			if (bitmap == null)
			{
				throw new ArgumentNullException(nameof(bitmap));
			}

			if ((bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                && (bitmap.PixelFormat != PixelFormat.Format32bppPArgb)
                && (bitmap.PixelFormat != PixelFormat.Format32bppRgb)
                && (bitmap.PixelFormat != PixelFormat.Format24bppRgb))
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, bitmap.PixelFormat));
			}

			IGorgonImageInfo info = new GorgonImageInfo(ImageType.Image2D, DXGI.Format.R8G8B8A8_UNorm)
			{
				Width = bitmap.Width,
				Height = bitmap.Height
			};

			IGorgonImage result = new GorgonImage(info);
			BitmapData bitmapLock = null;

			try
			{
				bitmapLock = bitmap.LockBits(new Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

			    if ((bitmap.PixelFormat == PixelFormat.Format32bppArgb)
                    || (bitmap.PixelFormat == PixelFormat.Format32bppPArgb)
                    || (bitmap.PixelFormat == PixelFormat.Format32bppRgb))
			    {
			        Transfer32Argb(bitmapLock, result.Buffers[0], result.FormatInfo.SizeInBytes);
			    }
			    else 
			    {
			        Transfer24Rgb(bitmapLock, result.Buffers[0], result.FormatInfo.SizeInBytes);
                }
			}
			catch
			{
				result.Dispose();
				throw;
			}
			finally
			{
				if (bitmapLock != null)
				{
					bitmap.UnlockBits(bitmapLock);
				}
			}

			return result;
		}
	}
}
