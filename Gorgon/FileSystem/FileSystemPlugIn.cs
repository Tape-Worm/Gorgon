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
		public abstract FileSystemEditorInfoAttribute FileSystemInfo
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a file system.
		/// </summary>
		/// <param name="name">Name of the file system.</param>
		/// <returns>A new file system object.</returns>
		internal FileSystem Create(string name)
		{
			return (FileSystem)CreateImplementation(new object[] { name });
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
