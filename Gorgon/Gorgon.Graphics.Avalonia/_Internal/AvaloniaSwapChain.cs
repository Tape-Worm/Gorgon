
// 
// Gorgon
// Copyright (C) 2023 Michael Winsor
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
// Created: February 19, 2023 9:53:51 PM
// 


using Avalonia;
using Avalonia.Rendering.Composition;
using Gorgon.Graphics.Core;

namespace Gorgon.Graphics.Avalonia;

/// <summary>
/// A helper class for composition-backed swapchains
/// </summary>
/// <remarks>
/// Initializes an instance of the <see cref="AvaloniaSwapChain"/> class
/// </remarks>
/// <param name="graphics">The graphics interface bound to the swap chain.</param>
/// <param name="gpuInterop">The interop layer for the GPU.</param>
/// <param name="surface">The surface to render into.</param>
internal class AvaloniaSwapChain(GorgonGraphics graphics, ICompositionGpuInterop gpuInterop, CompositionDrawingSurface surface)
        : IAsyncDisposable, IGorgonGraphicsObject
{

    // The GPU interop for composition.
    private readonly ICompositionGpuInterop _gpuInterop = gpuInterop;
    // The surface that will receive the drawing information.
    private readonly CompositionDrawingSurface _surface = surface;
    // The swap chain render target images.
    private readonly List<AvaloniaSwapChainImage> _pendingImages = [];
    // The currently active render target image.
    private AvaloniaSwapChainImage _currentImage;



    /// <summary>
    /// Property to return the graphics interface that built this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
        private set;
    } = graphics;



    /// <summary>
    /// Function to retrieve the image for the swap chain that is not already in flight.
    /// </summary>
    /// <param name="pixelSize">The size of the image.</param>
    /// <returns>An existing available image, or a new image.</returns>
    private AvaloniaSwapChainImage GetImage(PixelSize pixelSize)
    {
        AvaloniaSwapChainImage first = null;
        bool multiple = false;

        for (int c = _pendingImages.Count - 1; c >= 0; --c)
        {
            AvaloniaSwapChainImage image = _pendingImages[c];
            bool noResize = (image.Width == pixelSize.Width) && (image.Height == pixelSize.Height);

            if ((image.ImageState == AvaloniaImageState.Error)
                || ((!noResize) && (image.ImageState == AvaloniaImageState.Available)))
            {
                image?.DisposeAsync();
                _pendingImages.RemoveAt(c);
            }

            if ((noResize) && (image.ImageState == AvaloniaImageState.Available))
            {
                if (first is null)
                {
                    first = image;
                }
                else
                {
                    multiple = true;
                }
            }
        }

        return multiple ? first : new AvaloniaSwapChainImage(Graphics, pixelSize, _gpuInterop, _surface);
    }

    /// <summary>
    /// Function to begin the rendering process.
    /// </summary>
    /// <param name="pixelSize">The size of the swap chain.</param>
    public GorgonRenderTarget2DView BeginRendering(PixelSize pixelSize)
    {
        AvaloniaSwapChainImage image = GetImage(pixelSize);

        image.Begin();
        _pendingImages.Remove(image);
        _currentImage = image;

        return _currentImage.RenderTargetView;
    }

    /// <summary>
    /// Function to send the graphics to avalonia.
    /// </summary>
    public void Present()
    {
        AvaloniaSwapChainImage image = Interlocked.Exchange(ref _currentImage, null);

        // We do not have an active render target image, so skip out.
        if (image is null)
        {
            return;
        }

        image.Present();
        _pendingImages.Add(image);
    }

    /// <summary>
    /// Function called to asynchronously dispose the resources used by the swap chain.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> for asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        // Get rid of the queued updates.
        foreach (AvaloniaSwapChainImage img in _pendingImages)
        {
            await img.DisposeAsync();
        }
    }




}
