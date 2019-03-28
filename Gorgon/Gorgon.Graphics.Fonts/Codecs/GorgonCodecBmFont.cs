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
// Created: February 24, 2017 9:03:08 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using DX = SharpDX;

namespace Gorgon.Graphics.Fonts.Codecs
{
	/// <summary>
	/// A font codec used to read font data using the BmFont format.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will read fonts created by the BmFont application by Andreas Jönsson (<a href="http://www.angelcode.com/products/bmfont/"/>). 
	/// </para>
	/// <para>
	/// This codec is very limited in scope and as such cannot read all types of font information persisted by BmFont. Below is the list of limitations for this codec:
	/// <list type="bullet">
	///		<item><description>Read only. Cannot output fonts in this format.</description></item>
	///		<item><description>Only supports text .fnt files.</description></item>
	///		<item><description>Does not support outlines due to the difference between how Gorgon handles font outlines and BmFont.</description></item>
	///		<item><description>Supports 32 bit textures only. 8 bit is not supported.</description></item>
	///		<item><description>All channels must be set to "glyph" (pack characters in multiple channels is not supported).</description></item>
	///		<item><description>ClearType anti-aliasing may produce odd artifacts when rendering.</description></item>
	///     <item><description>Importing a BmFont via a stream requires that the stream be a file stream since the textures are stored externally.</description></item>
	/// </list>
	/// </para>
	/// </remarks>
	public class GorgonCodecBmFont
		: GorgonFontCodec
	{
		#region Constants.
		// The line with the font information.
		private const string InfoLine = "info";
		// The line with common information.
		private const string CommonLine = "common";
		// The line that contains texture information.
		private const string PageLine = "page";
		// The line that contains the character count.
		private const string CharCountLine = "chars";
		// The line that contains glyph information.
		private const string CharLine = "char";
		// The line that contains the kerning count.
		private const string KerningCountLine = "kernings";
		// The line that contains kerning information.
		private const string KerningLine = "kerning";
		// The tag representing a count for characters or kernings.
		private const string CountTag = "count";
		// The tag representing the font family.
		private const string FaceTag = "face";
		// The tag representing the font size, in pixels.
		private const string SizeTag = "size";
		// The tag that indicates whether the font is bolded or not.
		private const string BoldTag = "bold";
		// The tag that indicates whether the font is italicized or not.
		private const string ItalicTag = "italic";
		// The tag that indicates whether the font is anti-aliased or not.
		private const string AaTag = "aa";
		// The tag that indicates the spacing between glyphs.
		private const string SpacingTag = "spacing";
		// The tag that indicates the font line height.
		private const string LineHeightTag = "lineHeight";
		// The tag that indicates the texture width.
		private const string ScaleWTag = "scaleW";
		// The tag that indicates the texture height.
		private const string ScaleHTag = "scaleH";
		// The tag that indicates the number of textures.
		private const string PagesTag = "pages";
		// The tag that indicates the ID of the texture.
		private const string PageIdTag = "id";
		// The tag that indicates the name of the texture.
		private const string PageFileTag = "file";
		// The tag that indicates the ID the character.
		private const string CharIdTag = "id";
		// The tag that indicates the x coordinate of the character glyph on the texture, in pixels.
		private const string CharXTag = "x";
		// The tag that indicates the width of the character glyph on the texture, in pixels.
		private const string CharWidthTag = "width";
		// The tag that indicates the y coordinate of the character glyph on the texture, in pixels.
		private const string CharYTag = "y";
		// The tag that indicates the height of the character glyph on the texture, in pixels.
		private const string CharHeightTag = "height";
		// The tag that indicates the horizontal offset of the character glyph on the texture, in pixels.
		private const string CharXOffsetTag = "xoffset";
		// The tag that indicates the vertical offset of the character glyph on the texture, in pixels.
		private const string CharYOffsetTag = "yoffset";
		// The tag that indicates the advancement for the glyph.
		private const string CharAdvanceTag = "xadvance";
		// The tag that indicates the texture that the glyph uses.
		private const string CharPageTag = "page";
		// The tag that indicates the first character in a kerning pair.
		private const string KernFirstTag = "first";
		// The tag that indicates the second character in a kerning pair.
		private const string KernSecondTag = "second";
		// The tag that indicates the the amount of kerning to apply.
		private const string KernAmountTag = "amount";
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the default filename extension for font files.
		/// </summary>
		public override string DefaultFileExtension => ".fnt";

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
		/// <remarks>
		/// While the BmFont format does have the ability to store outlined characters, they are not usable by Gorgon's current outline glyph rendering at this time.
		/// </remarks>
		public override bool SupportsFontOutlines => false;

		/// <summary>
		/// Property to return the friendly description of the codec.
		/// </summary>
		public override string CodecDescription => Resources.GORGFX_DESC_BMFONT_CODEC;

		/// <summary>
		/// Property to return the abbreviated name of the codec (e.g. GorFont).
		/// </summary>
		public override string Codec => "BmFont";
		#endregion

		#region Methods.
		/// <summary>
		/// Function to break a series of line items into a list of key/value pairs for processing.
		/// </summary>
		/// <param name="lineItems">The list of line items to parse.</param>
		/// <returns>A new dictionary containing the key/value pairs.</returns>
		private static Dictionary<string, string> GetLineKeyValuePairs(string[] lineItems)
		{
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (string line in lineItems)
			{
				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				string[] keyValuePair = line.Split(new[]
				                                {
					                                '='
				                                },
				                                StringSplitOptions.RemoveEmptyEntries);

				// If there's no value, then use an empty string for that, otherwise, add as normal.
				result.Add(keyValuePair[0], keyValuePair.Length == 1 ? string.Empty : keyValuePair[1]);
			}

			return result;
		}

		/// <summary>
		/// Function to parse the info line in the file.
		/// </summary>
		/// <param name="line">The line containing the font info.</param>
		/// <returns>A new <seealso cref="BmFontInfo"/> containing some of the font information.</returns>
		private static BmFontInfo ParseInfoLine(string line)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
			}

			string[] infoItems = line.Split(new []
			                                {
				                                ' '
			                                },
			                                StringSplitOptions.RemoveEmptyEntries);

			Dictionary<string, string> keyValues = GetLineKeyValuePairs(infoItems);

			// Check for the "info" tag.
			if (!keyValues.ContainsKey(InfoLine))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
			}

			// Get supported tags.
			string face = keyValues[FaceTag];
			string size = keyValues[SizeTag];
			string bold = keyValues[BoldTag];
			string italic = keyValues[ItalicTag];
			string aa = keyValues[AaTag];
			string spacing = keyValues[SpacingTag];
            FontStyle style = FontStyle.Normal;
			
			if ((string.Equals(bold, "1", StringComparison.OrdinalIgnoreCase))
				&& (string.Equals(italic, "1", StringComparison.OrdinalIgnoreCase)))
			{
				style = FontStyle.BoldItalics;
			} else if (string.Equals(italic, "1", StringComparison.OrdinalIgnoreCase))
			{
				style = FontStyle.Italics;
			} else if (string.Equals(bold, "1", StringComparison.OrdinalIgnoreCase))
			{
				style = FontStyle.Bold;
			}

			// Create with required settings.
			var result = new BmFontInfo(face, Convert.ToSingle(size))
			             {
				             PackingSpacing = spacing.Length > 0 ? Convert.ToInt32(spacing[0]) : 1,
				             FontStyle = style,
				             AntiAliasingMode =
					             (aa.Length > 0 && string.Equals(aa, "1", StringComparison.OrdinalIgnoreCase)) ? FontAntiAliasMode.AntiAlias : FontAntiAliasMode.None,
				             DefaultCharacter = ' '
			             };

			return result;
		}

		/// <summary>
		/// Function to parse the common info line in the font file.
		/// </summary>
		/// <param name="fontInfo">The font information structure to populate.</param>
		/// <param name="line">The line containing the common info.</param>
		private static void ParseCommonLine(BmFontInfo fontInfo, string line)
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
			}

			string[] infoItems = line.Split(new[]
											{
												' '
											},
											StringSplitOptions.RemoveEmptyEntries);

			Dictionary<string, string> keyValues = GetLineKeyValuePairs(infoItems);

			// Check for the "info" tag.
			if (!keyValues.ContainsKey(CommonLine))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
			}

			// Get supported tags.
			string lineHeight = keyValues[LineHeightTag];
			string textureWidth = keyValues[ScaleWTag];
			string textureHeight = keyValues[ScaleHTag];
			string textureCount = keyValues[PagesTag];

			fontInfo.LineHeight = Convert.ToSingle(lineHeight);
			fontInfo.TextureHeight = Convert.ToInt32(textureHeight);
			fontInfo.TextureWidth = Convert.ToInt32(textureWidth);
			fontInfo.FontTextures = new string[Convert.ToInt32(textureCount)];
		}

		/// <summary>
		/// Function to read the texture information from the font.
		/// </summary>
		/// <param name="fontInfo">The font information to update.</param>
		/// <param name="reader">The reader that is reading the file data.</param>
		private static void ParseTextures(BmFontInfo fontInfo, StreamReader reader)
		{
			for (int i = 0; i < fontInfo.FontTextures.Length; ++i)
			{
				string line = reader.ReadLine();

				if (string.IsNullOrWhiteSpace(line))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
				}

				string[] lineItems = line.Split(new[]
				                              {
					                              ' '
				                              },
				                              StringSplitOptions.RemoveEmptyEntries);
				Dictionary<string, string> keyValues = GetLineKeyValuePairs(lineItems);

				if (!keyValues.ContainsKey(PageLine))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
				}

				int id = Convert.ToInt32(keyValues[PageIdTag]);
				string fileName = keyValues[PageFileTag].Trim('\"');

				fontInfo.FontTextures[id] = fileName;
			}
		}

		/// <summary>
		/// Function to read in the character information.
		/// </summary>
		/// <param name="fontInfo">The font information to update.</param>
		/// <param name="reader">The reader that is reading the file data.</param>
		private static void ParseCharacters(BmFontInfo fontInfo, StreamReader reader)
		{
			string countLine = reader.ReadLine();
			var characterList = new StringBuilder();

			if (string.IsNullOrWhiteSpace(countLine))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
			}

			string[] lineItems = countLine.Split(new[]
			                                     {
				                                     ' '
			                                     },
			                                     StringSplitOptions.RemoveEmptyEntries);

			Dictionary<string, string> keyValues = GetLineKeyValuePairs(lineItems);

			if (!keyValues.ContainsKey(CharCountLine))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
			}

			int count = Convert.ToInt32(keyValues[CountTag]);

			// Iterate through the characters so we have enough info to build out glyph data.
			for (int i = 0; i < count; ++i)
			{
				string line = reader.ReadLine();

				if (string.IsNullOrWhiteSpace(line))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
				}

				lineItems = line.Split(new[]
				                       {
					                       ' '
				                       },
				                       StringSplitOptions.RemoveEmptyEntries);

				keyValues = GetLineKeyValuePairs(lineItems);

				if (!keyValues.ContainsKey(CharLine))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
				}

				char character = Convert.ToChar(Convert.ToInt32(keyValues[CharIdTag]));
				characterList.Append(character);

				fontInfo.GlyphRects[character] = new DX.Rectangle(Convert.ToInt32(keyValues[CharXTag]),
				                                                  Convert.ToInt32(keyValues[CharYTag]),
				                                                  Convert.ToInt32(keyValues[CharWidthTag]),
				                                                  Convert.ToInt32(keyValues[CharHeightTag]));
				fontInfo.GlyphOffsets[character] = new DX.Point(Convert.ToInt32(keyValues[CharXOffsetTag]), Convert.ToInt32(keyValues[CharYOffsetTag]));
				fontInfo.CharacterAdvances[character] = Convert.ToInt32(keyValues[CharAdvanceTag]);
				fontInfo.GlyphTextureIndices[character] = Convert.ToInt32(keyValues[CharPageTag]);
				
			}

			fontInfo.Characters = characterList.ToString();
		}

		/// <summary>
		/// Function to read in the kerning information.
		/// </summary>
		/// <param name="fontInfo">The font information to update.</param>
		/// <param name="reader">The reader that is reading the file data.</param>
		private static void ParseKerning(BmFontInfo fontInfo, StreamReader reader)
		{
			if (reader.EndOfStream)
			{
				fontInfo.UseKerningPairs = false;
				return;
			}
			
			string countLine = reader.ReadLine();

			if (string.IsNullOrWhiteSpace(countLine))
			{
				fontInfo.UseKerningPairs = false;
				return;
			}

			string[] lineItems = countLine.Split(new[]
												 {
													 ' '
												 },
												 StringSplitOptions.RemoveEmptyEntries);

			Dictionary<string, string> keyValues = GetLineKeyValuePairs(lineItems);

			if (!keyValues.ContainsKey(KerningCountLine))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
			}

			int count = Convert.ToInt32(keyValues[CountTag]);

			if (count < 1)
			{
				fontInfo.UseKerningPairs = false;
				return;
			}

			fontInfo.UseKerningPairs = true;

			// Iterate through the characters so we have enough info to build out glyph data.
			for (int i = 0; i < count; ++i)
			{
				string line = reader.ReadLine();

				if (string.IsNullOrWhiteSpace(line))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
				}

				lineItems = line.Split(new[]
									   {
										   ' '
									   },
									   StringSplitOptions.RemoveEmptyEntries);

				keyValues = GetLineKeyValuePairs(lineItems);

				if (!keyValues.ContainsKey(KerningLine))
				{
					throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
				}

				var pair = new GorgonKerningPair(Convert.ToChar(Convert.ToInt32(keyValues[KernFirstTag])), Convert.ToChar(Convert.ToInt32(keyValues[KernSecondTag])));
				fontInfo.KerningPairs[pair] = Convert.ToInt32(keyValues[KernAmountTag]);
			}
		}

		/// <summary>
		/// Function to retrieve the image codec based on the file name.
		/// </summary>
		/// <param name="fileNameExtension">The file name extension to evaluate.</param>
		/// <returns>A new <seealso cref="IGorgonImageCodec"/> to read the file with.</returns>
		private static IGorgonImageCodec GetImageCodec(string fileNameExtension)
		{
			if (!fileNameExtension.StartsWith("."))
			{
				fileNameExtension = "." + fileNameExtension;
			}

            switch (fileNameExtension.ToUpperInvariant())
			{
				case ".DDS":
					return new GorgonCodecDds();
				case ".PNG":
					return new GorgonCodecPng();
				case ".TGA":
					return new GorgonCodecTga();
				default:
					throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_FONT_TEXTURE_NOT_VALID, "*.dds\n*.png\n*.tga"));
			}
		}

		/// <summary>
		/// Function to read the textures for the font.
		/// </summary>
		/// <param name="filePath">The path to the font file.</param>
		/// <param name="fontInfo">The information about the font.</param>
		/// <returns>A list of textures.</returns>
		private IReadOnlyList<GorgonTexture2D> ReadTextures(string filePath, BmFontInfo fontInfo)
		{
			var textures = new GorgonTexture2D[fontInfo.FontTextures.Length];
			var directory = new DirectoryInfo(Path.GetDirectoryName(filePath).FormatDirectory(Path.DirectorySeparatorChar));

			Debug.Assert(directory.Exists, "Font directory should exist, but does not.");

			for (int i = 0; i < fontInfo.FontTextures.Length; ++i)
			{
				var fileInfo = new FileInfo(directory.FullName.FormatDirectory(Path.DirectorySeparatorChar) + fontInfo.FontTextures[i].FormatFileName());

				if (!fileInfo.Exists)
				{
					throw new FileNotFoundException(string.Format(Resources.GORGFX_ERR_FONT_TEXTURE_FILE_NOT_FOUND, fileInfo.FullName));
				}

				IGorgonImageCodec codec = GetImageCodec(fileInfo.Extension);

				using (IGorgonImage image = codec.LoadFromFile(fileInfo.FullName))
				{
				    image.ToTexture2D(Factory.Graphics,
				                      new GorgonTexture2DLoadOptions
				                      {
				                          Name = $"BmFont_Texture_{Guid.NewGuid():N}"
				                      });

				}
			}

			return textures;
		}

		/// <summary>
		/// Function to build builds from the font information.
		/// </summary>
		/// <param name="textures">The list of textures loaded.</param>
		/// <param name="fontInfo">The font information to retrieve glyph data from.</param>
		/// <returns>A new list of glyphs.</returns>
		private IReadOnlyList<GorgonGlyph> GetGlyphs(IReadOnlyList<GorgonTexture2D> textures, BmFontInfo fontInfo)
		{
			var glyphs = new List<GorgonGlyph>();

			foreach (char character in fontInfo.Characters)
			{
				int advance = fontInfo.CharacterAdvances[character];

				// Build a glyph that is not linked to a texture if it's whitespace.
				if (char.IsWhiteSpace(character))
				{
					glyphs.Add(CreateGlyph(character, advance));
					continue;
				}

				int textureIndex = fontInfo.GlyphTextureIndices[character];
				GorgonTexture2D texture = textures[textureIndex];

				DX.Rectangle glyphRectangle = fontInfo.GlyphRects[character];
				DX.Point offset = fontInfo.GlyphOffsets[character];

				GorgonGlyph glyph = CreateGlyph(character, advance);
				glyph.Offset = offset;
				glyph.UpdateTexture(texture, glyphRectangle, DX.Rectangle.Empty, 0);

				glyphs.Add(glyph);
			}

			return glyphs;
		}

        /// <summary>
        /// Function to write the font data to the stream.
        /// </summary>
        /// <param name="fontData">The font data to write.</param>
        /// <param name="stream">The stream to write into.</param>
        /// <exception cref="NotSupportedException">This operation is not supported by this codec.</exception>
        protected override void OnWriteFontData(GorgonFont fontData, Stream stream) => throw new NotSupportedException();

        /// <summary>
        /// Function to read the meta data for font data within a stream.
        /// </summary>
        /// <param name="stream">The stream containing the metadata to read.</param>
        /// <returns>
        /// The font meta data as a <see cref="IGorgonFontInfo"/> value.
        /// </returns>
        protected override IGorgonFontInfo OnGetMetaData(Stream stream)
		{
			using (var reader = new StreamReader(stream, Encoding.ASCII, true, 80000, true))
			{
				BmFontInfo result = ParseInfoLine(reader.ReadLine());
				ParseCommonLine(result, reader.ReadLine());
				ParseTextures(result, reader);
				ParseCharacters(result, reader);
				ParseKerning(result, reader);

				return result;
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
		    if (!(stream is FileStream fileStream))
			{
				throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_BMFONT_NEEDS_FILE_STREAM);
			}

			var fontInfo = (BmFontInfo)OnGetMetaData(fileStream);

			// Read in textures.
			IReadOnlyList<GorgonTexture2D> textures = ReadTextures(fileStream.Name, fontInfo);
			// Get glyphs
			IReadOnlyList<GorgonGlyph> glyphs = GetGlyphs(textures, fontInfo);
			// Get kerning pairs.
			IReadOnlyDictionary<GorgonKerningPair, int> kerningPairs = null;

			// If we're using kerning pairs, then copy the kerning pairs to a read only dictionary.
			if (fontInfo.UseKerningPairs)
			{
				kerningPairs = fontInfo.KerningPairs.ToDictionary(k => k.Key, v => v.Value);
			}

			return BuildFont(new GorgonFontInfo(fontInfo), fontInfo.LineHeight, fontInfo.LineHeight, -1, -1, textures, glyphs, kerningPairs);
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

			long position = stream.Position;
			var reader = new StreamReader(stream, Encoding.ASCII, true, 80000, true);
			
			try
			{
				string line = reader.ReadLine();
				return line?.StartsWith("info ", StringComparison.OrdinalIgnoreCase) ?? false;
			}
			finally
			{
				reader.Dispose();
				stream.Position = position;
			}
		}
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonCodecGorFont" /> class.
        /// </summary>
        /// <param name="factory">The font factory that holds cached font information.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="factory"/> parameter is <b>null</b>.</exception>
        public GorgonCodecBmFont(GorgonFontFactory factory)
            : base(factory) => CodecCommonExtensions = new[]
                                    {
                                        ".fnt"
                                    };
        #endregion
    }
}
