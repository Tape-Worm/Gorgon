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
// Created: Tuesday, June 14, 2011 10:13:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;

namespace Gorgon.Collections.Specialized
{
	/// <summary>
	/// Read-only collection for Gorgon library named objects.
	/// </summary>
	/// <typeparam name="T">Type of object, must implement <see cref="INamedObject">INamedObject</see>.</typeparam>
	public sealed class GorgonNamedObjectReadOnlyCollection<T>
		: GorgonBaseNamedObjectList<T>
		where T : INamedObject
	{
		#region Properties.
		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		/// </returns>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to return an object by its index.
		/// </summary>
		public T this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return an object by its name.
		/// </summary>
		public T this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseNamedObjectCollection&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="caseSensitive">TRUE if the key names are case sensitive, FALSE if not.</param>
		/// <param name="source">Collection to use as the source.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="source"/> parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="source"/> parameter contains 0 items.</exception>
		public GorgonNamedObjectReadOnlyCollection(bool caseSensitive, IEnumerable<T> source)
			: base(caseSensitive)
		{
		    if (source == null)
		    {
		        throw new ArgumentNullException("source");
		    }

		    foreach (T item in source)
		    {
		        AddItem(item);
		    }
		}
		#endregion
	}
}
