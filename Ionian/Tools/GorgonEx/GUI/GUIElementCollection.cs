#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Monday, June 09, 2008 9:01:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Extras.GUI
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
