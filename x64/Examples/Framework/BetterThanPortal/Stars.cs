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
// Created: Wednesday, January 02, 2008 4:28:42 PM
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
	/// Object representing the star layers.
	/// </summary>
	public class Stars
	{
		#region Value Types.
		/// <summary>
		/// Value type representing a star or various dust debris.
		/// </summary>
		private struct Star
		{
			#region Variables.
			/// <summary>
			/// Position of the star.
			/// </summary>
			public Vector2D Position;
			/// <summary>
			/// Distance from the foreground.
			/// </summary>
			public float Distance;
			/// <summary>
			/// Sprite used for the star.
			/// </summary>
			public Sprite StarSprite;
			#endregion

			#region Methods.
			/// <summary>
			/// Function to draw the star.
			/// </summary>
			public void Draw()
			{
				StarSprite.Position = Position;
				StarSprite.Draw();
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="Star"/> struct.
			/// </summary>
			/// <param name="sprite">The sprite.</param>
			/// <param name="rnd">Random number generator.</param>
			public Star(Sprite sprite, Random rnd)
			{
				Position.X = (float)rnd.Next(0, Gorgon.Screen.Width);
				Position.Y = (float)rnd.Next(0, Gorgon.Screen.Height);

				Distance = (float)(rnd.NextDouble() + 0.125);

				StarSprite = sprite;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private Star[] _stars = null;						// List of stars.
		private Sprite[] _starSprites = new Sprite[4];		// Star sprites.
		private Random _rnd = new Random();					// Random numbers.
		private Drawing.Color _tint = Drawing.Color.White;	// Tinting.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the tinting for the sprite.
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

				foreach (Star star in _stars)
					star.StarSprite.Color = _tint;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the stars per frame.
		/// </summary>
		/// <param name="camera">Camera to use.</param>
		/// <param name="frameTime">Frame delta time.</param>
		public void Update(Camera camera, float frameTime)
		{
			for (int i = 0; i < _stars.Length; i++)
			{
				_stars[i].Position -= (camera.Heading * frameTime) * _stars[i].Distance;

				if (_stars[i].Position.X < 0)
				{
					_stars[i] = new Star(_starSprites[_rnd.Next(0, 4)], _rnd);
					_stars[i].Position.X = Gorgon.Screen.Width;
				}
				if (_stars[i].Position.X > Gorgon.Screen.Width)
				{
					_stars[i] = new Star(_starSprites[_rnd.Next(0, 4)], _rnd);
					_stars[i].Position.X = 0;
				}

				if (_stars[i].Position.Y < 0)
				{
					_stars[i] = new Star(_starSprites[_rnd.Next(0, 4)], _rnd);
					_stars[i].Position.Y = Gorgon.Screen.Height;
				}
				if (_stars[i].Position.Y > Gorgon.Screen.Height)
				{
					_stars[i] = new Star(_starSprites[_rnd.Next(0, 4)], _rnd);
					_stars[i].Position.Y = 0;
				}
			}
		}

		/// <summary>
		/// Function to draw the stars.
		/// </summary>
		public void Draw()
		{
			for (int i = 0; i < _stars.Length; i++)
				_stars[i].Draw();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Stars"/> class.
		/// </summary>
		/// <param name="fileSystem">The file system containing the star sprites.</param>
		public Stars(FileSystems.FileSystem fileSystem, int maxStars)
		{
			_rnd = new Random(Environment.TickCount);

			// Load the stars from the file system.
			for (int i = 0; i < _starSprites.Length; i++)
				_starSprites[i] = Sprite.FromFileSystem(fileSystem, "/Sprites/Star" + string.Format("{0}", i + 1) + ".gorSprite");

			_stars = new Star[maxStars];

			// Initialize.
			for (int i = 0; i < maxStars; i++)
				_stars[i] = new Star(_starSprites[_rnd.Next(0, 4)], _rnd);
		}
		#endregion
	}
}
