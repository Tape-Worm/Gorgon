#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, April 06, 2007 12:38:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object representing a file system type list.
	/// </summary>
	public class FileSystemProviderList
		: BaseCollection<FileSystemProvider>
	{
		#region Properties.
		/// <summary>
		/// Property to return a file system provider by index.
		/// </summary>
		public FileSystemProvider this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a file system provider by its key.
		/// </summary>
		public FileSystemProvider this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}

		/// <summary>
		/// Property to return a file system by its type.
		/// </summary>
		/// <param name="type">Type of file system.</param>
		/// <returns>File system of the specified type.</returns>
		public FileSystemProvider this[Type type]
		{
			get
			{
				foreach (FileSystemProvider info in this)
				{
					if (info.Type == type)
						return info;
				}

                throw new KeyNotFoundException("The provider type '" + type.Name + "' was not found.");
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add file system information.
		/// </summary>
		/// <param name="plugIn">Plug-in used by the file system.</param>
		internal void Add(FileSystemPlugIn plugIn)
		{
            if (plugIn == null)
                throw new ArgumentNullException("plugIn");

            if (Contains(plugIn.Name))
                throw new ArgumentException("The provider plug-in '" + plugIn.Name + "' is already loaded.");

			AddItem(plugIn.Name, new FileSystemProvider(plugIn));
		}

		/// <summary>
		/// Function to add file system information.
		/// </summary>
		/// <param name="typeInfo">Information about the file system object type.</param>
		internal void Add(Type typeInfo)
		{
			FileSystemProvider fsType = null;		// File system type information.

            if (typeInfo == null)
                throw new ArgumentNullException("typeInfo");

			if (Contains(typeInfo))
                throw new ArgumentException("The provider with type '" + typeInfo.Name + "' is already loaded.");

			fsType = new FileSystemProvider(typeInfo);

			AddItem(fsType.Name, fsType);
		}

		/// <summary>
		/// Function clear the list.
		/// </summary>
		internal void Clear()
		{
			while(Count > 0)
				this[0].Dispose();
			ClearItems();
		}

		/// <summary>
		/// Function to remove an item by name.
		/// </summary>
		/// <param name="name">Name of the item to remove.</param>
		internal void Remove(string name)
		{
			RemoveItem(name);
		}

		/// <summary>
		/// Function to return whether the file system info list contains information about the type of file system passed.
		/// </summary>
		/// <param name="type">Type of file system.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public bool Contains(Type type)
		{
			foreach (FileSystemProvider info in this)
			{
				if (info.Type == type)
					return true;
			}

			return false;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal FileSystemProviderList()
			: base(8, false)
		{
		}
		#endregion
	}
}
