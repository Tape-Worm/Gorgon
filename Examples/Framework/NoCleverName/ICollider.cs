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
// Created: TOBEREPLACED
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
