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
        #region Value types.
        /// <summary>
        /// Value type for a ball.
        /// </summary>
        private class Ball
        {
            #region Variables.
            /// <summary>Position of the ball.</summary>
            public Vector2D Position;
            /// <summary>Delta for the position of the ball.</summary>
            public Vector2D PositionDelta;
            /// <summary>Scale of the ball.</summary>
            public float Scale;
            /// <summary>Delta for the scale of the ball</summary>
            public float ScaleDelta;
            /// <summary>Rotation of the ball.</summary>
            public float Rotation;
            /// <summary>Rotation delta for the ball.</summary>
            public float RotationDelta;
            /// <summary>Color of the ball.</summary>
            public Drawing.Color Color;
			/// <summary>Opacity of the ball.</summary>
			public float Opacity;
			/// <summary>Opacity of the ball.</summary>
			public float OpacityDelta;
			/// <summary>TRUE to use the checker ball, FALSE to use the bubble.</summary>
			public bool Checkered;
			#endregion
        }
        #endregion

        #region Variables.
        private int _screenWidth = 640;                                             // Screen width.
        private int _screenHeight = 480;                                            // Screen height.
        private BackBufferFormats _screenFormat = BackBufferFormats.BufferRGB888;   // Screen backbuffer format.
        private bool _isWindowed = true;                                            // Flag to indicate whether to to use windowed mode or fullscreen mode.
        private int _ballCount = 1000;                                              // Ball count.
        private Image _ballImage = null;                                            // Main ball demo image.
        private Sprite _ball = null;                                                // Ball sprite.
        private Sprite _back = null;                                                // Background sprite.
        private Ball[] _balls = null;                                               // Balls to bounce.
        private Sprite _window = null;                                              // Window for text.
        private TextSprite _text = null;                                            // Text.
		private Vector2D _checkeredBall = Vector2D.Zero;							// Checkered ball offset.
		private Vector2D _bubbleBall = Vector2D.Zero;								// Bubble offset.
		private Random _rnd = new Random();											// Random number generator.
		private RenderImage _counterImage = null;									// Frame counter image.
		private StringBuilder _fpsString = null;									// FPS string.
		private Font _font = null;													// Text font.
		private bool _showHelp = true;												// Flag to show help.
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the OnFrameBegin event of the Screen object.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Screen_OnFrameBegin(object sender, FrameEventArgs e)
        {
			Ball currentBall = null;			// Current ball.

			// Draw background.
			for (int y = 0; y < _screenHeight; y += 64)
			{
				for (int x = 0; x < _screenWidth; x += 64)
				{
					_back.SetPosition(x, y);
					_back.Draw();
				}
			}

			// Draw balls.
			for (int i = 0; i < _ballCount; i++)
			{
				currentBall = _balls[i];
                currentBall.Position = Vector2D.Add(currentBall.Position, Vector2D.Multiply(currentBall.PositionDelta, e.FrameDeltaTime));
				currentBall.Scale += currentBall.ScaleDelta * e.FrameDeltaTime;
				currentBall.Rotation += currentBall.RotationDelta * e.FrameDeltaTime;
				currentBall.Opacity += currentBall.OpacityDelta * e.FrameDeltaTime;

				// Adjust position.
				if ((currentBall.Position.X > _screenWidth) || (currentBall.Position.X < 0))
					currentBall.PositionDelta.X = -currentBall.PositionDelta.X;

				if ((currentBall.Position.Y > _screenHeight) || (currentBall.Position.Y < 0))
					currentBall.PositionDelta.Y = -currentBall.PositionDelta.Y;

				// Adjust scale.
				if ((currentBall.Scale > 1.5f) || (currentBall.Scale < 0.5f))
					currentBall.ScaleDelta = -currentBall.ScaleDelta;

				// Adjust opacity.
				if ((currentBall.Opacity > 255.0f) || (currentBall.Opacity < 4.0f))
				{
					if (currentBall.Opacity > 255.0f)
						currentBall.Opacity = 255.0f;
					if (currentBall.Opacity < 4.0f)
					{
						currentBall.Opacity = 4.0f;
						currentBall.Checkered = !currentBall.Checkered;
					}
					currentBall.OpacityDelta = -currentBall.OpacityDelta;
				}

				// Update and draw the sprite.
				_ball.Rotation = currentBall.Rotation;
				_ball.UniformScale = currentBall.Scale;
				_ball.Position = currentBall.Position;
				_ball.Color = currentBall.Color;
				_ball.Opacity = (byte)currentBall.Opacity;

				// Determine which image to display.
				if (currentBall.Checkered)
					_ball.ImageOffset = _checkeredBall;
				else
					_ball.ImageOffset = _bubbleBall;
				_ball.Draw();
			}

            // Display FPS and sprite count.
			_window.Draw();
			_fpsString.Length = 0;
			_fpsString.AppendFormat("FPS: {0:0.0}\nFrame delta: {1:0.0}ms\nBall count: {2}", e.TimingData.AverageFps, e.TimingData.FrameDrawTime, _ballCount);
			_text.Color = Drawing.Color.White;
			_text.Shadowed = false;
			_text.Position = new Vector2D(0, 0);
			_text.Text = _fpsString.ToString();
			_text.Draw();

			if (_showHelp)
			{
				_text.Position = new Vector2D(0, _text.Height + 8.0f);
				_text.Shadowed = true;
				_text.Color = Drawing.Color.Yellow;
				_fpsString.Length = 0;
				_fpsString.Append("Help:\nF1 - Show or hide this help message.\n\u2191 (up arrow) - Increase the ball count by 1 ball.\n");
				_fpsString.Append("\u2193 (down arrow) - Decrease the ball count by 1 ball.\nShift + \u2191 (up arrow) - Increase the ball count by 100 balls.\n");
				_fpsString.Append("Shift + \u2193 (down arrow) - Decrease the ball count by 100 balls.\nCtrl + \u2191 (up arrow) - Increase the ball count by 1000 balls.\n");
				_fpsString.Append("Ctrl + \u2193 (down arrow) - Decrease the ball count by 1000 balls.\n");
				if (Gorgon.Screen.Windowed)
					_fpsString.Append("Alt + Enter - Switch to fullscreen.\n");
				else
					_fpsString.Append("Alt + Enter - Switch to windowed.\n");
				_fpsString.Append("ESC - Exit.");
				_text.Text = _fpsString.ToString();
				_text.Draw();
			}
		}

        /// <summary>
        /// Handles the device reset.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Gorgon_OnDeviceReset(object sender, EventArgs e)
        {
            // Get the mode data.
            _screenWidth = Gorgon.CurrentRenderTarget.Width;
			_screenHeight = Gorgon.CurrentRenderTarget.Height;
			_screenFormat = Gorgon.CurrentVideoMode.Format;

			// Draw decoration.
			Gorgon.CurrentRenderTarget = _counterImage;
			_counterImage.BeginDrawing();
			_counterImage.FilledRectangle(0, 0, _counterImage.Width, _counterImage.Height, Drawing.Color.FromArgb(128, 0, 0, 0));
			_counterImage.Rectangle(0, 0, _counterImage.Width, _counterImage.Height, Drawing.Color.FromArgb(221, 215, 190));
			_counterImage.EndDrawing();
			Gorgon.CurrentRenderTarget = null;
		}

        /// <summary>
        /// Function to generate the balls.
        /// </summary>
        private void GenerateBalls()
        {
            float halfWidth = _screenWidth / 2.0f;      // Half of the width.
            float halfHeight = _screenHeight / 2.0f;    // Half of the width.
			int ballArray = _ballCount * 5;				// Length of ball array.

            // Create ball array.
			if ((_balls == null) || (ballArray > _balls.Length))
				_balls = new Ball[ballArray];

            // Generate balls.
            for (int i = 0; i < ballArray; i++)
            {
				_balls[i] = new Ball();
				_balls[i].Position = new Vector2D(halfWidth - (_ball.Width / 2.0f), halfHeight - (_ball.Height / 2.0f));
				_balls[i].PositionDelta = new Vector2D(((float)_rnd.NextDouble() * _screenWidth) - (halfWidth), ((float)_rnd.NextDouble() * _screenHeight) - (halfHeight));
				_balls[i].Scale = ((float)_rnd.NextDouble() * 1.5f) + 0.5f;
				_balls[i].ScaleDelta = ((float)_rnd.NextDouble() * 2.0f) - 1.0f;
                _balls[i].Rotation = ((float)_rnd.NextDouble() * 360.0f);
                _balls[i].RotationDelta = ((float)_rnd.NextDouble() * 360.0f) - 180.0f;
                _balls[i].Color = Drawing.Color.FromArgb(255, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));
				_balls[i].Opacity = ((float)(_rnd.NextDouble() * 251)) + 4.0f;
				_balls[i].OpacityDelta = (float)(_rnd.NextDouble() * 256) - 128.0f;
				if (_rnd.Next(_ballCount) > (_ballCount / 2))
					_balls[i].Checkered = false;
				else
					_balls[i].Checkered = true;
            }
        }

        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        /// <returns>TRUE if successful, FALSE if not.</returns>
        private bool Initialize()
        {
            try
            {
                // If we're using less than 24 bit color, then enable dithering.
				if ((_screenFormat != BackBufferFormats.BufferRGB888) && (_screenFormat != BackBufferFormats.BufferRGB101010A2))
					Gorgon.GlobalStateSettings.GlobalDither = true;

				// Create font.
				_font = new Font("Arial9pt", "Arial", 9.0f, true, true);
				_font.CharacterList += "\u2191\u2193";

                // Get the ball image.
                _ballImage = Image.FromFile("BallDemo.png");

                // Create background sprite.
				_back = new Sprite("Background", _ballImage, new Vector2D(64, 64));
				_back.BlendingMode = BlendingModes.None;

                // Create ball sprite.
				_ball = new Sprite("Ball", _ballImage, new Vector2D(64.0f, 0.0f), new Vector2D(64.0f, 64.0f), new Vector2D(32.0f, 32.0f));
				_ball.Smoothing = Smoothing.Smooth;
				_checkeredBall = new Vector2D(64.0f, 0.0f);
				_bubbleBall = new Vector2D(0.0f, 64.0f);

				// Create frame stats window image.
				_counterImage = new RenderImage("StatsImage", 140, 50, ImageBufferFormats.BufferRGB888A8);

				// Draw decoration.
				Gorgon.CurrentRenderTarget = _counterImage;
				_counterImage.BeginDrawing();
				_counterImage.FilledRectangle(0, 0, _counterImage.Width, _counterImage.Height, Drawing.Color.FromArgb(128, 0, 0, 0));
				_counterImage.Rectangle(0, 0, _counterImage.Width, _counterImage.Height, Drawing.Color.FromArgb(221, 215, 190));
				_counterImage.EndDrawing();
				Gorgon.CurrentRenderTarget = null;

				// Create window.
				_window = new Sprite("Window", _counterImage);

				// Text for window.
				_text = new TextSprite("WindowText", string.Empty, _font, Vector2D.Zero, Drawing.Color.White);

				// Create the string.
				_fpsString = new StringBuilder(256);

				Gorgon.Screen.BackgroundColor = Drawing.Color.AntiqueWhite;

                // Generate the balls.
                GenerateBalls();
                return true;
            }
            catch (GorgonException gEx)
            {
                UI.ErrorBox(this, "Unable to initialize the program.\nException:" + gEx.Message, gEx);
                return false;
            }
        }

		/// <summary>
		/// Handles the FormClosing event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			Gorgon.Terminate();
		}
		
		/// <summary>
        /// Handles the Load event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Get settings.
				_screenWidth = Properties.Settings.Default.ScreenWidth;
				_screenHeight = Properties.Settings.Default.ScreenHeight;
				_screenFormat = (BackBufferFormats)Enum.Parse(typeof(BackBufferFormats), Properties.Settings.Default.ScreenFormat, true);
				_isWindowed = Properties.Settings.Default.Windowed;
				_ballCount = Properties.Settings.Default.BallCount;
				if (_ballCount < 1)
					_ballCount = 1;
				if (_ballCount > 10000000)
					_ballCount = 10000000;

				// Initialize with background render.
				Gorgon.Initialize(true, false);

				// Display logo.
				Gorgon.LogoVisible = true;

				// Set the video mode to match the form client area.
				Gorgon.SetMode(this, _screenWidth, _screenHeight, _screenFormat, _isWindowed);

				// Set the frame handler.
				Gorgon.Idle += new FrameEventHandler(Screen_OnFrameBegin);
				Gorgon.DeviceReset += new EventHandler(Gorgon_OnDeviceReset);

				// Initialize.
				if (!Initialize())
				{
					Close();
					return;
				}

				// Run the application.
				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Error in application.", ex);
				Close();
			}
		}			

        /// <summary>
        /// Handles the KeyDown event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.F1)
				_showHelp = !_showHelp;

			if (e.KeyCode == Keys.Up)
			{
				if ((e.Modifiers & Keys.Shift) != 0)
					_ballCount += 100;
				else
				{
					if ((e.Modifiers & Keys.Control) != 0)
						_ballCount += 1000;
					else
						_ballCount++;
				}
				if (_ballCount > 10000000)
					_ballCount = 10000000;

				if (_ballCount > _balls.Length)
					GenerateBalls();
			}

			if (e.KeyCode == Keys.Down)
			{
				if ((e.Modifiers & Keys.Shift) != 0)
					_ballCount -= 100;
				else
				{
					if ((e.Modifiers & Keys.Control) != 0)
						_ballCount -= 1000;
					else
						_ballCount--;
				}

				if (_ballCount < 1)
					_ballCount = 1;
			}

			if (e.KeyCode == Keys.Escape)
				Close();

			if ((e.KeyCode == Keys.Enter) && (e.Alt))
				Gorgon.Screen.Windowed = !Gorgon.Screen.Windowed;
		}
        #endregion

        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm()
        {
			// Optimization to keep windows from trying to paint the window and slowing us down.
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			InitializeComponent();
        }
        #endregion
    }
}