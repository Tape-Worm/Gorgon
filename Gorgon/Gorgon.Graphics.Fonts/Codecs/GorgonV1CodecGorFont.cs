#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: February 23, 2017 11:39:19 PM
// 
#endregion

using System;
using System.IO;
using System.Threading.Tasks;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.IO;

namespace Gorgon.Graphics.Fonts.Codecs
{
    /// <summary>
    /// A font codec used to read version 1.0 of the font data using the standard Gorgon Font format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The version number of 1.0 does not represent the version of Gorgon, but the version of the format.
    /// </para>
    /// </remarks>
    public sealed class GorgonV1CodecGorFont
        : GorgonFontCodec
    {
        #region Constants.
        // BRSHDATA chunk.
        private const string BrushChunk = "BRSHDATA";
        // FONTINFO chunk.
        private const string FontInfoChunk = "FONTINFO";
        // Header for a Gorgon font file.
        private const string FileHeader = "GORFNT10";
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the default filename extension for font files.
        /// </summary>
        public override string DefaultFileExtension => ".gorFont";

        /// <summary>
        /// Property to return whether the codec supports decoding of font data.
        /// </summary>
        /// <remarks>
        /// If this value is <b>false</b>, then the codec is effectively write only.
        /// </remarks>
        public override bool CanDecode => true;

        /// <summary>
        /// Property to return whether the codec supports encoding of font data.
        /// </summary>
        /// <remarks>
        /// If this value is <b>false</b>, then the codec is effectively read only.
        /// </remarks>
        public override bool CanEncode => false;

        /// <summary>
        /// Property to return whether the codec supports fonts with outlines.
        /// </summary>
        public override bool SupportsFontOutlines => true;

        /// <summary>
        /// Property to return the friendly description of the codec.
        /// </summary>
        public override string CodecDescription => Resources.GORGFX_DESC_GORFONT_CODEC;

        /// <summary>
        /// Property to return the abbreviated name of the codec (e.g. GorFont).
        /// </summary>
        public override string Codec => "GorFont";
        #endregion

        #region Methods.
        /// <summary>
        /// Function to read the chunk containing the font information.
        /// </summary>
        /// <param name="fontFile">The chunk file reader containing the font data.</param>
        /// <param name="name">Used defined name for the font.</param>
        /// <returns>A new <seealso cref="GorgonFontInfo"/> containing information about the font.</returns>
        private static GorgonFontInfo GetFontInfo(GorgonChunkFileReader fontFile, string name)
        {
            GorgonBinaryReader reader = fontFile.OpenChunk(FontInfoChunk);
            var info = new GorgonFontInfo(reader.ReadString(), reader.ReadSingle(), reader.ReadValue<FontHeightMode>())
            {
                Name = name,
                FontStyle = reader.ReadValue<FontStyle>(),
                DefaultCharacter = reader.ReadChar(),
                Characters = reader.ReadString(),
                AntiAliasingMode = reader.ReadValue<FontAntiAliasMode>(),
                OutlineColor1 = new GorgonColor(reader.ReadInt32()),
                OutlineColor2 = new GorgonColor(reader.ReadInt32()),
                OutlineSize = reader.ReadInt32(),
                PackingSpacing = reader.ReadInt32(),
                TextureWidth = reader.ReadInt32(),
                TextureHeight = reader.ReadInt32(),
                UsePremultipliedTextures = reader.ReadValue<bool>(),
                UseKerningPairs = reader.ReadBoolean()
            };

            return info;
        }

        /// <summary>
        /// Function to write the font data to the stream.
        /// </summary>
        /// <param name="fontData">The font data to write.</param>
        /// <param name="stream">The stream to write into.</param>
        /// <exception cref="NotSupportedException">This codec does not support writing.</exception>
        protected override void OnWriteFontData(GorgonFont fontData, Stream stream) => throw new NotSupportedException();

        /// <summary>
        /// Function to read the meta data for font data within a stream.
        /// </summary>
        /// <param name="stream">The stream containing the metadata to read.</param>
        /// <returns>
        /// The font meta data as a <see cref="GorgonFontInfo"/> value.
        /// </returns>
        protected override GorgonFontInfo OnGetMetaData(Stream stream)
        {
            GorgonChunkFileReader fontFile = null;

            try
            {
                fontFile = new GorgonChunkFileReader(stream,
                                                     new[]
                                                     {
                                                         FileHeader.ChunkID()
                                                     });
                fontFile.Open();

                return GetFontInfo(fontFile, null);
            }
            finally
            {
                fontFile?.Close();
            }
        }

        /// <summary>Function to load the font data, with the specified size, from a stream.</summary>
        /// <param name="name">The name to assign to the font.</param>
        /// <param name="stream">The stream containing the font data.</param>
        /// <returns>A new , or, an existing font from the  cache.</returns>
        /// <seealso cref="GorgonFont" />
        /// <seealso cref="GorgonFontFactory" />
        protected override async Task<GorgonFont> OnLoadFromStreamAsync(string name, Stream stream)
        {
            var fontFile = new GorgonChunkFileReader(stream,
                                                     new[]
                                                     {
                                                         FileHeader.ChunkID()
                                                     });
            GorgonFontInfo fontInfo = null;

            try
            {
                fontFile.Open();

                fontInfo = GetFontInfo(fontFile, name);

                // If the font is already loaded, then just reuse it.
                if (Factory.HasFont(fontInfo))
                {
                    return Factory.GetFont(fontInfo);
                }

                GorgonBinaryReader reader = null;
                GorgonGlyphBrush fontBrush = null;

                if (fontFile.Chunks.Contains(BrushChunk))
                {
                    reader = fontFile.OpenChunk(BrushChunk);
                    var brushType = (GlyphBrushType)reader.ReadInt32();

                    fontBrush = brushType switch
                    {
                        GlyphBrushType.Hatched => new GorgonGlyphHatchBrush(),
                        GlyphBrushType.LinearGradient => new GorgonGlyphLinearGradientBrush(),
                        GlyphBrushType.PathGradient => new GorgonGlyphPathGradientBrush(),
                        GlyphBrushType.Texture => new GorgonGlyphTextureBrush(),
                        _ => new GorgonGlyphSolidBrush(),
                    };
                    fontBrush.ReadBrushData(reader);
                    fontFile.CloseChunk();
                }
                else
                {
                    fontBrush = new GorgonGlyphSolidBrush();
                }

#if NET5_0_OR_GREATER
                fontInfo = fontInfo with
                {
                    Brush = fontBrush
                };
#endif

                return await Factory.GetFontAsync(fontInfo);
            }
            finally
            {
                var brush = fontInfo?.Brush as IDisposable;
                brush?.Dispose();
                fontFile.Close();
            }
        }

        /// <summary>
        /// Function to load the font data, with the specified size, from a stream.
        /// </summary>
        /// <param name="name">The name to assign to the font.</param>
        /// <param name="stream">The stream containing the font data.</param>
        /// <returns>A new <seealso cref="GorgonFont"/>, or, an existing font from the <seealso cref="GorgonFontFactory"/> cache.</returns>
        protected override GorgonFont OnLoadFromStream(string name, Stream stream)
        {
            var fontFile = new GorgonChunkFileReader(stream,
                                                     new[]
                                                     {
                                                         FileHeader.ChunkID()
                                                     });
            GorgonFontInfo fontInfo = null;


            try
            {
                fontFile.Open();

                fontInfo = GetFontInfo(fontFile, name);

                // If the font is already loaded, then just reuse it.
                if (Factory.HasFont(fontInfo))
                {
                    return Factory.GetFont(fontInfo);
                }

                GorgonBinaryReader reader = null;
                GorgonGlyphBrush fontBrush = null;

                if (fontFile.Chunks.Contains(BrushChunk))
                {
                    reader = fontFile.OpenChunk(BrushChunk);
                    var brushType = (GlyphBrushType)reader.ReadInt32();

                    fontBrush = brushType switch
                    {
                        GlyphBrushType.Hatched => new GorgonGlyphHatchBrush(),
                        GlyphBrushType.LinearGradient => new GorgonGlyphLinearGradientBrush(),
                        GlyphBrushType.PathGradient => new GorgonGlyphPathGradientBrush(),
                        GlyphBrushType.Texture => new GorgonGlyphTextureBrush(),
                        _ => new GorgonGlyphSolidBrush(),
                    };
                    fontBrush.ReadBrushData(reader);
                    fontFile.CloseChunk();
                }
                else
                {
                    fontBrush = new GorgonGlyphSolidBrush();
                }

#if NET5_0_OR_GREATER
                fontInfo = fontInfo with
                {
                    Brush = fontBrush
                };
#endif

                return Factory.GetFont(fontInfo);
            }
            finally
            {
                var brush = fontInfo?.Brush as IDisposable;
                brush?.Dispose();
                fontFile.Close();
            }
        }

        /// <summary>
        /// Function to determine if this codec can read the font data within the stream or not.
        /// </summary>
        /// <param name="stream">The stream that is used to read the font data.</param>
        /// <returns><b>true</b> if the codec can read the file, <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> is write-only or if the stream cannot perform seek operations.</exception>
        /// <exception cref="EndOfStreamException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>
        /// <para>
        /// Implementors should ensure that the stream position is restored prior to exiting this method. Failure to do so may cause problems when reading the data from the stream.
        /// </para>
        /// </remarks>
        public override bool IsReadable(Stream stream)
        {
            ulong fileHeaderID = FileHeader.ChunkID();

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY);
            }

            if (!stream.CanSeek)
            {
                throw new IOException(Resources.GORGFX_ERR_STREAM_NO_SEEK);
            }

            if (stream.Length - stream.Position < FileHeader.Length)
            {
                return false;
            }

            long position = stream.Position;
            var reader = new GorgonBinaryReader(stream, true);

            try
            {
                ulong fileData = reader.ReadUInt64();
                return fileData == fileHeaderID;
            }
            finally
            {
                reader.Dispose();
                stream.Position = position;
            }
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonV1CodecGorFont"/> class.
        /// </summary>
        /// <param name="factory">The font factory that holds cached font information.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="factory"/> parameter is <b>null</b>.</exception>
        public GorgonV1CodecGorFont(GorgonFontFactory factory)
            : base(factory) => CodecCommonExtensions = new[]
                                    {
                                        ".gorFont"
                                    };
        #endregion
    }
}
