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
// Created: Wednesday, August 22, 2012 9:06:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// Interface for collidable objects.
	/// </summary>
	public abstract class Gorgon2DCollider
	{
		#region Variables.
		private bool _enabled = true;           // Flag to indicate that the collider is enabled.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the collision object that is attached to this collider.
		/// </summary>
		public I2DCollisionObject CollisionObject
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to set or return whether this collider is enabled or not.
		/// </summary>
		public bool Enabled
		{
			get
			{
				if (CollisionObject == null)
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
		public RectangleF ColliderBoundaries
		{
			get;
			protected set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the collider on the object to match the collision object transformation.
		/// </summary>
		/// <remarks>This function must be called to update the collider object boundaries from the collision object after transformation.</remarks>
		protected internal abstract void UpdateFromCollisionObject();
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DCollider"/> class.
		/// </summary>
		protected Gorgon2DCollider()
		{
			ColliderBoundaries = new RectangleF(0, 0, 1, 1);
		}
		#endregion
	}
}