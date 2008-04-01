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
// Created: Sunday, April 22, 2007 6:54:17 PM
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
	/// Object representing a list of files.
	/// </summary>
	public class FileList
		: BaseCollection<FileSystemFile>
	{
		#region Properties.
		/// <summary>
		/// Property to return the file by index.
		/// </summary>
		public FileSystemFile this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return the file with the specified key name.
		/// </summary>
		public FileSystemFile this[string key]
		{
			get
			{
				if ((key == string.Empty) || (key == null))
					throw new ArgumentNullException("key");

				if (!Contains(key))
					throw new FileSystemFileNotFoundException(key);

				return GetItem(key);
			}			
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the list.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to add a file to the list.
		/// </summary>
		/// <param name="file">File to add.</param>
		internal void AddFile(FileSystemFile file)
		{
			if (Contains(file.FullPath))
				throw new FileSystemFileExistsException(file.FullPath);

			AddItem(file.FullPath, file);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal FileList()
			: base(64, false)
		{
		}
		#endregion
	}
}
