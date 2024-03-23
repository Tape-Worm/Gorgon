
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
        _previewSprite.Size = new Vector2(currentSprite.Size.X * scale, currentSprite.Size.Y * scale);
        _previewSprite.TextureRegion = currentSprite.TextureRegion;
        _previewSprite.TextureArrayIndex = currentSprite.TextureArrayIndex;
    }

    /// <summary>
    /// Function to render the sprite preview.
    /// </summary>
    private void DrawPreview()
    {
        GorgonSprite sprite = DataContext.Sprites[DataContext.CurrentPreviewSprite];
        float scale = CalculateScaling(new Vector2(sprite.Size.X + 8, sprite.Size.Y + 8), new Vector2(MainRenderTarget.Width, MainRenderTarget.Height));
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

        Vector2 textSize = text.MeasureText(Renderer.DefaultFont, false);
        Vector2 pos = new(MainRenderTarget.Width * 0.5f - textSize.X * 0.5f, MainRenderTarget.Height * 0.5f - textSize.Y * 0.5f);

        float percent = (float)prog.Current / prog.Total;
        GorgonRectangleF barOutline = new(pos.X, MainRenderTarget.Height * 0.5f - (textSize.Y + 4) * 0.5f,
                                        textSize.X + 4, textSize.Y + 8);
        GorgonRectangleF bar = new(barOutline.X + 1, barOutline.Y + 1, ((barOutline.X - 2) * percent), barOutline.Y - 2);

        Renderer.Begin();
        Renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, MainRenderTarget.Width, MainRenderTarget.Height),
                                    new GorgonColor(GorgonColors.White, 0.80f));
        Renderer.DrawString(text, pos, color: GorgonColors.Black);
        Renderer.DrawString(Resources.GOREST_TEXT_GEN_CANCEL_MSG, new Vector2(pos.X, bar.Bottom + 4), color: GorgonColors.Black);
        Renderer.DrawRectangle(barOutline, GorgonColors.Black, 2);
        Renderer.End();

        Renderer.Begin(_inverted);
        Renderer.DrawFilledRectangle(bar, GorgonColors.White);
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

        float scale = CalculateScaling(new Vector2(DataContext.Texture.Width + 8, DataContext.Texture.Height + 8), new Vector2(MainRenderTarget.Width, MainRenderTarget.Height));
        _textureSprite.Size = new Vector2(scale * DataContext.Texture.Width, scale * DataContext.Texture.Height).Truncate();
        _textureSprite.Position = new Vector2(-_textureSprite.Size.X * 0.5f, -_textureSprite.Size.Y * 0.5f).Truncate();

        Renderer.Begin(camera: _camera);

        _textureSprite.TextureArrayIndex = DataContext.StartArrayIndex;
        Renderer.DrawSprite(_textureSprite);

        Renderer.End();

        Renderer.Begin(_inverted, _camera);
        Vector2 pos = new(_textureSprite.Position.X + (DataContext.GridOffset.X * scale), _textureSprite.Position.Y + (DataContext.GridOffset.Y * scale));

        int maxWidth = (int)((DataContext.CellSize.X * DataContext.GridSize.X) * scale);
        int maxHeight = (int)((DataContext.CellSize.Y * DataContext.GridSize.Y) * scale);

        Renderer.DrawRectangle(new GorgonRectangleF(pos.X - 1, pos.Y - 1, maxWidth + 2, maxHeight + 2),
                                GorgonColors.DeepPink);

        for (int x = 1; x < DataContext.GridSize.X; ++x)
        {
            Vector2 start = new((int)((x * DataContext.CellSize.X) * scale) + pos.X, pos.Y);
            Vector2 end = new((int)((x * DataContext.CellSize.X) * scale) + pos.X, pos.Y + maxHeight);

            Renderer.DrawLine(start.X, start.Y, end.X, end.Y, GorgonColors.DeepPink);
        }

        for (int y = 1; y < DataContext.GridSize.Y; ++y)
        {
            Vector2 start = new(pos.X, (int)((y * DataContext.CellSize.Y) * scale) + pos.Y);
            Vector2 end = new(pos.X + maxWidth, (int)((y * DataContext.CellSize.Y) * scale) + pos.Y);

            Renderer.DrawLine(start.X, start.Y, end.X, end.Y, GorgonColors.DeepPink);
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

        _camera = new GorgonOrthoCamera(Graphics, new Vector2(MainRenderTarget.Width, MainRenderTarget.Height))
        {
            Anchor = new Vector2(0.5f, 0.5f)
        };

        Gorgon2DBatchStateBuilder builder = new();
        _inverted = builder.BlendState(GorgonBlendState.Inverted)
                            .Build();

        _textureSprite = new GorgonSprite
        {
            Texture = DataContext.Texture,
            TextureRegion = new GorgonRectangleF(0, 0, 1, 1),
            Size = new Vector2(DataContext.Texture.Width, DataContext.Texture.Height),
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
