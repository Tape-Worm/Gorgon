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
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Asteroid object.
	/// </summary>
	public class Asteroid
		: ICollider
	{
		#region Variables.
		private Vector2D _position = Vector2D.Zero;			// Position.
		private Sprite _asteroid = null;					// Sprite representing the asteroid.
		private float _yVelocity;							// Y velocity.
		private float _rotate;								// Asteroid rotation.
		private int _score = 0;								// Score for the asteroid.
		private Sprite[] _asteroidSprites;					// List of asteroid sprites.
		private BoundingCircle _bounds;						// Bounding circle.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the score value for the asteroid.
		/// </summary>
		public int Score
		{
			get
			{
				return _score;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the asteroid.
		/// </summary>
		public void Reset()
		{
			// Select asteroid.
			switch (MainForm.Random.Next(3))
			{
				case 0:
					_asteroid = _asteroidSprites[0];
					_score = 50;
					break;
				case 1:
					_asteroid = _asteroidSprites[1];
					_score = 100;
					break;
				default:
					_asteroid = _asteroidSprites[2];
					_score = 25;
					break;
			}

			_asteroid.Scale = MainForm.SpriteScales;
			_position = new Vector2D(MainForm.Random.Next((int)_asteroid.Axis.X, (int)(Gorgon.Screen.Width - _asteroid.Axis.X)), -_asteroid.Axis.Y);
			_yVelocity = (float)(MainForm.Random.NextDouble() * 110.0) + _asteroid.Width / 4.0f;
		}

		/// <summary>
		/// Function to update the asteroid.
		/// </summary>
		/// <param name="frameTime">Frame delta.</param>
		/// <param name="difficultyModifier">Modifier for difficulty.</param>
		public void Update(float frameTime, float difficultyModifier)
		{
			_rotate += (((float)(_asteroid.Height / 4.0f)) + MainForm.Random.Next(5)) * frameTime;
			if (_rotate > 359.9f)
				_rotate = 0.0f;
			_position.Y += ((_yVelocity + difficultyModifier) * frameTime);

			if (_position.Y == 0)
				_asteroid.BlendingMode = BlendingModes.None;

			// If we're offscreen, then reset.
			if (_position.Y > Gorgon.Screen.Height + _asteroid.Axis.Y)
				Reset();
		}

		/// <summary>
		/// Function to draw the asteroid.
		/// </summary>
		public void Draw()
		{
			_asteroid.Rotation = _rotate;
			_asteroid.Position = _position;
			_asteroid.Draw();
			_bounds = _asteroid.BoundingCircle;
#if (DEBUG && DEBUGDATA)
			Gorgon.Screen.BeginDrawing();
			Gorgon.Screen.Circle(_asteroid.BoundingCircle.Center.X, _asteroid.BoundingCircle.Center.Y, _asteroid.BoundingCircle.Radius, Drawing.Color.Red);
			Gorgon.Screen.EndDrawing();
#endif
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Asteroid"/> class.
		/// </summary>
		/// <param name="asteroid1">Asteroid sprite.</param>
		/// <param name="asteroid2">Asteroid sprite.</param>
		/// <param name="asteroid3">Asteroid sprite.</param>
		public Asteroid(Sprite asteroid1, Sprite asteroid2, Sprite asteroid3)
		{			
			_asteroidSprites = new Sprite[3];
			_asteroidSprites[0] = asteroid1;
			_asteroidSprites[1] = asteroid2;
			_asteroidSprites[2] = asteroid3;
			Reset();
		}
		#endregion

		#region ICollider Members
		#region Properties.
		/// <summary>
		/// Property to return the collision rectangle for an object.
		/// </summary>
		public System.Drawing.Rectangle CollisionRectangle
		{
			get 
			{
				return System.Drawing.Rectangle.Empty;
			}
		}

		/// <summary>
		/// Property to return the position of the collider object.
		/// </summary>
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
			// Check each object.
			foreach (T collider in objects)
			{
				if (CollidesWith<T>(collider))
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
		public bool CollidesWith<T>(T colliderObject)
			where T : ICollider
		{
			return _bounds.Intersects(colliderObject.CollisionRectangle);
		}
		#endregion
		#endregion
	}
}
