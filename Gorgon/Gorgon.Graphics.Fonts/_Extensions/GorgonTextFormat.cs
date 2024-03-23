
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
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
// Created: January 26, 2021 1:16:56 PM
// 

using System.Numerics;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Fonts.Properties;
using Gorgon.Math;

namespace Gorgon.Graphics.Fonts;

/// <summary>
/// Functionality for formatting text using a <see cref="GorgonFont"/>
/// </summary>
public static class GorgonTextFormat
{
    /// <summary>
    /// Function to measure the width of an individual line of text.
    /// </summary>
    /// <param name="font">The font to use.</param>
    /// <param name="line">The line to measure.</param>
    /// <param name="useOutline"><b>true</b> to use the font outline, <b>false</b> to disregard it.</param>
    /// <returns>The width of the line.</returns>
    private static float GetLineWidth(GorgonFont font, string line, bool useOutline)
    {
        float size = 0;
        bool firstChar = true;

        if (!font.TryGetDefaultGlyph(out GorgonGlyph defaultGlyph))
        {
            throw new GorgonException(GorgonResult.CannotEnumerate, string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, font.DefaultCharacter));
        }

        if (!font.HasOutline)
        {
            useOutline = false;
        }

        for (int i = 0; i < line.Length; i++)
        {
            char character = line[i];

            if (!font.Glyphs.TryGetValue(character, out GorgonGlyph glyph))
            {
                glyph = defaultGlyph;
            }

            // Skip out on carriage returns and newlines.
            if (character is '\r' or '\n')
            {
                continue;
            }

            // Whitespace will use the glyph width.
            if (char.IsWhiteSpace(character))
            {
                size += glyph.Advance;
                continue;
            }

            // Include the initial offset.
            if (firstChar)
            {
                size += (useOutline && glyph.OutlineCoordinates.Width > 0) ? glyph.OutlineOffset.X : glyph.Offset.X;
                firstChar = false;
            }

            size += glyph.Advance;

            if (!font.UseKerningPairs)
            {
                continue;
            }

            if ((i == line.Length - 1)
                || (font.KerningPairs.Count == 0))
            {
                continue;
            }

            GorgonKerningPair kerning = new(character, line[i + 1]);

            if (font.KerningPairs.TryGetValue(kerning, out int kernAmount))
            {
                size += kernAmount;
            }
        }

        return size;
    }

    /// <summary>
    /// Function to format a string for rendering using a <see cref="GorgonFont"/>.
    /// </summary>
    /// <param name="renderText">The text to render.</param>
    /// <param name="tabSpacing">[Optional] The number of spaces used to replace a tab control character.</param>
    /// <returns>The formatted text.</returns>
    /// <remarks>
    /// <para>
    /// This method will format the string so that all control characters such as carriage return, and tabs are converted into spaces. 
    /// </para>
    /// <para>
    /// If the <paramref name="tabSpacing"/> parameter is changed from its default of 4, then that will be the number of spaces substituted for the tab control character.
    /// </para>
    /// </remarks>
    public static string FormatStringForRendering(this string renderText, int tabSpacing = 4)
    {
        if (string.IsNullOrEmpty(renderText))
        {
            return string.Empty;
        }

        StringBuilder workingBuffer = new(renderText);
        tabSpacing = tabSpacing.Max(1);

        // Strip all carriage returns.
        workingBuffer.Replace("\r", string.Empty);

        // Convert tabs to spaces.
        workingBuffer.Replace("\t", new string(' ', tabSpacing));

        return workingBuffer.ToString();
    }

    /// <summary>
    /// Function to perform word wrapping on a string based on a Gorgon font.
    /// </summary>
    /// <param name="text">The text to word wrap.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="wordWrapWidth">The maximum width, in pixels, that must be met for word wrapping to occur.</param>
    /// <returns>The string with word wrapping.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="font"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="font"/> does not have a glyph for its default character.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="wordWrapWidth"/> is the maximum number of pixels required for word wrapping, if an individual font glyph cell width (the <see cref="GorgonGlyph.Offset"/> + 
    /// <see cref="GorgonGlyph.Advance"/>) exceeds that of the <paramref name="wordWrapWidth"/>, then the parameter value is updated to glyph cell width.
    /// </para>
    /// </remarks>
    public static string WordWrap(this string text, GorgonFont font, float wordWrapWidth)
    {
        if (font is null)
        {
            throw new ArgumentNullException(nameof(font));
        }

        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        StringBuilder wordText = new(text);

        if (!font.TryGetDefaultGlyph(out GorgonGlyph defaultGlyph))
        {
            throw new GorgonException(GorgonResult.CannotEnumerate, string.Format(Resources.GORGFX_ERR_FONT_DEFAULT_CHAR_NOT_VALID, font.DefaultCharacter));
        }

        int maxLength = wordText.Length;
        int index = 0;
        float position = 0.0f;
        bool firstChar = true;

        while (index < maxLength)
        {
            char character = wordText[index];

            // Don't count newline or carriage return.
            if (character is '\n' or '\r')
            {
                firstChar = true;
                position = 0;
                ++index;
                continue;
            }

            if (!font.Glyphs.TryGetValue(character, out GorgonGlyph glyph))
            {
                glyph = defaultGlyph;
            }

            float glyphCellWidth = glyph.Advance;

            if (firstChar)
            {
                glyphCellWidth += glyph.Offset.X;
                firstChar = false;
            }

            // If we're using kerning, then adjust for the kerning value.
            if ((font.UseKerningPairs)
                && (index < maxLength - 1))
            {
                if (font.KerningPairs.TryGetValue(new GorgonKerningPair(character, wordText[index + 1]), out int kernValue))
                {
                    glyphCellWidth += kernValue;
                }
            }

            position += glyphCellWidth;

            // Update the word wrap boundary if the cell size exceeds it.
            if (glyphCellWidth > wordWrapWidth)
            {
                wordWrapWidth = glyphCellWidth;
            }

            // We're not at the break yet.
            if (position < wordWrapWidth)
            {
                ++index;
                continue;
            }

            int whiteSpaceIndex = index;

            // If we hit the max width, then we need to find the previous whitespace and inject a newline.
            while ((whiteSpaceIndex <= index) && (whiteSpaceIndex >= 0))
            {
                char breakChar = wordText[whiteSpaceIndex];

                if ((char.IsWhiteSpace(breakChar))
                    && (breakChar != '\n')
                    && (breakChar != '\r'))
                {
                    index = whiteSpaceIndex;
                    break;
                }

                --whiteSpaceIndex;
            }

            // If we're at the beginning, then we cannot wrap this text, so we'll break it at the border specified.
            if (index != whiteSpaceIndex)
            {
                if (index != 0)
                {
                    wordText.Insert(index, '\n');
                    maxLength = wordText.Length;
                    ++index;
                }
                position = 0;
                firstChar = true;
                // Move to next character.
                ++index;
                continue;
            }

            // Extract the space.
            wordText[whiteSpaceIndex] = '\n';
            position = 0;
            firstChar = true;
            index = whiteSpaceIndex + 1;
        }

        return wordText.ToString();
    }

    /// <summary>
    /// Function to measure a single line of text using a Gorgon font.
    /// </summary>
    /// <param name="text">The single line of text to measure.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="useOutline">[Optional] <b>true</b> to include the outline in the measurement, <b>false</b> to exclude.</param>
    /// <param name="lineSpacing">[Optional] The factor used to determine the amount of space between each line.</param>
    /// <returns>A vector containing the width and height of the text line when rendered using this font.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="font"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="font"/> does not have a glyph for its default character.</exception>
    /// <remarks>
    /// <para>
    /// This will measure the specified <paramref name="text"/> and return the size, in pixels, of the region containing the text. Unlike the <see cref="MeasureText"/> method, this method does not format 
    /// the text or take into account newline/carriage returns. It is meant for a single line of text only.
    /// </para>
    /// <para>
    /// If the <paramref name="useOutline"/> parameter is <b>true</b>, then the outline size is taken into account when measuring, otherwise only the standard glyph size is taken into account. If the font 
    /// <see cref="GorgonFont.HasOutline"/> property is <b>false</b>, then this parameter is ignored.
    /// </para>
    /// <para>
    /// The <paramref name="lineSpacing"/> parameter adjusts the amount of space between each line by multiplying it with the <see cref="GorgonFont.FontHeight"/> value (and the 
    /// <see cref="IGorgonFontInfo.OutlineSize"/> * 2 if <paramref name="useOutline"/> is <b>true</b> and <see cref="GorgonFont.HasOutline"/> is <b>true</b>). For example, to achieve a double spacing 
    /// effect, change this value to 2.0f.
    /// </para>
    /// </remarks>
    /// <seealso cref="MeasureText"/>
    public static Vector2 MeasureLine(this string text, GorgonFont font, bool useOutline = false, float lineSpacing = 1.0f)
    {
        if (font is null)
        {
            throw new ArgumentNullException(nameof(font));
        }

        if (string.IsNullOrEmpty(text))
        {
            return Vector2.Zero;
        }

        float lineWidth = GetLineWidth(font, text, useOutline);

        if ((font.HasOutline) && (useOutline))
        {
            lineWidth += font.OutlineSize;
        }

        return new Vector2(lineWidth, font.FontHeight.FastFloor() * lineSpacing);
    }

    /// <summary>
    /// Function to measure the specified text using a Gorgon font.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="useOutline">[Optional] <b>true</b> to include the outline in the measurement, <b>false</b> to exclude.</param>
    /// <param name="tabSpaceCount">[Optional] The number of spaces represented by a tab control character.</param>
    /// <param name="lineSpacing">[Optional] The factor used to determine the amount of space between each line.</param>
    /// <param name="wordWrapWidth">[Optional] The maximum width to return if word wrapping is required.</param>
    /// <returns>A vector containing the width and height of the text when rendered using this font.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="font"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="font"/> does not have a glyph for its default character.</exception>
    /// <remarks>
    /// <para>
    /// This will measure the specified <paramref name="text"/> and return the size, in pixels, of the region containing the text.
    /// </para>
    /// <para>
    /// If the <paramref name="wordWrapWidth"/> is specified and greater than zero, then word wrapping is assumed to be on and the text will be handled using word wrapping.
    /// </para>
    /// <para>
    /// If the <paramref name="useOutline"/> parameter is <b>true</b>, then the outline size is taken into account when measuring, otherwise only the standard glyph size is taken into account. If the 
    /// font <see cref="GorgonFont.HasOutline"/> property is <b>false</b>, then this parameter is ignored.
    /// </para>
    /// <para>
    /// The <paramref name="lineSpacing"/> parameter adjusts the amount of space between each line by multiplying it with the <see cref="GorgonFont.FontHeight"/> value (and the 
    /// <see cref="IGorgonFontInfo.OutlineSize"/> * 2 if <paramref name="useOutline"/> is <b>true</b> and <see cref="GorgonFont.HasOutline"/> is <b>true</b>). For example, to achieve a double spacing 
    /// effect, change this value to 2.0f.
    /// </para>
    /// <para>
    /// If measuring a single line of text with no breaks (i.e. newline or carriage return), and no word wrapping, then call the <see cref="MeasureLine"/> method instead for better performance.
    /// </para>
    /// </remarks>
    /// <seealso cref="MeasureLine"/>
    public static Vector2 MeasureText(this string text, GorgonFont font, bool useOutline = false, int tabSpaceCount = 4, float lineSpacing = 1.0f, float? wordWrapWidth = null)
    {
        if (font is null)
        {
            throw new ArgumentNullException(nameof(font));
        }

        if (string.IsNullOrEmpty(text))
        {
            return Vector2.Zero;
        }

        string formattedText = FormatStringForRendering(text, tabSpaceCount);

        if (string.IsNullOrEmpty(formattedText))
        {
            return Vector2.Zero;
        }

        Vector2 result = Vector2.Zero;

        if (wordWrapWidth is not null)
        {
            formattedText = WordWrap(formattedText, font, wordWrapWidth.Value);
        }

        string[] lines = formattedText.GetLines();
        float fontHeight = font.FontHeight.FastFloor();

        if (lines.Length == 0)
        {
            return result;
        }

        if (lineSpacing.EqualsEpsilon(1.0f))
        {
            result.Y = lines.Length * fontHeight;
        }
        else
        {
            // For a modified line spacing, we have to adjust for the last line not being affected by the line spacing.
            result.Y = ((lines.Length - 1) * (fontHeight * lineSpacing)) + fontHeight;
        }

        if ((font.HasOutline) && (useOutline))
        {
            result.Y += font.OutlineSize * 0.5f;
        }

        // Get width.
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < lines.Length; ++i)
        {
            float lineWidth = GetLineWidth(font, lines[i], useOutline);

            if ((font.HasOutline) && (useOutline))
            {
                lineWidth += font.OutlineSize;
            }

            result.X = result.X.Max(lineWidth);
        }

        return result;
    }
}
