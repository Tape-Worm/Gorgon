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
// Created: Saturday, January 06, 2007 2:15:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface to represent a moveable object.
	/// </summary>
	public interface IRenderableTransform
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the position of the object.
		/// </summary>
		Vector2D Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		Vector2D Scale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the rotation angle in degrees.
		/// </summary>
		float Rotation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the object.
		/// </summary>
		Vector2D Size
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		Vector2D Axis
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset within the source image to start drawing from.
		/// </summary>
		Vector2D ImageOffset
		{
			get;
			set;
		}
		#endregion
	}
}
