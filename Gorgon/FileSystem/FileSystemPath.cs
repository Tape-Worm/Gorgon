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
// Created: Friday, April 20, 2007 1:21:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GorgonLibrary.Internal;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object representing a piece of a path for a file system.
	/// </summary>
	public class FileSystemPath
		: NamedObject
	{
		#region Variables.
		private FileSystemPath _parent = null;				// Parent of the path.
		private FileSystemFileList _files = null;			// File list.
		private static char[] _invalidCharacters = null;	// Invalid filename characters.
		private FileSystemPathList _paths = null;			// Path list.
		private FileList _allFiles = null;					// A list of all files under this path node.
		private bool _filesUpdated = true;					// Flag to indicate that the files have been updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Read-only property to return the name of this object.
		/// </summary>
		/// <value>
		/// A <see cref="string">string</see> containing the name of this object.
		/// </value>
		/// <remarks>The name of an object need not be unique, however if it is used as a key value for a collection then it should be unique.</remarks>
		public override string Name
		{
			get
			{
				return base.Name;
			}
			protected internal set
			{
				base.Name = value;
				FilesUpdated();
			}
		}


		/// <summary>
		/// Property to return the parent of the path.
		/// </summary>
		public FileSystemPath Parent
		{
			get
			{
				return _parent;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				_parent = value;
				FilesUpdated();
			}
		}

		/// <summary>
		/// Property to return the invalid characters for a filename.
		/// </summary>
		public static char[] InvalidCharacters
		{
			get
			{
				if (_invalidCharacters == null)
				{
					char[] pathChars = null;		// Known invalid characters.

					pathChars = Path.GetInvalidPathChars();

					_invalidCharacters = new char[pathChars.Length + 3];

					pathChars.CopyTo(_invalidCharacters,0);

					_invalidCharacters[pathChars.Length] = '*';
					_invalidCharacters[pathChars.Length + 1] = '?';
					_invalidCharacters[pathChars.Length + 2] = ':';
				}

				return _invalidCharacters;
			}
		}
		
		/// <summary>
		/// Property to return the full path for this path node.
		/// </summary>
		public string FullPath
		{
			get
			{
				if (_parent == null)
					return @"\";
				else
					return _parent.FullPath + Name + @"\";
			}
		}

		/// <summary>
		/// Property to return the list of files within the path.
		/// </summary>
		public FileSystemFileList Files
		{
			get
			{
				return _files;
			}
		}

		/// <summary>
		/// Property to return the paths that are under this path.
		/// </summary>
		public FileSystemPathList ChildPaths
		{
			get
			{
				return _paths;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to recursively grab all file entries.
		/// </summary>
		/// <param name="files">List of files.</param>
		private void GetAllFiles(FileList files)
		{
			// Read all children.
			if (_filesUpdated)
			{
				// Clear current file list.
				_allFiles.Clear();

				foreach (FileSystemPath childPath in _paths)
				{
					childPath.GetAllFiles(childPath._allFiles);
					foreach (FileSystemFile file in childPath._allFiles)
						_allFiles.AddFile(file);
				}

				// Add to the list.
				foreach (FileSystemFile file in _files)
					files.AddFile(file);
			}

			_filesUpdated = false;
		}

		/// <summary>
		/// Function to that will tell this path that its files were updated.
		/// </summary>
		internal void FilesUpdated()
		{
			_filesUpdated = true;

			// Inform the parent that an update needs to happen.
			if ((_parent != null) && (!_parent._filesUpdated))
				_parent.FilesUpdated();			

			if (_paths != null)
			{
				foreach (FileSystemPath child in _paths)
				{
					if (!child._filesUpdated)
						child.FilesUpdated();
				}
			}
		}

		/// <summary>
		/// Function to return all files from this path and child paths.
		/// </summary>
		public FileList GetFiles()
		{
			if (_filesUpdated)
				GetAllFiles(_allFiles);

			return _allFiles;
		}

		/// <summary>
		/// Function to create a deep copy of this path.
		/// </summary>
		/// <param name="newName">New name of the path.</param>
		/// <returns>A deep copy of the path.</returns>
		public FileSystemPath Copy(string newName)
		{
			FileSystemPath newPath = null;		// New path.

			if (string.IsNullOrEmpty(newName))
				throw new ArgumentNullException("newName");

			// Create path.
			newPath = new FileSystemPath(null, newName);

			// Copy all children.
			foreach(FileSystemPath child in _paths)
				newPath.ChildPaths.Add(child.Copy(child.Name));

			// Copy files.
			foreach(FileSystemFile file in _files)
				newPath.Files.Add(file.Filename + file.Extension, file.Data, file.Size, file.CompressedSize, file.DateTime, file.IsEncrypted);

			newPath.FilesUpdated();
			return newPath;
		}

		/// <summary>
		/// Function to return whether or not a path is valid.
		/// </summary>
		/// <param name="pathName">Name of the path to validate.</param>
		/// <returns>TRUE if valid, FALSE if not.</returns>
		public static bool ValidPath(string pathName)
		{
			if ((pathName == string.Empty) || (pathName == null))
				return false;

			if (pathName.ToLower().IndexOfAny(InvalidCharacters) > -1)
				return false;

			return true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent">Parent of this path.</param>
		/// <param name="name">Name of the path.</param>
		internal FileSystemPath(FileSystemPath parent, string name)
			: base(name)
		{
			if ((name == null) || (name == string.Empty))
				throw new ArgumentNullException("name");

			_parent = parent;

			// Create file list.
			_files = new FileSystemFileList(this);

			// Create path list.
			_paths = new FileSystemPathList(this);

			// Create file list.
			_allFiles = new FileList();
		}
		#endregion
	}
}
