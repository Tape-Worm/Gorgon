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
// Created: Sunday, May 06, 2012 2:15:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// A collection of documents.
	/// </summary>
	class DocumentCollection
		: GorgonBaseNamedObjectDictionary<Document>
	{
		#region Properties.
		/// <summary>
		/// Property to return the document assigned to a tab.
		/// </summary>
		/// <param name="tab">Tab to look up.</param>
		/// <returns>The document for the tab page.</returns>
		public Document this[KRBTabControl.TabPageEx tab]
		{
			get
			{
				return (from Document documentTab in this
						where documentTab.Tab == tab
						select documentTab).FirstOrDefault();
			}
		}

		/// <summary>
		/// Property to return a document by its name.
		/// </summary>
		public Document this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to rename the specified document.
		/// </summary>
		/// <param name="document">Document to rename.</param>
		/// <param name="newName">New name for the document.</param>
		public void Rename(string oldName, string newName)
		{
			if (string.IsNullOrEmpty(oldName))
				throw new ArgumentException("The old name cannot be empty.");
			if (string.IsNullOrEmpty(newName))
				throw new ArgumentException("The new name cannot be empty.");

			if (Contains(newName))
				throw new ArgumentException("The name '" + newName + "' already exists.");

			if (!Contains(oldName))
				throw new ArgumentException("The old name '" + oldName + "' does not belong to this collection.");

			if (string.Compare(oldName, newName, false) != 0)
			{
				Document document = this[oldName];
				Remove(oldName);
				Add(document);
			}
		}

		/// <summary>
		/// Function to add the specified document.
		/// </summary>
		/// <param name="document">The document to add.</param>
		public void Add(Document document)
		{
			AddItem(document);
		}

		/// <summary>
		/// Function to remove a document.
		/// </summary>
		/// <param name="document">Document to remove.</param>
		public void Remove(Document document)
		{
			if (Contains(document))
				RemoveItem(document);
		}

		/// <summary>
		/// Function to remove a document.
		/// </summary>
		/// <param name="name">Name of the document to remove.</param>
		public void Remove(string name)
		{
			if (Contains(name))
				RemoveItem(name);
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
		/// Initializes a new instance of the <see cref="DocumentCollection"/> class.
		/// </summary>
		public DocumentCollection()
			: base(false)
		{
		}
		#endregion
	}
}
