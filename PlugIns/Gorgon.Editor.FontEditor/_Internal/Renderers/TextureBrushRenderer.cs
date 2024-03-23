
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
// Created: August 3, 2020 4:40:15 PM
// 


using System.ComponentModel;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// This is a renderer that will render the texture used in a texture brush
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FontRenderer"/> class.</remarks>
/// <param name="renderer">The 2D renderer used to render our font.</param>
/// <param name="mainRenderTarget">The main render target for the view.</param>
/// <param name="clipper">The clipper used to cut out a region of the texture.</param>
/// <param name="dataContext">The view model for our text data.</param>
internal class TextureBrushRenderer(Gorgon2D renderer, GorgonSwapChain mainRenderTarget, IRectClipperService clipper, IFontContent dataContext)
        : DefaultContentRenderer<IFontContent>(typeof(FontTextureBrush).FullName, renderer, mainRenderTarget, dataContext)
{

    // The editor context.
    private IFontTextureBrush _context;
    // The texture to display when a sprite lacks a texture association.
    private GorgonTexture2DView _noImage;
    // The image to use as the brush.
    private GorgonTexture2DView _image;
    // The target for rendering to the screen.
    private GorgonRenderTarget2DView _target;
    private GorgonTexture2DView _targetTexture;
    // The clipper used to cut out a part of the texture.
    private readonly IRectClipperService _clipper = clipper;



    /// <summary>Handles the PropertyChanging event of the Context control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangingEventArgs" /> instance containing the event data.</param>
    private void Context_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFontTextureBrush.Texture):
                _image?.Dispose();
                _image = null;
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the Context control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
    private void Context_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IFontTextureBrush.Region):
                _clipper.RectChanged -= Clipper_RectChanged;
                _clipper.Rectangle = _context.Region;
                _clipper.RectChanged += Clipper_RectChanged;
                break;
            case nameof(IFontTextureBrush.Texture):
                if (_context.Texture is not null)
                {
                    _image = _context.Texture.ToTexture2DView(Graphics, new GorgonTexture2DLoadOptions
                    {
                        Binding = TextureBinding.ShaderResource,
                        Usage = ResourceUsage.Immutable
                    });

                    RenderRegion = new GorgonRectangleF(0, 0, _image.Width + 32, _image.Height + 32);
                }
                else
                {
                    RenderRegion = new GorgonRectangleF(0, 0, ClientSize.X, ClientSize.Y);
                }

                _clipper.Bounds = RenderRegion;
                _clipper.Rectangle = RenderRegion;

                CreateTarget();
                MoveTo(Vector2.Zero, -1);
                break;
        }
    }

    /// <summary>
    /// Function to create the target used to render the texture.
    /// </summary>
    private void CreateTarget()
    {
        _targetTexture?.Dispose();
        _target?.Dispose();

        _target = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo((int)RenderRegion.Width, (int)RenderRegion.Height, BufferFormat.R8G8B8A8_UNorm)
        {
            Name = "Sprite Texture RTV",
            Binding = TextureBinding.ShaderResource
        });

        _targetTexture = _target.GetShaderResourceView();
    }

    /// <summary>Function called when the camera is panned.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is panned around the view.</remarks>
    protected override void OnCameraMoved() => _clipper?.Refresh();

    /// <summary>Function called when the camera is zoomed.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is zoomed in or out.</remarks>
    protected override void OnCameraZoomed() => _clipper?.Refresh();

    /// <summary>Function to handle a mouse move event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
    protected override void OnMouseMove(MouseArgs args)
    {
        if (_image is null)
        {
            return;
        }

        args.Handled = _clipper.MouseMove(args);
        base.OnMouseMove(args);
    }

    /// <summary>Function to handle a mouse down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
    protected override void OnMouseDown(MouseArgs args)
    {
        if (_image is null)
        {
            return;
        }

        args.Handled = _clipper.MouseDown(args);
        base.OnMouseDown(args);
    }

    /// <summary>Function to handle a mouse up event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
    protected override void OnMouseUp(MouseArgs args)
    {
        if (_image is null)
        {
            return;
        }

        args.Handled = _clipper.MouseUp(args);
        base.OnMouseUp(args);
    }

    /// <summary>Function to handle a preview key down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a preview key down event in their own content view.</remarks>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
        if (_image is null)
        {
            return;
        }

        args.IsInputKey = _clipper.KeyDown(args.KeyCode, args.Modifiers);
        base.OnPreviewKeyDown(args);
    }

    /// <summary>Handles the RectChanged event of the Clipper control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Clipper_RectChanged(object sender, EventArgs e)
    {
        if ((_context?.SetRegionCommand is null) || (!_context.SetRegionCommand.CanExecute(_clipper.Rectangle)))
        {
            return;
        }

        _context.SetRegionCommand.Execute(_clipper.Rectangle);
    }

    /// <summary>Function to render the background.</summary>
    /// <remarks>Developers can override this method to render a custom background.</remarks>
    protected override void OnRenderBackground()
    {
        if (_context.Texture is null)
        {
            float newSize = RenderRegion.Width < RenderRegion.Height ? RenderRegion.Width : RenderRegion.Height;
            Vector2 size = new(newSize.Min(_noImage.Width), newSize.Min(_noImage.Width));
            Vector2 halfClient = new(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);
            Vector2 pos = new((int)(halfClient.X - size.X * 0.5f), (int)(halfClient.Y - size.Y * 0.5f));

            Renderer.Begin();
            Renderer.DrawFilledRectangle(new GorgonRectangleF(pos.X, pos.Y, size.X, size.Y), GorgonColors.White, _noImage, new GorgonRectangleF(0, 0, 1, 1));
            Renderer.End();

            return;
        }

        GorgonRectangleF textureSize = new(0, 0, (float)ClientSize.X / BackgroundPattern.Width, (float)ClientSize.Y / BackgroundPattern.Height);

        Renderer.Begin();
        Renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, ClientSize.X, ClientSize.Y), GorgonColors.White, BackgroundPattern, textureSize, textureSampler: GorgonSamplerState.PointFilteringWrapping);
        Renderer.End();
    }

    /// <summary>
    /// Function to render the texture.
    /// </summary>
    private void RenderTexture()
    {
        GorgonRenderTargetView prevTarget = Graphics.RenderTargets[0];
        GorgonRange<float>? prevAlphaTest = Renderer.PrimitiveAlphaTestRange;
        GorgonRectangleF clearRegion = _image.ToPixel(_context.Region);

        _target.Clear(GorgonColors.BlackTransparent);

        Graphics.SetRenderTarget(_target);
        Renderer.PrimitiveAlphaTestRange = null;
        Renderer.Begin();

        Renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, _image.Width, _image.Height),
                                     GorgonColors.White,
                                     _image,
                                     new GorgonRectangleF(0, 0, 1, 1),
                                     0,
                                     GorgonSamplerState.PointFiltering);

        // Remove the area where the sprite is located.
        Renderer.DrawFilledRectangle(clearRegion, GorgonColors.BlackTransparent);

        Renderer.End();
        Renderer.PrimitiveAlphaTestRange = prevAlphaTest;
        Graphics.SetRenderTarget(prevTarget);
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void OnRenderContent()
    {
        if (_image is null)
        {
            return;
        }

        Vector2 halfRegion = new(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f);

        RenderTexture();

        Renderer.Begin(camera: Camera);
        Renderer.DrawFilledRectangle(new GorgonRectangleF(halfRegion.X,
                                                       halfRegion.Y,
                                                       RenderRegion.Width,
                                                       RenderRegion.Height),
                                    GorgonColors.White,
                                    _targetTexture,
                                    new GorgonRectangleF(0, 0, 1, 1),
                                    textureSampler: GorgonSamplerState.PointFiltering);
        Renderer.End();

        Renderer.Begin();
        _clipper.Render();
        Renderer.End();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        RenderRegion = new GorgonRectangleF(0, 0, _context.Texture?.Width ?? ClientSize.X, _context.Texture?.Height ?? ClientSize.Y);

        if (_context.Texture is not null)
        {
            _image = _context.Texture.ToTexture2DView(Graphics, new GorgonTexture2DLoadOptions
            {
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable
            });
            RenderRegion = GorgonRectangleF.Expand(RenderRegion, 32);

            CreateTarget();
        }

        _clipper.Camera = Camera;
        _clipper.AllowManualInput = false;
        _clipper.ClipAgainstBoundaries = true;
        _clipper.Bounds = RenderRegion;
        _clipper.Rectangle = _context.Region;

        _context.PropertyChanged += Context_PropertyChanged;
        _context.PropertyChanging += Context_PropertyChanging;
        _clipper.RectChanged += Clipper_RectChanged;

        MoveTo(Vector2.Zero, -1);
    }

    /// <summary>
    /// Function called when the renderer needs to clean up any resource data.
    /// </summary>
    /// <remarks>Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad" /> method. Failure to do so can cause memory leakage.</remarks>
    protected override void OnUnload()
    {
        _clipper.RectChanged -= Clipper_RectChanged;
        _context.PropertyChanged -= Context_PropertyChanged;
        _context.PropertyChanging -= Context_PropertyChanging;

        _targetTexture?.Dispose();
        _target?.Dispose();
        _image?.Dispose();
        base.OnUnload();
    }

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

    /// <summary>Function to create resources required for the lifetime of the viewer.</summary>
    public void CreateResources()
    {
        using MemoryStream stream = CommonEditorResources.MemoryStreamManager.GetStream(Resources.SpriteEditor_Bg_1024x1024);
        _noImage = GorgonTexture2DView.FromStream(Graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
        {
            Name = "Font Editor - No texture",
            Usage = ResourceUsage.Immutable
        });

        _context = DataContext.TextureEditor.TextureBrush;
    }

    /// <summary>
    /// Function to set the view to a default zoom level.
    /// </summary>
    public void DefaultZoom() => MoveTo(Vector2.Zero, 1);


}
