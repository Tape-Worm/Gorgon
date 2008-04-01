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
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="description">Description of the filesystem type.</param>
		/// <param name="iscompressed">TRUE if the file system compresses its data, FALSE if not.</param>
		/// <param name="ispackfile">TRUE if the file system is a pack file, FALSE if folder based.</param>
        /// <param name="fileSystemID">ID of the file system.</param>
		public FileSystemInfoAttribute(string description, bool iscompressed, bool ispackfile, string fileSystemID)
		{
            if (string.IsNullOrEmpty(description))
                throw new ArgumentNullException("description");

            if (string.IsNullOrEmpty(fileSystemID))
                throw new ArgumentNullException("fileSystemID");

			_description = description;
			_isCompressed = iscompressed;
			_isPackFile = ispackfile;
            _ID = fileSystemID;
		}
		#endregion
	}
}
