#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Thursday, October 26, 2006 12:27:02 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing a serializer for FX shader files.
	/// </summary>
	public class ShaderSerializer<T>
		: Serializer
		where T : Shader, ISerializable
	{
		#region Variables.
		private StreamWriterEx _writer = null;			// Stream writer object.
		private StreamReaderEx _reader = null;			// Stream reader object.
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
			_writer = new StreamWriterEx(SerializationStream, Encoding.UTF8, DontCloseStream);
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
			_reader = new StreamReaderEx(SerializationStream, true, DontCloseStream);
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
			throw new Exception("The method or operation is not implemented.");
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
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write an unsigned long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, ulong value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write a long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, long value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write an unsigned integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, uint value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write an integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, int value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, float value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, double value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write an unsigned short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, ushort value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write a short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, short value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write a signed byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, sbyte value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to write a byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, byte value)
		{
			throw new Exception("The method or operation is not implemented.");
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
			// Write directly to the stream using binary data.
			SerializationStream.Write(values, start, count);
		}

		/// <summary>
		/// Function to write a boolean value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, bool value)
		{
			throw new Exception("The method or operation is not implemented.");
		}


		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override bool ReadBool(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of bytes to read.</param>
		/// <returns>Value in the stream.</returns>
		public override byte[] ReadBytes(string name, int count)
		{
			byte[] data = new byte[count];		// Data to read.

			if (SerializationStream.Position + count > SerializationStream.Length)
				throw new ArgumentOutOfRangeException("count");

			SerializationStream.Read(data, 0, count);

			return data;
		}

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override byte[] ReadBytes(string name)
		{
			throw new NotImplementedException("This overload of ReadBytes does not apply to the shader serializer.");
		}

		/// <summary>
		/// Function to read a byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override byte ReadByte(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read a signed byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override sbyte ReadSByte(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read a short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override short ReadInt16(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read an unsigned short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override ushort ReadUInt16(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read a double value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override double ReadDouble(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read a floating point value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override float ReadSingle(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read an integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override int ReadInt32(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read an unsigned integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override uint ReadUInt32(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read a long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override long ReadInt64(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read an unsigned long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override ulong ReadUInt64(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read an array of character values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of characters to read.</param>
		/// <returns>Value in the stream.</returns>
		public override char[] ReadChars(string name, int count)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read a character value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override char ReadChar(string name)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Function to read a string values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override string ReadString(string name)
		{
			return _reader.ReadToEnd();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shader">Shader to (de)serialize.</param>
		/// <param name="stream">Stream to write or read data through.</param>
		internal ShaderSerializer(BaseShader<T> shader, Stream stream) 
			: base(shader as ISerializable, stream)
		{
		}
		#endregion
	}
}
