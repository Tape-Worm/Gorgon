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
	/// Object representing an alien that will try and kill you.
	/// </summary>
	public class Alien
		: ICollider
	{
		#region Constants.
		/// <summary>
		/// Maximum alien blaster shots.
		/// </summary>
		private const int MaxAlienBlasters = 8;
		#endregion

		#region Variables.
		private Sprite _alienSprite = null;				// Alien sprite.
		private Sprite _alienBlaster = null;			// Alien blaster sprite.
		private Sprite _alienShield = null;				// Alien shield sprite.
		private Drawing.Color _shieldColor;				// Color adjustment.
		private float _shieldColorValue;				// Color value.
		private bool _shieldAnim = false;				// Flag to indicate the shield animation direction.
		private Vector2D _position = Vector2D.Zero;		// Alien position.	
		private bool _active = false;					// Flag to set whether the alien is active or not.
		private bool _direction = false;				// Flag to indicate which direction the alien is headed.
		private List<Projectile> _blasters = null;		// List of projectiles.
		private PreciseTimer _blasterTimer = null;		// Time between blasts.
		private int _shieldLevel = 2;					// Shield level.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the shield level.
		/// </summary>
		public int ShieldLevel
		{
			get
			{
				return _shieldLevel;
			}
			set
			{
				_shieldLevel = value;
			}
		}

		/// <summary>
		/// Property to return the score for the alien.
		/// </summary>
		public int Score
		{
			get
			{
				// Alien is worth 1000 points, and the higher up the alien is on the screen, the more it's worth.
				return 1000 + (int)((Gorgon.Screen.Height - this.Position.Y) * MainForm.SpriteScales.Y);
			}
		}

		/// <summary>
		/// Property to set or return the status of the alien.
		/// </summary>
		public bool Active
		{
			get
			{
				return _active;
			}
			set
			{
				_active = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if a blaster from the alien collided with the player.
		/// </summary>
		/// <param name="player">Player to test.</param>
		/// <returns>TRUE if collision, FALSE if not.</returns>
		public bool BlasterCollidesWithPlayer(Player player)
		{
			foreach (Projectile blaster in _blasters)
			{
				if (blaster.CollidesWith<Player>(player))
				{
					blaster.Reset();
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Function to reset the alien data.
		/// </summary>
		public void Reset()
		{
			_active = false;
			_shieldLevel = 2;
			_shieldColor = Drawing.Color.Blue;
			_shieldColorValue = 0;
			_position = new Vector2D(MainForm.Random.Next((int)(_alienSprite.Axis.X) + 10, Gorgon.Screen.Width - (int)(_alienSprite.Axis.X) - 10), -_alienSprite.Height);
			if (MainForm.Random.Next(0, MainForm.Random.Next(100)) > 50)
				_direction = false;
			else
				_direction = true;

			_blasters.Clear();

			for (int i = 0; i < MaxAlienBlasters; i++)
				_blasters.Add(new Projectile(_alienBlaster, (Gorgon.Screen.Height * 0.91667f), this));

			_blasterTimer.Reset();
		}

		/// <summary>
		/// Function to update the alien.
		/// </summary>
		/// <param name="frameTime">Frame delta.</param>
		/// <param name="difficultyMod">Difficulty modifier.</param>
		public void Update(float frameTime, float difficultyMod)
		{
			if (!_active)
				return;

			_alienSprite.Animations[0].Advance(frameTime * 1000.0f);

			if (!_direction)
				_position.X -= (difficultyMod + MainForm.Random.Next((int)(Gorgon.Screen.Width * 0.25f), (int)(Gorgon.Screen.Width * 0.4f))) * frameTime;
			else
				_position.X += (difficultyMod + MainForm.Random.Next((int)(Gorgon.Screen.Width * 0.25f), (int)(Gorgon.Screen.Width * 0.4f))) * frameTime;

			_position.Y += (MainForm.Random.Next((int)(Gorgon.Screen.Height * 0.16667f), (int)(Gorgon.Screen.Height * 0.33333f)) + difficultyMod) * frameTime;

			if (_position.Y > Gorgon.Screen.Height + _alienSprite.Height)
			{
				Reset();
				return;
			}

			if ((_position.X <= _alienSprite.Axis.X) || (_position.X >= Gorgon.Screen.Width - _alienSprite.Axis.X))
				_direction = !_direction;

			// Fire the blasters.
			foreach (Projectile blaster in _blasters)
			{
				if ((_position.Y < Gorgon.Screen.Height - _alienSprite.Height) && (_position.Y > 0) && (_blasterTimer.Seconds >= 1.0) && (!blaster.Active))
				{
					blaster.Reset();
					blaster.Active = true;
					_blasterTimer.Reset();
				}					

				blaster.Update(frameTime, difficultyMod);
			}

			if (_shieldLevel == 2)
				_alienShield.Opacity = 255;
			if (_shieldLevel == 1)
				_alienShield.Opacity = 128;

			if (_shieldAnim)
				_shieldColorValue -= 255.0f * frameTime;
			else
				_shieldColorValue += 255.0f * frameTime;

			if ((_shieldColorValue >= 255.0f) || (_shieldColorValue <= 0.0f))
			{
				if (_shieldColorValue > 255.0f)
					_shieldColorValue = 255.0f;
				if (_shieldColorValue < 0.0f)
					_shieldColorValue = 0.0f;
				_shieldAnim = !_shieldAnim;
			}

			_shieldColor = Drawing.Color.FromArgb(_alienShield.Opacity, (byte)_shieldColorValue / 2, (byte)_shieldColorValue / 3, (byte)_shieldColorValue);
		}

		/// <summary>
		/// Function to draw the alien sprite.
		/// </summary>
		public void Draw()
		{
			if (!_active)
				return;

			foreach (Projectile blaster in _blasters)
				blaster.Draw();
					
			_alienSprite.Position = _position;
			_alienSprite.Draw();
		
			// Draw the shield.
			if (_shieldLevel > 0)
			{
				_alienShield.Color = _shieldColor;
				_alienShield.Position = _position;
				_alienShield.Draw();
			}

#if (DEBUG && DEBUGDATA)
			Gorgon.Screen.BeginDrawing();
			Gorgon.Screen.Rectangle(CollisionRectangle.Left, CollisionRectangle.Top, CollisionRectangle.Width, CollisionRectangle.Height, Drawing.Color.Red);
			Gorgon.Screen.EndDrawing();
#endif
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Alien"/> class.
		/// </summary>
		/// <param name="alienSprite">The alien sprite to use.</param>
		/// <param name="blasterSprite">The blaster sprite for the alien weapon.</param>
		/// <param name="shieldSprite">THe shield sprite for the alien.</param>
		public Alien(Sprite alienSprite, Sprite blasterSprite, Sprite shieldSprite)
		{
			_alienSprite = alienSprite;
			_alienSprite.Animations[0].AnimationState = AnimationState.Playing;
			_alienBlaster = blasterSprite;
			_alienShield = shieldSprite;
			_alienSprite.Scale = MainForm.SpriteScales;
			_alienBlaster.Scale = MainForm.SpriteScales;
			_alienShield.Scale = MainForm.SpriteScales;
			_blasterTimer = new PreciseTimer();
			_blasters = new List<Projectile>();
			Reset();
		}
		#endregion

		#region ICollider Members
		#region Properties.
		/// <summary>
		/// Property to return the collision rectangle for an object.
		/// </summary>
		/// <value></value>
		public System.Drawing.Rectangle CollisionRectangle
		{
			get 
			{
				if (_shieldLevel > 0)
					return Drawing.Rectangle.Round(_alienShield.AABB);
				else
					return Drawing.Rectangle.Round(_alienSprite.AABB);
			}
		}

		/// <summary>
		/// Property to return the position of the collider object.
		/// </summary>
		/// <value></value>
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
		public void CollidesWith<T>(List<T> objects, List<T> colliders) where T : ICollider
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to determine what objects collide with this object.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="colliderObject">Object to check for collision.</param>
		/// <returns>
		/// TRUE if a collision has occured, FALSE if not.
		/// </returns>
		public bool CollidesWith<T>(T colliderObject) where T : ICollider
		{
			throw new NotImplementedException();
		}
		#endregion
		#endregion
	}
}
