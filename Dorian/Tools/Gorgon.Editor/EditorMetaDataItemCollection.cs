#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, February 24, 2014 10:40:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Collections;
using GorgonLibrary.Editor.Properties;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A collection of meta data items.
	/// </summary>
	/// <remarks>This collection uses case sensitive keys to index the meta data items.</remarks>
	public class EditorMetaDataItemCollection
		: GorgonBaseNamedObjectDictionary<EditorMetaDataItem>
	{
		#region Properties.
		/// <summary>
		/// Property to set or return a meta data item in the collection.
		/// </summary>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the item referenced by the <paramref name="name"/> parameter could not be found in the collection.</exception>
		public EditorMetaDataItem this[string name]
		{
			get
			{
				if (!Contains(name))
				{
					throw new KeyNotFoundException(string.Format(Resources.GOREDIT_METADATA_NOT_FOUND, name));
				}

				return GetItem(name);
			}
			set
			{
				if (value == null)
				{
					if (Contains(name))
					{
						Remove(name);
					}
					return;
				}

				if (Contains(name))
				{
					SetItem(name, value);
					return;
				}

				AddItem(value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the meta data item list.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to add a range of items to the collection.
		/// </summary>
		/// <param name="items">Items to add to the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="items"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="items"/> parameter already contains a meta data item with the same name as another in the collection.</exception>
		public void AddRange(IEnumerable<EditorMetaDataItem> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			foreach (EditorMetaDataItem item in items)
			{
				Add(item);	
			}
		}

		/// <summary>
		/// Function to add a new metadata item to the collection.
		/// </summary>
		/// <param name="item">Item to add to the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="item"/> parameter already contains a meta data item with the same name.</exception>
		public void Add(EditorMetaDataItem item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			if (Contains(item.Name))
			{
				throw new ArgumentException(string.Format(Resources.GOREDIT_METADATA_ALREADY_EXISTS, item.Name), "item");
			}

			AddItem(item);
		}

		/// <summary>
		/// Function to remove a metadata item by name.
		/// </summary>
		/// <param name="itemName">Name of the item to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="itemName"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="itemName"/> parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the item referenced by the <paramref name="itemName"/> parameter could not be found in the collection.</exception>
		public void Remove(string itemName)
		{
			if (itemName == null)
			{
				throw new ArgumentNullException("itemName");
			}

			if (string.IsNullOrWhiteSpace(itemName))
			{
				throw new ArgumentException(Resources.GOREDIT_PARAMETER_MUST_NOT_BE_EMPTY, "itemName");
			}

			if (!Contains(itemName))
			{
				throw new KeyNotFoundException(string.Format(Resources.GOREDIT_METADATA_NOT_FOUND, itemName));
			}

			RemoveItem(itemName);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">Item to remove from the collection.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="item"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown when the <paramref name="item"/> could not be found in the collection.</exception>
		public void Remove(EditorMetaDataItem item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}

			if (!Contains(item))
			{
				throw new KeyNotFoundException(string.Format(Resources.GOREDIT_METADATA_NOT_FOUND, item.Name));
			}

			RemoveItem(item);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMetaDataItemCollection"/> class.
		/// </summary>
		internal EditorMetaDataItemCollection()
			: base(true)
		{
		}
		#endregion
	}
}
