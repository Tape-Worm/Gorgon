
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
// Created: April 1, 2019 11:51:35 PM
// 

using System.ComponentModel;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A renderer to use with the sprite picker tool
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="PickSpriteViewer"/> class.</remarks>
/// <param name="renderer">The 2D renderer for the application </param>
/// <param name="swapChain">The swap chain for the render area.</param>
/// <param name="dataContext">The graphics interface for the application.</param>
/// <param name="picker">The sprite picker used to automatically clip sprite data.</param>
/// <param name="selectionRect">The marching ants rectangle used to draw selection rectangles.</param>
internal class PickSpriteViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext, PickClipperService picker, IMarchingAnts selectionRect)
        : SpriteViewer(ViewerName, renderer, swapChain, dataContext)
{
    /// <summary>
    /// The name of the viewer.
    /// </summary>
    public const string ViewerName = "ContextSpritePick";

    // Marching ants rectangle.
    private readonly IMarchingAnts _marchAnts = selectionRect;
    // The rectangle clipping service.
    private readonly PickClipperService _picker = picker;
    // The render target for the sprite texture.
    private GorgonRenderTarget2DView _spriteTarget;
    // The sprite texture to display in the background.
    private GorgonTexture2DView _spriteTexture;
    // The sprite to render.
    private readonly GorgonSprite _sprite = new();
    // The region for the sprite.
    private GorgonRectangleF _spriteRegion;

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
        GorgonRectangleF clearRegion = _sprite.Texture.ToPixel(_sprite.TextureRegion);

        _spriteTarget.Clear(GorgonColors.BlackTransparent);

        Graphics.SetRenderTarget(_spriteTarget);
        Renderer.PrimitiveAlphaTestRange = null;
        Renderer.Begin();

        Renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, _sprite.Texture.Width, _sprite.Texture.Height),
                                     GorgonColors.White,
                                     _sprite.Texture,
                                     new GorgonRectangleF(0, 0, 1, 1),
                                     DataContext.SpritePickContext.ArrayIndex,
                                     GorgonSamplerState.PointFiltering);

        // Remove the area where the sprite is located.
        Renderer.DrawFilledRectangle(clearRegion, GorgonColors.BlackTransparent);

        Renderer.End();
        Renderer.PrimitiveAlphaTestRange = prevAlphaTest;
        Graphics.SetRenderTarget(prevTarget);
    }

    /// <summary>
    /// Function to update the working sprite and selection rectangle.
    /// </summary>
    private void UpdateWorkingSprite()
    {
        //_clipper.Rectangle = DataContext.SpriteClipContext.SpriteRectangle;
        _sprite.TextureArrayIndex = DataContext.SpritePickContext.ArrayIndex;
        _sprite.TextureRegion = _sprite.Texture.ToTexel((GorgonRectangle)DataContext.SpritePickContext.SpriteRectangle);
        _sprite.Size = DataContext.SpritePickContext.SpriteRectangle.Size.Truncate();
        _sprite.Position = new Vector2(DataContext.SpritePickContext.SpriteRectangle.X - (RenderRegion.Width * 0.5f),
                                          DataContext.SpritePickContext.SpriteRectangle.Y - (RenderRegion.Height * 0.5f))
                                 .Truncate();

        _spriteRegion = Renderer.MeasureSprite(_sprite);
    }

    /// <summary>Function to handle a mouse up event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
    protected override void OnMouseUp(MouseArgs args)
    {
        base.OnMouseDown(args);

        if (DataContext is null)
        {
            return;
        }

        Vector2 position = new(args.CameraSpacePosition.X + RenderRegion.Width * 0.5f,
                                      args.CameraSpacePosition.Y + RenderRegion.Height * 0.5f);

        GorgonRectangleF? newRect = _picker.Pick(position, DataContext.Settings.ClipMaskValue, DataContext.Settings.ClipMaskType);

        if (newRect is null)
        {
            Cursor.Current = Cursors.Default;
            return;
        }

        DataContext.SpritePickContext.SpriteRectangle = newRect.Value;
        Cursor.Current = Cursors.Hand;
    }

    /// <summary>Function to handle a mouse move event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
    protected override void OnMouseMove(MouseArgs args)
    {
        base.OnMouseMove(args);

        Vector2 position = new(args.CameraSpacePosition.X + RenderRegion.Width * 0.5f,
                                      args.CameraSpacePosition.Y + RenderRegion.Height * 0.5f);

        if (RenderRegion.Contains(position.X, position.Y))
        {
            Cursor.Current = Cursors.Hand;
        }
        else
        {
            Cursor.Current = Cursors.Default;
        }
    }

    /// <summary>Function to draw the sprite.</summary>
    protected override void DrawSprite()
    {
        Vector2 halfRegion = new(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f);

        // We'll need to draw the marching ants rectangle in standard client space. 
        // So, we can just get the camera to tell us where that is.
        Vector3 spriteTopLeft = new(_spriteRegion.Left, _spriteRegion.Top, 0);
        Vector3 spriteBottomRight = new(_spriteRegion.Right, _spriteRegion.Bottom, 0);
        Camera.Unproject(spriteTopLeft, out Vector3 transformedTopLeft);
        Camera.Unproject(spriteBottomRight, out Vector3 transformedBottomRight);

        GorgonRectangleF marchAntsRect = new()
        {
            Left = (int)transformedTopLeft.X,
            Top = (int)transformedTopLeft.Y,
            Right = (int)transformedBottomRight.X,
            Bottom = (int)transformedBottomRight.Y
        };

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
        _marchAnts.Animate();
        _marchAnts.Draw(marchAntsRect);
        Renderer.End();
    }

    /// <summary>Function called to perform custom loading of resources.</summary>
    protected override void OnLoad()
    {
        SpriteOpacity = 1.0f;
        TextureOpacity = 0.5f;

        base.OnLoad();

        RenderRegion = new GorgonRectangleF(0, 0, DataContext.Texture.Width, DataContext.Texture.Height);

        CreateSpriteTexture();

        _picker.ImageData = DataContext.SpritePickContext.ImageData?.Buffers[0, DataContext.SpritePickContext.ArrayIndex];
        _picker.Padding = DataContext.SpritePickContext.Padding;

        _sprite.Color = GorgonColors.White;
        _sprite.Texture = DataContext.Texture;
        _sprite.TextureArrayIndex = DataContext.ArrayIndex;
        _sprite.TextureSampler = GorgonSamplerState.PointFiltering;

        DataContext.SpritePickContext.SpriteRectangle = DataContext.Texture.ToPixel(DataContext.TextureCoordinates);

        UpdateWorkingSprite();

        DataContext.SpritePickContext.PropertyChanging += SpritePickContext_PropertyChanging;
        DataContext.SpritePickContext.PropertyChanged += SpritePickContext_PropertyChanged;
    }

    /// <summary>Handles the PropertyChanging event of the SpritePickContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
    private void SpritePickContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpritePickContext.ImageData):
                _picker.ImageData = null;
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the SpritePickContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void SpritePickContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpritePickContext.ImageData):
                _picker.ImageData = DataContext.SpritePickContext.ImageData?.Buffers[0, DataContext.SpritePickContext.ArrayIndex];
                break;
            case nameof(ISpritePickContext.SpriteRectangle):
                UpdateWorkingSprite();
                break;
            case nameof(ISpritePickContext.ArrayIndex):
                _sprite.TextureArrayIndex = DataContext.SpritePickContext.ArrayIndex;
                _picker.ImageData = DataContext.SpritePickContext.ImageData?.Buffers[0, DataContext.SpritePickContext.ArrayIndex];
                break;
            case nameof(ISpritePickContext.Padding):
                _picker.Padding = DataContext.SpritePickContext.Padding;
                UpdateWorkingSprite();
                break;
        }
    }

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(ISpriteContent.Texture):
                UpdateWorkingSprite();
                break;
        }
    }

    /// <summary>Function called to perform custom unloading of resources.</summary>
    protected override void OnUnload()
    {
        DataContext.SpritePickContext.PropertyChanging -= SpritePickContext_PropertyChanging;
        DataContext.SpritePickContext.PropertyChanged -= SpritePickContext_PropertyChanged;

        DestroySpriteTexture();

        base.OnUnload();
    }

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom() => MoveTo(Vector2.Zero, ZoomLevels.ToWindow.GetScale());
}
