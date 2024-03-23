
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, January 23, 2013 7:37:44 PM
// 


using System.Text;
using Gorgon.Graphics;
using Gorgon.IO;

namespace GorgonLibrary.IO;

/// <summary>
/// Reads Gorgon chunked formatted data
/// </summary>
/// <remarks>This object will take data and turn it into chunks of data.  This is similar to the old IFF format in that 
/// it allows Gorgon's file formats to be future proof.  That is, if a later version of Gorgon has support for a feature
/// that does not exist in a previous version, then the older version will be able to read the file and skip the 
/// unnecessary parts.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonChunkReader" /> class
/// </remarks>
/// <param name="stream">The stream.</param>
internal class GorgonChunkReader(Stream stream)
        : GorgonChunkedFormat(stream, ChunkAccessMode.Read)
{

    /// <summary>
    /// Function to determine if the next bytes indicate match the chunk ID.
    /// </summary>
    /// <param name="chunkName">Name of the chunk.</param>
    /// <returns>TRUE if the next bytes are a the specified chunk ID, FALSE if not.</returns>
    /// <remarks>The <paramref name="chunkName"/> parameter must be at least 8 characters in length, if it is not, then an exception will be thrown. 
    /// If the chunkName parameter is longer than 8 characters, then it will be truncated to 8 characters.
    /// </remarks>
    public bool HasChunk(string chunkName)
    {
        // If we're at the end of the stream, then obviously we don't have the chunk ID.
        if (Reader.BaseStream.Position + 8 > Reader.BaseStream.Length)
        {
            return false;
        }

        long streamPosition = Reader.BaseStream.Position;
        ulong chunkID = chunkName.ChunkID();
        ulong streamData = ReadUInt64();

        // Reset the stream position.
        Reader.BaseStream.Position = streamPosition;

        return chunkID == streamData;
    }

    /// <summary>
    /// Function to read a signed byte from the stream.
    /// </summary>
    /// <returns>The signed byte in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public sbyte ReadSByte()
    {
        ValidateAccess(false);

        return Reader.ReadSByte();
    }

    /// <summary>
    /// Function to read an unsigned byte from the stream.
    /// </summary>
    /// <returns>Unsigned byte in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public byte ReadByte()
    {
        ValidateAccess(false);
        return Reader.ReadByte();
    }

    /// <summary>
    /// Function to read a signed 16 bit integer from the stream.
    /// </summary>
    /// <returns>The signed 16 bit integer in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public short ReadInt16()
    {
        ValidateAccess(false);
        return Reader.ReadInt16();
    }

    /// <summary>
    /// Function to read an unsigned 16 bit integer from the stream.
    /// </summary>
    /// <returns>The unsigned 16 bit integer in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public ushort ReadUInt16()
    {
        ValidateAccess(false);
        return Reader.ReadUInt16();
    }

    /// <summary>
    /// Function to read a signed 32 bit integer from the stream.
    /// </summary>
    /// <returns>The signed 32 bit integer in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public int ReadInt32()
    {
        ValidateAccess(false);
        return Reader.ReadInt32();
    }

    /// <summary>
    /// Function to read a unsigned 32 bit integer from the stream.
    /// </summary>
    /// <returns>The unsigned 32 bit integer in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public uint ReadUInt32()
    {
        ValidateAccess(false);
        return Reader.ReadUInt32();
    }

    /// <summary>
    /// Function to read a signed 64 bit integer from the stream.
    /// </summary>
    /// <returns>The signed 64 bit integer in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public long ReadInt64()
    {
        ValidateAccess(false);
        return Reader.ReadInt64();
    }

    /// <summary>
    /// Function to read an unsigned 64 bit integer from the stream.
    /// </summary>
    /// <returns>The unsigned 64 bit integer in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public ulong ReadUInt64()
    {
        ValidateAccess(false);
        return Reader.ReadUInt64();
    }

    /// <summary>
    /// Function to read a boolean value from the stream.
    /// </summary>
    /// <returns>The boolean value in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public bool ReadBoolean()
    {
        ValidateAccess(false);
        return Reader.ReadBoolean();
    }

    /// <summary>
    /// Function to read a single character from the stream.
    /// </summary>
    /// <returns>The single character in the stream.</returns>
    /// <remarks>This will read 2 bytes for the character since the default encoding for .NET is UTF-16.</remarks>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public char ReadChar()
    {
        ValidateAccess(false);

        return (char)ReadInt16();
    }

    /// <summary>
    /// Function to read a string from the stream with the specified encoding.
    /// </summary>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>The string value in the stream.</returns>
    /// <remarks>If the <paramref name="encoding"/> is NULL (Nothing in VB.Net), UTF-8 encoding will be used instead.</remarks>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public string ReadString(Encoding encoding)
    {
        ValidateAccess(false);

        return Reader.BaseStream.ReadString(encoding);
    }

    /// <summary>
    /// Function to read a string from the stream.
    /// </summary>
    /// <returns>The string value in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public string ReadString()
    {
        ValidateAccess(false);
        return Reader.BaseStream.ReadString(null);
    }

    /// <summary>
    /// Function to read double precision value from the stream.
    /// </summary>
    /// <returns>The double precision value in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public double ReadDouble()
    {
        ValidateAccess(false);
        return Reader.ReadDouble();
    }

    /// <summary>
    /// Function to read a single precision floating point value from the stream.
    /// </summary>
    /// <returns>The single precision floating point value in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public float ReadFloat()
    {
        ValidateAccess(false);
        return Reader.ReadSingle();
    }

    /// <summary>
    /// Function to read a decimal value from the stream.
    /// </summary>
    /// <returns>The decimal value in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public decimal ReadDecimal()
    {
        ValidateAccess(false);
        return Reader.ReadDecimal();
    }

    /// <summary>
    /// Function to read an array of bytes from the stream.
    /// </summary>
    /// <param name="data">Array of bytes in the stream.</param>
    /// <param name="startIndex">Starting index in the array.</param>
    /// <param name="count">Number of bytes in the array to read.</param>
    /// <exception cref="IOException">Thrown when the stream is read-only.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
    /// <para>-or-</para>
    /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
    /// </exception>
    public void Read(byte[] data, int startIndex, int count)
    {
        ValidateAccess(false);

        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if ((data.Length == 0) || (count <= 0))
        {
            return;
        }

        Reader.Read(data, startIndex, count);
    }

    /// <summary>
    /// Function to read a range of generic values.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
    /// <param name="value">Array of values to read.</param>
    /// <param name="startIndex">Starting index in the array.</param>
    /// <param name="count">Number of array elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (Nothing in VB.Net).</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="startIndex"/> parameter is less than 0.
    /// <para>-or-</para>
    /// <para>Thrown when the startIndex parameter is equal to or greater than the number of elements in the value parameter.</para>
    /// <para>-or-</para>
    /// <para>Thrown when the sum of startIndex and <paramref name="count"/> is greater than the number of elements in the value parameter.</para>
    /// </exception>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public void ReadRange<T>(T[] value, int startIndex, int count)
        where T : unmanaged
    {
        ValidateAccess(false);

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if ((value.Length == 0) || (count <= 0))
        {
            return;
        }

        Reader.ReadRange(value, startIndex, count);
    }

    /// <summary>
    /// Function to read a range of generic values.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
    /// <param name="value">Array of values to read.</param>
    /// <param name="count">Number of array elements to copy.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (Nothing in VB.Net).</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="count"/> parameter is greater than the number of elements in the value parameter.
    /// </exception>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public void ReadRange<T>(T[] value, int count)
        where T : unmanaged => ReadRange(value, 0, count);

    /// <summary>
    /// Function to read a range of generic values.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
    /// <param name="value">Array of values to read.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="value"/> parameter is NULL (Nothing in VB.Net).</exception>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public void ReadRange<T>(T[] value)
        where T : unmanaged
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        ReadRange(value, 0, value.Length);
    }

    /// <summary>
    /// Function to read a range of generic values.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
    /// <param name="count">Number of array elements to copy.</param>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public T[] ReadRange<T>(int count)
        where T : unmanaged
    {
        T[] array = new T[count];

        ReadRange(array, 0, count);

        return array;
    }

    /// <summary>
    /// Function to read a generic value from the stream.
    /// </summary>
    /// <typeparam name="T">Type of value to read.  Must be a value type.</typeparam>
    /// <returns>The value in the stream.</returns>
    public T Read<T>()
        where T : unmanaged => Reader.ReadValue<T>();

    /// <summary>
    /// Function to read a point from the stream.
    /// </summary>
    /// <returns>The point in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public Point ReadPoint()
    {
        ValidateAccess(false);

        return new Point(Reader.ReadInt32(), Reader.ReadInt32());
    }

    /// <summary>
    /// Function to read a point from the stream.
    /// </summary>
    /// <returns>The point in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public PointF ReadPointF()
    {
        ValidateAccess(false);

        return new PointF(Reader.ReadSingle(), Reader.ReadSingle());
    }

    /// <summary>
    /// Function to read a point from the stream.
    /// </summary>
    /// <returns>The point in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public Size ReadSize()
    {
        ValidateAccess(false);

        return new Size(Reader.ReadInt32(), Reader.ReadInt32());
    }

    /// <summary>
    /// Function to read a point from the stream.
    /// </summary>
    /// <returns>The point in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public SizeF ReadSizeF()
    {
        ValidateAccess(false);

        return new SizeF(Reader.ReadSingle(), Reader.ReadSingle());
    }

    /// <summary>
    /// Function to read a rectangle from the stream.
    /// </summary>
    /// <returns>The rectangle in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public Rectangle ReadRectangle()
    {
        ValidateAccess(false);

        return new Rectangle(Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32(), Reader.ReadInt32());
    }

    /// <summary>
    /// Function to read a rectangle from the stream.
    /// </summary>
    /// <returns>The rectangle in the stream.</returns>
    /// <exception cref="IOException">Thrown when the stream is write-only.</exception>
    public GorgonRectangleF ReadRectangleF()
    {
        ValidateAccess(false);

        return new GorgonRectangleF(Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle(), Reader.ReadSingle());
    }
}
