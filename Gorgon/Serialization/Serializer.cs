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
// Created: Thursday, August 10, 2006 9:13:40 PM
// 
#endregion

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Reflection;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Abstract object representing a serialization object.
	/// </summary>
	public abstract class Serializer
	{
		#region Variables.
		private bool _dontCloseStream = false;						// Flag to indicate whether the underlying stream should close when the reader/writer is closed.
		private SortedList _parameters = null;						// Parameter list.
		private Stream _stream = null;								// File stream.
		private ISerializable _serialObject = null;					// Object to serialize.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the stream being used.
		/// </summary>
		protected Stream SerializationStream
		{
			get
			{
				return _stream;
			}
		}

		/// <summary>
		/// Property to return the object that is being serialized.
		/// </summary>
		public ISerializable SerializationObject
		{
			get
			{
				return _serialObject;
			}
		}

		/// <summary>
		/// Property to return whether or not to close the underlying stream when the reader/writer is closed.
		/// </summary>
		public bool DontCloseStream
		{
			get
			{
				return _dontCloseStream;
			}
			set
			{
				_dontCloseStream = value;
			}
		}

		/// <summary>
		/// Property to return the parameter list interface.
		/// </summary>
		public SortedList Parameters
		{
			get
			{
				return _parameters;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to write a comment.
		/// </summary>
		/// <param name="comment">Comment to write.</param>
		public virtual void WriteComment(string comment)
		{
		}

		/// <summary>
		/// Function to write the start of a group of data.
		/// </summary>
		/// <param name="groupName">Name of the group.</param>
		public virtual void WriteGroupBegin(string groupName)
		{
		}

		/// <summary>
		/// Function to write the end of the group started with WriteGroupBegin().
		/// </summary>
		public virtual void WriteGroupEnd()
		{
		}

		/// <summary>
		/// Function to write a string value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, string value);

		/// <summary>
		/// Function to write a character value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, char value);

		/// <summary>
		/// Function to write an array of character values to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="values">Values to write.</param>
		/// <param name="start">Array index to start at.</param>
		/// <param name="count">Number of bytes to read.</param>
		public abstract void Write(string name, char[] values, int start, int count);

		/// <summary>
		/// Function to write an unsigned long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, ulong value);

		/// <summary>
		/// Function to write a long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, long value);

		/// <summary>
		/// Function to write an unsigned integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, uint value);

		/// <summary>
		/// Function to write an integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, int value);

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, float value);

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, double value);

		/// <summary>
		/// Function to write an unsigned short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, ushort value);

		/// <summary>
		/// Function to write a short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, short value);

		/// <summary>
		/// Function to write a signed byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, sbyte value);

		/// <summary>
		/// Function to write a byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, byte value);

		/// <summary>
		/// Function to write an array of byte values to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="values">Values to write.</param>
		/// <param name="start">Array index to start at.</param>
		/// <param name="count">Number of bytes to read.</param>
		public abstract void Write(string name, byte[] values, int start, int count);

		/// <summary>
		/// Function to write a boolean value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public abstract void Write(string name, bool value);

		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract bool ReadBool(string name);

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of bytes to read.</param>
		/// <returns>Value in the stream.</returns>
		public abstract byte[] ReadBytes(string name, int count);

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract byte[] ReadBytes(string name);

		/// <summary>
		/// Function to read a byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract byte ReadByte(string name);

		/// <summary>
		/// Function to read a signed byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract sbyte ReadSByte(string name);

		/// <summary>
		/// Function to read a short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract short ReadInt16(string name);

		/// <summary>
		/// Function to read an unsigned short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract ushort ReadUInt16(string name);

		/// <summary>
		/// Function to read a double value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract double ReadDouble(string name);

		/// <summary>
		/// Function to read a floating point value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract float ReadSingle(string name);

		/// <summary>
		/// Function to read an integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract int ReadInt32(string name);

		/// <summary>
		/// Function to read an unsigned integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract uint ReadUInt32(string name);

		/// <summary>
		/// Function to read a long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract long ReadInt64(string name);

		/// <summary>
		/// Function to read an unsigned long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract ulong ReadUInt64(string name);

		/// <summary>
		/// Function to read an array of character values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of characters to read.</param>
		/// <returns>Value in the stream.</returns>
		public abstract char[] ReadChars(string name, int count);

		/// <summary>
		/// Function to read a character value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract char ReadChar(string name);

		/// <summary>
		/// Function to read a string values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public abstract string ReadString(string name);

		/// <summary>
		/// Function to read an attribute for an item.
		/// </summary>
		/// <param name="item">Item that contains the attribute.</param>
		/// <param name="attributeName">Attribute to read.</param>
		/// <returns>Value for the attribute.</returns>
		public virtual string ReadAttribute(string item, string attributeName)
		{
			return string.Empty;
		}

		/// <summary>
		/// Function to move to a field within the data.
		/// </summary>
		/// <param name="name">Name of the field.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public virtual bool GotoField(string name)
		{
			return true;
		}

		/// <summary>
		/// Function to perform serialization.
		/// </summary>
		public abstract void Serialize();

		/// <summary>
		/// Function to perform deserialization.
		/// </summary>
		public abstract void Deserialize();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serialObject">Object to serialize.</param>
		/// <param name="stream">Existing stream to use.</param>
		protected Serializer(ISerializable serialObject, Stream stream)
		{
			_serialObject = serialObject;
			_stream = stream;
			_parameters = new SortedList();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if ((_stream != null) && (!DontCloseStream))
					_stream.Close();
			}

			_stream = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
