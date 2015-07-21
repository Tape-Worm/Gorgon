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
// Created: Friday, February 01, 2013 8:27:21 AM
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
using System.Drawing;
using System.Drawing.Imaging;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using Gorgon.IO;
using Gorgon.Math;
using SharpDX.WIC;
using Bitmap = SharpDX.WIC.Bitmap;
using DX = SharpDX;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Gorgon.Graphics
{
    /// <summary>
    /// A Gorgon image data converter for WIC images.
    /// </summary>
    class GorgonWICImage
        : IDisposable
    {
        #region Value Types.
        /// <summary>
        /// A value typ hold a WIC to System.Drawing.Imaging.PixelFormat value.
        /// </summary>
        private struct WICPixelFormat
        {
            /// <summary>
            /// WIC GUID to convert from/to.
            /// </summary>
            public readonly Guid WICGuid;
            /// <summary>
            /// Pixel format to convert from/to.
            /// </summary>
            public readonly PixelFormat PixelFormat;

            /// <summary>
            /// Initializes a new instance of the <see cref="WICPixelFormat" /> struct.
            /// </summary>
            /// <param name="guid">The GUID.</param>
            /// <param name="format">The format.</param>
            public WICPixelFormat(Guid guid, PixelFormat format)
            {
                WICGuid = guid;
                PixelFormat = format;
            }
        }

        /// <summary>
        /// A value to hold a WIC to Gorgon buffer format value.
        /// </summary>
        private struct WICGorgonFormat
        {
            /// <summary>
            /// WIC GUID to convert from/to.
            /// </summary>
            public readonly Guid WICGuid;
            /// <summary>
            /// Gorgon buffer format to convert from/to.
            /// </summary>
            public readonly BufferFormat GorgonFormat;

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
            public readonly Guid Source;
            /// <summary>
            /// Destination format to convert to.
            /// </summary>
            public readonly Guid Destination;

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

	    private bool _disposed;                             // Flag to indicate that the object was disposed.

        // Formats for conversion between System.Drawing.Images and WIC.
        private readonly WICPixelFormat[] _wicPixelFormats =                            
        {            
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format64bppPBGRA, PixelFormat.Format64bppPArgb),            
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format64bppBGRA, PixelFormat.Format64bppArgb),            
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format48bppBGR, PixelFormat.Format48bppRgb),            
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format32bppPBGRA, PixelFormat.Format32bppPArgb),            
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format32bppBGRA, PixelFormat.Format32bppArgb),
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format24bppBGR, PixelFormat.Format24bppRgb),
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format16bppGray, PixelFormat.Format16bppGrayScale),
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format16bppBGRA5551, PixelFormat.Format16bppArgb1555),
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format16bppBGR565, PixelFormat.Format16bppRgb565),
            new WICPixelFormat(SharpDX.WIC.PixelFormat.Format16bppBGR555, PixelFormat.Format16bppRgb555)
        };

        // Formats for conversion between Gorgon and WIC.
        private readonly WICGorgonFormat[] _wicGorgonFormats = 
		{
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format128bppRGBAFloat, BufferFormat.R32G32B32A32_Float),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format64bppRGBAHalf, BufferFormat.R16G16B16A16_Float),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format64bppRGBA, BufferFormat.R16G16B16A16_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppRGBA, BufferFormat.R8G8B8A8_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppBGRA, BufferFormat.B8G8R8A8_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppBGR, BufferFormat.B8G8R8X8_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppRGBA1010102XR, BufferFormat.R10G10B10_XR_BIAS_A2_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppRGBA1010102, BufferFormat.R10G10B10A2_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppRGBE, BufferFormat.R9G9B9E5_SharedExp),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format16bppBGR565, BufferFormat.B5G6R5_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format16bppBGRA5551, BufferFormat.B5G5R5A1_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppRGBE, BufferFormat.R9G9B9E5_SharedExp),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format32bppGrayFloat, BufferFormat.R32_Float),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format16bppGrayHalf, BufferFormat.R16_Float),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format16bppGray, BufferFormat.R16_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format8bppGray, BufferFormat.R8_UIntNormal),
			new WICGorgonFormat(SharpDX.WIC.PixelFormat.Format8bppAlpha, BufferFormat.A8_UIntNormal)
		};

        // Best fit for supported format conversions.
        private readonly WICNearest[] _wicBestFitFormat = 
		{
            new WICNearest(SharpDX.WIC.PixelFormat.Format1bppIndexed, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format2bppIndexed, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format4bppIndexed, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format8bppIndexed, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format2bppGray, SharpDX.WIC.PixelFormat.Format8bppGray),
            new WICNearest(SharpDX.WIC.PixelFormat.Format4bppGray, SharpDX.WIC.PixelFormat.Format8bppGray),
            new WICNearest(SharpDX.WIC.PixelFormat.Format16bppGrayFixedPoint, SharpDX.WIC.PixelFormat.Format16bppGrayHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format32bppGrayFixedPoint, SharpDX.WIC.PixelFormat.Format32bppGrayFloat),
            new WICNearest(SharpDX.WIC.PixelFormat.Format16bppBGR555, SharpDX.WIC.PixelFormat.Format16bppBGRA5551),
            new WICNearest(SharpDX.WIC.PixelFormat.Format32bppBGR101010, SharpDX.WIC.PixelFormat.Format32bppRGBA1010102),
            new WICNearest(SharpDX.WIC.PixelFormat.Format24bppBGR, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format24bppRGB, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format32bppPBGRA, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format32bppPRGBA, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format48bppRGB, SharpDX.WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format48bppBGR, SharpDX.WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppBGRA, SharpDX.WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppPRGBA, SharpDX.WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppPBGRA, SharpDX.WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format48bppRGBFixedPoint, SharpDX.WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format48bppBGRFixedPoint, SharpDX.WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppRGBAFixedPoint, SharpDX.WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppBGRAFixedPoint, SharpDX.WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppRGBFixedPoint, SharpDX.WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppRGBHalf, SharpDX.WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format48bppRGBHalf, SharpDX.WIC.PixelFormat.Format64bppRGBAHalf),
            new WICNearest(SharpDX.WIC.PixelFormat.Format128bppPRGBAFloat, SharpDX.WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(SharpDX.WIC.PixelFormat.Format128bppRGBFloat, SharpDX.WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(SharpDX.WIC.PixelFormat.Format128bppRGBAFixedPoint, SharpDX.WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(SharpDX.WIC.PixelFormat.Format128bppRGBFixedPoint, SharpDX.WIC.PixelFormat.Format128bppRGBAFloat),
            new WICNearest(SharpDX.WIC.PixelFormat.Format32bppCMYK, SharpDX.WIC.PixelFormat.Format32bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format64bppCMYK, SharpDX.WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format40bppCMYKAlpha, SharpDX.WIC.PixelFormat.Format64bppRGBA),
            new WICNearest(SharpDX.WIC.PixelFormat.Format80bppCMYKAlpha, SharpDX.WIC.PixelFormat.Format64bppRGBA),
			new WICNearest(SharpDX.WIC.PixelFormat.Format96bppRGBFixedPoint, SharpDX.WIC.PixelFormat.Format128bppRGBAFloat)
		};
        #endregion

		#region Properties.
	    /// <summary>
	    /// Property to return the factory object.
	    /// </summary>
	    public ImagingFactory Factory
	    {
		    get;
		    private set;
	    }
	    #endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the equivalent Gorgon buffer format from a WIC GUID.
		/// </summary>
		/// <param name="wicGUID">WIC GUID to look up.</param>
		/// <returns>The buffer format if found, or BufferFormat.Unknown if not found.</returns>
		private BufferFormat GetGorgonFormat(Guid wicGUID)
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
		/// Function to retrieve a WIC format GUID from a System.Drawing PixelFormat.
		/// </summary>
		/// <param name="format">Pixel format to translate.</param>
		/// <returns>The GUID to convert into, NULL if the pixel format is not supported.</returns>
		private Guid GetGUID(PixelFormat format)
		{
            for (int i = 0; i < _wicPixelFormats.Length; i++)
            {
                if (_wicPixelFormats[i].PixelFormat == format)
                {
                    return _wicPixelFormats[i].WICGuid;
                }
            }

            // We don't care about indexed formats because they'll be upconverted to 32 bpp.
            switch (format)
            {
                case PixelFormat.Format1bppIndexed:			
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format32bppRgb:
                    return SharpDX.WIC.PixelFormat.Format32bppBGRA;
                default:
                    return Guid.Empty;
            }
		}

        /// <summary>
        /// Function to retrieve a WIC equivalent format GUID based on the Gorgon buffer format.
        /// </summary>
        /// <param name="format">Format to look up.</param>
        /// <returns>The GUID for the format, or NULL (<i>Nothing</i> in VB.Net) if not found.</returns>
        public Guid GetGUID(BufferFormat format)
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
                    return SharpDX.WIC.PixelFormat.Format32bppRGBA;
                case BufferFormat.D32_Float:
                    return SharpDX.WIC.PixelFormat.Format32bppGrayFloat;
                case BufferFormat.D16_UIntNormal:
                    return SharpDX.WIC.PixelFormat.Format16bppGray;
                case BufferFormat.B8G8R8A8_UIntNormal_sRGB:
                    return SharpDX.WIC.PixelFormat.Format32bppBGRA;
                case BufferFormat.B8G8R8X8_UIntNormal_sRGB:
                    return SharpDX.WIC.PixelFormat.Format32bppBGR;
            }

            return Guid.Empty;
        }

		/// <summary>
		/// Function to find the best buffer format for a given pixel format.
		/// </summary>
		/// <param name="sourcePixelFormat">Pixel format to translate.</param>
		/// <param name="flags">Flags to apply to the pixel format conversion.</param>
		/// <param name="updatedPixelFormat">The updated pixel format after flags are applied.</param>
		/// <returns>The buffer format, or Unknown if the format couldn't be converted.</returns>
		public BufferFormat FindBestFormat(Guid sourcePixelFormat, WICFlags flags, out Guid updatedPixelFormat)
		{
			BufferFormat result = GetGorgonFormat(sourcePixelFormat);

			updatedPixelFormat = Guid.Empty;

			if (result == BufferFormat.Unknown)
			{
				if (sourcePixelFormat == SharpDX.WIC.PixelFormat.Format96bppRGBFixedPoint)
				{
					updatedPixelFormat = SharpDX.WIC.PixelFormat.Format128bppRGBAFloat;
					result = BufferFormat.R32G32B32A32_Float;
				}
				else
				{
					// Find the best fit format if we couldn't find an exact match.
					for (int i = 0; i < _wicBestFitFormat.Length; i++)
					{
						if (_wicBestFitFormat[i].Source != sourcePixelFormat)
						{
							continue;
						}

						updatedPixelFormat = _wicBestFitFormat[i].Destination;
						result = GetGorgonFormat(updatedPixelFormat);

						// We couldn't find the format, bail out.
						if (result == BufferFormat.Unknown)
						{
							return result;
						}
						break;
					}
				}
			}

			switch (result)
			{
				case BufferFormat.B8G8R8A8_UIntNormal:
				case BufferFormat.B8G8R8X8_UIntNormal:
					if ((flags & WICFlags.ForceRGB) == WICFlags.ForceRGB)
					{
						result = BufferFormat.R8G8B8A8_UIntNormal;
						updatedPixelFormat = SharpDX.WIC.PixelFormat.Format32bppRGBA;
					}
					break;
				case BufferFormat.R10G10B10_XR_BIAS_A2_UIntNormal:
					if ((flags & WICFlags.NoX2Bias) == WICFlags.NoX2Bias)
					{
						result = BufferFormat.R10G10B10A2_UIntNormal;
						updatedPixelFormat = SharpDX.WIC.PixelFormat.Format32bppRGBA1010102;
					}
					break;
				case BufferFormat.B5G5R5A1_UIntNormal:
				case BufferFormat.B5G6R5_UIntNormal:
					if ((flags & WICFlags.No16BPP) == WICFlags.No16BPP)
					{
						result = BufferFormat.R8G8B8A8_UIntNormal;
						updatedPixelFormat = SharpDX.WIC.PixelFormat.Format32bppRGBA;
					}				
					break;
				case BufferFormat.R1_UIntNormal:
					if ((flags & WICFlags.AllowMono) != WICFlags.AllowMono)
					{
						result = BufferFormat.R1_UIntNormal;
						updatedPixelFormat = SharpDX.WIC.PixelFormat.Format8bppGray;
					}
					break;
			}

			return result;
		}

        /// <summary>
        /// Function to retrieve the bits per pixel in the specified WIC image format.
        /// </summary>
        /// <param name="wicPixelFormat">Image format to look up.</param>
        /// <returns>The bits per pixel of the format, or -1 if the format/bpp could not be determined.</returns>
        public int GetBitsPerPixel(Guid wicPixelFormat)
        {
            using (var component = new ComponentInfo(Factory, wicPixelFormat))
            {
                if (component.ComponentType != ComponentType.PixelFormat)
                {
                    return -1;
                }

                using (var formatInfo = component.QueryInterfaceOrNull<PixelFormatInfo>())
                {
                    if (formatInfo == null)
                    {
                        return -1;
                    }
					
                    return formatInfo.BitsPerPixel;
                }
            }
        }

        /// <summary>
        /// Function to create a list of WIC bitmaps from Gorgon image data.
        /// </summary>
        /// <param name="data">Data to convert to the list of WIC bitmaps.</param>
        /// <returns>The list of WIC bitmaps.</returns>
        public Bitmap[] CreateWICBitmapsFromImageData(GorgonImageData data)
        {
            int bitmapIndex = 0;
            Guid bitmapFormat = GetGUID(data.Settings.Format);
                       
            if (bitmapFormat == Guid.Empty)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported,
                    string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, data.Settings.Format));
            }

            // Make room for all the buffers.
            var bitmaps = new Bitmap[data.Buffers.Count];

            // Copy to the bitmap.
            foreach (var buffer in data.Buffers)
            {
                var pointer = new DX.DataRectangle(buffer.Data.BaseIntPtr, buffer.PitchInformation.RowPitch);
                bitmaps[bitmapIndex] = new Bitmap(Factory, buffer.Width, buffer.Height, bitmapFormat, pointer, pointer.Pitch * buffer.Height);
                bitmapIndex++;
            }

            return bitmaps;
        }

        /// <summary>
        /// Function to create a WIC bitmap from a Gorgon image buffer.
        /// </summary>
        /// <param name="buffer">Image buffer containing the image data to convert.</param>
        /// <returns>The WIC bitmap.</returns>
        public Bitmap CreateWICBitmapFromImageBuffer(GorgonImageBuffer buffer)
        {
            var pointer = new DX.DataRectangle(buffer.Data.BaseIntPtr, buffer.PitchInformation.RowPitch);

            Guid bitmapFormat = GetGUID(buffer.Format);

            if (bitmapFormat == Guid.Empty)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported,
                    string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, buffer.Format));
            }

            return new Bitmap(Factory,
                                  buffer.Width,
                                  buffer.Height,
                                  bitmapFormat,
                                  pointer,
                                  pointer.Pitch * buffer.Height);
        }

        /// <summary>
        /// Function to convert a WIC bitmap to a System.Drawing.Image
        /// </summary>
        /// <param name="bitmap">Bitmap to convert.</param>
        /// <param name="format">Pixel format to use.</param>
        /// <returns>The converted bitmap.</returns>
        public Image CreateGDIImageFromWICBitmap(Bitmap bitmap, PixelFormat format)
        {
            System.Drawing.Bitmap result = null;
            BitmapData lockData = null;
            Guid conversionFormat = GetGUID(format);

            if (conversionFormat == Guid.Empty)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported,
                    string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, format));
            }

            try
            {
                // Create the new bitmap.
                result = new System.Drawing.Bitmap(bitmap.Size.Width, bitmap.Size.Height, format);

                lockData = result.LockBits(new Rectangle(0, 0, bitmap.Size.Width, bitmap.Size.Height), ImageLockMode.WriteOnly, format);

                // We need to convert, so copy using the format converter.
                if (bitmap.PixelFormat != conversionFormat)
                {
                    using (var converter = new FormatConverter(Factory))
                    {
                        converter.Initialize(bitmap, conversionFormat, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);
                        converter.CopyPixels(lockData.Stride, lockData.Scan0, lockData.Stride * lockData.Height);
                    }
                }
                else
                {
                    // Otherwise, copy it all in one shot.
                    bitmap.CopyPixels(lockData.Stride, lockData.Scan0, lockData.Stride * lockData.Height);
                }

                return result;
            }
            finally
            {
                if (lockData != null)
                {
                    result.UnlockBits(lockData);
                }
            }
        }

        /// <summary>
        /// Function to create a WIC bitmap from a System.Drawing.Image object.
        /// </summary>
        /// <param name="image">Image to convert.</param>
        public Bitmap CreateWICImageFromImage(Image image)
        {
            BitmapData bmpData = null;
            var imageBitmap = image as System.Drawing.Bitmap;
            bool bitmapClone = false;

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

			// If the image being passed is not a bitmap, then make it into one.
			// Or, if the image is a 32bpp RGB (without alpha), or if the image is indexed.
            // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            if ((imageBitmap == null) || (image.PixelFormat == PixelFormat.Format32bppRgb) 
				|| ((image.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed))
            {
                imageBitmap = new System.Drawing.Bitmap(image);
                bitmapClone = true;
            }

            try
            {
				// Try to get a compatible WIC format.
				Guid guid = GetGUID(imageBitmap.PixelFormat);
                
				if (guid == Guid.Empty)
				{
					throw new GorgonException(GorgonResult.FormatNotSupported, string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, image.PixelFormat));
				}
						
				bmpData = imageBitmap.LockBits(new Rectangle(0, 0, imageBitmap.Width, imageBitmap.Height), ImageLockMode.ReadOnly, imageBitmap.PixelFormat);

				var pointer = new DX.DataRectangle(bmpData.Scan0, bmpData.Stride);
				var result = new Bitmap(Factory, imageBitmap.Width, imageBitmap.Height, guid, pointer, bmpData.Stride * bmpData.Height);
				result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

				return result;
            }
            finally
            {
                if (bmpData != null)
                {
                    imageBitmap.UnlockBits(bmpData);						
                }

                if (bitmapClone)
                {
                    imageBitmap.Dispose();
                }
            }
        }

		/// <summary>
		/// Function to convert the format of a bitmap into the format of the buffer.
		/// </summary>
		/// <param name="sourceFormat">Format to convert from.</param>
		/// <param name="destFormat">Format to convert into.</param>
		/// <param name="dithering">Dithering to apply.</param>
		/// <param name="filter">Filtering to apply to scaled bitmaps.</param>
		/// <param name="bitmap">Bitmap to convert.</param>
		/// <param name="bitmapPalette">Palette for the bitmap.</param>
		/// <param name="alphaValue">Value of pixel to consider transparent.</param>
		/// <param name="buffer">Buffer holding the converted data.</param>
		/// <param name="scale"><b>true</b> to scale when converting, <b>false</b> to keep at original size.</param>
		/// <param name="clip"><b>true</b> to perform clipping, <b>false</b> to keep at original size.</param>
		private void ConvertFormat(Guid sourceFormat, Guid destFormat, BitmapDitherType dithering, 
                                    BitmapInterpolationMode filter, BitmapSource bitmap, 
                                    Palette bitmapPalette, int alphaValue, 
                                    GorgonImageBuffer buffer, bool scale, bool clip)
		{
			BitmapSource source = bitmap;
		    double alphaPercent = alphaValue / 255.0;
			var paletteType = BitmapPaletteType.Custom;

			// If we have a palette, then confirm that the dithering method is valid.
			if (bitmapPalette != null)
			{
				// Do not apply dithering if we're using
				// a custom palette and request ordered dithering.
				switch (dithering)
				{
					case BitmapDitherType.Ordered4x4:
					case BitmapDitherType.Ordered8x8:
					case BitmapDitherType.Ordered16x16:					
						if (bitmapPalette.TypeInfo == BitmapPaletteType.Custom)
						{
							dithering = BitmapDitherType.None;
						}
						break;
				}
				paletteType = bitmapPalette.TypeInfo;
			}

			try
			{
				// Create a scaler if need one.
				if ((scale) && (!clip))
				{
					var scaler = new BitmapScaler(Factory);
					scaler.Initialize(bitmap, buffer.Width, buffer.Height, filter);
					source = scaler;
				}

				// Create a clipper if we want to clip and the image needs resizing.
				if ((clip) && (scale) && ((buffer.Width < bitmap.Size.Width) || (buffer.Height < bitmap.Size.Height)))
				{
					var clipper = new BitmapClipper(Factory);
					clipper.Initialize(bitmap, new DX.Rectangle(0, 0, buffer.Width < bitmap.Size.Width ? buffer.Width : bitmap.Size.Width, 
																			 buffer.Height < bitmap.Size.Height ? buffer.Height : bitmap.Size.Height));
					source = clipper;
				}

				using (var converter = new FormatConverter(Factory))
				{
					if (!converter.CanConvert(sourceFormat, destFormat))
					{
					    throw new GorgonException(GorgonResult.FormatNotSupported,
					        string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, buffer.Format));
					}

					converter.Initialize(source, destFormat, dithering, bitmapPalette, alphaPercent, paletteType);

					if (!scale)
					{
						int rowPitch = (GetBitsPerPixel(destFormat) * bitmap.Size.Width + 7) / 8;
						int slicePitch = rowPitch * bitmap.Size.Height;

						if ((rowPitch != buffer.PitchInformation.RowPitch) || (slicePitch != buffer.PitchInformation.SlicePitch))
						{
                            throw new GorgonException(GorgonResult.CannotWrite, Resources.GORGFX_IMAGE_PITCH_TOO_SMALL);
						}
					}
					converter.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data.BaseIntPtr, buffer.PitchInformation.SlicePitch);
				}
			}
			finally
			{
				if (source != bitmap)
				{
					source.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to clip or scale the bitmap up or down to the size of the buffer.
		/// </summary>
		/// <param name="bitmap">Bitmap to scale.</param>
		/// <param name="filter">Filtering to apply to the scaled bitmap.</param>
		/// <param name="buffer">Buffer to receive the scaled bitmap.</param>
		/// <param name="clip"><b>true</b> to clip, <b>false</b> to scale.</param>
		/// <returns><b>true</b> if clipping/scaling was performed, <b>false</b> if not.</returns>
		private bool ResizeBitmap(BitmapSource bitmap, BitmapInterpolationMode filter, GorgonImageBuffer buffer, bool clip)
		{
			if (!clip)
			{
				using (var scaler = new BitmapScaler(Factory))
				{
					scaler.Initialize(bitmap, buffer.Width, buffer.Height, filter);
					scaler.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data.BaseIntPtr, buffer.PitchInformation.SlicePitch);
				}

				return true;
			}

		    if ((buffer.Width >= bitmap.Size.Width) && (buffer.Height >= bitmap.Size.Height))
		    {
		        return false;
		    }

		    ClipBitmap(bitmap, buffer);
		    return true;
		}

		/// <summary>
		/// Function to clip a bitmap.
		/// </summary>
		/// <param name="bitmap">Bitmap to clip.</param>
		/// <param name="buffer">Buffer containing clipped data.</param>
		private void ClipBitmap(BitmapSource bitmap, GorgonImageBuffer buffer)
		{
			using (var clipper = new BitmapClipper(Factory))
			{
				clipper.Initialize(bitmap, new DX.Rectangle(0, 0, buffer.Width < bitmap.Size.Width ? buffer.Width : bitmap.Size.Width, 
																		 buffer.Height < bitmap.Size.Height ? buffer.Height : bitmap.Size.Height));
				clipper.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data.BaseIntPtr, buffer.PitchInformation.SlicePitch);
			}
		}

        /// <summary>
        /// Function to create a WIC bitmap from a System.Drawing.Image object.
        /// </summary>
        /// <param name="bitmap">WIC bitmap to copy to our image data.</param>
		/// <param name="filter">Filter used to scale the image.</param>
		/// <param name="ditherFlags">Flags used to dither the image.</param>
		/// <param name="buffer">Buffer for holding the image data.</param>
		/// <param name="clip"><b>true</b> to clip the data, <b>false</b> to scale it.</param>
        public void AddWICBitmapToImageData(Bitmap bitmap, ImageFilter filter, ImageDithering ditherFlags, GorgonImageBuffer buffer, bool clip)
        {
            Guid conversionFormat = GetGUID(buffer.Format);
			bool needsResize = (buffer.Width != bitmap.Size.Width) || (buffer.Height != bitmap.Size.Height);			

            if (conversionFormat == Guid.Empty)
            {
                throw new GorgonException(GorgonResult.FormatNotSupported,
                    string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, buffer.Format));
            }

            // Turn off filtering if we're not resizing.
            if (!needsResize)
            {
				filter = ImageFilter.Point;
            }
            
            // If the pixel format of the bitmap is not the same as our
            // conversion format, then we need to convert the image.
            if (bitmap.PixelFormat != conversionFormat)
            {
				ConvertFormat(bitmap.PixelFormat, conversionFormat, (BitmapDitherType)ditherFlags, (BitmapInterpolationMode)filter, bitmap, null, 0, buffer, needsResize, clip);
			}
            else
            {
                // Just dump without converting because our formats are equal.
				if ((!needsResize) || (!ResizeBitmap(bitmap, (BitmapInterpolationMode)filter, buffer, clip)))
				{
					bitmap.CopyPixels(buffer.PitchInformation.RowPitch, buffer.Data.BaseIntPtr, buffer.PitchInformation.SlicePitch);
				}
            }            
        }

        /// <summary>
        /// Function to perform conversion/transformation of a bitmap.
        /// </summary>
        /// <param name="sourceData">WIC bitmap to transform.</param>
        /// <param name="destData">Destination data buffer.</param>
        /// <param name="rowPitch">Number of bytes per row in the image.</param>
        /// <param name="slicePitch">Number of bytes in total for the image.</param>
        /// <param name="destFormat">Destination format for transformation.</param>
        /// <param name="isSourcesRGB"><b>true</b> if the source format is sRGB.</param>
        /// <param name="isDestsRGB"><b>true</b> if the destination format is sRGB.</param>
        /// <param name="dither">Dithering to apply to images that get converted to a lower bit depth.</param>
        /// <param name="destRect">Rectangle containing the area to scale or clip</param>
        /// <param name="clip"><b>true</b> to perform clipping instead of scaling.</param>
        /// <param name="scaleFilter">Filter to apply to scaled data.</param>
        public void TransformImageData(BitmapSource sourceData, IntPtr destData, int rowPitch, int slicePitch, Guid destFormat, bool isSourcesRGB, bool isDestsRGB, ImageDithering dither, Rectangle destRect, bool clip, ImageFilter scaleFilter)
        {
            BitmapSource source = sourceData;
            FormatConverter converter = null;
            BitmapClipper clipper = null;
            BitmapScaler scaler = null;
	        ColorContext sourceContext = null;
	        ColorContext destContext = null;
	        ColorTransform sRGBTransform = null;

            try
            {
                if (destFormat != Guid.Empty)
                {
	                if (sourceData.PixelFormat != destFormat)
	                {
		                converter = new FormatConverter(Factory);

		                if (!converter.CanConvert(sourceData.PixelFormat, destFormat))
		                {
			                throw new GorgonException(GorgonResult.FormatNotSupported,
			                                          string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, destFormat));
		                }

		                converter.Initialize(source, destFormat, (BitmapDitherType)dither, null, 0, BitmapPaletteType.Custom);
		                source = converter;
	                }

	                if ((isDestsRGB)
						|| (isSourcesRGB))
	                {
		                sRGBTransform = new ColorTransform(Factory);
						sourceContext = new ColorContext(Factory);
		                destContext = new ColorContext(Factory);

			            sourceContext.InitializeFromExifColorSpace(isSourcesRGB ? 1 : 2);
		                destContext.InitializeFromExifColorSpace(isDestsRGB ? 1 : 2);

		                sRGBTransform.Initialize(source, sourceContext, destContext, destFormat);
		                source = sRGBTransform;
	                }
                }

                if (destRect.IsEmpty)
                {
                    source.CopyPixels(rowPitch, destData, slicePitch);
                    return;
                }

                Guid pixelFormat = source.PixelFormat;

                if (!clip)
                {
                    scaler = new BitmapScaler(Factory);
                    scaler.Initialize(source,
                                        destRect.Width,
                                        destRect.Height,
                                        (BitmapInterpolationMode)scaleFilter);
                    source = scaler;
                }
                else
                {
                    destRect.Width = destRect.Width.Min(source.Size.Width);
                    destRect.Height = destRect.Height.Min(source.Size.Height);

                    if ((destRect.Width < source.Size.Width)
                        || (destRect.Height < source.Size.Height))
                    {
                        clipper = new BitmapClipper(Factory);
                        clipper.Initialize(source,
                                            new DX.Rectangle(destRect.X, destRect.Y, destRect.Width, destRect.Height));
                        source = clipper;
                        destRect.X = 0;
                        destRect.Y = 0;
                    }
                }

                // We have a change of format (probably due to the filter when scaling)... so we need to convert.
                if (source.PixelFormat != pixelFormat)
                {
                    converter = new FormatConverter(Factory);

                    if (!converter.CanConvert(source.PixelFormat, pixelFormat))
                    {
                        throw new GorgonException(GorgonResult.FormatNotSupported,
                                                    string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, pixelFormat));
                    }

                    converter.Initialize(source,
                                            pixelFormat,
                                            BitmapDitherType.None,
                                            null,
                                            0,
                                            BitmapPaletteType.Custom);
                    source = converter;
                }

                source.CopyPixels(rowPitch, destData, slicePitch);
            }
            finally
            {
                if (converter != null)
                {
                    converter.Dispose();
                }

	            if (sourceContext != null)
	            {
		            sourceContext.Dispose();
	            }

	            if (destContext != null)
	            {
		            destContext.Dispose();
	            }

	            if (sRGBTransform != null)
	            {
		            sRGBTransform.Dispose();
	            }

                if (scaler != null)
                {
                    scaler.Dispose();
                }

                if (clipper != null)
                {
                    clipper.Dispose();
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonWICImage" /> class.
        /// </summary>
        public GorgonWICImage()
        {
            Factory = new ImagingFactory();
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
		        if (Factory != null)
		        {
			        Factory.Dispose();
		        }
	        }

	        Factory = null;
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