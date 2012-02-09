#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Thursday, February 09, 2012 7:59:01 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Filters applied to an image when it is loaded from a stream, file, etc...
	/// </summary>
	[Flags()]
	public enum ImageFilters
	{
		///<summary>No scaling or filtering will take place. Pixels outside the bounds of the source image are assumed to be transparent black.</summary>
		None = 1,
		/// <summary>
		/// Each destination pixel is computed by sampling the nearest pixel from the source image.
		/// </summary>
		Point = 2,
		/// <summary>
		/// Each destination pixel is computed by sampling the four nearest pixels from the source image. This filter works best when the scale on both axes is less than two.
		/// </summary>
		Linear = 3,
		/// <summary>
		/// Every pixel in the source image contributes equally to the destination image.  This is the slowest of the filters.
		/// </summary>
		Triangle = 4,
		/// <summary>
		/// Each pixel is computed by averaging a 2x2(x2) box of pixels from the source image. This filter works only when the dimensions of the destination are half those of the source, as is the case with mipmaps.
		/// </summary>
		Box = 5,
		/// <summary>
		/// Pixels off the edge of the texture on the u-axis should be mirrored, not wrapped.
		/// </summary>
		MirrorU = 65536,
		/// <summary>
		/// Pixels off the edge of the texture on the v-axis should be mirrored, not wrapped.
		/// </summary>
		MirrorV = 131072,
		/// <summary>
		/// Pixels off the edge of the texture on the w-axis should be mirrored, not wrapped.
		/// </summary>
		MirrorW = 262144,
		/// <summary>
		/// The resulting image must be dithered using a 4x4 ordered dither algorithm. This happens when converting from one format to another.
		/// </summary>
		Dither = 524288,
		/// <summary>
		/// Do diffuse dithering on the image when changing from one format to another.
		/// </summary>
		DitherDiffusion = 1048576,
		/// <summary>
		/// Input data is in standard RGB (sRGB) color space.
		/// </summary>
		SRgbIn = 2097152,
		/// <summary>
		/// Output data is in standard RGB (sRGB) color space.
		/// </summary>
		SRgbOut = 4194304
	}

	/// <summary>
	/// Textures interface.
	/// </summary>
	public sealed class GorgonTextures
	{
		#region Variables.
		private GorgonGraphics _graphics = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the maximum width of a texture.
		/// </summary>
		public int MaxWidth
		{
			get
			{
				switch (_graphics.VideoDevice.SupportedFeatureLevel)
				{
					case DeviceFeatureLevel.SM2_a_b:
						return 4096;
					case DeviceFeatureLevel.SM4:
					case DeviceFeatureLevel.SM4_1:
						return 8192;
					default:
						return 16384;
				}
			}
		}

		/// <summary>
		/// Property to return the maximum height of a texture.
		/// </summary>
		public int MaxHeight
		{
			get
			{
				switch (_graphics.VideoDevice.SupportedFeatureLevel)
				{
					case DeviceFeatureLevel.SM2_a_b:
						return 4096;
					case DeviceFeatureLevel.SM4:
					case DeviceFeatureLevel.SM4_1:
						return 8192;
					default:
						return 16384;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the width and height as powers of two.
		/// </summary>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <returns>The width and height bumped to the nearest power of two.</returns>
		private Tuple<int, int> GetPow2Size(int width, int height)
		{
			// Do width.
			while ((width != 0) && ((width & (width - 1)) != 0))
			{
				width++;
			}

			while ((height != 0) && ((height & (height - 1)) != 0))
			{
				height++;
			}

			return new Tuple<int, int>(width, height);
		}

		/// <summary>
		/// Function to validate the 2D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		/// <param name="isReading">TRUE if reading from a stream, FALSE if not.</param>
		private void ValidateTexture2D(ref GorgonTexture2DSettings settings, bool isReading)
		{			
			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Multisampling.Count == 0)
				settings.Multisampling = new GorgonMultiSampling(1, 0);

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_UIntNormal;

			if (settings.Width < 0)
				settings.Width = 0;

			if (settings.Height < 0)
				settings.Height = 0;

			// Direct3D 9 video devices require resizing to power of two if there is more than 1 mip level.
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (settings.MipCount != 1))
			{
				Tuple<int, int> newSize = GetPow2Size(settings.Width, settings.Height);
				settings.Width = newSize.Item1;
				settings.Height = newSize.Item2;
			}

			if (!isReading)
			{
				if (settings.Width <= 0)
					throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be at least 1.");
				if (settings.Height <= 0)
					throw new GorgonException(GorgonResult.CannotCreate, "The texture height must be at least 1.");
			}

			if (settings.Width > MaxWidth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be less or equal to " + MaxWidth.ToString() + ".");
			if (settings.Height > MaxHeight)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture height must be less or equal to " + MaxHeight.ToString() + ".");

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports2DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture.  The format '" + settings.Format.ToString() + "' is not supported by the hardware.");			
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream to load the texture from.</param>
		/// <param name="length">Size of the texture in the stream, in bytes.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="stream"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonTexture2D FromStream(string name, Stream stream, int length, GorgonTexture2DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			GorgonTexture2D result = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(stream, "stream");

			ValidateTexture2D(ref settings, true);

			result = new GorgonTexture2D(_graphics, name, settings);
			result.Initialize(stream, length, filter, mipFilter);

			_graphics.TrackedObjects.Add(result);

			return result;
		}

		/// <summary>
		/// Function to load an image from a file.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		public GorgonTexture2D FromFile(string name, string filePath, GorgonTexture2DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			Stream stream = null;

			try
			{
				stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				return FromStream(name, stream, (int)stream.Length, settings, filter, mipFilter);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
			}
		}

		/// <summary>
		/// Function to create a new 2D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <returns>A new 2D texture.</returns>
		public GorgonTexture2D CreateTexture(string name, GorgonTexture2DSettings settings, GorgonTexture2DData? data)
		{
			GorgonTexture2D texture = null;

			GorgonDebug.AssertParamString(name, "name");

			ValidateTexture2D(ref settings, false);

			texture = new GorgonTexture2D(_graphics, name, settings);
			texture.Initialize(data);

			_graphics.TrackedObjects.Add(texture);
			return texture;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextures"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface.</param>
		internal GorgonTextures(GorgonGraphics graphics)
		{
			_graphics = graphics;
		}
		#endregion
	}
}
