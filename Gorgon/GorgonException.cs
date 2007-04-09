#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Wednesday, April 27, 2005 10:30:08 AM
// 
#endregion

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SharpUtilities;

namespace GorgonLibrary
{
	#region Enumerator.
	/// <summary>
	/// Enumeration for types of members that are throwing the not implemented exception.
	/// </summary>
	public enum NotImplementedTypes
	{
		/// <summary>
		/// Property is not implemented.
		/// </summary>
		Property = 0,
		/// <summary>
		/// Method is not implemented.
		/// </summary>
		Method = 1
	}

	/// <summary>
	/// Enumerator for generic error codes. 
	/// </summary>
	public enum GenericErrors 
	{
		/// <summary>This functionality has not been implemented yet.</summary>
		NotImplemented			=	0x7FFF0003,
		/// <summary>Buffer read/write caused a buffer overflow.</summary>
		BufferOverflowed		=	0x7FFF0004,
		/// <summary>No parent for this object.</summary>
		InvalidParent			=	0x7FFF0006,
		/// <summary>Operation is illegal while in debug mode.</summary>
		IllegalOperation		=	0x7FFF0009,
		/// <summary>Initialize() function was not called.</summary>
		NotInitialized			=	0x7FFF000A,
		/// <summary>Invalid filename.</summary>
		InvalidFilename			=	0x7FFF000B
	}
	#endregion	

	/// <summary>
	/// Base exception class for Gorgon.
	/// </summary>
	/// <remarks>All exceptions for Gorgon will be derived from this.</remarks>
	public abstract class GorgonException
		: SharpException
	{
		#region Variables.
		/// <summary>Type for error code.</summary>
		protected Type _errorCodeType = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the error code as a string.
		/// </summary>		
		public string ErrorName
		{
			get
			{
				if (_errorCodeType == null)
					return string.Empty;

				return _errorCodeType.Name + "." + Enum.GetName(_errorCodeType,HResult);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Message for the exception.</param>
		/// <param name="code">Error code associated with the exception.</param>
		/// <param name="ex">Exception that spawned this exception.</param>
		protected GorgonException(string message, int code, Exception ex)
			: base((ex == null ? message : message + "\nSource exception: (" + ex.GetType().Name + ")\n" + ex.Message), ex)
		{
			HResult = (int)code;
		}
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Message for the exception.</param>
		/// <param name="code">Error code associated with the exception.</param>
		/// <param name="ex">Exception that spawned this exception.</param>
		public GorgonException(string message, GenericErrors code, Exception ex)
			: this(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}		
		#endregion
	}
	
	/// <summary>
	///	No parent form exception.
	/// </summary>
	public class InvalidParentFormException 
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="description">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidParentFormException(string description, Exception ex)
			: base(description, GenericErrors.InvalidParent, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InvalidParentFormException(Exception ex)
			: this("Cannot bind to the requested control because it has no parent form.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Functionality not implemented exception.
	/// </summary>
	public class NotImplementedException 
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="description">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public NotImplementedException(string description, Exception ex)
			: base(description, GenericErrors.NotImplemented, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="memberType">Type of member that is throwing the exception.</param>
		/// <param name="memberName">Name of the member.</param>
		/// <param name="ex">Source exception.</param>
		public NotImplementedException(NotImplementedTypes memberType, string memberName, Exception ex)
			: this("The " + memberType.ToString() + ": '" + memberName + "' is not implemented within this context.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Buffer overflowed exception.
	/// </summary>
	public class BufferOverflowedException 
		: GorgonException 
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="description">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public BufferOverflowedException(string description, Exception ex)
			: base(description, GenericErrors.BufferOverflowed, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="bufferIndex">Index within the buffer that caused the overflow.</param>
		/// <param name="read">TRUE if reading, FALSE if writing.</param>
		/// <param name="ex">Source exception.</param>
		public BufferOverflowedException(int bufferIndex, bool read, Exception ex)
			: this("The " + (read ? "read" : "write") + " operation caused a buffer overflow at " + bufferIndex.ToString() + ".", ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Invalid file name exception.
	/// </summary>
	public class InvalidFilenameException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="description">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidFilenameException(string description, Exception ex)
			: base(description, GenericErrors.InvalidFilename, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InvalidFilenameException(Exception ex)
			: this("The filename was invalid, it is probably empty or NULL.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Illegal operation exception.
	/// </summary>
	public class IllegalOperationException 
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="description">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public IllegalOperationException(string description, Exception ex)
			: base(description, GenericErrors.IllegalOperation, ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Library not initialized exception.
	/// </summary>
	public class NotInitializedException 
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="description">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public NotInitializedException(string description, Exception ex)
			: base(description, GenericErrors.NotInitialized, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>		
		public NotInitializedException(Exception ex)
			: this("The library was not initialized.\nYou must call Initialize() first.", ex)
		{
		}
		#endregion
	}
}
