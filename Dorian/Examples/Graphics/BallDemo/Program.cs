#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, February 20, 2012 6:03:01 PM
// 
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Renderers;

namespace GorgonLibrary.Graphics.Example
{
	/// <summary>
	/// Main class for the application.
	/// </summary>
	static class Program
	{
		private static Random _rnd = new Random();															// Random number generator.
		private static MainForm _form = null;																// Our application form.
		private static GorgonGraphics _graphics = null;														// Our main graphics interface.
		private static Gorgon2D _2D = null;																	// Our 2D interface.
		private static GorgonSwapChain _mainScreen = null;													// The swap chain for our primary display.
		private static GorgonTexture2D _ballTexture = null;													// Texture for the balls.
		private static GorgonSprite _wall = null;															// Background checked wall sprite.
		private static GorgonSprite _ball = null;															// Ball sprite.		
		private static IList<Ball> _ballList = null;														// Our list of balls.
		private static int _ballCount = 0;																	// Number of balls.
		private static float _accumulator = 0;																// Our accumulator for running at a fixed frame rate.
		private static float _maxSimulationFPS = (float)GorgonFrameRate.FpsToMilliseconds(60) / 1000.0f;	// Maximum FPS for our ball simulation.

		/// <summary>
		/// Function to generate the balls.
		/// </summary>
		/// <param name="ballCount">Ball count to add to the total ball count.</param>
		private static void GenerateBalls(int ballCount)
		{
			float halfWidth = _mainScreen.Settings.Width / 2.0f;		// Half of the width.
			float halfHeight = _mainScreen.Settings.Height / 2.0f;		// Half of the width.
			int start = _ballCount;

			_ballCount += ballCount;
			if (_ballCount < 1)
				_ballCount = 1;
			if (_ballCount > 1048576)
				_ballCount = 1048576;

			// Create ball array.
			if (_ballList == null)
				_ballList = new Ball[1048576];

			// Generate balls.
			for (int i = start; i < _ballCount; i++)
			{
				Ball ball = new Ball();
				ball.Position = new Vector2(halfWidth - (_ball.Size.X / 2.0f), halfHeight - (_ball.Size.Y / 2.0f));
				ball.PositionDelta = new Vector2(((float)_rnd.NextDouble() * _mainScreen.Settings.Width) - (halfWidth), ((float)_rnd.NextDouble() * _mainScreen.Settings.Height) - (halfHeight));
				ball.Scale = ((float)_rnd.NextDouble() * 1.5f) + 0.5f;
				ball.ScaleDelta = ((float)_rnd.NextDouble() * 2.0f) - 1.0f;
				ball.Rotation = ((float)_rnd.NextDouble() * 360.0f);
				ball.RotationDelta = ((float)_rnd.NextDouble() * 360.0f) - 180.0f;
				ball.Color = Color.FromArgb(255, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));
				ball.Opacity = ((float)(_rnd.NextDouble())) + 0.4f;
				ball.OpacityDelta = (float)(_rnd.NextDouble()) - 0.5f;
				if (_rnd.Next(_ballCount) > (_ballCount / 2))
					ball.Checkered = false;
				else
					ball.Checkered = true;
				_ballList[i] = ball;
			}
		}

		/// <summary>
		/// Function to perform the transformation of the balls.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		private static void Transform(float frameTime)
		{
			// Transform balls.
			for (int i = 0; i < _ballCount; i++)
			{
				Ball currentBall = _ballList[i];
				Vector2 posDelta = Vector2.Zero;

				currentBall.Position = Vector2.Add(currentBall.Position, Vector2.Multiply(currentBall.PositionDelta, frameTime));
				currentBall.Scale += currentBall.ScaleDelta * frameTime;
				currentBall.Rotation += currentBall.RotationDelta * frameTime;
				currentBall.Opacity += currentBall.OpacityDelta * frameTime;

				if (currentBall.Rotation > 360.0f)
					currentBall.Rotation = currentBall.Rotation - 360.0f;

				if (currentBall.Rotation < 0.0f)
					currentBall.Rotation = currentBall.Rotation + 360.0f;

				// Adjust position.
				if ((currentBall.Position.X > _mainScreen.Settings.Width) || (currentBall.Position.X < 0))
				{
					currentBall.PositionDelta.X = -currentBall.PositionDelta.X;
					currentBall.RotationDelta = -currentBall.RotationDelta;
				}

				if ((currentBall.Position.Y > _mainScreen.Settings.Height) || (currentBall.Position.Y < 0))
				{
					currentBall.PositionDelta.Y = -currentBall.PositionDelta.Y;
					currentBall.RotationDelta = -currentBall.RotationDelta;
				}

				// Adjust scale.
				if ((currentBall.Scale > 1.5f) || (currentBall.Scale < 0.5f))
					currentBall.ScaleDelta = -currentBall.ScaleDelta;

				// Adjust opacity.
				if ((currentBall.Opacity > 1.0f) || (currentBall.Opacity < 0.04f))
				{
					if (currentBall.Opacity > 1.0f)
						currentBall.Opacity = 1.0f;
					if (currentBall.Opacity < 0.4f)
					{
						currentBall.Opacity = 0.4f;
						currentBall.Checkered = !currentBall.Checkered;
					}
					currentBall.OpacityDelta = -currentBall.OpacityDelta;
				}
			}
		}

		/// <summary>
		/// Function for the main idle loop.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <remarks>This is used as the main loop for the application.  All drawing and logic can go in here.</remarks>
		/// <returns>TRUE to keep running, FALSE to exit.</returns>
		private static bool Idle(GorgonFrameRate timing)
		{
			_mainScreen.Clear(Color.Black, 1.0f, 0);

			// Draw background.
			for (int y = 0; y < _mainScreen.Settings.Height; y += (int)_wall.Size.Y)
			{
			    for (int x = 0; x < _mainScreen.Settings.Width; x += (int)_wall.Size.X)
			    {
					_wall.Color = Color.White;
			        _wall.Position = new Vector2(x, y);
			        _wall.Draw();
			    }
			}

			// Update the simulation at our desired frame rate.
			if (timing.FrameDelta < 0.166667f)
				_accumulator += timing.FrameDelta;
			else
				_accumulator += 0.166667f;

			while (_accumulator >= _maxSimulationFPS)
			{
				Transform(_maxSimulationFPS);
				_accumulator -= _maxSimulationFPS;
			}

			// Draw balls.
			for (int i = 0; i < _ballCount; i++)
			{
				_ball.Angle = _ballList[i].Rotation;
				_ball.Position = _ballList[i].Position;
				_ball.Color = _ballList[i].Color;
				_ball.Opacity = _ballList[i].Opacity;
				_ball.Scale = new Vector2(_ballList[i].Scale, _ballList[i].Scale);

				if (_ballList[i].Checkered)
					_ball.TextureOffset = new Vector2(64, 0);
				else
					_ball.TextureOffset = new Vector2(0, 64);
				_ball.Draw();
			}

			for (int i = 0; i < 1000; i++)
			{
				_pt.Position = new Vector2((float)_rnd.NextDouble() * 400, (float)_rnd.NextDouble() * 400);
				_pt.Color = new GorgonColor((float)_rnd.NextDouble(), (float)_rnd.NextDouble(), (float)_rnd.NextDouble(), 1.0f);
				//_pt.Draw();
			}

			//_line.Angle = _ballList[0].Rotation;

			_rect.Position = new Vector2(20, 20);
			_rect.IsFilled = false;
			_rect.Size = new Vector2(100, 100);
			//_rect.Angle = _ballList[0].Rotation;
			//_rect.Draw();
			_rect.Position = new Vector2(140, 20);
			_rect.IsFilled = true;
			//_rect.Angle = 0;
			_rect.Size = new Vector2(150, 150);
			//_rect.Draw();

			_ball.Scale = new Vector2(1);
			_ball.Position = new Vector2(370, 300);
			_ball.Angle = 0.0f;
			_ball.TextureOffset = new Vector2(64, 0);
			_ball.Color = Color.White;
			_ball.Size = new Vector2(128, 128);
			_ball.Draw();
			_ball.Size = new Vector2(64, 64);
			
			_2D.Drawing.FilledRectangle(new RectangleF(0, 0, 640, 480), Color.Blue);
			_2D.Drawing.DrawPoint(new Vector2(320, 240), Color.Yellow);
			_2D.Drawing.DrawLine(new Vector2(10, 10), new Vector2(300, 220), Color.Green);
			_2D.Drawing.FilledEllipse(new RectangleF(40.0f, 40.0f, 400.0f, 400.0f), Color.White, 64, _ballTexture, new RectangleF(64, 0, 64.0f, 64.0f));
			_2D.Drawing.DrawEllipse(new RectangleF(40.0f, 40.0f, 400.0f, 400.0f), Color.Red, 64);
			_2D.Drawing.FilledTriangle(new Vector2(600, 300), new GorgonTriangle.TrianglePoint()
			{
				Position = new Vector2(0, 0),
				Color = Color.Red
			},
			new GorgonTriangle.TrianglePoint()
			{
				Position = new Vector2(50, 50),
				Color = Color.Green
			},
			new GorgonTriangle.TrianglePoint()
			{
				Position = new Vector2(-50, 50),
				Color = Color.Blue
			}, null);

			//_line.Angle = _ballList[0].Rotation;
			//_line.Color = _pt.Color;
			//_line.LineThickness = new Vector2(4, 4);
			//_line.EndPoint = new Vector2(0, 20);
			//_line.Draw();

			//_line.EndPoint = new Vector2(0, 0);
			//_line.Draw();

			//_line.EndPoint = new Vector2(20, 0);
			//_line.Draw();

			//_line.EndPoint = new Vector2(40, 0);
			//_line.Draw();

			//_line.EndPoint = new Vector2(40, 20);
			//_line.Draw();
			
			//_line.EndPoint = new Vector2(40, 40);
			//_line.Draw();

			//_line.EndPoint = new Vector2(20, 40);
			//_line.Draw();

			//_line.EndPoint = new Vector2(0, 40);
			//_line.Draw();

			_wall.Color = Color.Blue;
			_wall.Position = new Vector2(300, 300);
			_wall.Size = new Vector2(100, 100);
			_wall.Draw();
			_wall.Size = new Vector2(63, 63);

			//_rect.Anchor = new Vector2(50, 50);
			//_rect.Angle = _ballList[0].Rotation;

			//_rect.LineThickness = new Vector2(3, 3);
			_rect.SetCornerColor(RectangleCorner.UpperLeft, Color.Red);
			_rect.SetCornerColor(RectangleCorner.UpperRight, Color.Green);
			_rect.SetCornerColor(RectangleCorner.LowerLeft, Color.Purple);
			_rect.Position = new Vector2(30, 30);
			_rect.Size = new Vector2(100, 100);
			_rect.IsFilled = true;
			_rect.Texture = _ballTexture;
			//_rect.Color = Color.White;
			_rect.TextureRegion = new RectangleF(67, 3, 58.0f, 58.0f);
			_rect.Angle += 10.0f * timing.FrameDelta;
			_rect.Draw();
			
			float rot = _ballList[0].Rotation / 360.0f;
			//_ellipse.Angle = _ballList[0].Rotation;
			//for (int i = 0; i < _ellipse.Quality; i++)
			//    _ellipse.SetPointColor(i, new GorgonColor(rot * ((float)i / (float)_ellipse.Quality), 0, 0));
			_ellipse.Angle = _ballList[0].Rotation;
			_ellipse.Draw();

			//_triangle.Angle = _ballList[0].Rotation;
			_triangle.Draw();

			_2D.Render();

			_form.Text = "FPS: " + timing.AverageFPS.ToString("###0.0") + " DT:" + (timing.AverageFrameDelta * 1000).ToString("##0.0") + " msec.  Ball Count:" + _ballCount.ToString();
			
			return true;
		}

		private static GorgonRectangle _rect = null;
		private static GorgonPoint _pt = null;
		private static GorgonLine _line = null;
		private static GorgonEllipse _ellipse = null;
		private static GorgonTriangle _triangle = null;

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
			_form = new MainForm();
			_form.Show();

			_form.KeyDown += new KeyEventHandler(_form_KeyDown);

			// Create the graphics interface.
			_graphics = new GorgonGraphics();

			// Create the primary swap chain.
			_mainScreen = _graphics.Output.CreateSwapChain("MainScreen", new GorgonSwapChainSettings()
						{
							Width = Properties.Settings.Default.ScreenWidth,
							Height = Properties.Settings.Default.ScreenHeight,
							Format = BufferFormat.R8G8B8A8_UIntNormal,
							Window = _form,
							IsWindowed = Properties.Settings.Default.Windowed							
						});

			if (_mainScreen.Settings.IsWindowed)
			{
				// Center the display.
				_form.Location = new Point(_mainScreen.VideoOutput.OutputBounds.Width / 2 - _form.Width / 2, _mainScreen.VideoOutput.OutputBounds.Height / 2 - _form.Height / 2);
			}

			// Load the ball texture.
			_ballTexture = _graphics.Textures.FromFile("BallTexture", @"..\..\..\..\Resources\Images\BallDemo.png", GorgonTexture2DSettings.FromFile);

			// Create the 2D interface.
			_2D = _graphics.Create2DRenderer(_mainScreen);
			_2D.IsLogoVisible = true;

			// Create the wall sprite.
			_wall = _2D.Renderables.CreateSprite("Wall", 63, 63);
			_wall.Texture = _ballTexture;
			_wall.BlendingMode = BlendingMode.None;

			// Create the ball sprite.
			_ball = _2D.Renderables.CreateSprite("Ball", 64, 64);
			_ball.Anchor = new Vector2(32, 32);
			_ball.Texture = _ballTexture;
			_ball.SmoothingMode = SmoothingMode.Smooth;

			_rect = _2D.Renderables.CreateRectangle("Rectum", RectangleF.FromLTRB(20, 20, 84, 84), Color.White, false);
			//_rect.PenSize = new Vector2(1, 8);
			//_rect.Color = Color.Purple;
			//_rect.Texture = _ballTexture;
			//_rect.TextureRegion = new RectangleF(64, 0, 64, 64);
			//_rect.TextureOffset = new Vector2(64, 0);

			_pt = _2D.Renderables.CreatePoint("Pointum", new Vector2(2, 2), Color.Cyan);
			_pt.PointThickness = new Vector2(3, 3);

			_line = _2D.Renderables.CreateLine("Lineum", new Vector2(20, 20), new Vector2(40, 20), Color.Cyan);
//			_line.PenSize = new Vector2(4, 4);
//			_line.Texture = _ballTexture;
			_line.TextureStart = new Vector2(128, 64);
			_line.TextureEnd = new Vector2(64, 0);
			//_line.Angle = 45.0f;
			//_line.Anchor = new Vector2(40, 40);
			//_line.PenSize = new Vector2(16, 16);

			_ellipse = _2D.Renderables.CreateEllipse("Ellipseum", new Vector2(300, 300), new Vector2(62, 62), Color.Cyan, false, 64);
			_ellipse.LineThickness = new Vector2(2.0f, 2.0f);
			_ellipse.Scale = new Vector2(2.5f, 2.5f);
			_ellipse.Texture = _ballTexture;
			//_ellipse.Anchor = new Vector2(31.5f, 31.5f);
			_ellipse.TextureRegion = new RectangleF(67, 3, 58, 58);
			//_ellipse.Size = new Vector2(198, 198);			

			_triangle = _2D.Renderables.CreateTriangle("Triangleum", 
				new GorgonTriangle.TrianglePoint 
				{
					Position = new Vector2(-50, 250),
					Color = Color.Red,
					TextureCoordinate = new Vector2(64, 64)
				},
				new GorgonTriangle.TrianglePoint
				{
					Position = new Vector2(0, 0),
					Color = Color.Cyan,
					TextureCoordinate = new Vector2(96, 0)
				},
				new GorgonTriangle.TrianglePoint
				{
					Position = new Vector2(50, 250),
					Color = Color.Yellow,
					TextureCoordinate = new Vector2(128, 64)
				}
				, true);
			_triangle.Position = new Vector2(150, 450);
			_triangle.LineThickness = new Vector2(4.0f, 4.0f);
			_triangle.Texture = _ballTexture;

			// Generate the ball list.
			GenerateBalls(Properties.Settings.Default.BallCount);
		}

		/// <summary>
		/// Handles the KeyDown event of the _form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private static void _form_KeyDown(object sender, KeyEventArgs e)
		{
			int ballIncrement = 1;
			switch (e.KeyCode)
			{
				case Keys.Up:
					if ((e.Control) && (e.Shift))
						ballIncrement = 1000;
					else
					{
						if (e.Control)
							ballIncrement = 100;
						if (e.Shift)
							ballIncrement = 10;
					}					
					GenerateBalls(ballIncrement);
					break;
				case Keys.Down:
					if ((e.Control) && (e.Shift))
						ballIncrement = 1000;
					else
					{
						if (e.Control)
							ballIncrement = 100;
						if (e.Shift)
							ballIncrement = 10;
					}					
					GenerateBalls(-ballIncrement);
					break;
				case Keys.Enter:
					if (e.Alt)
						_mainScreen.UpdateSettings(!_mainScreen.Settings.IsWindowed);
					break;
				case Keys.A:
					_2D.IsAlphaTestEnabled = !_2D.IsAlphaTestEnabled;
					break;
				case Keys.B:
					_2D.IsBlendingEnabled = !_2D.IsBlendingEnabled;
					break;
				case Keys.M:
					_2D.IsMultisamplingEnabled = !_2D.IsMultisamplingEnabled;
					break;
				case Keys.V:
					if (_2D.Viewport == null)
					{
						_2D.Viewport = new GorgonViewport(10.0f, 10.0f, 800.0f, 600.0f, 0.0f, 1.0f);
						_2D.ProjectionMatrix = Matrix.OrthoOffCenterLH(10.0f, _form.ClientSize.Width, _form.ClientSize.Height, 10.0f, 0.0f, 100.0f);
					}
					else
					{
						_2D.Viewport = null;
						_2D.ProjectionMatrix = null;
					}					
					break;
				case Keys.C:
					if (_2D.ClipRegion == null)
						_2D.ClipRegion = new Rectangle(-10, -10, 640, 480);
					else
						_2D.ClipRegion = null;
					break;
			}
		}

		/// <summary>
		/// Function to perform clean up operations.
		/// </summary>
		private static void CleanUp()
		{
			if (_form != null)
				_form.Dispose();

			if (_graphics != null)
				_graphics.Dispose();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				Initialize();

				Gorgon.Run(_form, Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => UI.GorgonDialogs.ErrorBox(null, ex));
			}
			finally
			{
				CleanUp();
			}
		}
	}
}