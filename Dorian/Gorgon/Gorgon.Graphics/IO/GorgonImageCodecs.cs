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
// Created: Wednesday, February 6, 2013 5:52:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// Image codecs for reading/writing image/texture data.
	/// </summary>
	/// <remarks>Codecs are used to encode and decode image data to and from a file, buffer or stream.  Gorgon currently has support for the following codecs built in:
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
	/// <para>These codecs are system codecs and are always present.  However, a user may define their own codec to read and write images (e.g. you don't want to require WIC).  To do so, the user must create a codec object that inherits from 
	/// <see cref="GorgonLibrary.IO.GorgonImageCodec">GorgonImageCodec</see>.</para>
	/// </remarks>
	public static class GorgonImageCodecs
	{
		#region Variables.
		private static GorgonCodecDDS _dds = null;						// DDS file format.
		private static GorgonCodecTGA _tga = null;						// TGA file format.
		private static GorgonCodecWIC _png = null;						// PNG file format.
		private static GorgonCodecWIC _bmp = null;						// BMP file format.
		private static GorgonCodecWIC _tiff = null;						// TIFF file format.
		private static GorgonCodecWIC _wmp = null;						// WMP file format.
		private static GorgonCodecGIF _gif = null;						// GIF file format.
		private static GorgonCodecWIC _jpg = null;						// JPG file format.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Truevision Graphics Array codec.
		/// </summary>
		public static GorgonCodecTGA Tga
		{
			get
			{
				return _tga;
			}
		}

		/// <summary>
		/// Property to return the Direct Draw Surface codec.
		/// </summary>
		public static GorgonCodecDDS Dds
		{
			get
			{
				return _dds;
			}
		}

		/// <summary>
		/// Property to return the Portable Network Graphics codec.
		/// </summary>
		public static GorgonCodecWIC Png
		{
			get
			{
				return _png;
			}
		}

		/// <summary>
		/// Property to return the Tagged Image File Format codec.
		/// </summary>
		public static GorgonCodecWIC Tiff
		{
			get
			{
				return _tiff;
			}
		}

		/// <summary>
		/// Property to return the Windows Bitmap codec.
		/// </summary>
		public static GorgonCodecWIC Bmp
		{
			get
			{
				return _bmp;
			}
		}

		/// <summary>
		/// Property to return the Windows Media Photo codec.
		/// </summary>
		public static GorgonCodecWIC Wmp
		{
			get
			{
				return _wmp;
			}
		}

		/// <summary>
		/// Property to return Joint Photographic Experts Group codec.
		/// </summary>
		public static GorgonCodecWIC Jpeg
		{
			get
			{
				return _jpg;
			}
		}

		/// <summary>
		/// Property to return the Graphics Interchange Format codec.
		/// </summary>
		public static GorgonCodecGIF Gif
		{
			get
			{
				return _gif;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonImageCodecs" /> class.
		/// </summary>
		static GorgonImageCodecs()
		{
			_dds = new GorgonCodecDDS();
			_tga = new GorgonCodecTGA();
			_png = new GorgonCodecWIC("PNG", "Portable Network Graphics", new string[] { "png" }, SharpDX.WIC.ContainerFormatGuids.Png);
			_bmp = new GorgonCodecWIC("BMP", "Windows Bitmap", new string[] { "bmp", "dib" }, SharpDX.WIC.ContainerFormatGuids.Bmp);
			_tiff = new GorgonCodecWIC("TIFF", "Tagged Image File Format", new string[] { "tif", "tiff" }, SharpDX.WIC.ContainerFormatGuids.Tiff);
			_wmp = new GorgonCodecWIC("WMP", "Windows Media Photo", new string[] { "wmp" }, SharpDX.WIC.ContainerFormatGuids.Wmp);
			_jpg = new GorgonCodecWIC("JPG", "Joint Photographic Experts Group", new string[] { "jpg", "jpeg", "jpe", "jif", "jfif", "jfi" }, SharpDX.WIC.ContainerFormatGuids.Jpeg);
			_gif = new GorgonCodecGIF();
		}
		#endregion
	}
}
