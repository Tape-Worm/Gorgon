#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Friday, November 17, 2006 1:50:40 AM
// 
#endregion


using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.FileSystems;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.FileSystems
{
    /// <summary>
    /// Entry point for the plug-in.
    /// </summary>
    public class GorgonZLibFileSystemPlugInEntry
        : FileSystemPlugIn
    {
        #region Properties.
		/// <summary>
		/// Property to return the file system information for the file system within the plug-in.
		/// </summary>
		/// <value></value>
		public override FileSystemEditorInfoAttribute FileSystemInfo
		{
			get 
			{
				object[] attributes = null;		// Attributes for the type.

				attributes = typeof(GorgonZLibFileSystem).GetCustomAttributes(typeof(FileSystemEditorInfoAttribute), true);

				if ((attributes == null) || (attributes.Length == 0))
					throw new CannotMountFileSystemException("Cannot load the file system plug-in.  The FileSystemEditorInfoAttribute is not present.", null);

				return (FileSystemEditorInfoAttribute)attributes[0];
			}
		}
        #endregion

        #region Methods.
		/// <summary>
		/// Function to create a new object from the plug-in.
		/// </summary>
		/// <param name="parameters">Parameters to pass.</param>
		/// <returns>The new object.</returns>
		protected override object CreateImplementation(object[] parameters)
		{
			try
			{
				if ((parameters[0] == null) || (parameters[0].ToString() == string.Empty))
					throw new ArgumentNullException();

				// Get the file system and mount it.
				return new GorgonZLibFileSystem(parameters[0].ToString(), this);
			}
			catch (Exception ex)
			{
				throw new CannotMountFileSystemException("Cannot mount the filesystem '" + parameters[0].ToString() + "'.", ex);
			}			
		}
        #endregion		

		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in.</param>
		public GorgonZLibFileSystemPlugInEntry(string plugInPath)
			: base("ZLib FileSystem", plugInPath)
		{
		}
		#endregion
	}
}
