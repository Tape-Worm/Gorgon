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
// Created: Tuesday, May 27, 2008 6:55:19 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Border style enumeration for panels.
	/// </summary>
	public enum PanelBorderStyle
	{
		/// <summary>
		/// No border.
		/// </summary>
		None = 0,
		/// <summary>
		/// Single thin line around the control.
		/// </summary>
		Single = 1
	}

	/// <summary>
	/// Enumeration containing mouse event types.
	/// </summary>
	public enum MouseEventType
	{
		/// <summary>
		/// Mouse was moved.
		/// </summary>
		MouseMoved = 1,
		/// <summary>
		/// Mouse button was released.
		/// </summary>
		MouseButtonUp = 2,
		/// <summary>
		/// Mouse button was pressed.
		/// </summary>
		MouseButtonDown = 3,
		/// <summary>
		/// Mouse wheel was moved.
		/// </summary>
		MouseWheelMove = 4
	}
}
