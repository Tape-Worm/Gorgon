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
using System.IO;
using System.Text;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;
using Color = System.Drawing.Color;
using FontStyle = Gorgon.Graphics.Fonts.FontStyle;
using Point = System.Drawing.Point;

namespace Gorgon.Examples
{
	/// <summary>
	/// Main class for the application.
	/// </summary>
	static class Program
	{
	    // Maximum FPS for our ball simulation.
		private const float MaxSimulationFPS = 1 / 60.0f;														
	    // Minimum FPS for our ball simulation.
		private const float MinSimulationFPS = 1 / 30.0f;														

	    // Our application form.
		private static MainForm _form;																            
	    // Our main graphics interface.
		private static GorgonGraphics _graphics;														        
	    // Our 2D interface.
		private static Gorgon2D _2D;																	        
	    // The swap chain for our primary display.
		private static GorgonSwapChain _mainScreen;													            
	    // Texture for the balls.
		private static GorgonTexture2DView _ballTexture;													    
	    // Background checked wall sprite.
		private static GorgonSprite _wall;															            
	    // Ball sprite.		
		private static GorgonSprite _ball;															            
	    // Our list of balls.
		private static IList<Ball> _ballList;														            
	    // Number of balls.
		private static int _ballCount;																	        
	    // Our accumulator for running at a fixed frame rate.
		private static float _accumulator;																        
	    // Font to display our FPS, etc...
		private static GorgonFont _ballFont;
        // The font factory.
	    private static GorgonFontFactory _fontFactory;
	    // Render target for the balls.
		private static GorgonRenderTarget2DView _ballTarget;
        // The view for rendering the stats render target.
	    private static GorgonTexture2DView _statsTexture;
	    // Frames per second text.
		private static StringBuilder _fpsText;														            
	    // Text sprite for our help text.
		private static GorgonTextSprite _helpTextSprite;														
	    // Flag to indicate that the help text should be shown.
		private static bool _showHelp = true;																    
	    // Flag to indicate that the animation is paused.
		private static bool _paused;

        /// <summary>
        /// Property to return the path to the resources for the example.
        /// </summary>
        /// <param name="resourceItem">The directory or file to use as a resource.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
        public static string GetResourcePath(string resourceItem)
		{
			string path = Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
				throw new ArgumentException(@"The resource was not specified.", nameof(resourceItem));
			}

			path = path.FormatDirectory(Path.DirectorySeparatorChar);

			// If this is a directory, then sanitize it as such.
			if (resourceItem.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path += resourceItem.FormatDirectory(Path.DirectorySeparatorChar);
			}
			else
			{
				// Otherwise, format the file name.
				path += resourceItem.FormatPath(Path.DirectorySeparatorChar);
			}

			// Ensure that we have an absolute path.
			return Path.GetFullPath(path);
		}

	    /// <summary>
	    /// Function to generate the balls.
	    /// </summary>
	    /// <param name="ballCount">Ball count to add to the total ball count.</param>
	    private static void GenerateBalls(int ballCount)
	    {
	        float halfWidth = _mainScreen.Width / 2.0f;
	        float halfHeight = _mainScreen.Height / 2.0f;
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
	                           Position = new DX.Vector2(halfWidth - (_ball.Size.Width / 2.0f), halfHeight - (_ball.Size.Height / 2.0f)),
	                           PositionDelta = new DX.Vector2((GorgonRandom.RandomSingle() * _mainScreen.Width) - (halfWidth),
	                                                       (GorgonRandom.RandomSingle() * _mainScreen.Height) - (halfHeight)),
	                           Scale = 1.0f,
	                           ScaleDelta = (GorgonRandom.RandomSingle() * 2.0f) - 1.0f,
	                           Rotation = 0,
	                           RotationDelta = (GorgonRandom.RandomSingle() * 360.0f) - 180.0f,
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

			    DX.Vector2.Multiply(ref  currentBall.PositionDelta, frameTime, out DX.Vector2 scaledDelta);
                DX.Vector2.Add(ref currentBall.Position, ref scaledDelta, out currentBall.Position);
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
				if ((currentBall.Position.X > _mainScreen.Width) || (currentBall.Position.X < 0))
				{
					currentBall.PositionDelta.X = -currentBall.PositionDelta.X;
					currentBall.RotationDelta = -currentBall.RotationDelta;
				}

				if ((currentBall.Position.Y > _mainScreen.Height) || (currentBall.Position.Y < 0))
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
			for (int y = 0; y < _mainScreen.Height; y += (int)_wall.Size.Height)
			{
				for (int x = 0; x < _mainScreen.Width; x += (int)_wall.Size.Height)
				{
					_wall.Color = Color.White;
					_wall.Position = new DX.Vector2(x, y);
                    _2D.DrawSprite(_wall);
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
			    Ball ball = _ballList[i];

				_ball.Angle = ball.Rotation;
				_ball.Position = ball.Position;
				_ball.Color = new GorgonColor(ball.Color, ball.Opacity);
				_ball.Scale = new DX.Vector2(ball.Scale, ball.Scale);

			    DX.Vector2 offset = ball.Checkered ? new DX.Vector2(0.5f, 0) : new DX.Vector2(0, 0.5f);
                _ball.TextureRegion = new DX.RectangleF(offset.X, offset.Y, _ball.TextureRegion.Width, _ball.TextureRegion.Height);
				
                _2D.DrawSprite(_ball);
			}
		}

		/// <summary>
		/// Function to perform rendering with the blur filter.
		/// </summary>
		private static void DrawBlurred()
		{
			/*_2D.Target = _ballTarget;
			_2D.Clear(GorgonColor.Transparent);

			DrawNoBlur();

			_2D.Target = null;

			if (_2D.Effects.GaussianBlur.RenderScene == null)
			{
				// Draw using the blur effect.
				_2D.Effects.GaussianBlur.RenderScene = pass =>
				{
					_2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
					_2D.Drawing.Blit(_ballTarget, new RectangleF(DX.Vector2.Zero, _2D.Effects.GaussianBlur.BlurRenderTargetsSize));
				};
			}

			_2D.Effects.GaussianBlur.Render();

			// Copy the blur output to our main target.
            _2D.Drawing.SmoothingMode = SmoothingMode.None;
			_2D.Drawing.Blit(_2D.Effects.GaussianBlur.Output, new RectangleF(DX.Vector2.Zero, new SizeF(_mainScreen.Settings.Width, _mainScreen.Settings.Height)));*/
            // TODO: Not implemented yet.
		}

		/// <summary>
		/// Function to draw the overlay elements, like text.
		/// </summary>
		private static void DrawOverlay()
		{
		    // Draw the draw call counter.
		    _fpsText.Length = 0;
            // The draw commands below this are 2 guaranteed to generate actual draw calls, hence the + 2.
		    _fpsText.AppendFormat(Resources.FPSLine, GorgonTiming.AverageFPS, GorgonTiming.AverageDelta * 1000.0f, _ballCount, _graphics.DrawCallCount);

		    _2D.DrawFilledRectangle(new DX.RectangleF(0, 0, _statsTexture.Width, _statsTexture.Height),
		                            GorgonColor.White,
		                            _statsTexture,
		                            new DX.RectangleF(0, 0, 1, 1));
            _2D.DrawString(_fpsText.ToString(), new DX.Vector2(3.0f, 0), _ballFont);
		}

		/// <summary>
		/// Function for the main idle loop.
		/// </summary>
		/// <remarks>This is used as the main loop for the application.  All drawing and logic can go in here.</remarks>
		/// <returns><b>true</b> to keep running, <b>false</b> to exit.</returns>
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

            // Begin our rendering.
            _2D.Begin();

			DrawBackground();

            // TODO: Not supported.
			/*if (_2D.Effects.GaussianBlur.BlurAmount >= 10.0f)
			{
				DrawNoBlur();
			}
			else
			{
				DrawBlurred();
			}*/

		    DrawNoBlur();

		    if (_showHelp)
		    {
		        _2D.DrawTextSprite(_helpTextSprite);
		    }

			_2D.End();

		    _2D.Begin();
		    DrawOverlay();
		    _2D.End();

			_mainScreen.Present();

            _graphics.ResetDrawCallStatistics();

			return true;
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
		    _form = new MainForm
		            {
		                ClientSize = new Size(Settings.Default.ScreenWidth, Settings.Default.ScreenHeight)
		            };
			_form.Show();

			// Create the graphics interface.
		    IReadOnlyList<IGorgonVideoAdapterInfo> adapters = GorgonGraphics.EnumerateAdapters();

            // TODO: Check to ensure we actually have something we can use.
			_graphics = new GorgonGraphics(adapters[0]);

			// Create the primary swap chain.
		    _mainScreen = new GorgonSwapChain(_graphics,
		                                      _form,
		                                      new GorgonSwapChainInfo("Main Screen")
		                                      {
		                                          Width = Settings.Default.ScreenWidth,
		                                          Height = Settings.Default.ScreenHeight,
                                                  Format = BufferFormat.R8G8B8A8_UNorm
		                                      });

			// Center the display.
			if (_mainScreen.IsWindowed)
			{
			    _form.Location =
			        new Point(Screen.PrimaryScreen.Bounds.Width / 2 - _form.Width / 2 + Screen.PrimaryScreen.Bounds.Left,
			                  Screen.PrimaryScreen.Bounds.Height / 2 - _form.Height / 2 + Screen.PrimaryScreen.Bounds.Top);
			}

			// Load the ball texture.
		    _ballTexture = GorgonTexture2DView.FromFile(_graphics,
		                                                GetResourcePath(@"Textures\Balls\BallsTexture.dds"),
		                                                new GorgonCodecDds(),
		                                                new GorgonTextureLoadOptions
		                                                {
		                                                    Usage = ResourceUsage.Immutable,
		                                                    Name = "Ball Texture"
		                                                });

			// Create the 2D interface.
            _2D = new Gorgon2D(_mainScreen.RenderTargetView);

			// Create the wall sprite.
            _wall = new GorgonSprite
                    {
                        Size = new DX.Size2F(63, 63),
                        Texture = _ballTexture,
                        TextureRegion = new DX.RectangleF(0, 0, 0.5f, 0.5f)
                    };

			// Create the ball sprite.
            _ball = new GorgonSprite
                    {
                        Size = new DX.Size2F(64, 64),
                        Texture = _ballTexture,
                        TextureRegion = new DX.RectangleF(0, 0, 0.5f, 0.5f),
                        Anchor = new DX.Vector2(0.5f, 0.5f)
                    };

			// Create the ball render target.
            _ballTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo("Ball Target")
                                                                                 {
                                                                                     Width = Settings.Default.ScreenWidth,
                                                                                     Height = Settings.Default.ScreenHeight,
                                                                                     Format = BufferFormat.R8G8B8A8_UNorm
                                                                                 });

            // TODO: Not supported yet.
            /*
			_2D.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(512, 512);
			_2D.Effects.GaussianBlur.BlurAmount = 10.0f;
            */

            _mainScreen.BeforeSwapChainResized += (sender, args) => _ballTarget.Dispose();
			
			// Ensure that our secondary camera gets updated.
			_mainScreen.AfterSwapChainResized += (sender, args) =>
			{
				// Fix any objects caught outside of the main target.
				for (int i = 0; i < _ballCount; i++)
				{
					_ballList[i].Position.X = _ballList[i].Position.X.Max(0).Min(args.Size.Width);
					_ballList[i].Position.Y = _ballList[i].Position.Y.Max(0).Min(args.Size.Height);
				}

			    _ballTarget = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo("Ball Target")
			                                                                         {
			                                                                             Width = args.Size.Width,
			                                                                             Height = args.Size.Height,
			                                                                             Format = BufferFormat.R8G8B8A8_UNorm
			                                                                         });

                // TODO: Not supported yet.
                /*
				DX.Vector2 newTargetSize;
				newTargetSize.X = (512.0f * (args.Size.Width / (float)Settings.Default.ScreenWidth)).Min(512);
				newTargetSize.Y = (512.0f * (args.Size.Height / (float)Settings.Default.ScreenHeight)).Min(512);

				_2D.Effects.GaussianBlur.BlurRenderTargetsSize = (Size)newTargetSize;
				_2D.Effects.Displacement.BackgroundImage = _ballTarget;*/
			};

			// Generate the ball list.
			GenerateBalls(Settings.Default.BallCount);

			// Assign event handlers.
			_form.KeyDown += Form_KeyDown;

			// Create our font.
            _fontFactory = new GorgonFontFactory(_graphics);
		    _ballFont = _fontFactory.GetFont(new GorgonFontInfo("Arial", 9.0f, FontHeightMode.Points, "Arial 9pt Bold")
		                                     {
		                                         FontStyle = FontStyle.Bold,
                                                 Characters = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890()_.-+:\u2191\u2193",
                                                 OutlineSize = 1,
                                                 OutlineColor1 = GorgonColor.Black,
                                                 OutlineColor2 = GorgonColor.Black
		                                     });

			// Create statistics render target.

		    _statsTexture = GorgonTexture2DView.CreateTexture(_graphics,
		                                                      new GorgonTexture2DInfo("Stats Render Target")
		                                                      {
		                                                          Width = (int)_ballFont
		                                                                       .MeasureText(string.Format(Resources.FPSLine, 999999, 999999.999, _ballCount, 9999),
		                                                                                    true).Width,
		                                                          Height = (int)((_ballFont.FontHeight * 4) + _ballFont.Descent),
		                                                          Format = BufferFormat.R8G8B8A8_UNorm,
		                                                          Binding = TextureBinding.RenderTarget
		                                                      });

		    // Statistics text buffer.
		    _fpsText = new StringBuilder(64);

		    _helpTextSprite = new GorgonTextSprite(_ballFont,
		                                           string.Format(Resources.HelpText,
		                                                         _graphics.VideoAdapter.Name,
		                                                         _graphics.VideoAdapter.FeatureSet,
		                                                         _graphics.VideoAdapter.Memory.Video.FormatMemory()))
		                      {
		                          Color = Color.Yellow,
		                          Position = new DX.Vector2(3, (_statsTexture.Height + 8.0f).FastFloor()),
		                          DrawMode = TextDrawMode.OutlinedGlyphs
		                      };

		    using (GorgonRenderTarget2DView rtv = _statsTexture.Texture.GetRenderTargetView())
		    {
		        // Draw our stats window frame.
		        rtv.Clear(new GorgonColor(0, 0, 0, 0.5f));
		        _graphics.SetRenderTarget(rtv);
		        _2D.Begin();
		        _2D.DrawRectangle(new DX.RectangleF(0, 0, rtv.Width, rtv.Height), new GorgonColor(0.86667f, 0.84314f, 0.7451f, 1.0f));
		        _2D.End();
		    }

		    // Set our main render target.
            _graphics.SetRenderTarget(_mainScreen.RenderTargetView);
		}

		/// <summary>
		/// Handles the KeyDown event of the _form control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private static void Form_KeyDown(object sender, KeyEventArgs e)
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
				    {
				        if (_mainScreen.IsWindowed)
				        {
				            IGorgonVideoOutputInfo output = _graphics.VideoAdapter.Outputs[_form.Handle];

				            if (output == null)
				            {
				                _mainScreen.EnterFullScreen();
				            }
				            else
				            {
                                var mode = new GorgonVideoMode(_form.ClientSize.Width, _form.ClientSize.Height, BufferFormat.R8G8B8A8_UNorm);
                                _mainScreen.EnterFullScreen(in mode, output);
				            }
				        }
				        else
				        {
                            _mainScreen.ExitFullScreen();
				        }
				    }

				    break;
                // TODO: Not supported yet
				/*case Keys.OemMinus:
					_2D.Effects.GaussianBlur.BlurAmount += 0.25f;
					if (_2D.Effects.GaussianBlur.BlurAmount > 10)
						_2D.Effects.GaussianBlur.BlurAmount = 10;
					break;
				case Keys.Oemplus:
					_2D.Effects.GaussianBlur.BlurAmount -= 0.25f;
					if (_2D.Effects.GaussianBlur.BlurAmount < 2)
						_2D.Effects.GaussianBlur.BlurAmount = 2;
					break;*/
			}
		}

		/// <summary>
		/// Function to perform clean up operations.
		/// </summary>
		private static void CleanUp()
		{
			_form?.Dispose();
            _fontFactory?.Dispose();
			_graphics?.Dispose();
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
				ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
			}
			finally
			{
				CleanUp();
			}
		}
	}
}