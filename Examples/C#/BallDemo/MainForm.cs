#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Sunday, November 26, 2006 1:31:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using SharpUtilities;
using SharpUtilities.Mathematics;
using SharpUtilities.Utility;

namespace BallDemo
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
        private struct Ball
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
        private string _resourcePath = string.Empty;                                // Path to resources.
        private int _ballCount = 1000;                                              // Ball count.
        private Image _ballImage = null;                                            // Main ball demo image.
        private Sprite _ball = null;                                                // Ball sprite.
        private Sprite _back = null;                                                // Background sprite.
        private Ball[] _balls = null;                                               // Balls to bounce.
        private Sprite _window = null;                                              // Window for text.
        private TextSprite _text = null;                                            // Text.
        private int _frameCount = 0;                                                // Frame count.
        private float _avgTime = 0.0f;                                              // Frame time.
        private float _fTime = 0.0f;                                                // Frame time.
		private Vector2D _checkeredBall = Vector2D.Zero;							// Checkered ball offset.
		private Vector2D _bubbleBall = Vector2D.Zero;								// Bubble offset.
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the FormClosing event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.FormClosingEventArgs"/> instance containing the event data.</param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            GorgonLibrary.Gorgon.Terminate();
        }

        /// <summary>
        /// Handles the OnFrameBegin event of the Screen object.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
        private void Screen_OnFrameBegin(object sender, FrameEventArgs e)
        {
            float dt = ((float)e.CurrentTarget.TimingData.FrameDrawTime) / 1000.0f;         // Current frame draw time.            

			// Average out the frame delta for smoother movement.
            _avgTime += dt;
            _frameCount++;

            if (_frameCount > 10)
            {
                _fTime = _avgTime / _frameCount;
                _avgTime = 0;
                _frameCount = 0;                
            }

            // Draw background.
            for (int x = 0; x < _screenWidth; x+= 256)
            {
                for (int y = 0; y < _screenHeight; y+= 256)
                {
                    _back.SetPosition(x, y);
					_back.Draw();        
                }
            }			

            // Draw balls.
            for (int i = 0; i < _ballCount; i++)
            {
                _balls[i].Position += _balls[i].PositionDelta * _fTime;

                if ((_balls[i].Position.X > _screenWidth) || (_balls[i].Position.X < 0))
                {
                    if (_balls[i].Position.X < 0)
                        _balls[i].Position.X = 0;
                    if (_balls[i].Position.X > _screenWidth)
                        _balls[i].Position.X = _screenWidth;
                    _balls[i].PositionDelta.X = -_balls[i].PositionDelta.X;
                }

                if ((_balls[i].Position.Y > _screenHeight) || (_balls[i].Position.Y < 0))
                {
                    if (_balls[i].Position.Y < 0)
                        _balls[i].Position.Y = 0;
                    if (_balls[i].Position.Y > _screenHeight)
                        _balls[i].Position.Y = _screenHeight;
                    _balls[i].PositionDelta.Y = -_balls[i].PositionDelta.Y;
                }

                _balls[i].Scale += _balls[i].ScaleDelta * _fTime;
                if ((_balls[i].Scale > 1.50f) || (_balls[i].Scale < 0.01f))
                {
                    if (_balls[i].Scale > 1.50f)
                        _balls[i].Scale = 1.50f;
                    if (_balls[i].Scale < 0.01f)
                        _balls[i].Scale = 0.01f;
                    _balls[i].ScaleDelta = -_balls[i].ScaleDelta;
                }

				_balls[i].Opacity += _balls[i].OpacityDelta * _fTime;
				if ((_balls[i].Opacity > 255.0f) || (_balls[i].Opacity < 4.0f))
				{
					if (_balls[i].Opacity > 255.0f)
						_balls[i].Opacity = 255.0f;
					if (_balls[i].Opacity < 4.0f)
					{
						_balls[i].Opacity = 4.0f;
						_balls[i].Checkered = !_balls[i].Checkered;
					}
					_balls[i].OpacityDelta = -_balls[i].OpacityDelta;					
				}

                _balls[i].Rotation += _balls[i].RotationDelta * _fTime;

                // Draw the sprite.
                _ball.Rotation = _balls[i].Rotation;
                _ball.UniformScale = _balls[i].Scale;
                _ball.Position = _balls[i].Position;
                _ball.Color = _balls[i].Color;
				_ball.Opacity = (byte)_balls[i].Opacity;
				if (_balls[i].Checkered)
					_ball.ImageOffset = _checkeredBall;
				else
					_ball.ImageOffset = _bubbleBall;
                _ball.Draw();
            }

            // Display FPS and sprite count.
			_text.Text = "FPS: " + e.CurrentTarget.TimingData.AverageFPS.ToString("0.0") + 
			    "\nFrame delta: " + e.CurrentTarget.TimingData.FrameDrawTime.ToString("0.0") +
			    "\nBall count: " + _ballCount;
			_window.Draw();
            _text.Draw();

            // Draw decoration.
            Gorgon.Screen.BeginDrawing();            
            Gorgon.Screen.Rectangle(0, 0, _window.Width, _window.Height, Drawing.Color.FromArgb(221,215,190));
            Gorgon.Screen.EndDrawing();
        }

        /// <summary>
        /// Handles the device reset.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void Gorgon_OnDeviceReset(object sender, EventArgs e)
        {
            // Get the mode data.
            _screenWidth = Gorgon.VideoMode.Width;
            _screenHeight = Gorgon.VideoMode.Height;
            _screenFormat = Gorgon.VideoMode.Format;

            // Re-create ball boundaries.
            GenerateBalls();
        }

        /// <summary>
        /// Function to generate the balls.
        /// </summary>
        private void GenerateBalls()
        {
            Random rnd = new Random();                  // Random number generator.
            float halfWidth = _screenWidth / 2.0f;      // Half of the width.
            float halfHeight = _screenHeight / 2.0f;      // Half of the width.

            // Create ball array.
            _balls = new Ball[_ballCount];

            // Generate balls.
            for (int i = 0; i < _ballCount; i++)
            {
                _balls[i].Position = new Vector2D(0, 0);
                _balls[i].PositionDelta = new Vector2D((float)rnd.NextDouble() * (halfWidth + _ball.Width) - (halfHeight + _ball.Height) + 2.0f, (float)rnd.NextDouble() * (halfWidth + _ball.Width) - (halfHeight + _ball.Height) + 2.0f);
                _balls[i].Scale = ((float)rnd.NextDouble() * 1.5f) - 0.5f;
                _balls[i].ScaleDelta = ((float)rnd.NextDouble() * 2.0f) - 1.0f;
                _balls[i].Rotation = ((float)rnd.NextDouble() * 360.0f);
                _balls[i].RotationDelta = ((float)rnd.NextDouble() * 360.0f) - 180.0f;
                _balls[i].Color = Drawing.Color.FromArgb(255, rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
				_balls[i].Opacity = ((float)(rnd.NextDouble() * 251)) + 4.0f;
				_balls[i].OpacityDelta = (float)(rnd.NextDouble() * 256) - 128.0f;
				if (rnd.Next(_ballCount) > (_ballCount / 2))
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
					Gorgon.StateManager.Dither = true;

                // Get the ball image.
                _ballImage = Gorgon.ImageManager.FromFile(_resourcePath + @"\BallDemo.png");

                // Create background sprite.
				_back = new Sprite("Background", _ballImage, new Vector2D(Gorgon.Screen.Width, Gorgon.Screen.Height));

                // Create ball sprite.
				_ball = new Sprite("Ball", _ballImage, new Vector2D(259.0f, 0.0f), new Vector2D(128.0f, 128.0f), new Vector2D(64.0f, 64.0f));
				_checkeredBall = new Vector2D(259.0f, 0.0f);
				_bubbleBall = new Vector2D(259.0f, 128.0f);

                // Create window.
				_window = new Sprite("Window", new Vector2D(120.0f, 50.0f));
                _window.Color = Drawing.Color.Black;
                _window.Opacity = 127;

                // Text for window.
				_text = new TextSprite("WindowText", string.Empty, new Vector2D(2.0f, 0), Drawing.Color.White);
				_text.ClippingViewport = new Viewport(0, 0, (int)_window.Width, (int)_window.Height);
                _text.Alignment = Alignment.Center;

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
        /// Handles the Load event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Get settings.
                _screenWidth = Properties.Settings.Default.ScreenWidth;
                _screenHeight = Properties.Settings.Default.ScreenHeight;
                _screenFormat = (BackBufferFormats)Enum.Parse(typeof(BackBufferFormats), Properties.Settings.Default.ScreenFormat, true);
                _isWindowed = Properties.Settings.Default.Windowed;
                _resourcePath = Properties.Settings.Default.ResourcePath + @"\BallDemo";
				_ballCount = Properties.Settings.Default.BallCount;
				if (_ballCount < 10)
					_ballCount = 10;
				if (_ballCount > 65535)
					_ballCount = 65535;

                // Initialize with background render.
                Gorgon.Initialize(true);

                // Display logo.
                Gorgon.LogoVisible = true;

				// Set the video mode to match the form client area.
				Gorgon.SetMode(this, _screenWidth, _screenHeight, _screenFormat, _isWindowed);

                // Set the frame handler.
                Gorgon.Screen.OnFrameBegin += new GorgonLibrary.Graphics.FrameEventHandler(Screen_OnFrameBegin);
                Gorgon.OnDeviceReset += new EventHandler(Gorgon_OnDeviceReset);

                // Initialize.
                if (!Initialize())
                {
                    Close();
                    return;
                }

                // Run the application.
                Gorgon.Go();
            }
            catch (SharpException sEx)
            {
                UI.ErrorBox(this, "Error in application.\nException:\n" + sEx.Message, sEx.ErrorLog);
                Close();
            }
            catch (Exception ex)
            {
                UI.ErrorBox(this, ex);
                Close();
            }
        }

        /// <summary>
        /// Handles the KeyDown event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                if ((e.Modifiers & Keys.Shift) != 0)
                    _ballCount += 100;
                else
                    _ballCount++;
                if (_ballCount > 65535)
                    _ballCount = 65535;
                GenerateBalls();
            }

            if (e.KeyCode == Keys.Down)
            {
                if ((e.Modifiers & Keys.Shift) != 0)
                    _ballCount -= 100;
                else
                    _ballCount--;
                if (_ballCount < 10)
                    _ballCount = 10;
                GenerateBalls();
            }

            if (e.KeyCode == Keys.Escape)            
                Close();            
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