﻿
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
// Created: March 31, 2019 10:25:11 PM
// 

using System.Numerics;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A renderer used to render when the sprite lacks a texture association
/// </summary>
internal class NoTextureViewer
    : SpriteViewer
{
    /// <summary>
    /// The name of the viewer.
    /// </summary>
    public const string ViewerName = "SpriteNoTextureRenderer";

    // The texture to display when a sprite lacks a texture association.
    private GorgonTexture2DView _noImage;

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _noImage?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Function to render the background.</summary>
    /// <remarks>Developers can override this method to render a custom background.</remarks>
    protected sealed override void OnRenderBackground()
    {
        float newSize = RenderRegion.Width < RenderRegion.Height ? RenderRegion.Width : RenderRegion.Height;
        Vector2 size = new(newSize.Min(_noImage.Width), newSize.Min(_noImage.Width));
        Vector2 halfClient = new(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);
        Vector2 pos = new((int)(halfClient.X - size.X * 0.5f), (int)(halfClient.Y - size.Y * 0.5f));

        Renderer.Begin();
        Renderer.DrawFilledRectangle(new GorgonRectangleF(pos.X, pos.Y, size.X, size.Y), GorgonColors.White, _noImage, new GorgonRectangleF(0, 0, 1, 1));
        Renderer.End();
    }

    /// <summary>Function called during resource creation.</summary>
    protected sealed override void OnCreateResources()
    {
        base.OnCreateResources();

        using MemoryStream stream = CommonEditorResources.MemoryStreamManager.GetStream(Resources.SpriteEditor_Bg_1024x1024);
        _noImage = GorgonTexture2DView.FromStream(Graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
        {
            Name = "Sprite Editor - No texture default texture",
            Usage = ResourceUsage.Immutable
        });
    }

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom()
    {
        // This viewer cannot pan or zoom.
    }

    /// <summary>Initializes a new instance of the <see cref="NoTextureViewer"/> class.</summary>
    /// <param name="renderer">The 2D renderer for the application.</param>
    /// <param name="swapChain">The swap chain for the render area.</param>
    /// <param name="sprite">The sprite used for rendering.</param>
    public NoTextureViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent sprite)
        : base(ViewerName, renderer, swapChain, sprite)
    {
        CanPanHorizontally = false;
        CanPanHorizontally = false;
        CanZoom = false;
    }
}
