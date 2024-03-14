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
// Created: May 22, 2020 7:07:48 PM
// 
#endregion

using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A renderer to use with the texture wrap editing tool.
/// </summary>
internal class TextureWrapViewer
    : SingleSpriteViewer
{
    #region Methods.
    /// <summary>Handles the PropertyChanged event of the WrappingEditor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void WrappingEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISpriteTextureWrapEdit.CurrentSampler):
                Sprite.TextureSampler = DataContext.WrappingEditor.CurrentSampler == GorgonSamplerState.Default ? null : DataContext.WrappingEditor.CurrentSampler;
                break;
        }
    }

    /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
    /// <remarks>
    /// Developers should always override this method if they've overridden the <see cref="Rendering.DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.
    /// </remarks>
    protected override void OnUnload()
    {
        DataContext.WrappingEditor.PropertyChanged -= WrappingEditor_PropertyChanged;
        base.OnUnload();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        DX.RectangleF rect = new DX.RectangleF(-DataContext.Size.Width - (DataContext.Size.Width * 0.5f),
                                               -DataContext.Size.Height - (DataContext.Size.Height * 0.5f),
                                               DataContext.Size.Width * 3,
                                               DataContext.Size.Height * 3).Truncate();
        Sprite.Position = Vector2.Zero;
        Sprite.CornerColors.SetAll(GorgonColor.White);
        Sprite.CornerOffsets.SetAll(Vector3.Zero);
        Sprite.TextureSampler = DataContext.WrappingEditor.CurrentSampler == GorgonSamplerState.Default ? null : DataContext.WrappingEditor.CurrentSampler;
        Sprite.Size = rect.Size;
        Sprite.Anchor = new Vector2(0.5f, 0.5f);
        Sprite.TextureRegion = new DX.RectangleF
        {
            Left = Sprite.TextureRegion.Left - Sprite.TextureRegion.Width,
            Top = Sprite.TextureRegion.Top - Sprite.TextureRegion.Height,
            Right = Sprite.TextureRegion.Right + Sprite.TextureRegion.Width,
            Bottom = Sprite.TextureRegion.Bottom + Sprite.TextureRegion.Height
        };
        RenderRegion = rect;

        DataContext.WrappingEditor.PropertyChanged += WrappingEditor_PropertyChanged;
    }

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom()
    {
        if (DataContext?.Texture is null)
        {
            return;
        }

        DX.RectangleF zoomRect = new DX.RectangleF(-DataContext.Size.Width - (DataContext.Size.Width * 0.5f),
                                                   -DataContext.Size.Height - (DataContext.Size.Height * 0.5f),
                                                   DataContext.Size.Width * 3,
                                                   DataContext.Size.Height * 3).Truncate();

        ZoomLevels spriteZoomLevel = GetNearestZoomFromRectangle(RenderRegion);

        Vector3 spritePosition = Camera.Unproject(new Vector3(zoomRect.X + zoomRect.Width * 0.5f, zoomRect.Y + zoomRect.Height * 0.5f, 0));

        ForceMoveTo(new Vector2(spritePosition.X, spritePosition.Y), spriteZoomLevel.GetScale(), true);
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="TextureWrapViewer"/> class.</summary>
    /// <param name="renderer">The 2D renderer for the application.</param>
    /// <param name="swapChain">The swap chain for the render area.</param>
    /// <param name="dataContext">The sprite view model.</param>        
    public TextureWrapViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext)
        : base(typeof(SpriteTextureWrapEdit).FullName, renderer, swapChain, dataContext) => CanZoom = CanPanVertically = CanPanHorizontally = false;
    #endregion
}
