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
	/// Formats for image files.
	/// </summary>
	public enum ImageFileFormat
	{
		/// <summary>
		/// Portable network graphics.
		/// </summary>
		PNG = 3,
		/// <summary>
		/// Joint Photographic Experts Group.
		/// </summary>
		JPG = 1,
		/// <summary>
		/// Windows bitmap.
		/// </summary>
		BMP = 0,
		/// <summary>
		/// Direct Draw Surface.
		/// </summary>
		DDS = 4
	}

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
		private GorgonTexture2D _logo = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the texture for the Gorgon logo.
		/// </summary>
		public GorgonTexture2D GorgonLogo
		{
			get
			{
				if (_logo == null)
				{
					_logo = FromGDIBitmap("Gorgon.Logo", Properties.Resources.Gorgon_2_x_Logo_Small);

					// Don't track this image.
					_graphics.RemoveTrackedObject(GorgonLogo);
				}

				return _logo;
			}
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
		/// Function to return correct settings object for the specified texture.
		/// </summary>
		/// <typeparam name="T">Type of texture.</typeparam>
		/// <returns>The correct settings object.</returns>
		private ITextureSettings GetSettings<T>()
			where T : GorgonTexture
		{
			if (typeof(T) == typeof(GorgonTexture1D))
				return new GorgonTexture1DSettings();

			if (typeof(T) == typeof(GorgonTexture2D))
				return new GorgonTexture2DSettings();

			if (typeof(T) == typeof(GorgonTexture3D))
				return new GorgonTexture3DSettings();

			throw new InvalidCastException("The settings could not be determined for the type '" + typeof(T).FullName + "'.");
		}

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
		private void ValidateTexture3D(ref ITextureSettings settings, bool isReading)
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
		/// Function to perform clean up of resources.
		/// </summary>
		internal void CleanUp()
		{
			if (_logo != null)
				_logo.Dispose();
			_logo = null;
		}

		/// <summary>
		/// Function to validate the 2D texture settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		/// <param name="isReading">TRUE if reading from a stream, FALSE if not.</param>
		internal void ValidateTexture2D(ref ITextureSettings settings, bool isReading)
		{			
			if (settings.ArrayCount < 1)
				settings.ArrayCount = 1;

			if (settings.ArrayCount > 2048)
				settings.ArrayCount = 2048;

			if (settings.IsTextureCube)
			{
				if ((settings.ArrayCount != 6) && ((_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) || (_graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
					throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture cube array.  SM2_a_b and SM4 devices require a maximum of 6 faces.");

				if ((settings.ArrayCount % 6) != 0)
					throw new GorgonException(GorgonResult.CannotCreate, "Cannot create the texture cube array.  The array count is not a multiple of 6.");
			}

			if (settings.MipCount < 0)
				settings.MipCount = 0;

			if (settings.Multisampling.Count == 0)
				settings.Multisampling = new GorgonMultisampling(1, 0);

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
		private void ValidateTexture1D(ref ITextureSettings settings, bool isReading)
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
		/// <returns>The new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or <paramref name="bitmap"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromGDIBitmap(string name, Image bitmap)
		{
			GorgonDebug.AssertNull<Image>(bitmap, "bitmap");

			GorgonTexture2DSettings settings = new GorgonTexture2DSettings()
			{
				Width = bitmap.Width,
				Height = bitmap.Height
			};

			return FromGDIBitmap(name, bitmap, settings);
		}

		/// <summary>
		/// Function to load a texture from a GDI+ bitmap object.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="bitmap">Bitmap to load.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <returns>The new 2D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="bitmap"/> or the <paramref name="settings"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public GorgonTexture2D FromGDIBitmap(string name, Image bitmap, GorgonTexture2DSettings settings)
		{
			GorgonTexture2D result = null;
			MemoryStream stream = null;

			GorgonDebug.AssertNull<Image>(bitmap, "bitmap");
			GorgonDebug.AssertNull<GorgonTexture2DSettings>(settings, "settings");

			try
			{
				stream = new MemoryStream();
				bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				stream.Position = 0;
				result = FromStream<GorgonTexture2D>(name, stream, (int)stream.Length, settings);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
			}			

			return result;
		}

		/// <summary>
		/// Function to load a texture from a byte array.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <returns>A new texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or <paramref name="imageData"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public T FromMemory<T>(string name, byte[] imageData)
			where T : GorgonTexture
		{
			return FromMemory<T>(name, imageData, GetSettings<T>());
		}

		/// <summary>
		/// Function to load a texture from a byte array.
		/// </summary>
		/// <typeparam name="T">Type of texture to load.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="imageData">Array containing the image data.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="imageData"/> or the <paramref name="settings"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the imageData parameter is empty.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public T FromMemory<T>(string name, byte[] imageData, ITextureSettings settings)
			where T : GorgonTexture
		{
			D3D.ImageInformation? info = null;
			GorgonTexture result = null;

			GorgonDebug.AssertNull<ITextureSettings>(settings, "settings");
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

			if (typeof(T) == typeof(GorgonTexture1D))
			{
				ValidateTexture1D(ref settings, true);
				result = new GorgonTexture1D(_graphics, name, settings);
			}
			else
			{
				if (typeof(T) == typeof(GorgonTexture2D))
				{
					ValidateTexture2D(ref settings, true);
					result = GorgonTexture2D.CreateTexture(_graphics, name, settings);
				}
				else
				{
					if (typeof(T) == typeof(GorgonTexture3DSettings))
					{
						ValidateTexture3D(ref settings, true);
						result = new GorgonTexture3D(_graphics, name, settings);
					}
					else
						throw new ArgumentException("Unknown settings type '" + settings.GetType().FullName + "'.", "settings");
				}
				
			}		

			result.InitializeFileData(imageData);

			_graphics.AddTrackedObject(result);

			return (T)result;
		}

		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <typeparam name="T">Type of texture.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream to load the texture from.</param>
		/// <param name="length">Size of the texture in the stream, in bytes.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or <paramref name="stream"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public T FromStream<T>(string name, Stream stream, int length)
			where T : GorgonTexture
		{
			return FromStream<T>(name, stream, length, GetSettings<T>());
		}
		
		/// <summary>
		/// Function to load an image from a stream.
		/// </summary>
		/// <typeparam name="T">Type of texture.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="stream">Stream to load the texture from.</param>
		/// <param name="length">Size of the texture in the stream, in bytes.</param>
		/// <param name="settings">Settings to apply to the texture.</param>
		/// <returns>A new 1D texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="stream"/> or <paramref name="settings"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public T FromStream<T>(string name, Stream stream, int length, ITextureSettings settings)
			where T : GorgonTexture
		{
			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertNull(stream, "stream");

			byte[] imageData = new byte[length];

			// Read the image data.
			stream.Read(imageData, 0, length);

			return FromMemory<T>(name, imageData, settings);
		}

		/// <summary>
		/// Function to load a texture from a file.
		/// </summary>
		/// <typeparam name="T">Type of texture.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="settings">Settings to apply to the loaded texture.</param>
		/// <returns>A new texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="filePath"/> or <paramref name="settings"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public T FromFile<T>(string name, string filePath, ITextureSettings settings)
			where T : GorgonTexture
		{
			Stream stream = null;

			try
			{
				stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				return FromStream<T>(name, stream, (int)stream.Length, settings);
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
			}
		}

		/// <summary>
		/// Function to load a texture from a file.
		/// </summary>
		/// <typeparam name="T">Type of texture.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="filePath">Path to the file.</param>
		/// <returns>A new texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or <paramref name="filePath"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name or the filePath parameters are empty strings.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too large or too small.
		/// <para>-or-</para>
		/// <para>Thrown when the format is not supported.</para>
		/// </exception>
		public T FromFile<T>(string name, string filePath)
			where T : GorgonTexture
		{
			return FromFile<T>(name, filePath, GetSettings<T>());
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

			return CreateTexture<GorgonTexture3D>(name, new GorgonTexture3DSettings()
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

			return CreateTexture<GorgonTexture2D>(name, new GorgonTexture2DSettings()
			{
				Width = width,
				Height = height,
				Format = format,
				MipCount = 1,
				ArrayCount = 1,
				Multisampling = new GorgonMultisampling(1, 0),
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

			return CreateTexture<GorgonTexture1D>(name, new GorgonTexture1DSettings()
			{
				Width = width,
				Format = format,
				MipCount = 1,
				ArrayCount = 1,
				Usage = usage
			}, null);
		}

		/// <summary>
		/// Function to create a new texture.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <returns>A new texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="settings"/> parameters are NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public T CreateTexture<T>(string name, ITextureSettings settings)
			where T : GorgonTexture
		{
			return CreateTexture<T>(name, settings, null);
		}

		/// <summary>
		/// Function to create a new texture.
		/// </summary>
		/// <typeparam name="T">Type of texture to create.</typeparam>
		/// <param name="name">Name of the texture.</param>
		/// <param name="settings">Settings for the texture.</param>
		/// <param name="data">Data used to initialize the texture.</param>
		/// <returns>A new texture.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or the <paramref name="settings"/> parameters are NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="data"/> parameter is NULL and the usage is set to immutable.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the texture size is too small or large.
		/// <para>-or-</para>
		/// <para>Thrown when the texture format isn't supported by the hardware.</para>
		/// </exception>
		public T CreateTexture<T>(string name, ITextureSettings settings, IEnumerable<ISubResourceData> data)
			where T : GorgonTexture
		{
			GorgonTexture texture = null;

			GorgonDebug.AssertNull<ITextureSettings>(settings, "settings");
			GorgonDebug.AssertParamString(name, "name");

			if ((settings.Usage == BufferUsage.Immutable) && ((data == null) || (data.Count() == 0)))
				throw new ArgumentException("Immutable textures require data to initialize them.", "data");

			if (typeof(T) == typeof(GorgonTexture1D))
			{
				ValidateTexture1D(ref settings, false);
				texture = new GorgonTexture1D(_graphics, name, settings);
			}
			else
			{
				if (typeof(T) == typeof(GorgonTexture2D))
				{
					ValidateTexture1D(ref settings, false);
					texture = GorgonTexture2D.CreateTexture(_graphics, name, settings);
				}
				else
				{
					if (typeof(T) == typeof(GorgonTexture3D))
					{
						ValidateTexture3D(ref settings, false);
						texture = new GorgonTexture3D(_graphics, name, settings);
					}
					else
						throw new ArgumentException("The settings type '" + settings.GetType().FullName + "' is unknown.", "settings");
				}
			}

			texture.Initialize(data);

			_graphics.AddTrackedObject(texture);
			return (T)texture;
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
