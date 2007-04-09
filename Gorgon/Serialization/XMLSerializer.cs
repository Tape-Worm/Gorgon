#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
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
		: Serializer<XmlReader, XmlWriter>
	{
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

			if (_stream == null)
				throw new SerializerNotOpenException(null);

			if (_writer != null)
				throw new SerializerAlreadyOpenException(null);
			
			// Use the stream for writing binary data.
			settings.CloseOutput = !DontCloseStream;
			settings.ConformanceLevel = ConformanceLevel.Document;
			settings.Encoding = Encoding.UTF8;
			settings.Indent = true;
			
			// Create xml writer.
			_writer = XmlWriter.Create(_stream, settings);
			
			// Write document start.
			_writer.WriteStartDocument(false);			
			_serialObject.WriteData(this);
			_writer.WriteEndDocument();			
		}

		/// <summary>
		/// Function to deserialize an object.
		/// </summary>
		public override void Deserialize()
		{
			XmlReaderSettings settings = new XmlReaderSettings();		// XML Settings.

			if (_stream == null)
				throw new SerializerNotOpenException(null);

			if (_reader != null)
				throw new SerializerAlreadyOpenException(null);

			// Use the stream for reading binary data.
			settings.CloseInput = !DontCloseStream;
			settings.ConformanceLevel = ConformanceLevel.Fragment;			

			// Create xml reader.
			_reader = XmlReader.Create(_stream, settings);

			// Read document.
			_serialObject.ReadData(this);
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
		/// Function to write a value to the serialization stream.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="name">Name of the value to write.</param>
		/// <typeparam name="T">Type of value to write.</typeparam>
		public override void Write<T>(string name, T value)
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
				throw new InvalidResourceException("The requested element '" + item + "' does not match the data '" + _reader.Name + "' in the sequence.", null);			

			if (!_reader.MoveToAttribute(attributeName))
				throw new InvalidResourceException("The requested attribute '" + attributeName + "' does not match the data '" + _reader.Name + "' in the sequence.", null);

			// Get the attribute value.
			return _reader.Value;
		}

		/// <summary>
		/// Function to read a value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		/// <typeparam name="T">Type of value to read.</typeparam>
		public override T Read<T>(string name)
		{
			Type type = typeof(T);						// Type of item.
			string typeName = type.Name.ToLower();		// Name of the type.

			// Read until we hit a content element.
			_reader.ReadToFollowing(name);

			if ((_reader.Name.ToLower() != name.ToLower()) || (_reader.EOF))
				throw new InvalidResourceException("The requested data '" + name + "' does not match the data '" + _reader.Name + "' in the sequence.", null);

			// Read primitive types.
			switch (typeName)
			{
				case "string":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsString(), type);
				case "bool":
				case "boolean":
					return (T)Convert.ChangeType((_reader.ReadElementContentAsString().ToLower() == "true"), type);
				case "byte":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsInt(), type);
				case "ulong":
				case "uint64":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsLong(), type);
				case "uint":
				case "uint32":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsInt(), type);
				case "ushort":
				case "uint16":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsInt(), type);
				case "long":
				case "int64":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsLong(), type);
				case "int":
				case "integer":
				case "int32":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsInt(), type);
				case "short":
				case "int16":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsInt(), type);
				case "single":
				case "float":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsFloat(), type);
				case "double":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsDouble(), type);
				case "decimal":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsDecimal(), type);
				case "char":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsInt(), type);
				case "sbyte":
					return (T)Convert.ChangeType(_reader.ReadElementContentAsInt(), type);
				default:
					throw new InvalidResourceException("Invalid data type: " + typeName, null);
			}
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
