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
	/// Reads Gorgon chunked formatted data.
	/// </summary>
	/// <remarks>This object will take data and turn it into chunks of data.  This is similar to the old IFF format in that 
	/// it allows Gorgon's file formats to be future proof.  That is, if a later version of Gorgon has support for a feature
	/// that does not exist in a previous version, then the older version will be able to read the file and skip the 
	/// unnecessary parts.</remarks>
	public class GorgonChunkReader
		: GorgonChunkedFormat
	{
		#region Methods.
		/// <summary>
		/// Function to read a signed byte from the stream.
		/// </summary>
		/// <returns>The signed byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public sbyte ReadSByte()
		{
			ValidateAccess(false);

			return Reader.ReadSByte();
		}

		/// <summary>
		/// Function to read an unsigned byte from the stream.
		/// </summary>
		/// <returns>Unsigned byte in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public byte ReadByte()
		{
			ValidateAccess(false);
			return Reader.ReadByte();
		}

		/// <summary>
		/// Function to read a signed 16 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Int16 ReadInt16()
		{
			ValidateAccess(false);
			return Reader.ReadInt16();
		}

		/// <summary>
		/// Function to read an unsigned 16 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 16 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public UInt16 ReadUInt16()
		{
			ValidateAccess(false);
			return Reader.ReadUInt16();
		}

		/// <summary>
		/// Function to read a signed 32 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Int32 ReadInt32()
		{
			ValidateAccess(false);
			return Reader.ReadInt32();
		}

		/// <summary>
		/// Function to read a unsigned 32 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 32 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public UInt32 ReadUInt32()
		{
			ValidateAccess(false);
			return Reader.ReadUInt32();
		}

		/// <summary>
		/// Function to read a signed 64 bit integer from the stream.
		/// </summary>
		/// <returns>The signed 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Int64 ReadInt64()
		{
			ValidateAccess(false);
			return Reader.ReadInt64();
		}

		/// <summary>
		/// Function to read an unsigned 64 bit integer from the stream.
		/// </summary>
		/// <returns>The unsigned 64 bit integer in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public UInt64 ReadUInt64()
		{
			ValidateAccess(false);
			return Reader.ReadUInt64();
		}

		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <returns>The boolean value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Boolean ReadBoolean()
		{
			ValidateAccess(false);
			return Reader.ReadBoolean();
		}

		/// <summary>
		/// Function to read a single character from the stream.
		/// </summary>
		/// <returns>The single character in the stream.</returns>
		/// <remarks>This will read 2 bytes for the character since the default encoding for .NET is UTF-16.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public char ReadChar()
		{
			ValidateAccess(false);

			return (char)ReadInt16();
		}

		/// <summary>
		/// Function to read a string from the stream with the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding to use.</returns>
		/// <returns>The string value in the stream.</returns>
		/// <remarks>If the <paramref name="encoding"/> is NULL (Nothing in VB.Net), UTF-8 encoding will be used instead.</remarks>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public string ReadString(Encoding encoding)
		{
			ValidateAccess(false);

			return Reader.BaseStream.ReadString(encoding);
		}

		/// <summary>
		/// Function to read a string from the stream.
		/// </summary>
		/// <returns>The string value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public string ReadString(string value)
		{
			ValidateAccess(false);
			return Reader.BaseStream.ReadString(null);
		}

		/// <summary>
		/// Function to read double precision value from the stream.
		/// </summary>
		/// <returns>The double precision value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public double ReadDouble(double value)
		{
			ValidateAccess(false);
			return Reader.ReadDouble();
		}

		/// <summary>
		/// Function to read a single precision floating point value from the stream.
		/// </summary>
		/// <returns>The single precision floating point value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public float ReadFloat(float value)
		{
			ValidateAccess(false);
			return Reader.ReadSingle();
		}

		/// <summary>
		/// Function to read a decimal value from the stream.
		/// </summary>
		/// <returns>The decimal value in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public decimal ReadDecimal()
		{
			ValidateAccess(false);
			return Reader.ReadDecimal();
		}

		/// <summary>
		/// Function to read an array of bytes from the stream.
		/// </summary>
		/// <param name="data">Array of bytes in the stream.</returns>
		/// <param name="startIndex">Starting index in the array.</returns>
		/// <param name="count">Number of bytes in the array to read.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public void Read(byte[] data, int startIndex, int count)
		{
			ValidateAccess(false);
			Reader.Read(data, startIndex, count);
		}

		/// <summary>
		/// Function to read a generic value from the stream.
		/// </summary>
		/// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
		/// <returns>The value in the stream.</returns>
		public unsafe T Read<T>()
			where T : struct
		{
			T returnVal = default(T);
			int size = DirectAccess.SizeOf<T>();
			byte* pointer = (byte*)DirectAccess.GetPtr<T>(ref returnVal);

			switch (size)
			{
				case 1:
					*pointer = Reader.ReadByte();
					break;
				case 2:
					*((short*)pointer) = Reader.ReadInt16();
					break;
				case 4:
					*((int*)pointer) = Reader.ReadInt32();
					break;
				case 8:
					*((long*)pointer) = Reader.ReadInt64();
					break;
				default:
					while (size > 0)
					{
						*pointer = Reader.ReadByte();

						pointer++;
						size--;
					}
					break;
			}

			return returnVal;
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Point ReadPoint(Point value)
		{
			ValidateAccess(false);

			return new Point(Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public PointF ReadPointF(PointF value)
		{
			ValidateAccess(false);

			return new PointF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Size ReadSize(Size value)
		{
			ValidateAccess(false);

			return new Size(Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a point from the stream.
		/// </summary>
		/// <returns>The point in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public SizeF ReadSizeF(SizeF value)
		{
			ValidateAccess(false);

			return new SizeF(Reader.ReadSingle(), Reader.ReadSingle());
		}

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public Rectangle ReadRectangle(Rectangle value)
		{
			ValidateAccess(false);

			return new Rectangle(Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32());
		}

		/// <summary>
		/// Function to read a rectangle from the stream.
		/// </summary>
		/// <returns>The rectangle in the stream.</returns>
		/// <exception cref="System.IO.IOException">Thrown when the stream is write-only.</exception>
		public RectangleF ReadRectangleF(RectangleF value)
		{
			ValidateAccess(false);

			return new RectangleF(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonChunkReader" /> class.
		/// </summary>
		/// <param name="stream">The stream.</returns>
		public GorgonChunkReader(Stream stream)
			: base(stream, ChunkAccessMode.Read)
		{
		}
		#endregion
	}
}
