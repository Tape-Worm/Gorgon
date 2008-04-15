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
// Created: Wednesday, November 01, 2006 5:33:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// File system type is invalid for the operation specified.
	/// </summary>
	public class FileSystemTypeInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemTypeInvalidException"/> class.
		/// </summary>
		/// <param name="fileSystemType">Type of the file system that is invalid for the operation.</param>
		/// <param name="ex">The inner exception.</param>
		public FileSystemTypeInvalidException(string fileSystemType, Exception ex)
			: base("The operation is not valid for file systems of the type '" + fileSystemType + "'.", ex)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FileSystemTypeInvalidException"/> class.
		/// </summary>
		/// <param name="fileSystemType">Type of the file system that is invalid for the operation.</param>
		public FileSystemTypeInvalidException(string fileSystemType)
			: this(fileSystemType, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system already exists.
	/// </summary>
	public class FileSystemExistsException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that previously existed.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemExistsException(string fileSystemName, Exception ex)
			: base("The file system '" + fileSystemName + "' already exists.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that previously existed.</param>
		public FileSystemExistsException(string fileSystemName)
			: this(fileSystemName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system does not exist.
	/// </summary>
	public class FileSystemDoesNotExistException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that could not be found.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemDoesNotExistException(string fileSystemName, Exception ex)
			: base("The file system '" + fileSystemName + "' does not exist.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that could not be found.</param>
		public FileSystemDoesNotExistException(string fileSystemName)
			: this(fileSystemName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system plug-in load failure.
	/// </summary>
	public class FileSystemPlugInLoadException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInPath">Path and filename of the plug-in that failed to load.</param>
		/// <param name="message">Exception information.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemPlugInLoadException(string plugInPath, string message, Exception ex)
			: base("The file system plug-in '" + plugInPath + "' failed to load.\n" + message, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInPath">Path and filename of the plug-in that failed to load.</param>
		/// <param name="message">Exception information.</param>
		public FileSystemPlugInLoadException(string plugInPath, string message)
			: this(plugInPath, message, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system plug-in not found.
	/// </summary>
	public class FileSystemPlugInNotFoundException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugIn">Plug-in name that was not found.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemPlugInNotFoundException(string plugIn, Exception ex)
			: base("The file system plug-in '" + plugIn + "' was not found.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugIn">Plug-in name that was not found.</param>
		public FileSystemPlugInNotFoundException(string plugIn)
			: this(plugIn, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system type has missing attribute information.
	/// </summary>
	public class FileSystemAttributeMissingException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of attribute.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemAttributeMissingException(Type type, Exception ex)
			: base("The file system type is missing the " + type.Name + " attribute.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of attribute.</param>
		public FileSystemAttributeMissingException(Type type)
			: this(type, null)
		{
		}
		#endregion
	}

    /// <summary>
    /// File system index reading exception.
    /// </summary>
    public class FileSystemIndexReadException
        : GorgonException
    {
		#region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemIndexReadException"/> class.
        /// </summary>
        /// <param name="ex">Source exception.</param>
		public FileSystemIndexReadException(Exception ex)
			: base("The was an error in the file system index.", ex)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemIndexReadException"/> class.
        /// </summary>
        public FileSystemIndexReadException()
			: this(null)
		{
		}
		#endregion
    }

	/// <summary>
	/// File system root path is invalid.
	/// </summary>
	public class FileSystemRootIsInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rootPath">Path to the root.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemRootIsInvalidException(string rootPath, Exception ex)
			: base("The file system root '" + rootPath + "' is not valid.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rootPath">Path to the root.</param>
		public FileSystemRootIsInvalidException(string rootPath)
			: this(rootPath, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FileSystemRootIsInvalidException()
			: base("The file system root cannot be NULL or empty.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system path not found.
	/// </summary>
	public class FileSystemPathNotFoundException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path of the entry.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemPathNotFoundException(string path, Exception ex)
			: base("The path '" + path + "' was not found.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		public FileSystemPathNotFoundException(string path)
			: this(path, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system path not found.
	/// </summary>
	public class FileSystemFileNotFoundException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path of the entry.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemFileNotFoundException(string path, Exception ex)
			: base("The file '" + path + "' was not found.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		public FileSystemFileNotFoundException(string path)
			: this(path, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system path already exists.
	/// </summary>
	public class FileSystemPathExistsException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemPathExistsException(string path, Exception ex)
			: base("The path '" + path + "' already exists.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		public FileSystemPathExistsException(string path)
			: this(path, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system file already exists.
	/// </summary>
	public class FileSystemFileExistsException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemFileExistsException(string path, Exception ex)
			: base("The file '" + path + "' already exists.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		public FileSystemFileExistsException(string path)
			: this(path, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system path is invalid.
	/// </summary>
	public class FileSystemPathInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemPathInvalidException(string path, Exception ex)
			: base("The file system path '" + path + "' contains invalid characters.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">Path/filename of the entry.</param>
		public FileSystemPathInvalidException(string path)
			: this(path, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FileSystemPathInvalidException()
			: base("The file system path cannot be NULL or empty.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system filename is invalid.
	/// </summary>
	public class FileSystemFilenameInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileName">The filename.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemFilenameInvalidException(string fileName, Exception ex)
			: base("The file system filename '" + fileName + "' contains invalid characters.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileName">The filename.</param>
		public FileSystemFilenameInvalidException(string fileName)
			: this(fileName, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FileSystemFilenameInvalidException()
			: base("The file system filename cannot be NULL or empty.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system path mount failure.
	/// </summary>
	public class FileSystemPathMountException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mountPath">Path and filename of the plug-in that failed to load.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemPathMountException(string mountPath, Exception ex)
			: base("The file system path '" + mountPath + "' could not be mounted.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mountPath">Path and filename of the plug-in that failed to load.</param>
		public FileSystemPathMountException(string mountPath)
			: this(mountPath, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system copy failure.
	/// </summary>
	public class FileSystemCopyException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source being copied.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemCopyException(string source, Exception ex)
			: base("Cannot copy '" + source + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source being copied.</param>
		public FileSystemCopyException(string source)
			: this(source, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system move failure.
	/// </summary>
	public class FileSystemMoveException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source being copied.</param>
		/// <param name="destination">Destination of the source.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemMoveException(string source, string destination, Exception ex)
			: base("Cannot move '" + source + "' to '" + destination + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source being copied.</param>
		/// <param name="destination">Destination of the source.</param>
		public FileSystemMoveException(string source, string destination)
			: this(source, destination, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system write failure.
	/// </summary>
	public class FileSystemWriteException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename being written.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemWriteException(string filename, Exception ex)
			: base("Cannot write '" + filename + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename being written.</param>
		public FileSystemWriteException(string filename)
			: this(filename, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system read failure.
	/// </summary>
	public class FileSystemReadException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename being written.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemReadException(string filename, Exception ex)
			: base("Cannot read '" + filename + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename being written.</param>
		public FileSystemReadException(string filename)
			: this(filename, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system delete failure.
	/// </summary>
	public class FileSystemDeleteException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename being written.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemDeleteException(string filename, Exception ex)
			: base("Cannot delete '" + filename + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filename">Filename being written.</param>
		public FileSystemDeleteException(string filename)
			: this(filename, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// File system directory header is invalid.
	/// </summary>
	public class FileSystemHeaderInvalidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public FileSystemHeaderInvalidException(Exception ex)
			: base("The file system directory index header is invalid.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public FileSystemHeaderInvalidException()
			: this(null)
		{
		}
		#endregion
	}

    /// <summary>
    /// File system access denied exception.
    /// </summary>
    public class FileSystemAccessDeniedException
        : GorgonException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileSystemName">Name of the file system that previously existed.</param>
        /// <param name="ex">Source exception.</param>
        public FileSystemAccessDeniedException(string fileSystemName, Exception ex)
            : base("Access denied to '" + fileSystemName + "'!", ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileSystemName">Name of the file system that previously existed.</param>
        public FileSystemAccessDeniedException(string fileSystemName)
            : this(fileSystemName, null)
        {
        }
        #endregion
    }
}
