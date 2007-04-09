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
// Created: Saturday, November 04, 2006 11:06:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using SharpUtilities;
using SharpUtilities.Collections;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object representing a list of file entries.
    /// </summary>
    public class FileSystemEntryManager
        : HashMap<FileSystemEntry>
    {
        #region Properties.
        /// <summary>
        /// Property to return a file system entry.
        /// </summary>
        /// <param name="key">Path to the entry.</param>
        public override FileSystemEntry this[string key]
        {
            get
            {
				// Get the filename.
				key = FileSystem.FullFileName(key).ToLower();

				if (!base.Contains(key))
					throw new FileSystemEntryDoesNotExistException(key, null);

                return base[key];
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to remove an item from the list.
        /// </summary>
        /// <param name="key">Name of the item.</param>
        protected override void RemoveItem(string key)
        {
			// Get the filename.
			key = FileSystem.FullFileName(key).ToLower();

			if (!base.Contains(key))
				throw new FileSystemEntryDoesNotExistException(key, null);
			
			base.RemoveItem(key);
        }

        /// <summary>
        /// Function to add a file system entry to the list.
        /// </summary>
        /// <param name="path">Path to the file.</param>
		/// <param name="objectData">Binary data for the object.</param>
		/// <param name="offset">Offset within the file system.</param>
		/// <param name="fileSize">Size of the file.</param>
		/// <param name="compressedSize">Compressed size of the file, 0 means uncompressed.</param>
		/// <param name="encrypted">TRUE if the entry is encrypted, FALSE if not.</param>
		/// <returns>A new file system entry.</returns>
        public FileSystemEntry AddEntry(string path, byte[] objectData, long offset, int fileSize, int compressedSize, bool encrypted)
        {
            FileSystemEntry entry = null;			// Entry.
			string filePath = string.Empty;			// Path to the entry.
			string fileName = string.Empty;			// Filename of the entry.
			string extension = string.Empty;		// Extension of the entry.

			// Validate arguments.
			if ((path == string.Empty) || (path == null))
				throw new ArgumentNullException("path");

			// Get full path.
			path = FileSystem.FullFileName(path);

			// Extract entry components.
			filePath = FileSystem.FullPathName(Path.GetDirectoryName(path));
			fileName = Path.GetFileNameWithoutExtension(path);
			extension = Path.GetExtension(path);

			// Create an entry and add it to the list.
			entry = new FileSystemEntry(filePath, fileName, extension, objectData, offset, fileSize, compressedSize, encrypted);

            if (base.Contains(entry.FullPath.ToLower()))
                throw new DuplicateObjectException(entry.FullPath);

            _items.Add(entry.FullPath.ToLower(), entry);

            return entry;
        }

        /// <summary>
        /// Function to determine if an object exists within the collection.
        /// </summary>
        /// <param name="key">Name of the object.</param>
        /// <returns>TRUE if the object exists, FALSE if not.</returns>
        public override bool Contains(string key)
        {
			// Get filename.
			key = FileSystem.FullFileName(key).ToLower();

			return base.Contains(key);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileSystem">File system that owns this object.</param>
        internal FileSystemEntryManager(FileSystem fileSystem)
            : base(53)
        {
        }
        #endregion
    }
}
