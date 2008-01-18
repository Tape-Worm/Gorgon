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
