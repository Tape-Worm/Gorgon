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
	/// Hit event arguments.
	/// </summary>
	public class HitEventArgs
		: EventArgs
	{
		#region Variables.
		private int _score;				// Score value of the object that was hit.
		private ICollider _object;		// Object that was hit.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the score value of the object.
		/// </summary>
		public int Score
		{
			get
			{
				return _score;
			}
		}

		/// <summary>
		/// Property to return the object that was hit.
		/// </summary>
		public ICollider HitObject
		{
			get
			{
				return _object;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="HitEventArgs"/> class.
		/// </summary>
		/// <param name="score">The score.</param>
		/// <param name="collideObject">The collide object.</param>
		public HitEventArgs(int score, ICollider collideObject)
		{
			_score = score;
			_object = collideObject;
		}
		#endregion
	}

	/// <summary>
	/// Event delegate for a missile hit.
	/// </summary>
	/// <param name="sender">Object that sent the event.</param>
	/// <param name="e">Event parameters.</param>
	public delegate void HitEventHandler(object sender, HitEventArgs e);

	/// <summary>
	/// Object representing the player.
	/// </summary>
	public class Player
		: ICollider
	{
		#region Events.
		/// <summary>
		/// Event fired when no lives are left for the player.
		/// </summary>
		public event EventHandler NoLivesLeft;
		/// <summary>
		/// Event fired when a missile hits an object.
		/// </summary>
		public event HitEventHandler MissileHit;
		#endregion

		#region Constants.
		/// <summary>
		/// Max missile count for the player.
		/// </summary>
		private const int MissileCount = 5;
		#endregion

		#region Variables.
		private Vector2D _position;							// Position of the player.
		private Sprite _playerSprite;						// Sprite used for the player.
		private Sprite _missileSprite;						// Sprite used for the player missile.
		private TextSprite _message;						// Message to display when dead.
		private int _lives = 3;								// Lives for the player.
		private bool _dead = false;							// Flag to indicate that the player is dead.
		private PreciseTimer _deathTimer;					// Death countdown.
		private PreciseTimer _godTimer;						// God mode countdown.
		private PreciseTimer _missileTimer;					// Missile delay timer.
		private bool _invicible = false;					// Flag to indicate invincibility.
		private bool _flashShip = false;					// Flag to flash the ship during invincibility.
		private List<Asteroid> _asteroidColliders;			// List of asteroids that collided with the player.
		private List<Projectile> _missiles;					// List of missiles.
		private int _playerScore = 0;						// Player score.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the position of the player.
		/// </summary>
		public Vector2D Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value.X > MovementRange.Right)
					value.X = MovementRange.Right;
				if (value.X < MovementRange.Left)
					value.X = MovementRange.Left;
				if (value.Y < MovementRange.Top)
					value.Y = MovementRange.Top;
				if (value.Y > MovementRange.Bottom)
					value.Y = MovementRange.Bottom;
				if (!_dead)
					_position = value;
			}
		}

		/// <summary>
		/// Property to return the number of lives left.
		/// </summary>
		public int LivesLeft
		{
			get
			{
				return _lives;
			}
		}

		/// <summary>
		/// Property to return whether the player is dead.
		/// </summary>
		public bool IsDead
		{
			get
			{
				return _dead;
			}
		}

		/// <summary>
		/// Property to return the movement range of the player.
		/// </summary>
		public Drawing.RectangleF MovementRange
		{
			get
			{
				return new Drawing.RectangleF(_playerSprite.Axis.X * _playerSprite.Scale.X, (Gorgon.Screen.Height / 2.0f) + _playerSprite.Axis.Y * _playerSprite.Scale.Y, Gorgon.Screen.Width - _playerSprite.ScaledWidth, (Gorgon.Screen.Height / 2.0f) - _playerSprite.Axis.Y *_playerSprite.Scale.Y);
			}
		}

		/// <summary>
		/// Property to return the score for this player.
		/// </summary>
		public int Score
		{
			get
			{
				return _playerScore;
			}
			set
			{
				if ((value < 0) || (value > 1000000))
					value = 0;

				_playerScore = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to reset the player.
		/// </summary>
		/// <param name="resetEverything">TRUE to reset everything, FALSE to just reset the dead flag.</param>
		private void Reset(bool resetEverything)
		{
			_dead = false;
			_position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height);
			_deathTimer.Reset();

			if (resetEverything)
			{
				_missiles.Clear();

				for (int i = 0; i < MissileCount; i++)
					_missiles.Add(new Projectile(_missileSprite, -(Gorgon.Screen.Height * 0.66667f) , this));

				_lives = 3;
				_playerScore = 0;
				if (NoLivesLeft != null)
					NoLivesLeft(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Function to kill the player.
		/// </summary>
		private void Die()
		{
			_lives--;
			_dead = true;
			_deathTimer.Reset();

			if (_lives > 0)
			{
				_invicible = true;
				_flashShip = false;
				_godTimer.Reset();
			}
		}

		/// <summary>
		/// Function to check the player missile and asteroid collisions.
		/// </summary>
		/// <param name="asteroids">List of asteroids to check.</param>
		public void CheckMissileAsteroidCollsion(List<Asteroid> asteroids)
		{
			foreach (Projectile missile in _missiles)
			{
				if (_asteroidColliders.Count > 0)
					_asteroidColliders.Clear();
								
				missile.CollidesWith<Asteroid>(asteroids, _asteroidColliders);				

				if (_asteroidColliders.Count > 0)
				{
					missile.Reset();

					foreach (Asteroid collide in _asteroidColliders)
					{
						_playerScore += collide.Score;
						if (MissileHit != null)
							MissileHit(this, new HitEventArgs(collide.Score, collide));
						collide.Reset();
					}
				}
			}
		}

		/// <summary>
		/// Function to check the player missile and alien collisions.
		/// </summary>
		/// <param name="alien">Alien to check.</param>
		public void CheckMissileAlienCollsion(Alien alien)
		{
			if (!alien.Active)
				return;

			foreach (Projectile missile in _missiles)
			{
				if (missile.CollidesWith<Alien>(alien))
				{
					missile.Reset();

					alien.ShieldLevel--;
					if (alien.ShieldLevel < 0)
					{
						_playerScore += alien.Score;
						if (MissileHit != null)
							MissileHit(this, new HitEventArgs(alien.Score, alien));
						alien.Reset();
					}
					else
					{
						_playerScore += 10;
						if (MissileHit != null)
							MissileHit(this, new HitEventArgs(10, alien));
					}
				}
			}
		}

		/// <summary>
		/// Function to check player and asteroid collision.
		/// </summary>
		/// <param name="asteroids">List of asteroids to check.</param>
		public void CheckAsteroidCollision(List<Asteroid> asteroids)
		{
			_asteroidColliders.Clear();

			if (_invicible)
				return;

			foreach (Asteroid asteroid in asteroids)
			{
				if ((asteroid.CollidesWith<Player>(this)) && (asteroid.Position.Y > 0))
				{
					_asteroidColliders.Add(asteroid);
					asteroid.Reset();
				}
			}

			if (_asteroidColliders.Count > 0)
				Die();
		}

		/// <summary>
		/// Function to check a player and alien collision.
		/// </summary>
		/// <param name="alien">Alien to check.</param>
		public void CheckAlienCollision(Alien alien)
		{
			if ((alien.Position.Y > (Gorgon.Screen.Height / 4.0f)) && (CollidesWith<Alien>(alien)) && (alien.Active) && (!_invicible))
			{
				alien.Reset();
				Die();
			}
		}

		/// <summary>
		/// Function to check a player and alien blaster collision.
		/// </summary>
		/// <param name="alien">Alien to check.</param>
		public void CheckAlienBlasterCollision(Alien alien)
		{
			if ((!_invicible) && (alien.BlasterCollidesWithPlayer(this)))
				Die();
		}

		/// <summary>
		/// Function to fire a missile.
		/// </summary>
		public void FireMissile()
		{
			if ((_missileTimer.Seconds > 0.5) && (!_dead))
			{
				_missileTimer.Reset();
				foreach (Projectile missile in _missiles)
				{
					if (!missile.Active)
					{
						missile.Reset();
						missile.Active = true;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Function to update the player.
		/// </summary>
		/// <param name="frameTime">Frame delta.</param>
		public void Update(float frameTime)
		{
			_playerSprite.Animations[0].Advance(frameTime * 1000.0f);

			// Update any missiles that we may have active.
			if (!_dead)
			{
				foreach (Projectile missile in _missiles)
					missile.Update(frameTime, 0.0f);
			}

			if (_invicible)
			{
				if (_godTimer.Seconds > 4)
				{
					_invicible = false;
					_flashShip = false;
				}
				else
					_flashShip = !_flashShip;
			}

			// We need to reset.
			if ((_deathTimer.Seconds >= 4.0) && (_dead))
			{
				if (_lives < 1)
					Reset(true);
				else
				{
					Reset(false);
					_playerSprite.Position = _position;
					_invicible = true;
					_godTimer.Reset();
					_flashShip = false;
				}
			}
		}

		/// <summary>
		/// Function to draw the player.
		/// </summary>
		public void Draw()
		{
			foreach (Projectile missile in _missiles)
				missile.Draw();


			if (_dead)
			{
				if (_lives > 0)
					_message.Text = "Crap.  Let's try that again in " + string.Format("{0:0}", 5.0 - _deathTimer.Seconds) + ".";
				else
					_message.Text = "OK.  Clearly you fail at life.";
				
				_message.Draw();
			}
			else
			{
				_playerSprite.Position = _position;
				if (!_flashShip)
					_playerSprite.Draw();

#if (DEBUG && DEBUGDATA)
				Gorgon.Screen.BeginDrawing();
				Gorgon.Screen.Rectangle(CollisionRectangle.Left, CollisionRectangle.Top, CollisionRectangle.Width, CollisionRectangle.Height, Drawing.Color.Green);
				Gorgon.Screen.Rectangle(MovementRange.Left, MovementRange.Top, MovementRange.Width, MovementRange.Height, Drawing.Color.Blue);
				Gorgon.Screen.EndDrawing();
#endif
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Player"/> class.
		/// </summary>
		/// <param name="playerSprite">The player sprite.</param>
		/// <param name="missileSprite">Sprite used for the player missile.</param>
		/// <param name="messageFont">Font used for messages.</param>
		public Player(Sprite playerSprite, Sprite missileSprite, Font messageFont)
		{
			float hScale = 0.0f;			// Horizontal scale.
			float vScale = 0.0f;			// Vertical scale.

			playerSprite.Animations[0].AnimationState = AnimationState.Playing;
			missileSprite.Animations[0].AnimationState = AnimationState.Playing;

			playerSprite.Scale = MainForm.SpriteScales;
			missileSprite.Scale = MainForm.SpriteScales;

			_playerSprite = playerSprite;
			_missileSprite = missileSprite;
			_deathTimer = new PreciseTimer();
			_godTimer = new PreciseTimer();
			_missileTimer = new PreciseTimer();
			_message = new TextSprite("MessageText", string.Empty, messageFont, Drawing.Color.Black);
			_message.WordWrap = true;
			_message.Alignment = Alignment.Center;
			_asteroidColliders = new List<Asteroid>();
			_missiles = new List<Projectile>();
			_position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height);
			
			hScale = Gorgon.Screen.Width / 1280.0f;
			vScale = Gorgon.Screen.Height / 1024.0f;

			Reset(true);
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
				Drawing.Rectangle playerBounds = Drawing.Rectangle.Round(_playerSprite.AABB);

				playerBounds.Inflate(-20, -20);

				return playerBounds;
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
			if (_invicible)
				return false;

			return colliderObject.CollisionRectangle.IntersectsWith(CollisionRectangle);
		}
		#endregion
		#endregion
	}
}
