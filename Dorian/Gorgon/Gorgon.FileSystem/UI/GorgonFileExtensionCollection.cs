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
// Created: Wednesday, May 30, 2012 9:42:39 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// File extension.
	/// </summary>
	struct GorgonFileExtension
		: INamedObject
	{
		/// <summary>
		/// Extension.
		/// </summary>
		public IList<string> Extension;
		/// <summary>
		/// Description.
		/// </summary>
		public string Description;

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return Description + " (" + string.Join(";", Extension) + ")";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileExtension"/> struct.
		/// </summary>
		/// <param name="ext">The extension.</param>
		/// <param name="desc">The description.</param>
		public GorgonFileExtension(string ext, string desc)
		{
			Extension = new List<string>();
			Extension.Add(ext);
			Description = desc;
		}

		#region INamedObject Members
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get 
			{
				return Description;
			}
		}
		#endregion
	}

	/// <summary>
	/// Collection of file extensions.
	/// </summary>
	class GorgonFileExtensionCollection
		: GorgonBaseNamedObjectCollection<GorgonFileExtension>
	{
		#region Properties.
		/// <summary>
		/// Property to return an extension by name.
		/// </summary>
		public GorgonFileExtension this[string extension]
		{
			get
			{
				return GetItem(extension);
			}
		}

		/// <summary>
		/// Property to return an extension by index.
		/// </summary>
		public GorgonFileExtension this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add an extension.
		/// </summary>
		/// <param name="extension">Extension to add.</param>
		public void Add(GorgonFileExtension extension)
		{
			AddItem(extension);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileExtensionCollection"/> class.
		/// </summary>
		public GorgonFileExtensionCollection()
			: base(false)
		{
		}
		#endregion
	}
}
