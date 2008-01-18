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
// Created: Wednesday, October 18, 2006 1:12:00 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Utility;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.FileSystems;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: Form
	{
		#region Variables.
		private FileSystem _bzipFS = null;					// BZip file system.
		private Image _spriteImage = null;					// Sprite image.
		private Sprite _base = null;						// Base sprite.
		private Sprite _mother1 = null;						// Mothership 1.
		private Sprite _mother2 = null;						// Mothership 2.
		private string _text = string.Empty;				// Text to display.
		private TextSprite _textSprite = null;				// Text sprite.
		private Font _textFont = null;						// Font for the text.
		private float _textY = 0.0f;						// Text vertical positioning.
		private Shader _blur = null;						// Blur shader.
		private bool _blurBounce = false;					// Blur bounce flag.
		private float _blurAmount = 1.0f;					// Blur amount.
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

			_textSprite.SetPosition(0, (int)_textY);
			_textSprite.Draw();

			if (_textSprite.Position.Y < -_textSprite.Height)
				_textY = Gorgon.Screen.Height + _textFont.LineHeight;

			// Scroll up.
			_textY -= (25.0f * e.FrameDeltaTime);

			// Set blur amount.
			if (_blur != null)
				_blur.Parameters["blurAmount"].SetValue(_blurAmount);
			else
				_mother2.Opacity = (byte)(34.0f * _blurAmount);

			if (!_blurBounce)
				_blurAmount += 1.0f * e.FrameDeltaTime;
			else
				_blurAmount -= 1.0f * e.FrameDeltaTime;

			if ((_blurAmount >= 7.5f) || (_blurAmount <= 0.5f))
				_blurBounce = !_blurBounce;

			// Draw the base.
			_base.SetPosition(Gorgon.Screen.Width / 4, Gorgon.Screen.Height / 4);
			_base.Draw();

			// Draw motherships.
			_mother1.SetPosition(Gorgon.Screen.Width - (Gorgon.Screen.Width / 4), Gorgon.Screen.Height / 4);
			_mother1.Draw();

			_mother2.SetPosition(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2);
			_mother2.Draw();
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
		/// Function called to initialize the application.
		/// </summary>
		private void Initialize()
		{
			// Create font.
			_textFont = new Font("Gigi_24pt", "Gigi", 24.0f, true);

			// Get the file system provider.
			if (Gorgon.Platform == PlatformID.x86)
				FileSystemProvider.Load(@"..\..\..\..\PlugIns\bin\Release\GorgonBZip2FileSystem.dll");
			else
				FileSystemProvider.Load(@"..\..\..\..\PlugIns\bin64\Release\GorgonBZip2FileSystem.dll");

			// Create the bzip file system.
			_bzipFS = FileSystem.Create("SomeBZipFileSystem", FileSystemProviderCache.Providers["SharpZip.BZip2 File System"]);

			// Mount the file system.
			_bzipFS.Root = @"..\..\..\..\Resources\FileSystems\BZipFileSystem.gorPack";

			// Mount the root, but do not recurse.
			_bzipFS.Mount(@"\", false);

			// Mount the images and sprites directories.
			_bzipFS.Mount(@"\Images");
			_bzipFS.Mount(@"\Sprites");
			_bzipFS.Mount(@"\Shaders");

			// Get the sprite image.
			_spriteImage = Image.FromFileSystem(_bzipFS, @"\Images\0_HardVacuum.png");

			// Get shader.
			if (Gorgon.CurrentDriver.PixelShaderVersion >= new Version(2, 0))
			{
				_blur = Shader.FromFileSystem(_bzipFS, @"\Shaders\Blur.fx");
				_blur.Parameters["sourceImage"].SetValue(_spriteImage);
			}

			// Get the sprites.
			_base = Sprite.FromFileSystem(_bzipFS, @"\Sprites\base.gorSprite");
			_mother1 = Sprite.FromFileSystem(_bzipFS, @"\Sprites\Mother.gorSprite");
			_mother2 = Sprite.FromFileSystem(_bzipFS, @"\Sprites\Mother2c.gorSprite");
			_mother2.Shader = _blur;

			// Get poetry.
			_text = Encoding.UTF8.GetString(_bzipFS.ReadFile(@"\SomeText.txt"));

			// Create text.
			_textSprite = new TextSprite("Poetry", _text, _textFont);
			_textSprite.Color = Drawing.Color.Black;
			_textSprite.Refresh();
			_textY = Gorgon.Screen.Height + _textFont.LineHeight;
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
				// Initialize the library.
				Gorgon.Initialize();

				// Display the logo and frame stats.
				Gorgon.LogoVisible = true;
				Gorgon.FrameStatsVisible = false;
				Gorgon.InvertFrameStatsTextColor = false;

				// Set the video mode to match the form client area.
				Gorgon.SetMode(this, 640, 480, BackBufferFormats.BufferRGB888, true);

				// Assign rendering event handler.
				Gorgon.Idle += new FrameEventHandler(Screen_OnFrameBegin);

				// Set the clear color to something ugly.
				Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(250, 245, 220);

				// Initialize this.
				this.Visible = true;
				this.Cursor = Cursors.WaitCursor;
				Initialize();

				// Begin execution.
				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "An unhandled error occured during execution, the program will now close.",ex);
				Application.Exit();
			}
			finally
			{
				this.Cursor = Cursors.Default;
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
