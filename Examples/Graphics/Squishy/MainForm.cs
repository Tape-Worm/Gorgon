#region MIT.
// 
// Examples.
// Copyright (C) 2009 Michael Winsor
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
// Created: Thursday, June 11, 2009 10:07:21 PM
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
		#region Variables.
		private TextSprite _message = null;			// Message to display.
		private Font _font = null;					// Text sprite font.
		private Image[] _ballFrames = null;			// Frames for our animation.
		private Sprite _ballSprite = null;			// Ball sprite.
		private Animation _rollAnimation = null;	// Rolling animation.
		private Animation _squishAnimation = null;	// Squishy animation.
		private Animation _current = null;			// Currently playing animation.
		private bool _displayMessage = false;		// Flag to display the message.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyDown event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				Close();
			if (e.KeyCode == Keys.S)
				Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;
			if ((e.KeyCode == Keys.R) && (_displayMessage))
			{
				_rollAnimation.Reset();
				_squishAnimation.Reset();
				_rollAnimation.AnimationState = AnimationState.Playing;
				_current = _rollAnimation;
				_displayMessage = false;
			}
		}

		/// <summary>
		/// Handles the OnFrameBegin event of the Screen control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.FrameEventArgs"/> instance containing the event data.</param>
		private void Screen_OnFrameBegin(object sender, FrameEventArgs e)
		{
			// Clear the screen.
			Gorgon.Screen.Clear();

			_current.Advance(e.FrameDeltaTime * 1000.0f);

			if (_displayMessage)
				_message.Draw();
		
			_ballSprite.Draw();
		}

		/// <summary>
		/// Handles the DeviceReset event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
			// Reset the positioning keys so that we're where we should be when the window resizes.
			KeyVector2D position = _rollAnimation.Tracks["Position"].GetKeyAtIndex(0) as KeyVector2D;
			position.Value = new Vector2D(-128.0f, Gorgon.Screen.Height - 64.0f);
			position = _rollAnimation.Tracks["Position"].GetKeyAtIndex(1) as KeyVector2D;
			position.Value = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height - 64.0f);

			// If we're playing the squishy animation, then we've stopped moving, just update the sprite position.
			if (_squishAnimation.AnimationState == AnimationState.Playing)
				_ballSprite.Position = new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height - 64.0f);
		}

		/// <summary>
		/// Handles the FormClosing event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Perform clean up.
			Gorgon.Terminate();
		}

		/// <summary>
		/// Handles the AnimationStopped event of the rollAnimation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void rollAnimation_AnimationStopped(object sender, EventArgs e)
		{
			_current = _squishAnimation;
			_squishAnimation.AnimationState = AnimationState.Playing;
		}

		/// <summary>
		/// Handles the AnimationStopped event of the squishAnimation control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void squishAnimation_AnimationStopped(object sender, EventArgs e)
		{
			_displayMessage = true;
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private void Initialize()
		{
			_font = new Font("Arial", "Arial", 9.0f, true, true);
			_message = new TextSprite("Message", "Press R to restart the animation.", _font, Drawing.Color.Black);

			_ballFrames = new Image[2];
			_ballFrames[0] = Image.FromFile(@"..\..\..\..\Resources\Squishy\0_BallSheet.png");
			_ballFrames[1] = Image.FromFile(@"..\..\..\..\Resources\Squishy\1_BallSheet.png");

			// Create our new sprite.
			_ballSprite = new Sprite("BallSprite", _ballFrames[0], new Vector2D(128, 128));
			_ballSprite.Axis = new Vector2D(64, 64);

			_rollAnimation = new Animation("RollingAnimation", 2000.0f);
			_ballSprite.Animations.Add(_rollAnimation);
			_rollAnimation.Tracks["Rotation"].AddKey(new KeyFloat(0.0f, 0.0f));
			_rollAnimation.Tracks["Rotation"].AddKey(new KeyFloat(2000.0f, 360.0f));
			_rollAnimation.Tracks["Position"].AddKey(new KeyVector2D(0.0f, new Vector2D(-128.0f, Gorgon.Screen.Height - 64.0f)));
			_rollAnimation.Tracks["Position"].AddKey(new KeyVector2D(2000.0f, new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height - 64.0f)));
			_rollAnimation.AnimationState = AnimationState.Playing;
						
			_rollAnimation.AnimationStopped += new EventHandler(rollAnimation_AnimationStopped);
			_current = _rollAnimation;

			_squishAnimation = new Animation("SquishAnimation", 1000.0f);
			_ballSprite.Animations.Add(_squishAnimation);

			// Calculate the offset in time for an individual key frame.
			float keyFrameTime = 1000.0f / 21.0f;

			// Add each image keyframe.
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime, _ballFrames[0], new Drawing.RectangleF(0, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 2.0f, _ballFrames[0], new Drawing.RectangleF(128, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 3.0f, _ballFrames[0], new Drawing.RectangleF(256, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 4.0f, _ballFrames[0], new Drawing.RectangleF(384, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 5.0f, _ballFrames[0], new Drawing.RectangleF(0, 128, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 6.0f, _ballFrames[0], new Drawing.RectangleF(0, 256, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 7.0f, _ballFrames[0], new Drawing.RectangleF(0, 384, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 8.0f, _ballFrames[0], new Drawing.RectangleF(128, 128, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 9.0f, _ballFrames[0], new Drawing.RectangleF(256, 128, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 10.0f, _ballFrames[0], new Drawing.RectangleF(384, 128, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 11.0f, _ballFrames[0], new Drawing.RectangleF(128, 256, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 12.0f, _ballFrames[0], new Drawing.RectangleF(128, 384, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 13.0f, _ballFrames[0], new Drawing.RectangleF(256, 256, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 14.0f, _ballFrames[0], new Drawing.RectangleF(384, 256, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 15.0f, _ballFrames[0], new Drawing.RectangleF(256, 384, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 16.0f, _ballFrames[0], new Drawing.RectangleF(384, 384, 128, 128)));
			// Here we will flip to our second set of frames.
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 17.0f, _ballFrames[1], new Drawing.RectangleF(0, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 18.0f, _ballFrames[1], new Drawing.RectangleF(128, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 19.0f, _ballFrames[1], new Drawing.RectangleF(256, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 20.0f, _ballFrames[1], new Drawing.RectangleF(384, 0, 128, 128)));
			_squishAnimation.Tracks["Image"].AddKey(new KeyImage(keyFrameTime * 21.0f, _ballFrames[1], new Drawing.RectangleF(0, 128, 128, 128)));

			_squishAnimation.AnimationStopped += new EventHandler(squishAnimation_AnimationStopped);
		}

		/// <summary>
		/// Handles the Load event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void MainForm_Load(object sender, EventArgs e)
		{
			try
			{
				ClientSize = new Drawing.Size(640, 480);

				// Initialize the library.
				Gorgon.Initialize();

				// Display the logo and frame stats.
				Gorgon.LogoVisible = true;
				Gorgon.FrameStatsVisible = false;

				// Set the video mode to match the form client area.
				Gorgon.SetMode(this);

				// Initialize our application.
				Initialize();

				// Assign rendering event handler.
				Gorgon.Idle += new FrameEventHandler(Screen_OnFrameBegin);

				Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);

				// Set the clear color to something ugly.
				Gorgon.Screen.BackgroundColor = Drawing.Color.White;
			
				// Begin execution.
				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "An unhandled error occured during execution, the program will now close.", ex.Message + "\n\n" + ex.StackTrace);
				Application.Exit();
			}
		}
		#endregion

		#region Constructor/Destructor.
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