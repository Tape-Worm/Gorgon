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
// Created: Tuesday, October 31, 2006 12:00:55 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Graphics;

namespace GorgonLibrary
{
	/// <summary>
	/// Enumeration containing resource related errors.
	/// </summary>
	public enum ResourceErrors
	{
		/// <summary>Invalid resource object.</summary>
		InvalidResource = 0x7FFF0001,
		/// <summary>Could not load the resource.</summary>
		CannotLoad = 0x7FFF0002,
		/// <summary>Could not save the resource.</summary>
		CannotSave = 0x7FFF0003,
		/// <summary>Could not create the resource.</summary>
		CannotCreate = 0x7FFF0004,
		/// <summary>Format is not supported by the hardware.</summary>
		FormatNotSupported = 0x7FFF0006,
		/// <summary>Resource already locked.</summary>
		AlreadyLocked = 0x7FFF0007,
		/// <summary>Resource not locked.</summary>
		NotLocked = 0x7FFF0008,
		/// <summary>Cannot update resource.</summary>
		CannotUpdate = 0x7FFF000A,
		/// <summary>Cannot lock the resource.</summary>
		CannotLock = 0x7FFF000E,
		/// <summary>Priority assignment is already in use.</summary>
		InvalidPriority = 0x7FFF000F
	}

	/// <summary>
	/// Base class for resource based exceptions.
	/// </summary>
	public abstract class ResourceException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		protected ResourceException(string message, int code, Exception ex)
			: base(message, code, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public ResourceException(string message, ResourceErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	/// Invalid resource object exception.
	/// </summary>
	public class InvalidResourceException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidResourceException(string message, Exception ex)
			: base(message, ResourceErrors.InvalidResource, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="objectName">Name of the resource.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidResourceException(string objectName, Type resourceType, Exception ex)
			: this("The " + resourceType.Name + " '" + objectName + "' contains invalid data.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot create exception.
	/// </summary>
	public class CannotCreateException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreateException(string message, Exception ex)
			: base(message, ResourceErrors.CannotCreate, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreateException(string name, Type resourceType, Exception ex)
			: this("Could not create " + resourceType.Name + (name == string.Empty ? string.Empty : " '" + name + "'") + ".", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot update exception.
	/// </summary>
	public class CannotUpdateException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotUpdateException(string message, Exception ex)
			: base(message, ResourceErrors.CannotUpdate, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotUpdateException(string name, Type resourceType, Exception ex)
			: this("Could not update the " + resourceType.Name + " '" + name + "'.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot load exception.
	/// </summary>
	public class CannotLoadException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLoadException(string message, Exception ex)
			: base(message, ResourceErrors.CannotLoad, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLoadException(string filename, Type resourceType, Exception ex)
			: this("Could not load " + resourceType.Name + " '" + filename + "'.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot save exception.
	/// </summary>
	public class CannotSaveException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotSaveException(string message, Exception ex)
			: base(message, ResourceErrors.CannotSave, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename of the object.</param>
		/// <param name="resourceType">Type of resource.</param>
		/// <param name="ex">Source exception.</param>
		public CannotSaveException(string filename, Type resourceType, Exception ex)
			: this("Could not save " + resourceType.Name + " '" + filename + "'.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Format not supported exception.
	/// </summary>
	public class FormatNotSupportedException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public FormatNotSupportedException(string message, Exception ex)
			: base(message, ResourceErrors.FormatNotSupported, ex)
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
		public FormatNotSupportedException(ImageBufferFormats format, Exception ex)
			: this("The format '" + format.ToString() + "' is not supported by the hardware.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Already locked exception.
	/// </summary>
	public class AlreadyLockedException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public AlreadyLockedException(string message, Exception ex)
			: base(message, ResourceErrors.AlreadyLocked, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of object.</param>
		/// <param name="ex">Source exception.</param>
		public AlreadyLockedException(Type type, Exception ex)
			: this("The " + type.Name + " is already locked.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Not locked exception.
	/// </summary>
	public class NotLockedException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public NotLockedException(string message, Exception ex)
			: base(message, ResourceErrors.NotLocked, ex)
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
		#endregion
	}

	/// <summary>
	/// Cannot lock exception.
	/// </summary>
	public class CannotLockException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLockException(string message, Exception ex)
			: base(message, ResourceErrors.CannotLock, ex)
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
		#endregion
	}

	/// <summary>
	/// Invalid priority exception.
	/// </summary>
	public class InvalidPriorityException 
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidPriorityException(string message, Exception ex)
			: base(message, ResourceErrors.InvalidPriority, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="priority">Priority number that was assigned.</param>
		/// <param name="type">Type of object.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidPriorityException(int priority, Type type, Exception ex)
			: this("There is already a " + type.Name + " with a priority of " + priority.ToString() + ".", ex)
		{
		}
		#endregion
	}
}
