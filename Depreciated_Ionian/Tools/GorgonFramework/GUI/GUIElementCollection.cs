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
// Created: Monday, June 09, 2008 9:01:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Collection of GUI elements.
	/// </summary>
	public class GUIElementCollection
		: Collection<GUIElement>
	{
		#region Variables.
		private GUISkin _owner = null;			// Skin that owns this list of elements.
		#endregion

		#region Propertie
		/// <summary>
		/// Property to set or return an element by its index.
		/// </summary>
		public new GUIElement this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
				if ((index < 0) || (index >= Count))
					throw new IndexOutOfRangeException("The index '" + index.ToString() + "' is not valid for this collection.");
				SetItem(index, value);
			}
		}

		/// <summary>
		/// Property to set or return an element by its name.
		/// </summary>
		public new GUIElement this[string key]
		{
			get
			{
				if (string.IsNullOrEmpty(key))
					throw new ArgumentNullException("key");

				return GetItem(key);
			}
			set
			{
				if (string.IsNullOrEmpty(key))
					throw new ArgumentNullException("key");

				if (Contains(key))
				{
					if (value == null)
						Remove(key);
					else
					{
						if (value.Owner != null)
							throw new ArgumentException("Element is already assigned to a skin.");
						else
							value.SetOwner(_owner);
						SetItem(key, value);
					}
				}
				else
				{
					if (value != null)
						AddItem(key, value);
					else
						throw new KeyNotFoundException("'" + key + "' was not found.");
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add an element to the list.
		/// </summary>
		/// <param name="element">Element to add.</param>
		public void Add(GUIElement element)
		{
			if (element == null)
				throw new ArgumentNullException("element");

			if (element.Owner != null)
				throw new ArgumentException("Element is already assigned to a skin.");

			element.SetOwner(_owner);
			AddItem(element.Name, element);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIElementCollection"/> class.
		/// </summary>
		/// <param name="owner">Skin that owns the list.</param>
		internal GUIElementCollection(GUISkin owner)
			: base(false)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
		}
		#endregion
	}
}
