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
				if (string.IsNullOrEmpty(key))
					throw new ArgumentNullException("key");

				key = TransformFilename(key);

				if (!Contains(key))
					throw new FileNotFoundException("The file '" + key + "' was not found");

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
				throw new ArgumentException("The filename '" + result + "' is not valid.");

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

			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException("fileName");

			// Get the file name.
			fileName = TransformFilename(fileName);

			newFile = new FileSystemFile(_owner, fileName, data, originalSize, compressedSize, dateTime, encrypted);

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
