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
// Created: Sunday, September 22, 2013 8:43:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Collections;
using Gorgon.Core.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of file extensions.
	/// </summary>
	/// <remarks>This collection is useful for building a series of file type descriptions for a file dialog.</remarks>
	public class GorgonFileExtensionCollection
		: GorgonBaseNamedObjectDictionary<GorgonFileExtension>
	{
		#region Properties.
		/// <summary>
		/// Property to set or return an extension in the list.
		/// </summary>
		public GorgonFileExtension this[string extension]
		{
			get
			{
				if (extension.StartsWith(".", StringComparison.Ordinal))
				{
					extension = extension.Substring(1);
				}

				return Items[extension];
			}
			set
			{
				if (extension.StartsWith(".", StringComparison.Ordinal))
				{
					extension = extension.Substring(1);
				}

				if (!Contains(extension))
				{
					Add(value);
					return;
				}

				UpdateItem(extension, value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a file extension to the list.
		/// </summary>
		/// <param name="extension">Extension to add.</param>
		public void Add(GorgonFileExtension extension)
		{
			if (extension.Extension == null)
			{
				extension = new GorgonFileExtension(string.Empty, extension.Description);
			}

			if (Contains(extension))
			{
				throw new ArgumentException(string.Format(Resources.GOR_FILE_EXTENSION_EXISTS, extension.Extension));
			}

			Items.Add(extension.Extension, extension);
		}

		/// <summary>
		/// Function to remove a file extension from the list.
		/// </summary>
		/// <param name="extension">Extension to remove.</param>
		public void Remove(string extension)
		{
			if (extension.StartsWith(".", StringComparison.Ordinal))
			{
				extension = extension.Substring(1);
			}

			if (!Contains(extension))
			{
				throw new KeyNotFoundException();
			}

			Items.Remove(extension);
		}

		/// <summary>
		/// Function to remove a file extension from the list.
		/// </summary>
		/// <param name="extension"></param>
		public void Remove(GorgonFileExtension extension)
		{
			if (!Contains(extension))
			{
				throw new KeyNotFoundException();
			}

			RemoveItem(extension);
		}

		/// <summary>
		/// Function to clear all items from the list.
		/// </summary>
		public void Clear()
		{
			Items.Clear();
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
