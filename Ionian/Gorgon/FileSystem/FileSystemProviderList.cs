#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, April 06, 2007 12:38:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;
using SharpUtilities.Collections;

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

				throw new SharpUtilities.Collections.KeyNotFoundException(type.Name);
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
			if (Contains(plugIn.Name))
				throw new DuplicateObjectException(plugIn.Name);

			AddItem(plugIn.Name, new FileSystemProvider(plugIn));
		}

		/// <summary>
		/// Function to add file system information.
		/// </summary>
		/// <param name="typeInfo">Information about the file system object type.</param>
		internal void Add(Type typeInfo)
		{
			FileSystemProvider fsType = null;		// File system type information.

			if (Contains(typeInfo))
				throw new DuplicateObjectException(this[typeInfo].Name);

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
