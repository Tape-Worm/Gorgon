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
// Created: Sunday, January 07, 2007 3:29:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpUtilities.IO;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing a binary serializer.
	/// </summary>
	public class BinarySerializer
		: Serializer<BinaryReaderEx, BinaryWriterEx>, ISerializer 
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
			if (_stream == null)
				throw new SerializerNotOpenException(null);

			if (_writer != null)
				throw new SerializerAlreadyOpenException(null);
			
			// Use the stream for writing binary data.
			_writer = new BinaryWriterEx(_stream, DontCloseStream);			
			_serialObject.WriteData(this);
		}

		/// <summary>
		/// Function to deserialize an object.
		/// </summary>
		public override void Deserialize()
		{
			if (_stream == null)
				throw new SerializerNotOpenException(null);

			if (_reader != null)
				throw new SerializerAlreadyOpenException(null);

			// Use the stream for reading binary data.
			_reader = new BinaryReaderEx(_stream, DontCloseStream);
			_serialObject.ReadData(this);
		}

		/// <summary>
		/// Function to write a value to the serialization stream.
		/// </summary>
		/// <param name="value">Value to write.</param>
		/// <param name="name">Name of the value to write.</param>
		/// <typeparam name="T">Type of value to write.</typeparam>
		public override void Write<T>(string name, T value)
		{
			// We don't care about the name, just write the value.
			_writer.GetType().InvokeMember("Write", System.Reflection.BindingFlags.InvokeMethod, null, _writer, new object[] { value });			
		}

		/// <summary>
		/// Function to read a value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		/// <typeparam name="T">Type of value to read.</typeparam>
		public override T Read<T>(string name)
		{	
			// Read primitive types.
			switch (typeof(T).Name.ToLower())
			{
				case "string":
					return (T)(object)_reader.ReadString();
				case "bool":
				case "boolean":
					return (T)(object)_reader.ReadBoolean();					
				case "byte":
					return (T)(object)_reader.ReadByte();
				case "ulong":
				case "uint64":
					return (T)(object)_reader.ReadUInt64();
				case "uint":
				case "uint32":
					return (T)(object)_reader.ReadUInt32();
				case "ushort":
				case "uint16":
					return (T)(object)_reader.ReadInt16();
				case "long":
				case "int64":
					return (T)(object)_reader.ReadInt64();
				case "int":
				case "integer":
				case "int32":
					return (T)(object)_reader.ReadInt32();
				case "short":
				case "int16":
					return (T)(object)_reader.ReadInt16();
				case "single":
				case "float":
					return (T)(object)_reader.ReadSingle();
				case "double":
					return (T)(object)_reader.ReadDouble();
				case "decimal":
					return (T)(object)_reader.ReadDecimal();
				case "char":
					return (T)(object)_reader.ReadChar();
				case "sbyte":
					return (T)(object)_reader.ReadSByte();
			}

			throw new InvalidResourceException("Unknown data type: " + typeof(T).Name, null);
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
			return Read<string>(attributeName);
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
