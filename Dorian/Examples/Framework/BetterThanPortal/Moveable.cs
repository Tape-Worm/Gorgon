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
			Position = Vector2D.Add(Position, Vector2D.Multiply(_heading, frameTime));

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
