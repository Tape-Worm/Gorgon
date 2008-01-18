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
// Created: Thursday, January 03, 2008 2:18:55 PM
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
	/// Object representing a sun.
	/// </summary>
	public class SunObject
		: SpaceObject
	{
		#region Variables.
		#endregion

		#region Properties.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to scale the planet.
		/// </summary>
		/// <param name="scaleValue">Scale value.</param>
		public void ScalePlanet(float scaleValue)
		{
			Sprite.UniformScale = scaleValue / Sprite.Width;
		}

		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="camera">Camera to use.</param>
		/// <param name="frameTime">Frame delta time.</param>
		public override void Update(Camera camera, float frameTime)
		{
			Sprite.Position = camera.ToScreen(Position);
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
		/// Initializes a new instance of the <see cref="PlanetaryObject"/> class.
		/// </summary>
		/// <param name="sprite">Sprite to represent the object.</param>
		/// <param name="position">The initial position of the object.</param>
		public SunObject(Sprite sprite, Vector2D position)
			: base(sprite, position)
		{
		}
		#endregion
	}
}
