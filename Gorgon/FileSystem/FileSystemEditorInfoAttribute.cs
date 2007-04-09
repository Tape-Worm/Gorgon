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
	public class FileSystemEditorInfoAttribute
		: Attribute
	{
		#region Variables.
		private string _typeName = string.Empty;			// File system type name.
		private bool _isCompressed = false;					// Flag to indicate whether a file system is compressed or not.
		private bool _isPackFile = false;					// Flag to indicate whether a file system is a pack file or a folder system.		
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the name of the file system type.
		/// </summary>
		public string TypeName
		{
			get
			{
				return _typeName;
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

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="typename">Name of the filesystem type.</param>
		/// <param name="compressed">TRUE if the file system compresses its data, FALSE if not.</param>
		/// <param name="packfile">TRUE if the file system is a pack file, FALSE if folder based.</param>
		public FileSystemEditorInfoAttribute(string typename, bool compressed, bool packfile)
		{
			_typeName = typename;
			_isCompressed = compressed;
			_isPackFile = packfile;
		}
		#endregion
	}
}
