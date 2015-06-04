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
// Created: Tuesday, October 8, 2013 11:26:13 PM
// 
#endregion

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Fonts.Properties;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;
using SlimMath;

namespace Fonts
{
	/// <summary>
	/// Main entry point to the application.
	/// </summary>
	static class Program
	{
		#region Variables.
		private static formMain _formMain;
		private static GorgonGraphics _graphics;
		private static Gorgon2D _renderer;
		private static GorgonFont _font;
		private static GorgonTexture2D _specialGlyphTexture;
		#endregion

		#region Methods.
		/// <summary>
		/// Property to return the path to the resources for the example.
		/// </summary>
		/// <param name="resourceItem">The directory or file to use as a resource.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
		private static string GetResourcePath(string resourceItem)
		{
			string path = Settings.Default.ResourceLocation;

			if (string.IsNullOrEmpty(resourceItem))
			{
				throw new ArgumentException("The resource was not specified.", "resourceItem");
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
		/// Function to handle idle CPU time.
		/// </summary>
		/// <returns><b>true</b> to continue handling idle time, <b>false</b> to stop.</returns>
		private static bool Idle()
		{
			_renderer.Clear(Color.Blue);

			_renderer.Drawing.DrawString(_font, "Test the font.\n\t\t\tBut this won't work will it?", new Vector2(0, 10), Color.White);

			_renderer.Render();

			return true;
		}

		/// <summary>
		/// Function to initialize the example application.
		/// </summary>
		private static void Initialize()
		{
			var pathBrush = new GorgonGlyphPathGradientBrush
			                {
				                SurroundColors = {
					                                 Color.Red,
					                                 Color.Green,
					                                 Color.Blue,
					                                 Color.Yellow
				                                 },
				                CenterColor = Color.White,
								Points =
								{
									new Vector2(0, 8),
									new Vector2(8, 0),
									new Vector2(16, 8),
									new Vector2(8, 16)
								},
								CenterPoint = new Vector2(8, 8),
								Interpolation = 
								{
									new GorgonGlyphBrushInterpolator(0, Color.Purple),
									new GorgonGlyphBrushInterpolator(0.5f, Color.Cyan),
									new GorgonGlyphBrushInterpolator(1.0f, Color.Firebrick)
								},
				                WrapMode = WrapMode.TileFlipXY
			                };

			var linBrush = new GorgonGlyphLinearGradientBrush
			               {
							   Angle = 45.0f,
							   StartColor = Color.Purple,
							   EndColor = Color.Yellow
			               };

			var hatchBrush = new GorgonGlyphHatchBrush
			                 {
								 HatchStyle = HatchStyle.Percent50,
								 ForegroundColor = Color.Purple,
								 BackgroundColor = Color.Yellow
			                 };

			_formMain = new formMain();

			// Create our graphics object(s).
			_graphics = new GorgonGraphics();

			var textBrush = new GorgonGlyphTextureBrush(_graphics.Textures.FromFile<GorgonTexture2D>("Stars",
				                                                                         GetResourcePath(@"Images\Stars-12.jpg"),
				                                                                         new GorgonCodecJPEG()))
			{
				TextureRegion = new RectangleF(0.0f, 0.0f, 0.5f, 0.5f),
				WrapMode = WrapMode.Tile
			};


			_renderer = _graphics.Output.Create2DRenderer(_formMain,
			                                              Settings.Default.ScreenResolution.Width,
			                                              Settings.Default.ScreenResolution.Height,
			                                              BufferFormat.Unknown,
			                                              !Settings.Default.FullScreen);

			Screen activeMonitor = Screen.FromControl(_formMain);

			// Center the window on the screen.
			_formMain.Location = new Point(activeMonitor.WorkingArea.Width / 2 - _formMain.Width / 2, activeMonitor.WorkingArea.Height / 2 - _formMain.Height / 2);

			_specialGlyphTexture = _graphics.Textures.FromFile<GorgonTexture2D>("StyledT", GetResourcePath(@"Fonts\StylizedT.png"),
			                                                                    new GorgonCodecPNG());

			_font = _graphics.Fonts.CreateFont("TestFont",
			                                   new GorgonFontSettings
			                                   {
												   AntiAliasingMode = FontAntiAliasMode.AntiAlias,
												   FontFamilyName = "Times New Roman",
												   FontStyle = FontStyle.Bold,
												   Size = 24.0f,
												   Brush = textBrush,
												   Glyphs =
												   {
														new GorgonGlyph('T', _specialGlyphTexture, new Rectangle(11, 14, 111, 97), new Point(-3, -8), 97)
												   }
			                                   });

			// TODO: This is for testing font capabilities.
			_renderer.Drawing.BlendingMode = BlendingMode.Modulate;

			_font.Save(@"d:\unpak\fontTest.gorFont");

			_font.Dispose();

			textBrush.Texture.Dispose();
			
			_font = _graphics.Fonts.FromFile("TestFont", @"d:\unpak\fontTest.gorFont");
		}
		#endregion

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

				// Initialize our example.
				Initialize();

				// Start it running.
				GorgonApplication.Run(_formMain, Idle);
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
			}
			finally
			{
				// Perform clean up.
				if (_renderer != null)
				{
					_renderer.Dispose();
				}

				if (_graphics != null)
				{
					_graphics.Dispose();
				}
			}
		}
	}
}
