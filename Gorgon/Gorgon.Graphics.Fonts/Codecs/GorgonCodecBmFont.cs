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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Graphics.Fonts.Properties;

namespace Gorgon.Graphics.Fonts.Codecs
{
    /// <summary>
    /// A font codec used to read font data using the BmFont format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will read fonts created by the <a href="https://www.angelcode.com/products/bmfont/" target="_blank">BmFont</a> application by Andreas Jönsson. 
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
        // The line that contains the character count.
        private const string CharCountLine = "chars";
        // The line that contains glyph information.
        private const string CharLine = "char";
        // The line that contains the kerning count.
        private const string KerningCountLine = "kernings";
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
        // The tag that indicates the texture width.
        private const string ScaleWTag = "scaleW";
        // The tag that indicates the texture height.
        private const string ScaleHTag = "scaleH";
        // The tag that indicates the number of textures.
        private const string PagesTag = "pages";
        // The tag that indicates the ID the character.
        private const string CharIdTag = "id";
        // The tag that indicates whether the font has an outline.
        private const string OutlineTag = "outline";
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
        public override bool SupportsFontOutlines => true;

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
        private static Dictionary<string, string> GetLineKeyValuePairs(string lineItems)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Strip out identifier.
            int keySep = lineItems.IndexOf(' ');

            if ((keySep == -1) || (keySep == lineItems.Length - 1))
            {
                return result;
            }

            result[lineItems.Substring(0, keySep).Trim()] = string.Empty;
            lineItems = lineItems[(keySep + 1)..].Trim();

            // Parse items.
            while (lineItems.Length > 0)
            {
                int valueSep = lineItems.IndexOf('=');

                if ((valueSep == -1) || (valueSep == lineItems.Length - 1))
                {
                    break;
                }

                string key = lineItems.Substring(0, valueSep).Trim();
                string value = string.Empty;
                
                lineItems = lineItems[(valueSep + 1)..].Trim();

                if (lineItems.Length == 0)
                {
                    break;
                }

                // We have a text field, parse it.
                if (lineItems[0] == '"')
                {
                    int start = 0;

                    while ((lineItems[++start] != '"') && (start < lineItems.Length))
                    {
                        value += lineItems[start];
                    }

                    if (start >= lineItems.Length)
                    {
                        break;
                    }

                    lineItems = lineItems[(start + 1)..].Trim();
                }
                else
                {
                    int valueEndSep = lineItems.IndexOf(' ');

                    if (valueEndSep != -1)
                    {
                        value = lineItems.Substring(0, valueEndSep).Trim();
                        lineItems = lineItems[(valueEndSep + 1)..].Trim();
                    }
                    else 
                    {
                        value = lineItems;
                        lineItems = string.Empty;                        
                    }                    
                }

                result[key] = value;
            }

            return result;
        }

        /// <summary>
        /// Function to parse the info line in the file.
        /// </summary>
        /// <param name="line">The line containing the font info.</param>
        /// <returns>A new <seealso cref="GorgonFontInfo"/> containing some of the font information.</returns>
        private static GorgonFontInfo ParseInfoLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
            }

            Dictionary<string, string> keyValues = GetLineKeyValuePairs(line);

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
            string outline = keyValues[OutlineTag];
            FontStyle style = FontStyle.Normal;

            if ((string.Equals(bold, "1", StringComparison.OrdinalIgnoreCase))
                && (string.Equals(italic, "1", StringComparison.OrdinalIgnoreCase)))
            {
                style = FontStyle.BoldItalics;
            }
            else if (string.Equals(italic, "1", StringComparison.OrdinalIgnoreCase))
            {
                style = FontStyle.Italics;
            }
            else if (string.Equals(bold, "1", StringComparison.OrdinalIgnoreCase))
            {
                style = FontStyle.Bold;
            }

            int packSpacing = 1;

            if (spacing.Length > 0)
            {
                string[] values = spacing.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > 0)
                {
                    packSpacing = Convert.ToInt32(values[0]);
                }
            }

            // Create with required settings.
            var result = new GorgonFontInfo(face, Convert.ToSingle(size))
            {
                PackingSpacing = packSpacing,
                FontStyle = style,
                AntiAliasingMode = ((aa.Length > 0) && (string.Equals(aa, "1", StringComparison.OrdinalIgnoreCase))) ? FontAntiAliasMode.AntiAlias : FontAntiAliasMode.None,
                DefaultCharacter = ' ',
                Brush = new GorgonGlyphSolidBrush(),                
            };

            if (int.TryParse(outline, out int outlineSize))
            {
                result.OutlineColor1 = result.OutlineColor2 = GorgonColor.Black;
                result.OutlineSize = outlineSize;
            }

            return result;
        }

        /// <summary>
        /// Function to parse the common info line in the font file.
        /// </summary>
        /// <param name="fontInfo">The font information structure to populate.</param>
        /// <param name="line">The line containing the common info.</param>
        /// <param name="textureSkipCount">The number of texture lines to be skipped.</param>
        private static void ParseCommonLine(GorgonFontInfo fontInfo, string line, out int textureSkipCount)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
            }

            Dictionary<string, string> keyValues = GetLineKeyValuePairs(line);

            // Check for the "info" tag.
            if (!keyValues.ContainsKey(CommonLine))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
            }

            // Get supported tags.
            string textureWidth = keyValues[ScaleWTag];
            string textureHeight = keyValues[ScaleHTag];
            string textureCount = keyValues[PagesTag];

            fontInfo.TextureHeight = Convert.ToInt32(textureHeight);
            fontInfo.TextureWidth = Convert.ToInt32(textureWidth);
            textureSkipCount = Convert.ToInt32(textureCount);
        }

        /// <summary>
        /// Function to skip the texture information from the font.
        /// </summary>
        /// <param name="textureCount">The number of texture lines to skip.</param>
        /// <param name="reader">The reader that is reading the file data.</param>
        private static void SkipTextures(StreamReader reader, int textureCount)
        {
            for (int i = 0; i < textureCount; ++i)
            {
                reader.ReadLine();
            }
        }

        /// <summary>
        /// Function to read in the character information.
        /// </summary>
        /// <param name="fontInfo">The font information to update.</param>
        /// <param name="reader">The reader that is reading the file data.</param>
        private static void ParseCharacters(GorgonFontInfo fontInfo, StreamReader reader)
        {
            string countLine = reader.ReadLine();
            var characterList = new StringBuilder();

            if (string.IsNullOrWhiteSpace(countLine))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
            }

            Dictionary<string, string> keyValues = GetLineKeyValuePairs(countLine);

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

                keyValues = GetLineKeyValuePairs(line);

                if (!keyValues.ContainsKey(CharLine))
                {
                    throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
                }

                characterList.Append(Convert.ToChar(Convert.ToInt32(keyValues[CharIdTag])));
            }

            fontInfo.Characters = characterList.ToString();
        }

        /// <summary>
        /// Function to read in the kerning information.
        /// </summary>
        /// <param name="fontInfo">The font information to update.</param>
        /// <param name="reader">The reader that is reading the file data.</param>
        private static void ParseKerning(GorgonFontInfo fontInfo, StreamReader reader)
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

            Dictionary<string, string> keyValues = GetLineKeyValuePairs(countLine);

            if (!keyValues.ContainsKey(KerningCountLine))
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
            }

            int count = Convert.ToInt32(keyValues[CountTag]);
            fontInfo.UseKerningPairs = count >= 1;

            if (!fontInfo.UseKerningPairs)
            {
                return;
            }

            // Skip these lines, we don't need them.
            for (int i = 0; i < count; ++i)
            {
                reader.ReadLine();
            }
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
            using var reader = new StreamReader(stream, Encoding.ASCII, true, 80000, true);
            GorgonFontInfo result = ParseInfoLine(reader.ReadLine());
            ParseCommonLine(result, reader.ReadLine(), out int textureLineSkip);
            SkipTextures(reader, textureLineSkip);
            ParseCharacters(result, reader);
            ParseKerning(result, reader);

            return result;
        }

        /// <summary>
        /// Function to load the font data, with the specified size, from a stream.
        /// </summary>
        /// <param name="name">The name to assign to the font.</param>
        /// <param name="stream">The stream containing the font data.</param>
        /// <returns>A new <seealso cref="GorgonFont"/>, or, an existing font from the <seealso cref="GorgonFontFactory"/> cache.</returns>
        protected override Task<GorgonFont> OnLoadFromStreamAsync(string name, Stream stream)
        {
            if (stream is not FileStream fileStream)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_BMFONT_NEEDS_FILE_STREAM);
            }

            IGorgonFontInfo fontInfo = OnGetMetaData(fileStream);

            return Factory.GetFontAsync(fontInfo);
        }

        /// <summary>
        /// Function to load the font data, with the specified size, from a stream.
        /// </summary>
        /// <param name="name">The name to assign to the font.</param>
        /// <param name="stream">The stream containing the font data.</param>
        /// <returns>A new <seealso cref="GorgonFont"/>, or, an existing font from the <seealso cref="GorgonFontFactory"/> cache.</returns>
        protected override GorgonFont OnLoadFromStream(string name, Stream stream)
        {
            if (stream is not FileStream fileStream)
            {
                throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_BMFONT_NEEDS_FILE_STREAM);
            }

            IGorgonFontInfo fontInfo = OnGetMetaData(fileStream);

            return Factory.GetFont(fontInfo);
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
        /// Initializes a new instance of the <see cref="GorgonCodecBmFont" /> class.
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
