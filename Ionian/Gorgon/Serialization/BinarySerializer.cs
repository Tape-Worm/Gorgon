#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Sunday, January 07, 2007 3:29:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing a binary serializer.
	/// </summary>
	public class BinarySerializer
		: Serializer
	{
		#region Variables.
		private BinaryWriterEx _writer = null;			// Binary stream writer object.
		private BinaryReaderEx _reader = null;			// Binary stream reader object.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to perform clean up of all objects.  FALSE to clean up only unmanaged objects.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_reader != null)
					_reader.Close();
				if (_writer != null)
					_writer.Close();
			}

			_reader = null;
			_writer = null;
		}

		/// <summary>
		/// Function to serialize an object.
		/// </summary>
		public override void Serialize()
		{
			if (SerializationStream == null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer stream is not open.");

			if (_writer != null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer is already open for writing.");
			
			// Use the stream for writing binary data.
			_writer = new BinaryWriterEx(SerializationStream, DontCloseStream);			
			SerializationObject.WriteData(this);
		}

		/// <summary>
		/// Function to deserialize an object.
		/// </summary>
		public override void Deserialize()
		{
			if (SerializationStream == null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer stream is not open.");

			if (_reader != null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer is already open for reading.");

			// Use the stream for reading binary data.
			_reader = new BinaryReaderEx(SerializationStream, DontCloseStream);
			SerializationObject.ReadData(this);
		}

		/// <summary>
		/// Function to write a string value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, string value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write a character value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, char value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write an array of character values to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="values">Values to write.</param>
		/// <param name="start">Array index to start at.</param>
		/// <param name="count">Number of bytes to read.</param>
		public override void Write(string name, char[] values, int start, int count)
		{
			if (start < 0)
				throw new ArgumentOutOfRangeException("start");
			if (start + count > values.Length)
				throw new ArgumentOutOfRangeException("start + count");
			if (count < 1)
				throw new ArgumentOutOfRangeException("count");

			_writer.Write(values, 0, values.Length);
		}

		/// <summary>
		/// Function to write an unsigned long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, ulong value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write a long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, long value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write an unsigned integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, uint value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write an integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, int value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, float value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, double value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write an unsigned short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, ushort value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write a short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, short value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write a signed byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, sbyte value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write a byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, byte value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to write an array of byte values to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="values">Values to write.</param>
		/// <param name="start">Array index to start at.</param>
		/// <param name="count">Number of bytes to read.</param>
		public override void Write(string name, byte[] values, int start, int count)
		{
			if (start < 0)
				throw new ArgumentOutOfRangeException("start");
			if (start + count > values.Length)
				throw new ArgumentOutOfRangeException("start + count");
			if (count < 1)
				throw new ArgumentOutOfRangeException("count");

			_writer.Write(values, 0, values.Length);
		}

		/// <summary>
		/// Function to write a boolean value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, bool value)
		{
			_writer.Write(value);
		}

		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override bool ReadBool(string name)
		{
			return _reader.ReadBoolean();
		}

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of bytes to read.</param>
		/// <returns>Value in the stream.</returns>
		public override byte[] ReadBytes(string name, int count)
		{
			return _reader.ReadBytes(count);
		}

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override byte[] ReadBytes(string name)
		{
			long size = SerializationStream.Length - SerializationStream.Position;	// Get the remaining size.
			byte[] result = null;													// Resulting data buffer.

			if (size <= 0)
				throw new InvalidDataException("The image has no more data to read.");

			// Allocate.
			result = new byte[size];

			// Copy the data into the buffer.
			_reader.Read(result, 0, (int)size);

			return result;
		}

		/// <summary>
		/// Function to read a byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override byte ReadByte(string name)
		{
			return _reader.ReadByte();
		}

		/// <summary>
		/// Function to read a signed byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override sbyte ReadSByte(string name)
		{
			return _reader.ReadSByte();
		}

		/// <summary>
		/// Function to read a short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override short ReadInt16(string name)
		{
			return _reader.ReadInt16();
		}

		/// <summary>
		/// Function to read an unsigned short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override ushort ReadUInt16(string name)
		{
			return _reader.ReadUInt16();
		}

		/// <summary>
		/// Function to read a double value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override double ReadDouble(string name)
		{
			return _reader.ReadDouble();
		}

		/// <summary>
		/// Function to read a floating point value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override float ReadSingle(string name)
		{
			return _reader.ReadSingle();
		}

		/// <summary>
		/// Function to read an integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override int ReadInt32(string name)
		{
			return _reader.ReadInt32();
		}

		/// <summary>
		/// Function to read an unsigned integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override uint ReadUInt32(string name)
		{
			return _reader.ReadUInt32();
		}

		/// <summary>
		/// Function to read a long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override long ReadInt64(string name)
		{
			return _reader.ReadInt64();
		}

		/// <summary>
		/// Function to read an unsigned long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override ulong ReadUInt64(string name)
		{
			return _reader.ReadUInt64();
		}

		/// <summary>
		/// Function to read an array of character values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of characters to read.</param>
		/// <returns>Value in the stream.</returns>
		public override char[] ReadChars(string name, int count)
		{
			return _reader.ReadChars(count);
		}

		/// <summary>
		/// Function to read a character value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override char ReadChar(string name)
		{
			return _reader.ReadChar();
		}

		/// <summary>
		/// Function to read a string values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override string ReadString(string name)
		{
			return _reader.ReadString();
		}

		/// <summary>
		/// Function to read an attribute for an item.
		/// </summary>
		/// <param name="item">Item that contains the attribute.</param>
		/// <param name="attributeName">Attribute to read.</param>
		/// <returns>Value for the attribute.</returns>
		public override string ReadAttribute(string item, string attributeName)
		{
			// Just return next item.
			return ReadString(attributeName);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serialObject">Object to serialize.</param>
		/// <param name="stream">Stream to write or read data through.</param>
		public BinarySerializer(ISerializable serialObject, Stream stream) 
			: base(serialObject, stream)
		{
		}
		#endregion
	}
}
