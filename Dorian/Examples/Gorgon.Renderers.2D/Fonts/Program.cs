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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fonts.Properties;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resourceItem"/> was NULL (Nothing in VB.Net) or empty.</exception>
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
		/// <returns>TRUE to continue handling idle time, FALSE to stop.</returns>
		private static bool Idle()
		{
			_renderer.Clear(Color.Ivory);

			_renderer.Drawing.DrawString(_font, "Test the font.\n\t\t\tBut this won't work will it?", new Vector2(0, 10), Color.Black);

			_renderer.Render();

			return true;
		}

		/// <summary>
		/// Function to initialize the example application.
		/// </summary>
		private static void Initialize()
		{
			_formMain = new formMain();

			// Create our graphics object(s).
			_graphics = new GorgonGraphics();

			_renderer = _graphics.Output.Create2DRenderer(_formMain,
			                                              Settings.Default.ScreenResolution.Width,
			                                              Settings.Default.ScreenResolution.Height,
			                                              BufferFormat.Unknown,
			                                              !Settings.Default.FullScreen);

			Screen activeMonitor = Screen.FromControl(_formMain);

			// Center the window on the screen.
			_formMain.Location = new Point(activeMonitor.WorkingArea.Width / 2 - _formMain.Width / 2, activeMonitor.WorkingArea.Height / 2 - _formMain.Height / 2);

			_font = _graphics.Fonts.CreateFont("TestFont",
			                                   new GorgonFontSettings
			                                   {
												   AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ,
												   FontFamilyName = "Times New Roman",
												   Size = 24.0f
			                                   });

			// TODO: This is for testing font capabilities.
			_renderer.Drawing.BlendingMode = BlendingMode.Modulate;

			_font.KerningPairs[new GorgonKerningPair('T', 'e')] = -10;

			_specialGlyphTexture = _graphics.Textures.FromFile<GorgonTexture2D>("StyledT", GetResourcePath(@"Fonts\StylizedT.png"),
			                                                                    new GorgonCodecPNG());

			_font.Textures.Add(_specialGlyphTexture);
			_font.Glyphs['T'] = new GorgonGlyph('T', _specialGlyphTexture, new Rectangle(11, 14, 111, 97), new Vector2(3, -8), new Vector3(-7, 104, 0));

			/*_font.Save(@"d:\unpak\fontTest.gorFont");

			_font.Dispose();*/

			//_font = _graphics.Fonts.FromFile("TestFont", @"d:\unpak\fontTest.gorFont");
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
				Gorgon.Run(_formMain, Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(null, ex));
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
