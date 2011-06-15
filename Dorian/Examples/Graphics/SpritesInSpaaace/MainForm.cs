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
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dialogs;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: Form
	{
		#region Value Types.
		/// <summary>
		/// Value type representing a star.
		/// </summary>
		private struct Star
		{
			/// <summary>
			/// Position of the star.
			/// </summary>
			public Vector2D Position;
			/// <summary>
			/// Magnitude of the star.
			/// </summary>
			public Drawing.Color Magnitude;
			/// <summary>
			/// Vertical delta.
			/// </summary>
			public float VDelta;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="position">Position of the star.</param>
			/// <param name="magnitude">Magnitude of the star.</param>
			public Star(Vector2D position, Drawing.Color magnitude)
			{
				Position = position;
				Magnitude = magnitude;
				VDelta = 0;
			}
		}
		#endregion

		#region Variables.
		private Random _rnd = new Random();			// Our handy dandy random number generator.
		private Star[,] _stars;						// Star positions and layers.
		private Image _spriteImage = null;			// Sprite image.
		private Sprite _ship = null;				// Ship sprite.
		private Sprite _planet = null;				// Planet sprite.
		private Sprite _cloud = null;				// Cloud sprite.
		private float _shipDelta = 60.0f;			// Ship delta.
		private float _shipPos = 0.0f;				// Ship position.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw a layer of stars.
		/// </summary>
		/// <param name="layer">Layer to draw.</param>
		/// <param name="deltaTime">Frame delta.</param>
		private void DrawStars(int layer, float deltaTime)
		{
			Gorgon.Screen.BeginDrawing();

			// Draw the stars.
			for (int i = 0; i < _stars.Length / 4; i++)
			{
				Gorgon.Screen.SetPoint((int)_stars[i, layer].Position.X, (int)_stars[i, layer].Position.Y, _stars[i, layer].Magnitude);

				// Move the stars down.
				_stars[i, layer].Position.Y += _stars[i, layer].VDelta * deltaTime;

				// Wrap around.
				if (_stars[i, layer].Position.Y > Gorgon.Screen.Height)
					_stars[i, layer].Position = new Vector2D((float)(_rnd.NextDouble() * Gorgon.Screen.Width), 0);
			}

			Gorgon.Screen.EndDrawing();
		}

		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			// Do nothing here.  When we need to update, we will.
			Gorgon.Screen.Clear();

			// Draw stars.
			DrawStars(3, e.FrameDeltaTime);

			// Draw the cloud.
			_cloud.Scale = new Vector2D((Gorgon.Screen.Width / _cloud.Width) * 2.0f, (Gorgon.Screen.Height / _cloud.Height) * 2.0f);
			_cloud.Position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);
			_cloud.Color = Drawing.Color.White;
			_cloud.Opacity = 255;
			_cloud.Draw();

			// Draw stars.
			DrawStars(2, e.FrameDeltaTime);

			// Draw the planet.
			_planet.UniformScale = (float)(Gorgon.Screen.Height / _planet.Height);
			_planet.Draw();

			// Draw stars.
			for (int layer = 1; layer >= 0; layer--)
				DrawStars(layer, e.FrameDeltaTime);

			// Move the ship left and right
			if (_ship.Position.X < 50)
				_shipDelta = 120.0f;
			if (_ship.Position.X > Gorgon.Screen.Width - 50)
				_shipDelta = -120.0f;

			_shipPos = _shipPos + (_shipDelta * e.FrameDeltaTime);

			_ship.SetPosition((int)_shipPos, _ship.Position.Y - (1.5f * e.FrameDeltaTime));
			_ship.Draw();

			// Draw another cloud.
			_cloud.Scale = new Vector2D((Gorgon.Screen.Width / _cloud.Width) * 1.7f, (Gorgon.Screen.Height / _cloud.Height) * 1.25f);
			_cloud.Position = new Vector2D((Gorgon.Screen.Width / 2.0f) - (Gorgon.Screen.Width / 8.0f), Gorgon.Screen.Height / 2.0f);
			_cloud.Color = Drawing.Color.Cyan;
			_cloud.Opacity = (byte)(((MathUtility.Abs((_shipPos - (Gorgon.Screen.Width / 2.0f)) / (Gorgon.Screen.Width / 2.0f))) * 223.0f) + 32.0f);
			_cloud.Draw();
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		private void Initialize()
		{
			// Set smoothing mode to all the sprites.
			Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;

			// Create stars.
			_stars = new Star[64,4];

			// Create the star.
			for (int layer = 0; layer < 4; layer++)
			{
				for (int i = 0; i < _stars.Length / 4; i++)
				{
					_stars[i, layer].Position = new Vector2D((float)(_rnd.NextDouble() * Gorgon.Screen.Width), (float)(_rnd.NextDouble() * Gorgon.Screen.Height));					

					// Select magnitude.
					switch (layer)
					{
						case 0:
							_stars[i, layer].Magnitude = Drawing.Color.FromArgb(255, 255, 255);
							_stars[i, layer].VDelta = (float)(_rnd.NextDouble() * 100.0) + 55.0f;
							break;
						case 1:
							_stars[i, layer].Magnitude = Drawing.Color.FromArgb(192, 192, 192);
							_stars[i, layer].VDelta = (float)(_rnd.NextDouble() * 50.0) + 27.5f;
							break;
						case 2:
							_stars[i, layer].Magnitude = Drawing.Color.FromArgb(128, 128, 128);
							_stars[i, layer].VDelta = (float)(_rnd.NextDouble() * 25.0) + 13.5f;
							break;
						default:
							_stars[i, layer].Magnitude = Drawing.Color.FromArgb(64, 64, 64);
							_stars[i, layer].VDelta = (float)(_rnd.NextDouble() * 12.5) + 1.0f;
							break;
					}
				}
			}

			// Load base image.
			if (_spriteImage == null)
				_spriteImage = Image.FromFile(@"..\..\..\..\Resources\Sprites\SpritesInSpaaace\SIS.png");

			// Load sprites.
			if (_ship == null)
				_ship = Sprite.FromFile(@"..\..\..\..\Resources\Sprites\SpritesInSpaaace\Ship.gorSprite");
			_ship.Position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height - _ship.Height);
			_shipPos = _ship.Position.X;

			if (_planet == null)
				_planet = Sprite.FromFile(@"..\..\..\..\Resources\Sprites\SpritesInSpaaace\Planet.gorSprite");
			_planet.UniformScale = 2.0f;
			_planet.Axis = new Vector2D(_planet.Width / 2.0f, _planet.Height / 2.0f);
			_planet.Position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);

			// Create a sprite.
			_cloud = new Sprite("Cloud", _spriteImage, new Vector2D(0, 173), new Vector2D(183, 67));
			_cloud.Scale = new Vector2D((Gorgon.Screen.Width / _cloud.Width) * 2.0f, (Gorgon.Screen.Height / _cloud.Height) * 2.0f);
			_cloud.Axis = new Vector2D(_cloud.Width / 2.0f, _cloud.Height / 2.0f);
			_cloud.Position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);
			_cloud.Opacity = 200;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape)
				Close();

			if (e.KeyCode == Keys.S)
				Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Perform clean up.
			Gorgon.Terminate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Initialize Gorgon
				// Set it up so that we won't be rendering in the background, but allow the screensaver to activate.
				Gorgon.Initialize(false, true);

				// Display the logo.
				Gorgon.LogoVisible = true;
				Gorgon.FrameStatsVisible = false;

				// Set the video mode.
				ClientSize = new Drawing.Size(640, 400);
				Gorgon.SetMode(this);

				// Set an ugly background color.
				Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(0, 0, 16);

				// Initialize the stars.
				Initialize();

				// Assign idle event.
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);

				Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);

				// Begin rendering.
				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to initialize the application.", ex);
			}
		}

		/// <summary>
		/// Handles the DeviceReset event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
			Initialize();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}
		#endregion
	}
}