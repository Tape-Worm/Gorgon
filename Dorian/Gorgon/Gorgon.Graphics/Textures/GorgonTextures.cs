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
using System.Drawing;
using D3D = SharpDX.Direct3D11;
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
		/// Property to return the texture for the Gorgon logo.
		/// </summary>
		public GorgonTexture2D GorgonLogo
		{
			get;
			private set;
		}

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

		/// <summary>
		/// Property to return the maximum depth of a texture.
		/// </summary>
		public int MaxDepth
		{
			get
			{
				if (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
					return 512;
				else
					return 2048;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the width and height as powers of two.
		/// </summary>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <param name="depth">Depth of the texture.</param>
		/// <returns>The width, height and depth bumped to the nearest power of two.</returns>
		private Tuple<int, int, int> GetPow2Size(int width, int height, int depth)
		{
			// Do width.
			while ((width != 0) && ((width & (width - 1)) != 0))
			{
				width++;
			}

			// Do height.			
			while ((height != 0) && ((height & (height - 1)) != 0))
			{
				height++;
			}

			// Do depth.
			while ((depth != 0) && ((depth & (depth - 1)) != 0))
			{
				depth++;
			}

			return new Tuple<int, int, int>(width, height, depth);
		}

		/// <summary>
		/// Function to validate the 3D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		/// <param name="isReading">TRUE if reading from a stream, FALSE if not.</param>
		private void ValidateTexture3D(ref GorgonTexture3DSettings settings, bool isReading)
		{
			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_IntNormal;

			if (settings.Width < 0)
				settings.Width = 0;

			if (settings.Height < 0)
				settings.Height = 0;

			// Direct3D 9 video devices require resizing to power of two.
			if (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
			{
				Tuple<int, int, int> newSize = GetPow2Size(settings.Width, settings.Height, settings.Depth);
				settings.Width = newSize.Item1;
				settings.Height = newSize.Item2;
				settings.Depth = newSize.Item3;
			}

			if (!isReading)
			{
				if (settings.Width <= 0)
					throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be at least 1.");
				if (settings.Height <= 0)
					throw new GorgonException(GorgonResult.CannotCreate, "The texture height must be at least 1.");
				if (settings.Depth <= 0)
					throw new GorgonException(GorgonResult.CannotCreate, "The texture depth must be at least 1.");
			}

			if (settings.Width > MaxDepth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be less than " + MaxDepth.ToString() + ".");
			if (settings.Height > MaxDepth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture height must be less than " + MaxDepth.ToString() + ".");
			if (settings.Depth > MaxDepth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture depth must be less than " + MaxDepth.ToString() + ".");

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports3DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture.  The format '" + settings.Format.ToString() + "' is not supported by the hardware.");


		}

		/// <summary>
		/// Function to validate the 2D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		/// <param name="isReading">TRUE if reading from a stream, FALSE if not.</param>
		internal void ValidateTexture2D(ref GorgonTexture2DSettings settings, bool isReading)
		{			
			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Multisampling.Count == 0)
				settings.Multisampling = new GorgonMultiSampling(1, 0);

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_IntNormal;

			if (settings.Width < 0)
				settings.Width = 0;

			if (settings.Height < 0)
				settings.Height = 0;

			// Direct3D 9 video devices require resizing to power of two if there is more than 1 mip level.
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (settings.MipCount != 1))
			{
				Tuple<int, int, int> newSize = GetPow2Size(settings.Width, settings.Height, 0);
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
				throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be less than " + MaxWidth.ToString() + ".");
			if (settings.Height > MaxHeight)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture height must be less than " + MaxHeight.ToString() + ".");

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports2DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture.  The format '" + settings.Format.ToString() + "' is not supported by the hardware.");			
		}

		/// <summary>
		/// Function to validate the 1D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		/// <param name="isReading">TRUE if reading from a stream, FALSE if not.</param>
		private void ValidateTexture1D(ref GorgonTexture1DSettings settings, bool isReading)
		{
			if (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
				throw new GorgonException(GorgonResult.CannotCreate, "1 dimensional textures are not supported on SM2_a_b devices.");

			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Format == BufferFormat.Unknown)
				settings.Format = BufferFormat.R8G8B8A8_IntNormal;

			if (settings.Width < 0)
				settings.Width = 0;

			// Direct3D 9 video devices require resizing to power of two if there is more than 1 mip level.
			if ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (settings.MipCount != 1))
			{
				Tuple<int, int, int> newSize = GetPow2Size(settings.Width, 0, 0);
				settings.Width = newSize.Item1;
			}

			if (!isReading)
			{
				if (settings.Width <= 0)
					throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be at least 1.");
			}

			if (settings.Width > MaxWidth)
				throw new GorgonException(GorgonResult.CannotCreate, "The texture width must be less than " + MaxWidth.ToString() + ".");

			// Check the format to see if it's available on this device.
			if (!_graphics.VideoDevice.Supports1DTextureFormat(settings.Format))
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture.  The format '" + settings.Format.ToString() + "' is not supported by the hardware.");
		}

		/// <summary>
		/// Function to load a texture from a GDI+ bitmap object.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="bitmap">Bitmap to load.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <returns>The new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromGDIBitmap(string name, Image bitmap, GorgonTexture2DSettings settings)
		{
			GorgonTexture2D result = null;
			MemoryStream stream = null;

			try
			{
				stream = new MemoryStream();
				bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				stream.Position = 0;
				result = FromStream(name, stream, (int)stream.Length, settings);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
			}			

			return result;
		}

		/// <summary>
		/// Function to load a 3D image from a byte array.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="imageData"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture3D FromMemory(string name, byte[] imageData, GorgonTexture3DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			D3D.ImageInformation? info = null;
			GorgonTexture3D result = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(imageData, "imageData");

			if (imageData.Length == 0)
				throw new ArgumentException("There is no data for the image.", "imageData");

			// Get the file information.
			info = D3D.ImageInformation.FromMemory(imageData);

			// Assign defaults.
			if (info != null)
			{
				// Only load 3D textures.
				if (info.Value.ResourceDimension != D3D.ResourceDimension.Texture3D)
					throw new ArgumentException("The specified texture is not a 3D texture.", "stream");

				if (settings.Format == BufferFormat.Unknown)
					settings.Format = (BufferFormat)info.Value.Format;
				if (settings.Width < 1)
					settings.Width = info.Value.Width;
				if (settings.Height < 1)
					settings.Height = info.Value.Height;
				if (settings.Depth < 1)
					settings.Depth = info.Value.Depth;
				if (settings.MipCount == 0)
					settings.MipCount = info.Value.MipLevels;
			}

			ValidateTexture3D(ref settings, true);

			result = new GorgonTexture3D(_graphics, name, settings);
			result.Initialize(imageData, filter, mipFilter);

			_graphics.AddTrackedObject(result);

			return result;
		}

		/// <summary>
		/// Function to load a 3D image from a byte array.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="imageData"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture3D FromMemory(string name, byte[] imageData, GorgonTexture3DSettings settings)
		{
			return FromMemory(name, imageData, settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to load a 2D image from a byte array.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="imageData"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromMemory(string name, byte[] imageData, GorgonTexture2DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			D3D.ImageInformation? info = null;
			GorgonTexture2D result = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(imageData, "imageData");

			if (imageData.Length == 0)
				throw new ArgumentException("There is no data for the image.", "imageData");			

			// Get the file information.
			info = D3D.ImageInformation.FromMemory(imageData);

			// Assign defaults.
			if (info != null)
			{
				// Only load 2D textures.
				if (info.Value.ResourceDimension != D3D.ResourceDimension.Texture2D)
					throw new ArgumentException("The specified texture is not a 2D texture.", "stream");

				if (settings.Format == BufferFormat.Unknown)
					settings.Format = (BufferFormat)info.Value.Format;
				if (settings.Width < 1)
					settings.Width = info.Value.Width;
				if (settings.Height < 1)
					settings.Height = info.Value.Height;
				if (settings.MipCount == 0)
					settings.MipCount = info.Value.MipLevels;
				if (settings.ArrayCount == 0)
					settings.ArrayCount = info.Value.ArraySize;
			}

			ValidateTexture2D(ref settings, true);

			result = GorgonTexture2D.CreateTexture(_graphics, name, settings);
			result.Initialize(imageData, filter, mipFilter);

			_graphics.AddTrackedObject(result);

			return result;
		}

		/// <summary>
		/// Function to load a 2D image from a byte array.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="imageData"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromMemory(string name, byte[] imageData, GorgonTexture2DSettings settings)
		{
			return FromMemory(name, imageData, settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to load a 1D image from a byte array.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="imageData"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture1D FromMemory(string name, byte[] imageData, GorgonTexture1DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			D3D.ImageInformation? info = null;
			GorgonTexture1D result = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(imageData, "imageData");

			if (imageData.Length == 0)
				throw new ArgumentException("There is no data for the image.", "imageData");

			// Get the file information.
			info = D3D.ImageInformation.FromMemory(imageData);

			// Assign defaults.
			if (info != null)
			{
				// Only load 2D textures.
				if (info.Value.ResourceDimension != D3D.ResourceDimension.Texture2D)
					throw new ArgumentException("The specified texture is not a 2D texture.", "stream");

				if (settings.Format == BufferFormat.Unknown)
					settings.Format = (BufferFormat)info.Value.Format;
				if (settings.Width < 1)
					settings.Width = info.Value.Width;
				if (settings.MipCount == 0)
					settings.MipCount = info.Value.MipLevels;
				if (settings.ArrayCount == 0)
					settings.ArrayCount = info.Value.ArraySize;
			}

			ValidateTexture1D(ref settings, true);

			result = new GorgonTexture1D(_graphics, name, settings);
			result.Initialize(imageData, filter, mipFilter);

			_graphics.AddTrackedObject(result);

			return result;
		}

		/// <summary>
		/// Function to load a 1D image from a byte array.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="imageData"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture1D FromMemory(string name, byte[] imageData, GorgonTexture1DSettings settings)
		{
			return FromMemory(name, imageData, settings, ImageFilters.None, ImageFilters.None);
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
		/// <returns>A new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="stream"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture3D FromStream(string name, Stream stream, int length, GorgonTexture3DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			byte[] imageData = new byte[length];

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(stream, "stream");

			// Read the image data.
			stream.Read(imageData, 0, length);

			return FromMemory(name, imageData, settings, filter, mipFilter);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream to load the texture from.</param>
		/// <param name="length">Size of the texture in the stream, in bytes.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="stream"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture3D FromStream(string name, Stream stream, int length, GorgonTexture3DSettings settings)
		{
			return FromStream(name, stream, length, settings, ImageFilters.None, ImageFilters.None);
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
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromStream(string name, Stream stream, int length, GorgonTexture2DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			byte[] imageData = new byte[length];

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(stream, "stream");

			// Read the image data.
			stream.Read(imageData, 0, length);

			return FromMemory(name, imageData, settings, filter, mipFilter);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream to load the texture from.</param>
		/// <param name="length">Size of the texture in the stream, in bytes.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="stream"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromStream(string name, Stream stream, int length, GorgonTexture2DSettings settings)
		{
			return FromStream(name, stream, length, settings, ImageFilters.None, ImageFilters.None);
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
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="stream"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture1D FromStream(string name, Stream stream, int length, GorgonTexture1DSettings settings, ImageFilters filter, ImageFilters mipFilter)
		{
			byte[] imageData = new byte[length];

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(stream, "stream");

			// Read the image data.
			stream.Read(imageData, 0, length);

			return FromMemory(name, imageData, settings, filter, mipFilter);
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream to load the texture from.</param>
		/// <param name="length">Size of the texture in the stream, in bytes.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="stream"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture1D FromStream(string name, Stream stream, int length, GorgonTexture1DSettings settings)
		{
			return FromStream(name, stream, length, settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to load a 3D image from a file.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture3D FromFile(string name, string filePath, GorgonTexture3DSettings settings, ImageFilters filter, ImageFilters mipFilter)
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
		/// Function to load a 3D image from a file.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <returns>A new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture3D FromFile(string name, string filePath, GorgonTexture3DSettings settings)
		{
			return FromFile(name, filePath, settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to load a 2D image from a file.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
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
		/// Function to load a 2D image from a file.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromFile(string name, string filePath, GorgonTexture2DSettings settings)
		{
			return FromFile(name, filePath, settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to load a 1D image from a file.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="filter">Filtering to apply to the image.</param>
		/// <param name="mipFilter">Filtering to apply to the mip levels.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture1D FromFile(string name, string filePath, GorgonTexture1DSettings settings, ImageFilters filter, ImageFilters mipFilter)
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
		/// Function to load a 1D image from a file.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture1D FromFile(string name, string filePath, GorgonTexture1DSettings settings)
		{
			return FromFile(name, filePath, settings, ImageFilters.None, ImageFilters.None);
		}

		/// <summary>
		/// Function to create a new 3D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <param name="depth">Depth of the texture.</param>
		/// <param name="format">Format of the the texture.</param>
		/// <param name="usage">Usage for the texture.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture3D CreateTexture(string name, int width, int height, int depth, BufferFormat format, BufferUsage usage)
		{
			if (usage == BufferUsage.Immutable)
				throw new ArgumentException("Cannot use immutable without initialization data.", "usage");

			return CreateTexture(name, new GorgonTexture3DSettings()
			{
				Width = width,
				Height = height,
				Depth = depth,
				Format = format,
				MipCount = 1,
				Usage = usage
			}, null);
		}

		/// <summary>
		/// Function to create a new 2D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="width">Width of the texture.</param>
		/// <param name="height">Height of the texture.</param>
		/// <param name="format">Format of the the texture.</param>
		/// <param name="usage">Usage for the texture.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture2D CreateTexture(string name, int width, int height, BufferFormat format, BufferUsage usage)
		{
			if (usage == BufferUsage.Immutable)
				throw new ArgumentException("Cannot use immutable without initialization data.", "usage");

			return CreateTexture(name, new GorgonTexture2DSettings()
			{
				Width = width,
				Height = height,
				Format = format,
				MipCount = 1,
				ArrayCount = 1,
				Multisampling = new GorgonMultiSampling(1, 0),
				Usage = usage
			}, null);
		}

		/// <summary>
		/// Function to create a new 1D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="width">Width of the texture.</param>
		/// <param name="format">Format of the the texture.</param>
		/// <param name="usage">Usage for the texture.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture1D CreateTexture(string name, int width, BufferFormat format, BufferUsage usage)
		{
			if (usage == BufferUsage.Immutable)
				throw new ArgumentException("Cannot use immutable without initialization data.", "usage");

			return CreateTexture(name, new GorgonTexture1DSettings()
			{
				Width = width,
				Format = format,
				MipCount = 1,
				ArrayCount = 1,
				Usage = usage
			}, null);
		}

		/// <summary>
		/// Function to create a new 3D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <returns>A new 3D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL and the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture3D CreateTexture(string name, GorgonTexture3DSettings settings, GorgonTexture3DData? data)
		{
			GorgonTexture3D texture = null;

			GorgonDebug.AssertParamString(name, "name");

			if ((settings.Usage == BufferUsage.Immutable) && (data == null))
				throw new ArgumentException("Immutable textures require data to initialize them.", "data");

			ValidateTexture3D(ref settings, false);

			texture = new GorgonTexture3D(_graphics, name, settings);
			texture.Initialize(data);

			_graphics.AddTrackedObject(texture);
			return texture;
		}

		/// <summary>
		/// Function to create a new 2D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <returns>A new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL and the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture2D CreateTexture(string name, GorgonTexture2DSettings settings, GorgonTexture2DData? data)
		{
			GorgonTexture2D texture = null;

			GorgonDebug.AssertParamString(name, "name");

			if ((settings.Usage == BufferUsage.Immutable) && (data == null))
				throw new ArgumentException("Immutable textures require data to initialize them.", "data");
			
			ValidateTexture2D(ref settings, false);

			texture = GorgonTexture2D.CreateTexture(_graphics, name, settings);
			texture.Initialize(data);

			_graphics.AddTrackedObject(texture);
			return texture;
		}

		/// <summary>
		/// Function to create a new 1D texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL and the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public GorgonTexture1D CreateTexture(string name, GorgonTexture1DSettings settings, GorgonDataStream data)
		{
			GorgonTexture1D texture = null;

			GorgonDebug.AssertParamString(name, "name");

			if ((settings.Usage == BufferUsage.Immutable) && (data == null))
				throw new ArgumentException("Immutable textures require data to initialize them.", "data");

			ValidateTexture1D(ref settings, false);

			texture = new GorgonTexture1D(_graphics, name, settings);
			texture.Initialize(data);

			_graphics.AddTrackedObject(texture);
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
			GorgonLogo = FromGDIBitmap("Gorgon.Logo", Properties.Resources.GorgonLogo3, GorgonTexture2DSettings.FromFile);
		}
		#endregion
	}
}
