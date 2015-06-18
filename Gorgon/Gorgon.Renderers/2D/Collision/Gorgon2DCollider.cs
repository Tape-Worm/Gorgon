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

using System.Drawing;

namespace Gorgon.Renderers
{
	/// <summary>
	/// Base class for collider types.
	/// </summary>
	/// <remarks>Colliders allow for various shapes to be used as detectors for collisions.   Using a <see cref="Gorgon.Renderers.Gorgon2DAABB">Gorgon2DAABB</see> object for example will create a rectangular 
	/// bounding area around the object and will expand or contract based on the orientation or scale and shift according to the location of the object.
	/// <para>Colliders don't necessarily have to be bound to an object to be used, they will often contain properties that will allow the user to shift their position (in screen space, when connected to an object, the positioning is local to that object) 
	/// or alter the size of the collider.</para>
	/// <para>In order for an object to use a collider, it must implement the <see cref="Gorgon.Renderers.I2DCollisionObject">I2DCollisionObject</see> interface.  When the collider is assigned 
	/// to the object it will take on the location and dimensions of the object.  When the collider position or size is modified, it will be relative to the object.  The Gorgon2DAABB and <see cref="Gorgon.Renderers.Gorgon2DBoundingCircle">Gorgon2DBoundingCircle</see> 
	/// colliders are relative to the center of the object.  Any custom colliders should do the same.</para>
	/// </remarks>
	public abstract class Gorgon2DCollider
	{
		#region Variables.
		private bool _enabled = true;           // Flag to indicate that the collider is enabled.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the collision object that is attached to this collider.
		/// </summary>
		public I2DCollisionObject CollisionObject
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether this collider is enabled or not.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return CollisionObject != null && _enabled;
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
		/// <remarks>This function must be called to update the collider object boundaries from the collision object after transformation.
		/// <para>This must be implemented in any child collider object.</para>
		/// </remarks>
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