﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 24, 2017 9:03:08 AM
// 

using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Fonts.Properties;

namespace Gorgon.Graphics.Fonts.Codecs;

/// <summary>
/// A font codec used to read font data using the BmFont format
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

    /// <summary>
    /// Function to break a series of line items into a list of key/value pairs for processing.
    /// </summary>
    /// <param name="lineItems">The list of line items to parse.</param>
    /// <returns>A new dictionary containing the key/value pairs.</returns>
    private static Dictionary<string, string> GetLineKeyValuePairs(string lineItems)
    {
        Dictionary<string, string> result = new(StringComparer.OrdinalIgnoreCase);

        // Strip out identifier.
        int keySep = lineItems.IndexOf(' ');

        if ((keySep == -1) || (keySep == lineItems.Length - 1))
        {
            return result;
        }

        result[lineItems[..keySep].Trim()] = string.Empty;
        lineItems = lineItems[(keySep + 1)..].Trim();

        // Parse items.
        while (lineItems.Length > 0)
        {
            int valueSep = lineItems.IndexOf('=');

            if ((valueSep == -1) || (valueSep == lineItems.Length - 1))
            {
                break;
            }

            string key = lineItems[..valueSep].Trim();
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
                    value = lineItems[..valueEndSep].Trim();
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
        GorgonFontStyle style = GorgonFontStyle.Normal;

        if ((string.Equals(bold, "1", StringComparison.OrdinalIgnoreCase))
            && (string.Equals(italic, "1", StringComparison.OrdinalIgnoreCase)))
        {
            style = GorgonFontStyle.BoldItalics;
        }
        else if (string.Equals(italic, "1", StringComparison.OrdinalIgnoreCase))
        {
            style = GorgonFontStyle.Italics;
        }
        else if (string.Equals(bold, "1", StringComparison.OrdinalIgnoreCase))
        {
            style = GorgonFontStyle.Bold;
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

        int.TryParse(outline, out int outlineSize);

        // Create with required settings.
        GorgonFontInfo result = new(face, Convert.ToSingle(size), GorgonFontHeightMode.Pixels)
        {
            PackingSpacing = packSpacing,
            FontStyle = style,
            AntiAliasingMode = ((aa.Length > 0) && (string.Equals(aa, "1", StringComparison.OrdinalIgnoreCase))) ? GorgonFontAntiAliasMode.AntiAlias : GorgonFontAntiAliasMode.None,
            DefaultCharacter = ' ',
            Brush = new GorgonGlyphSolidBrush(),
            OutlineColor1 = outlineSize > 0 ? GorgonColors.Black : GorgonColors.BlackTransparent,
            OutlineColor2 = outlineSize > 0 ? GorgonColors.Black : GorgonColors.BlackTransparent,
            OutlineSize = outlineSize
        };

        return result;
    }

    /// <summary>
    /// Function to parse the common info line in the font file.
    /// </summary>
    /// <param name="line">The line containing the common info.</param>
    /// <param name="textureInfo">The texture info from the line.</param>
    private static void ParseCommonLine(string line, out (int textureSkipCount, int textureWidth, int textureHeight) textureInfo)
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
        textureInfo = (Convert.ToInt32(textureCount), Convert.ToInt32(textureWidth), Convert.ToInt32(textureHeight));
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
    /// <param name="reader">The reader that is reading the file data.</param>
    /// <returns>The list of characters as a string.</returns>
    private static string ParseCharacters(StreamReader reader)
    {
        string countLine = reader.ReadLine();
        StringBuilder characterList = new();

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

        return characterList.ToString();
    }

    /// <summary>
    /// Function to read in the kerning information.
    /// </summary>
    /// <param name="reader">The reader that is reading the file data.</param>
    /// <returns>The kerning flag.</returns>
    private static bool ParseKerning(StreamReader reader)
    {
        if (reader.EndOfStream)
        {
            return false;
        }

        string countLine = reader.ReadLine();

        if (string.IsNullOrWhiteSpace(countLine))
        {
            return true;
        }

        Dictionary<string, string> keyValues = GetLineKeyValuePairs(countLine);

        if (!keyValues.ContainsKey(KerningCountLine))
        {
            throw new GorgonException(GorgonResult.CannotRead, Resources.GORGFX_ERR_FONT_FILE_FORMAT_INVALID);
        }

        int count = Convert.ToInt32(keyValues[CountTag]);

        if (count < 1)
        {
            return false;
        }

        // Skip these lines, we don't need them.
        for (int i = 0; i < count; ++i)
        {
            reader.ReadLine();
        }
        return true;
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
    /// The font meta data as a <see cref="GorgonFontInfo"/> value.
    /// </returns>
    protected override GorgonFontInfo OnGetMetaData(Stream stream)
    {
        using StreamReader reader = new(stream, Encoding.ASCII, true, 80000, true);
        GorgonFontInfo result = ParseInfoLine(reader.ReadLine());
        ParseCommonLine(reader.ReadLine(), out (int TextureLineSkip, int TextureWidth, int TextureHeight) textureInfo);
        SkipTextures(reader, textureInfo.TextureLineSkip);
        string characters = ParseCharacters(reader);
        ParseKerning(reader);

        result = result with
        {
            Characters = characters,
            TextureWidth = textureInfo.TextureWidth,
            TextureHeight = textureInfo.TextureHeight
        };

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

        GorgonFontInfo fontInfo = OnGetMetaData(fileStream);

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

        GorgonFontInfo fontInfo = OnGetMetaData(fileStream);

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
        StreamReader reader = new(stream, Encoding.ASCII, true, 80000, true);

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

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonCodecBmFont" /> class.
    /// </summary>
    /// <param name="factory">The font factory that holds cached font information.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="factory"/> parameter is <b>null</b>.</exception>
    public GorgonCodecBmFont(GorgonFontFactory factory)
        : base(factory) => CodecCommonExtensions =
                                [
                                    ".fnt"
                                ];

}
