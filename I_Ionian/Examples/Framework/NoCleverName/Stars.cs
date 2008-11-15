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
	/// Class for drawing the background stars.
	/// </summary>
	public class Stars
	{
		#region Value Types.
		/// <summary>
		/// Value type containing star information.
		/// </summary>
		private struct StarData
		{
			/// <summary>
			/// Position of the star.
			/// </summary>
			public Vector2D Position;
			/// <summary>
			/// Alpha level of the star.
			/// </summary>
			public byte Alpha;
			/// <summary>
			/// Y vector velocity.
			/// </summary>
			public float YVelocity;
			/// <summary>
			/// Scaling.
			/// </summary>
			public float Scale;
		}
		#endregion

		#region Constants.
		/// <summary>
		/// Number of stars.
		/// </summary>
		private const int StarCount = 250;
		#endregion

		#region Variables.
		private Sprite _starSprite = null;			// Star sprite.
		private StarData[] _stars = null;			// Stars.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to initialize the stars.
		/// </summary>
		public void Initialize()
		{
			_stars = new StarData[StarCount];

			// Generate.
			for (int i = 0; i < StarCount; i++)
			{
				_stars[i].Position = new Vector2D(MainForm.Random.Next(0, Gorgon.Screen.Width), MainForm.Random.Next(0, Gorgon.Screen.Height));
				_stars[i].Alpha = (byte)MainForm.Random.Next(160, 255);
				_stars[i].YVelocity = (float)(MainForm.Random.NextDouble() * 100.0);
				_stars[i].Scale = (float)(MainForm.Random.NextDouble() * 2.0);
			}
		}

		/// <summary>
		/// Function to update the stars.
		/// </summary>
		/// <param name="frameTime">Frame delta.</param>
		/// <param name="velocityMod">Velocity modifier.</param>
		public void Update(float frameTime, float velocityMod)
		{
			for (int i = 0; i < StarCount; i++)
			{
                _stars[i].Position = Vector2D.Add(_stars[i].Position, new Vector2D(0, (_stars[i].YVelocity + (velocityMod * 2)) * frameTime));

				if (_stars[i].Position.Y > Gorgon.Screen.Height)
				{
					_stars[i].Position = new Vector2D(MainForm.Random.Next(0, Gorgon.Screen.Width), 0);
					_stars[i].Alpha = (byte)MainForm.Random.Next(160, 255);
					_stars[i].YVelocity = (float)(MainForm.Random.NextDouble() * 100.0);
				}
			}
		}

		/// <summary>
		/// Function to draw the stars.
		/// </summary>
		public void Draw()
		{
			// Draw the stars.
			for (int i = 0; i < StarCount; i++)
			{
				_starSprite.UniformScale = _stars[i].Scale;
				_starSprite.Opacity = _stars[i].Alpha;
				_starSprite.Position = _stars[i].Position;
				_starSprite.Draw();
			}			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Stars"/> class.
		/// </summary>
		public Stars()
		{
			_starSprite = new Sprite("StarSprite", new Vector2D(1, 1));
			_starSprite.Color = Drawing.Color.Black;

			Initialize();
		}
		#endregion
	}
}
