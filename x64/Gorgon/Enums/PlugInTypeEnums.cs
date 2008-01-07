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
// Created: Tuesday, May 02, 2006 8:08:49 PM
// 
#endregion

namespace GorgonLibrary.PlugIns
{   
    /// <summary>
	/// Enumeration containing definitions for various plug-in types.
	/// </summary>
	public enum PlugInType
	{
		/// <summary>GUI plug-in.</summary>
		GUI = 0,
		/// <summary>Input plug-in.</summary>
		Input = 1,
        /// <summary>File system plug-in.</summary>
        FileSystem = 2,
		/// <summary>Custom user plug-in.</summary>
		UserDefined = 0x7FFFFFFE
	}
}