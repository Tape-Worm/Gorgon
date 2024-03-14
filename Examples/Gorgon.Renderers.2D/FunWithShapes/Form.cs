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
// Created: July 16, 2018 2:41:40 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// Main application form.
/// </summary>
public partial class Form
    : System.Windows.Forms.Form
{
    #region Variables.
    // Half the width and height of the "screen".
    private DX.Size2F _halfSize;
    // Our core graphics interface.
    private GorgonGraphics _graphics;
    // The swap chain that represents our "screen".
    private GorgonSwapChain _screen;
    // Our 2D renderer.
    private Gorgon2D _renderer;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to draw the pretty picture.
    /// </summary>
    private void DrawAPrettyPicture()
    {
        // Paint color.
        Color paintColor;

        // Clear the back buffer.
        _screen.RenderTargetView.Clear(Color.FromArgb(0, 0, 64));

        // First, we need to inform the renderer that we're about draw some stuff.
        _renderer.Begin();

        // Draw some points as stars.
        for (int x = 0; x < 1000; x++)
        {
            // Color.
            int colorSwitch = GorgonRandom.RandomInt32(160) + 95;   // Color component for the points.

            // Get the star color.
            paintColor = Color.FromArgb(colorSwitch, colorSwitch, colorSwitch);

            _renderer.DrawFilledRectangle(new DX.RectangleF(GorgonRandom.RandomSingle(_screen.Width), GorgonRandom.RandomSingle(_screen.Height), 1, 1), paintColor);
        }

        // Draw lines.
        for (int x = 0; x < 360; x++)
        {
            float cos = (x + (x / 2.0f)).FastCos();     // Cosine.			
            float sin = (x + (x / 3.0f)).FastSin();     // Sin.

            // Set up a random color.				
            paintColor = Color.FromArgb((byte)GorgonRandom.RandomInt32(128, 255), GorgonRandom.RandomInt32(64, 255), GorgonRandom.RandomInt32(64, 255), 0);
            var startPosition = new Vector2(sin + _halfSize.Width, cos + _halfSize.Height);
            var endPosition = new Vector2((cos * (GorgonRandom.RandomSingle(_halfSize.Width * 0.82f))) + startPosition.X, (sin * (GorgonRandom.RandomSingle(_halfSize.Height * 0.82f))) + startPosition.Y);
            _renderer.DrawLine(startPosition.X, startPosition.Y, endPosition.X, endPosition.Y, paintColor);
        }

        // Draw a filled circle.
        float size = (_halfSize.Width / 2.0f) + (GorgonRandom.RandomInt32(10) - 8);
        float half = size / 2.0f;
        _renderer.DrawFilledEllipse(new DX.RectangleF(_halfSize.Width - half, _halfSize.Height - half, size, size), Color.Yellow);

        // Draw some circles in the filled circle (sunspots). 
        for (int x = 0; x < 25; x++)
        {
            //float radius = GorgonRandom.RandomSingle(5.0f);
            float radius = 4;
            var spotPosition = new Vector2((GorgonRandom.RandomSingle((_halfSize.Height / 2.0f)) + _halfSize.Width - (_halfSize.Height / 4.0f)),
                                                     (GorgonRandom.RandomSingle((_halfSize.Height / 2.0f)) + _halfSize.Height - (_halfSize.Height / 4.0f)));
            _renderer.DrawEllipse(new DX.RectangleF(spotPosition.X - (radius * 0.5f),
                                                    spotPosition.Y - (radius * 0.5f),
                                                    radius,
                                                    radius),
                                  Color.Black);
        }

        // Draw some black bars.
        _renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _screen.Width, _screen.Height / 6.0f), Color.Black);
        _renderer.DrawFilledRectangle(new DX.RectangleF(0, _screen.Height - (_screen.Height / 6.0f), _screen.Width, _screen.Height / 6.0f), Color.Black);

        // Tell the renderer that we're done drawing so we can actually render the shapes.            
        _renderer.End();

        GorgonExample.DrawStatsAndLogo(_renderer);

        // Always call this when done or you won't see anything.
        _screen.Present(1);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
    /// </summary>
    /// <param name="e">A <see cref="KeyEventArgs"></see> that contains the event data.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }
    }

    /// <summary>
    /// Raises the <see cref="E:Paint"/> event.
    /// </summary>
    /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_renderer is null)
        {
            return;
        }

        // If we get repainted we can redraw the image.
        DrawAPrettyPicture();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
    /// </summary>
    /// <param name="e">A <see cref="FormClosingEventArgs"></see> that contains the event data.</param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        GorgonExample.UnloadResources();

        // Perform clean up.
        Gorgon2D renderer = Interlocked.Exchange(ref _renderer, null);
        GorgonSwapChain screen = Interlocked.Exchange(ref _screen, null);
        GorgonGraphics graphics = Interlocked.Exchange(ref _graphics, null);

        renderer?.Dispose();
        screen?.Dispose();
        graphics?.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
    /// </summary>
    /// <param name="e">An <see cref="EventArgs"></see> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        GorgonExample.ShowStatistics = false;
        Cursor.Current = Cursors.WaitCursor;

        try
        {
            Show();
            Application.DoEvents();

            // Initialize Gorgon
            // Set it up so that we won't be rendering in the background, but allow the screensaver to activate.
            IReadOnlyList<IGorgonVideoAdapterInfo> adapters = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);
            if (adapters.Count == 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate,
                                          "No suitable video adapter found in the system.\nGorgon requires a minimum of a Direct3D 11.2 capable video device.");
            }

            _graphics = new GorgonGraphics(adapters[0]);

            // Set the video mode.
            ClientSize = new Size(640, 400);
            _screen = new GorgonSwapChain(_graphics,
                                          this,
                                          new GorgonSwapChainInfo(ClientSize.Width, ClientSize.Height, BufferFormat.R8G8B8A8_UNorm)
                                          {
                                              Name = "FunWithShapes SwapChain"
                                          });
            _screen.SwapChainResized += Screen_AfterSwapChainResized;
            _graphics.SetRenderTarget(_screen.RenderTargetView);
            _halfSize = new DX.Size2F(_screen.Width / 2.0f, _screen.Height / 2.0f);

            // Create our 2D renderer so we can draw stuff.
            _renderer = new Gorgon2D(_graphics);

            LabelPleaseWait.Visible = false;

            GorgonExample.LoadResources(_graphics);

            // Draw the image.
            DrawAPrettyPicture();
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
    /// Handles the AfterSwapChainResized event of the Screen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
    private void Screen_AfterSwapChainResized(object sender, SwapChainResizedEventArgs e)
    {
        _halfSize = new DX.Size2F(e.Size.Width / 2.0f, e.Size.Height / 2.0f);

        // Update the image.
        DrawAPrettyPicture();
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Constructor.
    /// </summary>
    public Form() => InitializeComponent();
    #endregion
}
