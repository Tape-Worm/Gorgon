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
// Created: Friday, April 20, 2007 1:34:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object representing a list of files.
	/// </summary>
	public class FileSystemFileList
		: Collection<FileSystemFile>
	{
		#region Variables.
		private FileSystemPath _owner = null;		// Path that owns this list.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the file by index.
		/// </summary>
		public override FileSystemFile this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return the file with the specified key name.
		/// </summary>
		public override FileSystemFile this[string key]
		{
			get
			{
				if ((key == string.Empty) || (key == null))
					throw new ArgumentNullException("key");

				key = TransformFilename(key);

				if (!Contains(key))
					throw new FileSystemFileNotFoundException(key);

				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a transformed file name.
		/// </summary>
		/// <param name="fileName">Name of the file to transform.</param>
		/// <returns>The transformed file name.</returns>
		private string TransformFilename(string fileName)
		{
			string result = string.Empty;		// Resulting filename.

			// Translate to back slash paths.
			result = fileName.Replace("/", @"\");

			// Remove all occurances of '\\', it causes the .NET GetDirectoryName function to screw up.
			while (result.IndexOf(@"\\") > -1)
				result = result.Replace(@"\\", @"\");

			// Remove path information.
			result = Path.GetFileName(result);

			if (!FileSystemFile.ValidFilename(result))
				throw new FileSystemFilenameInvalidException(result);

			return result;
		}

		/// <summary>
		/// Function to remove an object from the list by key.
		/// </summary>
		/// <param name="key">Key of the object to remove.</param>
		protected override void RemoveItem(string key)
		{
			if ((key == string.Empty) || (key == null))
				throw new ArgumentNullException("key");

			key = TransformFilename(key);

			if (!Contains(key))
				throw new FileSystemFileNotFoundException(key);
			_owner.FilesUpdated();
			base.RemoveItem(key);
		}

		/// <summary>
		/// Function to remove an object from the list by index.
		/// </summary>
		/// <param name="index">Index to remove at.</param>
		protected override void RemoveItem(int index)
		{
			_owner.FilesUpdated();
			base.RemoveItem(index);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		protected override void ClearItems()
		{
			_owner.FilesUpdated();
			base.ClearItems();
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

			return base.Contains(TransformFilename(key));
		}

		/// <summary>
		/// Function to add a file to the list.
		/// </summary>
		/// <param name="fileName">Filename of the file to add.</param>
		/// <param name="data">Data for the file.</param>
		/// <param name="originalSize">Original file size.</param>
		/// <param name="compressedSize">Compressed size if compressed.</param>
		/// <param name="encrypted">TRUE if encrypted, FALSE if not.</param>
		/// <param name="dateTime">File create/update date and time.</param>
		public FileSystemFile Add(string fileName, byte[] data, int originalSize, int compressedSize, DateTime dateTime, bool encrypted)
		{
			FileSystemFile newFile = null;		// New file.

			// Get the file name.
			fileName = TransformFilename(fileName);

			newFile = new FileSystemFile(_owner, fileName, data, originalSize, compressedSize, dateTime, encrypted);

			if (Contains(fileName))
				throw new FileSystemFileExistsException(newFile.FullPath);

			AddItem(fileName, newFile);
			_owner.FilesUpdated();
			return newFile;
		}	
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Path that owns this list.</param>
		internal FileSystemFileList(FileSystemPath owner)
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
