﻿#region MIT
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
// Created: June 29, 2016 10:49:02 PM
// 
#endregion

using System;
using DXGI = SharpDX.DXGI;
using Gorgon.Graphics.Imaging.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Imaging
{
	/// <summary>
	/// Flags to control how pixel conversion should be handled.
	/// </summary>
	[Flags]
	public enum ImageBitFlags
	{
		/// <summary>
		/// No modifications to conversion process.
		/// </summary>
		None = 0,
		/// <summary>
		/// Set a known opaque alpha value in the alpha channel.
		/// </summary>
		OpaqueAlpha = 1,
		/// <summary>
		/// Enables specific legacy format conversion cases.
		/// </summary>
		Legacy = 2
	}

	/// <summary>
	/// Utilities to facilitate in manipulating image data.
	/// </summary>
	static class ImageUtilities
	{
		/// <summary>
		/// Function to expand a 16BPP scan line in an image to a 32BPP RGBA line.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="srcFormat">Format to convert from.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="srcFormat" /> is not a 16 BPP format.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// Use this to expand a 16 BPP (B5G6R5 or B5G5R5A1 format) into a 32 BPP R8G8B8A8 (normalized unsigned integer) format.
		/// </para>
		/// </remarks>
		public static unsafe void Expand16BPPScanline(void* src, int srcPitch, DXGI.Format srcFormat, void* dest, int destPitch, ImageBitFlags bitFlags)
		{
			var srcPtr = (ushort*)src;
			var destPtr = (uint*)dest;

			if ((srcFormat != DXGI.Format.B5G5R5A1_UNorm) && (srcFormat != DXGI.Format.B5G6R5_UNorm) && (srcFormat != DXGI.Format.B4G4R4A4_UNorm))
			{
				throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_IS_NOT_16BPP, srcFormat), nameof(srcFormat));
			}

			if (src == null)
			{
				throw new ArgumentNullException(nameof(src));
			}

			if (dest == null)
			{
				throw new ArgumentNullException(nameof(dest));
			}

			for (int srcCount = 0, destCount = 0; ((srcCount < srcPitch) && (destCount < destPitch)); srcCount += 2, destCount += 4)
			{
				ushort srcPixel = *(srcPtr++);
				uint R = 0, G = 0, B = 0, A = 0;

				switch (srcFormat)
				{
					case DXGI.Format.B5G6R5_UNorm:
						R = (uint)((srcPixel & 0xF800) >> 11);
						G = (uint)((srcPixel & 0x07E0) >> 5);
						B = (uint)(srcPixel & 0x001F);
						R = ((R << 3) | (R >> 2));
						G = ((G << 2) | (G >> 4)) << 8;
						B = ((B << 3) | (B >> 2)) << 16;
						A = 0xFF000000;
						break;
					case DXGI.Format.B5G5R5A1_UNorm:
						R = (uint)((srcPixel & 0x7C00) >> 10);
						G = (uint)((srcPixel & 0x03E0) >> 5);
						B = (uint)(srcPixel & 0x001F);
						R = ((R << 3) | (R >> 2));
						G = ((G << 3) | (G >> 2)) << 8;
						B = ((B << 3) | (B >> 2)) << 16;
						A = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
								? 0xFF000000
								: (((srcPixel & 0x8000) != 0) ? 0xFF000000 : 0);
						break;
					case DXGI.Format.B4G4R4A4_UNorm:
						A = (uint)((srcPixel & 0xF000) >> 12);
						R = (uint)((srcPixel & 0xF00) >> 8);
						G = (uint)((srcPixel & 0xF0) >> 4);
						B = (uint)(srcPixel & 0xF);
						R = ((R << 4) | R);
						G = ((G << 4) | G) << 8;
						B = ((B << 4) | B) << 16;
						A = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
								? 0xFF000000
								: ((A << 4) | A) << 24;
						break;
				}

				*(destPtr++) = R | G | B | A;
			}
		}

		/// <summary>
		/// Function to copy (or update in-place) with bits swizzled to match another format.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="format">Format of the destination buffer.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// <remarks>
		/// <para>
		/// Use this method to copy a single scanline and swizzle the bits of an image and (optionally) set an opaque constant alpha value.
		/// </para>
		/// </remarks>
		public static unsafe void SwizzleScanline(void* src, int srcPitch, void* dest, int destPitch, DXGI.Format format, ImageBitFlags bitFlags)
		{
			int size = srcPitch.Min(destPitch);
			uint r, g, b, a, pixel;

			if (src == null)
			{
				throw new ArgumentNullException(nameof(src));
			}

			if (dest == null)
			{
				throw new ArgumentNullException(nameof(dest));
			}

			if (format == DXGI.Format.Unknown)
			{
				throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format),
											nameof(format));
			}

			var srcPtr = (uint*)src;
			var destPtr = (uint*)dest;

			switch (format)
			{
				case DXGI.Format.R10G10B10A2_Typeless:
				case DXGI.Format.R10G10B10A2_UInt:
				case DXGI.Format.R10G10B10A2_UNorm:
				case DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm:
					for (int i = 0; i < size; i += 4)
					{
						if (src != dest)
						{
							pixel = *(srcPtr++);
						}
						else
						{
							pixel = *(destPtr);
						}

						r = ((pixel & 0x3FF00000) >> 20);
						g = (pixel & 0x000FFC00);
						b = ((pixel & 0x000003FF) << 20);
						a = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xC0000000 : pixel & 0xC0000000;

						*destPtr = r | g | b | a;
						destPtr++;
					}
					return;
				case DXGI.Format.R8G8B8A8_Typeless:
				case DXGI.Format.R8G8B8A8_UNorm:
				case DXGI.Format.R8G8B8A8_UNorm_SRgb:
				case DXGI.Format.B8G8R8A8_UNorm:
				case DXGI.Format.B8G8R8X8_UNorm:
				case DXGI.Format.B8G8R8A8_Typeless:
				case DXGI.Format.B8G8R8A8_UNorm_SRgb:
				case DXGI.Format.B8G8R8X8_Typeless:
				case DXGI.Format.B8G8R8X8_UNorm_SRgb:
					for (int i = 0; i < size; i += 4)
					{
						if (src != dest)
						{
							pixel = *(srcPtr++);
						}
						else
						{
							pixel = *(destPtr);
						}

						r = ((pixel & 0xFF0000) >> 16);
						g = (pixel & 0x00FF00);
						b = ((pixel & 0x0000FF) << 16);
						a = ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha) ? 0xFF000000 : pixel & 0xFF000000;

						*destPtr = r | g | b | a;
						destPtr++;
					}
					return;
			}

			if (src != dest)
			{
				DirectAccess.MemoryCopy(dest, src, size);
			}
		}
		
		/// <summary>
		/// Function to copy a scanline from the source to the destination.
		/// </summary>
		/// <param name="src">The source data to copy.</param>
		/// <param name="srcPitch">The pitch of the source data scanline.</param>
		/// <param name="dest">The destination buffer that will receive the copied data.</param>
		/// <param name="format">The format used to copy.</param>
		/// <param name="flipHorizontal"><b>true</b> to write horizontal pixel values from right to left, or <b>false</b> to write left to right.</param>
		/// <returns><b>true</b> if the line contains all 0 alpha values, <b>false</b> if not.</returns>
		public static unsafe bool CopyScanline(void* src, int srcPitch, void* dest, DXGI.Format format, bool flipHorizontal)
		{
			bool result = true;

			// Do a straight copy.
			switch (format)
			{
				case DXGI.Format.R32G32B32A32_Typeless:
				case DXGI.Format.R32G32B32A32_Float:
				case DXGI.Format.R32G32B32A32_UInt:
				case DXGI.Format.R32G32B32A32_SInt:
				{
					uint alphaMask = (format == DXGI.Format.R32G32B32_Float)
						                 ? 0x3F800000
						                 : ((format == DXGI.Format.R32G32B32_SInt) ? 0x7FFFFFFF : 0xFFFFFFFF);

					var srcPtr = (uint*)src;
					var destPtr = (uint*)dest;

					for (int i = 0; i < srcPitch; i += 16)
					{
						uint alpha = *(srcPtr + 3) & alphaMask;

						if (alpha != 0)
						{
							result = false;
						}

						// If not in place copy, then copy to destination.
						if (dest == src)
						{
							continue;
						}

						if (!flipHorizontal)
						{
							*(destPtr++) = *(srcPtr++);
							*(destPtr++) = *(srcPtr++);
							*(destPtr++) = *(srcPtr++);
							*(destPtr++) = *(srcPtr++);
						}
						else
						{
							*(destPtr--) = *(srcPtr++);
							*(destPtr--) = *(srcPtr++);
							*(destPtr--) = *(srcPtr++);
							*(destPtr--) = *(srcPtr++);
						}
					}
				}
					return result;
				case DXGI.Format.R16G16B16A16_Typeless:
				case DXGI.Format.R16G16B16A16_Float:
				case DXGI.Format.R16G16B16A16_UNorm:
				case DXGI.Format.R16G16B16A16_UInt:
				case DXGI.Format.R16G16B16A16_SNorm:
				case DXGI.Format.R16G16B16A16_SInt:
				{
					uint alphaMask = 0xFFFF0000;

					switch (format)
					{
						case DXGI.Format.R16G16B16A16_Float:
							alphaMask = 0x3C000000;
							break;
						case DXGI.Format.R16G16B16A16_SInt:
						case DXGI.Format.R16G16B16A16_SNorm:
							alphaMask = 0x7FFF0000;
							break;
					}

					var srcPtr = (uint*)src;
					var destPtr = (uint*)dest;

					for (int i = 0; i < srcPitch; i += 8)
					{
						uint alpha = *(srcPtr + 1) & alphaMask;

						if (alpha != 0)
						{
							result = false;
						}

						// If not in-place copy, then copy from the source.
						if (src == dest)
						{
							continue;
						}

						if (!flipHorizontal)
						{
							*(destPtr++) = *(srcPtr++);
							*(destPtr++) = *(srcPtr++);
						}
						else
						{
							*(destPtr--) = *(srcPtr++);
							*(destPtr--) = *(srcPtr++);
						}
					}
				}
					return result;
				case DXGI.Format.R10G10B10A2_Typeless:
				case DXGI.Format.R10G10B10A2_UNorm:
				case DXGI.Format.R10G10B10A2_UInt:
				case DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm:
				{
					var srcPtr = (uint*)src;
					var destPtr = (uint*)dest;

					for (int i = 0; i < srcPitch; i += 4)
					{
						uint pixel = *(srcPtr++);
						uint alpha = pixel & 0xC0000000;

						if (alpha != 0)
						{
							result = false;
						}

						if (dest == src)
						{
							continue;
						}

						if (!flipHorizontal)
						{
							*(destPtr++) = pixel;
						}
						else
						{
							*(destPtr--) = pixel;
						}
					}
				}
					return result;
				case DXGI.Format.R8G8B8A8_Typeless:
				case DXGI.Format.R8G8B8A8_UNorm:
				case DXGI.Format.R8G8B8A8_UNorm_SRgb:
				case DXGI.Format.R8G8B8A8_UInt:
				case DXGI.Format.R8G8B8A8_SNorm:
				case DXGI.Format.R8G8B8A8_SInt:
				case DXGI.Format.B8G8R8A8_UNorm:
				case DXGI.Format.B8G8R8A8_Typeless:
				case DXGI.Format.B8G8R8A8_UNorm_SRgb:
				{
					uint alphaMask = ((format == DXGI.Format.R8G8B8A8_SInt) || (format == DXGI.Format.R8G8B8A8_SNorm)) ? 0x7F000000 : 0xFF000000;

					var srcPtr = (uint*)src;
					var destPtr = (uint*)dest;

					for (int i = 0; i < srcPitch; i += 4)
					{
						uint pixel = *(srcPtr++);
						uint alpha = pixel & alphaMask;

						if (alpha != 0)
						{
							result = false;
						}

						if (src == dest)
						{
							continue;
						}

						if (!flipHorizontal)
						{
							*(destPtr++) = pixel;
						}
						else
						{
							*(destPtr--) = pixel;
						}
					}
				}
					return result;
				case DXGI.Format.B5G5R5A1_UNorm:
				case DXGI.Format.B4G4R4A4_UNorm:
				{
					ushort alphaMask = (ushort)(format == DXGI.Format.B5G5R5A1_UNorm ? 0x8000 : 0xF000);
					var srcPtr = (ushort*)src;
					var destPtr = (ushort*)dest;

					for (int i = 0; i < srcPitch; i += 2)
					{
						ushort pixel = *(srcPtr++);
						int alpha = pixel & alphaMask;

						if (alpha != 0)
						{
							result = false;
						}

						if (src == dest)
						{
							continue;
						}

						// If not in-place copy, then copy from the source.
						if (!flipHorizontal)
						{
							*(destPtr++) = pixel;
						}
						else
						{
							*(destPtr--) = pixel;
						}
					}
				}
					return result;
				case DXGI.Format.R8_UNorm:
				case DXGI.Format.B5G6R5_UNorm:
					if (dest == src)
					{
						return false;
					}

					if (flipHorizontal)
					{
						byte* srcPtr = (byte*)src;
						byte* destPtr = (byte*)dest;

						for (int x = 0; x < srcPitch; ++x)
						{
							*(destPtr--) = *(srcPtr++);
						}
						return false;
					}

					DirectAccess.MemoryCopy(dest, src, srcPitch);

					return false;
				case DXGI.Format.A8_UNorm:
				{
					var srcPtr = (byte*)src;
					var destPtr = (byte*)dest;

					for (int x = 0; x < srcPitch; ++x)
					{
						byte alpha = *(srcPtr++);

						if (alpha != 0)
						{
							result = false;
						}

						if (dest == src)
						{
							continue;
						}

						if (!flipHorizontal)
						{
							*(destPtr++) = alpha;
						}
						else
						{
							*(destPtr--) = alpha;
						}
					}
				}
					return result;
			}

			return false;
		}

		/// <summary>
		/// Function to copy (or update in-place) a line with opaque alpha substituion (if required).
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		/// <param name="format">Format of the destination buffer.</param>
		/// <param name="bitFlags">Image bit conversion control flags.</param>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> parameter is Unknown.</exception>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="src"/> or the <paramref name="dest"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="srcPitch"/> or the <paramref name="destPitch"/> parameter is less than 0.</exception>
		/// <remarks>Use this method to copy a single scanline of an image and (optionally) set an opaque constant alpha value.</remarks>
		public static unsafe void CopyScanline(void* src, int srcPitch, void* dest, int destPitch, DXGI.Format format, ImageBitFlags bitFlags)
		{
			if (src == null)
			{
				throw new ArgumentNullException(nameof(src));
			}

			if (dest == null)
			{
				throw new ArgumentNullException(nameof(dest));
			}

			if (format == DXGI.Format.Unknown)
			{
				throw new ArgumentException(string.Format(Resources.GORIMG_ERR_FORMAT_NOT_SUPPORTED, format), nameof(format));
			}

			int size = (src == dest) ? destPitch : (srcPitch.Min(destPitch));

			if ((bitFlags & ImageBitFlags.OpaqueAlpha) == ImageBitFlags.OpaqueAlpha)
			{
				// Do a straight copy.
				switch (format)
				{
					case DXGI.Format.R32G32B32A32_Typeless:
					case DXGI.Format.R32G32B32A32_Float:
					case DXGI.Format.R32G32B32A32_UInt:
					case DXGI.Format.R32G32B32A32_SInt:
						{
							uint alpha = (format == DXGI.Format.R32G32B32_Float) ? 0x3F800000
												: ((format == DXGI.Format.R32G32B32_SInt) ? 0x7FFFFFFF : 0xFFFFFFFF);

							var srcPtr = (uint*)src;
							var destPtr = (uint*)dest;

							for (int i = 0; i < size; i += 16)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*(destPtr++) = *(srcPtr++);
									*(destPtr++) = *(srcPtr++);
									*(destPtr++) = *(srcPtr++);
								}
								
								*(destPtr++) = alpha;
							}
						}
						return;
					case DXGI.Format.R16G16B16A16_Typeless:
					case DXGI.Format.R16G16B16A16_Float:
					case DXGI.Format.R16G16B16A16_UNorm:
					case DXGI.Format.R16G16B16A16_UInt:
					case DXGI.Format.R16G16B16A16_SNorm:
					case DXGI.Format.R16G16B16A16_SInt:
						{
							ushort alpha = 0xFFFF;

							switch (format)
							{
								case DXGI.Format.R16G16B16A16_Float:
									alpha = 0x3C00;
									break;
								case DXGI.Format.R16G16B16A16_SInt:
								case DXGI.Format.R16G16B16A16_SNorm:
									alpha = 0x7FFF;
									break;
							}

							var srcPtr = (ushort*)src;
							var destPtr = (ushort*)dest;

							for (int i = 0; i < size; i += 8)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*destPtr = *srcPtr;
									srcPtr += 4;
								}
								*destPtr += 3;
								*(destPtr++) = alpha;
							}
						}
						return;
					case DXGI.Format.R10G10B10A2_Typeless:
					case DXGI.Format.R10G10B10A2_UNorm:
					case DXGI.Format.R10G10B10A2_UInt:
					case DXGI.Format.R10G10B10_Xr_Bias_A2_UNorm:
						{
							var srcPtr = (uint*)src;
							var destPtr = (uint*)dest;

							for (int i = 0; i < size; i += 4)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*destPtr = (*srcPtr) & 0x3FFFFFFF;
									srcPtr++;
								}
								*destPtr |= 0xC0000000;
								destPtr++;
							}
						}
						return;
					case DXGI.Format.R8G8B8A8_Typeless:
					case DXGI.Format.R8G8B8A8_UNorm:
					case DXGI.Format.R8G8B8A8_UNorm_SRgb:
					case DXGI.Format.R8G8B8A8_UInt:
					case DXGI.Format.R8G8B8A8_SNorm:
					case DXGI.Format.R8G8B8A8_SInt:
					case DXGI.Format.B8G8R8A8_UNorm:
					case DXGI.Format.B8G8R8A8_Typeless:
					case DXGI.Format.B8G8R8A8_UNorm_SRgb:
						{
							uint alpha = ((format == DXGI.Format.R8G8B8A8_SInt) || (format == DXGI.Format.R8G8B8A8_SNorm)) ? 0x7F000000 : 0xFF000000;

							var srcPtr = (uint*)src;
							var destPtr = (uint*)dest;

							for (int i = 0; i < size; i += 4)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*destPtr = (*srcPtr) & 0xFFFFFF;
									srcPtr++;
								}
								*destPtr |= alpha;
								destPtr++;
							}
						}
						return;
					case DXGI.Format.B4G4R4A4_UNorm:
						{
							var srcPtr = (ushort*)src;
							var destPtr = (ushort*)dest;

							for (int i = 0; i < size; i += 2)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*(destPtr++) = (ushort)((*srcPtr++) | 0xF000);
								}
								else
								{
									*(destPtr++) |= 0xF000;
								}
							}
						}
						return;
					case DXGI.Format.B5G5R5A1_UNorm:
						{
							var srcPtr = (ushort*)src;
							var destPtr = (ushort*)dest;

							for (int i = 0; i < size; i += 2)
							{
								// If not in-place copy, then copy from the source.
								if (src != dest)
								{
									*(destPtr++) = (ushort)((*srcPtr++) | 0x8000);
								}
								else
								{
									*(destPtr++) |= 0x8000;
								}
							}
						}
						return;
					case DXGI.Format.A8_UNorm:
						DirectAccess.FillMemory(dest, 0xFF, size);
						return;
				}
			}

			// Copy if not doing an in-place update.
			if (dest != src)
			{
				DirectAccess.MemoryCopy(dest, src, size);
			}
		}

		/// <summary>
		/// Function to expand a 24 bit per pixel scanline into a 32 bit per pixel scanline.
		/// </summary>
		/// <param name="src">The source data to expand.</param>
		/// <param name="srcPitch">The number of bytes for a scanline in the source data.</param>
		/// <param name="dest">The pointer to the destination buffer to fill.</param>
		/// <param name="reverse"><b>true</b> to fill the destination from the right side, <b>false</b> to fill from the left.</param>
		public static unsafe void Expand24BPPScanLine(void* src, int srcPitch, void* dest, bool reverse)
		{
			var srcPtr = (byte*)src;
			var destPtr = (uint*)dest;

			for (int x = 0; x < srcPitch; x += 3)
			{
				uint pixel = (uint)((*(srcPtr++)) | (*(srcPtr++) << 8) | (*(srcPtr++) << 16) | 0xFF000000);

				if (reverse)
				{
					*(destPtr--) = pixel;
				}
				else
				{
					*(destPtr++) = pixel;
				}
			}
		}

		/// <summary>
		/// Function to compress a 32 bit scanline to a 24 bit bit scanline.
		/// </summary>
		/// <param name="src">The pointer to the source data.</param>
		/// <param name="srcPitch">The pitch of the source data.</param>
		/// <param name="dest">The pointer to the destination data.</param>
		/// <param name="destPitch">The pitch of the destination data.</param>
		public static unsafe void Compress24BPPScanLine(void* src, int srcPitch, void* dest, int destPitch)
		{
			var srcPtr = (uint*)src;
			var destPtr = (byte*)dest;
			byte* endPtr = destPtr + destPitch;

			for (int srcCount = 0; srcCount < srcPitch; srcCount += 4)
			{
				uint pixel = *(srcPtr++);

				// Ensure we don't have a buffer overrun.
				if (destPtr + 2 > endPtr)
				{
					return;
				}

				*(destPtr++) = (byte)((pixel & 0xFF));				//R
				*(destPtr++) = (byte)((pixel & 0xFF00) >> 8);		//G
				*(destPtr++) = (byte)((pixel & 0xFF0000) >> 16);    //B
			}
		}
	}
}