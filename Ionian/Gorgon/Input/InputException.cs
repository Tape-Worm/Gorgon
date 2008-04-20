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
// Created: Saturday, September 30, 2006 9:57:44 PM
// 
#endregion

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	///	Cannot capture raw input.
	/// </summary>
	public class InputCannotCaptureRawDataException 
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="ex">Source exception.</param>
		public InputCannotCaptureRawDataException(Exception ex)
			: base("Cannot capture the raw input data.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>		
		public InputCannotCaptureRawDataException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	///	Cannot bind mouse.
	/// </summary>
	public class InputCannotBindMouseException
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InputCannotBindMouseException(Exception ex)
			: base("Failed to bind the mouse to the window.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public InputCannotBindMouseException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	///	Cannot bind keyboard.
	/// </summary>
	public class InputCannotBindKeyboardException 
		: GorgonException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InputCannotBindKeyboardException(Exception ex)
			: base("Failed to bind the keyboard to the window.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public InputCannotBindKeyboardException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Input plug-in is an invalid type.
	/// </summary>
	public class InputPlugInInvalidType
		: GorgonException 
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="pluginName">Path and filename of the plug-in.</param>
		/// <param name="ex">Source exception.</param>
		public InputPlugInInvalidType(string pluginName, Exception ex)
			: base("The plug-in '" + pluginName + "' is not a valid input plug-in.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="pluginName">Path and filename of the plug-in.</param>
		public InputPlugInInvalidType(string pluginName)
			: this(pluginName, null)
		{
		}
		#endregion
	}
}
