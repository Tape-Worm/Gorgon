#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, August 26, 2013 10:57:41 PM
// 
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Math;
using GorgonLibrary.UI;
using SlimMath;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Examples
{
	/// <summary>
	/// State for the application.
	/// </summary>
	enum ProgramState
	{
		Initialization = 0,
		DrawLogo = 1,
		MainExecution = 2
	}

	/// <summary>
	/// The launcher program.
	/// </summary>
	static class Program
	{
		#region Constants.
		private const int SrcCopy = 0x00CC0020; // BitBlt dwRop parameter
		#endregion

		#region Variables.
		private static GorgonColor _normalButtonState = Color.FromArgb(96, Color.Black);		// Normal button state.
		private static GorgonColor _minButtonState = Color.FromArgb(255, Color.DarkGray);		// Minimize button hover state.
		private static GorgonColor _closeButtonState = Color.FromArgb(255, Color.DarkRed);		// Close button hover state.

		private static formMain _form;												// The application form.
		private static GorgonGraphics _graphics;									// The graphics interface.
		private static GorgonSwapChain _screen;										// Main screen.
		private static Gorgon2D _renderer;											// 2D renderer.
		private static GorgonRenderTarget2D _blurBackground;						// Source blur background picture.
		private static ProgramState _currentState = ProgramState.Initialization;	// Current state.
		private static GorgonSprite _backgroundSprite;								// Background sprite.
		private static GorgonSprite _blurSprite;									// Sprite to display the background image.
		private static GorgonTexture2D _originalBackground;							// Original unblurred background image.
		private static GorgonTexture2D _logo;										// Logo texture.
		private static GorgonSprite _logoSprite;									// Logo sprite.
		private static GorgonTimer _timer;											// Timer.
		private static float _startTime = -1.0f;									// Start time.
		private static Func<bool> _renderAction;									// Action for rendering.
		private static GorgonFont _marlettFont;										// Marlett font.
		private static GorgonFont _windowFont;										// Window font.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		#region Win32
		[DllImport("gdi32.dll")]
		private static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);
		[DllImport("gdi32.dll")]
		private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hWnd,IntPtr hDC);
		#endregion

		/// <summary>
		/// Function to perform a fading in of the background.
		/// </summary>
		private static void FadeBackground()
		{
			if (_form.Opacity < 1)
			{
				_form.Opacity = 1;
			}

			if (_blurSprite.Opacity < 1)
			{
				_backgroundSprite.Opacity -= GorgonTiming.Delta;
				_blurSprite.Opacity += GorgonTiming.Delta;
				_backgroundSprite.Draw();
			}

			if (_blurSprite.Opacity > 0.10f)
			{
				_currentState = ProgramState.DrawLogo;
			}
		}

		/// <summary>
		/// Function to draw the logo for the application.
		/// </summary>
		/// <returns>TRUE if done, FALSE if not.</returns>
		private static bool FadeInLogo()
		{
			if (_logoSprite.Opacity < 1)
			{
				_logoSprite.Opacity += GorgonTiming.Delta * 0.25f;
			}

			if (_renderer.Effects.Wave.Length > 0)
			{
				_renderer.Effects.Wave.Length -= GorgonTiming.Delta * _renderer.Effects.Wave.Length;
				_renderer.Effects.Wave.Period -= GorgonTiming.Delta * (1.0f / _screen.Settings.Height);
				_renderer.Effects.Wave.Render();

				if (_renderer.Effects.Wave.Length >= 0.01f)
				{
					return false;
				}

				_timer.Reset();
				_renderer.Effects.Wave.Length = 0.0f;
			}
			else
			{
				_logoSprite.Draw();

				if (_timer.Seconds < 1)
				{
					return false;
				}

				_timer.Reset();
				return true;
			}

			return false;
		}
		
		/// <summary>
		/// Function to move the logo.
		/// </summary>
		/// <returns>TRUE if finished, FALSE if not.</returns>
		private static bool MoveLogo()
		{
			if (_startTime < 0)
			{
				_startTime = GorgonTiming.SecondsSinceStart;
			}

			float time = (GorgonTiming.SecondsSinceStart - _startTime) / 1.25f;

			if (time > 1.0f)
			{
				time = 1.0f;
			}

			var startPosition = new Vector2(_screen.Settings.Width / 2.0f - _logo.Settings.Width / 2.0f,
				_screen.Settings.Height / 2.0f - _logo.Settings.Height / 2.0f);
			var startSize = new Vector2(653, 156);
			var endSize = new Vector2(256, 61);
			var endPosition = new Vector2(_screen.Settings.Width - 256, _screen.Settings.Height - 64);

			_logoSprite.ScaledSize = startSize + ((endSize - startSize) * time);
			_logoSprite.Position = startPosition + ((endPosition - startPosition) * time);

			_logoSprite.Draw();

			return time >= 1.0f;
		}

		/// <summary>
		/// Function to fade out the logo.
		/// </summary>
		private static void FadeOutLogo()
		{
			if (!_renderer.IsLogoVisible)
			{
				_renderer.IsLogoVisible = true;
			}

			if (_logoSprite.Opacity > 0)
			{
				_logoSprite.Opacity -= GorgonTiming.Delta * 0.75f;
				_logoSprite.Draw();
				return;
			}

			
			_currentState = ProgramState.MainExecution;
		}

		/// <summary>
		/// Function called during idle time.
		/// </summary>
		/// <returns></returns>
		private static bool Idle()
		{
			_blurSprite.Draw();

			// Fade the background.
		    if (_backgroundSprite.Opacity > 0)
		    {
		        FadeBackground();
		    }
		    else
		    {
                // We don't need this guy any more.
		        if (_originalBackground != null)
		        {
		            _originalBackground.Dispose();
		            _originalBackground = null;
		            _backgroundSprite.Texture = null;
		        }
		    }

			switch (_currentState)
			{
				case ProgramState.DrawLogo:
					bool result = _renderAction != null && _renderAction();

					if ((result) && (_renderAction == FadeInLogo))
					{
						_renderAction = MoveLogo;
					}
					else if ((result && (_renderAction == MoveLogo)))
					{
						_renderAction = null;
					} 
					else if (_renderAction == null)
					{
						FadeOutLogo();
					}
					break;
				case ProgramState.MainExecution:
					// TODO: This is an experiment, and thus, awful code.
					var minButtonLocation = new RectangleF(_screen.Settings.Width - 86, 0, 43, 19);
					var closeButtonLocation = new RectangleF(_screen.Settings.Width - 43, 0, 43, 19);

					Vector2 textSize = _renderer.Drawing.MeasureString(_windowFont,
						"Gorgon Demos/Examples",
						false,
						new RectangleF(0, 0, _screen.Settings.Width, closeButtonLocation.Height));

					
					_renderer.Drawing.FilledRectangle(new RectangleF(0, 0, _screen.Settings.Width, closeButtonLocation.Height), _normalButtonState);

					Point position = Cursor.Position;
					position.X -= _screen.VideoOutput.OutputBounds.X;
					
					if (minButtonLocation.Contains(position))
					{
						_renderer.Drawing.FilledRectangle(minButtonLocation, _minButtonState);
					}

					if (closeButtonLocation.Contains(position))
					{
						_renderer.Drawing.FilledRectangle(closeButtonLocation, _closeButtonState);
					}
					
					_renderer.Drawing.DrawString(_marlettFont, "0", new Vector2(_screen.Settings.Width - 72, 2), Color.White);
					_renderer.Drawing.DrawString(_marlettFont, "r", new Vector2(_screen.Settings.Width - 31, 2), Color.White);
					_renderer.Drawing.DrawString(_windowFont, "Gorgon Demos/Examples", new Vector2(_screen.Settings.Width / 2.0f - textSize.X / 2.0f, -3), Color.White);

					_renderer.Drawing.DrawString(_windowFont, minButtonLocation.ToString(), new Vector2(0, 84), Color.White);
					_renderer.Drawing.DrawString(_windowFont, closeButtonLocation.ToString(), new Vector2(0, 104), Color.White);
					break;
			}

			_renderer.Drawing.DrawString(_windowFont, Cursor.Position.ToString(), new Vector2(0, 64), Color.White);
			_renderer.Render(1);

			return true;
		}

		/// <summary>
		/// Function to capture the desktop.
		/// </summary>
		/// <returns>A bitmap containing the desktop.</returns>
		private static Image CaptureScreen()
		{
			Screen currentScreen = Screen.FromControl(_form);
			IntPtr screenHWND = GetDesktopWindow();
			IntPtr screenDC = GetWindowDC(screenHWND);
			IntPtr destDC = CreateCompatibleDC(screenDC);
			IntPtr bitmap = CreateCompatibleBitmap(screenDC, currentScreen.Bounds.Width, currentScreen.WorkingArea.Height);
			IntPtr prevDC = SelectObject(destDC, bitmap);

			// Copy to the bitmap.
			BitBlt(destDC, 0, 0, currentScreen.Bounds.Width, currentScreen.WorkingArea.Height, screenDC, currentScreen.Bounds.X,
				currentScreen.Bounds.Y, SrcCopy);

			SelectObject(destDC, prevDC);
			DeleteDC(destDC);
			ReleaseDC(screenHWND, screenDC);

			Image result = Image.FromHbitmap(bitmap);
			DeleteObject(bitmap);

			return result;
		}

		/// <summary>
		/// Function to blur the background image.
		/// </summary>
		/// <param name="sourceImage">The image used to blur.</param>
		private static void BlurBackground(GorgonTexture2D sourceImage)
		{
            _renderer.Effects.GrayScale.RenderScene = pass =>
            {
                _renderer.Target = _blurBackground;
                _renderer.Drawing.Blit(sourceImage, new RectangleF(0, 0, _blurBackground.Settings.Width, _blurBackground.Settings.Height));
            };

            _renderer.Effects.GrayScale.Render();

            // Blur to the output.
			_renderer.Effects.GrayScale.RenderScene = pass =>
			{
				_renderer.Target = _blurBackground;
				_renderer.Drawing.Blit(sourceImage, new RectangleF(0, 0, _blurBackground.Settings.Width, _blurBackground.Settings.Height));
			};

			_renderer.Effects.GrayScale.Render();

            _renderer.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(_blurBackground.Settings.Width, _blurBackground.Settings.Height);
            _renderer.Effects.GaussianBlur.BlurAmount = 4.5f;
            _renderer.Effects.GaussianBlur.RenderScene = pass => _renderer.Drawing.Blit(_blurBackground, Vector2.Zero);
            _renderer.Effects.GaussianBlur.Render();

		    _renderer.Target = _blurBackground;
            _renderer.Drawing.Blit(_renderer.Effects.GaussianBlur.Output, new RectangleF(0, 0, _blurBackground.Settings.Width, _blurBackground.Settings.Height));
		    _renderer.Target = null;

            _renderer.Effects.GaussianBlur.FreeResources();

            _blurSprite.Opacity = 0;
		}
			
		/// <summary>
		/// Function used to initialize the launcher.
		/// </summary>
		private static void Initialize()
		{
            _form.Show();

			_graphics = new GorgonGraphics(DeviceFeatureLevel.SM2_a_b);

			using(Image backgroundImage = CaptureScreen())
			{
				_originalBackground = _graphics.Textures.CreateTexture<GorgonTexture2D>("BackgroundImage", backgroundImage);

				_marlettFont = _graphics.Fonts.CreateFont("Marlett",
					new GorgonFontSettings
					{
						FontFamilyName = "Marlett",
						Size = 11.25f,
						AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ,
						FontHeightMode = FontHeightMode.Points,
						TextureSize = new Size(64, 32),
						Characters = "0r "
					});

				_windowFont = _graphics.Fonts.CreateFont("Segoe UI",
					new GorgonFontSettings
					{
						FontFamilyName = "Segoe UI",
						FontStyle = FontStyle.Bold,
						OutlineColor = Color.Black,
						OutlineSize = 1,
						Size = 11.25f,
						AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ,
						FontHeightMode = FontHeightMode.Points
					});
			
				_blurBackground = _graphics.Output.CreateRenderTarget("BlurSourceImage",
					new GorgonRenderTarget2DSettings
					{
						Width = _form.Width / 2,
						Height = _form.Height / 2,
						Format = BufferFormat.R8G8B8A8_UIntNormal
					});

				_screen = _graphics.Output.CreateSwapChain("Screen",
					new GorgonSwapChainSettings
					{
						Window = _form,
						Format = BufferFormat.R8G8B8A8_UIntNormal,
						IsWindowed = true
					});

				_renderer = _graphics.Output.Create2DRenderer(_screen);
				_renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;

				_blurSprite = _renderer.Renderables.CreateSprite("Background",
					new GorgonSpriteSettings
					{
						Size = new Vector2(_screen.Settings.Width, _screen.Settings.Height),
						Color = GorgonColor.White,
						Texture = _blurBackground,
						TextureRegion = new RectangleF(0, 0, 1.0f, 1.0f)
					});

				_backgroundSprite = _renderer.Renderables.CreateSprite("Background",
					new GorgonSpriteSettings
					{
						Size = new Vector2(_screen.Settings.Width, _screen.Settings.Height),
						Color = GorgonColor.White,
						Texture = _originalBackground,
						TextureRegion = new RectangleF(0, 0, 1.0f, 1.0f)
					});

			    BlurBackground(_originalBackground);

			    _logo = _graphics.Textures.CreateTexture<GorgonTexture2D>("Logo", Properties.Resources.Gorgon_2_x_Logo_Full);
				_logoSprite = _renderer.Renderables.CreateSprite("Logo",
					new GorgonSpriteSettings
					{
						Size = new Vector2(653, 156),
						Color = GorgonColor.White,
						Texture = _logo,
                        // Adjust the texture region to match the aspect of the internal logo badge.
                        TextureRegion = new RectangleF(_logo.ToTexel(new Vector2(-16f, -16f)), _logo.ToTexel(new Vector2(653.0f, 156.0f))),
						InitialPosition = new Vector2(_screen.Settings.Width / 2.0f, _screen.Settings.Height / 2.0f)
					});

			    _logoSprite.TextureSampler.BorderColor = GorgonColor.Transparent;
                _logoSprite.TextureSampler.HorizontalWrapping = TextureAddressing.Border;
                _logoSprite.TextureSampler.VerticalWrapping = TextureAddressing.Border;
				_logoSprite.Opacity = 0;
				_logoSprite.SmoothingMode = SmoothingMode.Smooth;
				_logoSprite.Position = new Vector2(_screen.Settings.Width / 2.0f - 327f, _screen.Settings.Height / 2.0f - 78f);

                _renderer.Effects.Wave.Length = 50.0f;
                _renderer.Effects.Wave.Period = 1.0f;
                _renderer.Effects.Wave.RenderScene = pass => _logoSprite.Draw();

				_timer = new GorgonTimer();
				_renderAction = FadeInLogo;

				_form.MouseDoubleClick += OnFormOnMouseDoubleClick;
				_form.KeyDown += OnFormOnKeyDown;
			}
		}

		/// <summary>
		/// Function to handle key presses on the form.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private static void OnFormOnKeyDown(object sender, KeyEventArgs args)
		{
			if ((_currentState != ProgramState.MainExecution) && ((args.KeyCode == Keys.Space)
			    || (args.KeyCode == Keys.Escape)
			    || (args.KeyCode == Keys.Enter)))
			{
				OnFormOnMouseDoubleClick(sender, new MouseEventArgs(MouseButtons.None, 0, 0, 0, 0));
				return;
			}

			switch (args.KeyCode)
			{
				case Keys.Escape:
					Gorgon.Quit();
					return;
			}
		}

		/// <summary>
		/// Function to handle a double click on the form to stop the animation.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private static void OnFormOnMouseDoubleClick(object sender, MouseEventArgs args)
		{
			if (_currentState != ProgramState.MainExecution)
			{
				_blurSprite.Opacity = 1;
				_backgroundSprite.Opacity = 0;
				_logoSprite.Opacity = 0;
				_renderer.IsLogoVisible = true;
				_currentState = ProgramState.MainExecution;
			}
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Gorgon.AllowBackground = true;

				Screen currentScreen = Screen.FromPoint(Cursor.Position);

				_form = new formMain
				{
					Location = currentScreen.WorkingArea.Location,
					Size = currentScreen.WorkingArea.Size,
					Visible = true,
					Opacity = 0
				};

				Initialize();
				

				Gorgon.Run(_form, Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(null, ex));
			}
			finally
			{
				if (_originalBackground != null)
				{
					_originalBackground.Dispose();
					_originalBackground = null;
				}

				if (_graphics != null)
				{
					_graphics.Dispose();
					_graphics = null;
				}
			}
		}
	}
}
