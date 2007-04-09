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
	/// Serializer error codes.
	/// </summary>
	public enum SerializerErrors
	{
		/// <summary>Reader/writer is already open.</summary>
		SerializerAlreadyOpen = 0x7FFF0002,
		/// <summary>Reader/write is not open.</summary>
		SerializerNotOpen = 0x7FFF0003,
	}

	/// <summary>
	/// Base exception for serializer exceptions.
	/// </summary>
	public abstract class SerializerException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public SerializerException(string message, SerializerErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	/// Serializer is not open exception.
	/// </summary>
	public class SerializerNotOpenException 
		: SerializerException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public SerializerNotOpenException(string description, Exception ex)
			: base(description, SerializerErrors.SerializerNotOpen, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public SerializerNotOpenException(Exception ex)
			: this("The stream for the serializer has not been opened yet.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Serializer is already open exception.
	/// </summary>
	public class SerializerAlreadyOpenException
		: SerializerException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public SerializerAlreadyOpenException(string description, Exception ex)
			: base(description, SerializerErrors.SerializerAlreadyOpen, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public SerializerAlreadyOpenException(Exception ex)
			: this("Cannot serialize the object.  No data was returned from serializer.", ex)
		{
		}
		#endregion
	}
}
