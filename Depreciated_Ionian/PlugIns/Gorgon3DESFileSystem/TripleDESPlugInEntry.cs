#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Tuesday, April 01, 2008 5:41:28 PM
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
    public class Gorgon3DESFileSystemPlugInEntry
        : FileSystemPlugIn
    {
        #region Properties.
		/// <summary>
		/// Property to return the file system information for the file system within the plug-in.
		/// </summary>
		/// <value></value>
		public override FileSystemInfoAttribute FileSystemInfo
		{
			get 
			{
				object[] attributes = null;		// Attributes for the type.

				attributes = typeof(Gorgon3DESFileSystem).GetCustomAttributes(typeof(FileSystemInfoAttribute), true);

				if ((attributes == null) || (attributes.Length == 0))
					throw new GorgonException(GorgonErrors.InvalidPlugin, "The provider is missing the FileSystemInfoAttribute.");

				return (FileSystemInfoAttribute)attributes[0];
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
			if (parameters.Length != 2)
				throw new ArgumentOutOfRangeException("Expecting 2 parameters:  Name and provider.");

			if ((parameters[0] == null) || (parameters[0].ToString() == string.Empty))
				throw new ArgumentNullException("name");
			if (parameters[1] == null)
				throw new ArgumentNullException("provider");

			// Get the file system and mount it.
            return new Gorgon3DESFileSystem(parameters[0].ToString(), parameters[1] as FileSystemProvider);			
		}
        #endregion		

		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in.</param>
		public Gorgon3DESFileSystemPlugInEntry(string plugInPath)
			: base("Gorgon.3DESFileSystem", plugInPath)
		{
		}
		#endregion
	}
}
