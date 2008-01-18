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
// Created: Thursday, January 03, 2008 1:14:06 PM
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
	/// Object representing a small ship.
	/// </summary>
	public class SmallShip
		: Moveable
	{
		#region Variables.

		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="camera">Camera to use.</param>
		/// <param name="frameTime">Frame delta time.</param>
		public override void Update(Camera camera, float frameTime)
		{
			base.Update(camera, frameTime);

			// Constrain.
			if (Velocity > 220.0f)
				Velocity = 220.0f;
			if (Velocity < -50.0f)
				Velocity = -50.0f;

			if (Angle < 0.0f)
				Angle = 359.9f;
			if (Angle > 359.9f)
				Angle = 0.0f;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public override void Draw()
		{
			Sprite.Draw();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Cruiser"/> class.
		/// </summary>
		/// <param name="sprite">Sprite to represent the object.</param>
		/// <param name="position">The initial position of the object.</param>
		public SmallShip(Sprite sprite, Vector2D position)
			: base(sprite, position)
		{
		}
		#endregion
	}
}
