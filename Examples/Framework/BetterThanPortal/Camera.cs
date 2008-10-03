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

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Object representing the 'camera'.
	/// </summary>
	public class Camera
	{
		#region Variables.
		private Vector2D _position = Vector2D.Zero;				// Position of the camera, i.e. the World position.
		private Moveable _target = null;						// Target object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the position of the camera.
		/// </summary>
		public Vector2D Position
		{
			get
			{
				if (_target != null)
					return Vector2D.Negate(_target.Position);

				return _position;
			}
			set
			{
				if (_target != null)
				{
					_target.Position = Vector2D.Negate(value);
					return;
				}
				_position = value;
			}
		}

		/// <summary>
		/// Property to return the heading of a target object.
		/// </summary>
		public Vector2D Heading
		{
			get
			{
				if (_target == null)
					return Vector2D.Zero;

				return _target.Heading;
			}
		}

		/// <summary>
		/// Property to set or return the target for the camera.
		/// </summary>
		public Moveable Target
		{
			get
			{
				return _target;
			}
			set
			{
				_target = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform the local position into a screen position.
		/// </summary>
		/// <param name="local">Local position to transform.</param>
		/// <returns>Local position transformed into world space.</returns>
		public Vector2D ToScreen(Vector2D local)
		{
			Vector2D screenSpace = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);			// Screen area.

			return Vector2D.Add(Vector2D.Add(local, Position), screenSpace);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Camera"/> class.
		/// </summary>
		/// <param name="position">The position of the camera.</param>
		public Camera(Vector2D position)
		{
			_position = position;
		}
		#endregion
	}
}
