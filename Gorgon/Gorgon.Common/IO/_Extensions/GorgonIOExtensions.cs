#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, January 21, 2013 9:03:04 AM
// 
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Core.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// String formatting extensions for IO operations.
    /// </summary>
    public static class GorgonIOExtensions
    {
        #region Variables.
        private static readonly string _directoryPathSeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
        private static readonly string _altPathSeparator = Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);

        private static readonly char[] _illegalPathChars = Path.GetInvalidPathChars();          // Illegal path characters.
        private static readonly char[] _illegalFileChars = Path.GetInvalidFileNameChars();      // Illegal file name characters.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to write a string into a stream.
        /// </summary>
        /// <param name="value">The string to write into the stream.</param>
        /// <param name="stream">Stream to encode the string into.</param>
        /// <remarks>This will encode the string as a series of bytes into a stream.  The length of the string will be prefixed to the 
        /// string as a series of 7 bit byte values.
        /// </remarks>
        /// <returns>The number of bytes written to the stream.</returns>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> parameter is read-only.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the stream parameter is NULL.</exception>
        public static int WriteToStream(this string value, Stream stream)
        {
            return WriteToStream(value, stream, null);
        }

        /// <summary>
        /// Function to encode a string into a stream with the specified encoding.
        /// </summary>
        /// <param name="value">The string to write into the stream.</param>
        /// <param name="stream">Stream to encode the string into.</param>
        /// <param name="encoding">Encoding for the string.</param>
        /// <remarks>This will encode the string as a series of bytes into a stream.  The length of the string will be prefixed to the 
        /// string as a series of 7 bit byte values.
        /// <para>If the <paramref name="encoding"/> parameter is NULL (Nothing in VB.Net), then UTF-8 encoding will be used.</para>
        /// </remarks>
        /// <returns>The number of bytes written to the stream.</returns>
        /// <exception cref="System.IO.IOException">Thrown when the <paramref name="stream"/> parameter is read-only.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when the stream parameter is NULL.</exception>
        public static int WriteToStream(this string value, Stream stream, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanWrite)
            {
                throw new IOException(Resources.GOR_STREAM_IS_READONLY);
            }

            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            byte[] stringData = encoding.GetBytes(value);
            int size = stringData.Length;
            int result = size + 1;

            // Build the 7 bit encoded length.
            while (size >= 0x80)
            {
                stream.WriteByte((byte)((size | 0x80) & 0xFF));
                size >>= 7;
                result++;
            }

            stream.WriteByte((byte)size);
            stream.Write(stringData, 0, stringData.Length);

            return result;
        }

        /// <summary>
        /// Function to write a string to a stream with the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to write the string into.</param>
        /// <param name="value">The string to write.</param>
        /// <param name="encoding">The encoding for the string.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL.</exception>
        /// <remarks>Gorgon stores its strings in a stream by prefixing the string data with the length of the string.  This length is encoded as 
        /// a series of 7-bit bytes.
        /// <para>If the <paramref name="encoding"/> parameter is NULL (Nothing in VB.Net), then UTF-8 encoding is used.</para>
        /// </remarks>
        public static void WriteString(this Stream stream, string value, Encoding encoding)
        {
            WriteToStream(value, stream, encoding);
        }

        /// <summary>
        /// Function to write a string to a stream with the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to write the string into.</param>
        /// <param name="value">The string to write.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL.</exception>
        /// <remarks>Gorgon stores its strings in a stream by prefixing the string data with the length of the string.  This length is encoded as 
        /// a series of 7-bit bytes.
        /// </remarks>
        public static void WriteString(this Stream stream, string value)
        {
            WriteToStream(value, stream, null);
        }

        /// <summary>
        /// Function to read a string from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the string from.</param>
        /// <returns>The string in the stream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL.</exception>
        /// <exception cref="System.IO.IOException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>Gorgon stores its strings in a stream by prefixing the string data with the length of the string.  This length is encoded as 
        /// a series of 7-bit bytes.
        /// </remarks>
        public static string ReadString(this Stream stream)
        {
            return ReadString(stream, null);
        }

        /// <summary>
        /// Function to read a string from a stream with the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to read the string from.</param>
        /// <param name="encoding">The encoding for the string.</param>
        /// <returns>The string in the stream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL.</exception>
        /// <exception cref="System.IO.IOException">Thrown when an attempt to read beyond the end of the stream is made.</exception>
        /// <remarks>Gorgon stores its strings in a stream by prefixing the string data with the length of the string.  This length is encoded as 
        /// a series of 7-bit bytes.
        /// <para>If the <paramref name="encoding"/> parameter is NULL (Nothing in VB.Net), then UTF-8 encoding is used.</para>
        /// </remarks>
        public static string ReadString(this Stream stream, Encoding encoding)
        {
            int stringLength = 0;

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

			if (encoding == null)
			{
				encoding = Encoding.UTF8;
			}

            // String length is encoded in a 7 bit integer.
            // We have to get each byte and shift it until there are no more high bits set, or the counter becomes larger than 32 bits.
            int counter = 0;
            while (true)
            {
                int value = stream.ReadByte();

                if (value == -1)
                {
                    throw new IOException(Resources.GOR_STREAM_EOS);
                }

                stringLength |= (value & 0x7F) << counter;
                counter += 7;
                if (((value & 0x80) == 0) || (counter > 32))
                    break;
            }

            if (stringLength == 0)
                return string.Empty;
            
            var byteData = new byte[stringLength];
            stream.Read(byteData, 0, byteData.Length);
            return encoding.GetString(byteData, 0, byteData.Length);
        }

        /// <summary>
        /// Function to return a properly file name.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>The formatted path to the file.</returns>
        public static string FormatFileName(this string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : RemoveIllegalFilenameChars(Path.GetFileName(path));
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
            string directorySep = _directoryPathSeparator;
            var doubleSeparator = new string(new [] { directorySeparator, directorySeparator});

            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            if ((char.IsWhiteSpace(directorySeparator)) || (_illegalPathChars.Contains(directorySeparator)))
            {
                directorySeparator = Path.DirectorySeparatorChar;
            }

            path = RemoveIllegalPathChars(path);

            var output = new StringBuilder(path);

            if (directorySeparator != Path.AltDirectorySeparatorChar)
            {
                output = output.Replace(Path.AltDirectorySeparatorChar, directorySeparator);
            }
            else
            {
                output = output.Replace(Path.DirectorySeparatorChar, directorySeparator);
                directorySep = _altPathSeparator;
            }

            if (output[output.Length - 1] != directorySeparator)
            {
                output.Append(directorySeparator);
            }

            // Remove doubled up separators.
            while (output.ToString().LastIndexOf(doubleSeparator, StringComparison.Ordinal) > -1)
            {
                output = output.Replace(doubleSeparator, directorySep);
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
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Length == 0)
            {
                return string.Empty;
            }

            var output = new StringBuilder(path);

            output = _illegalPathChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

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
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Length == 0)
            {
                return string.Empty;
            }

            var filePath = new StringBuilder(FormatDirectory(Path.GetDirectoryName(path), Path.DirectorySeparatorChar));
            var output = new StringBuilder(Path.GetFileName(path));

            output = _illegalFileChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

            filePath.Append(output);

            return filePath.ToString();
        }
        #endregion
    }
}
