
// 
// Gorgon
// Copyright (C) 2019 Michael Winsor
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
// Created: May 20, 2019 11:43:38 PM
// 


using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Examples;

/// <summary>
/// This will represent a 2D layer that renders our background stars
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="BgStarLayer"/> class.</remarks>
/// <param name="renderer">The 2D renderer for the application.</param>
internal class BgStarLayer(Gorgon2D renderer)
        : Layer2D(renderer)
{

    /// <summary>
    /// Property to set or return the texture used to draw the stars on the background.
    /// </summary>
    public GorgonTexture2DView StarsTexture
    {
        get;
        set;
    }



    /// <summary>Function called to update items per frame on the layer.</summary>
    protected sealed override void OnUpdate()
    {
        // Since this layer doesn't move, there's nothing to really update.
    }

    /// <summary>Function used to render data into the layer.</summary>
    public override void Render()
    {
        Renderer.Begin(Gorgon2DBatchState.NoBlend);
        Blit(StarsTexture,
            new GorgonRectangleF(0, 0, OutputSize.X / (float)StarsTexture.Width, OutputSize.Y / (float)StarsTexture.Height),
            GorgonSamplerState.Wrapping);
        Renderer.End();
    }


}
