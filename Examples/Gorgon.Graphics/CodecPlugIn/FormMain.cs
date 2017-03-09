#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 5, 2017 10:33:01 PM
// 
#endregion

using System;
using System.IO;
using System.Windows.Forms;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Plugins;
using Gorgon.UI;

namespace CodecPlugIn
{
    /// <summary>
    /// Our main UI window for the example.
    /// </summary>
    public partial class FormMain : Form
    {
		#region Variables.
		// The main graphics interface.
		private GorgonGraphics _graphics;
		// The swap chain to use.
	    private GorgonSwapChain _swap;
		// Image to display, loaded from our plug-in.
		private GorgonTexture _texture;
		// The image in system memory.
	    private IGorgonImage _image;
		// Our custom codec loaded from the plug-in.
		private IGorgonImageCodec _customCodec;
		// The blitter used to draw the texture.
	    private GorgonTextureBlitter _blitter;
		#endregion

		#region Methods.
		/// <summary>
		/// Function called after the swap chain is resized.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		private void AfterSwapChainResized(object sender, EventArgs e)
		{
			_blitter.RenderTarget = _swap.RenderTargetView;
		}

		/// <summary>
		/// Function called during idle time.
		/// </summary>
		/// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
		private bool Idle()
		{
			_swap.RenderTargetView.Clear(GorgonColor.White);

			var windowSize = new DX.Size2F(ClientSize.Width, ClientSize.Height);
			var imageSize = new DX.Size2F(_texture.Info.Width, _texture.Info.Height);

			// Calculate the scale between the images.
			var scale = new DX.Size2F(windowSize.Width / imageSize.Width, windowSize.Height / imageSize.Height);

			// Only scale on a single axis if we don't have a 1:1 aspect ratio.
			if (scale.Height > scale.Width)
			{
				scale.Height = scale.Width;
			}
			else
			{
				scale.Width = scale.Height;
			}

			// Scale the image.
			var size = new DX.Size2((int)(scale.Width * imageSize.Width), (int)(scale.Height * imageSize.Height));

			// Find the position.
			var location = new DX.Point((int)(windowSize.Width / 2 - size.Width / 2), (int)(windowSize.Height / 2 - size.Height / 2));

			_blitter.Blit(_texture, location.X, location.Y, size.Width, size.Height);

			_swap.Present();

			return true;
		}

		/// <summary>
		/// Function to load our useless image codec plug-in.
		/// </summary>
		/// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
	    private bool LoadCodec()
		{
			const string pluginName = "Gorgon.Graphics.Example.TvImageCodecPlugIn";

			using (GorgonPluginAssemblyCache pluginAssemblies = new GorgonPluginAssemblyCache(GorgonApplication.Log))
			{
				// Load our plug-in.
				string plugInPath = GorgonApplication.StartupPath + "TVImageCodec.dll";

				if (!File.Exists(plugInPath))
				{
					return false;
				}

				// Ensure that we can load this file.
				if (!pluginAssemblies.IsPluginAssembly(plugInPath))
				{
					return false;
				}

				pluginAssemblies.Load(plugInPath);

				// Activate the plugin service.
				GorgonPluginService pluginService = new GorgonPluginService(pluginAssemblies);

				// Find the plugin.
				var plugIn = pluginService.GetPlugin<GorgonImageCodecPlugIn>(pluginName);

				if (plugIn == null)
				{
					return false;
				}

				_customCodec = plugIn.CreateCodec(pluginName);
			}

			return _customCodec != null;
		}

		/// <summary>
		/// Function to convert the image to use our custom codec.
		/// </summary>
		private void ConvertImage()
		{
			// The path to our image file for our custom codec.
			string tempPath = Path.ChangeExtension(Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) + Path.GetRandomFileName(), "tvImage");

			try
			{
				// Save the current texture using our useless new custom codec.
				_customCodec.SaveToFile(_image.ConvertToFormat(DXGI.Format.R8G8B8A8_UNorm), tempPath);
				_image.Dispose();
				_texture?.Dispose();

				_image = _customCodec.LoadFromFile(tempPath);
				
				_texture = _image.ToTexture("Converted Texture", _graphics);
				
			}
			catch
			{
				// Clean up the new texture should we have an exception (this shouldn't happen, better safe than sorry).
				_image?.Dispose();
				throw;
			}
			finally
			{
				try
				{
					File.Delete(tempPath);
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// Intentionally left blank.
					// If we can't clean up the temp file, then it's no big deal right now.
				}
			}
	    }

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
	    {
		    base.OnFormClosing(e);
			
			_blitter?.Dispose();
			_texture?.Dispose();
			_swap?.Dispose();
			_graphics?.Dispose();
		    _image?.Dispose();
	    }

	    /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Cursor.Current = Cursors.WaitCursor;

            try
            {
				// Load the custom codec.
				if (!LoadCodec())
				{
					GorgonDialogs.ErrorBox(this, "Unable to load the image codec plug-in.");
					GorgonApplication.Quit();
					return;
				}


				// Set up the graphics interface.
				IGorgonVideoDeviceList devices = new GorgonVideoDeviceList();
				devices.Enumerate();

				// 
				_graphics = new GorgonGraphics(devices[0]);

	            _swap = new GorgonSwapChain("Codec Plugin SwapChain",
	                                        _graphics,
	                                        this,
	                                        new GorgonSwapChainInfo
	                                        {
		                                        Width = ClientSize.Width,
		                                        Height = ClientSize.Height,
		                                        Format = DXGI.Format.R8G8B8A8_UNorm
	                                        });
				_swap.AfterSwapChainResized += AfterSwapChainResized;

				// Load the image to use as a texture.
	            IGorgonImageCodec png = new GorgonCodecPng();
				_image = png.LoadFromFile(Program.GetResourcePath(@"Textures\CodecPlugIn\SourceTexture.png"));

				ConvertImage();

				_blitter = new GorgonTextureBlitter(_graphics, _swap.RenderTargetView);

				GorgonApplication.IdleMethod = Idle;

				/*_graphics = new GorgonGraphics();

                // Create our 2D renderer to display the image.
                _2D = _graphics.Output.Create2DRenderer(this, 1280, 800);

                // Center the window on the screen.
                Screen currentMonitor = Screen.FromControl(this);
                Location = new Point(currentMonitor.WorkingArea.Left + (currentMonitor.WorkingArea.Width / 2 - Width / 2),
                                     currentMonitor.WorkingArea.Top + (currentMonitor.WorkingArea.Height / 2 - Height / 2));

                // Load our base texture.
                _image = _graphics.Textures.FromMemory<GorgonTexture2D>("SourceTexture",
                                                                        Resources.SourceTexture,
                                                                        new GorgonCodecDDS());


	            // Convert the image to our custom codec.
	            ConvertImage();

                // Set up our idle time processing.
                GorgonApplication.IdleMethod = () =>
                                                   {
                                                       _2D.Clear(Color.White);

                                                       // Draw to the window.
                                                       Draw();

                                                       // Render with a vsync interval of 2 (typically 30 FPS).  
                                                       // We're not making an action game here.
                                                       _2D.Render(2);
                                                       return true;
                                                   };*/
			}
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
                GorgonApplication.Quit();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormMain"/> class.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
        }
        #endregion
    }
}
