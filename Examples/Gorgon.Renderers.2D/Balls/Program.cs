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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Example.Properties;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.UI;
using SlimMath;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// Main class for the application.
	/// </summary>
	static class Program
	{
		private const float MaxSimulationFPS = 1 / 60.0f;														// Maximum FPS for our ball simulation.
		private const float MinSimulationFPS = 1 / 30.0f;														// Minimum FPS for our ball simulation.

		private static MainForm _form;																            // Our application form.
		private static GorgonGraphics _graphics;														        // Our main graphics interface.
		private static Gorgon2D _2D;																	        // Our 2D interface.
		private static GorgonSwapChain _mainScreen;													            // The swap chain for our primary display.
		private static GorgonTexture2D _ballTexture;													        // Texture for the balls.
		private static GorgonSprite _wall;															            // Background checked wall sprite.
		private static GorgonSprite _ball;															            // Ball sprite.		
		private static IList<Ball> _ballList;														            // Our list of balls.
		private static int _ballCount;																	        // Number of balls.
		private static float _accumulator;																        // Our accumulator for running at a fixed frame rate.
		private static GorgonFont _ballFont;															        // Font to display our FPS, etc...
		private static GorgonRenderTarget2D _ballTarget;											            // Render target for the balls.
		private static GorgonRenderTarget2D _statsTarget;											            // Render target for statistics.
		private static StringBuilder _fpsText;														            // Frames per second text.
		private static StringBuilder _helpText;														            // Help text.
		private static GorgonText _helpTextSprite;																// Text sprite for our help text.
		private static bool _showHelp = true;																    // Flag to indicate that the help text should be shown.
		private static bool _paused;																			// Flag to indicate that the animation is paused.

		/// <summary>
		/// Property to return the path to the resources for the example.
		/// </summary>
		/// <param name="resourceItem">The directory or file to use as a resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (Nothing in VB.Net) or empty.</exception>
		public static string GetResourcePath(string resourceItem)
		{
			string path = Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
				throw new ArgumentException(Resources.GOR_RESOURCE_NOT_SPECIFIED, "resourceItem");
			}

			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
			{
				path += Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
			}

			path = path.RemoveIllegalPathChars();

			// If this is a directory, then sanitize it as such.
			if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
			{
				path += resourceItem.RemoveIllegalPathChars();
			}
			else
			{
				// Otherwise, sanitize the file name.
				path += resourceItem.RemoveIllegalFilenameChars();
			}

			// Ensure that 
			return Path.GetFullPath(path);
		}

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
		    {
		        _ballCount = 1;
		    }
		    if (_ballCount > 1048576)
		    {
		        _ballCount = 1048576;
		    }

		    // Create ball array.
			if (_ballList == null)
			{
				_ballList = new Ball[1048576];
			}

			// Generate balls.
			for (int i = start; i < _ballCount; i++)
			{
				var ball = new Ball
				    {
				        Position = new Vector2(halfWidth - (_ball.Size.X/2.0f), halfHeight - (_ball.Size.Y/2.0f)),
				        PositionDelta = new Vector2((GorgonRandom.RandomSingle()*_mainScreen.Settings.Width) - (halfWidth),
				                                    (GorgonRandom.RandomSingle()*_mainScreen.Settings.Height) - (halfHeight)),
				        Scale = 1.0f,
				        ScaleDelta = (GorgonRandom.RandomSingle()*2.0f) - 1.0f,
				        Rotation = 0,
				        RotationDelta = (GorgonRandom.RandomSingle()*360.0f) - 180.0f,
				        Color = Color.White,
				        Opacity = 1.0f,
				        OpacityDelta = GorgonRandom.RandomSingle() - 0.5f,
				        Checkered = true
				    };

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

			    currentBall.Position = Vector2.Add(currentBall.Position, Vector2.Multiply(currentBall.PositionDelta, frameTime));
				currentBall.Scale += currentBall.ScaleDelta * frameTime;
				currentBall.Rotation += currentBall.RotationDelta * frameTime;
				currentBall.Opacity += currentBall.OpacityDelta * frameTime;

				if (currentBall.Rotation > 360.0f)
				{
					currentBall.Rotation = currentBall.Rotation - 360.0f;
				}

				if (currentBall.Rotation < 0.0f)
				{
					currentBall.Rotation = currentBall.Rotation + 360.0f;
				}

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
				if ((currentBall.Scale > 2.0f) || (currentBall.Scale < 0.5f))
				{
					currentBall.ScaleDelta = -currentBall.ScaleDelta;

					if (currentBall.Scale < 0.5f)
					{
						currentBall.OpacityDelta = GorgonRandom.RandomSingle() * 0.5f * (currentBall.OpacityDelta / currentBall.OpacityDelta.Abs());
					}
				}

				// Adjust opacity.
			    if ((currentBall.Opacity <= 1.0f)
			        && (currentBall.Opacity >= 0.0f))
			    {
			        continue;
			    }

				if (currentBall.Opacity > 1.0f)
				{
					currentBall.Opacity = 1.0f;
					currentBall.OpacityDelta = -currentBall.OpacityDelta;
					continue;
				}

				currentBall.Opacity = 0.0f;
				currentBall.Checkered = !currentBall.Checkered;
				currentBall.Color = Color.FromArgb(255, GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255),
					GorgonRandom.RandomInt32(0, 255));
				currentBall.OpacityDelta = GorgonRandom.RandomSingle() * 0.5f;
			}
		}

		/// <summary>
		/// Function to draw the background for the balls.
		/// </summary>
		private static void DrawBackground()
		{
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
		}

		/// <summary>
		/// Function to draw the balls without any effects.
		/// </summary>
		private static void DrawNoBlur()
		{
			// Draw balls.
			for (int i = 0; i < _ballCount; i++)
			{
				_ball.Angle = _ballList[i].Rotation;
				_ball.Position = _ballList[i].Position;
				_ball.Color = _ballList[i].Color;
				_ball.Opacity = _ballList[i].Opacity;
				_ball.Scale = new Vector2(_ballList[i].Scale, _ballList[i].Scale);

				_ball.TextureOffset = _ballList[i].Checkered ? new Vector2(0.5f, 0) : new Vector2(0, 0.5f);

				_ball.Draw();
			}
		}

		/// <summary>
		/// Function to perform rendering with the blur filter.
		/// </summary>
		private static void DrawBlurred()
		{
			_2D.Target = _ballTarget;
			_2D.Clear(GorgonColor.Transparent);

			DrawNoBlur();

			_2D.Target = null;

			if (_2D.Effects.GaussianBlur.RenderScene == null)
			{
				// Draw using the blur effect.
				_2D.Effects.GaussianBlur.RenderScene = pass =>
				{
					_2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
					_2D.Drawing.Blit(_ballTarget, new RectangleF(Vector2.Zero, _2D.Effects.GaussianBlur.BlurRenderTargetsSize));
				};
			}

			_2D.Effects.GaussianBlur.Render();

			// Copy the blur output to our main target.
            _2D.Drawing.SmoothingMode = SmoothingMode.None;
			_2D.Drawing.Blit(_2D.Effects.GaussianBlur.Output, new RectangleF(Vector2.Zero, new SizeF(_mainScreen.Settings.Width, _mainScreen.Settings.Height)));
		}

		/// <summary>
		/// Function to draw the overlay elements, like text.
		/// </summary>
		private static void DrawOverlay()
		{
			if (_showHelp)
			{
				_helpTextSprite.Draw();
			}

			_fpsText.Length = 0;
			_fpsText.AppendFormat(Resources.FPSLine, GorgonTiming.AverageFPS, GorgonTiming.AverageDelta * 1000.0f, _ballCount);

			_2D.Drawing.Blit(_statsTarget, Vector2.Zero);
			_2D.Drawing.DrawString(_ballFont, _fpsText.ToString(), new Vector2(3.0f, 0), Color.White);

			// Draw the draw call counter.
			_fpsText.Length = 0;
			_fpsText.AppendFormat(Resources.DrawCallsLine, GorgonRenderStatistics.DrawCallCount);
			_2D.Drawing.DrawString(_ballFont, _fpsText.ToString(), new Vector2(3.0f, (_ballFont.FontHeight * 3) + 2), Color.White);
		}

		/// <summary>
		/// Function for the main idle loop.
		/// </summary>
		/// <remarks>This is used as the main loop for the application.  All drawing and logic can go in here.</remarks>
		/// <returns><c>true</c> to keep running, <c>false</c> to exit.</returns>
		private static bool Idle()
		{
			if (!_paused)
			{
				// Update the simulation at our desired frame rate.
				if (GorgonTiming.Delta < MinSimulationFPS)
				{
					_accumulator += GorgonTiming.Delta;
				}
				else
				{
					_accumulator += MinSimulationFPS;
				}

				while (_accumulator >= MaxSimulationFPS)
				{
					Transform(MaxSimulationFPS);
					_accumulator -= MaxSimulationFPS;
				}
			}

			DrawBackground();

			if (_2D.Effects.GaussianBlur.BlurAmount >= 10.0f)
			{
				DrawNoBlur();
			}
			else
			{
				DrawBlurred();
			}

			DrawOverlay();

			_2D.Render();
			GorgonRenderStatistics.EndFrame();

			return true;
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
			_form = new MainForm();
			_form.Show();

			// Create the graphics interface.
			_graphics = new GorgonGraphics();

			// Create the primary swap chain.
			_mainScreen = _graphics.Output.CreateSwapChain("MainScreen", new GorgonSwapChainSettings
			    {
					Width = Settings.Default.ScreenWidth,
					Height = Settings.Default.ScreenHeight,
					Format = BufferFormat.R8G8B8A8_UIntNormal,
					Window = _form,
					IsWindowed = Settings.Default.Windowed
				});

			// Center the display.
			if (_mainScreen.Settings.IsWindowed)
			{
				_form.Location =
					new Point(
						_mainScreen.VideoOutput.OutputBounds.Width / 2 - _form.Width / 2 + _mainScreen.VideoOutput.OutputBounds.Left,
						_mainScreen.VideoOutput.OutputBounds.Height / 2 - _form.Height / 2 + _mainScreen.VideoOutput.OutputBounds.Top);
			}

			// Load the ball texture.
			_ballTexture = _graphics.Textures.FromFile<GorgonTexture2D>("BallTexture", GetResourcePath(@"Textures\Balls\BallsTexture.dds"), new GorgonCodecDDS());

			// Create the 2D interface.
			_2D = _graphics.Output.Create2DRenderer(_mainScreen);
			_2D.IsLogoVisible = true;

			// Set our drawing code to use modulated blending.
			_2D.Drawing.BlendingMode = BlendingMode.Modulate;
			_2D.Drawing.Blending.DestinationAlphaBlend = BlendType.InverseSourceAlpha;

			// Create the wall sprite.
			_wall = _2D.Renderables.CreateSprite("Wall", new Vector2(63, 63), _ballTexture);
			_wall.BlendingMode = BlendingMode.None;

			// Create the ball sprite.
			_ball = _2D.Renderables.CreateSprite("Ball", new Vector2(64, 64), _ballTexture, new Vector2(0.5f, 0.5f));
			_ball.SmoothingMode = SmoothingMode.Smooth;
			_ball.Anchor = new Vector2(32, 32);
			_ball.Blending.DestinationAlphaBlend = BlendType.InverseSourceAlpha;

			// Create the ball render target.
			_ballTarget = _graphics.Output.CreateRenderTarget("BallTarget", new GorgonRenderTarget2DSettings
			{
				DepthStencilFormat = BufferFormat.Unknown,
				Width = Settings.Default.ScreenWidth,
				Height = Settings.Default.ScreenHeight,
				Format = BufferFormat.R8G8B8A8_UIntNormal,
				Multisampling = GorgonMultisampling.NoMultiSampling
			});
			_2D.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(512, 512);
			_2D.Effects.GaussianBlur.BlurAmount = 10.0f;
			
			// Ensure that our secondary camera gets updated.
			_mainScreen.AfterSwapChainResized += (sender, args) =>
			{
				// Fix any objects caught outside of the main target.
				for (int i = 0; i < _ballCount; i++)
				{
					_ballList[i].Position.X = _ballList[i].Position.X.Max(0).Min(args.Width);
					_ballList[i].Position.Y = _ballList[i].Position.Y.Max(0).Min(args.Height);
				}
				
				_ballTarget.Dispose();

				_ballTarget = _graphics.Output.CreateRenderTarget("BallTarget", new GorgonRenderTarget2DSettings
				{
					DepthStencilFormat = BufferFormat.Unknown,
					Width = args.Width,
					Height = args.Height,
					Format = BufferFormat.R8G8B8A8_UIntNormal,
					Multisampling = GorgonMultisampling.NoMultiSampling
				});

				Vector2 newTargetSize;
				newTargetSize.X = (512.0f * (args.Width / (float)Settings.Default.ScreenWidth)).Min(512);
				newTargetSize.Y = (512.0f * (args.Height / (float)Settings.Default.ScreenHeight)).Min(512);

				_2D.Effects.GaussianBlur.BlurRenderTargetsSize = (Size)newTargetSize;
				_2D.Effects.Displacement.BackgroundImage = _ballTarget;
			};

			// Generate the ball list.
			GenerateBalls(Settings.Default.BallCount);

			// Assign event handlers.
			_form.KeyDown += _form_KeyDown;

			// Create our font.
			_ballFont = _graphics.Fonts.CreateFont("Arial 9pt Bold", new GorgonFontSettings
			{
				AntiAliasingMode = FontAntiAliasMode.AntiAlias,
				FontStyle = FontStyle.Bold,
				FontFamilyName = "Arial",
				FontHeightMode = FontHeightMode.Points,
				Characters = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890()_.-+:\u2191\u2193",
				Size = 9.0f,
				OutlineColor1 = GorgonColor.Black,
				OutlineSize = 1
			});

			// Create statistics render target.
			_statsTarget = _graphics.Output.CreateRenderTarget("Statistics", new GorgonRenderTarget2DSettings
			{
				Width = (int)_2D.Drawing.MeasureString(_ballFont, string.Format(Resources.FPSLine, 999999, 999999.999, _ballCount), false, _form.ClientSize).X,
				Height = (int)((_ballFont.FontHeight * 4) + _ballFont.Descent),
				Format = BufferFormat.R8G8B8A8_UIntNormal
			});

			// Draw our stats window frame.
			_2D.Target = _statsTarget;
			_2D.Clear(new GorgonColor(0, 0, 0, 0.5f));
			_2D.Drawing.DrawRectangle(new RectangleF(0, 0, _statsTarget.Settings.Width - 1, _statsTarget.Settings.Height - 1),
				new GorgonColor(0.86667f, 0.84314f, 0.7451f, 1.0f));
			_2D.Target = null;

			// Statistics text buffer.
			_fpsText = new StringBuilder(64);
			_helpText = new StringBuilder();
		    _helpText.AppendFormat(Resources.HelpText,
		                           _graphics.VideoDevice.Name,
		                           _graphics.VideoDevice.SupportedFeatureLevel,
		                           _graphics.VideoDevice.DedicatedVideoMemory.FormatMemory());

			// Create a static text block.  This will perform MUCH better than drawing the text 
			// every frame with DrawString.
			_helpTextSprite = _2D.Renderables.CreateText("Help Text", _ballFont, _helpText.ToString(), Color.Yellow);
			_helpTextSprite.Position = new Vector2(3, _statsTarget.Settings.Height + 8.0f);
			_helpTextSprite.Blending.DestinationAlphaBlend = BlendType.InverseSourceAlpha;
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
				case Keys.Pause:
					_paused = !_paused;
					break;
				case Keys.F1:
					_showHelp = !_showHelp;
					break;
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
				case Keys.OemMinus:
					_2D.Effects.GaussianBlur.BlurAmount += 0.25f;
					if (_2D.Effects.GaussianBlur.BlurAmount > 10)
						_2D.Effects.GaussianBlur.BlurAmount = 10;
					break;
				case Keys.Oemplus:
					_2D.Effects.GaussianBlur.BlurAmount -= 0.25f;
					if (_2D.Effects.GaussianBlur.BlurAmount < 2)
						_2D.Effects.GaussianBlur.BlurAmount = 2;
					break;
			}
		}

		/// <summary>
		/// Function to perform clean up operations.
		/// </summary>
		private static void CleanUp()
		{
			if (_form != null)
			{
				_form.Dispose();
			}

			if (_graphics != null)
			{
				_graphics.Dispose();
			}
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

				GorgonApplication.Run(_form, Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, _ => GorgonDialogs.ErrorBox(null, _), true);
			}
			finally
			{
				CleanUp();
			}
		}
	}
}