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
