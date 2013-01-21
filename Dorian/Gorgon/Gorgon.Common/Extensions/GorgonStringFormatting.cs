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
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary
{
	/// <summary>
	/// Various path formatting methods.
	/// </summary>
	public static class GorgonStringFormattingExtension
	{
        /// <summary>
        /// Function to shorten a string and prefix an ellipses to the string.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>This overload will output a shorted version of <paramref name="theString"/> and will prefix an ellipses '...' to it. 
        /// <para>This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing. 
        /// This way it will get the true length of the string.</para>
        /// <para>If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.</para>
        /// <para>If the maxwidth is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.</para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth)
        {
            return Ellipses(theString, maxWidth);
        }

        /// <summary>
        /// Function to shorten a string and prefix an ellipses to the string.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="values">Values to put into the string placeholders.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>This overload will output a shorted version of <paramref name="theString"/> and will prefix an ellipses '...' to it. 
        /// <para>This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing 
        /// and will replace variable values. This way it will get the true length of the string.</para>
        /// <para>If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.</para>
        /// <para>If the maxwidth is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.</para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, params object[] values)
        {
            return Ellipses(theString, maxWidth, false, 4, values);
        }

        /// <summary>
        /// Function to shorten a string and prefix or postfix an ellipses to the string.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="prefix">TRUE to put the ellipses on the beginning of the string, or FALSE to put on the end.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>This will output a shorted version of <paramref name="theString"/> and will prefix or postfix an ellipses '...' to it. 
        /// <para>This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing. 
        /// This way it will get the true length of the string.</para>
        /// <para>If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.</para>
        /// <para>If the maxwidth is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.</para>
        /// <para>Specifying TRUE for <paramref name="prefix"/> will put the ellipses on the beginning of the string, FALSE will put it on the end.</para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, bool prefix)
        {
            return Ellipses(theString, maxWidth, prefix, 4, null);
        }

        /// <summary>
        /// Function to shorten a string and prefix or postfix an ellipses to the string.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="prefix">TRUE to put the ellipses on the beginning of the string, or FALSE to put on the end.</param>
        /// <param name="values">Values to put into the string placeholders.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>This will output a shorted version of <paramref name="theString"/> and will prefix or postfix an ellipses '...' to it. 
        /// <para>This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing 
        /// and will replace variable values. This way it will get the true length of the string.</para>
        /// <para>If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.</para>
        /// <para>If the maxwidth is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.</para>
        /// <para>Specifying TRUE for <paramref name="prefix"/> will put the ellipses on the beginning of the string, FALSE will put it on the end.</para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, bool prefix, params object[] values)
        {
            return Ellipses(theString, maxWidth, prefix, 4, values);
        }

        /// <summary>
        /// Function to shorten a string and prefix or postfix an ellipses to the string.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="prefix">TRUE to put the ellipses on the beginning of the string, or FALSE to put on the end.</param>
        /// <param name="tabSpaceCount">Number of spaces to insert for the tab character.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>This will output a shorted version of <paramref name="theString"/> and will prefix or postfix an ellipses '...' to it. 
        /// <para>This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing.  
        /// This way it will get the true length of the string.</para>
        /// <para>If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.</para>
        /// <para>If the maxwidth is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.</para>
        /// <para>Specifying TRUE for <paramref name="prefix"/> will put the ellipses on the beginning of the string, FALSE will put it on the end.</para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, bool prefix, int tabSpaceCount)
        {
            return Ellipses(theString, maxWidth, prefix, tabSpaceCount, null);
        }

        /// <summary>
        /// Function to shorten a string and prefix or postfix an ellipses to the string.
        /// </summary>
        /// <param name="theString">The string to shorten.</param>
        /// <param name="maxWidth">The maximum width, in characters, of the string.</param>
        /// <param name="prefix">TRUE to put the ellipses on the beginning of the string, or FALSE to put on the end.</param>
        /// <param name="tabSpaceCount">Number of spaces to insert for the tab character.</param>
        /// <param name="values">Values to put into the string placeholders.</param>
        /// <returns>The shortened string with ellipses.</returns>
        /// <remarks>This will output a shorted version of <paramref name="theString"/> and will prefix or postfix an ellipses '...' to it. 
        /// <para>This function will do formatting on the string, such as tab replacement and split the newline characters into new lines before processing 
        /// and will replace variable values. This way it will get the true length of the string.</para>
        /// <para>If the <paramref name='maxWidth'/> is less than the length of a line, then the ellipses will be added to the string, and the string will 
        /// be truncated to the max width plus the length of the ellipses, otherwise it will just output the line.</para>
        /// <para>If the maxwidth is less than the length of the string plus the ellipses length, then just the first few characters (up to the width 
        /// specified by max width) will be output without ellipses.</para>
        /// <para>Specifying TRUE for <paramref name="prefix"/> will put the ellipses on the beginning of the string, FALSE will put it on the end.</para>
        /// </remarks>        
        public static string Ellipses(this string theString, int maxWidth, bool prefix, int tabSpaceCount, params object[] values)
        {
            string result = string.Empty;
            IList<string> lines = null;
            string tabSpaces = string.Empty;
            string ellipses = "...";

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
            if ((values != null) && (values.Length > 0))
            {
                theString = string.Format(theString, values);
            }
            theString = theString.Replace("\t", tabSpaces);

            // Split into multiple lines.            
            lines = theString.Split(new char[] { '\n' });

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
                    string shortenedLine = string.Empty;
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
                            lines[i] = lines[i].Substring(lines[i].Length - difference);
                        }
                    }
                }

                result += lines[i];
            }

            return result;
        }

        /// <summary>
        /// Initializes the <see cref="GorgonStringFormattingExtension"/> class.
        /// </summary>
        static GorgonStringFormattingExtension()
		{
			
		}
	}
}
