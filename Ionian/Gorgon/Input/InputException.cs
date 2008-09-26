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
