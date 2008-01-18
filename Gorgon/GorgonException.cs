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
	#endregion	

	/// <summary>
	/// Base exception class for Gorgon.
	/// </summary>
	/// <remarks>All exceptions for Gorgon will be derived from this.</remarks>
	public class GorgonException
		: SharpException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Message for the exception.</param>
		/// <param name="ex">Exception that spawned this exception.</param>
		public GorgonException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Message for the exception.</param>
		public GorgonException(string message)
			: this(message, null)
		{
		}
		#endregion
	}
	
	/// <summary>
	///	Library not initialized.
	/// </summary>
	public class NotInitializedException 
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="ex">Exception that spawned this exception.</param>
		public NotInitializedException(Exception ex)
			: base("The library was not initialized.\nYou must call Initialize() first.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>		
		public NotInitializedException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot enumerate.
	/// </summary>
	public class CannotEnumerateException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="enumObject">Object/device that is being enumerated.</param>
		/// <param name="ex">Source exception.</param>
		public CannotEnumerateException(string enumObject, Exception ex)
			: base("Cannot enumerate the " + enumObject + ".", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="enumObject">Object/device that is being enumerated.</param>
		public CannotEnumerateException(string enumObject)
			: this(enumObject, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot create exception.
	/// </summary>
	public class CannotCreateException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreateException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		public CannotCreateException(string message)
			: base(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreateException(string name, Type resourceType, Exception ex)
			: base("Could not create " + resourceType.Name + (name == string.Empty ? string.Empty : " '" + name + "'") + ".", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		public CannotCreateException(string name, Type resourceType)
			: base("Could not create " + resourceType.Name + (name == string.Empty ? string.Empty : " '" + name + "'") + ".", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot update exception.
	/// </summary>
	public class CannotUpdateException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotUpdateException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		public CannotUpdateException(string message)
			: base(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotUpdateException(string name, Type resourceType, Exception ex)
			: base("Could not update the " + resourceType.Name + " '" + name + "'.", ex)
		{
		}


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		public CannotUpdateException(string name, Type resourceType)
			: base("Could not update the " + resourceType.Name + " '" + name + "'.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot load exception.
	/// </summary>
	public class CannotLoadException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLoadException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLoadException(string filename, Type resourceType, Exception ex)
			: base("Could not load " + resourceType.Name + " '" + filename + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		public CannotLoadException(string message)
			: base(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		public CannotLoadException(string filename, Type resourceType)
			: base("Could not load " + resourceType.Name + " '" + filename + "'.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot save exception.
	/// </summary>
	public class CannotSaveException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotSaveException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotSaveException(string filename, Type resourceType, Exception ex)
			: base("Could not save " + resourceType.Name + " '" + filename + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		public CannotSaveException(string message)
			: base(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		public CannotSaveException(string filename, Type resourceType)
			: base("Could not save " + resourceType.Name + " '" + filename + "'.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Image buffer format not supported exception.
	/// </summary>
	public class FormatNotSupportedException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public FormatNotSupportedException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public FormatNotSupportedException(Exception ex)
			: this("The selected buffer format is not supported by the hardware.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		public FormatNotSupportedException(string message)
			: this(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FormatNotSupportedException()
			: this("The selected buffer format is not supported by the hardware.", null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="format">Format that caused the exception.</param>
		/// <param name="ex">Source exception.</param>
		public FormatNotSupportedException(BackBufferFormats format, Exception ex)
			: this("The backbuffer format '" + format.ToString() + "' is not supported by the hardware.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="format">Format that caused the exception.</param>
		/// <param name="ex">Source exception.</param>
		public FormatNotSupportedException(GorgonLibrary.Graphics.ImageBufferFormats format, Exception ex)
			: this("The format '" + format.ToString() + "' is not supported by the hardware.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="format">Format that caused the exception.</param>
		public FormatNotSupportedException(BackBufferFormats format)
			: this("The backbuffer format '" + format.ToString() + "' is not supported by the hardware.", null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="format">Format that caused the exception.</param>
		public FormatNotSupportedException(GorgonLibrary.Graphics.ImageBufferFormats format)
			: this("The format '" + format.ToString() + "' is not supported by the hardware.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Not locked exception.
	/// </summary>
	public class NotLockedException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public NotLockedException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of object.</param>
		/// <param name="ex">Source exception.</param>
		public NotLockedException(Type type, Exception ex)
			: this("The " + type.Name + " is not locked.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>		
		public NotLockedException(string message)
			: this(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of object.</param>		
		public NotLockedException(Type type)
			: this("The " + type.Name + " is not locked.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot lock exception.
	/// </summary>
	public class CannotLockException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLockException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of object.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLockException(Type type, Exception ex)
			: this("Could not lock the " + type.Name + ".", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		public CannotLockException(string message)
			: base(message, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of object.</param>
		public CannotLockException(Type type)
			: this("Could not lock the " + type.Name + ".", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot read exception.
	/// </summary>
	public class CannotReadException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotReadException(string message, Exception ex)
			: base(message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		public CannotReadException(string message)
			: base(message, null)
		{
		}
		#endregion
	}
}
