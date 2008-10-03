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
// Created: Sunday, January 07, 2007 3:17:39 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing an XML serializer.
	/// </summary>
	public class XMLSerializer
		: Serializer
	{
		#region Variables.
		private XmlWriter _writer = null;			// XML stream writer object.
		private XmlReader _reader = null;			// XML stream reader object.
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
			XmlWriterSettings settings = new XmlWriterSettings();		// XML Settings.

			if (SerializationStream == null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer stream is not open.");

			if (_writer != null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer is already open for writing.");
			
			// Use the stream for writing binary data.
			settings.CloseOutput = !DontCloseStream;
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;
			
			// Create xml writer.
			_writer = XmlWriter.Create(SerializationStream, settings);
			
			// Write document start.
			_writer.WriteStartDocument(false);			
			SerializationObject.WriteData(this);
			_writer.WriteEndDocument();			
		}

		/// <summary>
		/// Function to deserialize an object.
		/// </summary>
		public override void Deserialize()
		{
			XmlReaderSettings settings = new XmlReaderSettings();		// XML Settings.

			if (SerializationStream == null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer stream is not open.");

			if (_reader != null)
				throw new GorgonException(GorgonErrors.CannotSave, "The serializer is already open for reading.");

			// Use the stream for reading binary data.
			settings.CloseInput = !DontCloseStream;
			settings.ConformanceLevel = ConformanceLevel.Fragment;			

			// Create xml reader.
			_reader = XmlReader.Create(SerializationStream, settings);

			// Read document.
			SerializationObject.ReadData(this);
		}

		/// <summary>
		/// Function to write a comment.
		/// </summary>
		/// <param name="comment">Comment to write.</param>
		public override void WriteComment(string comment)
		{
			_writer.WriteComment(comment);
		}

		/// <summary>
		/// Function to write the start of a group of data.
		/// </summary>
		/// <param name="groupName">Name of the group.</param>
		public override void WriteGroupBegin(string groupName)
		{
			_writer.WriteStartElement(groupName);
		}

		/// <summary>
		/// Function to write the end of the group started with WriteGroupBegin().
		/// </summary>
		public override void WriteGroupEnd()
		{
			_writer.WriteEndElement();
		}

		/// <summary>
		/// Function to write a string value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, string value)
		{
			_writer.WriteElementString(name, value);
		}

		/// <summary>
		/// Function to write a character value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, char value)
		{
			_writer.WriteElementString(name, value.ToString());
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

			_writer.WriteStartElement(name);
			_writer.WriteAttributeString("Start", start.ToString());
			_writer.WriteAttributeString("Count", count.ToString());

			for (int i = start; i < start + count; i++)
				_writer.WriteCharEntity(values[i]);

			_writer.WriteEndElement();
		}

		/// <summary>
		/// Function to write an unsigned long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, ulong value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write a long value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, long value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write an unsigned integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, uint value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write an integer value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, int value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, float value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write a floating point value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, double value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write an unsigned short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, ushort value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write a short value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, short value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write a signed byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, sbyte value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to write a byte value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, byte value)
		{
			_writer.WriteElementString(name, value.ToString());
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

			_writer.WriteStartElement(name);
			_writer.WriteAttributeString("Start", start.ToString());
			_writer.WriteAttributeString("Count", count.ToString());
			_writer.WriteBase64(values, start, count);
			_writer.WriteEndElement();
		}

		/// <summary>
		/// Function to write a boolean value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		public override void Write(string name, bool value)
		{
			_writer.WriteElementString(name, value.ToString());
		}

		/// <summary>
		/// Function to read an attribute for an item.
		/// </summary>
		/// <param name="item">Item that contains the attribute.</param>
		/// <param name="attributeName">Attribute to read.</param>
		/// <returns>Value for the attribute.</returns>
		public override string ReadAttribute(string item, string attributeName)
		{
			// Go to the element.
			if (!_reader.ReadToFollowing(item))
				throw new MissingFieldException("The requested element '" + item + "' does not match the data '" + _reader.Name + "' in the sequence.");

			if (!_reader.MoveToAttribute(attributeName))
				throw new MissingFieldException("The requested attribute '" + attributeName + "' does not match the data '" + _reader.Name + "' in the sequence.");

			// Get the attribute value.
			return _reader.Value;
		}


		/// <summary>
		/// Function to read a boolean value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override bool ReadBool(string name)
		{
			string value = string.Empty;		// Value.

			// Read until we hit a content element.
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			value = _reader.ReadElementContentAsString();

			return string.Compare(value, "true", true) == 0;
		}

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of bytes to read.</param>
		/// <returns>Value in the stream.</returns>
		public override byte[] ReadBytes(string name, int count)
		{
			byte[] buffer = new byte[count];		// Buffer to hold the data.
			int totalCount = 0;						// Total size.

			// Read until we hit a content element.
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence."); 
			totalCount = Convert.ToInt32(_reader.GetAttribute("Count"));
			if (count > totalCount)
				throw new ArgumentOutOfRangeException("count");

			_reader.ReadElementContentAsBase64(buffer, 0, count);
			return buffer;
		}

		/// <summary>
		/// Function to read an array of byte values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override byte[] ReadBytes(string name)
		{
			throw new NotImplementedException("This overload of ReadBytes does not apply to the XML serializer.");
		}

		/// <summary>
		/// Function to read a byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override byte ReadByte(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return (byte)_reader.ReadElementContentAsInt();
		}

		/// <summary>
		/// Function to read a signed byte value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override sbyte ReadSByte(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return (sbyte)_reader.ReadElementContentAsInt();
		}

		/// <summary>
		/// Function to read a short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override short ReadInt16(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return (short)_reader.ReadElementContentAsInt();
		}

		/// <summary>
		/// Function to read an unsigned short value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override ushort ReadUInt16(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return (ushort)_reader.ReadElementContentAsInt();
		}

		/// <summary>
		/// Function to read a double value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override double ReadDouble(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return _reader.ReadElementContentAsDouble();
		}

		/// <summary>
		/// Function to read a floating point value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override float ReadSingle(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return _reader.ReadElementContentAsFloat();
		}

		/// <summary>
		/// Function to read an integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override int ReadInt32(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return _reader.ReadElementContentAsInt();
		}

		/// <summary>
		/// Function to read an unsigned integer value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override uint ReadUInt32(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return (uint)_reader.ReadElementContentAsInt();
		}

		/// <summary>
		/// Function to read a long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override long ReadInt64(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return _reader.ReadElementContentAsLong();
		}

		/// <summary>
		/// Function to read an unsigned long value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override ulong ReadUInt64(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return (ulong)_reader.ReadElementContentAsLong();
		}

		/// <summary>
		/// Function to read an array of character values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <param name="count">The count of characters to read.</param>
		/// <returns>Value in the stream.</returns>
		public override char[] ReadChars(string name, int count)
		{
			int totalCount = 0;				// Count in array.
			char[] chars;					// Characters.
			string value = string.Empty;	// Element value.

			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			totalCount = Convert.ToInt32(_reader.GetAttribute("Count"));

			if (count > totalCount)
				throw new ArgumentOutOfRangeException("count");

			// Read the characters as a string.
			value = _reader.ReadElementContentAsString();
			chars = new char[count];
			value.CopyTo(0, chars, 0, count);

			return chars;
		}

		/// <summary>
		/// Function to read a character value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override char ReadChar(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return Convert.ToChar(_reader.ReadElementContentAsString());
		}

		/// <summary>
		/// Function to read a string values from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		public override string ReadString(string name)
		{
			if ((!_reader.ReadToFollowing(name)) || (_reader.EOF))
				throw new MissingFieldException("The requested element '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.");
			return _reader.ReadElementContentAsString();
		}

		/// <summary>
		/// Function to move to a field within the data.
		/// </summary>
		/// <param name="name">Name of the field.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public override bool GotoField(string name)
		{
			return _reader.ReadToFollowing(name);			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serialObject">Object to serialize.</param>
		/// <param name="stream">Stream to write or read data through.</param>
		public XMLSerializer(ISerializable serialObject, Stream stream) 
			: base(serialObject, stream)
		{
		}
		#endregion
	}
}
