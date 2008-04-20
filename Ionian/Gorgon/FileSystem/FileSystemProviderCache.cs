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
// Created: Friday, April 06, 2007 12:38:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Object representing a file system provider cache.
	/// </summary>
	public static class FileSystemProviderCache
	{
		#region Variables.
		private static FileSystemProviderList _providers = new FileSystemProviderList();		// File system provider list.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of loaded providers.
		/// </summary>
		public static FileSystemProviderList Providers
		{
			get
			{
				return _providers;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to unload all the providers and file systems.
		/// </summary>
		public static void UnloadAll()
		{
			FileSystemCache.DestroyAll();
			_providers.Clear();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static FileSystemProviderCache()
		{
			_providers = new FileSystemProviderList();
		}
		#endregion
	}
}
