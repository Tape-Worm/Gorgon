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
using System.IO.Compression;
using System.Linq;
using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Graphics.Imaging;
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
		// FONTHIGH chunk.
		private const string FontHeightChunk = "FONTHIGH";
		// TEXTURES chunk.
		private const string TextureChunk = "FNTTXTRS";
		// GLYFDATA chunk.
		private const string GlyphDataChunk = "GLYFDATA";
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
		/// <returns>A new <seealso cref="IGorgonFontInfo"/> containing information about the font.</returns>
		private static IGorgonFontInfo GetFontInfo(GorgonChunkFileReader fontFile)
		{
			GorgonBinaryReader reader = fontFile.OpenChunk(FontInfoChunk);
			GorgonFontInfo info = new GorgonFontInfo(reader.ReadString(), reader.ReadSingle(), reader.ReadValue<FontHeightMode>())
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
		/// Function to read the glyphs from the texture.
		/// </summary>
		/// <param name="fontData">The font data to write.</param>
		/// <param name="textures">The textures for the glyphs.</param>
		/// <param name="fontFile">Font file to read.</param>
		private static void WriteGlyphs(GorgonFont fontData, IReadOnlyDictionary<GorgonTexture, IReadOnlyList<GorgonGlyph>> textures, GorgonChunkFileWriter fontFile)
		{
			// Write glyph data.
			GorgonBinaryWriter writer = fontFile.OpenChunk(GlyphDataChunk);

			GorgonGlyph[] nonTextureGlyphs = (from GorgonGlyph glyph in fontData.Glyphs
			                                  where glyph.TextureView == null
			                                  select glyph).ToArray();

			// Write all information for glyphs that don't use textures (whitespace).
			writer.Write(nonTextureGlyphs.Length);
			foreach (GorgonGlyph glyph in nonTextureGlyphs)
			{
				writer.Write(glyph.Character);
				writer.Write(glyph.Advance);
			}

			// Glyphs are grouped by associated texture.
			writer.Write(textures.Count);

			foreach (KeyValuePair<GorgonTexture, IReadOnlyList<GorgonGlyph>> glyphGroup in textures)
			{
				writer.Write(glyphGroup.Key.Name);
				writer.Write(glyphGroup.Value.Count);

				foreach (GorgonGlyph glyph in glyphGroup.Value)
				{
					writer.Write(glyph.Character);
					writer.Write(glyph.GlyphCoordinates.Left);
					writer.Write(glyph.GlyphCoordinates.Top);
					writer.Write(glyph.GlyphCoordinates.Width);
					writer.Write(glyph.GlyphCoordinates.Height);
					writer.Write(glyph.Offset.X);
					writer.Write(glyph.Offset.Y);
					writer.Write(glyph.OutlineCoordinates.Left);
					writer.Write(glyph.OutlineCoordinates.Top);
					writer.Write(glyph.OutlineCoordinates.Width);
					writer.Write(glyph.OutlineCoordinates.Height);
					writer.Write(glyph.OutlineOffset.X);
					writer.Write(glyph.OutlineOffset.Y);
					writer.Write(glyph.Advance);
					writer.Write(glyph.TextureIndex);
				}
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to write out textures as internal data in the font file.
		/// </summary>
		/// <param name="textures">The textures for the font.</param>
		/// <param name="fontFile">The font file that is being persisted.</param>
		private static void WriteInternalTextures(IReadOnlyDictionary<GorgonTexture, IReadOnlyList<GorgonGlyph>> textures, GorgonChunkFileWriter fontFile)
		{
			GorgonBinaryWriter writer = fontFile.OpenChunk(TextureChunk);

			// Write out how many textures to expect.
			writer.Write(textures.Count);

			foreach (GorgonTexture texture in textures.Keys)
			{
				writer.Write(texture.Name);

				// Ensure that we know how many array indices there are.
				writer.Write(texture.Info.ArrayCount);

				// Compress and store the backing textures for this font.
				using (IGorgonImage imageData = texture.ToImage())
				using (GorgonDataStream memoryStream = new GorgonDataStream(imageData.ImageData))
				using (GZipStream streamData = new GZipStream(writer.BaseStream, CompressionLevel.Optimal, true))
				{
					memoryStream.CopyTo(streamData, 80000);

					// Ensure the remainder gets pumped into the base stream.
					streamData.Flush();
				}
			}

			fontFile.CloseChunk();
		}

		/// <summary>
		/// Function to read the textures from the font file itself.
		/// </summary>
		/// <param name="fontFile">Font file reader.</param>
		/// <param name="width">The width of the texture, in pixels.</param>
		/// <param name="height">The height of the texture, in pixels.</param>
		private IReadOnlyList<GorgonTexture> ReadInternalTextures(GorgonChunkFileReader fontFile, int width, int height)
		{
			IGorgonImage image = null;
			GorgonBinaryReader reader = fontFile.OpenChunk(TextureChunk);
			List<GorgonTexture> result = new List<GorgonTexture>();
			GorgonDataStream memoryStream = null;
			GZipStream textureStream = null;

			try
			{
				int textureCount = reader.ReadInt32();

				for (int i = 0; i < textureCount; i++)
				{
					string textureName = reader.ReadString();

					image = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, BufferFormat.R8G8B8A8_UNorm)
					                        {
						                        ArrayCount = reader.ReadInt32(),
						                        Depth = 1,
						                        MipCount = 1,
						                        Width = width,
						                        Height = height
					                        });

					// If the texture size is invalid, then the file has been corrupted.
					if (reader.BaseStream.Position >= reader.BaseStream.Length)
					{
						throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_CORRUPT);
					}

					// Decompress the image data into our image buffer.
					memoryStream = new GorgonDataStream(image.ImageData);
					textureStream = new GZipStream(reader.BaseStream, CompressionMode.Decompress, true);
					textureStream.CopyTo(memoryStream, 80000);

					// Convert to a texture and add that texture to our internal list.
					result.Add(image.ToTexture(textureName, Factory.Graphics));
					image.Dispose();
					image = null;
				}

				fontFile.CloseChunk();

				return result;
			}
			finally
			{
				image?.Dispose();
				textureStream?.Dispose();
				memoryStream?.Dispose();
			}
		}

		/// <summary>
		/// Function to read the glyphs from the texture.
		/// </summary>
		/// <param name="fontFile">Font file to read.</param>
		/// <param name="textures">The textures for the glyphs.</param>
		private IReadOnlyList<GorgonGlyph> ReadGlyphs(GorgonChunkFileReader fontFile, IReadOnlyList<GorgonTexture> textures)
		{
			List<GorgonGlyph> result = new List<GorgonGlyph>();

			// Get glyph information.
			GorgonBinaryReader reader = fontFile.OpenChunk(GlyphDataChunk);

			// Read all information for glyphs that don't use textures (whitespace).
			int nonTextureGlyphCount = reader.ReadInt32();
			for (int i = 0; i < nonTextureGlyphCount; ++i)
			{
				result.Add(CreateGlyph(reader.ReadChar(), reader.ReadInt32()));
			}

			// Glyphs are grouped by associated texture.
			int groupCount = reader.ReadInt32();

			for (int i = 0; i < groupCount; i++)
			{
				// Get the name of the texture.
				string textureName = reader.ReadString();
				// Get a count of all the glyphs associated with the texture.
				int glyphCount = reader.ReadInt32();

				// Locate the texture in our texture cache.
				GorgonTexture texture = textures.FirstOrDefault(item => string.Equals(textureName, item.Name, StringComparison.OrdinalIgnoreCase));

				// The associated texture was not found, thus the file is corrupt.
				if (texture == null)
				{
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_FONT_GLYPH_TEXTURE_NOT_FOUND, textureName));
				}

				// Read the glyphs for this texture.
				for (int j = 0; j < glyphCount; j++)
				{
					char glyphChar = reader.ReadChar();
					DX.Rectangle glyphRect = new DX.Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
					DX.Point offset = new DX.Point(reader.ReadInt32(), reader.ReadInt32());
					DX.Rectangle outlineRect = new DX.Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
					DX.Point outlineOffset = new DX.Point(reader.ReadInt32(), reader.ReadInt32());
					int advance = reader.ReadInt32();
					int textureIndex = reader.ReadInt32();

					GorgonGlyph glyph;

					// For whitespace, we don't have a texture.
					if (char.IsWhiteSpace(glyphChar))
					{
						glyph = CreateGlyph(glyphChar, advance);
					}
					else
					{
						glyph = CreateGlyph(glyphChar, advance);
						glyph.Offset = offset;
						glyph.OutlineOffset = outlineOffset;
					}

					glyph.UpdateTexture(texture, glyphRect, outlineRect, textureIndex);

					result.Add(glyph);
				}
			}

			fontFile.CloseChunk();

			return result;
		}

		/// <summary>
		/// Function to read the kerning pairs for the font, if they exist.
		/// </summary>
		/// <param name="fontFile">Font file to read.</param>
		private static Dictionary<GorgonKerningPair, int> ReadKernPairs(GorgonChunkFileReader fontFile)
		{
			Dictionary<GorgonKerningPair, int> result = new Dictionary<GorgonKerningPair, int>();
			GorgonBinaryReader reader = fontFile.OpenChunk(KernDataChunk);

			// Read optional kerning information.
			int kernCount = reader.ReadInt32();

			for (int i = 0; i < kernCount; ++i)
			{
				GorgonKerningPair kernPair = new GorgonKerningPair(reader.ReadChar(), reader.ReadChar());
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
			GorgonChunkFileWriter fontFile = new GorgonChunkFileWriter(stream, FileHeader.ChunkID());
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

				IReadOnlyDictionary<GorgonTexture, IReadOnlyList<GorgonGlyph>> textureGlyphs = fontData.Glyphs.GetGlyphsByTexture();

				WriteInternalTextures(textureGlyphs, fontFile);

				WriteGlyphs(fontData, textureGlyphs, fontFile);

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

				return GetFontInfo(fontFile);
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
			GorgonChunkFileReader fontFile = new GorgonChunkFileReader(stream,
			                                         new[]
			                                         {
				                                         FileHeader.ChunkID()
			                                         });

			try
			{
				fontFile.Open();

				IGorgonFontInfo fontInfo = GetFontInfo(fontFile);

				// If the font is already loaded, then just reuse it.
				if (Factory.HasFont(name, fontInfo))
				{
					return Factory.GetFont(name, fontInfo);
				}

				GorgonBinaryReader reader = fontFile.OpenChunk(FontHeightChunk);

				// Read in information about the font height.
				float fontHeight = reader.ReadSingle();
				float lineHeight = reader.ReadSingle();
				float ascent = reader.ReadSingle();
				float descent = reader.ReadSingle();
				fontFile.CloseChunk();

				// Read textures.
				IReadOnlyList<GorgonTexture> textures = ReadInternalTextures(fontFile, fontInfo.TextureWidth, fontInfo.TextureHeight);
				// Get glyphs and bind to textures.
				IReadOnlyList<GorgonGlyph> glyphs = ReadGlyphs(fontFile, textures);

				// If this font has kerning data, then load that in.
				IReadOnlyDictionary<GorgonKerningPair, int> kerningPairs = null;

				if (fontFile.Chunks.Contains(KernDataChunk))
				{
					kerningPairs = ReadKernPairs(fontFile);
				}

				// Finally, assign the font information to the actual font.
				return BuildFont(name, fontInfo, fontHeight, lineHeight, ascent, descent, textures, glyphs, kerningPairs);
			}
			finally
			{
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
			GorgonBinaryReader reader = new GorgonBinaryReader(stream, true);

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
			: base(factory)
		{
			CodecCommonExtensions = new[]
			                        {
				                        ".gorFont"
			                        };
		}
		#endregion
	}
}
