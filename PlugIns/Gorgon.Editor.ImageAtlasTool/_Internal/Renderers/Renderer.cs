
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
// Created: April 24, 2019 10:14:49 PM
// 

using System.Numerics;
using Gorgon.Editor.ImageAtlasTool.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;

namespace Gorgon.Editor.ImageAtlasTool;

/// <summary>
/// The renderer used to draw the texture and sprites
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="Renderer"/> class.</remarks>
/// <param name="renderer">The 2D renderer for the application.</param>
/// <param name="swapChain">The swap chain bound to the window.</param>
/// <param name="dataContext">The data context for the renderer.</param>
internal class Renderer(Gorgon2D renderer, GorgonSwapChain swapChain, IImageAtlas dataContext)
                : DefaultToolRenderer<IImageAtlas>("Atlas Renderer", renderer, swapChain, dataContext)
{

    // The camera used to render.
    private GorgonOrthoCamera _camera;
    // The texture to display.
    private GorgonTexture2DView _texture;
    // The sprite used to display the texture.
    private GorgonSprite _textureSprite;

    /// <summary>
    /// Function to build a texture for the current image.
    /// </summary>
    private void GetTexture()
    {
        _texture?.Dispose();

        if (DataContext?.CurrentImage.image is null)
        {
            return;
        }

        _texture = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo(DataContext.CurrentImage.image.Width,
                                                                                            DataContext.CurrentImage.image.Height,
                                                                                            DataContext.CurrentImage.image.Format)
        {
            Name = "ImagePreview",
            IsCubeMap = false,
            Usage = ResourceUsage.Immutable,
            Binding = TextureBinding.ShaderResource
        }, DataContext.CurrentImage.image);
    }

    /// <summary>
    /// Function to draw a message on the screen when no atlas is loaded.
    /// </summary>
    private void DrawMessage()
    {
        Vector2 textSize = Resources.GORIAG_TEXT_NO_ATLAS.MeasureText(Renderer.DefaultFont, false);

        Renderer.Begin(camera: _camera);
        Renderer.DrawFilledRectangle(new GorgonRectangleF(-MainRenderTarget.Width * 0.5f, -MainRenderTarget.Height * 0.5f, MainRenderTarget.Width, MainRenderTarget.Height), new GorgonColor(GorgonColors.White, 0.75f));
        Renderer.DrawString(Resources.GORIAG_TEXT_NO_ATLAS, new Vector2((int)(-textSize.X * 0.5f), (int)(-textSize.Y * 0.5f)), color: GorgonColors.Black);
        Renderer.End();
    }

    /// <summary>
    /// Function to draw the selected image.
    /// </summary>
    private void DrawImage()
    {
        string text = DataContext.CurrentImage.file?.Name ?? string.Empty;
        Vector2 textSize = text.MeasureText(Renderer.DefaultFont, false);

        float scale = CalculateScaling(new Vector2(_texture.Width + 8, _texture.Height + 8), new Vector2(MainRenderTarget.Width, MainRenderTarget.Height));
        Vector2 size = new Vector2(scale * _texture.Width, scale * _texture.Height).Truncate();
        Vector2 position = new Vector2(-size.X * 0.5f, -size.Y * 0.5f).Truncate();

        Renderer.Begin(camera: _camera);
        Renderer.DrawFilledRectangle(new GorgonRectangleF(position.X, position.Y, size.X, size.Y),
                                     GorgonColors.White,
                                     _texture,
                                     new GorgonRectangleF(0, 0, 1, 1),
                                     textureSampler: GorgonSamplerState.PointFiltering);
        Renderer.End();

        Renderer.Begin();
        Renderer.DrawFilledRectangle(new GorgonRectangleF(0, ClientSize.Y - textSize.Y - 2, ClientSize.X, textSize.Y + 4),
                                     new GorgonColor(GorgonColors.Black, 0.80f));
        Renderer.DrawString(text, new Vector2(ClientSize.X * 0.5f - textSize.X * 0.5f, ClientSize.Y - textSize.Y - 2), color: GorgonColors.White);
        Renderer.End();
    }

    /// <summary>Function called when a property on the <see cref="DefaultToolRenderer{T}.DataContext"/> is changing.</summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanging(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(IImageAtlas.CurrentImage):
                _texture?.Dispose();
                _texture = null;
                break;
        }
    }

    /// <summary>Function called when a property on the <see cref="DefaultToolRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(IImageAtlas.CurrentImage):
                GetTexture();
                break;
        }
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void OnRenderContent()
    {
        OnRenderBackground();

        if ((DataContext.Atlas is null) || (DataContext.Atlas.Textures.Count == 0))
        {
            if (_texture is not null)
            {
                DrawImage();
            }
            else
            {
                DrawMessage();
            }
            return;
        }

        GorgonTexture2DView texture = _textureSprite.Texture = DataContext.Atlas.Textures[DataContext.PreviewTextureIndex];

        float scale = CalculateScaling(new Vector2(texture.Width + 8, texture.Height + 8), new Vector2(MainRenderTarget.Width, MainRenderTarget.Height));
        _textureSprite.Size = new Vector2(scale * texture.Width, scale * texture.Height).Truncate();
        _textureSprite.Position = new Vector2(-_textureSprite.Size.X * 0.5f, -_textureSprite.Size.Y * 0.5f).Truncate();

        Renderer.Begin(camera: _camera);
        _textureSprite.TextureArrayIndex = DataContext.PreviewArrayIndex;
        Renderer.DrawSprite(_textureSprite);

        Renderer.End();
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _texture?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultToolRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        _camera = new GorgonOrthoCamera(Graphics, new Vector2(MainRenderTarget.Width, MainRenderTarget.Height))
        {
            Anchor = new Vector2(0.5f, 0.5f)
        };

        GorgonTexture2DView texture = (DataContext.Atlas is not null) && (DataContext.Atlas.Textures.Count > 0) ? DataContext.Atlas.Textures[0] : null;

        _textureSprite = new GorgonSprite
        {
            Texture = texture,
            TextureRegion = new GorgonRectangleF(0, 0, 1, 1),
            Size = new Vector2(texture is not null ? texture.Width : 1, texture is not null ? texture.Height : 1),
            TextureSampler = GorgonSamplerState.PointFiltering
        };
    }
}
