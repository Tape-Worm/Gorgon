#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Thursday, August 10, 2006 9:13:40 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Abstract object representing a serialization object.
	/// </summary>
	/// <typeparam name="R">Reader type.</typeparam>
	/// <typeparam name="W">Writer type.</typeparam>
	public abstract class Serializer<R, W>
		: ISerializer
	{
		#region Variables.
		private bool _dontCloseStream = false;			// Flag to indicate whether the underlying stream should close when the reader/writer is closed.

		/// <summary>Object to serialize.</summary>
		protected ISerializable _serialObject = null;
		/// <summary>Stream to use.</summary>
		protected Stream _stream = null;
		/// <summary>List of parameters for the serializer.</summary>
		protected SerializationParameterList _parameters;
		/// <summary>Reader object.</summary>
		protected R _reader = default(R);
		/// <summary>Writer object.</summary>
		protected W _writer = default(W);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the reader object for this serializer.
		/// </summary>
		public virtual R Reader
		{
			get
			{
				return _reader;
			}
		}

		/// <summary>
		/// Property to return the writer object for this serializer.
		/// </summary>
		public virtual W Writer
		{
			get
			{
				return _writer;
			}
		}
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
			_parameters = new SerializationParameterList();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~Serializer()
		{
			Dispose(false);
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

		#region ISerializer Members
		#region Properties.
		/// <summary>
		/// Property to return the stream being used.
		/// </summary>
		public Stream Stream
		{
			get
			{
				return _stream;
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
		public SerializationParameterList Parameters
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
		/// Function to write a value to the serialization stream.
		/// </summary>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		/// <typeparam name="T">Type of value to write.</typeparam>
		public virtual void Write<T>(string name, T value)
		{
			throw new NotImplementedException(NotImplementedTypes.Method, "ShaderSerializer.Write<T>(...)", null);
		}

		/// <summary>
		/// Function to read a value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <returns>Value in the stream.</returns>
		/// <typeparam name="T">Type of value to read.</typeparam>
		public virtual T Read<T>(string name)
		{
			throw new NotImplementedException(NotImplementedTypes.Method, "ShaderSerializer.Read<T>(...)", null);
		}

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
		#endregion
	}
}
