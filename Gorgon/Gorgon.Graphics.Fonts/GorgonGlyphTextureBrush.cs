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
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.IO;
using DX = SharpDX;

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
    /// The texture used by this brush is a <see cref="IGorgonImage"/> and not a <see cref="GorgonTexture2D"/>, and must be a 2D image, and have a format of <c>R8G8B8A8_UNorm_SRgb</c>,
    /// <c>BufferFormat.R8G8B8A8_UNorm</c>, <c>BufferFormat.B8G8R8A8_UNorm</c>, or <c>BufferFormat.B8G8R8A8_UNorm_SRgb</c>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGlyphSolidBrush"/>
    /// <seealso cref="GorgonGlyphHatchBrush"/>
    /// <seealso cref="GorgonGlyphLinearGradientBrush"/>
    /// <seealso cref="GorgonGlyphPathGradientBrush"/>
    public class GorgonGlyphTextureBrush
        : GorgonGlyphBrush, IDisposable
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
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Function to write out the specifics of the font brush data to a file writer.</summary>
        /// <param name="writer">The writer used to write the brush data.</param>
        internal override void WriteBrushData(GorgonBinaryWriter writer)
        {
            writer.Write((int)WrapMode);
            writer.WriteValue(TextureRegion);

            // Mark this as the size.
            long sizePosition = writer.BaseStream.Position;
            writer.Write(0);

            // Encode the texture brush as a PNG stream.
            long size = writer.BaseStream.Length;
            var codec = new GorgonCodecPng();

            codec.Save(Image, writer.BaseStream);

            // Calculate the number of bytes written.
            size = writer.BaseStream.Length - size;

            // Move back and write the size of the image data.
            writer.BaseStream.Position = sizePosition;
            writer.Write((int)size);

            // Return to the end of the stream.
            writer.BaseStream.Position = writer.BaseStream.Length;
        }

        /// <summary>Function to read back the specifics of the font brush data from a file reader.</summary>
        /// <param name="reader">The reader used to read the brush data.</param>
        internal override void ReadBrushData(GorgonBinaryReader reader)
        {
            WrapMode = (GlyphBrushWrapMode)reader.ReadInt32();
            TextureRegion = reader.ReadValue<DX.RectangleF>();
            int imageSize = reader.ReadInt32();

            Image?.Dispose();
            var codec = new GorgonCodecPng();
            Image = codec.FromStream(reader.BaseStream, imageSize);
        }

        /// <summary>
        /// Function to convert this brush to the equivalent GDI+ brush type.
        /// </summary>
        /// <returns>
        /// The GDI+ brush type for this object.
        /// </returns>
        internal override Brush ToGDIBrush()
        {
            if (Image is null)
            {
                return null;
            }

            Bitmap brushBitmap = null;
            IGorgonImage image = Image;

            try
            {
                // Clone the image data and convert it into a GDI+ compatible bitmap so we can use it as a brush.
                if (image.FormatInfo.IsCompressed)
                {
                    // If the image data is compressed, convert it back into standard 32 bit RGBA format, GDI+ doesn't 
                    // work with block compression.
                    image = Image.Clone()
                                 .Decompress()
                                 .EndUpdate();
                }

                brushBitmap = image.Buffers[0].ToBitmap();

                var textureRect = new RectangleF(0, 0, image.Width, image.Height);
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
                if (image != Image)
                {
                    image.Dispose();
                }
                brushBitmap?.Dispose();
            }
        }

        /// <summary>Function to clone an object.</summary>
        /// <returns>The cloned object.</returns>
        public override GorgonGlyphBrush Clone() => new GorgonGlyphTextureBrush(Image) // The image is cloned in the constructor.
        {            
            WrapMode = WrapMode,
            TextureRegion = TextureRegion
        };

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() => Image?.Dispose();

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <span class="keyword">
        ///     <span class="languageSpecificText">
        ///       <span class="cs">true</span>
        ///       <span class="vb">True</span>
        ///       <span class="cpp">true</span>
        ///     </span>
        ///   </span>
        ///   <span class="nu">
        ///     <span class="keyword">true</span> (<span class="keyword">True</span> in Visual Basic)</span> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <span class="keyword"><span class="languageSpecificText"><span class="cs">false</span><span class="vb">False</span><span class="cpp">false</span></span></span><span class="nu"><span class="keyword">false</span> (<span class="keyword">False</span> in Visual Basic)</span>.
        /// </returns>
        public override bool Equals(GorgonGlyphBrush other)
        {
            var brush = other as GorgonGlyphTextureBrush;

            return ((brush == this) || ((brush != null)
                && (brush.WrapMode == WrapMode)
                && (brush.TextureRegion.Equals(TextureRegion))
                && ((brush.Image == Image) || ((brush.Image.ImageType == Image.ImageType)
                        && (brush.Image.SizeInBytes == Image.SizeInBytes)
                        && (brush.Image.Format == Image.Format)
                        && (brush.Image.Width == Image.Width)
                        && (brush.Image.Height == Image.Height)
                        && (brush.Image.MipCount == Image.MipCount)
                        && (brush.Image.ArrayCount == Image.ArrayCount)
                        && (brush.Image.Depth == Image.Depth)))));
        }
        #endregion

        #region Constructor
        /// <summary>Initializes a new instance of the <see cref="GorgonGlyphTextureBrush"/> class.</summary>
        internal GorgonGlyphTextureBrush() => TextureRegion = new DX.RectangleF(0, 0, 1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGlyphPathGradientBrush"/> class.
        /// </summary>
        /// <param name="textureImage">The image to use as a texture.</param>
        /// <remarks>
        /// <para>
        /// The image format for the brush must be <see cref="BufferFormat.R8G8B8A8_UNorm"/>, <see cref="BufferFormat.R8G8B8A8_UNorm_SRgb"/>, <see cref="BufferFormat.B8G8R8A8_UNorm"/>, 
        /// <see cref="BufferFormat.B8G8R8A8_UNorm_SRgb"/>, or one of the compressed formats (e.g. BC3, BC7, etc..) and must be a 2D image.
        /// </para>
        /// <para>
        /// A copy of the <paramref name="textureImage"/> is stored in this object instead of a direct reference to the original image.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="textureImage"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="textureImage"/> parameter uses an unsupported format or is not a 2D image.</exception>
        public GorgonGlyphTextureBrush(IGorgonImage textureImage)
        {
            if (textureImage is null)
            {
                throw new ArgumentNullException(nameof(textureImage));
            }

            if ((textureImage.Format != BufferFormat.R8G8B8A8_UNorm_SRgb)
                && (textureImage.Format != BufferFormat.R8G8B8A8_UNorm)
                && (textureImage.Format != BufferFormat.B8G8R8A8_UNorm)
                && (textureImage.Format != BufferFormat.B8G8R8A8_UNorm_SRgb)
                && (!textureImage.FormatInfo.IsCompressed))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, textureImage.Format), nameof(textureImage));
            }

            if (textureImage.ImageType != ImageType.Image2D)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_FONT_GLYPH_IMAGE_NOT_2D, nameof(textureImage));
            }

            Image = textureImage.Clone();
            TextureRegion = new DX.RectangleF(0, 0, 1, 1);
        }
        #endregion
    }
}
