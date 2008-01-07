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
// Created: Wednesday, January 02, 2008 10:43:03 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// An object in space that can move.
	/// </summary>
	public abstract class Moveable
		: SpaceObject
	{
		#region Variables.
		private Vector2D _heading = Vector2D.Zero;			// Heading of the object.
		private float _velocity = 0.0f;						// Velocity of the object. 
		private float _angle = 0.0f;						// Angle of the object.
		private float _maxVelocity = 0.0f;					// Maximum velocity.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the initial angle for the object.
		/// </summary>
		public float Angle
		{
			get
			{
				return _angle;
			}
			set
			{
				_angle = value;
			}
		}

		/// <summary>
		/// Property to set or return the maximum velocity.
		/// </summary>
		public float MaxVelocity
		{
			get
			{
				return _maxVelocity;
			}
			set
			{
				_maxVelocity = value;
			}
		}

		/// <summary>
		/// Property to set or return the velocity of the object.
		/// </summary>
		public float Velocity
		{
			get
			{
				return _velocity;
			}
			set
			{
				_velocity = value;

				if ((_maxVelocity > 0) && (_velocity > _maxVelocity))
					_velocity = _maxVelocity;
			}
		}

		/// <summary>
		/// Property to return the heading of the object.
		/// </summary>
		public Vector2D Heading
		{
			get
			{
				return _heading;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="camera">Camera to use.</param>
		/// <param name="frameTime">Frame delta time.</param>
		public override void Update(Camera camera, float frameTime)
		{
			float rads = MathUtility.Radians(_angle);		// Convert to radians.
			
			// Calculate heading.
			_heading.X = (MathUtility.Sin(rads) * (_velocity));
			_heading.Y = (MathUtility.Cos(rads) * (_velocity));

			// Calculate the current position.
			Position += _heading * frameTime;

			// Convert to screen space.
			Sprite.Rotation = 180.0f - _angle;
			Sprite.Position = camera.ToScreen(Position);

			// Set sprite color and blend mode.
			Sprite.Color = Tint;
			Sprite.BlendingMode = BlendMode;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Moveable"/> class.
		/// </summary>
		/// <param name="sprite">Sprite to represent the object.</param>
		/// <param name="position">The initial position of the object.</param>
		protected Moveable(Sprite sprite, Vector2D position)
			: base(sprite, position)
		{
		}
		#endregion
	}
}
