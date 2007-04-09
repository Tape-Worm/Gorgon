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
using SharpUtilities;

namespace GorgonLibrary.Input
{
	#region Enumerator.
	/// <summary>
	/// Enumerator for generic error codes. 
	/// </summary>
	public enum InputErrors
	{
		/// <summary>Cannot create input interface.</summary>
		CannotCreateInput = 0x7FFF0001,
		/// <summary>Cannot capture the raw input data.</summary>
		CannotCaptureRawInput = 0x7FFF0002,
		/// <summary>Cannot bind the mouse.</summary>
		CannotBindMouse = 0x7FFF0003,
		/// <summary>Cannot bind the keyboard.</summary>
		CannotBindKeyboard = 0x7FFF0004
	}
	#endregion

	/// <summary>
	/// Base input exception class.
	/// </summary>
	public abstract class InputException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public InputException(string message, InputErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	///	Cannot create input interface exception.
	/// </summary>
	public class CannotCreateInputException 
		: InputException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreateInputException(string message, Exception ex)
			: base(message, InputErrors.CannotCreateInput, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotCreateInputException(Exception ex)
			: this("Could not create the input interface.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Cannot capture raw input exception.
	/// </summary>
	public class CannotCaptureRawInputException 
		: InputException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCaptureRawInputException(string message, Exception ex)
			: base(message, InputErrors.CannotCaptureRawInput, ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Cannot bind mouse exception.
	/// </summary>
	public class CannotBindMouseException 
		: InputException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public CannotBindMouseException(string message, Exception ex)
			: base(message, InputErrors.CannotBindMouse, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotBindMouseException(Exception ex)
			: this("Failed to bind the mouse to the window.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	///	Cannot bind keyboard exception.
	/// </summary>
	public class CannotBindKeyboardException 
		: InputException
	{
		#region Constructor/Destructor
		/// <summary>
		/// Constructor.
		/// </summary>		
		/// <param name="message">Message to display.</param>
		/// <param name="ex">Source exception.</param>
		public CannotBindKeyboardException(string message, Exception ex)
			: base(message, InputErrors.CannotBindKeyboard, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotBindKeyboardException(Exception ex)
			: this("Failed to bind the keyboard to the window.", ex)
		{
		}
		#endregion
	}
}
