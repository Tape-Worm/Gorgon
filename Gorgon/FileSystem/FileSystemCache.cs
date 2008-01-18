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
// Created: Monday, September 10, 2007 11:33:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GorgonLibrary;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object representing a cache for file systems.
	/// </summary>
	public static class FileSystemCache
	{
		#region Variables.
		private static FileSystemList _fileSystems = null;		// List of file systems.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the file systems.
		/// </summary>
		public static FileSystemList FileSystems
		{
			get
			{
				return _fileSystems;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove all file systems.
		/// </summary>
		public static void DestroyAll()
		{
			_fileSystems.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static FileSystemCache()
		{
			_fileSystems = new FileSystemList();
		}
		#endregion
	}
}
