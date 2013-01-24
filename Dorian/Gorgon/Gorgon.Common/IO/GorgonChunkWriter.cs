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
// Created: Wednesday, January 23, 2013 7:37:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Native;

namespace GorgonLibrary.IO
{
	/// <summary>
	/// Writes Gorgon chunked formatted data.
	/// </summary>
	/// <remarks>This object will take data and turn it into chunks of data.  This is similar to the old IFF format in that 
	/// it allows Gorgon's file formats to be future proof.  That is, if a later version of Gorgon has support for a feature
	/// that does not exist in a previous version, then the older version will be able to read the file and skip the 
	/// unnecessary parts.</remarks>
	public class GorgonChunkWriter
		: GorgonChunkedFormat
	{
		#region Methods.
		/// <summary>
		/// Function to write a signed byte to the stream.
		/// </summary>
		/// <param name="value">Value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteSByte(sbyte value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write an unsigned byte to the stream.
		/// </summary>
		/// <param name="value">Unsigned byte to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteByte(byte value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a signed 16 bit integer to the stream.
		/// </summary>
		/// <param name="value">The signed 16 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteInt16(Int16 value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write an unsigned 16 bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 16 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteUInt16(UInt16 value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a signed 32 bit integer to the stream.
		/// </summary>
		/// <param name="value">The signed 32 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteInt32(Int32 value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a unsigned 32 bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 32 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteUInt32(UInt32 value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a signed 64 bit integer to the stream.
		/// </summary>
		/// <param name="value">The signed 64 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteInt64(Int64 value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write an unsigned 64 bit integer to the stream.
		/// </summary>
		/// <param name="value">The unsigned 64 bit integer to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteUInt64(UInt64 value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a boolean value to the stream.
		/// </summary>
		/// <param name="value">The boolean value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteBoolean(Boolean value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a single character to the stream.
		/// </summary>
		/// <param name="value">The single character to write to the stream.</param>
		/// <remarks>This will write 2 bytes for the character since the default encoding for .NET is UTF-16.  Surrogate characters are not allowed, and will throw an exception.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="value"/> parameter is a surrogate character.</exception>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteChar(char value)
		{
			ValidateAccess(true);
			if (char.IsSurrogate(value))
			{
				throw new ArgumentException("Cannot write out surrogate to a single character.", "value");
			}

			WriteInt16((short)value);
		}

		/// <summary>
		/// Function to write a string to the stream with the specified encoding.
		/// </summary>
		/// <param name="value">The string value to write to the stream.</param>
		/// <param name="encoding">The encoding to use.</param>
		/// <remarks>If the <paramref name="encoding"/> is NULL (Nothing in VB.Net), UTF-8 encoding will be used instead.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteString(string value, Encoding encoding)
		{
			ValidateAccess(true);
			value.WriteToStream(Writer.BaseStream, encoding);
		}

		/// <summary>
		/// Function to write a string to the stream.
		/// </summary>
		/// <param name="value">The string value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteString(string value)
		{
			ValidateAccess(true);
			value.WriteToStream(Writer.BaseStream, null);
		}

		/// <summary>
		/// Function to write double precision value to the stream.
		/// </summary>
		/// <param name="value">The double precision value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteDouble(double value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a single precision floating point value to the stream.
		/// </summary>
		/// <param name="value">The single precision floating point value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteFloat(float value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write a decimal value to the stream.
		/// </summary>
		/// <param name="value">The decimal value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteDecimal(decimal value)
		{
			ValidateAccess(true);
			Writer.Write(value);
		}

		/// <summary>
		/// Function to write an array of bytes to the stream.
		/// </summary>
		/// <param name="data">Array of bytes to write to the stream.</param>
		/// <param name="startIndex">Starting index in the array.</param>
		/// <param name="count">Number of bytes in the array to write.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void Write(byte[] data, int startIndex, int count)
		{
			ValidateAccess(true);
			Writer.Write(data, startIndex, count);
		}

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WritePoint(Point value)
		{
			ValidateAccess(true);

			Writer.Write(value.X);
			Writer.Write(value.Y);
		}

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WritePoint(PointF value)
		{
			ValidateAccess(true);

			Writer.Write(value.X);
			Writer.Write(value.Y);
		}

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteSize(Size value)
		{
			ValidateAccess(true);

			Writer.Write(value.Width);
			Writer.Write(value.Height);
		}

		/// <summary>
		/// Function to write a point to the stream.
		/// </summary>
		/// <param name="value">The point to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteSize(SizeF value)
		{
			ValidateAccess(true);

			Writer.Write(value.Width);
			Writer.Write(value.Height);
		}

		/// <summary>
		/// Function to write a rectangle to the stream.
		/// </summary>
		/// <param name="value">The rectangle to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteRectangle(Rectangle value)
		{
			ValidateAccess(true);

			Writer.Write(value.X);
			Writer.Write(value.Y);
			Writer.Write(value.Width);
			Writer.Write(value.Height);
		}

		/// <summary>
		/// Function to write a rectangle to the stream.
		/// </summary>
		/// <param name="value">The rectangle to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public void WriteRectangle(RectangleF value)
		{
			ValidateAccess(true);

			Writer.Write(value.X);
			Writer.Write(value.Y);
			Writer.Write(value.Width);
			Writer.Write(value.Height);
		}

		/// <summary>
		/// Function to write a generic value to the stream.
		/// </summary>
		/// <typeparam name="T">Type of value to write.  Must be a value type.</typeparam>
		/// <param name="value">Value to write to the stream.</param>
		/// <exception cref="System.IO.IOException">Thrown when the stream is read-only.</exception>
		public unsafe void Write<T>(T value)
			where T : struct
		{
			ValidateAccess(true);

			int size = DirectAccess.SizeOf<T>();
			byte *pointer = (byte *)DirectAccess.GetPtr<T>(ref value);

			switch (size)
			{
				case 1:
					Writer.Write(*pointer);
					break;
				case 2:
					Writer.Write(*(short*)pointer);
					break;
				case 4:
					Writer.Write(*(int*)pointer);
					break;
				case 8:
					Writer.Write(*(long*)pointer);
					break;
				default:
					while (size > 0)
					{
						byte byteValue = *pointer;
						Writer.Write(byteValue);

						pointer++;
						size--;
					}
					break;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonChunkWriter" /> class.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public GorgonChunkWriter(Stream stream)
			: base(stream, ChunkAccessMode.Write)
		{
		}
		#endregion
	}
}
