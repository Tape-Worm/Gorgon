#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 2, 2019 5:24:26 PM
// 
#endregion

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A renderer for a single sprite (without background).
/// </summary>
internal class SingleSpriteViewer
    : SpriteViewer
{
    #region Variables.
    // The controller for animating the content.
    private readonly ImageAnimationController _animController = new();
    private IGorgonAnimation _opacityAnimation;
    private readonly GorgonAnimationBuilder _animationBuilder = new();
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return whether the opactiy animation is playing.
    /// </summary>
    protected override bool IsAnimating => _animController.State == AnimationState.Playing;

    /// <summary>
    /// Property to return the boundaries of the sprite.
    /// </summary>
    protected DX.RectangleF SpriteRegion
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the sprite used for display.
    /// </summary>
    protected GorgonSprite Sprite
    {
        get;
        private set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to update the sprite colors.
    /// </summary>
    private void UpdateSpriteColors()
    {
        // We keep this separate so the opactiy animation can update the sprite vertex color values.
        for (int i = 0; i < Sprite.CornerColors.Count; ++i)
        {
            GorgonColor color = DataContext.VertexColors[i];
            Sprite.CornerColors[i] = new GorgonColor(color, SpriteOpacity.Min(color.Alpha));
        }
    }

    /// <summary>
    /// Function to update the sprite for rendering.
    /// </summary>
    protected void UpdateSprite()
    {
        UpdateSpriteColors();
        for (int i = 0; i < DataContext.VertexOffsets.Count; ++i)
        {
            Sprite.CornerOffsets[i] = DataContext.VertexOffsets[i];
        }

        Sprite.Texture = DataContext.Texture;
        Sprite.TextureRegion = DataContext.TextureCoordinates;
        Sprite.TextureArrayIndex = DataContext.ArrayIndex;
        Sprite.TextureSampler = DataContext.IsPixellated ? GorgonSamplerState.PointFiltering : GorgonSamplerState.Default;

        Sprite.Position = Vector2.Zero;
        Sprite.Anchor = new Vector2(0.5f, 0.5f);
        Sprite.Size = DataContext.Size;

        SpriteRegion = Renderer.MeasureSprite(Sprite);
    }

    /// <summary>
    /// Function to animate the sprite texture opacity.
    /// </summary>
    protected override void AnimateTexture()
    {
        _animController.Stop();

        _opacityAnimation = _animationBuilder.Clear()
                                             .EditSingle(nameof(ISpriteViewer.TextureOpacity))
                                             .SetInterpolationMode(TrackInterpolationMode.Spline)
                                             .SetKey(new GorgonKeySingle(0, TextureOpacity))
                                             .SetKey(new GorgonKeySingle(0.35f, 0.0f))
                                             .EndEdit()
                                             .EditSingle(nameof(ISpriteViewer.SpriteOpacity))
                                             .SetInterpolationMode(TrackInterpolationMode.Spline)
                                             .SetKey(new GorgonKeySingle(0, SpriteOpacity))
                                             .SetKey(new GorgonKeySingle(0.525f, Sprite.CornerColors.Max(item => item.Alpha)))
                                             .EndEdit()
                                             .Build("OpacityAnimation");

        _animController.Play(this, _opacityAnimation);
    }

    /// <summary>Function to render the background.</summary>
    /// <remarks>Developers can override this method to render a custom background.</remarks>
    protected override void OnRenderBackground()
    {
        var textureSize = new DX.RectangleF(0, 0, RenderRegion.Width / BackgroundPattern.Width * Camera.Zoom.X, RenderRegion.Height / BackgroundPattern.Height * Camera.Zoom.X);

        Renderer.Begin(camera: Camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f, RenderRegion.Width, RenderRegion.Height), new GorgonColor(GorgonColor.White, TextureOpacity), BackgroundPattern, textureSize);            
        Renderer.End();

        Renderer.Begin();
        textureSize = new DX.RectangleF(0, 0, ClientSize.Width / BackgroundPattern.Width, ClientSize.Height / BackgroundPattern.Height);
        Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, ClientSize.Width, ClientSize.Height), new GorgonColor(GorgonColor.White, 1.0f - TextureOpacity), BackgroundPattern, textureSize);
        Renderer.End();
    }

    /// <summary>Function to draw the sprite.</summary>
    protected override void DrawSprite()
    {
        base.DrawSprite();

        var halfRegion = new Vector2(DataContext.Texture.Width * -0.5f, DataContext.Texture.Height * -0.5f);


        Renderer.Begin(camera: Camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(halfRegion.X,
                                                       halfRegion.Y,
                                                       DataContext.Texture.Width,
                                                       DataContext.Texture.Height),
                                    new GorgonColor(GorgonColor.White, TextureOpacity),
                                    Sprite.Texture,
                                    new DX.RectangleF(0, 0, 1, 1),
                                    textureSampler: GorgonSamplerState.PointFiltering);

        Renderer.DrawSprite(Sprite);
        Renderer.End();

        if (!IsAnimating)
        {
            return;
        }

        _animController.Update();
        UpdateSpriteColors();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    protected override void OnLoad()
    {
        SpriteOpacity = 0;
        TextureOpacity = 1;

        base.OnLoad();

        RenderRegion = new DX.RectangleF(0, 0, DataContext.Texture.Width, DataContext.Texture.Height);

        UpdateSprite();
    }

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom()
    {
        if (DataContext?.Texture is null)
        {
            return;
        }

        DX.RectangleF zoomRect = SpriteRegion;
        zoomRect.Inflate(zoomRect.Width * 0.25f, zoomRect.Height * 0.25f);

        ZoomLevels spriteZoomLevel = GetNearestZoomFromRectangle(zoomRect);

        Vector3 spritePosition = Camera.Unproject(new Vector3(zoomRect.X + zoomRect.Width * 0.5f, zoomRect.Y + zoomRect.Height * 0.5f, 0));

        ForceMoveTo(new Vector2(spritePosition.X, spritePosition.Y), spriteZoomLevel.GetScale(), true);
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="SingleSpriteViewer"/> class.</summary>
    /// <param name="name">The name of the renderer.</param>
    /// <param name="renderer">The 2D renderer for the application </param>
    /// <param name="swapChain">The swap chain for the render area.</param>
    /// <param name="dataContext">The graphics interface for the application.</param>
    protected SingleSpriteViewer(string name, Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext)
        : base(name, renderer, swapChain, dataContext) => Sprite = new GorgonSprite();            
    #endregion
}
