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
		private Font _helpFont = null;						// Font for the help text.
		private float _textY = 0.0f;						// Text vertical positioning.
		private FXShader _blur = null;						// Blur shader.
		private bool _blurBounce = false;					// Blur bounce flag.
		private float _blurAmount = 1.0f;					// Blur amount.
		private bool _showHelp = true;						// Flag to show help.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyDown event of the MainForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F1)
				_showHelp = !_showHelp;
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
                _blurAmount += 5.0f * e.FrameDeltaTime;
            else
                _blurAmount -= 5.0f * e.FrameDeltaTime;

			if ((_blurAmount >= 7.5f) || (_blurAmount <= 0.5f))
				_blurBounce = !_blurBounce;

			// Draw the base.
			_base.SetPosition(Gorgon.Screen.Width / 4, Gorgon.Screen.Height / 4);
			_base.Draw();

			// Draw motherships.
			_mother1.SetPosition(Gorgon.Screen.Width - (Gorgon.Screen.Width / 4), Gorgon.Screen.Height / 4);
			_mother1.Draw();

			_mother2.SetPosition(Gorgon.Screen.Width / 2, Gorgon.Screen.Height / 2);
			Gorgon.CurrentShader = _blur;
			_mother2.Draw();
			Gorgon.CurrentShader = null;
			if (_showHelp)
			{
				_textSprite.SetPosition(0, 0);
				_textSprite.Font = _helpFont;
				_textSprite.Color = Drawing.Color.Blue;
				_textSprite.Text = "F1 - Show/hide this help text.\nS - Show frame statistics.\nESC - Exit.";
				_textSprite.Draw();

				// Reset our text.
				_textSprite.Color = Drawing.Color.Black;
				_textSprite.Text = _text;
				_textSprite.Font = _textFont;
			}
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
			_helpFont = new Font("Arial_9pt", "Arial", 10.0f, true, true);

			// Get the file system provider.
			FileSystemProvider.Load(@"..\..\..\..\PlugIns\bin\Release\GorgonBZip2FileSystem.dll");

			// Create the bzip file system.
            _bzipFS = FileSystem.Create("SomeBZipFileSystem", FileSystemProviderCache.Providers["Gorgon.BZip2FileSystem"]);

			// Mount the file system.
			_bzipFS.AssignRoot(@"..\..\..\..\Resources\FileSystems\BZipFileSystem.gorPack");

			// Get the sprite image.
			_spriteImage = Image.FromFileSystem(_bzipFS, @"\Images\0_HardVacuum.png");

			// Get shader.
			if (Gorgon.CurrentDriver.PixelShaderVersion >= new Version(2, 0))
			{
#if DEBUG
				_blur = FXShader.FromFileSystem(_bzipFS, @"\Shaders\Blur.fx", ShaderCompileOptions.Debug);
#else
				_blur = FXShader.FromFileSystem(_bzipFS, @"\Shaders\Blur.fx", ShaderCompileOptions.OptimizationLevel3);
#endif
			}

			// Get the sprites.
			_base = Sprite.FromFileSystem(_bzipFS, @"\Sprites\base.gorSprite");
			_mother1 = Sprite.FromFileSystem(_bzipFS, @"\Sprites\Mother.gorSprite");
			_mother2 = Sprite.FromFileSystem(_bzipFS, @"\Sprites\Mother2c.gorSprite");

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
