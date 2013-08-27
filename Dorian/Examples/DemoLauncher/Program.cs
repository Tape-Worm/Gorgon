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
		private static float _startTime;											// Start time.
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
		private static void DrawLogo()
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

				if (!(_renderer.Effects.Wave.Length < 0.01f))
				{
					return;
				}
				_timer.Reset();
				_renderer.Effects.Wave.Length = 0.0f;
			}
			else
			{
				_logoSprite.Draw();

				if (!(_timer.Seconds > 1))
				{
					return;
				}

				_timer.Reset();
				_currentState = ProgramState.MainExecution;
			}
		}

		/// <summary>
		/// Function to update the main user interface.
		/// </summary>
		private static void UpdateMainUI()
		{
			float time = (GorgonTiming.SecondsSinceStart - _startTime) / 1.25f;

			if (time <= 1.0f)
			{
				var startPosition = new Vector2(_screen.Settings.Width / 2.0f - _logo.Settings.Width / 2.0f,
					_screen.Settings.Height / 2.0f - _logo.Settings.Height / 2.0f);
				var startSize = new Vector2(653, 156);
				var endSize = new Vector2(256, 61);
				var endPosition = new Vector2(_screen.Settings.Width - 256, _screen.Settings.Height - 64);

				_logoSprite.ScaledSize = startSize + ((endSize - startSize) * time);
				_logoSprite.Position = startPosition + ((endPosition - startPosition) * time);

				_logoSprite.Draw();
			}
			else
			{
				if (!_renderer.IsLogoVisible)
				{
					_renderer.IsLogoVisible = true;
				}

			    if (!(_logoSprite.Opacity > 0))
			    {
			        return;
			    }

			    _logoSprite.Opacity -= GorgonTiming.Delta * 0.75f;
			    _logoSprite.Draw();
			}
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
					DrawLogo();
					break;
				case ProgramState.MainExecution:
					if (_startTime.EqualsEpsilon(0))
					{
						_startTime = GorgonTiming.SecondsSinceStart;
					}

					UpdateMainUI();
					break;
			}

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
			IntPtr bitmap = CreateCompatibleBitmap(screenDC, currentScreen.Bounds.Width, currentScreen.Bounds.Height);
			IntPtr prevDC = SelectObject(destDC, bitmap);

			// Copy to the bitmap.
			BitBlt(destDC, 0, 0, currentScreen.Bounds.Width, currentScreen.Bounds.Height, screenDC, currentScreen.Bounds.X,
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
				_form = new formMain
				{
					Location = Cursor.Position
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
