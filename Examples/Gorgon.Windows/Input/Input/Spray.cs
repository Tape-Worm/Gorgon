
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
using Gorgon.Graphics;
using Gorgon.Math;
using DrawingGraphics = System.Drawing.Graphics;

namespace Gorgon.Examples;

/// <summary>
/// The type of action when spraying.
/// </summary>
public enum SprayAction
{
    None = 0,
    Random = 1,
    Erase = 2,
    Brush1 = 3,
    Brush2 = 4,
    Brush3 = 5,
    Brush4 = 6,
    Brush5 = 7,
    Brush6 = 8,
    Brush7 = 9,
    Brush8 = 0xa,
    Brush9 = 0xb,
    Brush10 = 0xc,
    Brush11 = 0xd,
    Brush12 = 0xe,
    Brush13 = 0xf,
    Brush14 = 0x10,
    Brush15 = 0x11,
    Clear = 0xff
}

/// <summary>
/// This object is responsible for drawing the spray effect on the display surface
/// </summary>
internal class Spray
    : IDisposable
{
    // The brushes we can use.
    private readonly Dictionary<SprayAction, GorgonColor> _brushes = new()
    {
        { SprayAction.Brush1, GorgonColors.Black },
        { SprayAction.Brush2, GorgonColors.Blue },
        { SprayAction.Brush3, GorgonColors.Green },
        { SprayAction.Brush4, GorgonColors.Cyan },
        { SprayAction.Brush5, GorgonColors.Red },
        { SprayAction.Brush6, GorgonColors.Purple },
        { SprayAction.Brush7, GorgonColors.Brown },
        { SprayAction.Brush8, GorgonColors.Gray50 },
        { SprayAction.Brush9, GorgonColors.Gray30 },
        { SprayAction.Brush10, GorgonColors.LightBlue },
        { SprayAction.Brush11, GorgonColors.LightGreen },
        { SprayAction.Brush12, GorgonColors.LightCyan },
        { SprayAction.Brush13, GorgonColors.LightRed },
        { SprayAction.Brush14, GorgonColors.LightPurple },
        { SprayAction.Brush15, GorgonColors.Yellow },
        { SprayAction.Erase, GorgonColors.White }
    };

    // Graphics interface.
    private DrawingGraphics _graphics;

    /// <summary>
    /// Property to set or return the size of the spray particles.
    /// </summary>
    public GorgonPoint SpraySize
    {
        get;
        set;
    } = new GorgonPoint(10, 10);

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
    public void Resize(GorgonPoint newSize)
    {
        _graphics.Dispose();

        Bitmap newBuffer = new(newSize.X, newSize.Y, PixelFormat.Format32bppArgb);
        _graphics = DrawingGraphics.FromImage(newBuffer);

        // Copy the old image to the new surface.
        _graphics.DrawImage(Surface, Point.Empty);
        Surface.Dispose();

        // Set the new buffer as the current surface.
        Surface = newBuffer;
    }

    /// <summary>
    /// Function to randomly "spray" a point on the surface.
    /// </summary>
    /// <param name="point">Origin point for the spray.</param>
    /// <param name="action">The action to perform.</param>
    /// <param name="alpha">The alpha value for the brush.</param>
    /// <returns>The current color for the brush, or <b>null</b> if no brush was used.</returns>
    public GorgonColor? PerformAction(GorgonPoint point, SprayAction action, float alpha)
    {
        if (action is SprayAction.None or SprayAction.Clear)
        {
            if (action == SprayAction.Clear)
            {
                _graphics.Clear(Color.White);
            }

            return null;
        }

        SolidBrush? brush;

        if (action == SprayAction.Erase)
        {
            alpha = 1.0f;
        }

        GorgonColor color = action == SprayAction.Random
            ? new GorgonColor(GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), GorgonRandom.RandomSingle(), alpha)
            : new(_brushes[action], alpha);

        brush = new SolidBrush(color);
        Point randomArea = new(GorgonRandom.RandomInt32(-SpraySize.X.Max(-4), SpraySize.X.Min(4)), GorgonRandom.RandomInt32(-SpraySize.Y.Max(-4), SpraySize.Y.Min(4)));
        Rectangle pos = new(point.X + randomArea.X, point.Y + randomArea.Y, SpraySize.X, SpraySize.Y);

        try
        {
            if ((SpraySize.X < 3) && (SpraySize.Y < 3))
            {
                _graphics.FillRectangle(brush, pos);
                return color;
            }

            _graphics.FillEllipse(brush, pos);
            return color;
        }
        finally
        {
            brush?.Dispose();
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Surface?.Dispose();
        _graphics?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Spray" /> class.
    /// </summary>
    /// <param name="size">Size of the drawing surface.</param>
    public Spray(GorgonPoint size)
    {
        Surface = new Bitmap(size.X, size.Y, PixelFormat.Format32bppArgb);
        _graphics = DrawingGraphics.FromImage(Surface);
        _graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
    }
}
