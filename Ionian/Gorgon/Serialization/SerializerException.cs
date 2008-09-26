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
// Created: Saturday, March 25, 2006 9:37:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

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
