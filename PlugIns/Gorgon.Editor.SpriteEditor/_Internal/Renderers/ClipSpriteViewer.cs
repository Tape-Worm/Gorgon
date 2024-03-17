
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


using System.ComponentModel;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A renderer used to render the current sprite along with its texture and providing feedback for clipping
/// </summary>
internal class ClipSpriteViewer
    : SpriteViewer
{

    // Event triggered when toggling the manual input UI.
    private event EventHandler ToggleManualInputEvent;

    /// <summary>
    /// Event triggered when the manual input window should be toggled on or off.
    /// </summary>
    public event EventHandler ToggleManualInput
    {
        add
        {
            if (value is null)
            {
                ToggleManualInputEvent = null;
                return;
            }

            ToggleManualInputEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            ToggleManualInputEvent -= value;
        }
    }



    // Marching ants rectangle.
    private readonly IRectClipperService _clipper;
    // The render target for the sprite texture.
    private GorgonRenderTarget2DView _spriteTarget;
    // The sprite texture to display in the background.
    private GorgonTexture2DView _spriteTexture;
    // The sprite to render.
    private readonly GorgonSprite _sprite;



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
        DX.RectangleF clearRegion = _sprite.Texture.ToPixel(_sprite.TextureRegion).ToRectangleF();

        _spriteTarget.Clear(GorgonColor.BlackTransparent);

        Graphics.SetRenderTarget(_spriteTarget);
        Renderer.PrimitiveAlphaTestRange = null;
        Renderer.Begin();

        Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, _sprite.Texture.Width, _sprite.Texture.Height),
                                     GorgonColor.White,
                                     _sprite.Texture,
                                     new DX.RectangleF(0, 0, 1, 1),
                                     DataContext.SpriteClipContext.ArrayIndex,
                                     GorgonSamplerState.PointFiltering);

        // Remove the area where the sprite is located.
        Renderer.DrawFilledRectangle(clearRegion, GorgonColor.BlackTransparent);

        Renderer.End();
        Renderer.PrimitiveAlphaTestRange = prevAlphaTest;
        Graphics.SetRenderTarget(prevTarget);
    }

    /// <summary>Handles the RectChanged event of the Clipper control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Clipper_RectChanged(object sender, EventArgs e) => DataContext.SpriteClipContext.SpriteRectangle = _clipper.Rectangle;

    /// <summary>Handles the PropertyChanged event of the SpriteClipContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void SpriteClipContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpriteClipContext.FixedSize):
                DX.Size2F? size = DataContext.SpriteClipContext.FixedSize;

                _clipper.AllowResize = size is null;

                if (size is null)
                {
                    break;
                }

                DX.RectangleF rect = new(DataContext.SpriteClipContext.SpriteRectangle.Left,
                                             DataContext.SpriteClipContext.SpriteRectangle.Top,
                                             size.Value.Width,
                                             size.Value.Height);

                // Ensure our fixed area doesn't exceed the bounds of the texture.
                if (rect.Right > _clipper.Bounds.Right)
                {
                    rect.Left -= (rect.Right - _clipper.Bounds.Right);

                    if (rect.Left < 0)
                    {
                        rect.Left = 0;
                    }
                }

                if (rect.Bottom > _clipper.Bounds.Bottom)
                {
                    rect.Top -= (rect.Bottom - _clipper.Bounds.Bottom);

                    if (rect.Top < 0)
                    {
                        rect.Top = 0;
                    }
                }

                DataContext.SpriteClipContext.SpriteRectangle = rect;
                break;
            case nameof(ISpriteClipContext.SpriteRectangle):
                UpdateWorkingSprite();
                break;
            case nameof(ISpriteClipContext.ArrayIndex):
                _sprite.TextureArrayIndex = DataContext.SpriteClipContext.ArrayIndex;
                break;
        }
    }

    /// <summary>
    /// Function to update the working sprite and selection rectangle.
    /// </summary>
    private void UpdateWorkingSprite()
    {
        _clipper.Rectangle = DataContext.SpriteClipContext.SpriteRectangle;
        _sprite.TextureArrayIndex = DataContext.SpriteClipContext.ArrayIndex;
        _sprite.TextureRegion = _sprite.Texture.ToTexel(_clipper.Rectangle.ToRectangle());
        _sprite.Size = _clipper.Rectangle.Size.Truncate();
        _sprite.Position = new Vector2(_clipper.Rectangle.X - (RenderRegion.Width * 0.5f),
                                          _clipper.Rectangle.Y - (RenderRegion.Height * 0.5f))
                                 .Truncate();
    }

    /// <summary>Handles the KeyboardIconClicked event of the Clipper control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Clipper_KeyboardIconClicked(object sender, EventArgs e)
    {
        EventHandler handler = ToggleManualInputEvent;
        handler?.Invoke(this, e);
    }

    /// <summary>Function called when the camera is zoomed.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is zoomed in or out.</remarks>
    protected override void OnCameraZoomed()
    {
        base.OnCameraZoomed();
        _clipper.Refresh();
    }

    /// <summary>Function called when the camera is panned.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is panned around the view.</remarks>
    protected override void OnCameraMoved()
    {
        base.OnCameraMoved();
        _clipper.Refresh();
    }

    /// <summary>Function called when the view has been resized.</summary>
    /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
    protected override void OnResizeEnd()
    {
        base.OnResizeEnd();
        _clipper.Refresh();
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ToggleManualInputEvent = null;
        }

        base.Dispose(disposing);
    }

    /// <summary>Function to handle a mouse move event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
    protected override void OnMouseMove(MouseArgs args)
    {
        args.Handled = _clipper.MouseMove(args);
        base.OnMouseMove(args);
    }

    /// <summary>Function to handle a mouse down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
    protected override void OnMouseDown(MouseArgs args)
    {
        args.Handled = _clipper.MouseDown(args);
        base.OnMouseDown(args);
    }

    /// <summary>Function to handle a mouse up event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
    protected override void OnMouseUp(MouseArgs args)
    {
        args.Handled = _clipper.MouseUp(args);
        base.OnMouseUp(args);
    }

    /// <summary>Function to handle a preview key down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a preview key down event in their own content view.</remarks>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
        args.IsInputKey = _clipper.KeyDown(args.KeyCode, args.Modifiers);
        base.OnPreviewKeyDown(args);
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        SpriteOpacity = 1.0f;
        TextureOpacity = 0.5f;

        base.OnLoad();

        RenderRegion = new DX.RectangleF(0, 0, DataContext.Texture.Width, DataContext.Texture.Height);

        CreateSpriteTexture();

        _clipper.Camera = Camera;
        _clipper.RectChanged += Clipper_RectChanged;
        _clipper.Bounds = RenderRegion;

        _sprite.Color = GorgonColor.White;
        _sprite.Texture = DataContext.Texture;
        _sprite.TextureArrayIndex = DataContext.ArrayIndex;
        _sprite.TextureSampler = GorgonSamplerState.PointFiltering;

        DataContext.SpriteClipContext.SpriteRectangle = DataContext.Texture.ToPixel(DataContext.TextureCoordinates).ToRectangleF();

        UpdateWorkingSprite();

        DataContext.SpriteClipContext.PropertyChanged += SpriteClipContext_PropertyChanged;
        _clipper.KeyboardIconClicked += Clipper_KeyboardIconClicked;
    }

    /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
    /// <remarks>Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.</remarks>
    protected override void OnUnload()
    {
        DataContext.SpriteClipContext.PropertyChanged -= SpriteClipContext_PropertyChanged;
        _clipper.RectChanged -= Clipper_RectChanged;
        _clipper.KeyboardIconClicked -= Clipper_KeyboardIconClicked;

        DestroySpriteTexture();
        base.OnUnload();
    }

    /// <summary>Function to draw the sprite.</summary>
    protected override void DrawSprite()
    {
        Vector2 halfRegion = new(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f);

        RenderSpriteTexture();

        Renderer.Begin(camera: Camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(halfRegion.X,
                                                       halfRegion.Y,
                                                       RenderRegion.Width,
                                                       RenderRegion.Height),
                                    new GorgonColor(GorgonColor.White, TextureOpacity),
                                    _spriteTexture,
                                    new DX.RectangleF(0, 0, 1, 1),
                                    textureSampler: GorgonSamplerState.PointFiltering);

        Renderer.DrawSprite(_sprite);
        Renderer.End();

        // Draw in client space.
        Renderer.Begin();
        _clipper.Render();
        Renderer.End();
    }

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom()
    {
        if (DataContext?.Texture is null)
        {
            return;
        }

        DX.RectangleF region = _sprite.Bounds;
        region.Inflate(32, 32);
        ZoomLevels spriteZoomLevel = GetNearestZoomFromRectangle(region);

        Vector3 spritePosition = Camera.Unproject(new Vector3(region.X + region.Width * 0.5f, region.Y + region.Height * 0.5f, 0));

        MoveTo(new Vector2(spritePosition.X, spritePosition.Y), spriteZoomLevel.GetScale());
    }



    /// <summary>Initializes a new instance of the <see cref="ClipSpriteViewer"/> class.</summary>
    /// <param name="renderer">The main renderer for the content view.</param>
    /// <param name="swapChain">The swap chain for the content view.</param>
    /// <param name="dataContext">The view model to assign to the renderer.</param>
    /// <param name="selectionRect">The selection rectangle for the sprite.</param>
    public ClipSpriteViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext, IRectClipperService selectionRect)
        : base(SpriteClipContext.ViewerName, renderer, swapChain, dataContext)
    {
        SpriteOpacity = 1.0f;
        TextureOpacity = 0.5f;

        _sprite = new GorgonSprite();
        _clipper = selectionRect;
    }

}
