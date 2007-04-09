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
    /// Object representing the file system manager.
    /// </summary>
    public class FileSystemManager
        : Collection<FileSystem>, IDisposable
	{
		#region Variables.
		private FileSystemTypeList _fileSystemTypes = null;			// File system types.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the file system type list.
		/// </summary>
		public FileSystemTypeList Types
		{
			get
			{
				return _fileSystemTypes;
			}
		}

		/// <summary>
        /// Property to return a file system.
        /// </summary>
        /// <param name="key">Name of the file system.</param>
        public override FileSystem this[string key]
        {
            get
            {
                if (!Contains(key))
                    throw new SharpUtilities.Collections.KeyNotFoundException(key);

                return base[key.ToLower()];
            }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to clear all of the file systems.
		/// </summary>
		protected override void ClearItems()
		{
			// Clean up.
			foreach (FileSystem filesystem in this)
				filesystem.Dispose();

			base.ClearItems();
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		protected override void RemoveItem(string key)
		{
			this[key].Dispose();
			base.RemoveItem(key.ToLower());
		}

		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		protected override void RemoveItem(int index)
		{
			this[index].Dispose();
			base.Remove(index);
		}

		/// <summary>
		/// Function to add a file system.
		/// </summary>
		/// <param name="fileSystem">File system to add.</param>
		internal void Add(FileSystem fileSystem)
		{
			// If this is not a plug-in file system (i.e. local or FolderFileSystem)
			// then record its type information.
			if (((IPlugIn)fileSystem).PlugInEntryPoint == null)
			{
				if (!_fileSystemTypes.Contains(fileSystem.GetType()))
					_fileSystemTypes.Add(fileSystem.GetType());
			}

			_items.Add(fileSystem.Name.ToLower(), fileSystem);
		}

		/// <summary>
		/// Function to clear all file systems, plug-ins, and information.
		/// </summary>
		public void ClearTypes()
		{
			Clear();

			// Remove all information.
			_fileSystemTypes.Clear();

			// Add the folder file system.
			_fileSystemTypes.Add(typeof(FolderFileSystem));
		}

        /// <summary>
        /// Function to add a file system.
        /// </summary>
		/// <param name="name">Name of the file system.</param>
		/// <param name="plugIn">Plug-in used to create the file system object.</param>
        public FileSystem Create(string name, FileSystemPlugIn plugIn)
        {
			FileSystem fileSystem = null;		// File system.

            if (Contains(name.ToLower()))
                throw new DuplicateObjectException(name);

			if (plugIn == null)
				throw new ArgumentNullException("plugIn");

			// Create the file system.
			fileSystem = plugIn.Create(name);

			return fileSystem;
        }

		/// <summary>
		/// Function to remove a file system.
		/// </summary>
		/// <param name="fileSystem">File system to remove.</param>
		public void Remove(FileSystem fileSystem)
		{
			// Destroy the file system.
			Remove(fileSystem.Name);
		}

        /// <summary>
        /// Function to add a file system from a plug-in.
        /// </summary>        
		/// <param name="fileSystemPath">Path to the file system plug-in.</param>
		/// <returns>File system plug-in that was loaded.</returns>
		public FileSystemPlugIn LoadPlugIn(string fileSystemPath)
        {
			FileSystemPlugIn plugIn = null;					// File system plug-in.
			FileSystemEditorInfoAttribute info = null;		// File system information.

            // Load the plug-in.
			plugIn = (FileSystemPlugIn)Gorgon.PlugIns.Load(fileSystemPath);

			if ((plugIn == null) || (plugIn.PlugInType != PlugInType.FileSystem))
				throw new CannotMountFileSystemException("The plug-in '" + fileSystemPath + "' is not a file system plug-in.", null);

			// Add the attribute.
			info = plugIn.FileSystemInfo;
			if (!_fileSystemTypes.Contains(info.TypeName))
				_fileSystemTypes.Add(plugIn);
			else
				plugIn = _fileSystemTypes[info.TypeName].PlugIn;

			return plugIn;
        }

        /// <summary>
        /// Function to unmount a plug-in filesystem.
        /// </summary>
        /// <param name="name">Name of the file system to unmount.</param>
        public void UnloadPlugIn(string name)
        {
			_fileSystemTypes.Remove(name);
		}

        /// <summary>
        /// Function to determine if an object exists within the collection.
        /// </summary>
        /// <param name="key">Name of the object.</param>
        /// <returns>TRUE if the object exists, FALSE if not.</returns>
        public override bool Contains(string key)
        {
            return base.Contains(key.ToLower());
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        internal FileSystemManager()
            : base(16)
        {
			_fileSystemTypes = new FileSystemTypeList();

			// Clear all information.
			ClearTypes();
		}
        #endregion

        #region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				ClearItems();
				_fileSystemTypes.Clear();
			}
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
        #endregion
    }
}
