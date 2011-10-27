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
	}
}
