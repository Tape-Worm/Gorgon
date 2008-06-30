#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Tuesday, May 27, 2008 2:09:11 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Interface representing a container for GUI objects.
	/// </summary>
	public interface IGUIContainer
	{
		/// <summary>
		/// Property to set or return whether this control will clip its children.
		/// </summary>
		bool ClipChildren
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the list of objects contained within this container.
		/// </summary>
		GUIObjectCollection GUIObjects
		{
			get;
		}
	}
}
