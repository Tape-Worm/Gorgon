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
// Created: Tuesday, April 17, 2007 12:56:25 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// File system read/write event arguments.
	/// </summary>
	public class FileSystemReadWriteEventArgs
		: EventArgs
	{
		#region Variables.
		private FileSystemFile _file = null;			// File system file.
		private object _data = null;					// User specific data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the file being read or written.
		/// </summary>
		public FileSystemFile File
		{
			get
			{
				return _file;
			}
		}

		/// <summary>
		/// Property to set or return user specific data.
		/// </summary>
		public object UserData
		{
			set
			{
				_data = value;
			}
			get
			{
				return _data;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="file">File being written.</param>
		/// <param name="userdata">User specific data.</param>
		public FileSystemReadWriteEventArgs(FileSystemFile file, object userdata)
		{
			_file = file;
			_data = userdata;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="file">File being written.</param>
		public FileSystemReadWriteEventArgs(FileSystemFile file)
			: this(file, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system data IO arguments.
	/// </summary>
	public class FileSystemDataIOEventArgs
		: EventArgs
	{
		#region Variables.
		private FileSystemFile _file = null;		// File system file.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the file being read or written.
		/// </summary>
		public FileSystemFile File
		{
			get
			{
				return _file;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="file">File system file being read or written.</param>
		public FileSystemDataIOEventArgs(FileSystemFile file)			
		{
			_file = file;
		}
		#endregion
	}

	/// <summary>
	/// Delegate used for file system read/write events.
	/// </summary>
	/// <param name="sender">Sender of the event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void FileSystemReadWriteHandler(object sender, FileSystemReadWriteEventArgs e);

	/// <summary>
	/// Delegate used for file system data IO events.
	/// </summary>
	/// <param name="sender">Sender of the event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void FileSystemDataIOHandler(object sender, FileSystemDataIOEventArgs e);
}
