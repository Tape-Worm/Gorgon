﻿
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: April 4, 2020 9:35:21 PM
// 

using System.Numerics;
using Gorgon.Core;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A renderer used to render the current sprite along with its texture
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="DefaultSpriteViewer"/> class.</remarks>
/// <param name="renderer">The main renderer for the content view.</param>
/// <param name="swapChain">The swap chain for the content view.</param>
/// <param name="dataContext">The view model to assign to the renderer.</param>
/// <param name="marchingAnts">The marching ants selection rectangle renderer.</param>
internal class DefaultSpriteViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext, IMarchingAnts marchingAnts)
        : SpriteViewer(ViewerName, renderer, swapChain, dataContext)
{
    /// <summary>
    /// The name of the viewer.
    /// </summary>
    public const string ViewerName = "DefaultSpriteViewer";

    // Marching ants rectangle.
    private readonly IMarchingAnts _marchAnts = marchingAnts;
    // The render target for the sprite texture.
    private GorgonRenderTarget2DView _spriteTarget;
    // The sprite texture to display in the background.
    private GorgonTexture2DView _spriteTexture;
    // The sprite to render.
    private readonly GorgonSprite _sprite = new();
    // The region where the sprite is located on the texture.
    private GorgonRectangleF _spriteRegion;

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(ISpriteContent.Anchor):
            case nameof(ISpriteContent.Size):
            case nameof(ISpriteContent.VertexOffsets):
            case nameof(ISpriteContent.TextureCoordinates):
            case nameof(ISpriteContent.ArrayIndex):
            case nameof(ISpriteContent.IsPixellated):
                UpdateSprite();
                break;
            case nameof(ISpriteContent.VertexColors):
                UpdateSpriteColors();
                break;
        }
    }

    /// <summary>
    /// Function to update the sprite colors.
    /// </summary>
    private void UpdateSpriteColors()
    {
        // We keep this separate so the opactiy animation can update the sprite vertex color values.
        for (int i = 0; i < DataContext.VertexColors.Count; ++i)
        {
            GorgonColor color = DataContext.VertexColors[i];
            _sprite.CornerColors[i] = new GorgonColor(color, SpriteOpacity.Min(color.Alpha));
        }
    }

    /// <summary>
    /// Function to update the sprite for rendering.
    /// </summary>
    private void UpdateSprite()
    {
        UpdateSpriteColors();
        for (int i = 0; i < DataContext.VertexOffsets.Count; ++i)
        {
            _sprite.CornerOffsets[i] = DataContext.VertexOffsets[i];
        }

        _sprite.Texture = DataContext.Texture;
        _sprite.TextureRegion = DataContext.TextureCoordinates;
        _sprite.TextureArrayIndex = DataContext.ArrayIndex;
        _sprite.TextureSampler = DataContext.IsPixellated ? GorgonSamplerState.PointFiltering : GorgonSamplerState.Default;

        GorgonRectangle spriteTextureLocation = DataContext.Texture.ToPixel(DataContext.TextureCoordinates);

        _sprite.Position = new Vector2(spriteTextureLocation.X + (int)(RenderRegion.Width * -0.5f),
                                          spriteTextureLocation.Y + (int)(RenderRegion.Height * -0.5f));
        _sprite.Size = DataContext.Size;

        _spriteRegion = Renderer.MeasureSprite(_sprite);
    }

    /// <summary>
    /// Function to release the texture resources.
    /// </summary>
    private void DestroySpriteTexture()
    {
        GorgonTexture2DView spriteTexture = Interlocked.Exchange(ref _spriteTexture, null);
        GorgonRenderTarget2DView spriteTarget = Interlocked.Exchange(ref _spriteTarget, null);

        spriteTexture?.Dispose();
        spriteTarget?.Dispose();
    }

    /// <summary>
    /// Function to create the texture resources.
    /// </summary>
    private void CreateSpriteTexture()
    {
        DestroySpriteTexture();

        _spriteTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo(DataContext.Texture.Width,
                                                                                                           DataContext.Texture.Height,
                                                                                                           BufferFormat.R8G8B8A8_UNorm)
        {
            Name = "Sprite Texture RTV",
            Binding = TextureBinding.ShaderResource
        });

        _spriteTexture = _spriteTarget.GetShaderResourceView();
    }

    /// <summary>
    /// Function to render the sprite texture (without the actual sprite).
    /// </summary>
    private void RenderSpriteTexture()
    {
        GorgonRenderTargetView prevTarget = Graphics.RenderTargets[0];
        GorgonRange<float>? prevAlphaTest = Renderer.PrimitiveAlphaTestRange;
        GorgonRectangleF clearRegion = DataContext.Texture.ToPixel(_sprite.TextureRegion);

        _spriteTarget.Clear(GorgonColors.BlackTransparent);

        Graphics.SetRenderTarget(_spriteTarget);
        Renderer.PrimitiveAlphaTestRange = null;
        Renderer.Begin(Gorgon2DBatchState.ModulatedAlphaOverwrite);

        Renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, DataContext.Texture.Width, DataContext.Texture.Height),
                                     GorgonColors.White,
                                     DataContext.Texture,
                                     new GorgonRectangleF(0, 0, 1, 1),
                                     DataContext.ArrayIndex,
                                     GorgonSamplerState.PointFiltering);

        // Remove the area where the sprite is located.
        Renderer.DrawFilledRectangle(clearRegion, GorgonColors.BlackTransparent);

        Renderer.End();
        Renderer.PrimitiveAlphaTestRange = prevAlphaTest;
        Graphics.SetRenderTarget(prevTarget);
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DestroySpriteTexture();
        }

        base.Dispose(disposing);
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        RenderRegion = new GorgonRectangleF(0, 0, DataContext.Texture.Width, DataContext.Texture.Height);

        CreateSpriteTexture();
        UpdateSprite();
    }

    /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
    /// <remarks>Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.</remarks>
    protected override void OnUnload()
    {
        DestroySpriteTexture();
        base.OnUnload();
    }

    /// <summary>Function to draw the sprite.</summary>
    protected override void DrawSprite()
    {
        Vector2 halfRegion = new(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f);

        // We'll need to draw the marching ants rectangle in standard client space. 
        // So, we can just get the camera to tell us where that is.
        Vector3 spriteTopLeft = new(_spriteRegion.Left, _spriteRegion.Top, 0);
        Vector3 spriteBottomRight = new(_spriteRegion.Right, _spriteRegion.Bottom, 0);
        Vector3 spriteAnchor = new((DataContext.Anchor.X * DataContext.Size.X) + _sprite.Position.X, _sprite.Position.Y + (DataContext.Anchor.Y * DataContext.Size.Y), 0);
        Camera.Unproject(spriteTopLeft, out Vector3 transformedTopLeft);
        Camera.Unproject(spriteBottomRight, out Vector3 transformedBottomRight);
        Camera.Unproject(spriteAnchor, out Vector3 transformedAnchor);
        GorgonRectangleF marchAntsRect = new()
        {
            Left = (int)transformedTopLeft.X,
            Top = (int)transformedTopLeft.Y,
            Right = (int)transformedBottomRight.X,
            Bottom = (int)transformedBottomRight.Y
        };

        if (IsAnimating)
        {
            UpdateSpriteColors();
        }

        RenderSpriteTexture();

        Renderer.Begin(camera: Camera);

        Renderer.DrawFilledRectangle(new GorgonRectangleF(halfRegion.X,
                                                        halfRegion.Y,
                                                        RenderRegion.Width,
                                                        RenderRegion.Height),
                                    new GorgonColor(GorgonColors.White, TextureOpacity),
                                    _spriteTexture,
                                    new GorgonRectangleF(0, 0, 1, 1),
                                    textureSampler: GorgonSamplerState.PointFiltering);

        Renderer.DrawSprite(_sprite);
        Renderer.End();

        // Draw in client space.
        Renderer.Begin();

        Renderer.DrawEllipse(new GorgonRectangleF(transformedAnchor.X - 4, transformedAnchor.Y - 4, 8, 8), GorgonColors.Black);
        Renderer.DrawEllipse(new GorgonRectangleF(transformedAnchor.X - 3, transformedAnchor.Y - 3, 6, 6), GorgonColors.White);

        _marchAnts.Animate();
        _marchAnts.Draw(marchAntsRect);
        Renderer.End();
    }

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom()
    {
        if (DataContext?.Texture is null)
        {
            return;
        }

        ZoomLevels spriteZoomLevel = GetNearestZoomFromRectangle(_spriteRegion);

        Vector3 spritePosition = Camera.Unproject(new Vector3(_spriteRegion.X + _spriteRegion.Width * 0.5f, _spriteRegion.Y + _spriteRegion.Height * 0.5f, 0));

        MoveTo(new Vector2(spritePosition.X, spritePosition.Y), spriteZoomLevel.GetScale());
    }
}
