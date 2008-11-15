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
// Created: Friday, April 20, 2007 2:01:12 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object representing a list of path paths.
	/// </summary>
	public class FileSystemPathList
		: Collection<FileSystemPath>
	{
		#region Variables.
		private FileSystemPath _owner = null;		// Owning path system path.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the path with the specified key name.
		/// </summary>
		public override FileSystemPath this[string key]
		{
			get
			{
				if ((key == string.Empty) || (key == null))
					throw new ArgumentNullException("key");

				key = TransformPath(key);

				if (!Contains(key))
					throw new FileNotFoundException("The file '" + key + "' was not found in this file system.");

				return base[key];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a transformed path name.
		/// </summary>
		/// <param name="pathName">Name of the path to transform.</param>
		/// <returns>The transformed path name.</returns>
		private string TransformPath(string pathName)
		{
			string result = string.Empty;		// Resulting pathname.

			// Append a path separator.
			pathName += @"\";

			// Translate to back slash paths.			
			result = pathName.Replace("/", @"\");

			// Remove all occurances of '\\', it causes the .NET GetDirectoryName function to screw up.
			while (result.IndexOf(@"\\") > -1)
				result = result.Replace(@"\\", @"\");

			// Get the path name.
			result = Path.GetDirectoryName(pathName);

			// Strip out the immediate path name.
			if (result.LastIndexOf(@"\") > -1)
				result = result.Substring(result.LastIndexOf(@"\") + 1);

			if (!FileSystemPath.ValidPath(result))
				throw new ArgumentException("The path '" + result + "' is not valid.");
			return result;
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		protected override void RemoveItem(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			key = TransformPath(key);

			base.RemoveItem(key);
			_owner.FilesUpdated();
		}

		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
			_owner.FilesUpdated();
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		protected override void ClearItems()
		{
			base.ClearItems();
			_owner.FilesUpdated();
		}


		/// <summary>
		/// Function to return whether a key exists in the collection or not.
		/// </summary>
		/// <param name="key">Key of the object in the collection.</param>
		/// <returns>TRUE if the object exists, FALSE if not.</returns>
		public override bool Contains(string key)
		{
			if ((key == string.Empty) || (key == null))
				throw new ArgumentNullException("key");

			return base.Contains(TransformPath(key));
		}

		/// <summary>
		/// Function to rename a path.
		/// </summary>
		/// <param name="oldName">Old name.</param>
		/// <param name="newName">New name.</param>
		public void Rename(string oldName, string newName)
		{
			FileSystemPath oldPath = null;		// Old path.

			if ((oldName == string.Empty) || (oldName == null))
				throw new ArgumentNullException("oldName");

			if ((newName == string.Empty) || (newName == null))
				throw new ArgumentNullException("newName");

			oldName = TransformPath(oldName);
			newName = TransformPath(newName);

			// Do nothing if the same name.
			if (string.Compare(oldName, newName, true) == 0)
				return;

			if (Contains(newName))
				throw new ArgumentException("The path '" + newName + "' already exists.");

			// Extract old name.
			oldPath = this[oldName];
			Remove(oldName);

			// Rename.
			oldPath.Name = newName;

			// Re-add with new name.
			Add(oldPath);

			_owner.FilesUpdated();
		}

		/// <summary>
		/// Function to add a path to the list.
		/// </summary>
		/// <param name="pathName">Pathname of the path to add.</param>
		public FileSystemPath Add(string pathName)
		{
			FileSystemPath newPath = null;		// New path.

			// Get the path name.
			pathName = TransformPath(pathName);

			// Do not allow us to create a root.
			if (pathName == @"\")
				throw new ArgumentException(@"Cannot create a root path '\'.");

			newPath = new FileSystemPath(_owner, pathName);

			AddItem(pathName, newPath);
			_owner.FilesUpdated();

			return newPath;
		}

		/// <summary>
		/// Function to add an existing path to the list.
		/// </summary>
		/// <param name="path">Path to add.</param>
		public void Add(FileSystemPath path)
		{
			path.Parent = _owner;

			AddItem(path.Name, path);
			_owner.FilesUpdated();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owner of the path system path list.</param>
		internal FileSystemPathList(FileSystemPath owner)
			: base(false)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			_owner = owner;
			_owner.FilesUpdated();
		}
		#endregion
	}
}
