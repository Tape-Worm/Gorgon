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
// Created: Wednesday, January 02, 2008 10:15:48 PM
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
