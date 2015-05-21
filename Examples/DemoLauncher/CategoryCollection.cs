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
// Created: Tuesday, September 23, 2014 2:35:19 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Gorgon.Collections;
using Gorgon.Examples.Properties;

namespace Gorgon.Examples
{
	/// <summary>
	/// A list of categories.
	/// </summary>
	class CategoryCollection
		: GorgonBaseNamedObjectDictionary<Category>, IDisposable
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed = false;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a category to the list.
		/// </summary>
		/// <param name="category">Category to add.</param>
		private void Add(Category category)
		{
			if (category == null)
			{
				return;
			}

			AddItem(category);
		}

		/// <summary>
		/// Function to read the categories and examples from the embedded example list.
		/// </summary>
		/// <returns>A new category collection.</returns>
		public static CategoryCollection Read()
		{
			var result = new CategoryCollection();
			var document = XDocument.Parse(Resources.Examples);

			XElement root = document.Element("Examples");

			if (root == null)
			{
				return result;
			}

			IEnumerable<XElement> categories = root.Elements("Category");

			foreach (XElement categoryNode in categories)
			{
				result.Add(Category.Read(categoryNode));
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Prevents a default instance of the <see cref="CategoryCollection"/> class from being created.
		/// </summary>
		private CategoryCollection()
			: base(false)
		{
			
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				foreach (Category category in this)
				{
					category.Dispose();
				}

				ClearItems();
			}

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
