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
// Created: Saturday, April 19, 2008 11:44:27 AM
// 
#endregion

using System;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Abstract Class representing a named object.
	/// </summary>
	/// <remarks>Classes inheriting from this object will have a name associated with it.  This is handy when dealing with unique objects within a collection.</remarks>
	public abstract class NamedObject
	{
		#region Variables.
		private string _objectName;     // The name of the object.
		#endregion
		
		#region Properties.
		/// <summary>
		/// Read-only property to return the name of this object.
		/// </summary>
		/// <remarks>The name of an object need not be unique, however if it is used as a key value for a collection then it should be unique.</remarks>
		/// <value>A <see cref="string">string</see> containing the name of this object.</value>
		public virtual string Name
		{
			get
			{
				return _objectName;
			}
			protected internal set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("newName");
				_objectName = value;
			}
		}        
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if this named object is the same as another named object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public override bool Equals(object obj)
		{
			if (obj is NamedObject)
				return (((NamedObject)obj).Name == _objectName);
			else
				return false;
		}

		/// <summary>
		/// Function to return the hash code for this object.
		/// </summary>
		/// <remarks>
		/// A hash code is used by dictionaries and hash tables to determine uniqueness of an object within those collections.
		/// <para>
		/// Hash values are related to their types, and as such should use one or more of its instance fields hash codes to build the hash code.
		/// </para>
		/// </remarks>
		/// <returns>A 32 bit integer representing the hash code.</returns>
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		/// <summary>
		/// Function to return the string representation of this object.
		/// </summary>
		/// <returns>A string containing the name of the type and the name stored within the object.</returns>
		public override string ToString()
		{
			return "Named object: [" + Name + "]";
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <remarks>
		/// Since the <seealso cref="Name"></seealso><see cref="Name">Name</see> property is read-only, the name for an object must be passed to this constructor.  
		/// <para>Typically it will be difficult to rename an object that resides in a collection with its name as the key, thus making the property writable would be counter-productive.</para>
		/// 	<para>However, objects that descend from this object may choose to implement their own renaming scheme if they so choose.</para>
		/// 	<para>Ensure that the name parameter has a non-null string or is not a zero-length string.  Failing to do so will raise a <see cref="System.ArgumentNullException">ArgumentNullException</see>.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		/// <param name="name">Name for this object.</param>
		protected NamedObject(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			Name = name;
		}
		#endregion
	}
}
