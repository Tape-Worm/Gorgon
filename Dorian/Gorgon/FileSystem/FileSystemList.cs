#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Friday, November 03, 2006 7:34:04 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
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
