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
// Created: Wednesday, January 02, 2008 10:26:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Abstract object that represents an object in space.
	/// </summary>
	public abstract class SpaceObject
	{
		#region Variables.
		private Vector2D _position = Vector2D.Zero;						// Position of the object in space.
		private Sprite _sprite = null;									// Sprite to use for the object.
		private Drawing.Color _tint = Drawing.Color.White;				// Tinting for the sprite.
		private BlendingModes _blendMode = BlendingModes.Modulated;		// Blending mode.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the sprite used to represent this object.
		/// </summary>
		protected Sprite Sprite
		{
			get
			{
				return _sprite;
			}
		}

		/// <summary>
		/// Property to set or return the tinting color for the sprite.
		/// </summary>
		public Drawing.Color Tint
		{
			get
			{
				return _tint;
			}
			set
			{
				_tint = value;
			}
		}

		/// <summary>
		/// Property to set or return the blending mode for the sprite.
		/// </summary>
		public BlendingModes BlendMode
		{
			get
			{
				return _blendMode;
			}
			set
			{
				_blendMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the position of the object.
		/// </summary>
		public Vector2D Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
			}
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="camera">Camera to use.</param>
		/// <param name="frameTime">Frame delta time.</param>
		public abstract void Update(Camera camera, float frameTime);

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public abstract void Draw();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="SpaceObject"/> class.
		/// </summary>
		/// <param name="sprite">Sprite to represent the object.</param>
		/// <param name="position">The initial position of the object.</param>
		protected SpaceObject(Sprite sprite, Vector2D position)
		{
			Position = position;
			_sprite = sprite;
		}
		#endregion
	}
}
