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
// Created: Wednesday, November 01, 2006 5:33:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.FileSystems
{
	/// <summary>
	/// File system authentication exception.
	/// </summary>
	public class InvalidAuthenticationDataException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that previously existed.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidAuthenticationDataException(string fileSystemName, Exception ex)
			: base("The authentication for the file system '" + fileSystemName + "' is not valid.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fileSystemName">Name of the file system that previously existed.</param>
        public InvalidAuthenticationDataException(string fileSystemName)
			: this(fileSystemName, null)
		{
		}
		#endregion
	}
}
