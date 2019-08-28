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
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// Extension methods for IO operations and string formatting.
    /// </summary>
    public static class GorgonIOExtensions
    {
        #region Variables.
        // The system directory path separator.
        private static readonly string _directoryPathSeparator = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
        // The system alternate path separator.
        private static readonly string _altPathSeparator = Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
        // Illegal path characters.
        private static readonly char[] _illegalPathChars = Path.GetInvalidPathChars();
        // Illegal file name characters.
        private static readonly char[] _illegalFileChars = Path.GetInvalidFileNameChars();
        // Buffer for reading a string back from a stream.
        private static byte[] _buffer;
        // Buffer to hold decoded characters when reading from a stream.
        private static char[] _charBuffer;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy the contents of this stream into another stream, up to a specified byte count.
        /// </summary>
        /// <param name="stream">The source stream that will be copied from.</param>
        /// <param name="destination">The stream that will receive the copy of the data.</param>
        /// <param name="count">The number of bytes to copy.</param>
        /// <param name="bufferSize">[Optional] The size of the temporary buffer used to buffer the data between streams.</param>
        /// <returns>The number of bytes copied, or 0 if no data was copied or at the end of a stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/>, or the <paramref name="stream"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="stream"/> is write-only.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destination"/> is read-only.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method is an extension of the <see cref="Stream.CopyTo(Stream,int)"/> method. But unlike that method, it will copy up to the number of bytes specified by <paramref name="count"/>. 
        /// </para>
        /// <para>
        /// The <paramref name="bufferSize"/> is used to copy data in blocks, rather than attempt to copy byte-by-byte. This may improve performance significantly. It is not recommended that the buffer 
        /// exceeds than 85,000 bytes. A value under this will ensure that the internal buffer will remain on the small object heap and be collected quickly when done. 
        /// </para>
        /// </remarks>
        public static int CopyToStream(this Stream stream, Stream destination, int count, int bufferSize = 81920)
        {
            if (stream.Length <= stream.Position)
            {
                return 0;
            }

            byte[] buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                return count < 1 ? 0 : CopyToStream(stream, destination, count, buffer);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Function to copy the contents of this stream into another stream, up to a specified byte count.
        /// </summary>
        /// <param name="stream">The source stream that will be copied from.</param>
        /// <param name="destination">The stream that will receive the copy of the data.</param>
        /// <param name="count">The number of bytes to copy.</param>
        /// <param name="buffer">The buffer to use for reading and writing the chunks of the file.</param>
        /// <returns>The number of bytes copied, or 0 if no data was copied or at the end of a stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="destination"/>,<paramref name="stream"/>, or the <paramref name="buffer"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="buffer"/> is less than 1 byte.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="stream"/> is write-only.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destination"/> is read-only.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method is an extension of the <see cref="Stream.CopyTo(Stream,int)"/> method. But unlike that method, it will copy up to the number of bytes specified by <paramref name="count"/>. 
        /// </para>
        /// <para>
        /// The <paramref name="buffer"/> is used to copy data in blocks, rather than attempt to copy byte-by-byte. This may improve performance significantly. It is not recommended that the buffer 
        /// exceeds than 85,000 bytes. A value under this will ensure that the internal buffer will remain on the small object heap and be collected quickly when done. 
        /// </para>
        /// </remarks>
        public static int CopyToStream(this Stream stream, Stream destination, int count, byte[] buffer)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_WRITEONLY, nameof(stream));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!destination.CanWrite)
            {
                throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_READONLY, nameof(destination));
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Length == 0)
            {
                throw new ArgumentEmptyException(nameof(buffer));
            }

            if (stream.Length <= stream.Position)
            {
                return 0;
            }

            if (count < 1)
            {
                return 0;
            }

            int bufferSize = buffer.Length;
            int result = 0;
            int bytesRead;

            while ((count > 0) && ((bytesRead = stream.Read(buffer, 0, count.Min(bufferSize))) != 0))
            {
                destination.Write(buffer, 0, bytesRead);
                result += bytesRead;
                count -= bytesRead;
            }

            return result;
        }

        /// <summary>
        /// Function to write a string into a stream with UTF-8 encoding.
        /// </summary>
        /// <param name="value">The string to write into the stream.</param>
        /// <param name="stream">Stream to encode the string into.</param>
        /// <remarks>
        /// <para>
        /// This will encode the string as a series of bytes into a stream.  The length of the string, in bytes, will be prefixed to the string as a series of 7 bit byte values.
        /// </para>
        /// <para>
        /// This method is <b>not</b> thread safe. Use care when using threads with this method.
        /// </para>
        /// </remarks>
        /// <returns>The number of bytes written to the stream.</returns>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> parameter is read-only.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        public static int WriteToStream(this string value, Stream stream) => WriteToStream(value, stream, null);

        /// <summary>
        /// Function to encode a string into a stream with the specified encoding.
        /// </summary>
        /// <param name="value">The string to write into the stream.</param>
        /// <param name="stream">Stream to encode the string into.</param>
        /// <param name="encoding">Encoding for the string.</param>
        /// <remarks>
        /// <para>
        /// This will encode the string as a series of bytes into a stream.  The length of the string, in bytes, will be prefixed to the string as a series of 7 bit byte values.
        /// </para>
        /// <para>If the <paramref name="encoding"/> parameter is <b>null</b>, then UTF-8 encoding will be used.</para>
        /// <para>
        /// This method is <b>not</b> thread safe. Use care when using threads with this method.
        /// </para>
        /// </remarks>
        /// <returns>The number of bytes written to the stream.</returns>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> parameter is read-only.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        public static int WriteToStream(this string value, Stream stream, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new IOException(Resources.GOR_ERR_STREAM_IS_READONLY);
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
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> parameter is read-only.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// Gorgon stores its strings in a stream by prefixing the string data with the length of the string, in bytes.  This length is encoded as a series of 7-bit bytes.
        /// </para>
        /// <para>
        /// If the <paramref name="encoding"/> parameter is <b>null</b>, then UTF-8 encoding is used.
        /// </para>
        /// <para>
        /// This method is <b>not</b> thread safe. Use care when using threads with this method.
        /// </para>
        /// </remarks>
        public static void WriteString(this Stream stream, string value, Encoding encoding) => WriteToStream(value, stream, encoding);

        /// <summary>
        /// Function to write a string to a stream with UTF-8 encoding.
        /// </summary>
        /// <param name="stream">The stream to write the string into.</param>
        /// <param name="value">The string to write.</param>
        /// <exception cref="IOException">Thrown when the <paramref name="stream"/> parameter is read-only.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// Gorgon stores its strings in a stream by prefixing the string data with the length of the string, in bytes.  This length is encoded as a series of 7-bit bytes.
        /// </para>
        /// <para>
        /// This method is <b>not</b> thread safe. Use care when using threads with this method.
        /// </para>
        /// </remarks>
        public static void WriteString(this Stream stream, string value) => WriteToStream(value, stream, null);

        /// <summary>
        /// Function to read a string from a stream using UTF-8 encoding.
        /// </summary>
        /// <param name="stream">The stream to read the string from.</param>
        /// <returns>The string in the stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when an attempt to read beyond the end of the <paramref name="stream"/> is made.</exception>
        /// <remarks>
        /// <para>
        /// Gorgon stores its strings in a stream by prefixing the string data with the length of the string.  This length is encoded as a series of 7-bit bytes.
        /// </para>
        /// <para>
        /// This method is <b>not</b> thread safe. Use care when using threads with this method.
        /// </para>
        /// </remarks>
        public static string ReadString(this Stream stream) => ReadString(stream, null);

        /// <summary>
        /// Function to read a string from a stream with the specified encoding.
        /// </summary>
        /// <param name="stream">The stream to read the string from.</param>
        /// <param name="encoding">The encoding for the string.</param>
        /// <returns>The string in the stream.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown when an attempt to read beyond the end of the <paramref name="stream"/> is made.</exception>
        /// <remarks>
        /// <para>
        /// Gorgon stores its strings in a stream by prefixing the string data with the length of the string.  This length is encoded as a series of 7-bit bytes.
        /// </para>
        /// <para>
        /// If the <paramref name="encoding"/> parameter is <b>null</b>, then UTF-8 encoding is used.
        /// </para>
        /// <para>
        /// This method is <b>not</b> thread safe. Use care when using threads with this method.
        /// </para>
        /// </remarks>
        public static string ReadString(this Stream stream, Encoding encoding)
        {
            int stringLength = 0;

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
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
                    throw new IOException(Resources.GOR_ERR_STREAM_EOS);
                }

                stringLength |= (value & 0x7F) << counter;
                counter += 7;
                if (((value & 0x80) == 0) || (counter > 32))
                {
                    break;
                }
            }

            if (stringLength == 0)
            {
                return string.Empty;
            }

            // Find the number of bytes required for 4096 characters.
            int maxByteCount = encoding.GetMaxByteCount(4096);
            int maxCharCount = encoding.GetMaxCharCount(maxByteCount);

            // If they've changed or haven't been allocated yet, then allocate our worker buffers.
            if ((_buffer == null) || (_buffer.Length < maxByteCount))
            {
                _buffer = new byte[maxByteCount];
            }

            if ((_charBuffer == null) || (_charBuffer.Length < maxCharCount))
            {
                _charBuffer = new char[maxCharCount];
            }

            Decoder decoder = encoding.GetDecoder();
            StringBuilder result = null;
            counter = 0;

            // Buffer the string in, just in case it's super long.
            while ((stream.Position < stream.Length) && (counter < stringLength))
            {
                // Fill the byte buffer.
                int bytesRead = stream.Read(_buffer, 0, stringLength <= _buffer.Length ? stringLength : _buffer.Length);

                if (bytesRead == 0)
                {
                    throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
                }

                // Get the characters.
                int charsRead = decoder.GetChars(_buffer, 0, bytesRead, _charBuffer, 0);

                // If we've already read the entire string, just dump it back out now.
                if ((counter == 0) && (bytesRead == stringLength))
                {
                    return new string(_charBuffer, 0, charsRead);
                }

                // We'll need a bigger string. So allocate a string builder and use that.
                if (result == null)
                {
                    // Try to max out the string builder size by the length of our string, in characters.
                    result = new StringBuilder(encoding.GetMaxCharCount(stringLength));
                }

                result.Append(_charBuffer, 0, charsRead);

                counter += bytesRead;
            }

            return result?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Function to format a filename with safe characters.
        /// </summary>
        /// <param name="path">The path containing the filename to evaluate.</param>
		/// <returns>A safe filename formatted with placeholder characters if invalid characters are found.</returns>
		/// <remarks>
		/// <para>
		/// This will replace any illegal filename characters with the underscore character.
		/// </para>
		/// <para>
		/// If <b>null</b> or <see cref="string.Empty"/> are passed to this method, then an empty string will be returned. If the path does not contain a 
		/// filename, then an empty string will be returned as well.
		/// </para>
		/// </remarks>
        public static string FormatFileName(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var output = new StringBuilder(path);

            output = _illegalPathChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

            string fileName = Path.GetFileName(output.ToString());

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            output.Length = 0;
            output.Append(fileName);

            output = _illegalFileChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

            return output.ToString();
        }

        /// <summary>
        /// Function to format a directory path with safe characters.
        /// </summary>
        /// <param name="path">The directory path to evaluate..</param>
        /// <param name="directorySeparator">Directory separator character to use.</param>
        /// <returns>A safe directory path formatted with placeholder characters if invalid characters are found. Directory separators will be replaced with the specified separator passed 
        /// to <paramref name="directorySeparator"/>.</returns> 
        /// <remarks>
        /// <para>
        /// This will replace any illegal path characters with the underscore character. Any doubled up directory separators (e.g. // or \\) will be replaced with the directory separator 
        /// passed to <paramref name="directorySeparator"/>.
        /// </para>
        /// <para>
		/// If <b>null</b> or <see cref="string.Empty"/> are passed to this method, then an empty string will be returned. If the path contains only a filename, 
		/// that string will be formatted as though it were a directory path.
		/// </para>
        /// </remarks>
        public static string FormatDirectory(this string path, char directorySeparator)
        {
            string directorySep = _directoryPathSeparator;
            string doubleSeparator = new string(new[] { directorySeparator, directorySeparator });

            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            if ((char.IsWhiteSpace(directorySeparator)) || (_illegalPathChars.Contains(directorySeparator)))
            {
                directorySeparator = Path.DirectorySeparatorChar;
            }

            var output = new StringBuilder(path);

            output = _illegalPathChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

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
            while (output.LastIndexOf(doubleSeparator, StringComparison.Ordinal) > -1)
            {
                output = output.Replace(doubleSeparator, directorySep);
            }

            return output.ToString();
        }

        /// <summary>
        /// Function to format a specific piece of a path.
        /// </summary>
        /// <param name="path">The path part to evaluate and repair.</param>
        /// <returns>A safe path part with placeholder characters if invalid characters are found.</returns>
        /// <remarks>
        /// This method removes illegal symbols from the <paramref name="path"/> and replaces them with an underscore character. It will not respect path separators and will consider those characters 
        /// as illegal if provided in the <paramref name="path"/> parameter.
        /// </remarks>
        public static string FormatPathPart(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            path = path.Replace(Path.DirectorySeparatorChar, '_');
            path = path.Replace(Path.AltDirectorySeparatorChar, '_');

            var output = new StringBuilder(path);

            output = _illegalPathChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

            return output.ToString();
        }

        /// <summary>
        /// Function to split a path into component parts.
        /// </summary>
        /// <param name="path">The path to split.</param>
        /// <param name="directorySeparator">The separator to split the path on.</param>
        /// <returns>An array containing the parts of the path, or an empty array if the path is <b>null</b> or empty.</returns>
        /// <remarks>
        /// This will take a path a split it into individual pieces for evaluation. The <paramref name="directorySeparator"/> parameter will be the character 
        /// used to determine how to split the path. For example:
        /// <code language="csharp">
        ///		string myPath = @"C:\Windows\System32\ddraw.dll";
        ///		string[] parts = myPath.GetPathParts(Path.DirectorySeparatorChar);
        ///		
        ///		foreach(string part in parts)
        ///     {
        ///			Console.WriteLine(part);
        ///		}
        /// 
        ///		/* Output should be:
        ///		 * C:
        ///		 * Windows
        ///		 * System32
        ///		 * ddraw.dll
        ///		 */
        /// </code>
        /// </remarks>
        public static string[] GetPathParts(this string path, char directorySeparator)
        {
            path = path.FormatPath(directorySeparator);

            return path.Split(new[]
                              {
                                  directorySeparator
                              },
                              StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Function to format a path with safe characters.
        /// </summary>
        /// <param name="path">Path to the file or folder to format.</param>
		/// <param name="directorySeparator">Directory separator character to use.</param>
        /// <returns>A safe path formatted with placeholder characters if invalid characters are found.</returns>
        /// <remarks>
        /// <para>
        /// If the path contains directories, they will be formatted according to the formatting applied by <see cref="FormatDirectory"/>, and if the path contains a filename, it will be 
        /// formatted according to the formatting applied by the <see cref="FormatFileName"/> method.
		/// </para>
		/// <para>
		/// If the last character in <paramref name="path"/> is not the same as the <paramref name="directorySeparator"/> parameter, then that last part of the path will be treated as a file. 
		/// </para>
		/// <para>
		/// If no directories are present in the path, then the see <paramref name="directorySeparator"/> is ignored.
		/// </para>
		/// </remarks>
		public static string FormatPath(this string path, char directorySeparator)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            // Filter out bad characters.
            var output = new StringBuilder(path);
            output = _illegalPathChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

            var filePath = new StringBuilder(FormatDirectory(Path.GetDirectoryName(output.ToString()), directorySeparator));

            path = output.ToString();

            // Try to get the filename portion.
            output.Length = 0;
            output.Append(Path.GetFileName(path));

            if (output.Length == 0)
            {
                return filePath.ToString();
            }

            output = _illegalFileChars.Aggregate(output, (current, illegalChar) => current.Replace(illegalChar, '_'));

            filePath.Append(output);

            return filePath.ToString();
        }

        /// <summary>
        /// Function to return the chunk ID based on the name of the chunk passed to this method.
        /// </summary>
        /// <param name="chunkName">The name of the chunk.</param>
        /// <returns>A <see cref="ulong"/> value representing the chunk ID of the name.</returns>
        /// <remarks>
        /// <para>
        /// This method is used to generate a new chunk ID for the <conceptualLink target="7b81343e-e2fc-4f0f-926a-d9193ae481fe">Gorgon chunked file format</conceptualLink>. It converts the characters in the string to their ASCII byte 
        /// equivalents, and then builds a <see cref="ulong"/> value from those bytes.
        /// </para>
        /// <para>
        /// Since the size of an <see cref="ulong"/> is 8 bytes, then the string should contain 8 characters. If it does not, then the ID will be padded with 0's on the right to take up the remaining 
        /// bytes. If the string is larger than 8 characters, then it will be truncated to the 8 character limit.
        /// </para>
        /// <para>
        /// The format of the long value is not endian specific and is encoded in the same order as the characters in the string.  For example, encoding the string 'TESTVALU' produces:<br/>
        /// <list type="table">
        /// <listheader>
        ///		<term>Byte</term>
        ///		<term>1</term>
        ///		<term>2</term>
        ///		<term>3</term>
        ///		<term>4</term>
        ///		<term>5</term>
        ///		<term>6</term>
        ///		<term>7</term>
        ///		<term>8</term>
        /// </listheader>
        ///		<item>
        ///			<term>Character</term>
        ///			<term>'T' (0x54)</term>
        ///			<term>'E' (0x45)</term>
        ///			<term>'S' (0x53)</term>
        ///			<term>'T' (0x54)</term>
        ///			<term>'V' (0x56)</term>
        ///			<term>'A' (0x41)</term>
        ///			<term>'L' (0x4C)</term>
        ///			<term>'U' (0x55)</term>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="chunkName"/> parameter is <b>null</b>.</exception>
        public static ulong ChunkID(this string chunkName)
        {
            if (chunkName == null)
            {
                throw new ArgumentNullException(nameof(chunkName));
            }

            if (chunkName.Length > 8)
            {
                chunkName = chunkName.Substring(0, 8);
            }
            else if (chunkName.Length < 8)
            {
                chunkName = chunkName.PadRight(8, '\0');
            }

            return ((ulong)((byte)chunkName[7]) << 56)
                   | ((ulong)((byte)chunkName[6]) << 48)
                   | ((ulong)((byte)chunkName[5]) << 40)
                   | ((ulong)((byte)chunkName[4]) << 32)
                   | ((ulong)((byte)chunkName[3]) << 24)
                   | ((ulong)((byte)chunkName[2]) << 16)
                   | ((ulong)((byte)chunkName[1]) << 8)
                   | (byte)chunkName[0];
        }
        #endregion
    }
}
