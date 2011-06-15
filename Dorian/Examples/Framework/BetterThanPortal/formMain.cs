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
using System.ComponentModel;
using System.Data;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.InputDevices;
using GorgonLibrary.Graphics;
using GorgonLibrary.Framework;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main form.
	/// </summary>
	public partial class formMain 
		: GorgonApplicationWindow
	{
		#region Constants
		private int NEBULA_LAYER_COUNT = 3;							// Nebula layer count.
		#endregion

		#region Variables.
		private NebulaLayer[] _nebulaLayers = null;					// Nebula layers.
		private LightningEffect _lightningFX = null;				// Lightning effect.
		private Stars _stars = null;								// Stars.
		private Camera _camera = null;								// Camera view.
		private PlanetaryObject _planet = null;						// The planet.
		private Cruiser _cruiser = null;							// Space cruiser.
		private SmallShip _smallShip = null;						// Small ship.
		private SunObject _largeStar = null;						// Large star.
		private Image _spriteSheet1 = null;							// Sprite sheet image.		
		private TextSprite _text;									// Text info.
		private Random _rnd = new Random();							// Random numbers.
		private bool _slowDown = false;								// Flag to indicate that we're slowing down.
		private PreciseTimer _slowDownTimer;						// Slowdown timer.
		private bool _showHelp = true;								// Flag to show help.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the LightningEvent event of the _lightningFX control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Example.LightningFlashEventArgs"/> instance containing the event data.</param>
		private void _lightningFX_LightningEvent(object sender, LightningFlashEventArgs e)
		{
			if (e.BlendMode != BlendingModes.None)
			{
				_smallShip.BlendMode = _cruiser.BlendMode = _planet.BlendMode = e.BlendMode;
				_stars.Tint = _smallShip.Tint = _cruiser.Tint = _planet.Tint = Drawing.Color.White;
			}
			else
			{
				_smallShip.BlendMode = _cruiser.BlendMode = _planet.BlendMode = BlendingModes.Modulated;
				_stars.Tint = _smallShip.Tint = _cruiser.Tint = _planet.Tint = Drawing.Color.FromArgb(212, 133, 179);
			}
		}

		/// <summary>
		/// Function called before Gorgon is shut down.
		/// </summary>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		/// <remarks>Users should override this function to perform clean up when the application closes.</remarks>
		protected override bool OnGorgonShutDown()
		{
			if (_lightningFX != null)
				_lightningFX.LightningEvent -= new LightningFlashEventHandler(_lightningFX_LightningEvent);

			// Remove events.
			if (_nebulaLayers != null)
			{
				foreach (NebulaLayer layer in _nebulaLayers)
					layer.Dispose();
			}

			return true;
		}

		/// <summary>
		/// Function called when a keyboard key is pushed down.
		/// </summary>
		/// <param name="e">Keyboard event parameters.</param>
		protected override void OnKeyboardKeyDown(KeyboardInputEventArgs e)
		{
			base.OnKeyboardKeyDown(e);

			// If we don't have an input interface, then leave.
			if (Input == null)
				return;

			if (Input.Keyboard.KeyStates[KeyboardKeys.F1] == KeyState.Down)
				_showHelp = !_showHelp;

            if (Input.Keyboard.KeyStates[KeyboardKeys.Left] == KeyState.Down)
				_smallShip.Angle += 2.5f;

            if (Input.Keyboard.KeyStates[KeyboardKeys.Right] == KeyState.Down)
				_smallShip.Angle -= 2.5f;

            if (Input.Keyboard.KeyStates[KeyboardKeys.Up] == KeyState.Down)			
			{
				_smallShip.Velocity += 2.5f;
				_slowDown = false;
			}

            if (Input.Keyboard.KeyStates[KeyboardKeys.Down] == KeyState.Down)
			{
				_smallShip.Velocity -= 2.5f;
				_slowDown = false;
			}

            if (Input.Keyboard.KeyStates[KeyboardKeys.Back] == KeyState.Down)
				_slowDown = true;

            if (Input.Keyboard.KeyStates[KeyboardKeys.C] == KeyState.Down)
			{
				if (_camera.Target == null)
					_camera.Target = _smallShip;
				else
				{
					if (_camera.Target == _smallShip)
						_camera.Target = _cruiser;
					else
						_camera.Target = null;
				}
			}
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);

			// Update lightning.
			_lightningFX.Update();

			// Update direction vector.
			for (int i = 0; i < _nebulaLayers.Length; i++)
			{
				if (_nebulaLayers[i] != null)
					_nebulaLayers[i].Update(e.FrameDeltaTime);
			}

			_smallShip.Update(_camera, e.FrameDeltaTime);
			_cruiser.Update(_camera, e.FrameDeltaTime);			

			// Update stars.
			_stars.Update(_camera, e.FrameDeltaTime);

			_largeStar.Update(_camera, e.FrameDeltaTime);
			_planet.Update(_camera, e.FrameDeltaTime);

			// Perform the slowing down if we've hit backspace.
			if (_slowDown)
			{
				if (_slowDownTimer.Milliseconds > 5.0f)
				{
					_smallShip.Velocity -= 0.5f;
					_slowDownTimer.Reset();
				}

				if (_smallShip.Velocity < 0)
				{
					_slowDown = false;
					_smallShip.Velocity = 0;
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

			_nebulaLayers[0].Draw();
			_stars.Draw();

			_largeStar.Draw();
			_planet.Draw();

			_nebulaLayers[1].Draw();

			_cruiser.Draw();

			_smallShip.Draw();

			_nebulaLayers[2].Draw();

			_text.Position = Vector2D.Zero;
			_text.Color = Drawing.Color.Yellow;
			_text.Text = string.Format("Your Position: {0:0}x{1:0}\n", _camera.Position.X, _camera.Position.Y);
			_text.Draw();

			if (_camera.Target != null)
			{
				float currentVelocity = _camera.Target.Velocity;		// Get the current velocity.
				float maxVelocity = _camera.Target.MaxVelocity;			// Maximum velocity.

				_text.SetPosition(0, FrameworkFont.CharacterHeight);
				_text.Text = "Velocity: ";
				_text.Draw();

				Gorgon.Screen.BeginDrawing();
				Gorgon.Screen.BlendingMode = BlendingModes.Modulated;

				Gorgon.Screen.FilledRectangle(_text.Width + 5.0f, _text.Position.Y + 2.0f, 150.0f, _text.Height, Drawing.Color.FromArgb(128, 64, 64, 64));

				Gorgon.Screen.BlendingMode = BlendingModes.Additive;
				
				if (currentVelocity > 0)
					Gorgon.Screen.FilledRectangle(_text.Width + 5.0f, _text.Position.Y + 2.0f, 150.0f * (currentVelocity / maxVelocity), _text.Height, Drawing.Color.FromArgb(255,Drawing.Color.Green));
				else
				{
					if (currentVelocity < 0)
						Gorgon.Screen.FilledRectangle(_text.Width + 5.0f, _text.Position.Y + 2.0f, 150.0f * (-currentVelocity / maxVelocity), _text.Height, Drawing.Color.FromArgb(255, Drawing.Color.Red));
				}

				Gorgon.Screen.BlendingMode = BlendingModes.Modulated;
				Gorgon.Screen.Rectangle(_text.Width + 5.0f, _text.Position.Y + 2.0f, 150.0f, _text.Height, Drawing.Color.Black);
				Gorgon.Screen.EndDrawing();
								
				_text.SetPosition(_text.Width + 35.0f, FrameworkFont.CharacterHeight);
				_text.Text = string.Format("{0:000.0} / {1:000.0}", currentVelocity, maxVelocity);
				_text.Draw();
			}

			if (_showHelp)
			{
				_text.SetPosition(0, FrameworkFont.CharacterHeight * 3.0f);
				_text.Color = Drawing.Color.White;
				_text.Text = "Hit F1 to show or hide this text.\nPress 'C' to switch camera views.\nPress \u2190 or \u2192 to turn.\nPress \u2191 or \u2193 to accelerate or decelerate.\nPress Backspace to set velocity to 0.\nPress ESC to quit.";
				_text.Draw();
			}
		}

		/// <summary>
		/// Function to do initialization.
		/// </summary>
		protected override void Initialize()
		{
			Sprite nebulaSprite = null;						// Nebula sprite.

			base.Initialize();

			Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;

			_lightningFX = new LightningEffect(Drawing.Color.FromArgb(212, 133, 179), Drawing.Color.White, BlendingModes.Additive);
			_camera = new Camera(Vector2D.Zero);

			// Load in the objects.
			_spriteSheet1 = Image.FromFileSystem(FileSystems[ApplicationName], "Images/SpaceStuff.png");

			_planet = new PlanetaryObject(Sprite.FromFileSystem(FileSystems[ApplicationName], "Sprites/Planet.gorSprite"), new Vector2D(-1000.0f, -1000.0f));
			_planet.ScalePlanet(512.0f);

			_smallShip = new SmallShip(Sprite.FromFileSystem(FileSystems[ApplicationName], "Sprites/SmallShip.gorSprite"), Vector2D.Zero);
			_smallShip.MaxVelocity = 220.0f;

			_cruiser = new Cruiser(Sprite.FromFileSystem(FileSystems[ApplicationName], "Sprites/BigShip.gorSprite"), new Vector2D(-800.0f, -750.0f));
			_cruiser.Angle = 47.5f;
			_cruiser.Velocity = 5.0f;
			_cruiser.MaxVelocity = 5.0f;

			_largeStar = new SunObject(Sprite.FromFileSystem(FileSystems[ApplicationName], "Sprites/LargeStar.gorSprite"), new Vector2D(-1550.0f, -1250.0f));

			// Create nebula layers.
			nebulaSprite = Sprite.FromFileSystem(FileSystems[ApplicationName], "Sprites/Nebula.gorSprite");
			nebulaSprite.Axis = new Vector2D(nebulaSprite.Width / 2.0f, nebulaSprite.Height / 2.0f);

			// Set up the nebula cloud layers.
			_nebulaLayers = new NebulaLayer[NEBULA_LAYER_COUNT];
			_nebulaLayers[0] = new NebulaLayer(nebulaSprite, _lightningFX);
			_nebulaLayers[0].Scale = new Vector2D(Gorgon.Screen.Width / nebulaSprite.Width * 1.15f, Gorgon.Screen.Height / nebulaSprite.Height * 1.15f);
			_nebulaLayers[0].Offset = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);
			_nebulaLayers[0].Tint = Drawing.Color.FromArgb(212, 133, 179);
			_nebulaLayers[0].BlendMode = BlendingModes.Modulated;
			_nebulaLayers[0].AllowRotation = true;
			_nebulaLayers[0].AngleVelocity = 0.3f;
			_nebulaLayers[0].AngleRange = 17.0f;

			_nebulaLayers[1] = new NebulaLayer(nebulaSprite, _lightningFX);
			_nebulaLayers[1].Scale = new Vector2D(Gorgon.Screen.Width / nebulaSprite.Width * 4.05f, Gorgon.Screen.Height / nebulaSprite.Height * 3.72f);
			_nebulaLayers[1].Offset = new Vector2D(Gorgon.Screen.Width / 8.0f, Gorgon.Screen.Height / 6.8f);
			_nebulaLayers[1].Tint = Drawing.Color.FromArgb(212, 133, 179);
			_nebulaLayers[1].BlendMode = BlendingModes.Modulated;
			_nebulaLayers[1].AllowRotation = true;
			_nebulaLayers[1].AngleVelocity = 1.5f;
			_nebulaLayers[1].AngleRange = 275.0f;
			_nebulaLayers[1].AllowTranslation = true;
			_nebulaLayers[1].OffsetVelocity = new Vector2D(-2.0f, 1.25f);
			_nebulaLayers[1].OffsetRange = new Vector2D(Gorgon.Screen.Width / 4.0f, Gorgon.Screen.Height / 4.0f);
			_nebulaLayers[1].AllowScale = true;
			_nebulaLayers[1].ScaleRange = new Vector2D(2.0f, 2.33f);
			_nebulaLayers[1].ScaleVelocity = new Vector2D(0.25f, 0.125f);

			_nebulaLayers[2] = new NebulaLayer(nebulaSprite, _lightningFX);
			_nebulaLayers[2].Angle = 176.3f;
			_nebulaLayers[2].Scale = new Vector2D(Gorgon.Screen.Width / nebulaSprite.Width * 6.05f, Gorgon.Screen.Height / nebulaSprite.Height * 4.32f);
			_nebulaLayers[2].Offset = new Vector2D(Gorgon.Screen.Width / 6.0f, Gorgon.Screen.Height / 2.1f);
			_nebulaLayers[2].Tint = Drawing.Color.FromArgb(212, 133, 179);
			_nebulaLayers[2].BlendMode = BlendingModes.Modulated;
			_nebulaLayers[2].AllowRotation = true;
			_nebulaLayers[2].AngleVelocity = 0.75f;
			_nebulaLayers[2].AngleRange = 120.0f;
			_nebulaLayers[2].AllowTranslation = true;
			_nebulaLayers[2].OffsetVelocity = new Vector2D(-1.25f, 0.75f);
			_nebulaLayers[2].OffsetRange = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);
			_nebulaLayers[2].Opacity = 192;
			_nebulaLayers[2].AllowScale = true;
			_nebulaLayers[2].ScaleRange = new Vector2D(4.333f, 3.0f);
			_nebulaLayers[2].ScaleVelocity = new Vector2D(-0.525f, 0.25f);			

			// Create stars.
			_stars = new Stars(FileSystems[ApplicationName], 16);

			// Create text.			
			FrameworkFont.CharacterList += "\u2190\u2191\u2192\u2193";
			FrameworkFont.Bold = true;
			// Add some padding.
			FrameworkFont.GlyphWidthPadding = 1;
			_text = new TextSprite("TextInfo", string.Empty, FrameworkFont);
			_text.Color = Drawing.Color.Yellow;
			_text.Shadowed = true;

			// Assign the 'player' ship as the primary camera target.
			_camera.Position = new Vector2D(1000.0f, 1000.0f);
			_camera.Target = _smallShip;

			// Set default 'tinting' for the objects.
			_stars.Tint = _smallShip.Tint = _cruiser.Tint = _planet.Tint = Drawing.Color.FromArgb(212, 133, 179);

			_lightningFX.LightningEvent += new LightningFlashEventHandler(_lightningFX_LightningEvent);

			_slowDownTimer = new PreciseTimer();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain"/> class.
		/// </summary>
		public formMain()
			: base(@".\BetterThanPortal.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}