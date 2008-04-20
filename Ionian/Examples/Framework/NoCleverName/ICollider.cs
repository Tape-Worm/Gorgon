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
// Created: Friday, December 21, 2007 4:14:54 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Interface for objects that can collide.
	/// </summary>
	public interface ICollider
	{
		#region Properties.
		/// <summary>
		/// Property to return the collision rectangle for an object.
		/// </summary>
		Rectangle CollisionRectangle
		{
			get;
		}

		/// <summary>
		/// Property to return the position of the collider object.
		/// </summary>
		Vector2D Position
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine what objects collide with this object.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="objects">List of objects to check.</param>
		/// <param name="colliders">List of objects that have collided.</param>
		void CollidesWith<T>(List<T> objects, List<T> colliders) where T : ICollider;

		/// <summary>
		/// Function to determine what objects collide with this object.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="colliderObject">Object to check for collision.</param>
		/// <returns>TRUE if a collision has occured, FALSE if not.</returns>
		bool CollidesWith<T>(T colliderObject) where T : ICollider;
		#endregion
	}
}
