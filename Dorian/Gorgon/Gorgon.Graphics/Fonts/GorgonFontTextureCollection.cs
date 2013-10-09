#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, September 22, 2013 11:36:42 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using GorgonLibrary.Collections;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of textures used for the font.
	/// </summary>
	public class GorgonFontTextureCollection
		: GorgonBaseNamedObjectCollection<GorgonTexture2D>
	{
		#region Variables.
		private readonly IList<GorgonTexture2D> _internal;	// Internally generated textures for the font.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a texture from the collection by name.
		/// </summary>
		public GorgonTexture2D this[string name]
		{
			get
			{
				return GetItem(name);
			}
			set
			{
				if (value == null)
				{
					if (!Contains(name))
					{
						throw new KeyNotFoundException(string.Format(Resources.GORGFX_FONT_TEXTURE_NOT_FOUND, name));
					}

					RemoveItem(GetItem(name));

					return;
				}

				// Remove the previous reference, and update it and change its name to the current reference.
				if (Contains(name))
				{
					SetItem(name, value);
					return;
				}

				AddItem(value);
			}
		}

		/// <summary>
		/// Property to return a texture from the collection by its index.
		/// </summary>
		public GorgonTexture2D this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Removes the item.
		/// </summary>
		/// <param name="item">The item.</param>
		protected override void RemoveItem(GorgonTexture2D item)
		{
			base.RemoveItem(item);

			// Remove internal textures.
			if (!_internal.Contains(item))
			{
				return;
			}

			_internal.Remove(item);
			item.Dispose();
		}

		/// <summary>
		/// Removes the item.
		/// </summary>
		/// <param name="name">The name.</param>
		protected override void RemoveItem(string name)
		{
			// Remove internal textures.
			if (_internal.Contains(this[name]))
			{
				_internal.Remove(this[name]);
				this[name].Dispose();
			}

			base.RemoveItem(name);
		}

		/// <summary>
		/// Function to determine if the texture referenced is internal (created by Gorgon) or external.
		/// </summary>
		/// <param name="texture">The texture to evaluate.</param>
		/// <returns>TRUE if internal, FALSE if not.</returns>
		internal bool IsInternal(GorgonTexture2D texture)
		{
			return _internal.Contains(texture);
		}

		/// <summary>
		/// Function add a texture to the collection and bind it to the internal font texture list.
		/// </summary>
		/// <param name="texture">Texture to add.</param>
		internal void AddBind(GorgonTexture2D texture)
		{
			Add(texture);
			_internal.Add(texture);
		}

		/// <summary>
		/// Function to add a texture to the list.
		/// </summary>
		/// <param name="texture">Texture to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the texture parameter already exists in this collection.</exception>
		public void Add(GorgonTexture2D texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}

			if ((Contains(texture.Name))
				|| (Contains(texture)))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_FONT_TEXTURE_EXISTS, texture.Name), "texture");
			}

			AddItem(texture);
		}

		/// <summary>
		/// Function to add a list of items to the collection.
		/// </summary>
		/// <param name="items">Items to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="items"/> parameter is NULL (Nothing in VB.Net).</exception>
		public void AddRange(IEnumerable<GorgonTexture2D> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}

			foreach (var item in items)
			{
				Add(item);
			}
		}

		/// <summary>
		/// Function to remove a texture from the list.
		/// </summary>
		/// <param name="texture">The texture to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">The texture does not exist in the collection.</exception>
		public void Remove(GorgonTexture2D texture)
		{
			if (texture == null)
			{
				throw new ArgumentNullException("texture");
			}

			if (!Contains(texture))
			{
				throw new KeyNotFoundException(string.Format(Resources.GORGFX_FONT_TEXTURE_NOT_FOUND, texture.Name));
			}

			RemoveItem(texture);
		}

		/// <summary>
		/// Function to remove a texture from the list.
		/// </summary>
		/// <param name="index">Index of the texture to remove.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when then <paramref name="index"/> parameter is less than 0 or larger than than Count-1 of textures.</exception>
		public void Remove(int index)
		{
			if ((index < 0)
				|| (index >= Count))
			{
				throw new IndexOutOfRangeException(string.Format(Resources.GORGFX_INDEX_OUT_OF_RANGE, index, 0, Count));
			}

			RemoveItem(index);
		}

		/// <summary>
		/// Function to remove a texture from the list.
		/// </summary>
		/// <param name="name">Name of the texture to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when then <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">The texture does not exist in the collection.</exception>
		public void Remove(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			if (name.Length == 0)
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "name");
			}

			if (!Contains(name))
			{
				throw new KeyNotFoundException(string.Format(Resources.GORGFX_FONT_TEXTURE_NOT_FOUND, name));
			}

			RemoveItem(name);
		}

		/// <summary>
		/// Function to remove all textures from the collection.
		/// </summary>
		public void Clear()
		{
			foreach (var item in this.Where(item => _internal.Contains(item)))
			{
				_internal.Remove(item);
				item.Dispose();
			}

			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFontTextureCollection"/> class.
		/// </summary>
		internal GorgonFontTextureCollection()
			: base(false)
		{
			_internal = new List<GorgonTexture2D>();
		}
		#endregion
	}
}
