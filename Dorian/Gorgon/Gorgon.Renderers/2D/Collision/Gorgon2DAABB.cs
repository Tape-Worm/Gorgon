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
		: I2DCollider
	{
		#region Variables.
		private I2DCollisionObject _collision = null;				// Collision object attached to this collider.
		private RectangleF _bounds = RectangleF.Empty;				// Collider boundaries.
		private bool _enabled = true;								// Flag to indicate that the collider is enabled.
		#endregion

		#region Properties.

		#endregion

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DAABB"/> class.
		/// </summary>
		public Gorgon2DAABB()
		{
		}
		#endregion

		#region ICollider Members
		#region Properties.
		/// <summary>
		/// Property to set or return whether this collider is enabled or not.
		/// </summary>
		public bool Enabled
		{
			get
			{
				if (_collision == null)
					return false;

				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		/// <summary>
		/// Property to set or return the boundaries for the collider.
		/// </summary>
		public System.Drawing.RectangleF ColliderBoundaries
		{
			get
			{
				if (!Enabled)
					return RectangleF.Empty;

				return _bounds;
			}
			set
			{
				_bounds = value;
			}
		}

		/// <summary>
		/// Property to set or return the collision object that is attached to this collider.
		/// </summary>
		public I2DCollisionObject CollisionObject
		{
			get
			{
				return _collision;
			}
			set
			{
				if (value == null)
				{
					_collision.Collider = null;
					return;
				}

				_collision = value;
				_collision.Collider = this;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the collider on the object to match the collision object transformation.
		/// </summary>
		/// <remarks>This function must be called to update the collider object boundaries from the collision object.</remarks>
		public void UpdateFromCollisionObject()
		{
			if ((!CollisionObject.NeedsColliderUpdate) || (CollisionObject == null) || (!Enabled))
				return;

			if ((CollisionObject.Vertices == null) || (CollisionObject.Vertices.Length == 0))
			{
				_bounds = RectangleF.Empty;
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

			_bounds = RectangleF.FromLTRB(min.X, min.Y, max.X, max.Y);
			CollisionObject.NeedsColliderUpdate = false;
		}
		#endregion
		#endregion
	}
}
