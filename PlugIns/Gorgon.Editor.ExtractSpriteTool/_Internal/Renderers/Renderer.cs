
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
using Gorgon.Editor.ExtractSpriteTool.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using DX = SharpDX;

namespace Gorgon.Editor.ExtractSpriteTool;

/// <summary>
/// The renderer used to draw the texture and sprites
/// </summary>
internal class Renderer
        : DefaultToolRenderer<IExtract>
{

    // The camera for viewing the scene.
    private GorgonOrthoCamera _camera;
    // The sprite used to display the texture.
    private GorgonSprite _textureSprite;
    // Inverted rendering.
    private Gorgon2DBatchState _inverted;
    // The sprite to display for preview.
    private readonly GorgonSprite _previewSprite = new();



    /// <summary>
    /// Function to update the preview sprite.
    /// </summary>
    /// <param name="scale">The current scale factor.</param>
    private void UpdatePreviewSprite(float scale)
    {
        GorgonSprite currentSprite = DataContext.Sprites[DataContext.CurrentPreviewSprite];
        _previewSprite.Size = new DX.Size2F(currentSprite.Size.Width * scale, currentSprite.Size.Height * scale);
        _previewSprite.TextureRegion = currentSprite.TextureRegion;
        _previewSprite.TextureArrayIndex = currentSprite.TextureArrayIndex;
    }

    /// <summary>
    /// Function to render the sprite preview.
    /// </summary>
    private void DrawPreview()
    {
        GorgonSprite sprite = DataContext.Sprites[DataContext.CurrentPreviewSprite];
        float scale = CalculateScaling(new DX.Size2F(sprite.Size.Width + 8, sprite.Size.Height + 8), new DX.Size2F(MainRenderTarget.Width, MainRenderTarget.Height));
        UpdatePreviewSprite(scale);

        Renderer.Begin(camera: _camera);
        Renderer.DrawSprite(_previewSprite);
        Renderer.End();
    }

    /// <summary>
    /// Function to draw the wait panel for generating sprites.
    /// </summary>
    private void DrawWaitingPanel()
    {
        ref readonly ProgressData prog = ref DataContext.ExtractTaskProgress;
        string text = string.Format(Resources.GOREST_PROGRESS_SPR_GEN, prog.Current, prog.Total);

        DX.Size2F textSize = text.MeasureText(Renderer.DefaultFont, false);
        var pos = new Vector2(MainRenderTarget.Width * 0.5f - textSize.Width * 0.5f, MainRenderTarget.Height * 0.5f - textSize.Height * 0.5f);

        float percent = (float)prog.Current / prog.Total;
        var barOutline = new DX.RectangleF(pos.X, MainRenderTarget.Height * 0.5f - (textSize.Height + 4) * 0.5f,
                                        textSize.Width + 4, textSize.Height + 8);
        var bar = new DX.RectangleF(barOutline.X + 1, barOutline.Y + 1, ((barOutline.Width - 2) * percent), barOutline.Height - 2);

        Renderer.Begin();
        Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, MainRenderTarget.Width, MainRenderTarget.Height),
                                    new GorgonColor(GorgonColor.White, 0.80f));
        Renderer.DrawString(text, pos, color: GorgonColor.Black);
        Renderer.DrawString(Resources.GOREST_TEXT_GEN_CANCEL_MSG, new Vector2(pos.X, bar.Bottom + 4), color: GorgonColor.Black);
        Renderer.DrawRectangle(barOutline, GorgonColor.Black, 2);
        Renderer.End();

        Renderer.Begin(_inverted);
        Renderer.DrawFilledRectangle(bar, GorgonColor.White);
        Renderer.End();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void OnRenderContent()
    {
        Graphics.SetRenderTarget(MainRenderTarget);

        OnRenderBackground();

        if (DataContext.IsInSpritePreview)
        {
            DrawPreview();
            return;
        }

        float scale = CalculateScaling(new DX.Size2F(DataContext.Texture.Width + 8, DataContext.Texture.Height + 8), new DX.Size2F(MainRenderTarget.Width, MainRenderTarget.Height));
        _textureSprite.Size = new DX.Size2F(scale * DataContext.Texture.Width, scale * DataContext.Texture.Height).Truncate();
        _textureSprite.Position = new Vector2(-_textureSprite.Size.Width * 0.5f, -_textureSprite.Size.Height * 0.5f).Truncate();

        Renderer.Begin(camera: _camera);

        _textureSprite.TextureArrayIndex = DataContext.StartArrayIndex;
        Renderer.DrawSprite(_textureSprite);

        Renderer.End();

        Renderer.Begin(_inverted, _camera);
        var pos = new Vector2(_textureSprite.Position.X + (DataContext.GridOffset.X * scale), _textureSprite.Position.Y + (DataContext.GridOffset.Y * scale));

        int maxWidth = (int)((DataContext.CellSize.Width * DataContext.GridSize.Width) * scale);
        int maxHeight = (int)((DataContext.CellSize.Height * DataContext.GridSize.Height) * scale);

        Renderer.DrawRectangle(new DX.RectangleF(pos.X - 1, pos.Y - 1, maxWidth + 2, maxHeight + 2),
                                GorgonColor.DeepPink);

        for (int x = 1; x < DataContext.GridSize.Width; ++x)
        {
            var start = new Vector2((int)((x * DataContext.CellSize.Width) * scale) + pos.X, pos.Y);
            var end = new Vector2((int)((x * DataContext.CellSize.Width) * scale) + pos.X, pos.Y + maxHeight);

            Renderer.DrawLine(start.X, start.Y, end.X, end.Y, GorgonColor.DeepPink);
        }

        for (int y = 1; y < DataContext.GridSize.Height; ++y)
        {
            var start = new Vector2(pos.X, (int)((y * DataContext.CellSize.Height) * scale) + pos.Y);
            var end = new Vector2(pos.X + maxWidth, (int)((y * DataContext.CellSize.Height) * scale) + pos.Y);

            Renderer.DrawLine(start.X, start.Y, end.X, end.Y, GorgonColor.DeepPink);
        }

        Renderer.End();

        if (DataContext.IsGenerating)
        {
            DrawWaitingPanel();
        }
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

        var builder = new Gorgon2DBatchStateBuilder();
        _inverted = builder.BlendState(GorgonBlendState.Inverted)
                            .Build();

        _textureSprite = new GorgonSprite
        {
            Texture = DataContext.Texture,
            TextureRegion = new DX.RectangleF(0, 0, 1, 1),
            Size = new DX.Size2F(DataContext.Texture.Width, DataContext.Texture.Height),
            TextureSampler = GorgonSamplerState.PointFiltering
        };

        _previewSprite.Texture = DataContext.Texture;
    }



    /// <summary>Initializes a new instance of the <see cref="Renderer"/> class.</summary>
    /// <param name="renderer">The 2D renderer for the application.</param>
    /// <param name="swapChain">The swap chain bound to the window.</param>
    /// <param name="dataContext">The data context for the renderer.</param>
    public Renderer(Gorgon2D renderer, GorgonSwapChain swapChain, IExtract dataContext)
        : base("Extract Renderer", renderer, swapChain, dataContext)
    {
        _previewSprite.Anchor = new Vector2(0.5f, 0.5f);
        _previewSprite.TextureSampler = GorgonSamplerState.PointFiltering;
    }

}
