#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, August 22, 2012 9:10:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Math;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A 2 dimensional axis aligned bounding box collider.
	/// </summary>
	public class Gorgon2DAABB
		: Gorgon2DCollider
	{
		#region Variables.
		private Vector2 _center = Vector2.Zero;             // Offset for the AABB.
		private Vector2 _size = new Vector2(1);             // Size of the AABB.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the center of the AAAB.
		/// </summary>
		/// <remarks>This is relative to the object being bound.</remarks>
		public Vector2 Center
		{
			get
			{
				return _center;
			}
			set
			{
				if (_center == value)
					return;

				_center = value;

				if (CollisionObject == null)
					ColliderBoundaries = new RectangleF(_center.X, _center.Y, _size.X, _size.Y);
				else
					UpdateFromCollisionObject();
			}
		}

		/// <summary>
		/// Property to set or return the size for the AABB.
		/// </summary>
		/// <remarks>This is relative to the object being bound.</remarks>
		public Vector2 Size
		{
			get
			{
				return _size;
			}
			set
			{
				if (_size == value)
					return;

				_size = value;
				if (CollisionObject == null)
					ColliderBoundaries = new RectangleF(_center.X, _center.Y, _size.X, _size.Y);
				else
					UpdateFromCollisionObject();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the collider on the object to match the collision object transformation.
		/// </summary>
		/// <remarks>This function must be called to update the collider object boundaries from the collision object after transformation.</remarks>
		protected internal override void UpdateFromCollisionObject()
		{
			Vector2 center = Vector2.Zero;
			Vector2 size = Vector2.Zero;

			if ((CollisionObject == null) || (!Enabled))
				return;

			if ((CollisionObject.Vertices == null) || (CollisionObject.Vertices.Length == 0))
			{
				ColliderBoundaries = new RectangleF(_center, _size);
				return;
			}

			// Define an infinite boundary.
			Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 max = new Vector2(float.MinValue, float.MinValue);

			// Determine the minimum and maximum extents.
			for (int i = 0; i < CollisionObject.Vertices.Length; i++)
			{
				Vector4 position = CollisionObject.Vertices[i].Position;

				min.X = min.X.Min(position.X);
				min.Y = min.Y.Min(position.Y);
				max.X = max.X.Max(position.X);
				max.Y = max.Y.Max(position.Y);
			}


			size = new Vector2(max.X - min.X, max.Y - min.Y);
			center = new Vector2(size.X / 2.0f + _center.X + min.X, size.Y / 2.0f + _center.Y + min.Y);

			Vector2.Modulate(ref size, ref _size, out size);

			ColliderBoundaries = new RectangleF(center.X - size.X / 2.0f, center.Y - size.Y / 2.0f, size.X, size.Y);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DAABB"/> class.
		/// </summary>
		public Gorgon2DAABB()
		{

		}
		#endregion
	}
}