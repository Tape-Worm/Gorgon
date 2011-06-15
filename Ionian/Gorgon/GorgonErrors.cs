#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Friday, September 26, 2008 1:20:47 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// A list of predefined error messages to package with <see cref="GorgonLibrary.GorgonException"/>.
	/// </summary>
	public static class GorgonErrors
	{
		private const int ErrorBase = 0x7FF000;		// Base error code.

		/// <summary>
		/// Library was not initialized.
		/// </summary>
		public static GorgonError NotInitialized
		{
			get
			{
				return new GorgonError("NotInitialized", ErrorBase + 1, "The library was not initialized.");
			}
		}

		/// <summary>
		/// Error creating an object.
		/// </summary>
		public static GorgonError CannotCreate
		{
			get
			{
				return new GorgonError("CannotCreate", ErrorBase + 2, "Error while trying to create an object.");
			}
		}

		/// <summary>
		/// Error updating an object.
		/// </summary>
		public static GorgonError CannotUpdate
		{
			get
			{
				return new GorgonError("CannotCreate", ErrorBase + 2, "Error while trying to update.");
			}
		}

		/// <summary>
		/// Error while trying to load an object.
		/// </summary>
		public static GorgonError CannotLoad
		{
			get
			{
				return new GorgonError("CannotLoad", ErrorBase + 3, "Cannot load the requested data.");
			}
		}

		/// <summary>
		/// Error while trying to load an object.
		/// </summary>
		public static GorgonError CannotSave
		{
			get
			{
				return new GorgonError("CannotSave", ErrorBase + 4, "Cannot save the data.");
			}
		}

		/// <summary>
		/// Error while trying to lock some data.
		/// </summary>
		public static GorgonError CannotLock
		{
			get
			{
				return new GorgonError("CannotLock", ErrorBase + 5, "Cannot lock the data.");
			}
		}

		/// <summary>
		/// Data was not locked.
		/// </summary>
		public static GorgonError NotLocked
		{
			get
			{
				return new GorgonError("NotLocked", ErrorBase + 6, "Data was not locked.");
			}
		}

		/// <summary>
		/// No applicable hardware accelerated devices were found.
		/// </summary>
		public static GorgonError NoHALDevices
		{
			get
			{
				return new GorgonError("NoHALDevices", ErrorBase + 7, "No applicable hardware accelerated devices were found.");
			}
		}

		/// <summary>
		/// Shader compilation failed.
		/// </summary>
		public static GorgonError ShaderCompilationFailed
		{
			get
			{
				return new GorgonError("ShaderCompilationFailed", ErrorBase + 8, "Shader compilation failed.");
			}
		}

		/// <summary>
		/// Cannot read data.
		/// </summary>
		public static GorgonError CannotReadData
		{
			get
			{
				return new GorgonError("CannotReadData", ErrorBase + 9, "Error reading the data.");
			}
		}

		/// <summary>
		/// Invalid format.
		/// </summary>
		public static GorgonError InvalidFormat
		{
			get
			{
				return new GorgonError("InvalidFormat", ErrorBase + 0xA, "The format is not valid or unable to find a valid format.");
			}
		}

		/// <summary>
		/// No valid device object was found.
		/// </summary>
		public static GorgonError NoDevice
		{
			get
			{
				return new GorgonError("NoDevice", ErrorBase + 0xB, "There is no device object for this operation.");
			}
		}

		/// <summary>
		/// Driver reported an internal error.
		/// </summary>
		public static GorgonError HardwareError
		{
			get
			{
				return new GorgonError("HardwareError", ErrorBase + 0xC, "There was an internal driver or hardware exception.");
			}
		}

		/// <summary>
		/// The plug-in type is not valid.
		/// </summary>
		public static GorgonError InvalidPlugin
		{
			get
			{
				return new GorgonError("InvalidPlugin", ErrorBase + 0xD, "This plug-in type is not valid.");
			}
		}

		/// <summary>
		/// Error while writing data.
		/// </summary>
		public static GorgonError CannotWriteData
		{
			get
			{
				return new GorgonError("CannotWrite", ErrorBase + 0xE, "Error writing the data.");
			}
		}

		/// <summary>
		/// Access is denied.
		/// </summary>
		public static GorgonError AccessDenied
		{
			get
			{
				return new GorgonError("AccessDenied", ErrorBase + 0xF, "Access is denied.");
			}
		}

		/// <summary>
		/// Cannot bind an input device.
		/// </summary>
		public static GorgonError CannotBindInputDevice
		{
			get
			{
				return new GorgonError("CannotBindInputDevice", ErrorBase + 0x10, "Error binding the input device.");
			}
		}

		/// <summary>
		/// Cannot bind render target(s).
		/// </summary>
		public static GorgonError CannotBindTarget
		{
			get
			{
				return new GorgonError("CannotBindTarget", ErrorBase + 0x11, "Cannot bind render target(s).");
			}
		}
	}

	/// <summary>
	/// A defined error message to be packaged with <see cref="GorgonLibrary.GorgonException"/>.
	/// </summary>
	public struct GorgonError
	{
		#region Variables.
		private string _description;				// Description of th error.
		private int _code;							// Error code.
		private string _name;						// Name of the error.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the error message to sent with the exception.
		/// </summary>
		public string Description
		{
			get
			{
				return _description;
			}
		}

		/// <summary>
		/// Property to set or return the error code to send with the exception.
		/// </summary>
		public int Code
		{
			get
			{
				return _code;
			}
		}

		/// <summary>
		/// Property to return the name of the error.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonError)
			{
				GorgonError error = (GorgonError)obj;
				return ((string.Compare(error.Name, this.Name, true) == 0) && (error.Code == this.Code));
			}

			return false;
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return string.Format("{0} Result: \"{1}\"\nCode: 0x{2:X}", Name, Description, Code);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return Code.GetHashCode() ^ Name.GetHashCode();
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to test for equality.
		/// </summary>
		/// <param name="left">The left item to test.</param>
		/// <param name="right">The right item to test.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonError left, GorgonError right)
		{
			return ((left.Code == right.Code) && (string.Compare(left.Name, right.Name, true) == 0));
		}

		/// <summary>
		/// Operator to test for inequality.
		/// </summary>
		/// <param name="left">The left item to test.</param>
		/// <param name="right">The right item to test.</param>
		/// <returns>TRUE if not equal, FALSE if the items are equal.</returns>
		public static bool operator !=(GorgonError left, GorgonError right)
		{
			return !(left == right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonError"/> struct.
		/// </summary>
		/// <param name="name">Name for the error.</param>
		/// <param name="code">The code.</param>
		/// <param name="description">The description.</param>
		public GorgonError(string name, int code, string description)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (string.IsNullOrEmpty(description))
				throw new ArgumentNullException("description");

			_name = name;
			_description = description;
			_code = code;
		}
		#endregion
	}
}
