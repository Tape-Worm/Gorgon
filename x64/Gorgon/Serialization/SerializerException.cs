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
// Created: Saturday, March 25, 2006 9:37:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Serializer is not open exception.
	/// </summary>
	public class SerializerNotOpenException 
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public SerializerNotOpenException(Exception ex)
			: base("The stream for the serializer has not been opened yet.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public SerializerNotOpenException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Serializer is already open exception.
	/// </summary>
	public class SerializerAlreadyOpenException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public SerializerAlreadyOpenException(Exception ex)
			: base("The serializer is already open.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public SerializerAlreadyOpenException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Serializer cannot serialize the object.
	/// </summary>
	public class SerializerCannotSerializeException
		: GorgonException 
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serializerTypeName">Type of serializer.</param>
		/// <param name="objectType">Type of object.</param>
		/// <param name="errors">List of miscellaneous errors that may have caused the exception.</param>
		/// <param name="ex">Source exception.</param>
		public SerializerCannotSerializeException(string serializerTypeName, string objectType, string errors, Exception ex)
			: base("The " + serializerTypeName + " cannot serialize the " + objectType + "." + (!string.IsNullOrEmpty(errors) ? "\n" + errors : string.Empty), ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serializerTypeName">Type of serializer.</param>
		/// <param name="objectType">Type of object.</param>
		/// <param name="ex">Source exception.</param>
		public SerializerCannotSerializeException(string serializerTypeName, string objectType, Exception ex)
			: this(serializerTypeName, objectType, string.Empty, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serializerTypeName">Type of serializer.</param>
		/// <param name="objectType">Type of object.</param>
		public SerializerCannotSerializeException(string serializerTypeName, string objectType)
			: this(serializerTypeName, objectType, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Serializer cannot de serialize the object.
	/// </summary>
	public class SerializerCannotDeserializeException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serializerTypeName">Type of serializer.</param>
		/// <param name="objectType">Type of object.</param>
		/// <param name="errors">List of miscellaneous errors that may have caused the exception.</param>
		/// <param name="ex">Source exception.</param>
		public SerializerCannotDeserializeException(string serializerTypeName, string objectType, string errors, Exception ex)
			: base("The " + serializerTypeName + " cannot de-serialize the " + objectType + "." + (!string.IsNullOrEmpty(errors) ? "\n" + errors : string.Empty), ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serializerTypeName">Type of serializer.</param>
		/// <param name="objectType">Type of object.</param>
		/// <param name="ex">Source exception.</param>
		public SerializerCannotDeserializeException(string serializerTypeName, string objectType, Exception ex)
			: this(serializerTypeName, objectType, string.Empty, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="serializerTypeName">Type of serializer.</param>
		/// <param name="objectType">Type of object.</param>
		public SerializerCannotDeserializeException(string serializerTypeName, string objectType)
			: this(serializerTypeName, objectType, null)
		{
		}
		#endregion
	}
}
