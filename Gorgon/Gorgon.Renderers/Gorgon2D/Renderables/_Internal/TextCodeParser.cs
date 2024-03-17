
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: June 12, 2018 12:37:25 PM
// 


using System.Globalization;
using System.Text;
using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Renderers;

/// <summary>
/// A parser used to analyze text with embedded codes
/// </summary>
internal class TextCodeParser
{
    // Buffer used to parse the string.
    private readonly StringBuilder _colorBuffer = new(16);
    // Buffer used to parse the string.
    private readonly StringBuilder _parseBuffer = new(256);

    /// <summary>
    /// Function to retrieve the color value from a hex code.
    /// </summary>
    /// <param name="startIndex">The starting index.</param>
    /// <param name="count">The number of characters.</param>
    /// <returns>The color value for the hex code </returns>
    private GorgonColor? GetColor(int startIndex, int count)
    {
        _colorBuffer.Length = 0;
        _colorBuffer.Append(_parseBuffer.ToString(startIndex, count));
        _colorBuffer.Replace(" ", string.Empty);

        if (_colorBuffer[0] != '#')
        {
            return null;
        }

        _colorBuffer.Replace("#", string.Empty);

        return !int.TryParse(_colorBuffer.ToString(), NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out int color)
            ? null
            : new GorgonColor(color);
    }

    /// <summary>
    /// Function to find the index of the control characters in a string builder.
    /// </summary>
    /// <param name="line">The line containing the text to evaluate.</param>
    /// <param name="value">The string value to look for.</param>
    /// <param name="startIndex">The starting index within the line to start searching from.</param>
    /// <returns>The index of the control character, or -1 if not found.</returns>
    private int IndexOfControlChars(StringBuilder line, string value, int startIndex)
    {
        if (line.Length < value.Length)
        {
            return -1;
        }

        int charCount = 0;

        for (int i = 0; i < value.Length; ++i)
        {
            char c = value[i];

            for (int j = startIndex; j < line.Length; ++j)
            {
                if (char.ToUpperInvariant(c) != char.ToUpperInvariant(line[j]))
                {
                    continue;
                }

                startIndex = j;
                ++charCount;
                break;
            }

            if (charCount == value.Length)
            {
                return (startIndex - charCount) + 1;
            }
        }

        return -1;
    }

    /// <summary>
    /// Function to parse the text.
    /// </summary>
    /// <param name="encodedText">The encoded text to parse.</param>
    /// <returns>A tuple containing the decoded text, and a list of color code blocks in the text.</returns>
    public (string decodedText, List<ColorBlock> colorBlocks) ParseColorCodes(string encodedText)
    {
        if (string.IsNullOrEmpty(encodedText))
        {
            return (encodedText, new List<ColorBlock>());
        }

        List<ColorBlock> blocks = [];
        int startTagIndex;
        _parseBuffer.Length = 0;
        _parseBuffer.Append(encodedText);

        do
        {               
            startTagIndex = IndexOfControlChars(_parseBuffer, "[c", 0);

            int endTagIndex;
            if (startTagIndex != -1)
            {
                endTagIndex = IndexOfControlChars(_parseBuffer, "]", startTagIndex);
            }
            else
            {
                break;
            }

            int closeTagIndex;
            if (endTagIndex != -1)
            {
                closeTagIndex = IndexOfControlChars(_parseBuffer, "[/c]", endTagIndex);
            }
            else
            {
                break;
            }

            if (closeTagIndex == -1)
            {
                break;
            }

            int tagValueIndex = startTagIndex + 2;
            int tagLength = endTagIndex - tagValueIndex;
            int startText = endTagIndex + 1;
            int endText = closeTagIndex - 1;
            int count = (endText + 1) - startText;

            if (count > 0)
            {
                GorgonColor? color = GetColor(tagValueIndex, tagLength);

                if (color is not null)
                {
                    blocks.Add(new ColorBlock(startText - (tagLength + 3), endText - (tagLength + 3), color.Value));
                }
            }

            _parseBuffer.Remove(closeTagIndex, 4);
            _parseBuffer.Remove(startTagIndex, startText - startTagIndex);
        } while (startTagIndex != -1);

        return (_parseBuffer.ToString(), blocks);
    }
}
