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
// Created: Friday, April 06, 2007 12:24:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Attribute for defining file system information.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public class FileSystemInfoAttribute
		: Attribute
	{
		#region Variables.
		private string _description = string.Empty;			// File system description.
		private bool _isCompressed = false;					// Flag to indicate whether a file system is compressed or not.
		private bool _isPackFile = false;					// Flag to indicate whether a file system is a pack file or a folder system.
        private bool _isEncrypted = false;                  // Flag to indicate whether a file system is encrypted or not.
        private string _ID = string.Empty;                  // ID for the file system type.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the ID of the file system type.
        /// </summary>
        /// <remarks>The ID of the file system is used to determine which file system provider should be used.  This ID is hardcoded to each provider plug-in, and is written to the file system header.</remarks>
        public string ID
        {
            get
            {
                return _ID;
            }
        }

		/// <summary>
		/// Property to return the name of the file system description.
		/// </summary>
		public string Description
		{
			get
			{
				return _description;
			}
		}

		/// <summary>
		/// Property to return whether the file system is compressed or not.
		/// </summary>
		public bool IsCompressed
		{
			get
			{
				return _isCompressed;
			}
		}

		/// <summary>
		/// Property to return whether the file system is a pack file or a folder based file system.
		/// </summary>
		public bool IsPackFile
		{
			get
			{
				return _isPackFile;
			}
		}

        /// <summary>
        /// Property to return whether the file system is encrypted or not.
        /// </summary>
        public bool IsEncrypted
        {
            get
            {
                return _isEncrypted;
            }
        }

		/// <summary>
		/// Property to return the file extensions associated with this provider.
		/// </summary>
		public string FileExtensions
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the filesystem type.</param>
		/// <param name="iscompressed">TRUE if the file system compresses its data, FALSE if not.</param>
		/// <param name="ispackfile">TRUE if the file system is a pack file, FALSE if folder based.</param>
        /// <param name="isencrypted">TRUE if the file system is encrypted, FALSE if not.</param>
        /// <param name="fileSystemID">ID of the file system.</param>
		/// <param name="fileExtensions">File extensions associated with this provider.</param>
		public FileSystemInfoAttribute(string description, bool iscompressed, bool ispackfile, bool isencrypted, string fileSystemID, string fileExtensions)
		{
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException("description");

            if (string.IsNullOrEmpty(fileSystemID))
                throw new ArgumentNullException("fileSystemID");

			if (string.IsNullOrEmpty(fileExtensions))
				FileExtensions = "All Files (*.*)|*.*";
			else
				FileExtensions = fileExtensions;

			_description = description;
			_isCompressed = iscompressed;
			_isPackFile = ispackfile;
            _isEncrypted = isencrypted;
            _ID = fileSystemID;
		}
		#endregion
	}
}
