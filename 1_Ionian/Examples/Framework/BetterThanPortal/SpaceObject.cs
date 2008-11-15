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
