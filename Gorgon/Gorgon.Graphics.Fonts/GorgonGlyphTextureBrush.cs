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
// Created: Saturday, October 12, 2013 10:28:27 PM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Math;
using Gorgon.Native;

namespace Gorgon.Graphics.Fonts
{
	/// <summary>
	/// Defines how to draw the <see cref="GorgonGlyphTextureBrush"/>, or <see cref="GorgonGlyphPathGradientBrush"/> if the paint area is larger than the texture region.
	/// </summary>
	public enum GlyphBrushWrapMode
	{
		/// <summary>
		/// Tiles the texture if the painted area is larger than the texture size.
		/// </summary>
		Tile = WrapMode.Tile,
		/// <summary>
		/// Reverses the texture horizontally and tiles the texture.
		/// </summary>
		TileFlipX = WrapMode.TileFlipX,
		/// <summary>
		/// Reverses the texture vertically and tiles the texture.
		/// </summary>
		TileFlipY = WrapMode.TileFlipY,
		/// <summary>
		/// Reverses the texture horizontally and vertically and tiles the texture.
		/// </summary>
		TileFlipXandY = WrapMode.TileFlipXY,
		/// <summary>
		/// Clamps the texture to the size requested.
		/// </summary>
		Clamp = WrapMode.Clamp
	}

	/// <summary>
	/// A brush used to draw glyphs using a texture.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will paint glyphs using the <see cref="IGorgonImage"/> provided to the constructor. 
	/// </para>
	/// <para>
	/// The texture used by this brush is a <see cref="IGorgonImage"/> and not a <see cref="GorgonTexture"/>, and must be a 2D image, and have a format of <c>R8G8B8A8_UNorm_SRgb</c>,
	/// <c>DXGI.Format.R8G8B8A8_UNorm</c>, <c>DXGI.Format.B8G8R8A8_UNorm</c>, or <c>DXGI.Format.B8G8R8A8_UNorm_SRgb</c>.
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonGlyphSolidBrush"/>
	/// <seealso cref="GorgonGlyphHatchBrush"/>
	/// <seealso cref="GorgonGlyphLinearGradientBrush"/>
	/// <seealso cref="GorgonGlyphPathGradientBrush"/>
	public class GorgonGlyphTextureBrush
		: GorgonGlyphBrush
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of brush.
		/// </summary>
		public override GlyphBrushType BrushType => GlyphBrushType.Texture;

		/// <summary>
		/// Property to set or return the wrapping mode for the gradient fill.
		/// </summary>
		public GlyphBrushWrapMode WrapMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the region to use when applying the <see cref="Image"/> as a texture.
		/// </summary>
		/// <remarks>
		/// This value is in relative texture coordinates.
		/// </remarks>
		public DX.RectangleF TextureRegion
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the <see cref="IGorgonImage"/> to apply to the brush.
		/// </summary>
		public IGorgonImage Image
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert the associated image texture to a GDI+ bitmap type.
		/// </summary>
		/// <returns>A new GDI+ bitmap containing the image.</returns>
		private Bitmap ConvertImageToGdiBitmap()
		{
			IGorgonImage image = null;
			Bitmap result = null;
			BitmapData lockData = null;

			try
			{
				// We have to convert to BGRA in order to use the image data with GDI+.
				if (Image.Info.Format != DXGI.Format.B8G8R8A8_UNorm)
				{
					image = Image.Clone();
					image.ConvertToFormat(DXGI.Format.B8G8R8A8_UNorm);
				}
				else
				{
					image = Image;
				}

				result = new Bitmap(image.Info.Width, image.Info.Height, PixelFormat.Format32bppArgb);

				lockData = result.LockBits(new Rectangle(0, 0, image.Info.Width, image.Info.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

				// Copy the image data into the GDI+ bitmap.
				int srcPitch = image.Buffers[0].PitchInformation.RowPitch;
				int destPitch = lockData.Stride;
				int srcOffset = 0;
				int destOffset = 0;
				IGorgonPointer srcPtr = image.Buffers[0].Data;
				IGorgonPointer destPtr = new GorgonPointerAlias(lockData.Scan0, lockData.Stride * lockData.Height);

				// Copy a single scan line at a time if the pitches mismatch.
				if (srcPitch != destPitch)
				{
					for (int y = 0; y < image.Info.Height; ++y)
					{
						srcPtr.CopyTo(destPtr, srcOffset, srcPitch.Min(destPitch), destOffset);
						srcOffset += srcPitch;
						destOffset += destPitch;
					}
				}
				else
				{
					// Otherwise, just copy everything in one shot.
					destPtr.CopyTo(srcPtr, (int)srcPtr.Size);
				}

				return result;
			}
			finally
			{
				if (lockData != null)
				{
					result.UnlockBits(lockData);
				}

				// If we made a copy, then destroy the copy.
				if (image != Image)
				{
					image?.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to convert this brush to the equivalent GDI+ brush type.
		/// </summary>
		/// <returns>
		/// The GDI+ brush type for this object.
		/// </returns>
		internal override Brush ToGDIBrush()
		{
			if (Image == null)
			{
				return null;
			}

			Bitmap brushBitmap = null;

			try
			{
				// Clone the image data and convert it into a GDI+ compatible bitmap so we can use it as a brush.
				brushBitmap = ConvertImageToGdiBitmap();

				var textureRect = new RectangleF(0, 0, Image.Info.Width, Image.Info.Height);
				var imageRect = new RectangleF(TextureRegion.X * textureRect.Width,
				                               TextureRegion.Y * textureRect.Height,
				                               TextureRegion.Width * textureRect.Width,
				                               TextureRegion.Height * textureRect.Height);

				imageRect = RectangleF.Intersect(textureRect, imageRect);

				if (imageRect == RectangleF.Empty)
				{
					imageRect = textureRect;
				}

				return new TextureBrush(brushBitmap, imageRect)
				       {
					       WrapMode = (WrapMode)WrapMode
				       };
			}
			finally
			{
				brushBitmap?.Dispose();
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyphPathGradientBrush"/> class.
		/// </summary>
		/// <param name="textureImage">The image to use as a texture.</param>
		/// <remarks>
		/// <para>
		/// The image format for the brush must be R8G8B8A8_UNorm, R8G8B8A8_UNorm_SRgb, B8G8R8A8_UNorm, or B8G8R8A8_UNorm_SRgb and must be a 2D image.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="textureImage"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="textureImage"/> parameter uses an unsupported format or is not a 2D image.</exception>
		public GorgonGlyphTextureBrush(IGorgonImage textureImage)
		{
			if (textureImage == null)
			{
				throw new ArgumentNullException(nameof(textureImage));	
			}

			if ((textureImage.Info.Format != DXGI.Format.R8G8B8A8_UNorm_SRgb)
				&& (textureImage.Info.Format != DXGI.Format.R8G8B8A8_UNorm)
				&& (textureImage.Info.Format != DXGI.Format.B8G8R8A8_UNorm)
				&& (textureImage.Info.Format != DXGI.Format.B8G8R8A8_UNorm_SRgb))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, textureImage.Info.Format), nameof(textureImage));
			}

			if (textureImage.Info.ImageType != ImageType.Image2D)
			{
				throw new ArgumentException(Resources.GORGFX_ERR_FONT_GLYPH_IMAGE_NOT_2D, nameof(textureImage));
			}

			Image = textureImage;
			TextureRegion = new DX.RectangleF(0, 0, 1, 1);
		}
		#endregion
	}
}
