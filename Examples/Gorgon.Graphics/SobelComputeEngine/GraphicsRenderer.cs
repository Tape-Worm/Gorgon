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
// Created: August 3, 2017 10:43:33 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// This is used to render our image data for the application.
/// </summary>
internal class GraphicsRenderer
    : IDisposable
{
    #region Variables.
    // The graphics interface to use.
    private readonly GorgonGraphics _graphics;
    // The swap chain used to render our data.
    private GorgonSwapChain _swapChain;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to render the data to the panel assigned in the <see cref="SetPanel"/> method.
    /// </summary>
    /// <param name="texture">The texture to render.</param>
    /// <param name="outputTexture">The output texture to render.</param>
    public void Render(GorgonTexture2DView texture, GorgonTexture2DView outputTexture)
    {
        if (_swapChain is null)
        {
            return;
        }

        if (_graphics.RenderTargets[0] != _swapChain.RenderTargetView)
        {
            _graphics.SetRenderTarget(_swapChain.RenderTargetView);
        }

        _swapChain.RenderTargetView.Clear(Color.CornflowerBlue);

        if ((texture is null)
            || (outputTexture is null))
        {
            _swapChain.Present(1);
            return;
        }

        // Get aspect ratio.
        var scale = new DX.Size2F((_swapChain.Width * 0.5f) / texture.Width, (float)_swapChain.Height / texture.Height);

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
        var size = new DX.Size2((int)(scale.Width * texture.Width), (int)(scale.Height * texture.Height));

        // Find the position.
        var bounds = new DX.Rectangle((_swapChain.Width / 4) - (size.Width / 2), ((_swapChain.Height / 2) - (size.Height / 2)), size.Width, size.Height);

        GorgonExample.Blitter.Blit(texture, bounds, blendState: GorgonBlendState.Default, samplerState: GorgonSamplerState.PointFiltering);

        bounds = new DX.Rectangle((_swapChain.Width - (_swapChain.Width / 4)) - (size.Width / 2), ((_swapChain.Height / 2) - (size.Height / 2)), size.Width, size.Height);
        GorgonExample.Blitter.Blit(outputTexture, bounds, blendState: GorgonBlendState.Default, samplerState: GorgonSamplerState.PointFiltering);

        _swapChain.Present(1);
    }

    /// <summary>
    /// Function to assign rendering to a win forms panel.
    /// </summary>
    /// <param name="panel">The panel to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="panel"/> parameter is <b>null</b>.</exception>
    public void SetPanel(Panel panel)
    {
        if (panel is null)
        {
            throw new ArgumentNullException(nameof(panel));
        }

        _swapChain = new GorgonSwapChain(_graphics,
                                         panel,
                                         new GorgonSwapChainInfo(panel.ClientSize.Width,
                                                                      panel.ClientSize.Height,
                                                                      BufferFormat.R8G8B8A8_UNorm)
                                         {
                                             Name = "ExampleSwapChain"
                                         });
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() => _swapChain?.Dispose();
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsRenderer" /> class.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    public GraphicsRenderer(GorgonGraphics graphics) => _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
    #endregion
}
