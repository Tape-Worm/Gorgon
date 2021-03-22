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
// Created: Saturday, January 19, 2013 7:32:49 PM
// 
#endregion

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.IO.Extensions;
using Gorgon.IO.Providers;
using Gorgon.Math;
using Gorgon.PlugIns;
using Gorgon.Renderers;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;
using FontStyle = Gorgon.Graphics.Fonts.FontStyle;

namespace Gorgon.Examples
{
    /// <summary>
    /// Main application form.
    /// </summary>
    /// <remarks>
    /// This example is a port of the Gorgon 1.x BZip packed file system example into Gorgon 3.x.
    /// 
    /// In this example we mount a Gorgon packed file as a virtual file system and pull in an image, 
    /// some Gorgon 2.0 sprites, the backing sprite image and some text for display.
    /// 
    /// The difference between this example and the folder file system example is that we're loading
    /// a packed file from the previous version of Gorgon as a file system.  The scenario is the same
    /// as loading a zip file:  Load the provider plug in into the file system, and mount the packed
    /// file.
    /// </remarks>
	public partial class Form
        : System.Windows.Forms.Form
    {
        #region Variables.
        // The plug in assembly cache.
        private GorgonMefPlugInCache _assemblyCache;
        // The file system.
        private GorgonFileSystem _fileSystem;
        // The graphics interface.		
        private GorgonGraphics _graphics;
        // The swap chain that represents our screen.
        private GorgonSwapChain _screen;
        // The 2D renderer interface.
        private Gorgon2D _renderer;
        // The effect for blurring our sprite.
        private Gorgon2DGaussBlurEffect _blurEffect;
        // The render target for blurring the image.
        private readonly GorgonRenderTarget2DView[] _blurredTarget = new GorgonRenderTarget2DView[2];
        // The texture to display from the render target.
        private readonly GorgonTexture2DView[] _blurredImage = new GorgonTexture2DView[2];
        // The blur amount.
        private float _blurAmount = 3.0f;
        // Font for text display.
        private GorgonFont _textFont;
        // Font for help screen.
        private GorgonFont _helpFont;
        // Sprites.
        private IList<GorgonSprite> _sprites;
        // Help text.
        private GorgonTextSprite _helpText;
        // Poetry text.
        private GorgonTextSprite _poetry;
        // Text position.
        private Vector2 _textPosition = Vector2.Zero;
        // Blur delta.
        private float _blurDelta = -16.0f;
        // Flag to show help.
        private bool _showHelp = true;
        // Show rendering statistics.	
        private bool _showStats;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load the Gorgon pack file provider plugin.
        /// </summary>
        /// <returns>The file system provider.</returns>
        private IGorgonFileSystemProvider LoadGorPackProvider()
        {
            // The Gorgon packed file provider plug in dll.
            const string gorPackDll = "Gorgon.FileSystem.GorPack.dll";
            // The name of the Gorgon packed file plugin.
            const string gorPackPlugInName = "Gorgon.IO.GorPack.GorPackProvider";

            // Like the zip file example, we'll just create the plugin infrastructure, grab the provider object 
            // and get rid of the plugin stuff since we won't need it again.
            _assemblyCache = new GorgonMefPlugInCache(GorgonApplication.Log);
            _assemblyCache.LoadPlugInAssemblies(GorgonExample.GetPlugInPath().FullName, gorPackDll);

            var plugIns = new GorgonMefPlugInService(_assemblyCache);
            return plugIns.GetPlugIn<GorgonFileSystemProvider>(gorPackPlugInName);
        }

        /// <summary>
        /// Function to reset the blur targets to the original image.
        /// </summary>
	    private void ResetBlur()
        {
            _sprites[2].Scale = new Vector2(1.0f, 1.0f);

            // Adjust the texture size to avoid bleed when blurring.  
            // Bleed means that other portions of the texture get pulled in to the texture because of bi-linear filtering (and the blur operates in a similar manner, and therefore unwanted 
            // pixels get pulled in as well).
            //
            // See http://tape-worm.net/?page_id=277 for more info.

            _sprites[2].Position = new Vector2(((_blurredImage[0].Width / 2.0f) - (_sprites[2].Size.Width / 2.0f)).FastFloor(),
                                                  ((_blurredImage[0].Height / 2.0f) - (_sprites[2].Size.Height / 2.0f)).FastFloor());

            for (int i = 0; i < _blurredTarget.Length; ++i)
            {
                _blurredTarget[i].Clear(GorgonColor.BlackTransparent);
                _graphics.SetRenderTarget(_blurredTarget[i]);
                _renderer.Begin();
                _renderer.DrawSprite(_sprites[2]);
                _renderer.End();
            }
        }

        /// <summary>
        /// Function to handle idle time processing.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;

            _screen.RenderTargetView.Clear(Color.FromArgb(250, 245, 220));

            // Reset the text position.
            if (_poetry.Position.Y < -_poetry.Size.Height)
            {
                _textPosition = new Vector2(0, height + _textFont.LineHeight);
            }

            // Scroll up.
            _textPosition.Y -= (25.0f * GorgonTiming.Delta);

            // Alter blur value.
            _blurAmount += _blurDelta * GorgonTiming.Delta;

            if (_blurAmount < 0.0f)
            {
                _blurAmount = 0.0f;
                _blurDelta = -_blurDelta;
            }

            if (_blurAmount > 64)
            {
                _blurAmount = 64;
                _blurDelta = -_blurDelta;
            }

            int index = 0;
            ResetBlur();

            if (_blurAmount > 0)
            {
                // Blur for the count we specify.
                int blurCount = (int)_blurAmount;
                for (int i = 0; i < blurCount; ++i)
                {
                    int imageIndex = index = i % 2;
                    int targetIndex = index == 0 ? 1 : 0;
                    _blurEffect.Render(_blurredImage[imageIndex], _blurredTarget[targetIndex]);
                }
            }

            // Switch back to our screen for rendering.
            _graphics.SetRenderTarget(_screen.RenderTargetView);


            // Draw the base.
            _renderer.Begin();

            // Draw text.
            _poetry.Position = _textPosition;
            _renderer.DrawTextSprite(_poetry);

            _sprites[0].Position = new Vector2(width / 4, height / 4);

            // Draw motherships.
            _sprites[1].Position = new Vector2(width - (width / 4), height / 4);

            _renderer.DrawSprite(_sprites[0]);
            _renderer.DrawSprite(_sprites[1]);

            // Draw our blurred image (we could have used a sprite here as well, but this works just as well).
            _renderer.DrawFilledRectangle(new DX.RectangleF((width / 2) - (_blurredImage[0].Width / 2.0f),
                                                            (height / 2) - (_blurredImage[0].Height / 2.0f),
                                                            _blurredImage[0].Width,
                                                            _blurredImage[0].Height),
                                          GorgonColor.White,
                                          _blurredImage[index],
                                          new DX.RectangleF(0, 0, 1, 1));

            // Draw help text.
            if (_showHelp)
            {
                _renderer.DrawTextSprite(_helpText);
            }

            _renderer.End();

            GorgonExample.ShowStatistics = _showStats;
            GorgonExample.DrawStatsAndLogo(_renderer);

            _screen.Present(1);
            return true;
        }

        /// <summary>
        /// Function to load a sprite from the file system.
        /// </summary>
        /// <param name="path">Path to the file to load.</param>
        /// <returns>A byte array containing the data for a file from the file system.</returns>
        private byte[] LoadFile(string path)
        {
            IGorgonVirtualFile file = _fileSystem.GetFile(path);

            if (file is null)
            {
                throw new FileNotFoundException($"The file '{path}' was not found in the file system.");
            }

            using Stream stream = file.OpenStream();
            byte[] result = new byte[stream.Length];

            stream.Read(result, 0, result.Length);

            return result;
        }

        /// <summary>
        /// Function called to initialize the application.
        /// </summary>
        private void Initialize()
        {
            GorgonExample.PlugInLocationDirectory = new DirectoryInfo(ExampleConfig.Default.PlugInLocation);
            GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

            // Resize and center the screen.
            var screen = Screen.FromHandle(Handle);
            ClientSize = new Size(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height);
            Location = new Point(screen.Bounds.Left + (screen.WorkingArea.Width / 2) - (ClientSize.Width / 2),
                                 screen.Bounds.Top + (screen.WorkingArea.Height / 2) - (ClientSize.Height / 2));

            // Initialize our graphics.
            IReadOnlyList<IGorgonVideoAdapterInfo> videoAdapters = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);

            if (videoAdapters.Count == 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "Gorgon requires at least a Direct3D 11.2 capable video device.\nThere is no suitable device installed on the system.");
            }

            // Find the best video device.
            _graphics = new GorgonGraphics(videoAdapters.OrderByDescending(item => item.FeatureSet).First());

            // Build our "screen".
            _screen = new GorgonSwapChain(_graphics,
                                          this,
                                          new GorgonSwapChainInfo(ClientSize.Width, ClientSize.Height, BufferFormat.R8G8B8A8_UNorm));

            if (!ExampleConfig.Default.IsWindowed)
            {
                // Go full screen by using borderless windowed mode.
                _screen.EnterFullScreen();
            }

            // Build up our 2D renderer.
            _renderer = new Gorgon2D(_graphics);

            // Load in the logo texture from our resources.
            GorgonExample.LoadResources(_graphics);

            // Create fonts.
            _textFont = GorgonExample.Fonts.GetFont(new GorgonFontInfo("GiGi", 24.0f, FontHeightMode.Points, "GiGi_24pt")
            {
                AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                TextureWidth = 512,
                TextureHeight = 256
            });

            // Use the form font for this one.
            _helpFont = GorgonExample.Fonts.GetFont(new GorgonFontInfo(Font.FontFamily.Name,
                                                                Font.Size,
                                                                Font.Unit == GraphicsUnit.Pixel ? FontHeightMode.Pixels : FontHeightMode.Points,
                                                                "Form Font")
            {
                AntiAliasingMode = FontAntiAliasMode.AntiAlias,
                FontStyle = FontStyle.Bold
            });

            // Get the Gorgon BZip packed file provider and create a file system that we can use it with.
            // Create our file system and mount the resources.
            _fileSystem = new GorgonFileSystem(LoadGorPackProvider(), GorgonApplication.Log);

            // Mount the packed file.
            _fileSystem.Mount(Path.Combine(GorgonExample.GetResourcePath(@"FileSystems").FullName, "BZipFileSystem.gorPack"));

            // In the previous versions of Gorgon, we used to load the image first, and then the sprites.
            // But in this version, we have an extension that will load the sprite textures for us.
            _sprites = new GorgonSprite[3];

            // The sprites are in the v2 format.
            IEnumerable<IGorgonSpriteCodec> v2Codec = new[] { new GorgonV2SpriteCodec(_renderer) };
            IEnumerable<IGorgonImageCodec> pngCodec = new[] { new GorgonCodecPng() };
            _sprites[0] = _fileSystem.LoadSpriteFromFileSystem(_renderer, "/Sprites/base.gorSprite", spriteCodecs: v2Codec, imageCodecs: pngCodec);
            _sprites[1] = _fileSystem.LoadSpriteFromFileSystem(_renderer, "/Sprites/Mother.gorSprite", spriteCodecs: v2Codec, imageCodecs: pngCodec);
            _sprites[2] = _fileSystem.LoadSpriteFromFileSystem(_renderer, "/Sprites/Mother2c.gorSprite", spriteCodecs: v2Codec, imageCodecs: pngCodec);

            // This is how you would get the sprites in v2 of Gorgon:
            /*_spriteImage = _graphics.Textures.FromMemory<GorgonTexture2D>("0_HardVacuum", LoadFile("/Images/0_HardVacuum.png"), new GorgonCodecPNG());

		    // Get the sprites.
		    // The sprites in the file system are from version 1.0 of Gorgon.
		    // This version is backwards compatible and can load any version
		    // of the sprites produced by older versions of Gorgon.
		    _sprites = new GorgonSprite[3];
		    _sprites[0] = _renderer.Renderables.FromMemory<GorgonSprite>("Base", LoadFile("/Sprites/base.gorSprite"));
		    _sprites[1] = _renderer.Renderables.FromMemory<GorgonSprite>("Mother", LoadFile("/Sprites/Mother.gorSprite"));
		    _sprites[2] = _renderer.Renderables.FromMemory<GorgonSprite>("Mother2c", LoadFile("/Sprites/Mother2c.gorSprite"));
            */

            // Get poetry.            
            _textPosition = new Vector2(0, ClientSize.Height + _textFont.LineHeight);

            _poetry = new GorgonTextSprite(_textFont, Encoding.UTF8.GetString(LoadFile("/SomeText.txt")))
            {
                Position = _textPosition,
                Color = Color.Black
            };

            // Set up help text.
            _helpText = new GorgonTextSprite(_helpFont, "F1 - Show/hide this help text.\nS - Show frame statistics.\nESC - Exit.")
            {
                Color = Color.Blue,
                Position = new Vector2(3, 3)
            };

            // Unlike the old example, we'll blend to render targets, ping-ponging back and forth, for a much better quality image and smoother transition.
            _blurEffect = new Gorgon2DGaussBlurEffect(_renderer, 3)
            {
                BlurRenderTargetsSize = new DX.Size2((int)_sprites[2].Size.Width * 2, (int)_sprites[2].Size.Height * 2),
                PreserveAlpha = false
            };
            _blurEffect.Precache();

            _blurredTarget[0] = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo(_blurEffect.BlurRenderTargetsSize.Width,
                                                                                                                    _blurEffect.BlurRenderTargetsSize.Height,
                                                                                                                    BufferFormat.R8G8B8A8_UNorm)
            {
                Name = "Blurred RTV",
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Default
            });
            _blurredTarget[1] = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, _blurredTarget[0]);
            _blurredImage[0] = _blurredTarget[0].GetShaderResourceView();
            _blurredImage[1] = _blurredTarget[1].GetShaderResourceView();

            GorgonApplication.IdleMethod = Idle;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
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

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.</summary>
        /// <param name="e">A <see cref="System.Windows.Forms.FormClosingEventArgs" /> that contains the event data. </param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            _blurredImage[1]?.Dispose();
            _blurredTarget[1]?.Dispose();
            _blurredImage[0]?.Dispose();
            _blurredTarget[0]?.Dispose();
            _blurEffect?.Dispose();
            _helpFont?.Dispose();
            _textFont?.Dispose();
            _renderer?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();
            _assemblyCache?.Dispose();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                Cursor = Cursors.WaitCursor;

                Visible = true;
                Application.DoEvents();

                // Initialize.
                Initialize();
            }
            catch (Exception ex)
            {
                ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
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
        public Form() => InitializeComponent();
        #endregion
    }
}