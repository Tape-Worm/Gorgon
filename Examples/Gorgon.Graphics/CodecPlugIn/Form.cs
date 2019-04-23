﻿#region MIT
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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Gorgon.Examples;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.PlugIns;
using Gorgon.UI;
using DX = SharpDX;

namespace CodecPlugIn
{
    /// <summary>
    /// Our main UI window for the example.
    /// </summary>
    public partial class Form : System.Windows.Forms.Form
    {
		#region Variables.
        // The cache that holds plugin information.
        private GorgonMefPlugInCache _pluginCache;
		// The main graphics interface.
		private GorgonGraphics _graphics;
		// The swap chain to use.
	    private GorgonSwapChain _swap;
		// Image to display, loaded from our plug-in.
		private GorgonTexture2DView _texture;
		// The image in system memory.
	    private IGorgonImage _image;
		// Our custom codec loaded from the plug-in.
		private IGorgonImageCodec _customCodec;
        #endregion

		#region Methods.
		/// <summary>
		/// Function called during idle time.
		/// </summary>
		/// <returns><b>true</b> to continue execution, <b>false</b> to stop.</returns>
		private bool Idle()
		{
			_swap.RenderTargetView.Clear(GorgonColor.White);

			var windowSize = new DX.Size2F(ClientSize.Width, ClientSize.Height);
			var imageSize = new DX.Size2F(_texture.Width, _texture.Height);

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
			var bounds = new DX.Rectangle((int)((windowSize.Width / 2) - (size.Width / 2)), (int)((windowSize.Height / 2) - (size.Height / 2)), size.Width, size.Height);

            _graphics.DrawTexture(_texture, bounds);

            GorgonExample.BlitLogo(_graphics);

			_swap.Present(1);

			return true;
		}

		/// <summary>
		/// Function to load our useless image codec plug-in.
		/// </summary>
		/// <returns><b>true</b> if successful, <b>false</b> if not.</returns>
	    private bool LoadCodec()
		{
			const string pluginName = "Gorgon.Graphics.Example.TvImageCodecPlugIn";

			_pluginCache = new GorgonMefPlugInCache(GorgonApplication.Log);
			
			// Load our plug-in.
			_pluginCache.LoadPlugInAssemblies(GorgonApplication.StartupPath.FullName, "TVImageCodec.dll");

			// Activate the plugin service.
			IGorgonPlugInService pluginService = new GorgonMefPlugInService(_pluginCache);

			// Find the plugin.
		    GorgonImageCodecPlugIn plugIn = pluginService.GetPlugIn<GorgonImageCodecPlugIn>(pluginName);

			if (plugIn == null)
			{
				return false;
			}

			_customCodec = plugIn.CreateCodec(pluginName);

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
				_customCodec.SaveToFile(_image.ConvertToFormat(BufferFormat.R8G8B8A8_UNorm), tempPath);
				_image.Dispose();
				_texture?.Dispose();

				_image = _customCodec.LoadFromFile(tempPath);
				
				_texture = _image.ToTexture2D(_graphics, new GorgonTexture2DLoadOptions
				                                         {
                                                             Name = "Converted Texture"
				                                         }).GetShaderResourceView();
				
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
			
            GorgonExample.UnloadResources();

            _pluginCache?.Dispose();
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
                GorgonExample.ResourceBaseDirectory = new DirectoryInfo(Settings.Default.ResourceLocation);

				// Load the custom codec.
				if (!LoadCodec())
				{
					GorgonDialogs.ErrorBox(this, "Unable to load the image codec plug-in.");
					GorgonApplication.Quit();
					return;
				}


                // Set up the graphics interface.
                // Find out which devices we have installed in the system.
                IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

                if (deviceList.Count == 0)
                {
                    GorgonDialogs.ErrorBox(this, "There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                    GorgonApplication.Quit();
                    return;
                }

                _graphics = new GorgonGraphics(deviceList[0]);

	            _swap = new GorgonSwapChain(_graphics,
	                                        this,
	                                        new GorgonSwapChainInfo("Codec PlugIn SwapChain")
	                                        {
		                                        Width = ClientSize.Width,
		                                        Height = ClientSize.Height,
		                                        Format = BufferFormat.R8G8B8A8_UNorm
	                                        });

                _graphics.SetRenderTarget(_swap.RenderTargetView);

				// Load the image to use as a texture.
	            IGorgonImageCodec png = new GorgonCodecPng();
				_image = png.LoadFromFile(Path.Combine(GorgonExample.GetResourcePath(@"Textures\CodecPlugIn\").FullName, "SourceTexture.png"));

                GorgonExample.LoadResources(_graphics);

				ConvertImage();

				GorgonApplication.IdleMethod = Idle;
			}
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
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
        /// Initializes a new instance of the <see cref="Form"/> class.
        /// </summary>
        public Form() => InitializeComponent();
        #endregion
    }
}