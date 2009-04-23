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
// Created: Friday, April 20, 2007 1:23:17 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GorgonLibrary.Internal;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object representing a file under the file system.
	/// </summary>
	public class FileSystemFile
		: NamedObject
	{
		#region Variables.
		private FileSystemPath _owner = null;					// Path that owns this file.
		private string _extension = string.Empty;				// Extension.
		private string _filename = string.Empty;				// File name.
		private static char[] _invalidCharacters = null;		// Invalid filename characters.
		private long _offset;									// File offset (for packed files).
		private int _size;										// Size of the file.
		private int _compressedSize;							// Compressed size of the file.
		private bool _encrypted;								// Flag to indicate that the file is encrypted.
		private DateTime _fileDateTime;							// File update/create date and time.
		private string _comment = string.Empty;					// Comment for the file.
		private byte[] _data;									// Byte data for the file.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the file creation/update date and time.
		/// </summary>
		public DateTime DateTime
		{
			get
			{
				return _fileDateTime;
			}
			set
			{
				_fileDateTime = value;
				_owner.FilesUpdated();
			}
		}

		/// <summary>
		/// Property to set or return the comment for the entry.
		/// </summary>
		/// <remarks>Has a maximum of 2048 characters.</remarks>
		public string Comment
		{
			get
			{
				return _comment;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				// No more than 2048 characters.
				if (value.Length > 2048)
					value = value.Substring(0, 2048);

				_comment = value;
				_owner.FilesUpdated();
			}
		}

		/// <summary>
		/// Property to return the raw file data in bytes.
		/// </summary>
		public byte[] Data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
				_owner.FilesUpdated();
			}
		}

		/// <summary>
		/// Property to set or return the offset of the file within a packed file system.
		/// </summary>
		public long Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
				_owner.FilesUpdated();
			}
		}

		/// <summary>
		/// Property to return the uncompressed size of the file in bytes.
		/// </summary>
		public int Size
		{
			get
			{
				return _size;
			}
		}

		/// <summary>
		/// Property to return the size of the compressed file in bytes.
		/// </summary>
		public int CompressedSize
		{
			get
			{
				return _compressedSize;
			}
		}

		/// <summary>
		/// Property to return whether or not a file is compressed.
		/// </summary>
		public bool IsCompressed
		{
			get
			{
				return (_compressedSize != 0);
			}
		}

		/// <summary>
		/// Property to return whether or not a file is encrypted.
		/// </summary>
		public bool IsEncrypted
		{
			get
			{
				return _encrypted;
			}
		}

		/// <summary>
		/// Property to return the name of the file.
		/// </summary>
		public string Filename
		{
			get
			{
				return _filename;
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
		/// Property to return the path that owns this file.
		/// </summary>
		public FileSystemPath Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the full path for the file.
		/// </summary>
		public string FullPath
		{
			get
			{
				return _owner.FullPath + Name;
			}
		}

		/// <summary>
		/// Property to return the invalid characters for a filename.
		/// </summary>
		public static char[] InvalidCharacters
		{
			get
			{
				if (_invalidCharacters == null)
					_invalidCharacters = Path.GetInvalidFileNameChars();

				return _invalidCharacters;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the filename is valid or not.
		/// </summary>
		/// <param name="fileName">File name to examine.</param>
		/// <returns>TRUE if valid, FALSE is not.</returns>
		public static bool ValidFilename(string fileName)
		{
			// Get the file name.
			fileName = Path.GetFileName(fileName);

			if ((fileName == string.Empty) || (fileName == null))
				return false;

			if (fileName.ToLower().IndexOfAny(InvalidCharacters) > -1)
				return false;

			return true;
		}

		/// <summary>
		/// Function to open a file stream into the file object data.
		/// </summary>
		/// <returns>A stream pointing at the file object data.</returns>
		public Stream OpenStream()
		{
			// We can't return a stream to empty data.
			if (_data == null)
				throw new GorgonException(GorgonErrors.CannotReadData, "Error reading the file data from '" + FullPath + "'.");

			return new MemoryStream(_data);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Path that owns this file.</param>
		/// <param name="name">File name.</param>
		/// <param name="data">Data for the file.</param>
		/// <param name="originalSize">Original file size.</param>
		/// <param name="compressedSize">Compressed size if compressed.</param>
        /// <param name="dateTime">File create/update date and time.</param>
        /// <param name="encrypted">TRUE if encrypted, FALSE if not.</param>
		internal FileSystemFile(FileSystemPath owner, string name, byte[] data, int originalSize, int compressedSize, DateTime dateTime, bool encrypted)
			: base(name)
		{
			if ((name == null) || (name == string.Empty))
				throw new ArgumentNullException("name");

			if (owner == null)
				throw new ArgumentNullException("owner");

			_owner = owner;
			_filename = Path.GetFileNameWithoutExtension(name);
			_extension = Path.GetExtension(name);

			if ((string.IsNullOrEmpty(_filename)) && (string.IsNullOrEmpty(_extension)))
				throw new ArgumentException("The filename '" + name + "' is invalid.", "name");

			if ((string.IsNullOrEmpty(_filename)) && (!String.IsNullOrEmpty(_extension)) && (name.StartsWith(".")))
			{				
				_filename = _extension;
				_extension = string.Empty;
			}

			_size = originalSize;
			_compressedSize = compressedSize;
			_encrypted = encrypted;
			_fileDateTime = dateTime;
			_data = data;
			_owner.FilesUpdated();
		}
		#endregion
	}
}
