#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, May 02, 2012 10:10:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Configuration
{
	/// <summary>
	/// A configuration value.
	/// </summary>
	public struct GorgonConfigurationValue
		: INamedObject, IEquatable<GorgonConfigurationValue>
	{
		#region Variables.
		private string _name;			// Name for the configuration value.
		private string _value;			// The value stored in the configuration value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the value for the configuration value.
		/// </summary>
		public string Value
		{
			get
			{
				return _value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{			
			return 281.GenerateHash(_name);
		}

		/// <summary>
		/// Function to determine if two values are equal.
		/// </summary>
		/// <param name="left">Left value to compare.</param>
		/// <param name="right">Right value to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonConfigurationValue left, ref GorgonConfigurationValue right)
		{
			return string.Compare(left.Name, right.Name, false) == 0;
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonConfigurationValue)
				return Equals((GorgonConfigurationValue)obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("Gorgon Configuration Value: {0} = \"{1}\"", _name, _value);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConfigurationValue"/> struct.
		/// </summary>
		/// <param name="name">The name for the configuration value.</param>
		/// <param name="value">The value to store.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public GorgonConfigurationValue(string name, string value)
		{
			GorgonDebug.AssertParamString(name, "name");

			if (value == null)
				value = string.Empty;

			_name = name;
			_value = value;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name for the configuration value.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}
		#endregion

		#region IEquatable<GorgonConfigurationValue> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonConfigurationValue other)
		{
			return GorgonConfigurationValue.Equals(ref this, ref other);
		}
		#endregion
	}
}
