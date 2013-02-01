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
// Created: Monday, January 28, 2013 11:04:37 PM
// 

// This code was adapted from the SharpDX ToolKit by Alexandre Mutel, which was adapted 
// from the DirectXText library by Chuck Walburn:
#region SharpDX and DirectXTex Licenses.
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
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTex http://directxtex.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the 
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and 
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to 
// the software.
// A "contributor" is any person that distributes its contribution under this 
// license.
// "Licensed patents" are a contributor's patent claims that read directly on 
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the 
// license conditions and limitations in section 3, each contributor grants 
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and 
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a 
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution 
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any 
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that 
// you claim are infringed by the software, your patent license from such 
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all 
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you 
// may do so only under this license by including a complete copy of this 
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that 
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license 
// cannot change. To the extent permitted under your local laws, the 
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.
#endregion

// SharpDX is available from http://sharpdx.org
// DirectXTex is available from http://directxtex.codeplex.com

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WIC = SharpDX.WIC;
using GorgonLibrary.IO;
using GorgonLibrary.Graphics;
using GorgonLibrary.Native;

namespace Tester_Graphics
{
	/// <summary>
	/// Flags for WIC image data.
	/// </summary>
	[Flags]
	internal enum WICFlags
	{
		/// <summary>
		/// Nothing.
		/// </summary>
		None = 0x0,
		/// <summary>
		/// Loads DXGI 1.1 BGR formats as DXGI_FORMAT_R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats 
		/// </summary>
		ForceRgb = 0x1,
		/// <summary>
		/// Loads DXGI 1.1 X2 10:10:10:2 format as DXGI_FORMAT_R10G10B10A2_UNORM 
		/// </summary>
		NoX2Bias = 0x2,
		/// <summary>
		/// Loads all images in a multi-frame file, converting/resizing to match the first frame as needed, defaults to 0th frame otherwise
		/// </summary>
		AllFrames = 0x10,
		/// <summary>
		/// Use ordered 4x4 dithering for any required conversions
		/// </summary>
		Dither = 0x10000,
		/// <summary>
		/// Use error-diffusion dithering for any required conversions
		/// </summary>
		DitherDiffusion = 0x20000,
		/// <summary>
		/// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
		/// </summary>
		FilterPoint = 0x100000,
		/// <summary>
		/// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
		/// </summary>
		FilterLinear = 0x200000,
		/// <summary>
		/// Filtering mode to use for any required image resizing (only needed when loading arrays of differently sized images; defaults to Fant)
		/// </summary>
		FilterCubic = 0x300000,
		/// <summary>
		/// Combination of Linear and Box filter
		/// </summary>
		FilterFant = 0x400000		
	};

	public class WICLoad
		: IDisposable
	{
		#region Value Types.
		/// <summary>
		/// A value to hold a WIC to Gorgon buffer format value.
		/// </summary>
		private struct WICGorgonFormat
		{
			/// <summary>
			/// WIC GUID to convert from/to.
			/// </summary>
			public Guid WICGuid;
			/// <summary>
			/// Gorgon buffer format to convert from/to.
			/// </summary>
			public BufferFormat GorgonFormat;

			/// <summary>
			/// Initializes a new instance of the <see cref="WICGorgonFormat" /> struct.
			/// </summary>
			/// <param name="guid">The GUID.</param>
			/// <param name="format">The format.</param>
			public WICGorgonFormat(Guid guid, BufferFormat format)
			{
				WICGuid = guid;
				GorgonFormat = format;
			}
		}

		/// <summary>
		/// A value to hold a nearest supported format conversion.
		/// </summary>
		private struct WICNearest
		{
			/// <summary>
			/// Source format to convert from.
			/// </summary>
			public Guid Source;
			/// <summary>
			/// Destination format to convert to.
			/// </summary>
			public Guid Destination;

			/// <summary>
			/// Initializes a new instance of the <see cref="WICNearest" /> struct.
			/// </summary>
			/// <param name="source">The source.</param>
			/// <param name="dest">The destination.</param>
			public WICNearest(Guid source, Guid dest)
			{
				Source = source;
				Destination = dest;
			}
		}
		#endregion

		#region Variables.
		private bool _disposed = false;																// Flag to indicate that the object was disposed.
		private WIC.ImagingFactory _factory = null;													// Factory for WIC image data.

		private readonly WICGorgonFormat[] _wicGorgonFormats = new []								// Formats for conversion between Gorgon and WIC.
		{
			new WICGorgonFormat(WIC.PixelFormat.Format128bppRGBAFloat, BufferFormat.R32G32B32A32_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format64bppRGBAHalf, BufferFormat.R16G16B16A16_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format64bppRGBA, BufferFormat.R16G16B16A16_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA, BufferFormat.R8G8B8A8_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppBGRA, BufferFormat.B8G8R8A8_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppBGR, BufferFormat.B8G8R8X8_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA1010102XR, BufferFormat.R10G10B10_XR_BIAS_A2_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBA1010102, BufferFormat.R10G10B10A2_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBE, BufferFormat.R9G9B9E5_SharedExp),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppBGR565, BufferFormat.B5G6R5_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppBGRA5551, BufferFormat.B5G5R5A1_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppRGBE, BufferFormat.R9G9B9E5_SharedExp),
			new WICGorgonFormat(WIC.PixelFormat.Format32bppGrayFloat, BufferFormat.R32_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppGrayHalf, BufferFormat.R16_Float),
			new WICGorgonFormat(WIC.PixelFormat.Format16bppGray, BufferFormat.R16_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format8bppGray, BufferFormat.R8_UIntNormal),
			new WICGorgonFormat(WIC.PixelFormat.Format8bppAlpha, BufferFormat.A8_UIntNormal)
		};

		private readonly WICNearest[] _wicBestFitFormat = new []										// Best fit for supported format conversions.
		{
            new WICNearest(WIC.PixelFormat.Format1bppIndexed, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format2bppIndexed, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format4bppIndexed, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format8bppIndexed, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format2bppGray, WIC.PixelFormat.Format8bppGray),
            new WICNearest(WIC.PixelFormat.Format4bppGray, WIC.PixelFormat.Format8bppGray),
            new WICNearest(WIC.PixelFormat.Format16bppGrayFixedPoint, WIC.PixelFormat.Format16bppGrayHalf),
            new WICNearest(WIC.PixelFormat.Format32bppGrayFixedPoint, WIC.PixelFormat.Format32bppGrayFloat),
            new WICNearest(WIC.PixelFormat.Format16bppBGR555, WIC.PixelFormat.Format16bppBGRA5551),
            new WICNearest(WIC.PixelFormat.Format32bppBGR101010, WIC.PixelFormat.Format32bppRGBA1010102),
            new WICNearest(WIC.PixelFormat.Format24bppBGR, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format24bppRGB, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format32bppPBGRA, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format32bppPRGBA, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format48bppRGB, WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(WIC.PixelFormat.Format48bppBGR, WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(WIC.PixelFormat.Format64bppBGRA, WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(WIC.PixelFormat.Format64bppPRGBA, WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(WIC.PixelFormat.Format64bppPBGRA, WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(WIC.PixelFormat.Format48bppRGBFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(WIC.PixelFormat.Format48bppBGRFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(WIC.PixelFormat.Format64bppRGBAFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(WIC.PixelFormat.Format64bppBGRAFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(WIC.PixelFormat.Format64bppRGBFixedPoint, WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(WIC.PixelFormat.Format64bppRGBHalf, WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(WIC.PixelFormat.Format48bppRGBHalf, WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(WIC.PixelFormat.Format128bppPRGBAFloat, WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(WIC.PixelFormat.Format128bppRGBFloat, WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(WIC.PixelFormat.Format128bppRGBAFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(WIC.PixelFormat.Format128bppRGBFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(WIC.PixelFormat.Format32bppCMYK, WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(WIC.PixelFormat.Format64bppCMYK, WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(WIC.PixelFormat.Format40bppCMYKAlpha, WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(WIC.PixelFormat.Format80bppCMYKAlpha, WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(WIC.PixelFormat.Format96bppRGBFixedPoint, WIC.PixelFormat.Format128bppRGBAFloat)
		};
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the equiavlent Gorgon buffer format from a WIC GUID.
		/// </summary>
		/// <param name="wicGUID">WIC GUID to look up.</param>
		/// <returns>The buffer format if found, or BufferFormat.Unknown if not found.</returns>
		private BufferFormat GetFormat(Guid wicGUID)
		{
			for (int i = 0; i < _wicGorgonFormats.Length; i++)
			{
				if (_wicGorgonFormats[i].WICGuid == wicGUID)
				{
					return _wicGorgonFormats[i].GorgonFormat;
				}
			}

			return BufferFormat.Unknown;
		}

		/// <summary>
		/// Function to retrieve a WIC equivalent format GUID based on the Gorgon buffer format.
		/// </summary>
		/// <param name="format">Format to look up.</param>
		/// <returns>The GUID for the format, or NULL (Nothing in VB.Net) if not found.</returns>
		private Guid? GetGUID(BufferFormat format)
		{
			for (int i = 0; i < _wicGorgonFormats.Length; i++)
			{
				if (_wicGorgonFormats[i].GorgonFormat == format)
				{
					return _wicGorgonFormats[i].WICGuid;
				}
			}

			switch (format)
			{
				case BufferFormat.R8G8B8A8_UIntNormal_sRGB:
					return WIC.PixelFormat.Format32bppRGBA;
				case BufferFormat.D32_Float:
					return WIC.PixelFormat.Format32bppGrayFloat;
				case BufferFormat.D16_UIntNormal:
					return WIC.PixelFormat.Format16bppGray;
				case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
					return WIC.PixelFormat.Format32bppBGRA;
				case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
					return WIC.PixelFormat.Format32bppBGR;
			}

			return null;
		}

		/// <summary>
		/// Function to allocate a buffer and copy the data from the texture into that buffer.
		/// </summary>
		/// <param name="texture">Texture to copy from.</param>
		/// <param name="pitchInfo">Pitch information.</param>
		/// <returns>A data stream containing the buffer.</returns>
		private unsafe GorgonDataStream GetTextureData(GorgonTexture texture, out GorgonFormatPitch pitchInfo)
		{
			GorgonDataStream result = null;			

			using (GorgonTexture stagingTexture = texture.GetStagingTexture<GorgonTexture>())
			{
				var imageData = stagingTexture.Lock<ISubResourceData>(BufferLockFlags.Read);

				try
				{
					var formatPitch = texture.FormatInformation.GetPitch(texture.Settings.Width, texture.Settings.Height, PitchFlags.None);
					result = new GorgonDataStream(stagingTexture.SizeInBytes);

					// Pitch information is identical, copy all data in one burst.
					if ((imageData.RowPitch == formatPitch.RowPitch) && (imageData.SlicePitch == formatPitch.SlicePitch))
					{
						DirectAccess.MemoryCopy(result.UnsafePointer, imageData.Data.UnsafePointer, stagingTexture.SizeInBytes);
					}
					else
					{
						byte* dest = (byte*)result.UnsafePointer;
						int depth = (stagingTexture.Settings.Depth < 1) ? 1 : stagingTexture.Settings.Depth;

						// Otherwise, we'll need to copy by scanline.
						for (int z = 0; z < depth; z++)
						{
							byte* source = ((byte *)imageData.Data.UnsafePointer) + (z * imageData.SlicePitch);

							for (int y = 0; y < stagingTexture.Settings.Height; y++)
							{
								DirectAccess.MemoryCopy(dest, source, formatPitch.RowPitch);
								source += imageData.RowPitch;
								dest += formatPitch.RowPitch;
							}
						}
					}

					pitchInfo = new GorgonFormatPitch(formatPitch.RowPitch, formatPitch.SlicePitch, System.Drawing.Size.Empty);

					return result;
				}
				finally
				{
					stagingTexture.Unlock();
				}				
			}
		}

		/// <summary>
		/// Function to save a PNG encoded image to a stream.
		/// </summary>
		/// <param name="texture">Texture to write to the stream.</param>
		/// <param name="stream">Stream to contain the image data.</param>
		public unsafe void SavePNGToStream(GorgonTexture texture, Stream stream)
		{
			GorgonFormatPitch pitchInfo = default(GorgonFormatPitch);

			using (GorgonDataStream imageData = GetTextureData(texture, out pitchInfo))
			{
				using (WIC.BitmapEncoder encoder = new WIC.BitmapEncoder(_factory, WIC.ContainerFormatGuids.Png, stream))
				{
					using (WIC.BitmapFrameEncode frame = new WIC.BitmapFrameEncode(encoder))
					{
						Guid? wicGuid = GetGUID(texture.Settings.Format);

						if (wicGuid == null)
						{
							throw new ArgumentException("The format '" + texture.Settings.Format.ToString() + "' is not supported.", "texture");
						}

						Guid target = wicGuid.Value;

						frame.Initialize();
						frame.SetSize(texture.Settings.Width, texture.Settings.Height);
						frame.SetResolution(72, 72);
						frame.SetPixelFormat(ref target);

						if (wicGuid.Value != target)
						{
							SharpDX.DataRectangle rect = new SharpDX.DataRectangle(imageData.BasePointer, pitchInfo.RowPitch);
							using (var sourceData = new WIC.Bitmap(_factory, texture.Settings.Width, texture.Settings.Height, wicGuid.Value, rect, pitchInfo.SlicePitch))
							{
								using (var converter = new WIC.FormatConverter(_factory))
								{
									int bitsPerPixel = 0;
									int rowPitch = 0;
									int slicePitch = 0;

									converter.Initialize(sourceData, target, WIC.BitmapDitherType.None, null, 0, WIC.BitmapPaletteType.Custom);

									using (var component = new WIC.ComponentInfo(_factory, target))
									{
										if (component.ComponentType != WIC.ComponentType.PixelFormat)
										{
											throw new InvalidDataException("The bits per pixel could not be determined from the format.");
										}

										using (var pixelInfo = component.QueryInterfaceOrNull<WIC.PixelFormatInfo>())
										{
											if (pixelInfo == null)
											{
												throw new InvalidDataException("The bits per pixel could not be determined from the format.");
											}

											bitsPerPixel = pixelInfo.BitsPerPixel;
										}
									}

									rowPitch = (texture.Settings.Width * bitsPerPixel + 7) / 8;
									slicePitch = rowPitch * texture.Settings.Height;
									using (GorgonDataStream dataPointer = new GorgonDataStream(slicePitch))
									{
										converter.CopyPixels(rowPitch, dataPointer.BasePointer, slicePitch);
										frame.WritePixels(texture.Settings.Height, dataPointer.BasePointer, rowPitch, slicePitch);
									}
								}
							}
						}
						else
						{
							frame.WritePixels(texture.Settings.Height, imageData.BasePointer, pitchInfo.RowPitch, pitchInfo.SlicePitch);
						}
						frame.Commit();

						encoder.Commit();
					}
				}
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="WICLoad" /> class.
		/// </summary>
		public WICLoad()
		{
			_factory = new WIC.ImagingFactory();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_factory != null)
					{
						_factory.Dispose();
					}

					_factory = null;
				}
				
				_disposed = true;
			}
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
