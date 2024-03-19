
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


using System.Numerics;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// This is a renderer that will print our text into the view
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FontRenderer"/> class.</remarks>
/// <param name="renderer">The 2D renderer used to render our font.</param>
/// <param name="mainRenderTarget">The main render target for the view.</param>
/// <param name="dataContext">The view model for our text data.</param>
internal class FontRenderer(Gorgon2D renderer, GorgonSwapChain mainRenderTarget, IFontContent dataContext)
        : DefaultContentRenderer<IFontContent>("FontRenderer", renderer, mainRenderTarget, dataContext)
{

    // The sprite used to render our text data.
    private GorgonTextSprite _textSprite;



    /// <summary>
    /// Function to retrieve the batch state for rendering.
    /// </summary>
    /// <returns>The rendering batch state.</returns>
    private Gorgon2DBatchState GetBatch() => DataContext.WorkingFont.UsePremultipliedTextures ? Gorgon2DBatchState.PremultipliedBlend : null;

    /// <summary>Function called when a property on the <see cref="P:Gorgon.Editor.Rendering.DefaultContentRenderer`1.DataContext" /> is changing.</summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanging(string propertyName)
    {
        base.OnPropertyChanging(propertyName);

        switch (propertyName)
        {
            case nameof(IFontContent.WorkingFont):
                _textSprite.Font = null;
                break;
        }
    }

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(IFontContent.FontUnits):
                _textSprite.TextureSampler = DataContext.FontUnits == GorgonFontHeightMode.Points ? GorgonSamplerState.AnisotropicFiltering : GorgonSamplerState.PointFiltering;
                break;
            case nameof(IFontContent.WorkingFont):
                DX.Size2F regionSize = Resources.GORFNT_TEXT_DEFAULT_MAIN_PREVIEW.MeasureText(DataContext.WorkingFont, DataContext.WorkingFont.HasOutline, wordWrapWidth: ClientSize.Width).Floor();
                RenderRegion = new DX.RectangleF(0, 0, regionSize.Width + 32, regionSize.Height + 32);

                _textSprite.Font = DataContext.WorkingFont;
                _textSprite.Text = Resources.GORFNT_TEXT_DEFAULT_MAIN_PREVIEW.WordWrap(DataContext.WorkingFont, ClientSize.Width);
                break;
        }
    }

    /// <summary>Function to render the background.</summary>
    /// <remarks>Developers can override this method to render a custom background.</remarks>
    protected override void OnRenderBackground()
    {
        base.OnRenderBackground();

        Renderer.Begin(camera: Camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f, RenderRegion.Width, RenderRegion.Height), new GorgonColor(GorgonColors.Black, 0.35f));
        Renderer.End();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void OnRenderContent()
    {
        if ((DataContext?.WorkingFont is null) || (_textSprite.Font != DataContext.WorkingFont))
        {
            return;
        }

        _textSprite.DrawMode = TextDrawMode.OutlinedGlyphs;

        Renderer.Begin(GetBatch(), Camera);

        // Draw a fake shadow.
        Vector2 basePosition = new Vector2(-(RenderRegion.Size.Width - 32) * 0.5f, -(RenderRegion.Size.Height - 32) * 0.5f).Floor();

        _textSprite.Position = basePosition + ((DataContext.WorkingFont.OutlineSize == 0) ? new Vector2(2) : new Vector2((DataContext.WorkingFont.OutlineSize * 0.5f).Max(2)).Floor());
        _textSprite.OutlineTint =
        _textSprite.Color = new GorgonColor(GorgonColors.Black * 0.25f, 0.25f);
        Renderer.DrawTextSprite(_textSprite);

        _textSprite.Position = basePosition;
        _textSprite.OutlineTint =
        _textSprite.Color = GorgonColors.White;
        Renderer.DrawTextSprite(_textSprite);

        Renderer.End();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        if (DataContext?.WorkingFont is null)
        {
            return;
        }

        DX.Size2F regionSize = Resources.GORFNT_TEXT_DEFAULT_MAIN_PREVIEW.MeasureText(DataContext.WorkingFont, DataContext.WorkingFont.HasOutline, wordWrapWidth: ClientSize.Width);
        RenderRegion = new DX.RectangleF(0, 0, regionSize.Width + 32, regionSize.Height + 32);
        _textSprite.Font = DataContext.WorkingFont;
        _textSprite.Text = Resources.GORFNT_TEXT_DEFAULT_MAIN_PREVIEW.WordWrap(DataContext.WorkingFont, ClientSize.Width);
        _textSprite.TextureSampler = DataContext.FontUnits == GorgonFontHeightMode.Points ? GorgonSamplerState.AnisotropicFiltering : GorgonSamplerState.PointFiltering;
    }

    /// <summary>Function to create resources required for the lifetime of the viewer.</summary>
    public void CreateResources()
        => _textSprite = new GorgonTextSprite(DataContext.WorkingFont)
        {
            Color = GorgonColors.Black,
            Text = "Should not see me"
        };

    /// <summary>
    /// Function to set the view to a default zoom level.
    /// </summary>
    public void DefaultZoom() => MoveTo(Vector2.Zero, -1);


}
