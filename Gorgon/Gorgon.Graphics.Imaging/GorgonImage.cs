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
// Created: June 20, 2016 8:37:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using DXGI = SharpDX.DXGI;
using Gorgon.Core;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging
{
	/// <summary>
	/// Holds raw data that is used to represent an image.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="GorgonImage"/> object will hold a blob of data in native (unmanaged) memory and represent that data as a series of pixels to be displayed, or manipulated. This image type is capable 
	/// of representing standard 2D images, but can also represent 1D and 3D images. And, depending on the type of image, there is also support for mip map levels, and arrayed images.
	/// </para>
	/// <para>
	/// Images can access their data directly through a <see cref="IGorgonPointer"/> interface that allows safe access to raw, unmanaged memory where the image data is stored. In cases where images have 
	/// multiple parts like depth slices for a 3D image, or an array for 2D images, this object will provide access through a series of buffers that will point to the individual locations for depth slices, 
	/// array indices, and mip map levels. These buffers will also provide their own <see cref="IGorgonPointer"/> that will allow safe and direct access to the native memory where the buffer is located.
	/// </para>
	/// <para>
	/// Because this object stored data in native memory instead of on the heaps provided by .NET, this object should be disposed by calling its <see cref="IDisposable.Dispose"/> method when it is no 
	/// longer required. Failure to do so might cause a memory leak until the garbage collector can deal with it.
	/// </para>
	/// </remarks>
	public class GorgonImage
		: IGorgonImage
	{
		#region Variables.
		// The pointer to the start of the image data.
		private IGorgonPointer _imageData;
		// Information used to create the image.
		private GorgonImageInfo _imageInfo;
		// The list of image buffers.
		private ImageBufferList _imageBuffers;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the pointer to the beginning of the internal buffer.
		/// </summary>
		public IGorgonPointer ImageData => _imageData;

		/// <summary>
		/// Property to return the information used to create the image.
		/// </summary>
		public IGorgonImageInfo Info => _imageInfo;

		/// <summary>
		/// Property to return information about the pixel format for this image.
		/// </summary>
		public GorgonFormatInfo FormatInfo
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
		public IGorgonImageBufferList Buffers => _imageBuffers;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the pointer to the image data.
		/// </summary>
		/// <param name="data">The image data base pointer.</param>
		/// <param name="copy"><b>true</b> to copy the data in the base pointer to a new pointer, or <b>false</b> to alias the existing base pointer.</param>
		private unsafe void GetImagePointer(IGorgonPointer data, bool copy)
		{
			// Create a buffer large enough to hold our data.
			if ((data != null) && (!copy))
			{
				_imageData = new GorgonPointerAlias((void *)data.Address, SizeInBytes);
				return;
			}

			_imageData = new GorgonPointer(SizeInBytes);

			if (data == null) 
			{
				DirectAccess.ZeroMemory((void*)_imageData.Address, SizeInBytes);
				return;
			}

			// Copy the data in chunks if it's larger than 2GB
			if (data.Size > int.MaxValue)
			{
				long size = data.Size;
				// Copy 1 GB at a time.
				int bufferSize = 1073741824;
				long offset = 0;

				while (size > 0)
				{
					_imageData.CopyFrom(data, offset, bufferSize, offset);

					offset += bufferSize;
					size -= bufferSize;

					// Copy remaining bytes.
					if (size > bufferSize)
					{
						bufferSize = (int)size;
					}
				}
			}
			else
			{
				// Otherwise, just make a simple copy.
				_imageData.CopyFrom(data, (int)data.Size);
			}
		}

		/// <summary>
		/// Function to initialize the image data.
		/// </summary>
		/// <param name="data">Pre-existing data to use.</param>
		/// <param name="copy"><b>true</b> to copy the data, <b>false</b> to take ownership of the pointer.  Only applies when data is non-null.</param>
		private void Initialize(IGorgonPointer data, bool copy)
		{
			GetImagePointer(data, copy);
			_imageBuffers = new ImageBufferList(this);
			_imageBuffers.CreateBuffers(_imageData);
		}

		/// <summary>
		/// Function to sanitize the image information.
		/// </summary>
		/// <param name="info">Information used to create this image.</param>
		private void SanitizeInfo(IGorgonImageInfo info)
		{
			var newInfo = new GorgonImageInfo(info);

			// Validate the array size.
			newInfo.ArrayCount = newInfo.ImageType == ImageType.Image3D ? 1 : newInfo.ArrayCount.Max(1);
			
			// Limit to the lesser value.
			int maxMipCount = CalculateMaxMipCount(info);
			newInfo.MipCount = newInfo.MipCount.Min(maxMipCount);

			if (newInfo.MipCount <= 0)
			{
				newInfo.MipCount = maxMipCount;
			}

			if (newInfo.ImageType != ImageType.ImageCube)
			{
				_imageInfo = newInfo;
				return;
			}

			// If we're an image cube, and we don't have an array count that's a multiple of 6, then up size until we do.
			while ((newInfo.ArrayCount % 6) != 0)
			{
				newInfo.ArrayCount++;
			}
		}

		/// <summary>
		/// Function to return the size of an image in bytes.
		/// </summary>
		/// <param name="imageType">The type of image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="depthOrArrayCount">Depth (for <see cref="ImageType.Image3D"/>) or array count (for <see cref="ImageType.Image1D"/> or <see cref="ImageType.Image2D"/>) of the image.</param>
		/// <param name="format">Format of the image.</param>
		/// <param name="mipCount">[Optional] Number of mip-map levels in the image.</param>
		/// <param name="pitchFlags">[Optional] Flags used to influence the row pitch of the image.</param>
		/// <returns>The number of bytes for the image.</returns>
		/// <exception cref="GorgonException">Thrown when the <paramref name="format"/> is not supported.</exception>
		/// <remarks>
		/// <para>
		/// The <paramref name="pitchFlags"/> parameter is used to compensate in cases where the original image data is not laid out correctly (such as with older DirectDraw DDS images).
		/// </para>
		/// </remarks>
		public static int CalculateSizeInBytes(ImageType imageType, int width, int height, int depthOrArrayCount, DXGI.Format format, int mipCount = 1, PitchFlags pitchFlags = PitchFlags.None)
		{
			if (format == DXGI.Format.Unknown)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
			}

			width = 1.Max(width);
			height = 1.Max(height);
			depthOrArrayCount = 1.Max(depthOrArrayCount);
			mipCount = 1.Max(mipCount);
			var formatInfo = new GorgonFormatInfo(format);
			int result = 0;

			if (formatInfo.SizeInBytes == 0)
			{
				throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format));
			}

			int mipWidth = width;
			int mipHeight = height;

			for (int mip = 0; mip < mipCount; mip++)
			{
				var pitchInfo = formatInfo.GetPitchForFormat(mipWidth, mipHeight, pitchFlags);
				result += pitchInfo.SlicePitch * depthOrArrayCount;

				if (mipWidth > 1)
				{
					mipWidth >>= 1;
				}

				switch (imageType)
				{
					case ImageType.Image2D:
					case ImageType.ImageCube:
						if (mipHeight > 1)
						{
							mipHeight >>= 1;
						}
						break;
					case ImageType.Image3D:
						if (depthOrArrayCount > 1)
						{
							depthOrArrayCount >>= 1;
						}
						break;
				}
			}

			return result;
		}

		/// <summary>
		/// Function to return the size, in bytes, of an image with the given <see cref="IGorgonImageInfo"/>.
		/// </summary>
		/// <param name="info">The <see cref="IGorgonImageInfo"/> used to describe the image.</param>
		/// <param name="pitchFlags">[Optional] Flags to influence the size of the row pitch.</param>
		/// <returns>The number of bytes for the image.</returns>
		/// <exception cref="ArgumentException">Thrown when the format value of the <paramref name="info"/> parameter is not supported.</exception>
		/// <remarks>
		/// <para>
		/// The <paramref name="pitchFlags"/> parameter is used to compensate in cases where the original image data is not laid out correctly (such as with older DirectDraw DDS images).
		/// </para>
		/// </remarks>
		public static int CalculateSizeInBytes(IGorgonImageInfo info, PitchFlags pitchFlags = PitchFlags.None)
		{
			if (info == null)
			{
				return 0;
			}

			return CalculateSizeInBytes(info.ImageType,
			                      info.Width,
			                      info.Height,
			                      info.ImageType == ImageType.Image3D ? info.Depth : info.ArrayCount,
			                      info.Format,
			                      info.MipCount,
			                      pitchFlags);
		}

		/// <summary>
		/// Function to return the maximum number of mip levels supported given the specified settings.
		/// </summary>
		/// <param name="info">The <see cref="IGorgonImageInfo"/> used to describe the image.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int CalculateMaxMipCount(IGorgonImageInfo info)
		{
			return info == null ? 0 : CalculateMaxMipCount(info.Width, info.Height, info.Depth);
		}

		/// <summary>
		/// Function to return the maximum number of mip levels supported in for an image.
		/// </summary>
		/// <param name="width">Width of the proposed image.</param>
		/// <param name="height">Height of the proposed image.</param>
		/// <param name="depth">Depth of the proposed image.</param>
		/// <returns>The number of possible mip-map levels in the image.</returns>
		public static int CalculateMaxMipCount(int width, int height, int depth)
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
		/// Function to return the number of depth slices for a 3D image with the given number of mip maps.
		/// </summary>
		/// <param name="maximumSlices">The maximum desired number of depth slices to return.</param>
		/// <param name="mipCount">The number of mip maps to use.</param>
		/// <returns>The actual number of depth slices.</returns>
		public static int CalculateDepthSliceCount(int maximumSlices, int mipCount)
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
		/// Function to return the number of depth slices available for a given mip map level.
		/// </summary>
		/// <param name="mipLevel">The mip map level to look up.</param>
		/// <returns>The number of depth slices for the given mip map level.</returns>
		/// <remarks>
		/// <para>
		/// For 1D and 2D images, the mip level will always return 1.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter exceeds the number of mip maps for the image or is less than 0.</exception>
		public int GetDepthCount(int mipLevel)
		{
			if ((mipLevel < 0)
				|| (mipLevel >= Info.MipCount))
			{
				throw new ArgumentOutOfRangeException(nameof(mipLevel), mipLevel, string.Format(Resources.GORIMG_ERR_INDEX_OUT_OF_RANGE, 0, Info.MipCount));
			}

			return Info.Depth <= 1 ? 1 : _imageBuffers.MipOffsetSize[mipLevel].Item2;
		}

		///// <summary>
		///// Function to read image data from a stream.
		///// </summary>
		///// <param name="stream">Stream that contains the image data.</param>
		///// <param name="size">The size of the image, in bytes.</param>
		///// <param name="codec">The codec that will read the file.</param>
		///// <returns>The image data from the stream.</returns>
		///// <remarks>This will load image data from a stream.  The image data in the stream must have been encoded by a supported image codec.  
		///// Gorgon supports several codecs such as Png, Dds, Tiff, Jpg, Bmp and Wmp "out of the box", additional user 
		///// codecs may be defined and used to load a texture.
		///// </remarks>
		///// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>NULL</b> (<i>Nothing</i> in VB.Net).
		///// <para>-or-</para>
		///// <para>The <paramref name="codec"/> parameter is <b>NULL</b>.</para>
		///// </exception>
		///// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than or equal to 0 or larger than the length of the <paramref name="stream"/>.</exception>
		///// <exception cref="System.ArgumentException">Thrown when the data in the stream cannot be read by the image codec.</exception>
		///// <exception cref="System.IO.IOException">Thrown when the stream is write-only.
		///// <para>-or-</para>
		///// <para>The image file is corrupted or unable to be read by a codec.</para>
		///// </exception>
		///// <exception cref="System.IO.EndOfStreamException">Thrown if an attempt to read beyond the end of the stream is made.</exception>
		//public static GorgonImageData FromStream(Stream stream, int size, GorgonImageCodec codec)
		//{
		//	GorgonImageData result = null;

		//	if (stream == null)
		//	{
		//		throw new ArgumentNullException(nameof(stream));
		//	}

		//	if (codec == null)
		//	{
		//		throw new ArgumentNullException(nameof(codec));
		//	}

		//	if ((size <= 0) || (size > stream.Length))
		//	{
		//		throw new ArgumentOutOfRangeException(nameof(size), string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, size, 1, stream.Length));
		//	}

		//	if (!stream.CanRead)
		//	{
		//		throw new IOException(Resources.GORGFX_STREAM_WRITE_ONLY);
		//	}

		//	// Check to see if the decoder can actually read the data in the stream.
		//	if (!codec.IsReadable(stream))
		//	{
		//		throw new IOException(string.Format(Resources.GORGFX_IMAGE_FILE_INCORRECT_DECODER, codec.Codec));
		//	}

		//	// Just apply directly if we're already using a data stream.
		//	var gorgonDataStream = stream as GorgonDataStream;

		//	// Load the data into unmanaged memory.
		//	try
		//	{
		//		// Use a memory stream for ease of access.
		//		if (gorgonDataStream == null)
		//		{
		//			gorgonDataStream = new GorgonDataStream(size);
		//			stream.CopyToStream(gorgonDataStream, size);
		//			gorgonDataStream.Position = 0;
		//		}

		//		result = codec.LoadFromStream(gorgonDataStream, size);

		//		// Post process our data.
		//		if ((codec.Width != 0) || (codec.Height != 0) || (codec.MipCount != 0) || (codec.Format != BufferFormat.Unknown))
		//		{
		//			codec.PostProcess(result);
		//		}
		//	}
		//	catch
		//	{
		//		result?.Dispose();
		//		throw;
		//	}
		//	finally
		//	{
		//		// If we haven't co-opted the pointer, then free the memory we've allocated.
		//		if (stream != gorgonDataStream)
		//		{
		//			gorgonDataStream?.Dispose();
		//		}
		//	}

		//	return result;
		//}

		/// <summary>
		/// Function to determine if the pixel format for this image can be converted to another pixel format.
		/// </summary>
		/// <param name="format">The pixel format to convert to.</param>
		/// <returns><b>true</b> if the the current pixel format and the requested pixel format can be converted, <b>false</b> if not.</returns>
		public bool CanConvertToFormat(DXGI.Format format)
		{
			if (format == DXGI.Format.Unknown)
			{
				return false;
			}

			if (format == Info.Format)
			{
				return true;
			}

			using (var wic = new WicUtilities())
			{
				return wic.CanConvertFormats(Info.Format,
				                             new[]
				                             {
					                             format
				                             }).Count > 0;
			}
		}

		/// <summary>
		/// Function to determine if the source format can convert to any of the formats in the destination list.
		/// </summary>
		/// <param name="destFormats">List of destination formats to compare.</param>
		/// <returns>A list of formats that the source format can be converted into, or an empty array if no conversion is possible.</returns>
		public IReadOnlyList<DXGI.Format> CanConvertToFormats(DXGI.Format[] destFormats)
		{
			if ((destFormats == null)
				|| (destFormats.Length == 0))
			{
				return new DXGI.Format[0];
			}
			
			using (var wic = new WicUtilities())
			{
				return wic.CanConvertFormats(Info.Format, destFormats);
			}
		}

		/// <summary>
		/// Function to copy the this image into the destination image.
		/// </summary>
		/// <param name="dest">The image that will receive the copy of this image.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="dest"/> parameter is <b>NULL</b>.</exception>
		/// <exception cref="ArgumentException">Thrown if the format, width, or height of the <paramref name="dest"/> image are not the same as this image.</exception>
		/// <remarks>
		/// <para>
		/// This will clone this image into the destination image specified by the <paramref name="dest"/> parameter. All information in the <paramref name="dest"/> image will be replaced by the copy made 
		/// by this method. If copying parts of an image into a new image is required, then see the <see cref="IGorgonImageBuffer"/>.<see cref="IGorgonImageBuffer.CopyTo"/> method.
		/// </para>
		/// </remarks>
		public void CopyTo(IGorgonImage dest)
		{
			if (dest == null)
			{
				throw new ArgumentNullException(nameof(dest));
			}

			// We need the concrete implementation for this, so we can get at its internals and force a copy.
			var concreteImage = (GorgonImage)dest;

			concreteImage._imageInfo = _imageInfo;
			concreteImage._imageData.Dispose();
			concreteImage.FormatInfo = new GorgonFormatInfo(Info.Format);
			concreteImage.SizeInBytes = CalculateSizeInBytes(Info);
			concreteImage.Initialize(ImageData, true);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_imageData?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImage" /> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonImageInfo"/> containing information used to create the image.</param>
		/// <param name="data">[Optional] A <see cref="IGorgonPointer"/> that points to a blob of existing image data.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>NULL</b>.</exception>
		/// <exception cref="GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		/// <remarks>
		/// <para>
		/// If the <paramref name="data"/> parameter is <b>NULL</b>, then a new, empty, image will be created, otherwise the buffer that is pointed at by <paramref name="data"/> will be wrapped by this 
		/// object to provide a view of the data as image data. The <paramref name="data"/> passed to this image must be large enough to accomodate the size of the image described by <paramref name="info"/>, 
		/// otherwise an exception will be thrown. To determine how large the image size will be, in bytes, use the static <see cref="CalculateSizeInBytes(IGorgonImageInfo,PitchFlags)"/> method to determine the 
		/// potential size of an image prior to creation.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// If the <paramref name="data"/> parameter is not omitted, then the user is responsible for managing the lifetime of the <see cref="IGorgonPointer"/> object passed in. Failure to do so may cause 
		/// a potential memory leak until garbage collection can recover the object.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonImage(IGorgonImageInfo info, IGorgonPointer data = null)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (info.Width < 1)
			{
				throw new ArgumentException(Resources.GORIMG_ERR_IMAGE_WIDTH_TOO_SMALL, nameof(info));
			}

			if ((info.Height < 1) && 
				((info.ImageType == ImageType.ImageCube) 
				|| (info.ImageType == ImageType.Image2D) 
				|| (info.ImageType == ImageType.Image3D)))
			{
				throw new ArgumentException(Resources.GORIMG_ERR_IMAGE_HEIGHT_TOO_SMALL, nameof(info));
			}

			if ((info.Depth < 1) && (info.ImageType == ImageType.Image3D))
			{
				throw new ArgumentException(Resources.GORIMG_ERR_IMAGE_DEPTH_TOO_SMALL, nameof(info));
			}

			FormatInfo = new GorgonFormatInfo(info.Format);

			// Create a copy of the settings so outside forces don't change it.
			SanitizeInfo(info);
			SizeInBytes = CalculateSizeInBytes(info);

			// Validate the image size.
			if ((data != null) && (SizeInBytes > data.Size))
			{
				throw new ArgumentException(string.Format(Resources.GORIMG_ERR_IMAGE_SIZE_MISMATCH, SizeInBytes, data.Size), nameof(data));
			}

			Initialize(data, false);
		}
		#endregion
	}
}
