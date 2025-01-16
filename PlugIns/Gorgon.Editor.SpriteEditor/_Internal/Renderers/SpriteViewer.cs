
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
// Created: March 15, 2019 11:56:19 AM
// 

using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Provides rendering functionality for the sprite editor
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SpriteViewer"/> class.</remarks>
/// <param name="name">The name of the renderer.</param>
/// <param name="renderer">The main renderer for the content view.</param>
/// <param name="swapChain">The swap chain for the content view.</param>
/// <param name="dataContext">The view model to assign to the renderer.</param>        
internal abstract class SpriteViewer(string name, Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext)
        : DefaultContentRenderer<ISpriteContent>(name, renderer, swapChain, dataContext), ISpriteViewer
{

    // The controller for animating the content.
    private readonly ImageAnimationController _animController = new();
    private IGorgonAnimation _opacityAnimation;
    private readonly GorgonAnimationBuilder _animationBuilder = new();

    /// <summary>
    /// Property to return whether the opactiy animation is playing.
    /// </summary>
    protected virtual bool IsAnimating => _animController.State == AnimationState.Playing;

    /// <summary>Property to set or return the opacity of the sprite texture in the view.</summary>        
    public float TextureOpacity
    {
        get;
        set;
    }

    /// <summary>Property to set or return the opacity of the sprite in the view.</summary>        
    public float SpriteOpacity
    {
        get;
        set;
    }

    /// <summary>
    /// Function to animate the sprite texture opacity.
    /// </summary>
    protected virtual void AnimateTexture()
    {
        _animController.Stop();

        _opacityAnimation = _animationBuilder.Clear()
                                             .EditSingle(nameof(ISpriteViewer.TextureOpacity))
                                             .SetInterpolationMode(TrackInterpolationMode.Spline)
                                             .SetKey(new GorgonKeySingle(0, TextureOpacity))
                                             .SetKey(new GorgonKeySingle(0.35f, 0.5f))
                                             .EndEdit()
                                             .EditSingle(nameof(ISpriteViewer.SpriteOpacity))
                                             .SetInterpolationMode(TrackInterpolationMode.Spline)
                                             .SetKey(new GorgonKeySingle(0, SpriteOpacity))
                                             .SetKey(new GorgonKeySingle(0.525f, DataContext.VertexColors.Max(item => item.Alpha)))
                                             .EndEdit()
                                             .Build("OpacityAnimation");

        _animController.Play(this, _opacityAnimation);
    }

    /// <summary>
    /// Function to draw the sprite.
    /// </summary>
    protected virtual void DrawSprite()
    {

    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected sealed override void OnRenderContent()
    {
        base.OnRenderContent();

        DrawSprite();

        if (_animController.State == AnimationState.Playing)
        {
            _animController.Update();
        }
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        AnimateTexture();
    }

    /// <summary>
    /// Function called during resource creation.
    /// </summary>
    protected virtual void OnCreateResources()
    {
    }

    /// <summary>Function to create resources required for the lifetime of the viewer.</summary>
    public void CreateResources() => OnCreateResources();

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public abstract void DefaultZoom();

}
