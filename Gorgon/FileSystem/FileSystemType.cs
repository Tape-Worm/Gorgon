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
// Created: Saturday, April 07, 2007 2:12:28 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object containing file system type information.
	/// </summary>
	public class FileSystemType
		: IDisposable
	{
		#region Variables.
		private FileSystemPlugIn _fileSystemPlugIn;			// File system plug-in.
		private FileSystemEditorInfoAttribute _info;		// File system information.
		private Type _fileSystemType;						// File system type.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the plug-in for a file system.
		/// </summary>
		public FileSystemPlugIn PlugIn
		{
			get
			{
				return _fileSystemPlugIn;
			}
		}

		/// <summary>
		/// Property to return the file system type.
		/// </summary>
		public Type Type
		{
			get
			{
				return _fileSystemType;
			}
		}

		/// <summary>
		/// Property to return information about a file system.
		/// </summary>
		public FileSystemEditorInfoAttribute Information
		{
			get
			{
				return _info;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugin">Plug-in to use for the file system.</param>
		internal FileSystemType(FileSystemPlugIn plugin)
		{
			_info = plugin.FileSystemInfo;
			_fileSystemPlugIn = plugin;
			_fileSystemType = null;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemType">File system type.</param>
		internal FileSystemType(Type fileSystemType)
		{
			object[] attributes = null;		// File system type attributes.

			_fileSystemPlugIn = null;
			_fileSystemType = fileSystemType;

			// Get the attributes.
			attributes = fileSystemType.GetCustomAttributes(typeof(FileSystemEditorInfoAttribute), true);

			if ((attributes == null) || (attributes.Length == 0))
				throw new CannotMountFileSystemException("File system type does not contain the FileSystemEditInfo attribute.", null);

			// Get the attribute.
			_info = (FileSystemEditorInfoAttribute)attributes[0];
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
				// Remove any related file systems.
				for (int i = 0; i < Gorgon.FileSystems.Count; i++)
				{
					// Remove by plug-in type.
					if (_fileSystemPlugIn != null)
					{
						if (((IPlugIn)Gorgon.FileSystems[i]).PlugInEntryPoint == _fileSystemPlugIn)
							Gorgon.FileSystems[i].Dispose();
					}
					else
					{
						// Remove by file system type.
						if (Gorgon.FileSystems[i].GetType() == _fileSystemType)
							Gorgon.FileSystems[i].Dispose();
					}
				}

				// Unload the plug-in.
				if (_fileSystemPlugIn != null)
					Gorgon.PlugIns.Unload(_fileSystemPlugIn.Name);
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
