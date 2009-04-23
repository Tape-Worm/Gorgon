#region MIT.
// 
// Examples.
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
// Created: Thursday, October 02, 2008 10:46:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Framework;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using Dialogs;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	/// <remarks>This is in no way a good example of game design.  In fact, it's horrible.  This is more a tutorial on how to use Gorgon than it is to write games.
	/// So please, do not copy this 'style'.  It's horrible.</remarks>
	public partial class MainForm 
		: GorgonApplicationWindow
	{
		#region Constants.
		/// <summary>Allow only five asteroids at a time.</summary>
		private const int AsteroidCount = 5;
		#endregion

		#region Variables.
		private Player _player = null;								// Player object.
		private Alien _alien = null;								// Alien idiot.
		private Image _spriteSheet = null;							// Our sprite sheet.
		private Stars _stars = null;								// Background stars.
		private List<Asteroid> _asteroids = null;					// Asteroids.
		private List<Asteroid> _asteroidColliders = null;			// List of asteroid collisions.
		private Drawing.Text.PrivateFontCollection _fonts = null;	// Font collection.
		private Font _font = null;									// Font for text.
		private TextSprite _text = null;							// Text to display.
		private PreciseTimer _timer = null;							// Timer.
		private PreciseTimer _alienTimer = null;					// Timer used to show the alien.
		private float _difficulty = 0.0f;							// Difficulty.
		private float _difficultyMod = 0.0f;						// Difficulty modifier.
		private static Random _rnd = new Random();					// Random numbers.
		private TitleAndSetup _title = null;						// Title screen.
		private Joystick _stick = null;								// Joystick to use.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return our global random number generator.
		/// </summary>
		public static Random Random
		{
			get
			{
				return _rnd;
			}
		}

		/// <summary>
		/// Property to return the scale for the resolutions.
		/// </summary>
		public static Vector2D SpriteScales
		{
			get
			{
				float x = Gorgon.Screen.Width / 1280.0f;		// X scale.
				float y = Gorgon.Screen.Height / 960.0f;		// Y scale.

				return new Vector2D(x, y);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the TitleClosed event of the _title control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _title_TitleClosed(object sender, EventArgs e)
		{
			_title.TitleClosed -= new EventHandler(_title_TitleClosed);
			_stick = _title.SelectedStick;
			if (_stick != null)
			{
				_stick.DeadZone[0] = new MinMaxRange(_stick.AxisRanges[0].Minimum + _stick.AxisRanges[0].Range / 4, _stick.AxisRanges[0].Maximum - _stick.AxisRanges[0].Range / 4);
				_stick.DeadZone[1] = new MinMaxRange(_stick.AxisRanges[1].Minimum + _stick.AxisRanges[1].Range / 4, _stick.AxisRanges[1].Maximum - _stick.AxisRanges[1].Range / 4);
				_stick.Exclusive = true;
				_stick.Enabled = true;
			}
			_title = null;
			_alienTimer.Reset();
		}

		/// <summary>
		/// Function to load a font from the file system.
		/// </summary>
		/// <param name="font">Font name to load.</param>
		private void LoadFont(string font)
		{
			byte[] fontData = null;				// Font data.

			// Read the font into a private collection.
			fontData = FileSystems["NoCleverNameFS"].ReadFile(font);

			// Convert byte data to a font.
			unsafe
			{
				fixed (byte* fontPointer = &fontData[0])
					_fonts.AddMemoryFont(new IntPtr((void*)fontPointer), fontData.Length);
			}

			// Create the bitmap font.
			_font = new Font("GameFont", _fonts.Families[0], 24.0f, false, true);
			_font.CharacterList = "!.:()/_'0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		}

		/// <summary>
		/// Handles the NoLivesLeft event of the _player control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _player_NoLivesLeft(object sender, EventArgs e)
		{
			// Reset all the other parts.
			foreach (Asteroid asteroid in _asteroids)
				asteroid.Reset();

			if (_alien != null)
				_alien.Reset();

			_timer.Reset();
			_difficultyMod = 0;
			_difficulty = 0;
		}
		
		/// <summary>
		/// Handles the MissileHit event of the _player control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _player_MissileHit(object sender, HitEventArgs e)
		{
			_difficulty += (float)e.Score / 100.0f;
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);

			// Do nothing when title is displayed.
			if (_title != null)
				return;

			if (_stick != null)
			{
				// Set the player position with the joystick.
				if (_stick.AxisDirection[0] == JoystickDirections.Left)
					_player.Position = new Vector2D(_player.Position.X - ((Gorgon.Screen.Width) * e.FrameDeltaTime), _player.Position.Y);
				if (_stick.AxisDirection[0] == JoystickDirections.Right)
					_player.Position = new Vector2D(_player.Position.X + ((Gorgon.Screen.Width) * e.FrameDeltaTime), _player.Position.Y);
				if (_stick.AxisDirection[1] == JoystickDirections.Up)
					_player.Position = new Vector2D(_player.Position.X, _player.Position.Y - ((Gorgon.Screen.Height) * e.FrameDeltaTime));
				if (_stick.AxisDirection[1] == JoystickDirections.Down)
					_player.Position = new Vector2D(_player.Position.X, _player.Position.Y + ((Gorgon.Screen.Height) * e.FrameDeltaTime));

				// Fire a missile if we hit a button.
				for (int i = 0; i < _stick.ButtonCount; i++)
				{
					if (_stick.Button[i])
					{
						_player.FireMissile();
						break;
					}
				}
			}

			_player.Update(e.FrameDeltaTime);

			if (!_player.IsDead)
			{
				_stars.Update(e.FrameDeltaTime, _difficultyMod);

				if (_difficulty > 30.0f)
				{
					_difficultyMod += 5.0f;
					if (_difficultyMod > Gorgon.Screen.Height)
						_difficultyMod = Gorgon.Screen.Height;
					_difficulty = 0.0f;
				}				

				foreach (Asteroid asteroid in _asteroids)
					asteroid.Update(e.FrameDeltaTime, _difficultyMod);


				// Set the alien to active at some random point.
				if ((!_alien.Active) && (_alienTimer.Seconds > 15 + _rnd.Next(10)))
					_alien.Active = true;
				else
				{
					// Keep resetting the timer.
					if (_alien.Active)
						_alienTimer.Reset();
				}

				// Update the alien.
				_alien.Update(e.FrameDeltaTime, _difficultyMod);

				// Check to see if the player has destroyed any asteroids.
				_player.CheckMissileAlienCollsion(_alien);
				_player.CheckMissileAsteroidCollsion(_asteroids);				

				// Check for player collisions.
				_player.CheckAsteroidCollision(_asteroids);
				_player.CheckAlienCollision(_alien);
				_player.CheckAlienBlasterCollision(_alien);

				// Reset mouse.
				if (_player.IsDead)
					Input.Mouse.SetPosition(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height);

				// Auto-increment score.
				if (_timer.Seconds > 1.0)
				{
					_player.Score++;

					_difficulty += 0.25f;
					_timer.Reset();
				}
			}
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			if (_title != null)
			{
				_title.DrawTitle();
				return;
			}

			_stars.Draw();
						
			foreach (Asteroid asteroid in _asteroids)
				asteroid.Draw();

			_alien.Draw();

			_player.Draw();

			_text.Text = "Score: " + _player.Score.ToString("0000000");
			_text.Alignment = Alignment.UpperLeft;
			_text.Draw();

			_text.Text = "Lives: " + _player.LivesLeft.ToString();
			_text.Alignment = Alignment.UpperRight;
			_text.Draw();

			// Draw debug data.
#if (DEBUG && DEBUGDATA)
			_text.Text = "Difficulty counter: " + _difficulty.ToString("0.000") + "\nDifficulty Modifier: " + _difficultyMod.ToString("0.000");
			if (_stick != null)
			{
				_text.Text += "\nStick Data:\nAxis 0:" + _stick.AxisDirection[0].ToString() + "\nAxis 1:" + _stick.AxisDirection[1] + "\nAxis 2:" + _stick.AxisDirection[2] +
				"\nX: " + _stick.X.ToString() + "\nY: " + _stick.Y.ToString() + "\nZ: " + _stick.Z.ToString();
			}
			_text.Alignment = Alignment.UpperLeft;
			_text.SetPosition(0, _font.CharacterHeight);
			_text.Draw();
			_text.Position = Vector2D.Zero;
#endif
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseMovement(MouseInputEventArgs e)
		{
			base.OnMouseMovement(e);

			if (_stick == null)
				_player.Position = e.Position;
		}

		/// <summary>
		/// Function called when a mouse button is pushed down.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseButtonDown(MouseInputEventArgs e)
		{
			base.OnMouseButtonDown(e);

			if (_stick == null)
				_player.FireMissile();
		}

		/// <summary>
		/// Function called when the video device is set to a lost state.
		/// </summary>
		protected override void OnDeviceLost()
		{
			base.OnDeviceLost();
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected override void OnDeviceReset()
		{
			base.OnDeviceReset();

			Input.Mouse.SetPositionRange(_player.MovementRange.Left, _player.MovementRange.Top, _player.MovementRange.Width, _player.MovementRange.Height);
		}

		/// <summary>
		/// Function called when a keyboard key is pushed down.
		/// </summary>
		/// <param name="e">Keyboard event parameters.</param>
		protected override void OnKeyboardKeyDown(KeyboardInputEventArgs e)
		{
			base.OnKeyboardKeyDown(e);

			if (_title != null)
				_title.KeyDown(e);
		}

		/// <summary>
		/// Function called before Gorgon is shut down.
		/// </summary>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		/// <remarks>Users should override this function to perform clean up when the application closes.</remarks>
		protected override bool OnGorgonShutDown()
		{
			if (_title != null)
			{
				_title.TitleClosed -= new EventHandler(_title_TitleClosed);
				_title = null;
			}

			if (_player != null)
			{
				_player.NoLivesLeft -= new EventHandler(_player_NoLivesLeft);
				_player.MissileHit -= new HitEventHandler(_player_MissileHit);
			}

			if (_fonts != null)
				_fonts.Dispose();

			return true;
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			Sprite asteroid1 = null;							// Asteroid sprite.
			Sprite asteroid2 = null;							// Asteroid sprite.
			Sprite asteroid3 = null;							// Asteroid sprite.

			try
			{
				// Smooth out items.
				Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;

				_timer = new PreciseTimer();
				_alienTimer = new PreciseTimer();
				_fonts = new Drawing.Text.PrivateFontCollection();

				// Load the font.
				LoadFont("/Fonts/JOYSTIX.TTF");
				_text = new TextSprite("GameText", string.Empty, _font, Drawing.Color.Black);

				// Load data.
				_spriteSheet = Image.FromFileSystem(FileSystems["NoCleverNameFS"], "/Images/CleverSprites.png");

				// Load player.
				_player = new Player(Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/You.gorSprite"), 
					Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/Rocket.gorSprite"), _font);
				_player.NoLivesLeft += new EventHandler(_player_NoLivesLeft);
				_player.MissileHit += new HitEventHandler(_player_MissileHit);
				Input.Mouse.Position = _player.Position;

				// Load alien.
				_alien = new Alien(Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/Alien.gorSprite"), 
					Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/Blaster.gorSprite"), 
					Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/AlienShield.gorSprite"));

				// Limit the mouse.
				Input.Mouse.SetPositionRange(_player.MovementRange.Left, _player.MovementRange.Top, _player.MovementRange.Width, _player.MovementRange.Height);

				// Create stars.
				_stars = new Stars();
				_asteroids = new List<Asteroid>();
				_asteroidColliders = new List<Asteroid>();

				asteroid1 = Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/Asteroid1.gorSprite");
				asteroid2 = Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/Asteroid2.gorSprite");
				asteroid3 = Sprite.FromFileSystem(FileSystems["NoCleverNameFS"], "/Sprites/Asteroid3.gorSprite");

				for (int i = 0; i < AsteroidCount; i++)				
					_asteroids.Add(new Asteroid(asteroid1, asteroid2, asteroid3));

				// Set up title screen.
				_title = new TitleAndSetup(_font, Input);
				_title.TitleClosed += new EventHandler(_title_TitleClosed);				
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error trying to initialize the application.", ex);
				Close();
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\NoCleverNameSettings.xml")
		{
			InitializeComponent();
 		}
		#endregion
	}
}