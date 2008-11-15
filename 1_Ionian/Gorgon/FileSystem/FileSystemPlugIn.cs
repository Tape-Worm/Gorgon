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
// Created: Thursday, April 05, 2007 8:15:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// Abstract object for file system plug-ins.
	/// </summary>
	public abstract class FileSystemPlugIn
		: PlugInEntryPoint
	{
		#region Properties.
		/// <summary>
		/// Property to return the file system information for the file system within the plug-in.
		/// </summary>
		public abstract FileSystemInfoAttribute FileSystemInfo
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a file system.
		/// </summary>
		/// <param name="name">Name of the file system.</param>
		/// <param name="provider">File system provider.</param>
		/// <returns>A new file system object.</returns>
		internal FileSystem Create(string name, FileSystemProvider provider)
		{
			return (FileSystem)CreateImplementation(new object[] { name, provider });
		}
		#endregion		

		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the file system plug-in.</param>
		/// <param name="plugInPath">Path to the plug-in.</param>
		protected FileSystemPlugIn(string name, string plugInPath)
			: base(name, plugInPath, PlugInType.FileSystem)
		{
		}		
		#endregion
	}
}
