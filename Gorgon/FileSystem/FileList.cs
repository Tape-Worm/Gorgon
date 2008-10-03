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
// Created: Sunday, April 22, 2007 6:54:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

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
					throw new System.IO.FileNotFoundException("The file '" + key + "' was not found");

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
