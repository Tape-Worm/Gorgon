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
using System.IO;
using System.Linq;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.IO;
using Gorgon.Memory;
using Gorgon.Native;
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
        : GorgonGlyphBrush
    {
        #region Variables.
        // The data for the image.
        private byte[] _imageData = Array.Empty<byte>();
        // The width and height of the texture.
        private int _width;
        private int _height;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of brush.
        /// </summary>
        public override GlyphBrushType BrushType => GlyphBrushType.Texture;

        /// <summary>
        /// Property to return the width of the texture, in pixels.
        /// </summary>
        public int TextureWidth => _width;

        /// <summary>
        /// Property to return the height of the texture, in pixels.
        /// </summary>
        public int TextureHeight => _height;

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
        /// Property to return the image data to apply to the brush.
        /// </summary>
        public Span<byte> Image => _imageData;
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

            using IGorgonImage image = ToGorgonImage();
            codec.Save(image, writer.BaseStream);

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
                        
            var codec = new GorgonCodecPng();
            using IGorgonImage image = codec.FromStream(reader.BaseStream, imageSize);

            if (image.Format != BufferFormat.R8G8B8A8_UNorm)
            {
                image.BeginUpdate()
                     .ConvertToFormat(BufferFormat.R8G8B8A8_UNorm)
                     .EndUpdate();
            }

            _imageData = new byte[image.Buffers[0].PitchInformation.SlicePitch];
            image.Buffers[0].Data.CopyTo(_imageData.AsSpan());

            _width = image.Width;
            _height = image.Height;
        }

        /// <summary>
        /// Function to convert this brush to the equivalent GDI+ brush type.
        /// </summary>
        /// <returns>
        /// The GDI+ brush type for this object.
        /// </returns>
        internal override Brush ToGDIBrush()
        {
            Bitmap brushBitmap = null;

            try
            {
                using IGorgonImage image = ToGorgonImage();
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
                brushBitmap?.Dispose();
            }
        }

        /// <summary>
        /// Function to convert this brush into a regular image.
        /// </summary>
        /// <returns>A new <see cref="IGorgonImage"/> containing the brush image data.</returns>
        public IGorgonImage ToGorgonImage() => new GorgonImage(new GorgonImageInfo(ImageType.Image2D, BufferFormat.R8G8B8A8_UNorm)
        {
            Width = _width,
            Height = _height
        }, _imageData.AsSpan());

        /// <summary>Function to clone an object.</summary>
        /// <returns>The cloned object.</returns>
        public override GorgonGlyphBrush Clone()
        {
            byte[] imageData = new byte[_imageData.Length];
            if (_imageData.Length > 0)
            {
                Array.Copy(_imageData, imageData, _imageData.Length);
            }
            return new GorgonGlyphTextureBrush(null) // The image is cloned in the constructor.
            {
                _imageData = imageData,
                _width = _width,
                _height = _height,
                WrapMode = WrapMode,
                TextureRegion = TextureRegion
            };
        }

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

            return ((brush == this) || ((brush is not null)
                && (brush.WrapMode == WrapMode)
                && (brush.TextureRegion.Equals(TextureRegion))
                && (brush.Image.SequenceEqual(Image))));
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
        /// <exception cref="ArgumentException">Thrown when the <paramref name="textureImage"/> parameter uses an unsupported format or is not a 2D image.</exception>
        public GorgonGlyphTextureBrush(IGorgonImage textureImage)
        {
            if (textureImage is null)
            {
                _imageData = Array.Empty<byte>();
                TextureRegion = new DX.RectangleF(0, 0, 1, 1);
                WrapMode = GlyphBrushWrapMode.Clamp;
                return;
            }

            if ((textureImage.Format != BufferFormat.R8G8B8A8_UNorm)
                && (!textureImage.FormatInfo.IsCompressed)
                && (!textureImage.CanConvertToFormat(BufferFormat.R8G8B8A8_UNorm)))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_FORMAT_NOT_SUPPORTED, textureImage.Format), nameof(textureImage));
            }

            if (textureImage.ImageType != ImageType.Image2D)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_FONT_GLYPH_IMAGE_NOT_2D, nameof(textureImage));
            }

            using IGorgonImage tempImage = textureImage.Clone();
            if (textureImage.FormatInfo.IsCompressed)
            {
                tempImage.Decompress()
                         .EndUpdate();
            }

            if (textureImage.Format != BufferFormat.R8G8B8A8_UNorm)
            {
                tempImage.BeginUpdate()
                         .ConvertToFormat(BufferFormat.R8G8B8A8_UNorm)
                         .EndUpdate();
            }

            _width = tempImage.Width;
            _height = tempImage.Height;

            _imageData = new byte[tempImage.Buffers[0].PitchInformation.SlicePitch];
            tempImage.Buffers[0].Data.CopyTo(_imageData.AsSpan());            
            TextureRegion = new DX.RectangleF(0, 0, 1, 1);
        }
        #endregion
    }
}
