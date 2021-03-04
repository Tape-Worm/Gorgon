#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, August 25, 2011 7:44:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Gorgon.Core
{
    /// <summary>
    /// Extension methods to provide formatting on the <see cref="string"/> type and additional functionality for the <see cref="StringBuilder"/> type.
    /// </summary>
    public static class GorgonStringFormattingExtension
    {
        /// <summary>
        /// Function to break a string into an array of strings based on the newline control characters present in the text.
        /// </summary>
        /// <param name="text">The text to evaluate.</param>
        /// <param name="buffer">The array of strings representing a single line per newline control character.</param>
        public static void GetLines(this StringBuilder text, ref string[] buffer)
        {
            int lineCount = 0;

            if (text.Length == 0)
            {
                buffer = Array.Empty<string>();
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

            if ((buffer is null) || (buffer.Length != lineCount))
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
                    buffer[line] = "\n";
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

        /// <summary>
        /// Function to break a string into an array of strings based on the newline control characters present in the text.
        /// </summary>
        /// <param name="renderText">The text to evaluate.</param>
        /// <returns>The array of strings representing a single line per newline control character.</returns>
        public static string[] GetLines(this string renderText) => string.IsNullOrEmpty(renderText) ? Array.Empty<string>() : renderText.Split('\n');

        /// <summary>
        /// Function to find the index of a character in a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="theString">The string to search.</param>
        /// <param name="character">Character to search for.</param>
        /// <returns>The index of the character, or -1 if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="theString"/> parameter is <b>null</b>.</exception>
        public static int IndexOf(this StringBuilder theString, char character)
        {
            if (theString is null)
            {
                throw new ArgumentNullException(nameof(theString));
            }

            if (theString.Length < 1)
            {
                return -1;
            }

            for (int i = 0; i < theString.Length; i++)
            {
                if (theString[i] == character)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Function to find the index of a character in a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="theString">The string to search.</param>
        /// <param name="characters">Characters to search for.</param>
        /// <param name="startIndex">[Optional] The index to start searching from.</param>
        /// <param name="comparison">[Optional] One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The index of the character, or -1 if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="theString"/> parameter is <b>null</b>.</exception>
        public static int IndexOf(this StringBuilder theString, string characters, int startIndex = 0, StringComparison comparison = StringComparison.InvariantCulture)
        {
            if (theString is null)
            {
                throw new ArgumentNullException(nameof(theString));
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if (theString.Length < 1)
            {
                return -1;
            }

            return string.IsNullOrWhiteSpace(characters)
                ? -1
                : characters.Length > theString.Length ? -1 : theString.ToString().IndexOf(characters, startIndex, comparison);
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to find the last index of a character in a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="theString">The string to search.</param>
        /// <param name="character">Character to search for.</param>
        /// <returns>The index of the character, or -1 if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="theString"/> parameter is <b>null</b>.</exception>
        public static int LastIndexOf(this StringBuilder theString, char character)
        {
            if (theString is null)
            {
                throw new ArgumentNullException(nameof(theString));
            }

            if (theString.Length < 1)
            {
                return -1;
            }

            for (int i = theString.Length - 1; i >= 0; i--)
            {
                if (theString[i] == character)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Function to find the last index of a character in a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="theString">The string to search.</param>
        /// <param name="characters">Characters to search for.</param>
        /// <param name="comparison">[Optional] One of the enumeration values that specifies the rules for the search.</param>
        /// <returns>The index of the character, or -1 if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="theString"/> parameter is <b>null</b>.</exception>
        public static int LastIndexOf(this StringBuilder theString, string characters, StringComparison comparison = StringComparison.InvariantCulture)
        {
            if (theString is null)
            {
                throw new ArgumentNullException(nameof(theString));
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if ((theString.Length < 1) || (string.IsNullOrWhiteSpace(characters)))
            {
                return -1;
            }

            return characters.Length > theString.Length ? -1 : theString.ToString().LastIndexOf(characters, comparison);
#pragma warning restore IDE0046 // Convert to conditional expression
        }

        /// <summary>
        /// Function to convert a Linq to XML document into a string including a declaration element.
        /// </summary>
        /// <param name="document">The document to convert.</param>
        /// <returns>The XML document serialized as a string.</returns>
        /// <remarks>
        /// This method addresses a shortcoming of the Linq-to-XML <see cref="XDocument"/>.<see cref="XNode.ToString()"/> method. The original method leaves out the declaration element when converted to a string.
        /// </remarks>
        public static string ToStringWithDeclaration(this XDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var serializedXML = new StringBuilder();

            using (TextWriter writer = new StringWriter(serializedXML))
            {
                document.Save(writer);
            }

            return serializedXML.ToString();
        }

        /// <summary>
        /// Function to shorten a string and prefix an ellipses to the <see cref="string"/>.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="values">Values to put into the string placeholders.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>
        /// <para>
        /// This overload will output a shorted version of <paramref name="theString"/> and will prefix an ellipses '...' to it. 
        /// </para>
        /// <para>
        /// This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing 
        /// and will replace variable values. This way it will get the true length of the string.
        /// </para>
        /// <para>
        /// If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.
        /// </para>
        /// <para>
        /// If the <paramref name="maxWidth"/> is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.
        /// </para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, params object[] values) => Ellipses(theString, maxWidth, false, 4, values);

        /// <summary>
        /// Function to shorten a string and prefix or postfix an ellipses to the <see cref="string"/>.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="prefix"><b>true</b> to put the ellipses on the beginning of the string, or <b>false</b> to put on the end.</param>
        /// <param name="values">Values to put into the string placeholders.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>
        /// <para>
        /// This will output a shorted version of <paramref name="theString"/> and will prefix or postfix an ellipses '...' to it. 
        /// </para>
        /// <para>
        /// This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing 
        /// and will replace variable values. This way it will get the true length of the string.
        /// </para>
        /// <para>
        /// If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.
        /// </para>
        /// <para>
        /// If the <paramref name="maxWidth"/> is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.
        /// </para>
        /// <para>
        /// Specifying <b>true</b> for <paramref name="prefix"/> will put the ellipses on the beginning of the string, <b>false</b> will put it on the end as a suffix.
        /// </para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, bool prefix, params object[] values) => Ellipses(theString, maxWidth, prefix, 4, values);


        /// <summary>
        /// Function to shorten a string and prefix or postfix an ellipses to the <see cref="string"/>.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="prefix"><b>true</b> to put the ellipses on the beginning of the string, or <b>false</b> to put on the end.</param>
        /// <param name="tabSpaceCount">Number of spaces to insert for the tab character.</param>
        /// <param name="values">Values to put into the string placeholders.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>
        /// <para>
        /// This will output a shorted version of <paramref name="theString"/> and will prefix or postfix an ellipses '...' to it. 
        /// </para>
        /// <para>
        /// This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing 
        /// and will replace variable values. This way it will get the true length of the string.
        /// </para>
        /// <para>
        /// If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.
        /// </para>
        /// <para>
        /// If the <paramref name="maxWidth"/> is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.
        /// </para>
        /// <para>
        /// Specifying <b>true</b> for <paramref name="prefix"/> will put the ellipses on the beginning of the string, <b>false</b> will put it on the end as a suffix.
        /// </para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, bool prefix, int tabSpaceCount, params object[] values)
        {
            string result = string.Empty;
            string tabSpaces = string.Empty;
            const string ellipses = "...";

            if (string.IsNullOrEmpty(theString))
            {
                return string.Empty;
            }

            if (maxWidth < 1)
            {
                return string.Empty;
            }

            // Set up the tabs.
            if (tabSpaceCount > 0)
            {
                tabSpaces = new string(' ', tabSpaceCount);
            }

            // Format the string.
            if ((values is not null) && (values.Length > 0))
            {
                theString = string.Format(theString, values);
            }
            theString = theString.Replace("\t", tabSpaces);

            // Split into multiple lines.            
            IList<string> lines = theString.Split('\n');

            // Process the string.
            for (int i = 0; i < lines.Count; i++)
            {
                // Wrap to a new line.
                if (result.Length > 0)
                {
                    result += "\n";
                }

                if (lines[i].Length >= maxWidth)
                {
                    int overrun = lines[i].Length - maxWidth;
                    int difference = lines[i].Length - overrun;

                    // If the resulting string length is greater than that of our line, then just output the line.
                    if (!prefix)
                    {
                        // If the ellipses is greater than the available space, then print the first characters + the ellipses,
                        // else only print the first characters.
                        if (difference > ellipses.Length)
                        {
                            lines[i] = lines[i].Substring(0, difference - ellipses.Length) + ellipses;
                        }
                        else
                        {
                            lines[i] = lines[i].Substring(0, maxWidth);
                        }
                    }
                    else
                    {
                        // If the ellipses is greater than the available space, then print the last characters + the ellipses,
                        // else only print the last characters.
                        if (difference > ellipses.Length)
                        {
                            lines[i] = ellipses + lines[i].Substring(overrun + ellipses.Length, difference - ellipses.Length);
                        }
                        else
                        {
                            lines[i] = lines[i][^difference..];
                        }
                    }
                }

                result += lines[i];
            }

            return result;
        }

        /// <summary>
        /// Function to return the length of a <see cref="string"/>, in bytes, with the specified <see cref="Encoding"/>.
        /// </summary>
        /// <param name="value">The string to measure.</param>
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
        public static int GetByteCount(this string value, bool includeLength, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            if (encoding is null)
            {
                encoding = Encoding.UTF8;
            }

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

        /// <summary>
        /// Function to determine if the string is composed of whitespace, or is empty.
        /// </summary>
        /// <param name="value">The string to examine.</param>
        /// <returns><b>true</b> if the string is composed of whitespace, or is empty. <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="value"/> parameter is <b>null</b>.</exception>
	    public static bool IsWhiteSpaceOrEmpty(this string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            for (int i = 0; i < value.Length; ++i)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
