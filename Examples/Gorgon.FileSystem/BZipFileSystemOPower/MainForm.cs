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
// Created: Monday, January 21, 2013 8:44:19 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;
using SlimMath;

namespace Gorgon.Examples
{
    /// <summary>
    /// Main application form.
    /// </summary>
    /// <remarks>
    /// This example is a port of the Gorgon 1.x BZip packed file system example into Gorgon 2.x.
    /// 
    /// In this example we mount a Gorgon packed file as a virtual file system and pull in an image, 
    /// some Gorgon 1.0 sprites, the backing sprite image and some text for display.
    /// 
    /// The difference between this example and the folder file system example is that we're loading
    /// a packed file from the previous version of Gorgon as a file system.  The scenario is the same
    /// as loading a zip file:  Load the provider plug-in into the file system, and mount the packed
    /// file.
    /// </remarks>
    public partial class MainForm
        : Form
    {
        #region Variables.
        private GorgonFileSystem _fileSystem;		    // The file system.
        private GorgonGraphics _graphics;			    // The graphics interface.		
        private Gorgon2D _2D;						    // The 2D renderer interface.
        private GorgonTexture2D _spriteImage;		    // The sprite image texture.
        private GorgonFont _textFont;				    // Font for text display.
        private GorgonFont _helpFont;				    // Font for help screen.
        private IList<GorgonSprite> _sprites;		    // Sprites.
        private GorgonText _helpText;				    // Help text.
        private GorgonText _poetry;					    // Poetry text.
        private Vector2 _textPosition = Vector2.Zero;	// Text position.
        private float _blurDelta = -2.0f;				// Blur delta.
        private bool _showHelp = true;					// Flag to show help.
        private bool _showStats;					    // Show rendering statistics.	
        #endregion

        #region Methods.
        /// <summary>
        /// Function to handle idle time processing.
        /// </summary>
        /// <returns>TRUE to continue processing, FALSE to stop.</returns>
        private bool Idle()
        {
            int width = ClientSize.Width;
			int height = ClientSize.Height;

            _2D.Clear(Color.FromArgb(250, 245, 220));

            // Reset the text position.
            if (_poetry.Position.Y < -_poetry.Size.Y)
            {
                _textPosition = new Vector2(0, height + _textFont.LineHeight);
            }

            // Scroll up.
            _textPosition.Y -= (25.0f * GorgonTiming.Delta);

            // Alter blur value.
            _2D.Effects.GaussianBlur.BlurAmount += _blurDelta * GorgonTiming.Delta;
            if (_2D.Effects.GaussianBlur.BlurAmount < 4.5f)
            {
                _2D.Effects.GaussianBlur.BlurAmount = 4.5f;
                _blurDelta = -_blurDelta;
            }

            if (_2D.Effects.GaussianBlur.BlurAmount > 13.0f)
            {
                _2D.Effects.GaussianBlur.BlurAmount = 13.0f;
                _blurDelta = -_blurDelta;
            }

            // Draw text.
            _poetry.Position = _textPosition;
            _poetry.Draw();

            // Draw the base.
            _sprites[0].Position = new Vector2(width / 4, height / 4);
            _sprites[0].Draw();

            // Draw motherships.
            _sprites[1].Position = new Vector2(width - (width / 4), height / 4);
            _sprites[1].Draw();

            // Draw the blurred sprite.
			// Draw the blurred sprite.
			_2D.Effects.GaussianBlur.Render();

			// Copy the target onto the screen (scaled to the original size of the sprite).
			// We scale the blit because the render targets used for blurring are much smaller
			// than the actual sprite.  If we didn't scale, the sprite would be clipped.
			_2D.Drawing.Blit(_2D.Effects.GaussianBlur.Output, new RectangleF(width / 2 - _sprites[2].Size.X / 2.0f,
																					height / 2 - _sprites[2].Size.Y / 2.0f,
																					_sprites[2].Size.X,
																					_sprites[2].Size.Y));

            // Draw help text.
            if (_showHelp)
            {
                _helpText.Draw();
            }

            // Show our rendering statistics.
            if (_showStats)
            {
	            RectangleF rectPosition = new RectangleF(0, 0, width, (_helpFont.FontHeight * 2.0f) + 2.0f);
                _2D.Drawing.FilledRectangle(rectPosition, Color.FromArgb(192, Color.Black));
	            _2D.Drawing.DrawLine(new Vector2(rectPosition.X, rectPosition.Bottom), new Vector2(rectPosition.Width, rectPosition.Bottom), Color.White);
                _2D.Drawing.DrawString(_helpFont, string.Format("FPS: {0}\nFrame Delta: {1}ms.", GorgonTiming.FPS.ToString("0.0"), (GorgonTiming.Delta * 1000).ToString("0.0##")), Vector2.Zero, Color.White);
            }

            _2D.Render();
            return true;
        }

        /// <summary>
        /// Function called to initialize the application.
        /// </summary>
        private void Initialize()
        {
            // Resize and center the screen.
            var screen = Screen.FromHandle(Handle);
            ClientSize = Settings.Default.Resolution;
            Location = new Point(screen.Bounds.Left + screen.WorkingArea.Width / 2 - ClientSize.Width / 2, screen.Bounds.Top + screen.WorkingArea.Height / 2 - ClientSize.Height / 2);

            // Initialize our graphics.
            _graphics = new GorgonGraphics();
            _2D = _graphics.Output.Create2DRenderer(this, ClientSize.Width, ClientSize.Height, BufferFormat.R8G8B8A8_UIntNormal, Settings.Default.IsWindowed);

            // Show the logo because I'm insecure.
            _2D.IsLogoVisible = true;

            // Create fonts.
            _textFont = _graphics.Fonts.CreateFont("GiGi_24pt", new GorgonFontSettings
                {
                FontFamilyName = "GiGi",
                AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                Size = 24.0f,
                FontHeightMode = FontHeightMode.Points,
                TextureSize = new Size(512, 256)
            });
			

            // Use the form font for this one.
            _helpFont = _graphics.Fonts.CreateFont("FormFont", new GorgonFontSettings
                {
                FontFamilyName = Font.FontFamily.Name,
                FontStyle = FontStyle.Bold,
                AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                Size = Font.Size,
                FontHeightMode = FontHeightMode.Points
            });

            // Load the Gorgon BZip packed file provider plug-in assembly.
            GorgonApplication.PlugIns.LoadPlugInAssembly(Program.PlugInPath + "Gorgon.FileSystem.GorPack.dll");            

            // Create our file system and mount the resources.
            _fileSystem = new GorgonFileSystem();
            
            // Add the Gorgon BZip packed file provider to the file system.
            _fileSystem.Providers.LoadProvider("GorgonLibrary.IO.GorgonGorPackPlugIn");

            // Mount the packed file.
            _fileSystem.Mount(Program.GetResourcePath(@"BZipFileSystem.gorPack"));

			// Get the sprite image.            
			_spriteImage = _graphics.Textures.FromMemory<GorgonTexture2D>("/Images/0_HardVacuum.png", _fileSystem.ReadFile("/Images/0_HardVacuum.png"), new GorgonCodecPNG());

            // Get the sprites.
            // The sprites in the file system are from version 1.0 of Gorgon.
            // This version is backwards compatible and can load any version
            // of the sprites produced by older versions of Gorgon.
            _sprites = new GorgonSprite[3];
            _sprites[0] = _2D.Renderables.FromMemory<GorgonSprite>("Base", _fileSystem.ReadFile("/Sprites/base.gorSprite"));
            _sprites[1] = _2D.Renderables.FromMemory<GorgonSprite>("Mother", _fileSystem.ReadFile("/Sprites/Mother.gorSprite"));
            _sprites[2] = _2D.Renderables.FromMemory<GorgonSprite>("Mother2c", _fileSystem.ReadFile("/Sprites/Mother2c.gorSprite"));

            // Get poetry.            
            _textPosition = new Vector2(0, ClientSize.Height + _textFont.LineHeight);
            _poetry = _2D.Renderables.CreateText("Poetry", _textFont, Encoding.UTF8.GetString(_fileSystem.ReadFile("/SomeText.txt")), Color.Black);
            _poetry.Position = _textPosition;

            // Set up help text.
            _helpText = _2D.Renderables.CreateText("Help", _helpFont, "F1 - Show/hide this help text.\nS - Show frame statistics.\nESC - Exit.", Color.Blue);
            _helpText.Position = new Vector2(3, 3);

            // Set the initial blur value.
            // We set a small render target for the blur, this will help
            // speed up the effect.
            _2D.Effects.GaussianBlur.BlurAmount = 13.0f;
            _2D.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(128, 128);
			_2D.Effects.GaussianBlur.RenderScene = pass =>
			{
				// Draw the sprite at the upper left corner instead of
				// centered.  Otherwise it'll be centered in the blur 
				// render target and will be clipped.
				_sprites[2].Anchor = Vector2.Zero;
				_sprites[2].Position = Vector2.Zero;
				// Scale to the size of the blur target.
				_sprites[2].Scale = new Vector2(1.0f, _2D.Effects.GaussianBlur.BlurRenderTargetsSize.Height / _sprites[2].Size.Y);
				// Adjust the texture size to avoid bleed when blurring.  
				// Bleed means that other portions of the texture get pulled
				// in to the texture because of bi-linear filtering (and the
				// blur operates in a similar manner, and therefore unwanted
				// pixels get pulled in as well).
				// See http://tape-worm.net/?page_id=277 for more info.
				_sprites[2].TextureSize = new Vector2(125.0f / _spriteImage.Settings.Width, _sprites[2].TextureSize.Y);

				_sprites[2].Draw();

				// Reset.
				_sprites[2].TextureSize = new Vector2(128.0f / _spriteImage.Settings.Width, _sprites[2].TextureSize.Y);
			};

            GorgonApplication.ApplicationIdleLoopMethod = Idle;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

	        if (e.KeyCode == Keys.Escape)
	        {
		        GorgonApplication.Quit();
	        }

            if (e.KeyCode == Keys.F1)
            {
                _showHelp = !_showHelp;
            }

            if (e.KeyCode == Keys.S)
            {
                _showStats = !_showStats;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                Cursor = Cursors.WaitCursor;

                // Initialize.
                Initialize();
            }
            catch (Exception ex)
            {
                GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(this, ex));
                GorgonApplication.Quit();
            }
            finally
            {
                Cursor = Cursors.Default;
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