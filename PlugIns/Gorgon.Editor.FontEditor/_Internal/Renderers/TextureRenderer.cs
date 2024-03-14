#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: August 3, 2020 4:40:15 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using DX = SharpDX;
using Gorgon.Editor.FontEditor;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;
using Gorgon.Math;
using System.Windows.Forms;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// This is a renderer that will render the textures that make up the font.
/// </summary>
internal class TextureRenderer
    : DefaultContentRenderer<IFontContent>
{
    #region Variables.
    // The editor context.
    private ITextureEditorContext _context;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve the batch state for rendering.
    /// </summary>
    /// <returns>The rendering batch state.</returns>
    private Gorgon2DBatchState GetBatch() => DataContext.WorkingFont.UsePremultipliedTextures ? Gorgon2DBatchState.PremultipliedBlend : null;

    /// <summary>
    /// Function to calculate the render region width so that all textures may be displayed.
    /// </summary>
    private void CalculateRenderRegion()
    {
        if ((DataContext?.WorkingFont is null) || (_context.Textures.Count == 0))
        {
            RenderRegion = new DX.RectangleF(0, 0, ClientSize.Width, ClientSize.Height);
            return;
        }            

        float height = DataContext.WorkingFont.TextureHeight > ClientSize.Height ? DataContext.WorkingFont.TextureHeight : ClientSize.Height;
        float width = DataContext.WorkingFont.TextureWidth > ClientSize.Width ? DataContext.WorkingFont.TextureWidth : ClientSize.Width;

        RenderRegion = new DX.RectangleF(0, 0, width, height);
    }

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(IFontContent.WorkingFont):
                CalculateRenderRegion();                    
                break;
        }            
    }

    /// <summary>Function to render the background.</summary>
    /// <remarks>Developers can override this method to render a custom background.</remarks>
    protected override void OnRenderBackground()
    {
        var textureSize = new DX.RectangleF(0, 0, (float)ClientSize.Width / BackgroundPattern.Width, (float)ClientSize.Height / BackgroundPattern.Height);

        Renderer.Begin();
        Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, ClientSize.Width, ClientSize.Height), GorgonColor.White, BackgroundPattern, textureSize, textureSampler: GorgonSamplerState.PointFilteringWrapping);
        Renderer.End();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void OnRenderContent()
    {
        if ((DataContext.WorkingFont is null) || (_context.Textures.Count == 0))
        {
            return;
        }

        GorgonTexture2DView texture;
        DX.Size2F textureSize;
        DX.RectangleF textureRegion;

        Renderer.Begin(GetBatch(), Camera);

        float xPos = 0;
        for (int l = _context.SelectedTexture; l >= 0; --l)
        {
            texture = _context.Textures[l];

            for (int a = _context.SelectedArrayIndex - 1; a >= 0; --a)
            {                    
                textureSize = new(texture.Width * 0.9f, texture.Height * 0.9f);
                xPos -= textureSize.Width + 8;
                textureRegion = new DX.RectangleF((-textureSize.Width * 0.5f) + xPos, -textureSize.Height * 0.5f, textureSize.Width, textureSize.Height).Truncate();
                DX.RectangleF screenRegion = ToClient(textureRegion);

                if ((screenRegion.Left >= ClientSize.Width) || (screenRegion.Right < 0))
                {
                    break;
                }

                Renderer.DrawFilledRectangle(textureRegion, new GorgonColor(GorgonColor.Black, 0.25f));
                Renderer.DrawFilledRectangle(textureRegion, new GorgonColor(GorgonColor.White, 0.25f), texture, new DX.RectangleF(0, 0, 1, 1), a, GorgonSamplerState.PointFiltering);                    
            }
        }

        xPos = 0;
        for (int r = _context.SelectedTexture; r < _context.Textures.Count; ++r)
        {
            texture = _context.Textures[r];

            for (int a = _context.SelectedArrayIndex + 1; a < texture.Texture.ArrayCount; ++a)
            {
                textureSize = new(texture.Width * 0.9f, texture.Height * 0.9f);
                xPos += textureSize.Width + 8;
                textureRegion = new DX.RectangleF((-textureSize.Width * 0.5f) + xPos, -textureSize.Height * 0.5f, textureSize.Width, textureSize.Height).Truncate();
                DX.RectangleF screenRegion = ToClient(textureRegion);

                if ((screenRegion.Left >= ClientSize.Width) || (screenRegion.Right < 0))
                {
                    break;
                }

                Renderer.DrawFilledRectangle(textureRegion, new GorgonColor(GorgonColor.Black, 0.25f));
                Renderer.DrawFilledRectangle(textureRegion, new GorgonColor(GorgonColor.White, 0.25f), texture, new DX.RectangleF(0, 0, 1, 1), a, GorgonSamplerState.PointFiltering);
            }
        }

        texture = _context.Textures[_context.SelectedTexture];
        textureSize = new(texture.Width, texture.Height);
        textureRegion = new DX.RectangleF(textureSize.Width * -0.5f, textureSize.Height * -0.5f, textureSize.Width, textureSize.Height).Truncate();
        Renderer.DrawFilledRectangle(textureRegion, new GorgonColor(GorgonColor.Black, 0.75f));
        Renderer.DrawFilledRectangle(textureRegion, GorgonColor.White, texture, new DX.RectangleF(0, 0, 1, 1), _context.SelectedArrayIndex, GorgonSamplerState.PointFiltering);

        Renderer.End();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad() => CalculateRenderRegion();

    /// <summary>Function to create resources required for the lifetime of the viewer.</summary>
    public void CreateResources() => _context = DataContext.TextureEditor;

    /// <summary>
    /// Function to set the view to a default zoom level.
    /// </summary>
    public void DefaultZoom() => MoveTo(Vector2.Zero, 1);
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="FontRenderer"/> class.</summary>
    /// <param name="renderer">The 2D renderer used to render our font.</param>
    /// <param name="mainRenderTarget">The main render target for the view.</param>
    /// <param name="dataContext">The view model for our text data.</param>
    public TextureRenderer(Gorgon2D renderer, GorgonSwapChain mainRenderTarget, IFontContent dataContext)
        : base("TextureEditor", renderer, mainRenderTarget, dataContext) => CanZoom = true;
    #endregion
}
