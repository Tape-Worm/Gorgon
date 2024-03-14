#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: March 31, 2018 1:12:38 PM
// 
#endregion

using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// This example focuses on Gorgon's Image library.
/// 
/// The image library is meant to hold 1D, 2D and 3D image data in memory. It is laid out much like a Direct 3D texture in that it supports image arrays, 
/// mip maps, image cubes and depth images.  It also supports many different image pixel layout formats like R8, R8G8B8A8_*, etc... 
/// 
/// The image manipulation functionality is built on a fluent interface, so that you can chain operations together like this:
/// image.Resize(160, 100, 1).Crop(new SharpDX.Rectangle(120, 80, 40, 40), 1)
/// 
/// Users can create their own image and modify the image data directly (provided they know the format of data in the image) by manipulating the buffers 
/// of the image. These buffers are indexed by mip level, and array (or depth) level, and have their own size (in the case of mip maps). By accessing the 
/// Data property (which is a GorgonNativeBuffer), users can manipulate image data at a byte level.
/// 
/// Gorgon also supports image codecs that load, and save image data.  It supports 6 formats out of the box:
/// Windows Bitmap (bmp)
/// Portable Network Graphics (png)
/// Joint Photographers Experts Group (jpeg)
/// Targa (tga)
/// Direct Draw Surface (dds)
/// Graphics Interface Format (gif) - This one supports animation.
/// 
/// Users can also add their own codecs by using the GorgonImageCodecPlugIn.  However, this is outside of the scope for this example and is demonstrated 
/// in the TvImageCodec example.
/// 
/// This example also includes code to show how to load and animate an animated GIF using a background task.
/// 
/// The ImageGallery and GifAnimator classes contain the relevant code for this example.
/// </summary>
public partial class Form
    : System.Windows.Forms.Form
{
    #region Variables.
    // The gallery used to display our images.
    private ImageGallery _gallery;
    // The animator for the GIF file.
    private GifAnimator _gifAnim;
    // The graphics context for the form.
    private System.Drawing.Graphics _graphics;
    #endregion

    #region Methods.
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.</summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        _gifAnim = new GifAnimator(SynchronizationContext.Current);
        _gallery = new ImageGallery(Font, DeviceDpi, _gifAnim);

        try
        {
            Cursor.Current = Cursors.WaitCursor;

            // Load our image data.
            _gallery.LoadImages(GorgonApplication.StartupPath.FullName);

            _graphics = CreateGraphics();

            // Begin our animation.
            _gifAnim.Animate(() => _gallery.RefreshGif(_graphics));
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

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize" /> event.</summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        Refresh();
        _gifAnim?.Reset();
    }

    /// <summary>
    /// Handles the Paint event of the ContentArea control.
    /// </summary>
    /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
    protected override void OnPaint(PaintEventArgs e)
    {
        _gallery?.DrawGallery(e.Graphics, ClientSize);
        _gifAnim?.Reset();
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs" /> that contains the event data. </param>
    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        if (_gifAnim is null)
        {
            return;
        }

        try
        {
            Cursor.Current = Cursors.WaitCursor;

            // Cancel form closure because we need to wait until the thread is done prior to moving on.
            e.Cancel = true;
            try
            {
                await _gifAnim.CancelAsync();
            }
            catch (OperationCanceledException)
            {
                // Do nothing, this could happen in Task.Delay.
            }

            e.Cancel = false;

            _graphics.Dispose();
            _gallery.Dispose();
            _gifAnim = null;
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
        Close();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="Form"/> class.
    /// </summary>
    public Form() => InitializeComponent();
    #endregion
}
