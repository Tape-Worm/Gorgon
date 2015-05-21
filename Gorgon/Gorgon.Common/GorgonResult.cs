#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, June 14, 2011 8:57:48 PM
// 
#endregion

using System;
using Gorgon.Core.Properties;

namespace Gorgon.Core
{
	/// <summary>
	/// A defined error message to be packaged with <see cref="GorgonException"/>.
	/// </summary>
	public struct GorgonResult
		: INamedObject, IEquatable<GorgonResult>
	{
		#region Predefined error codes.
		private const int ErrorBase = 0x7FF000;		// Base error code.

		/// <summary>
		/// Library was not initialized.
		/// </summary>
		public static GorgonResult NotInitialized
		{
			get
			{
				return new GorgonResult("NotInitialized", ErrorBase + 1, "The library was not initialized.");
			}
		}

		/// <summary>
		/// Error creating an object.
		/// </summary>
		public static GorgonResult CannotCreate
		{
			get
			{
				return new GorgonResult("CannotCreate", ErrorBase + 2, "Error while trying to create an object.");
			}
		}

		/// <summary>
		/// Error while writing data.
		/// </summary>
		public static GorgonResult CannotWrite
		{
			get
			{
				return new GorgonResult("CannotWrite", ErrorBase + 0xa, "Error while writing data.");
			}
		}

		/// <summary>
		/// Access is denied.
		/// </summary>
		public static GorgonResult AccessDenied
		{
			get
			{
				return new GorgonResult("AccessDenied", ErrorBase + 3, "Access is denied.");
			}
		}

		/// <summary>
		/// Error accessing driver.
		/// </summary>
		public static GorgonResult DriverError
		{
			get
			{
				return new GorgonResult("DriverError", ErrorBase + 4, "Error while accessing driver.");
			}
		}

		/// <summary>
		/// Error while reading data.
		/// </summary>
		public static GorgonResult CannotRead
		{
			get
			{
				return new GorgonResult("CannotRead", ErrorBase + 5, "Error while reading data.");
			}
		}

		/// <summary>
		/// Error trying to bind.
		/// </summary>
		public static GorgonResult CannotBind
		{
			get
			{
				return new GorgonResult("CannotBind", ErrorBase + 6, "Error trying to bind to object.");
			}
		}

		/// <summary>
		/// Error trying to enumerate objects.
		/// </summary>
		public static GorgonResult CannotEnumerate
		{
			get
			{
				return new GorgonResult("CannotEnumerate", ErrorBase + 7, "Unable to perform enumeration.");
			}
		}

		/// <summary>
		/// Format is not supported.
		/// </summary>
		public static GorgonResult FormatNotSupported
		{
			get
			{
				return new GorgonResult("FormatNotSupported", ErrorBase + 8, "The requested format is not supported.");
			}
		}

		/// <summary>
		/// File format is not supported.
		/// </summary>
		public static GorgonResult InvalidFileFormat
		{
			get
			{
				return new GorgonResult("InvalidFileFormat", ErrorBase + 9, "The file format is not supported.");
			}
		}

		/// <summary>
		/// Error while trying to perform a rollback operation.
		/// </summary>
		public static GorgonResult CannotRollback
		{
			get
			{
				return new GorgonResult("CannotWrite", ErrorBase + 0xb, "Unable to perform rollback operation.");
			}
		}
		#endregion

		#region Variables.
		private readonly string _description;				// Description of th error.
		private readonly int _code;							// Error code.
		private readonly string _name;						// Name of the error.
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
		/// Function to compare two instances for equality.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonResult left, ref GorgonResult right)
		{
			return ((left.Code == right.Code) && (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)));
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonResult)
			{
				return ((GorgonResult)obj).Equals(this);
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
			return string.Format(Resources.GOR_GORGONRESULT_TOSTRING, Name, Description, Code.FormatHex());
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
		    return 281.GenerateHash(Code.GenerateHash(Name));
		}
		#endregion

		#region Operators.
		/// <summary>
		/// Operator to test for equality.
		/// </summary>
		/// <param name="left">The left item to test.</param>
		/// <param name="right">The right item to test.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonResult left, GorgonResult right)
		{
			return ((left.Code == right.Code) && (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)));
		}

		/// <summary>
		/// Operator to test for inequality.
		/// </summary>
		/// <param name="left">The left item to test.</param>
		/// <param name="right">The right item to test.</param>
		/// <returns>TRUE if not equal, FALSE if the items are equal.</returns>
		public static bool operator !=(GorgonResult left, GorgonResult right)
		{
			return ((left.Code != right.Code) || (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase)));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonResult"/> struct.
		/// </summary>
		/// <param name="name">Name for the error.</param>
		/// <param name="code">The code.</param>
		/// <param name="description">The description.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> or <paramref name="description"/> parameter is NULL (or Nothing in VB.NET)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or <paramref name="description"/> parameter is an empty string.</exception>
		public GorgonResult(string name, int code, string description)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "name");
			}

			if (description == null)
			{
				throw new ArgumentNullException("description");
			}

			if (string.IsNullOrEmpty(description))
			{
				throw new ArgumentException(Resources.GOR_PARAMETER_MUST_NOT_BE_EMPTY, "description");
			}

			_name = name;
			_description = description;
			_code = code;
		}
		#endregion

		#region IEquatable<GorgonResult> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonResult other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
