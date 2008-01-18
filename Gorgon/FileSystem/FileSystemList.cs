#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Friday, November 03, 2006 7:34:04 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;
using SharpUtilities.Collections;
using GorgonLibrary.FileSystems;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object representing the file system list.
    /// </summary>
    public class FileSystemList
        : BaseCollection<FileSystem>
	{
		#region Properties.
		/// <summary>
        /// Property to return a file system.
        /// </summary>
        /// <param name="key">Name of the file system.</param>
        public FileSystem this[string key]
        {
            get
            {
				if (!Contains(key))
					throw new FileSystemDoesNotExistException(key);

                return GetItem(key);
            }
        }

		/// <summary>
		/// Property to return a file system.
		/// </summary>
		/// <param name="index">Index of the file system.</param>
		public FileSystem this[int index]
		{
			get
			{
				if ((index < 0) || (index >= Count))
					throw new IndexOutOfBoundsException(index);

				return GetItem(index);
			}
		}
		#endregion

        #region Methods.
		/// <summary>
		/// Function to add a file system.
		/// </summary>
		/// <param name="fileSystem">File system to add.</param>
		internal void Add(FileSystem fileSystem)
		{
			if (Contains(fileSystem.Name))
				throw new FileSystemExistsException(fileSystem.Name);

			AddItem(fileSystem.Name, fileSystem);
		}

		/// <summary>
		/// Function to clear the list.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to remove a file system.
		/// </summary>
		/// <param name="fileSystem">File system to remove.</param>
		internal void Remove(string fileSystem)
		{
			// Destroy the file system.
			RemoveItem(fileSystem);
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        internal FileSystemList()
            : base(16, false)
        {
		}
        #endregion
    }
}
