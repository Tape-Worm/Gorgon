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
// Created: Saturday, November 04, 2006 11:03:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Object representing a file system entry.
    /// </summary>
    public class FileSystemEntry
    {
		// TODO: Add encrypted flag, also should have a way to pass in encryption
		// keys and the like, this will likely be at the file system level.
        #region Variables.
        private string _path;						// File path.
        private string _extension;					// File extension.
        private string _filename;					// Name of the file.
		private int _fileSize = 0;					// Size of the file.
		// TODO:  We should make this a byte array.
		// This way when we retrieve an object through the indexer, it should deserialize
		// at that point.  The only concern is overhead for converting the object back
		// from binary data when indexed.  We could cache the object once it has been 
		// indexed/created and use that.  However we would need to destroy the object
		// once new data has been assigned to the entry.
		private byte[] _data;						// Object to be stored (used when adding/saving).
		private int _compressed = 0;				// Compressed size of the file.
		private long _offset = 0;					// Offset of the file within an archive.
		private string _fullPath = string.Empty;	// Full path and filename.
		private bool _encrypted = false;			// Flag to indicate whether the entry is encrypted or not.
		#endregion

        #region Properties.
		/// <summary>
		/// Property to set or return whether this entry is encrypted.
		/// </summary>
		public bool Encrypted
		{
			get
			{
				return _encrypted;
			}
			set
			{
				_encrypted = value;
			}
		}

		/// <summary>
		/// Property to return the absolute path from the root.
		/// </summary>
		public string FullPath
		{
			get
			{
				return _fullPath;
			}
		}

		/// <summary>
		/// Property to return the uncompressed size of the file.
		/// </summary>
		public int FileSize
		{
			get
			{
				return _fileSize;
			}
		}

		/// <summary>
		/// Property to return the compressed size of the file.
		/// </summary>
		public int CompressedFileSize
		{
			get
			{
				return _compressed;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the file within an archive filesystem.
		/// </summary>
		public long Offset
		{
			get
			{
				return _offset;
			}
		}

		/// <summary>
		/// Property to return whether or not the file is compressed or not.
		/// </summary>
		public bool IsCompressed
		{
			get
			{
				return (_compressed != 0);
			}
		}

        /// <summary>
        /// Property to return the file path.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Property to return the file extension.
        /// </summary>
        public string Extension
        {
            get
            {
                return _extension;
            }
        }

        /// <summary>
        /// Property to return the filename.
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }
        }

		/// <summary>
		/// Property to set or return the binary object data for this entry.
		/// </summary>
		public byte[] ObjectData
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Constructor.
        /// </summary>        
        /// <param name="filePath">Path to the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="extension">Extension of the file.</param>
		/// <param name="objectData">Binary data for the object.</param>
		/// <param name="offset">Offset within the file system.</param>
		/// <param name="fileSize">Size of the file.</param>
		/// <param name="compressedSize">Compressed size of the file, 0 means uncompressed.</param>
		/// <param name="encrypted">TRUE if the entry is encrypted, FALSE if not.</param>
        internal FileSystemEntry(string filePath, string fileName, string extension, byte[] objectData, long offset, int fileSize, int compressedSize, bool encrypted)
        {
            _path = filePath;
            _filename = fileName;
			_extension = extension;
			_fileSize = fileSize;
			_data = objectData;
			_fullPath = filePath + fileName + extension;
			_compressed = compressedSize;
			_encrypted = encrypted;
			_offset = offset;
        }
        #endregion    
    }
}
