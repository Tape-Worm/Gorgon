
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
// Created: Thursday, January 10, 2013 8:17:04 AM
// 


using System.Drawing.Imaging;
using Gorgon.Core;
using DrawingGraphics = System.Drawing.Graphics;

namespace Gorgon.Examples;

/// <summary>
/// This object is responsible for drawing the spray effect on the display surface
/// </summary>
internal class Spray
    : IDisposable
{

    // Graphics interface.
    private DrawingGraphics _graphics;
    // List if brushes to use.
    private readonly SolidBrush[] _brushes;



    /// <summary>
    /// Property to return the image used for the surface.
    /// </summary>
    public Image Surface
    {
        get;
        private set;
    }



    /// <summary>
    /// Function to resize the drawing area.
    /// </summary>
    public void Resize(Size newSize)
    {
        if (_graphics is not null)
        {
            _graphics.Dispose();
            _graphics = null;
        }

        var newBuffer = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format32bppArgb);
        _graphics = DrawingGraphics.FromImage(newBuffer);

        if (Surface is not null)
        {
            // Copy the old image to the new surface.
            _graphics.DrawImage(Surface, Point.Empty);
            Surface.Dispose();
            Surface = null;
        }

        // Set the new buffer as the current surface.
        Surface = newBuffer;
    }

    /// <summary>
    /// Function to randomly "spray" a point on the surface.
    /// </summary>
    /// <param name="point">Origin point for the spray.</param>
    /// <param name="erase"><b>true</b> to erase, or <b>false</b> to draw with colors.</param>
    public void SprayPoint(Point point, bool erase)
    {
        var randomArea = new Point(GorgonRandom.RandomInt32(-10, 10), GorgonRandom.RandomInt32(-10, 10));
        if (!erase)
        {
            _graphics.FillEllipse(_brushes[GorgonRandom.RandomInt32(0, _brushes.Length - 1)], new Rectangle(point.X + randomArea.X, point.Y + randomArea.Y, 10, 10));
        }
        else
        {
            _graphics.FillEllipse(Brushes.White, new Rectangle(point.X + randomArea.X, point.Y + randomArea.Y, 10, 10));
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        foreach (SolidBrush brush in _brushes)
        {
            brush.Dispose();
        }

        Surface?.Dispose();
        _graphics?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Spray" /> class.
    /// </summary>
    /// <param name="size">Size of the drawing surface.</param>
    public Spray(Size size)
    {
        Surface = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
        _graphics = DrawingGraphics.FromImage(Surface);

        _brushes = new SolidBrush[64];

        for (int i = 0; i < _brushes.Length; ++i)
        {
            _brushes[i] = new SolidBrush(Color.FromArgb(GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255), GorgonRandom.RandomInt32(0, 255)));
        }
    }
}
