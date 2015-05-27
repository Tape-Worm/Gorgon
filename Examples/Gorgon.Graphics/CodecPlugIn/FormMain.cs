#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, November 03, 2014 8:48:53 PM
// 
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gorgon;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Example.Properties;
using Gorgon.IO;
using Gorgon.Renderers;
using Gorgon.UI;
using SlimMath;

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
        // Our 2D graphics interface.
        private Gorgon2D _2D;
        // Image to display, loaded from our plug-in.
        private GorgonTexture2D _image;
		// Our custom codec loaded from the plug-in.
	    private GorgonImageCodec _customCodec;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to draw the image to the screen.
        /// </summary>
        private void Draw()
        {
            if (_image == null)
            {
                return;
            }

            Vector2 windowSize = ClientSize;
            Vector2 imageSize = _image.Settings.Size;
            Vector2 newSize;
            Vector2 position;

            // Calculate the scale between the images.
            var scale = new Vector2(windowSize.X / imageSize.X, windowSize.Y / imageSize.Y);

            // Only scale on a single axis if we don't have a 1:1 aspect ratio.
            if (scale.Y > scale.X)
            {
                scale.Y = scale.X;
            }
            else
            {
                scale.X = scale.Y;
            }

            // Scale the image.
            Vector2.Modulate(ref scale, ref imageSize, out newSize);

            // Set up to center the window.
            Vector2.Divide(ref windowSize, 2.0f, out windowSize);
            Vector2.Divide(ref newSize, 2.0f, out imageSize);

            // Find the position.
            Vector2.Subtract(ref windowSize, ref imageSize, out position);

            // Now draw the image.
            _2D.Drawing.BlendingMode = BlendingMode.Modulate;
            _2D.Drawing.SmoothingMode = SmoothingMode.Smooth;
            _2D.Drawing.Blit(_image, new RectangleF((Point)position, (Size)newSize), new RectangleF(0, 0, 1, 1));
        }

		/// <summary>
		/// Function to load our useless image codec plug-in.
		/// </summary>
		/// <returns><c>true</c> if successful, <c>false</c> if not.</returns>
	    private bool LoadCodec()
		{
			// Load our plug-in.
			string plugInPath = GorgonApplication.ApplicationDirectory + "TVImageCodec.dll";

			if (!File.Exists(plugInPath))
			{
				return false;
			}

			if (!GorgonApplication.PlugIns.IsPlugInAssembly(plugInPath))
			{
				return false;
			}

			GorgonApplication.PlugIns.LoadPlugInAssembly(plugInPath);

			// Get the plug-in object.
			var plugIn = GorgonApplication.PlugIns["GorgonLibrary.Graphics.Example.TvImageCodecPlugIn"] as GorgonCodecPlugIn;

			if (plugIn == null)
			{
				return false;
			}

			// Create the codec.
			_customCodec = plugIn.CreateCodec();

			return _customCodec != null;
		}

		/// <summary>
		/// Function to convert the image to use our custom codec.
		/// </summary>
		private void ConvertImage()
		{
			// The path to our image file for our custom codec.
			string tempPath = Path.ChangeExtension(Path.GetTempPath().FormatDirectory(Path.DirectorySeparatorChar) + Path.GetRandomFileName(), "Useless");
			// The new texture holding image data read by the custom codec.
			GorgonTexture2D newTexture = null;

			try
			{
				// Save the current texture using our useless new custom codec.
				_image.Save(tempPath, _customCodec);

				newTexture = _graphics.Textures.FromFile<GorgonTexture2D>("UselessTexture", tempPath, _customCodec);

				// Free the old texture and assign to the new one.
				_image.Dispose();
				_image = newTexture;
			}
			catch
			{
				// Clean up the new texture should we have an exception (this shouldn't happen, better safe than sorry).
				if (newTexture != null)
				{
					newTexture.Dispose();
				}

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
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Set up the graphics interface.
                _graphics = new GorgonGraphics();

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


				// Load the custom codec.
	            if (!LoadCodec())
	            {
					GorgonDialogs.ErrorBox(this, "Unable to load the useless image codec plug-in.");
					GorgonApplication.Quit();
		            return;
	            }

				// Convert the image to our custom codec.
				ConvertImage();

                // Set up our idle time processing.
                GorgonApplication.ApplicationIdleLoopMethod = () =>
                                                   {
                                                       _2D.Clear(Color.White);

                                                       // Draw to the window.
                                                       Draw();

                                                       // Render with a vsync interval of 2 (typically 30 FPS).  
                                                       // We're not making an action game here.
                                                       _2D.Render(2);
                                                       return true;
                                                   };
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
