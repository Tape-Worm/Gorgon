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
// Created: Tuesday, July 19, 2011 7:30:08 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics.Properties;

namespace Gorgon.Collections
{
	/// <summary>
	/// The named value type to be stored in the collection.
	/// </summary>
	/// <typeparam name="T">Type of data for the value.</typeparam>
	public struct GorgonNamedValue<T>
		: INamedObject
	{
		#region Variables.
		/// <summary>
		/// Name of the value.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Value to bind with the name.
		/// </summary>
		public T Value;		
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonNamedValue&lt;T&gt;"/> struct.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonNamedValue(string name, T value)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "name");
			}

			Name = name;
			Value = value;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the value.
		/// </summary>
		string INamedObject.Name
		{
			get
			{
				return Name;
			}
		}
		#endregion
	}

	/// <summary>
	/// A collection of named values that can only be assigned to.
	/// </summary>
	/// <typeparam name="T">Type of data for the value.</typeparam>
	public class GorgonCustomValueCollection<T>
		: GorgonBaseNamedObjectDictionary<GorgonNamedValue<T>>
	{
		#region Variables.
		private readonly List<string> _names;		// List of names.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a value by its name.
		/// </summary>
		public T this[string name]
		{
			get
			{
				return GetItem(name).Value;
			}
			set
			{
				if (Contains(name))
				{
					SetItem(name, new GorgonNamedValue<T>(name, value));
				}
				else
				{
					Add(name, value);
				}
			}
		}

		/// <summary>
		/// Property to return the list of names for the values stored in this collection.
		/// </summary>
		public IEnumerable<string> Names
		{
			get
			{
				return _names;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a named value to the collection.
		/// </summary>
		/// <param name="name">Name of the value to add.</param>
		/// <param name="value">Value to set.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		private void Add(string name, T value)
		{
			AddItem(new GorgonNamedValue<T>(name, value));
			_names.Add(name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonCustomValueCollection&lt;T&gt;"/> class.
		/// </summary>
		internal GorgonCustomValueCollection()
			: base(false)
		{
			_names = new List<string>();
		}
		#endregion
	}
}
