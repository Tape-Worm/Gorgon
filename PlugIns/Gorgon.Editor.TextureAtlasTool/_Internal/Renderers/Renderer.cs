
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
using Gorgon.Editor.Rendering;
using Gorgon.Editor.TextureAtlasTool.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using DX = SharpDX;

namespace Gorgon.Editor.TextureAtlasTool;

/// <summary>
/// The renderer used to draw the texture and sprites
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="Renderer"/> class.</remarks>
/// <param name="renderer">The 2D renderer for the application.</param>
/// <param name="swapChain">The swap chain bound to the window.</param>
/// <param name="dataContext">The data context for the renderer.</param>
internal class Renderer(Gorgon2D renderer, GorgonSwapChain swapChain, ITextureAtlas dataContext)
                : DefaultToolRenderer<ITextureAtlas>("Atlas Renderer", renderer, swapChain, dataContext)
{

    // The camera used to render.
    private GorgonOrthoCamera _camera;
    // The sprite used to display the texture.
    private GorgonSprite _textureSprite;



    /// <summary>
    /// Function to draw a message on the screen when no atlas is loaded.
    /// </summary>
    private void DrawMessage()
    {
        DX.Size2F textSize = Resources.GORTAG_TEXT_NO_ATLAS.MeasureText(Renderer.DefaultFont, false);

        Renderer.Begin(camera: _camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(-MainRenderTarget.Width * 0.5f, -MainRenderTarget.Height * 0.5f, MainRenderTarget.Width, MainRenderTarget.Height), new GorgonColor(GorgonColors.White, 0.75f));
        Renderer.DrawString(Resources.GORTAG_TEXT_NO_ATLAS, new Vector2((int)(-textSize.Width * 0.5f), (int)(-textSize.Height * 0.5f)), color: GorgonColors.Black);
        Renderer.End();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void OnRenderContent()
    {
        OnRenderBackground();

        if ((DataContext.Atlas is null) || (DataContext.Atlas.Textures.Count == 0))
        {
            DrawMessage();
            return;
        }

        GorgonTexture2DView texture = _textureSprite.Texture = DataContext.Atlas.Textures[DataContext.PreviewTextureIndex];

        float scale = CalculateScaling(new DX.Size2F(texture.Width + 8, texture.Height + 8), new DX.Size2F(MainRenderTarget.Width, MainRenderTarget.Height));
        _textureSprite.Size = new DX.Size2F(scale * texture.Width, scale * texture.Height).Truncate();
        _textureSprite.Position = new Vector2(-_textureSprite.Size.Width * 0.5f, -_textureSprite.Size.Height * 0.5f).Truncate();

        Renderer.Begin(camera: _camera);
        _textureSprite.TextureArrayIndex = DataContext.PreviewArrayIndex;
        Renderer.DrawSprite(_textureSprite);

        Renderer.End();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultToolRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        _camera = new GorgonOrthoCamera(Graphics, new DX.Size2F(MainRenderTarget.Width, MainRenderTarget.Height))
        {
            Anchor = new Vector2(0.5f, 0.5f)
        };

        GorgonTexture2DView texture = (DataContext.Atlas is not null) && (DataContext.Atlas.Textures.Count > 0) ? DataContext.Atlas.Textures[0] : null;

        _textureSprite = new GorgonSprite
        {
            Texture = texture,
            TextureRegion = new DX.RectangleF(0, 0, 1, 1),
            Size = new DX.Size2F(texture is not null ? texture.Width : 1, texture is not null ? texture.Height : 1),
            TextureSampler = GorgonSamplerState.PointFiltering
        };
    }


}
