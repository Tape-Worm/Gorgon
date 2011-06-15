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
// Created: Tuesday, June 14, 2011 8:52:39 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// A utility class.
	/// </summary>
	public static class GorgonUtility
	{
		#region Methods.
		/// <summary>
		/// Function to throw an exception if a string is null or empty.
		/// </summary>
		/// <param name="value">The value being passed.</param>
		/// <param name="paramName">The name of the parameter.</param>
		/// <remarks>This will only throw exceptions when we're in DEBUG mode.  Release mode will do nothing.</remarks>
		public static void AssertParamString(string value, string paramName)
		{
#if DEBUG
			if (value == null)
				throw new ArgumentNullException(paramName);
			if (value == string.Empty)
				throw new ArgumentException("The parameter must not be a zero-length string.", paramName);
#endif
		}

		/// <summary>
		/// Function to return a properly formatted path name including a file name.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The formatted path to the file.</returns>
		public static string FormatFileName(string path)
		{
			string directory = string.Empty;
			string filename = string.Empty;

			if (string.IsNullOrEmpty(path))
				return string.Empty;

			directory = Path.GetDirectoryName(path);

			filename = Path.GetFileName(path);

			return filename;
		}

		/// <summary>
		/// Function to return a properly formatted path name.
		/// </summary>
		/// <param name="path">Path to repair.</param>
		/// <param name="directorySeparator">Directory separator character to use.</param>
		/// <returns>The formatted path.</returns>
		/// <remarks>When the <paramref name="directorySeparator"/> character is whitespace or illegal, then the system will use the <see cref="F:System.IO.Path.DirectorySeparatorChar"/> character.</remarks>
		public static string FormatDirectory(string path, char directorySeparator)
		{
			char[] illegalChars = Path.GetInvalidPathChars();

			if (string.IsNullOrEmpty(path))
				return string.Empty;

			if ((char.IsWhiteSpace(directorySeparator)) || (illegalChars.Contains(directorySeparator)))
				directorySeparator = Path.DirectorySeparatorChar;

			RemoveIllegalPathChars(path);

			StringBuilder output = new StringBuilder(path);

			if (directorySeparator != Path.AltDirectorySeparatorChar)
				output = output.Replace(Path.AltDirectorySeparatorChar, directorySeparator);
			if (directorySeparator != Path.DirectorySeparatorChar)
				output = output.Replace(Path.DirectorySeparatorChar, directorySeparator);
			if (output[output.Length - 1] != directorySeparator)
				output.Append(directorySeparator);

			return output.ToString();
		}

		/// <summary>
		/// Function to remove any illegal path characters from a path.
		/// </summary>
		/// <param name="path">Path to fix.</param>
		/// <returns>The corrected path.</returns>
		/// <remarks>This will replace any illegal characters with the '_' symbol.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> is NULL (or Nothing in VB.NET).</exception>
		public static string RemoveIllegalPathChars(string path)
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
		/// Function to remove any illegal filename characters from a path.
		/// </summary>
		/// <param name="path">Path to fix.</param>
		/// <returns>The corrected file name.</returns>
		/// <remarks>This will replace any illegal characters with the '_' symbol.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="path"/> is NULL (or Nothing in VB.NET).</exception>
		public static string RemoveIllegalFilenameChars(string path)
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
		/// Function to return the path to the per-user roaming directory for a given application.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <returns>The per-user roaming data directory.</returns>
		/// <remarks>The function will trim leading and trailing spaces.  If the parameter only contains whitespace, it will be treated as an empty string.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the parameter is an empty string.</exception>
		public static string GetUserApplicationPath(string applicationName)
		{
			StringBuilder outputDir = new StringBuilder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

			AssertParamString(applicationName, "applicationName");

			if (outputDir[outputDir.Length - 1] != Path.DirectorySeparatorChar)
				outputDir.Append(Path.DirectorySeparatorChar);

			outputDir.Append(applicationName);

			return FormatDirectory(outputDir.ToString(), Path.DirectorySeparatorChar);
		}

		/// <summary>
		/// Function to return a formatted string containing the memory amount.
		/// </summary>
		/// <param name="amount">Amount of memory in bytes to format.</param>
		/// <returns>A string containing the formatted amount of memory.</returns>
		public static string FormatMemoryAmount(long amount)
		{
			double scale = amount;
			string result = string.Empty;

			if (amount < 0)
				return "Unknown.";

			scale = amount / 1125899906842624.0;

			if (scale > 1.0)
				return scale.ToString("0.0") + " PB";

			scale = amount / 1099511627776.0;

			if (scale > 1.0)
				return scale.ToString("0.0") + " TB";

			scale = amount / 1073741824.0;

			if (scale > 1.0)
				return scale.ToString("0.0") + " GB";

			scale = amount / 1048576.0;

			if (scale > 1.0)
				return scale.ToString("0.0") + " MB";

			scale = amount / 1024.0;

			if (scale > 1.0)
				return scale.ToString("0.0") + " KB";

			return amount.ToString() + " bytes";
		}

		/// <summary>
		/// Function to format a pointer (IntPtr) value into a hexadecimal string.
		/// </summary>
		/// <param name="pointer">Pointer to format.</param>
		/// <returns>The formatted address of the pointer.</returns>
		/// <remarks>This function will take into account whether the application is x64 or x86 and will format accordingly.</remarks>
		public static string FormatHex(IntPtr pointer)
		{
			if (Gorgon.PlatformArchitecture == PlatformArchitecture.x64)
				return pointer.ToInt64().ToString("x").PadLeft(16, '0');
			else
				return pointer.ToInt32().ToString("x").PadLeft(8, '0');
		}
		#endregion
	}
}
