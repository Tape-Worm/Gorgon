
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: Tuesday, September 18, 2012 8:01:47 PM
// 

using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.UI.WindowsForms;
using DrawingGraphics = System.Drawing.Graphics;

namespace Gorgon.Examples;

/// <summary>
/// Our form for the example
/// </summary>
public partial class Form : System.Windows.Forms.Form
{

    // GDI+ form graphics interface.
    private DrawingGraphics? _formGraphics;
    // GDI+ graphics interface.
    private DrawingGraphics? _graphics;
    // Image for our form.
    private Bitmap? _bitmap;

    /// <summary>
    /// Property to set or return the log for the application.
    /// </summary>
    public required IGorgonLog Log
    {
        get;
        init;
    }

    /// <summary>
    /// Property to return the size of the graphics canvas.
    /// </summary>
    public Size GraphicsSize => _bitmap?.Size ?? Size.Empty;

    /// <inheritdoc/>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            _formGraphics = PanelGraphics.CreateGraphics();
            _bitmap = new Bitmap(PanelGraphics.ClientSize.Width, PanelGraphics.ClientSize.Height, _formGraphics);
            _graphics = DrawingGraphics.FromImage(_bitmap);
            _graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            _graphics.Clear(Color.Black);
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.Error(this, e), Log);
        }
    }

    /// <inheritdoc/>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (e.KeyCode == Keys.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);

        try
        {
            if (_bitmap is null)
            {
                return;
            }

            _formGraphics?.Dispose();
            _graphics?.Dispose();
            _bitmap?.Dispose();
            _bitmap = null;
            _graphics = null;

            _formGraphics = PanelGraphics.CreateGraphics();
            _bitmap = new Bitmap(PanelGraphics.ClientSize.Width, PanelGraphics.ClientSize.Height, _formGraphics);
            _graphics = DrawingGraphics.FromImage(_bitmap);
            _graphics.Clear(Color.Black);
        }
        catch (Exception ex)
        {
            ex.Handle(e => GorgonDialogs.Error(this, e), Log);
        }
    }

    /// <inheritdoc/>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        _graphics?.Dispose();

        _formGraphics?.Dispose();

        _bitmap?.Dispose();
    }

    /// <summary>
    /// Function to draw a dot (or line) on the screen.
    /// </summary>
    /// <param name="lastX">Last horizontal coordinate.</param>
    /// <param name="lastY">Last vertical coordinate.</param>
    /// <param name="x">Horizontal coordinate.</param>
    /// <param name="y">Vertical coordinate.</param>
    /// <param name="color">Color of the pixel.</param>
    public void Draw(int lastX, int lastY, int x, int y, Color color)
    {
        if ((_bitmap is null) || (_graphics is null))
        {
            return;
        }

        if ((lastX == x) && (lastY == y))
        {
            _bitmap.SetPixel(x, y, color);
        }
        else
        {
            using Pen pen = new(color);
            _graphics.DrawLine(pen, new Point(lastX, lastY), new Point(x, y));
        }
    }

    /// <summary>
    /// Function to draw the FPS on the screen.
    /// </summary>
    /// <param name="fps">Frames per second.</param>
    public void DrawFPS(string fps) => LabelFps.Text = fps;

    /// <summary>
    /// Function to perform a page flip.
    /// </summary>
    public void Flip()
    {
        if ((_bitmap is null) || (_formGraphics is null))
        {
            return;
        }

        _formGraphics.DrawImageUnscaled(_bitmap, 0, 0, _bitmap.Width, _bitmap.Height);
    }

    /// <summary>
    /// Function to clear the buffer.
    /// </summary>
    public void Clear() => _graphics?.Clear(Color.Black);

    /// <summary>
    /// Initializes a new instance of the <see cref="Form" /> class.
    /// </summary>
    public Form() => InitializeComponent();
}
