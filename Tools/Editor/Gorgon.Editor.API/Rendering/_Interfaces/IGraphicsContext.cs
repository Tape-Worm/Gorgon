
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 26, 2018 6:58:49 PM
// 


using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;

namespace Gorgon.Editor.Rendering;

/// <summary>
/// A graphics context for passing the graphics interfaces to various views
/// </summary>
public interface IGraphicsContext
{

    /// <summary>
    /// Property to return information about the video adapter selected.
    /// </summary>
    IGorgonVideoAdapterInfo VideoAdapter
    {
        get;
    }

    /// <summary>
    /// Property to return the blitter used to arbitrarily render a full texture.
    /// </summary>
    GorgonTextureBlitter Blitter
    {
        get;
    }

    /// <summary>
    /// Property to return the factory used to create fonts.
    /// </summary>
    GorgonFontFactory FontFactory
    {
        get;
    }

    /// <summary>
    /// Property to return the graphics interface.
    /// </summary>
    GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the 2D renderer.
    /// </summary>
    Gorgon2D Renderer2D
    {
        get;
    }



    /// <summary>
    /// Function to return a leased out swap chain.
    /// </summary>
    /// <param name="swapChain">The swap chain to return.</param>        
    void ReturnSwapPresenter(ref GorgonSwapChain swapChain);

    /// <summary>
    /// Function to retrieve the swap chain for a specific control.
    /// </summary>
    /// <param name="control">The control that will be bound to the swap chain.</param>
    /// <returns>A new swap chain bound to the control.</returns>
    GorgonSwapChain LeaseSwapPresenter(Control control);

}
