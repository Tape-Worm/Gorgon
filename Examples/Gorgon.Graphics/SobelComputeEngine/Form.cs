
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 3, 2017 12:44:06 PM
// 


using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// The main application window
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
public partial class Form
    : System.Windows.Forms.Form
{

    // The graphics interface for the application.
    private GorgonGraphics _graphics;
    // The renderer used to display our textures.
    private GraphicsRenderer _renderer;
    // The texture that we will process.
    private GorgonTexture2DView _sourceTexture;
    // The output texture.
    private GorgonTexture2D _outputTexture;
    // The sobel edge detection functionality.
    private Sobel _sobel;
    // The shader used to render the sobel edge detection.
    private GorgonComputeShader _sobelShader;
    // The output view for the output texture.
    private GorgonTexture2DView _outputView;
    // The output uav for the output texture.
    private GorgonTexture2DReadWriteView _outputUav;



    /// <summary>
    /// Handles the Click event of the ButtonImagePath control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonImagePath_Click(object sender, EventArgs e)
    {
        Cursor.Current = Cursors.WaitCursor;

        IGorgonImage image = null;
        GorgonCodecPng png = new();

        try
        {
            if (DialogOpenPng.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            TextImagePath.Text = DialogOpenPng.FileName;
            _sourceTexture?.Texture?.Dispose();
            _outputTexture?.Dispose();
            _sourceTexture = null;
            _outputTexture = null;

            image = png.FromFile(DialogOpenPng.FileName);
            _sourceTexture = image.BeginUpdate()
                                  .ConvertToFormat(BufferFormat.R8G8B8A8_UNorm)
                                  .EndUpdate()
                                  .ToTexture2D(_graphics,
                                               new GorgonTexture2DLoadOptions
                                               {
                                                   Name =
                                                       Path.GetFileNameWithoutExtension(DialogOpenPng.FileName)
                                               }).GetShaderResourceView();

            _outputTexture = new GorgonTexture2D(_graphics,
                                                 new GorgonTexture2DInfo(_sourceTexture)
                                                 {
                                                     Name = "Output",
                                                     Format = BufferFormat.R8G8B8A8_Typeless,
                                                     Binding = TextureBinding.ShaderResource | TextureBinding.ReadWriteView
                                                 });

            // Get an SRV for the output texture so we can render it later.
            _outputView = _outputTexture.GetShaderResourceView(BufferFormat.R8G8B8A8_UNorm);

            // Get a UAV for the output.
            _outputUav = _outputTexture.GetReadWriteView(BufferFormat.R32_UInt);

            // Process the newly loaded texture.
            _sobel.Process(_sourceTexture, _outputUav, TrackThickness.Value, TrackThreshold.Value / 100.0f);

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
        _renderer.Render(_sourceTexture, _outputView);

        return true;
    }

    /// <summary>
    /// Handles the ValueChanged event of the TrackThreshold control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void TrackThreshold_ValueChanged(object sender, EventArgs e)
    {
        if (_sourceTexture is null)
        {
            return;
        }

        Cursor.Current = Cursors.WaitCursor;

        try
        {
            // Unfortunately, because the two interfaces are disconnected, we need to ensure that we're not using a resource on the graphics side 
            // when we want access to it on the compute side. This needs to be fixed as it can be a bit on the slow side to constantly do this 
            // every frame.
            _sobel.Process(_sourceTexture, _outputUav, TrackThickness.Value, TrackThreshold.Value / 100.0f);

            GorgonCodecPng png = new();
            using GorgonTexture2D tempTexture = new(_graphics, new GorgonTexture2DInfo(_outputTexture)
            {
                Format = BufferFormat.R8G8B8A8_UNorm
            });

            _outputTexture.CopyTo(tempTexture);
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
    /// <param name="e">A <see cref="FormClosingEventArgs" /> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        GorgonApplication.IdleMethod = null;

        _sobel?.Dispose();
        _sobelShader?.Dispose();
        _outputTexture?.Dispose();
        _sourceTexture?.Texture?.Dispose();
        _renderer?.Dispose();
        _graphics?.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        Cursor.Current = Cursors.WaitCursor;

        try
        {
            // Find out which devices we have installed in the system.
            IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

            if (deviceList.Count == 0)
            {
                GorgonDialogs.ErrorBox(this, "There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                GorgonApplication.Quit();
                return;
            }

            _graphics = new GorgonGraphics(deviceList[0]);
            _renderer = new GraphicsRenderer(_graphics);
            _renderer.SetPanel(PanelDisplay);

            // Load the compute shader.
#if DEBUG
            _sobelShader = GorgonShaderFactory.Compile<GorgonComputeShader>(_graphics, Resources.ComputeShader, "SobelCS", true);
#else
            _sobelShader = GorgonShaderFactory.Compile<GorgonComputeShader>(_graphics, Resources.ComputeShader, "SobelCS");
#endif
            _sobel = new Sobel(_graphics, _sobelShader);

            GorgonExample.LoadResources(_graphics);

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



    /// <summary>
    /// Initializes a new instance of the <see cref="Form"/> class.
    /// </summary>
    public Form() => InitializeComponent();

}
