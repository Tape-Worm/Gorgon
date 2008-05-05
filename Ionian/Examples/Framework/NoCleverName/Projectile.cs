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
// Created: Tuesday, December 18, 2007 11:09:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Projectile object.
	/// </summary>
	public class Projectile
		: ICollider
	{
		#region Variables.
		private Vector2D _position = Vector2D.Zero;			// Position.
		private float _velocity = 0.0f;						// Vertical velocity.
		private Sprite _sprite = null;						// Sprite.
		private bool _active = false;						// Flag to indicate whether the projectile is active or not.
		private ICollider _owner = null;					// Object that owns this projectile.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether a projectile is offscreen.
		/// </summary>
		private bool IsOffscreen
		{
			get
			{
				if (_velocity < 0)
					return (_position.Y < 0);
				else
					return (_position.Y > Gorgon.Screen.Height);
			}
		}

		/// <summary>
		/// Property to set or return whether a projectile is active.
		/// </summary>
		public bool Active
		{
			get
			{
				return _active;
			}
			set
			{
				_active = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the missile data.
		/// </summary>
		public void Reset()
		{
			_active = false;
			_position = _owner.Position;
		}

		/// <summary>
		/// Function to update the projectile position.
		/// </summary>
		/// <param name="frameTime">Frame delta.</param>
		/// <param name="difficultyMod">Difficulty modifier.</param>
		public void Update(float frameTime, float difficultyMod)
		{
			if (!_active)
				return;

			if (_sprite.Animations.Count > 0)
				_sprite.Animations[0].Advance(frameTime * 1000.0f);
			_position.Y += (_velocity + difficultyMod / 1.25f) * frameTime;

			// Turn this projectile off if it's offscreen.
			if (IsOffscreen)
				Reset();
		}

		/// <summary>
		/// Function to draw the projectile.
		/// </summary>
		public void Draw()
		{
			_sprite.Position = _position;

			if (!_active)
				return;
			
			_sprite.Draw();

#if (DEBUG && DEBUGDATA)
			Gorgon.Screen.BeginDrawing();
			Gorgon.Screen.Rectangle(CollisionRectangle.Left, CollisionRectangle.Top, CollisionRectangle.Width, CollisionRectangle.Height, Drawing.Color.Purple);
			Gorgon.Screen.EndDrawing();
#endif
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Projectile"/> class.
		/// </summary>
		/// <param name="sprite">The sprite used for the projectile.</param>
		/// <param name="velocity">Vertical velocity of the projectile.</param>
		/// <param name="owner">Object that owns this projectile.</param>
		public Projectile(Sprite sprite, float velocity, ICollider owner)
		{
			_sprite = sprite;
			_velocity = velocity;
			_owner = owner;
			Reset();
		}
		#endregion

		#region ICollider Members
		#region Properties.
		/// <summary>
		/// Property to return the collision rectangle for an object.
		/// </summary>
		/// <value></value>
		public Drawing.Rectangle CollisionRectangle
		{
			get 
			{
				if (_velocity < 0)
					return new Drawing.Rectangle((int)_position.X - 5, (int)(_position.Y - ((_sprite.Axis.Y * _sprite.Scale.Y) / 2.0f)) - 5, 10, 10);
				else
					return new Drawing.Rectangle((int)_position.X - 5, (int)(_position.Y + (_sprite.ScaledHeight * 0.75f)) - 5, 10, 10);
			}
		}

		/// <summary>
		/// Property to return the position of the collider object.
		/// </summary>
		/// <value></value>
		public Vector2D Position
		{
			get
			{
				return _position;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine what objects collide with this object.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="objects">List of objects to check.</param>
		/// <param name="colliders">List of objects that have collided.</param>
		public void CollidesWith<T>(List<T> objects, List<T> colliders)
			where T : ICollider
		{
			if (!_active)
				return;

			foreach (T collider in objects)
			{
				//				if (collider.CollisionRectangle.IntersectsWith(CollisionRectangle))
				if (collider.CollidesWith<Projectile>(this))
					colliders.Add(collider);
			}
		}

		/// <summary>
		/// Function to determine what objects collide with this object.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="colliderObject">Object to check for collision.</param>
		/// <returns>
		/// TRUE if a collision has occured, FALSE if not.
		/// </returns>
		public bool CollidesWith<T>(T colliderObject) where T : ICollider
		{
			if (!_active)
				return false;

			return colliderObject.CollisionRectangle.IntersectsWith(CollisionRectangle);
		}
		#endregion
		#endregion
	}
}
