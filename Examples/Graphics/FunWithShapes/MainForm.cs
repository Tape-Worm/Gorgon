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
using System.Drawing;
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
		#region Variables.
		private Random _rnd = new Random();		// Random number generator.
		private float _halfWidth;				// Half of screen width.
		private float _halfHeight;				// Half of screen height.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a random floating point number between 0..1.0f.
		/// </summary>
		/// <remarks>This is just for convenience.</remarks>
		private float RandomValue
		{
			get
			{
				return (float)_rnd.NextDouble();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw the pretty picture.
		/// </summary>
		private void DrawAPrettyPicture()
		{
			Color paintColor;		// Paint color.
			float sin = 0.0f;		// Sin.
			float cos = 0.0f;		// Cosine.			
			int colorSwitch = 0;	// Color component for the points.

			// Clear the back buffer.
			Gorgon.Screen.Clear();

			// First we need to lock the render target down for our drawing.  This ensures that the
			// drawing commands are sent to the right buffer.
			Gorgon.Screen.BeginDrawing();

			// Draw some points as stars.
			for (int x = 0; x < 1000; x++)
			{
				// Color.
				colorSwitch = _rnd.Next(160) + 95;

				// Get the star color.
				paintColor = Color.FromArgb(colorSwitch, colorSwitch, colorSwitch);

				Gorgon.Screen.SetPoint(RandomValue * Gorgon.Screen.Width, RandomValue * Gorgon.Screen.Height, paintColor);
			}

			// Set the blending mode so we can use translucent shapes.
			Gorgon.Screen.BlendingMode = BlendingModes.Modulated;

			// Draw lines.
			for (int x = 0; x < 360; x++)
			{
				cos = MathUtility.Cos(x + (x / 2));
				sin = MathUtility.Sin(x + (x / 3));

				// Set up a random color.				
				paintColor = Color.FromArgb((byte)_rnd.Next(128) + 127, _rnd.Next(64) + 191, _rnd.Next(64) + 191, 0);
				Gorgon.Screen.Line(sin + _halfWidth, cos + _halfHeight, cos * (RandomValue * _halfWidth), sin * (RandomValue * _halfHeight), paintColor);
			}

			// Draw a filled circle.
			Gorgon.Screen.FilledCircle(_halfWidth, _halfHeight, (_halfHeight / 2.0f) + (_rnd.Next(10) - 8), Color.Yellow);

			// Draw some circles in the filled circle.
			for (int x = 0; x < 25; x++)
				Gorgon.Screen.Circle((RandomValue * (_halfHeight / 2.0f)) + _halfWidth - (_halfHeight / 4.0f), 
					(RandomValue * (_halfHeight / 2.0f)) + _halfHeight - (_halfHeight / 4.0f), RandomValue * 5.0f, Color.Black);

			// Draw some black bars.
			Gorgon.Screen.FilledRectangle(0, 0, Gorgon.Screen.Width, Gorgon.Screen.Height / 6.0f, Color.Black);
			Gorgon.Screen.FilledRectangle(0, Gorgon.Screen.Height - (Gorgon.Screen.Height / 6.0f), Gorgon.Screen.Width, (Gorgon.Screen.Height / 6.0f) + 1.0f, Color.Black);

			// Always call this when done.
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
			Gorgon.Stop();
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
		}

		/// <summary>
		/// Raises the <see cref="E:Paint"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// If we get repainted we can redraw the image.
			DrawAPrettyPicture();
			Gorgon.Go();
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
				ClientSize = new Size(640, 400);
				Gorgon.SetMode(this);

				_halfWidth = Gorgon.Screen.Width / 2.0f;
				_halfHeight = Gorgon.Screen.Height / 2.0f;

				// Set an ugly background color.
				Gorgon.Screen.BackgroundColor = Color.FromArgb(0,0,64);

				// Assign idle event.
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);

				// Assign a device reset event so our image will be resized.
				Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);

				// Draw the image.
				DrawAPrettyPicture();

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
			// Get half width/height.
			_halfWidth = Gorgon.Screen.Width / 2.0f;
			_halfHeight = Gorgon.Screen.Height / 2.0f;

			// Update the image.
			DrawAPrettyPicture();

			// Restart drawing.
			Gorgon.Go();
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