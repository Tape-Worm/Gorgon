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
// Created: Monday, June 27, 2011 8:59:49 AM
// 
#endregion

using Gorgon.Collections;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of files available from the file system.
	/// </summary>
	/// <remarks>Users should be aware that file names in this collection are NOT case sensitive.</remarks>
	public class GorgonFileSystemFileEntryCollection
		: GorgonBaseNamedObjectCollection<GorgonFileSystemFileEntry>
	{
		#region Variables.
		private readonly GorgonFileSystemDirectory _parent;					// File system directory parent.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return a file system file entry by name.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>The file system file entry requested.</returns>
		public GorgonFileSystemFileEntry this[string fileName]
		{
			get
			{
				return GetItem(fileName.RemoveIllegalFilenameChars());
			}
			internal set
			{
				bool exists = Contains(fileName);
				string formattedFileName = fileName.RemoveIllegalFilenameChars();

				// If we pass in NULL, remove the file.
				if (value == null)
				{
					if (exists)
					{
						Remove(GetItem(formattedFileName));						
					}

					return;
				}

				if (!exists)
				{
					Add(value);
				}
				else
				{					
					SetItem(formattedFileName, value);
				}
			}
		}

		/// <summary>
		/// Property to return a file system file entry by index.
		/// </summary>
		/// <param name="index">Index of the file system file entry.</param>
		/// <returns>The file system file entry specified.</returns>
		public GorgonFileSystemFileEntry this[int index]
		{
			get
			{				
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove a file from the list of files.
		/// </summary>
		/// <param name="file">File to remove.</param>
		internal void Remove(GorgonFileSystemFileEntry file)
		{
			RemoveItem(file);
		}

		/// <summary>
		/// Function to add a file system file entry to the collection.
		/// </summary>
		/// <param name="fileEntry">File entry to add.</param>
		internal void Add(GorgonFileSystemFileEntry fileEntry)
		{
			fileEntry.Directory = _parent;
			AddItem(fileEntry);
		}

		/// <summary>
		/// Function to clear all directories from this collection.
		/// </summary>
		internal void Clear()
		{
			foreach (GorgonFileSystemFileEntry fileEntry in this)
			{
				fileEntry.Directory = null;
			}

			ClearItems();
		}
			
		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public override bool Contains(string name)
		{
			return base.Contains(name.RemoveIllegalFilenameChars());
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemFileEntryCollection"/> class.
		/// </summary>
		/// <param name="parent">The parent directory that owns this collection.</param>
		internal GorgonFileSystemFileEntryCollection(GorgonFileSystemDirectory parent)			
			: base(false)
		{
			_parent = parent;
		}
		#endregion
	}
}
