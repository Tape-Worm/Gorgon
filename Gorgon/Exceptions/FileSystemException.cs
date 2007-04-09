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
	/// Enumeration containing file system error codes.
	/// </summary>
	public enum FileSystemErrors
	{
        /// <summary>
        /// Invalid filename extension.
        /// </summary>
        InvalidExtension = 0x7FFFF001,
        /// <summary>
        /// Cannot register file types.
        /// </summary>
        CannotRegisterTypes = 0x7FFFF002,
        /// <summary>
        /// Cannot add a file system entry.
        /// </summary>
        CannotAddEntry = 0x7FFFF003,
        /// <summary>
        /// Filename/path is invalid.
        /// </summary>
        InvalidFilePath = 0x7FFFF004,
        /// <summary>
        /// Cannot mount filesystem.
        /// </summary>
        CannotMount = 0x7FFFF005,
        /// <summary>
        /// File system entry does not exist.
        /// </summary>
        EntryDoesNotExist = 0x7FFFF006,
        /// <summary>
        /// Cannot delete file system entry.
        /// </summary>
        CannotDelete = 0x7FFFF007,
        /// <summary>
        /// Invalid file type.
        /// </summary>
        InvalidFileType = 0x7FFFF008,
        /// <summary>
        /// Invalid root path.
        /// </summary>
        InvalidRootPath = 0x7FFFF009,
        /// <summary>
        /// Cannot copy the specified file system.
        /// </summary>
        CannotCopyFileSystem = 0x7FFFF00A,
		/// <summary>
		/// Cannot move file/path.
		/// </summary>
		CannotMove = 0x7FFFF00B,
		/// <summary>
		/// Entry already exists.
		/// </summary>
		EntryAlreadyExists = 0x7FFFF00C
	}

	/// <summary>
	/// Base exception for file systems.
	/// </summary>
	public class FileSystemException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public FileSystemException(string message, FileSystemErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

    /// <summary>
    /// Invalid file extension exception.
    /// </summary>
    public class InvalidExtensionException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Source exception.</param>
        public InvalidExtensionException(string message, Exception ex)
            : base(message, FileSystemErrors.InvalidExtension, ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ex">Source exception.</param>
        public InvalidExtensionException(Exception ex)
            : this("The extension passed was invalid.", ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Cannot register file types exception.
    /// </summary>
    public class CannotRegisterFileTypeException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Source exception.</param>
        public CannotRegisterFileTypeException(string message, Exception ex)
            : base(message, FileSystemErrors.CannotRegisterTypes, ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ex">Source exception.</param>
        public CannotRegisterFileTypeException(Exception ex)
            : this("Unable to register a file type.", ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Cannot add file system entry exception.
    /// </summary>
    public class CannotAddFileSystemEntryException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entry">File system entry path.</param>
        /// <param name="ex">Source exception.</param>
        public CannotAddFileSystemEntryException(string entry, Exception ex)
            : base("Cannot add file system entry " + entry, FileSystemErrors.CannotAddEntry, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Invalid filesystem path exception.
    /// </summary>
    public class InvalidPathException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">File system path.</param>
        /// <param name="ex">Source exception.</param>
        public InvalidPathException(string path, Exception ex)
            : base("The path '" + path + "' is invalid.", FileSystemErrors.InvalidFilePath, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Cannot mount file system exception.
    /// </summary>
    public class CannotMountFileSystemException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Source exception.</param>
        public CannotMountFileSystemException(string message, Exception ex)
            : base(message, FileSystemErrors.CannotMount, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Cannot mount file system exception.
    /// </summary>
    public class CannotMountFilePathException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Source exception.</param>
        public CannotMountFilePathException(string message, Exception ex)
            : base(message, FileSystemErrors.CannotMount, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// File system entry does not exist exception.
    /// </summary>
    public class FileSystemEntryDoesNotExistException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="ex">Source exception.</param>
        public FileSystemEntryDoesNotExistException(string filename, Exception ex)
            : base("The file system entry '" + filename + "' does not exist.", FileSystemErrors.EntryDoesNotExist, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Cannot delete file system entry exception.
    /// </summary>
    public class CannotDeleteEntryException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="ex">Source exception.</param>
        public CannotDeleteEntryException(string filename, Exception ex)
            : base("Could not delete the file system entry '" + filename + "'.", FileSystemErrors.CannotDelete, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Invalid file type exception.
    /// </summary>
    public class InvalidFileTypeException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Source exception.</param>
        public InvalidFileTypeException(string message, Exception ex)
            : base(message, FileSystemErrors.InvalidFileType, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Invalid root path exception.
    /// </summary>
    public class InvalidRootException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootpath">Root path.</param>
        /// <param name="ex">Source exception.</param>
        public InvalidRootException(string rootpath, Exception ex)
            : base("The root path '" + rootpath + "' is invalid.", FileSystemErrors.InvalidRootPath, ex)
        {
        }
        #endregion
    }

    /// <summary>
    /// Cannot copy file system exception.
    /// </summary>
    public class CannotCopyFileSystemException
        : FileSystemException
    {
        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="ex">Source exception.</param>
        public CannotCopyFileSystemException(string message, Exception ex)
            : base(message, FileSystemErrors.CannotCopyFileSystem, ex)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="system">File system to copy.</param>
        /// <param name="ex">Source exception.</param>
        public CannotCopyFileSystemException(FileSystem system, Exception ex)
            : this("Cannot copy the file system '" + system.Name + "'.", ex)
        {
        }
        #endregion
    }

	/// <summary>
	/// Cannot move exception.
	/// </summary>
	public class CannotMoveException
		: FileSystemException
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotMoveException(string message, Exception ex)
			: base(message, FileSystemErrors.CannotMove, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source">Source path.</param>
		/// <param name="dest">Destination path.</param>
		/// <param name="message">Optional message.</param>
		/// <param name="ex">Source exception.</param>
		public CannotMoveException(string source, string dest, string message, Exception ex)
			: this("Cannot move '" + source + "' to '" + dest + "'." + ((message == string.Empty) ? string.Empty : "\n" + message), ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot move exception.
	/// </summary>
	public class EntryExistsException
		: FileSystemException
	{
		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="ex">Source exception.</param>
		public EntryExistsException(string message, Exception ex)
			: base(message, FileSystemErrors.CannotMove, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entry">Entry that is duplicated.</param>
		/// <param name="ex">Source exception.</param>
		public EntryExistsException(FileSystemEntry entry, Exception ex)
			: this("Entry '" + entry.FullPath + "' already exists.", ex)
		{
		}
		#endregion
	}
}
