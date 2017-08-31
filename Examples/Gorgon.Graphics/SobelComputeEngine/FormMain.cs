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
// Created: August 3, 2017 12:44:06 PM
// 
#endregion

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DXGI = SharpDX.DXGI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Example;
using Gorgon.Graphics.Example.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.UI;

namespace SobelComputeEngine
{
    /// <summary>
    /// The main application window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This simple application will take an input PNG file and run an edge detection filter (sobel) against it and overlay that resulting image on the 
    /// source image in another pane. 
    /// 
    /// This example uses an optimized Sobel edge detection algorithm found at http://homepages.inf.ed.ac.uk/rbf/HIPR2/sobel.htm. 
    /// 
    /// We begin this example by setting up 
    /// </para>
    /// </remarks>
    public partial class FormMain 
        : GorgonFlatForm
    {
        #region Variables.
        // The graphics interface for the application.
        private GorgonGraphics _graphics;
        // The renderer used to display our textures.
        private GraphicsRenderer _renderer;
        // The texture that we will process.
        private GorgonTexture _sourceTexture;
        // The output texture.
        private GorgonTexture _outputTexture;
        // The sobel edge detection functionality.
        private Sobel _sobel;
        // The shader used to render the sobel edge detection.
        private GorgonComputeShader _sobelShader;
        // The output view for the output texture.
        private GorgonTextureView _outputView;
        // The output uav for the output texture.
        private GorgonTextureUav _outputUav;
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the ButtonImagePath control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonImagePath_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            IGorgonImage image = null;
            GorgonCodecPng png = new GorgonCodecPng();

            try
            {
                if (DialogOpenPng.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                TextImagePath.Text = DialogOpenPng.FileName;
                _sourceTexture?.Dispose();
                _outputTexture?.Dispose();
                _sourceTexture = null;
                _outputTexture = null;

                image = png.LoadFromFile(DialogOpenPng.FileName);
                _sourceTexture = image.ConvertToFormat(DXGI.Format.R8G8B8A8_UNorm).ToTexture(Path.GetFileNameWithoutExtension(DialogOpenPng.FileName), _graphics);

                _outputTexture = new GorgonTexture("Output",
                                                   _graphics,
                                                   new GorgonTextureInfo(_sourceTexture.Info)
                                                   {
                                                       Format = DXGI.Format.R8G8B8A8_Typeless,
                                                       Binding = TextureBinding.ShaderResource | TextureBinding.UnorderedAccess
                                                   });

                // Get an SRV for the output texture so we can render it later.
                _outputView = _outputTexture.GetShaderResourceView(DXGI.Format.R8G8B8A8_UNorm);

                // Get a UAV for the output.
                _outputUav = _outputTexture.GetUnorderedAccessView(DXGI.Format.R32_UInt);

                // Process the newly loaded texture.
                _sobel.Process(_sourceTexture.DefaultShaderResourceView, _outputUav, TrackThickness.Value, TrackThreshold.Value / 100.0f);

                TrackThreshold.Enabled = TrackThickness.Enabled = true;
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
                TrackThreshold.Enabled = TrackThickness.Enabled = false;
            }
            finally
            {
                image?.Dispose();
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Function to perform the rendering for the application.
        /// </summary>
        /// <returns><b>true</b> to continue rendering, <b>false</b> to stop.</returns>
        private bool Idle()
        {
            _renderer.Render(_sourceTexture?.DefaultShaderResourceView, _outputView);

            return true;
        }

        /// <summary>
        /// Handles the ValueChanged event of the TrackThreshold control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TrackThreshold_ValueChanged(object sender, EventArgs e)
        {
            if (_sourceTexture == null)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // Unfortunately, because the two interfaces are disconnected, we need to ensure that we're not using a resource on the graphics side 
                // when we want access to it on the compute side. This needs to be fixed as it can be a bit on the slow side to constantly do this 
                // every frame.
                _sobel.Process(_sourceTexture.DefaultShaderResourceView, _outputUav, TrackThickness.Value, TrackThreshold.Value / 100.0f);
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            GorgonApplication.IdleMethod = null;

            _sobel?.Dispose();
            _sobelShader?.Dispose();
            _outputTexture?.Dispose();
            _sourceTexture?.Dispose();
            _renderer?.Dispose();
            _graphics?.Dispose();
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
                IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList();
                videoDevices.Enumerate();

                IGorgonVideoDeviceInfo videoDeviceInfo = videoDevices.FirstOrDefault(item => item.SupportedFeatureLevel >= FeatureLevelSupport.Level_11_0);

                // Do not allow us to continue further if we don't have a device capable of supporting the compute functionality.
                if (videoDeviceInfo == null)
                {
                    GorgonDialogs.ErrorBox(this, "This example requires a minimum feature level of 11.0.");
                    GorgonApplication.Quit();
                    return;
                }

                _graphics = new GorgonGraphics(videoDeviceInfo);
                _renderer = new GraphicsRenderer(_graphics);
                _renderer.SetPanel(PanelDisplay);

                // Load the compute shader.
#if DEBUG
                _sobelShader = GorgonShaderFactory.Compile<GorgonComputeShader>(_graphics.VideoDevice, Resources.ComputeShader, "SobelCS", true);
#else
                _sobelShader = GorgonShaderFactory.Compile<GorgonComputeShader>(_graphics.VideoDevice, Resources.ComputeShader, "SobelCS");
#endif
                _sobel = new Sobel(_graphics, _sobelShader);

                GorgonApplication.IdleMethod = Idle;
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
                GorgonApplication.Quit();
                Close();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        #endregion

        #region Constructor/Finalizer.
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
