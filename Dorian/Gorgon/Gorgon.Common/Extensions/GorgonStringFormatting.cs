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
using System.IO;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary
{
	/// <summary>
	/// Various path formatting methods.
	/// </summary>
	public static class GorgonStringFormattingExtension
	{
		private static List<string> _pathParts = null;			// Parts for a path.

		/// <summary>
		/// Function to determine if this path is valid.
		/// </summary>
		/// <param name="path">Path to a file or directory.</param>
		/// <returns>TRUE if valid, FALSE if not.</returns>
		public static bool IsValidPath(this string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;

			int lastIndexOfSep = -1;			
			string fileName = string.Empty;
			string directory = string.Empty;			

			var illegalChars = Path.GetInvalidFileNameChars();

			lastIndexOfSep = path.LastIndexOf(Path.DirectorySeparatorChar.ToString());

			if (lastIndexOfSep == -1)
				lastIndexOfSep = path.LastIndexOf(Path.AltDirectorySeparatorChar.ToString());

			if (lastIndexOfSep == -1)
				fileName = path;
			else
			{
				directory = path.Substring(0, lastIndexOfSep);
				if (lastIndexOfSep < path.Length - 1)
					fileName = path.Substring(lastIndexOfSep + 1);
			}

			_pathParts.Clear();
			if (!string.IsNullOrEmpty(directory))
			{
				directory = directory.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
				_pathParts.AddRange(directory.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries));
			}

			if (!string.IsNullOrEmpty(fileName))
				_pathParts.Add(fileName);

			foreach (var part in _pathParts)
			{
				if (illegalChars.Any(item => part.Contains(item)))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Function to return a properly file name.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The formatted path to the file.</returns>
		public static string FormatFileName(this string path)
		{
			string filename = string.Empty;

			if (string.IsNullOrEmpty(path))
				return string.Empty;

			filename = RemoveIllegalFilenameChars(Path.GetFileName(path));

			return filename;
		}

		/// <summary>
		/// Function to return a properly formatted directory name.
		/// </summary>
		/// <param name="path">Path to repair.</param>
		/// <param name="directorySeparator">Directory separator character to use.</param>
		/// <returns>The formatted path.</returns>
		/// <remarks>When the <paramref name="directorySeparator"/> character is whitespace or illegal, then the system will use the <see cref="F:System.IO.Path.DirectorySeparatorChar"/> character.</remarks>
		public static string FormatDirectory(this string path, char directorySeparator)
		{
			char[] illegalChars = Path.GetInvalidPathChars();
			string doubleSeparator = directorySeparator.ToString() + directorySeparator.ToString();

			if (string.IsNullOrEmpty(path))
				return string.Empty;

			if ((char.IsWhiteSpace(directorySeparator)) || (illegalChars.Contains(directorySeparator)))
				directorySeparator = Path.DirectorySeparatorChar;

			path = RemoveIllegalPathChars(path);

			StringBuilder output = new StringBuilder(path);

			if (directorySeparator != Path.AltDirectorySeparatorChar)
				output = output.Replace(Path.AltDirectorySeparatorChar, directorySeparator);
			if (directorySeparator != Path.DirectorySeparatorChar)
				output = output.Replace(Path.DirectorySeparatorChar, directorySeparator);
			if (output[output.Length - 1] != directorySeparator)
				output.Append(directorySeparator);

			// Remove doubled up separators.
			while (output.ToString().LastIndexOf(doubleSeparator) > -1)
			{
				output = output.Replace(doubleSeparator, directorySeparator.ToString());
			}

			return output.ToString();
		}

		/// <summary>
		/// Function to remove any illegal path characters from a path.
		/// </summary>
		/// <param name="path">Path to fix.</param>
		/// <returns>The corrected path.</returns>
		/// <remarks>This will replace any illegal characters with the '_' symbol.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> is NULL (or Nothing in VB.NET).</exception>
		public static string RemoveIllegalPathChars(this string path)
		{
			char[] illegalChars = Path.GetInvalidPathChars();

			if (path == null)
				throw new ArgumentNullException("path");

			if (path.Length == 0)
				return string.Empty;

			StringBuilder output = new StringBuilder(path);

			foreach (char illegalChar in illegalChars)
				output = output.Replace(illegalChar, '_');

			return output.ToString();
		}

		/// <summary>
		/// Function to remove any illegal file name characters from a path.
		/// </summary>
		/// <param name="path">Path to fix.</param>
		/// <returns>The corrected file name.</returns>
		/// <remarks>This will replace any illegal characters with the '_' symbol.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> is NULL (or Nothing in VB.NET).</exception>
		public static string RemoveIllegalFilenameChars(this string path)
		{
			char[] illegalChars = Path.GetInvalidFileNameChars();

			if (path == null)
				throw new ArgumentNullException("path");

			if (path.Length == 0)
				return string.Empty;

			StringBuilder filePath = new StringBuilder(FormatDirectory(Path.GetDirectoryName(path), Path.DirectorySeparatorChar));
			StringBuilder output = new StringBuilder(Path.GetFileName(path));

			foreach (char illegalChar in illegalChars)
				output = output.Replace(illegalChar, '_');

			filePath.Append(output);

			return filePath.ToString();
		}

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
			_pathParts = new List<string>();
		}
	}
}
