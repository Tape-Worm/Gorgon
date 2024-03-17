
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Thursday, January 10, 2013 8:33:49 AM
// 


using System.Drawing.Imaging;
using DrawingGraphics = System.Drawing.Graphics;

namespace Gorgon.Examples;

/// <summary>
/// This will display our mouse cursor on a double buffered surface
/// </summary>
internal class MouseCursor
    : IDisposable
{

    // Flag to indicate that the object was disposed.
    private bool _disposed;
    // Buffered graphics context.
    private BufferedGraphicsContext _graphicsContext;
    // Buffered graphics page.
    private BufferedGraphics _buffer;
    // Image to use for double buffering our mouse.
    private Image _mouseImage;
    // Graphics interface for the mouse double buffer image.
    private DrawingGraphics _imageGraphics;
    // GDI+ graphics interface.
    private DrawingGraphics _graphics;
    // Color to clear the surface with.
    private readonly Color _clearColor;



    /// <summary>
    /// Property to set or return the cursor hot spot.
    /// </summary>
    public Point Hotspot
    {
        get;
        set;
    }



    /// <summary>
    /// Function to clean up any objects that are allocating memory.
    /// </summary>
    private void CleanUp()
    {
        if (_buffer is not null)
        {
            _buffer.Dispose();
            _buffer = null;
        }

        if (_graphicsContext is not null)
        {
            _graphicsContext.Dispose();
            _graphicsContext = null;
        }

        if (_graphics is not null)
        {
            _graphics.Dispose();
            _graphics = null;
        }

        if (_imageGraphics is not null)
        {
            _imageGraphics.Dispose();
            _imageGraphics = null;
        }

        if (_mouseImage is null)
        {
            return;
        }

        _mouseImage.Dispose();
        _mouseImage = null;
    }

    /// <summary>
    /// Function to create the double buffer surface resources.
    /// </summary>
    /// <param name="displayControl">The control that will be used as the display.</param>
    private void CreateDoubleBufferSurface(Control displayControl)
    {
        _graphics = DrawingGraphics.FromHwnd(displayControl.Handle);

        _mouseImage = new Bitmap(displayControl.ClientSize.Width, displayControl.ClientSize.Height, PixelFormat.Format32bppArgb);
        _imageGraphics = DrawingGraphics.FromImage(_mouseImage);

        _graphicsContext = BufferedGraphicsManager.Current;
        _buffer = _graphicsContext.Allocate(_imageGraphics, new Rectangle(0, 0, _mouseImage.Width, _mouseImage.Height));
    }

    /// <summary>
    /// Handles the Resize event of the displayControl control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void DisplayControl_Resize(object sender, EventArgs e)
    {
        Control displayControl = (Control)sender;
        System.Windows.Forms.Form ownerForm = displayControl.FindForm();

        CleanUp();

        if ((ownerForm is not null) && (ownerForm.WindowState != FormWindowState.Minimized))
        {
            CreateDoubleBufferSurface(displayControl);
        }
    }

    /// <summary>
    /// Function to draw the mouse cursor.
    /// </summary>
    /// <param name="position">Position of the mouse cursor.</param>
    /// <param name="cursor">The image used for the mouse cursor.</param>
    /// <param name="additionalBuffer">Image to copy into the buffer before the cursor is displayed.</param>
    public void DrawMouseCursor(Point position, Image cursor, Image additionalBuffer)
    {
        _buffer.Graphics.Clear(_clearColor);
        if (additionalBuffer is not null)
        {
            _buffer.Graphics.DrawImage(additionalBuffer, Point.Empty);
        }

        _buffer.Graphics.DrawImage(cursor, new Point(position.X + Hotspot.X, position.Y + Hotspot.Y));
        _buffer.Render(_graphics);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            CleanUp();
        }

        _disposed = true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MouseCursor" /> class.
    /// </summary>
    /// <param name="displayControl">The control that acts as our display area.</param>
    public MouseCursor(Control displayControl)
    {
        CreateDoubleBufferSurface(displayControl);

        _clearColor = displayControl.BackColor;
        displayControl.Resize += DisplayControl_Resize;
    }
}
