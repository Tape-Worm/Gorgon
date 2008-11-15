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
