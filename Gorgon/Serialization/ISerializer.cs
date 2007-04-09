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
// Created: Thursday, August 10, 2006 11:47:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Interface for serializer objects.
	/// </summary>
	public interface ISerializer
		: IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the parameter list interface.
		/// </summary>
		SerializationParameterList Parameters
		{
			get;
		}

		/// <summary>
		/// Property to return whether the reader/writer should close the underlying stream or not.
		/// </summary>
		bool DontCloseStream
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the stream being used.
		/// </summary>
		Stream Stream
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform serialization.
		/// </summary>
		void Serialize();
		/// <summary>
		/// Function to perform deserialization.
		/// </summary>
		void Deserialize();

		/// <summary>
		/// Function to write a comment.
		/// </summary>
		/// <param name="comment">Comment to write.</param>
		void WriteComment(string comment);

		/// <summary>
		/// Function to write the start of a group of data.
		/// </summary>
		/// <param name="groupName">Name of the group.</param>
		void WriteGroupBegin(string groupName);

		/// <summary>
		/// Function to write the end of the group started with WriteGroupBegin().
		/// </summary>
		void WriteGroupEnd();

		/// <summary>
		/// Function to write a value to the serialization stream.
		/// </summary>
		/// <typeparam name="T">Type of value to write.</typeparam>
		/// <param name="name">Name of the value to write.</param>
		/// <param name="value">Value to write.</param>
		void Write<T>(string name, T value);

		/// <summary>
		/// Function to read a value from the stream.
		/// </summary>
		/// <param name="name">Name of the data.</param>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <returns>Value in the stream.</returns>
		T Read<T>(string name);

		/// <summary>
		/// Function to read an attribute for an item.
		/// </summary>
		/// <param name="field">Item that contains the attribute.</param>
		/// <param name="attributeName">Attribute to read.</param>
		/// <returns>Value for the attribute.</returns>
		string ReadAttribute(string field, string attributeName);

		/// <summary>
		/// Function to move to a field within the data.
		/// </summary>
		/// <param name="name">Name of the field.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		bool GotoField(string name);
		#endregion
	}
}
