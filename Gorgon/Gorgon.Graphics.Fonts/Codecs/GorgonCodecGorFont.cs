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
using System.Collections.Generic;
using System.IO;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.IO;

namespace Gorgon.Graphics.Fonts.Codecs
{
	/// <summary>
	/// A font codec used to read/write font data using the standard Gorgon Font format.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This codec will create binary font data using the native font file format for Gorgon. 
	/// </para>
	/// </remarks>
	public sealed class GorgonCodecGorFont
		: GorgonFontCodec
	{
        #region Constants.
        // BRSHDATA chunk.
        private const string BrushChunk = "BRSHDATA";
		// FONTHIGH chunk.
		private const string FontHeightChunk = "FONTHIGH";
		// KERNPAIR chunk.
		private const string KernDataChunk = "KERNPAIR";
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
		public override bool CanEncode => true;

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
		/// <returns>A new <seealso cref="IGorgonFontInfo"/> containing information about the font.</returns>
		private static GorgonFontInfo GetFontInfo(GorgonChunkFileReader fontFile, string name)
		{
			GorgonBinaryReader reader = fontFile.OpenChunk(FontInfoChunk);
		    var info = new GorgonFontInfo(reader.ReadString(), reader.ReadSingle(), reader.ReadValue<FontHeightMode>(), name)
		               {
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
			fontFile.CloseChunk();

			return info;
		}

		/// <summary>
		/// Function to write out the kerning pair information for the font.
		/// </summary>
		/// <param name="fontData">The font data to write.</param>
		/// <param name="fontFile">The font file that is being persisted.</param>
		private static void WriteKerningValues(GorgonFont fontData, GorgonChunkFileWriter fontFile)
		{
			GorgonBinaryWriter writer = fontFile.OpenChunk(KernDataChunk);

			writer.Write(fontData.KerningPairs.Count);

			foreach (KeyValuePair<GorgonKerningPair, int> kerningInfo in fontData.KerningPairs)
			{
				writer.Write(kerningInfo.Key.LeftCharacter);
				writer.Write(kerningInfo.Key.RightCharacter);
				writer.Write(kerningInfo.Value);
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to read the kerning pairs for the font, if they exist.
		/// </summary>
		/// <param name="fontFile">Font file to read.</param>
		private static Dictionary<GorgonKerningPair, int> ReadKernPairs(GorgonChunkFileReader fontFile)
		{
			var result = new Dictionary<GorgonKerningPair, int>();
			GorgonBinaryReader reader = fontFile.OpenChunk(KernDataChunk);

			// Read optional kerning information.
			int kernCount = reader.ReadInt32();

			for (int i = 0; i < kernCount; ++i)
			{
				var kernPair = new GorgonKerningPair(reader.ReadChar(), reader.ReadChar());
				result[kernPair] = reader.ReadInt32();
			}

			fontFile.CloseChunk();

			return result;
		}

		/// <summary>
		/// Function to write the font data to the stream.
		/// </summary>
		/// <param name="fontData">The font data to write.</param>
		/// <param name="stream">The stream to write into.</param>
		/// <remarks>
		/// <para>
		/// Implementors must override this method to write out the font data in the expected format.
		/// </para>
		/// </remarks>
		protected override void OnWriteFontData(GorgonFont fontData, Stream stream)
		{
			var fontFile = new GorgonChunkFileWriter(stream, FileHeader.ChunkID());
			IGorgonFontInfo fontInfo = fontData.Info;

			try
			{
				fontFile.Open();

				GorgonBinaryWriter writer = fontFile.OpenChunk(FontInfoChunk);

				writer.Write(fontInfo.FontFamilyName);
				writer.Write(fontInfo.Size);
				writer.WriteValue(fontInfo.FontHeightMode);
				writer.WriteValue(fontInfo.FontStyle);
				writer.Write(fontInfo.DefaultCharacter);
				writer.Write(string.Join(string.Empty, fontInfo.Characters));
				writer.WriteValue(fontInfo.AntiAliasingMode);
				writer.Write(fontInfo.OutlineColor1.ToARGB());
				writer.Write(fontInfo.OutlineColor2.ToARGB());
				writer.Write(fontInfo.OutlineSize);
				writer.Write(fontInfo.PackingSpacing);
				writer.Write(fontInfo.TextureWidth);
				writer.Write(fontInfo.TextureHeight);
				writer.Write(fontInfo.UsePremultipliedTextures);
				writer.Write(fontInfo.UseKerningPairs);
				fontFile.CloseChunk();

				writer = fontFile.OpenChunk(FontHeightChunk);
				writer.Write(fontData.FontHeight);
				writer.Write(fontData.LineHeight);
				writer.Write(fontData.Ascent);
				writer.Write(fontData.Descent);
				fontFile.CloseChunk();

                if (fontInfo.Brush != null)
                {
                    writer = fontFile.OpenChunk(BrushChunk);
                    writer.Write((int)fontInfo.Brush.BrushType);
                    fontInfo.Brush.WriteBrushData(writer);
                    fontFile.CloseChunk();
                }

				if (fontInfo.UseKerningPairs)
				{
					WriteKerningValues(fontData, fontFile);
				}
			}
			finally
			{
				fontFile.Close();
			}
		}

		/// <summary>
		/// Function to read the meta data for font data within a stream.
		/// </summary>
		/// <param name="stream">The stream containing the metadata to read.</param>
		/// <returns>
		/// The font meta data as a <see cref="IGorgonFontInfo"/> value.
		/// </returns>
		protected override IGorgonFontInfo OnGetMetaData(Stream stream)
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

				GorgonBinaryReader reader = fontFile.OpenChunk(FontHeightChunk);

				// Read in information about the font height.
				float fontHeight = reader.ReadSingle();
				float lineHeight = reader.ReadSingle();
				float ascent = reader.ReadSingle();
				float descent = reader.ReadSingle();
				fontFile.CloseChunk();

                if (fontFile.Chunks.Contains(BrushChunk))
                {
                    reader = fontFile.OpenChunk(BrushChunk);
                    var brushType = (GlyphBrushType)reader.ReadInt32();

                    switch (brushType)
                    {
                        case GlyphBrushType.Hatched:
                            fontInfo.Brush = new GorgonGlyphHatchBrush();
                            break;
                        case GlyphBrushType.LinearGradient:
                            fontInfo.Brush = new GorgonGlyphLinearGradientBrush();
                            break;
                        case GlyphBrushType.PathGradient:
                            fontInfo.Brush = new GorgonGlyphPathGradientBrush();
                            break;
                        case GlyphBrushType.Texture:
                            fontInfo.Brush = new GorgonGlyphTextureBrush();
                            break;
                        default:
                            fontInfo.Brush = new GorgonGlyphSolidBrush();
                            break;
                    }

                    fontInfo.Brush.ReadBrushData(reader);
                    fontFile.CloseChunk();
                }
                else
                {
                    fontInfo.Brush = new GorgonGlyphSolidBrush();
                }

				// If this font has kerning data, then load that in.
				IReadOnlyDictionary<GorgonKerningPair, int> kerningPairs = null;

				if (fontFile.Chunks.Contains(KernDataChunk))
				{
					kerningPairs = ReadKernPairs(fontFile);
				}

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

			if (stream == null)
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
        /// Initializes a new instance of the <see cref="GorgonCodecGorFont"/> class.
        /// </summary>
        /// <param name="factory">The font factory that holds cached font information.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="factory"/> parameter is <b>null</b>.</exception>
        public GorgonCodecGorFont(GorgonFontFactory factory)
            : base(factory) => CodecCommonExtensions = new[]
                                    {
                                        ".gorFont"
                                    };
        #endregion
    }
}
