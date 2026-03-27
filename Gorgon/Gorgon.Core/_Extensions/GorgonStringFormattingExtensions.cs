// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: November 17, 2023 5:09:21 PM
//

using System.Xml.Linq;
using Gorgon.Collections;

namespace Gorgon.Core;

/// <summary>
/// Extension methods to provide formatting on the <see cref="string"/> type.
/// </summary>
public static class GorgonStringFormattingExtension
{
    extension(string text)
    {
        /// <summary>
        /// Function to provide an enumerator that allows the splitting of a string using the specified separators.
        /// </summary>
        /// <param name="separators">The separators to use.</param>
        /// <param name="includeBlanks">[Optional] <b>true</b> to keep empty entries, <b>false</b> to skip.</param>
        /// <returns>The <see cref="GorgonSpanCharEnumerator"/> that will enumerate the string.</returns>
        /// <remarks>
        /// <para>
        /// This provides an enumerator that does not perform any allocations, which will deliver a more performant way to split strings.
        /// </para>
        /// </remarks>
        public GorgonSpanCharEnumerator GetSplitEnumerator(ReadOnlySpan<char> separators, bool includeBlanks = false) => new(text.AsSpan(), separators, includeBlanks, false);

        /// <summary>
        /// Function to provide an enumerator that allows the splitting of a string using the specified separators.
        /// </summary>
        /// <param name="separators">The separators to use.</param>
        /// <param name="includeBlanks">[Optional] <b>true</b> to keep empty entries, <b>false</b> to skip.</param>
        /// <returns>The <see cref="GorgonSpanCharEnumerator"/> that will enumerate the string.</returns>
        /// <remarks>
        /// <para>
        /// This provides an enumerator that does not perform any allocations, which will deliver a more performant way to split strings.
        /// </para>
        /// <para>
        /// This method provides an iterator that walks through the span in reverse.
        /// </para>
        /// </remarks>
        public GorgonSpanCharEnumerator GetReverseSplitEnumerator(ReadOnlySpan<char> separators, bool includeBlanks = false) => new(text.AsSpan(), separators, includeBlanks, true);

        /// <summary>
        /// Function to break a string into an array of strings based on the newline control characters present in the text.
        /// </summary>
        /// <returns>The array of strings representing a single line per newline control character.</returns>
        public string[] GetLines() => string.IsNullOrEmpty(text) ? [] : text.Split('\n');
    }

    extension(StringBuilder text)
    {
        /// <summary>
        /// Function to break a string into an array of strings based on the newline control characters present in the text.
        /// </summary>
        /// <param name="buffer">The array of strings representing a single line per newline control character.</param>
        public void GetLines(ref string[] buffer)
        {
            int lineCount = 0;

            if (text.Length == 0)
            {
                buffer = [];
                return;
            }

            int startChar = 0;
            int charCount = 0;

            // Find out how many lines we have.
            for (int i = 0; i < text.Length; ++i)
            {
                char character = text[i];

                if (character == '\n')
                {
                    ++lineCount;
                }
            }

            // We'll always have at least 1 line.
            ++lineCount;

            if (buffer.Length != lineCount)
            {
                buffer = new string[lineCount];
            }

            int line = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                char character = text[i];

                if (character != '\n')
                {
                    ++charCount;
                    continue;
                }

                if (charCount == 0)
                {
                    buffer[line] = string.Empty;
                    ++startChar;
                }
                else
                {
                    buffer[line] = text.ToString(startChar, charCount);
                    startChar += charCount + 1;
                }

                ++line;
                charCount = 0;
            }

            if (line == lineCount)
            {
                return;
            }

            // Get last line.
            buffer[line] = text.ToString(startChar, charCount);
        }
    }

    extension(XDocument document)
    {
        /// <summary>
        /// Function to convert a Linq to XML document into a string including a declaration element.
        /// </summary>
        /// <returns>The XML document serialized as a string.</returns>
        /// <remarks>
        /// <para>
        /// This method addresses a shortcoming of the Linq-to-XML <see cref="XDocument"/>.<see cref="XNode.ToString()"/> method. The original method leaves out the declaration element when converted to a string.
        /// </para>
        /// </remarks>
        public string ToStringWithDeclaration()
        {
            StringBuilder serializedXML = new();

            if (document.Declaration is not null)
            {
                serializedXML.Append(document.Declaration.ToString());
                serializedXML.Append("\r\n");
            }
            else
            {
                serializedXML.Append("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>\r\n");
            }

            serializedXML.Append(document.ToString());

            return serializedXML.ToString();
        }
    }

    extension(string theString)
    {
        /// <summary>
        /// Function to shorten a string and prefix or postfix an ellipses to the <see cref="string"/>.
        /// </summary>
        /// <param name="maxLength">The maximum length of the string before adding ellipses.</param>
        /// <param name="prefix"><b>true</b> to put the ellipses on the beginning of the string, or <b>false</b> to put on the end.</param>
        /// <returns>The shortened string with ellipses if the string exceeds the <paramref name="maxLength"/> value. Otherwise, the original string is returned.</returns>
        /// <remarks>
        /// <para>
        /// This will output a shorted version of the string and will prefix or postfix an ellipses '...' to it. 
        /// </para>
        /// <para>
        /// If the <paramref name='maxLength'/> is less than the length of the string, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max length, and ellipses will be appended; otherwise it will just output the string as is.
        /// </para>
        /// <para>
        /// Specifying <b>true</b> for <paramref name="prefix"/> will put the ellipses on the beginning of the string, <b>false</b> will put it on the end as a suffix.
        /// </para>
        /// </remarks>        
        public string Ellipses(int maxLength, bool prefix = false)
        {
            const string ellipses = "...";

            if (string.IsNullOrEmpty(theString))
            {
                return string.Empty;
            }

            if (maxLength < 1)
            {
                return string.Empty;
            }

            if (theString.Length <= maxLength)
            {
                return theString;
            }

            StringBuilder sb = new(theString);

            if (!prefix)
            {
                int newSize = maxLength;
                sb.Length = newSize;
                sb.Append(ellipses);

                return sb.ToString();
            }

            sb.Remove(0, sb.Length - maxLength);
            sb.Insert(0, ellipses);

            return sb.ToString();
        }
    }

    extension(ReadOnlySpan<char> value)
    {
        /// <summary>
        /// Function to return the length of a <see cref="ReadOnlySpan{T}"/> of <see cref="char"/>, in bytes, with the specified <see cref="Encoding"/>.
        /// </summary>
        /// <param name="includeLength"><b>true</b> to include the number of bytes for the encoded length, <b>false</b> to exclude.</param>
        /// <param name="encoding">[Optional] The encoding for the string.</param>
        /// <returns>The length of the string, in bytes.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="includeLength"/> parameter is <b>true</b>, then the return value will also include the number of 7-bit bytes required to encode the length of the string.
        /// </para>
        /// <para>
        /// If the <paramref name="encoding"/> parameter is <b>null</b>, then UTF-8 encoding will be used.
        /// </para>
        /// </remarks>
        public int GetByteCount(bool includeLength, Encoding? encoding = null)
        {
            if (value.IsEmpty)
            {
                return 0;
            }

            encoding ??= Encoding.UTF8;
            int size = encoding.GetByteCount(value);
            int result = size;

            if (!includeLength)
            {
                return result;
            }

            result++;

            while (size >= 0x80)
            {
                size >>= 7;
                result++;
            }

            return result;
        }
    }

    extension(string value)
    {
        /// <summary>
        /// Function to return the length of a <see cref="string"/>, in bytes, with the specified <see cref="Encoding"/>.
        /// </summary>
        /// <param name="includeLength"><b>true</b> to include the number of bytes for the encoded length, <b>false</b> to exclude.</param>
        /// <param name="encoding">[Optional] The encoding for the string.</param>
        /// <returns>The length of the string, in bytes.</returns>
        /// <remarks>
        /// <para>
        /// If the <paramref name="includeLength"/> parameter is <b>true</b>, then the return value will also include the number of 7-bit bytes required to encode the length of the string.
        /// </para>
        /// <para>
        /// If the <paramref name="encoding"/> parameter is <b>null</b>, then UTF-8 encoding will be used.
        /// </para>
        /// </remarks>
        public int GetByteCount(bool includeLength, Encoding? encoding = null) => GetByteCount(value.AsSpan(), includeLength, encoding);
    }
}
