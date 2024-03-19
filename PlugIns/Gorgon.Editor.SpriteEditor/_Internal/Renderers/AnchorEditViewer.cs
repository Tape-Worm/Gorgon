
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
// Created: May 19, 2020 12:37:42 PM
// 


using System.Buffers;
using System.ComponentModel;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A renderer used to render the current sprite for editing the anchor point
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="AnchorEditViewer"/> class.</remarks>
/// <param name="dataContext">The sprite view model.</param>        
/// <param name="swapChain">The swap chain for the render area.</param>
/// <param name="renderer">The 2D renderer for the application.</param>
/// <param name="anchorService">The service used to modify the anchor.</param>
internal class AnchorEditViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext, IAnchorEditService anchorService)
        : SingleSpriteViewer(typeof(SpriteAnchorEdit).FullName, renderer, swapChain, dataContext)
{

    // The service used for modifying the anchor.
    private readonly IAnchorEditService _anchorService = anchorService;
    // The controller for our animations.
    private readonly GorgonSpriteAnimationController _controller = new();
    // The scaling/rotation animation.
    private IGorgonAnimation _scaleRotateAnim;



    /// <summary>Handles the AnchorChanged event of the AnchorService control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void AnchorService_AnchorChanged(object sender, EventArgs e) => DataContext.AnchorEditor.Anchor = _anchorService.AnchorPosition;

    /// <summary>Handles the PropertyChanged event of the AnchorEditor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void AnchorEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        void StopAnimation()
        {
            if ((!DataContext.AnchorEditor.PreviewRotation) && (!DataContext.AnchorEditor.PreviewScale))
            {
                _controller.Stop();
            }
        }

        switch (e.PropertyName)
        {
            case nameof(ISpriteAnchorEdit.PreviewRotation):
                StopAnimation();
                _scaleRotateAnim.SingleTracks[GorgonSpriteAnimationController.AngleTrack.TrackName].IsEnabled = DataContext.AnchorEditor.PreviewRotation;
                if ((DataContext.AnchorEditor.PreviewRotation) && (_controller.State != AnimationState.Playing))
                {
                    _controller.Play(Sprite, _scaleRotateAnim);
                }
                else
                {
                    Sprite.Angle = 0;
                }
                break;
            case nameof(ISpriteAnchorEdit.PreviewScale):
                StopAnimation();
                _scaleRotateAnim.Vector2Tracks[GorgonSpriteAnimationController.ScaleTrack.TrackName].IsEnabled = DataContext.AnchorEditor.PreviewScale;
                if ((DataContext.AnchorEditor.PreviewScale) && (_controller.State != AnimationState.Playing))
                {
                    _controller.Play(Sprite, _scaleRotateAnim);
                }
                else
                {
                    Sprite.Scale = Vector2.One;
                }
                break;
            case nameof(ISpriteAnchorEdit.Anchor):
                _anchorService.AnchorPosition = DataContext.AnchorEditor.Anchor;
                break;
        }
    }

    /// <summary>Function to handle a mouse down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
    protected override void OnMouseDown(MouseArgs args)
    {
        args.Handled = _anchorService.MouseDown(args);
        base.OnMouseDown(args);
    }

    /// <summary>Function to handle a mouse up event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
    protected override void OnMouseUp(MouseArgs args)
    {
        args.Handled = _anchorService.MouseUp(args);
        base.OnMouseUp(args);
    }

    /// <summary>Function to handle a mouse move event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
    protected override void OnMouseMove(MouseArgs args)
    {
        args.Handled = _anchorService.MouseMove(args);
        base.OnMouseMove(args);
    }

    /// <summary>Function to handle a preview key down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a preview key down event in their own content view.</remarks>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
        switch (args.KeyCode)
        {
            case Keys.S:
                DataContext.AnchorEditor.PreviewScale = !DataContext.AnchorEditor.PreviewScale;
                args.IsInputKey = true;
                break;
            case Keys.R:
                DataContext.AnchorEditor.PreviewRotation = !DataContext.AnchorEditor.PreviewRotation;
                args.IsInputKey = true;
                break;
            case Keys.Enter when ((DataContext.AnchorEditor.OkCommand is not null) && (DataContext.AnchorEditor.OkCommand.CanExecute(null))):
                DataContext.AnchorEditor.OkCommand.Execute(null);
                args.IsInputKey = true;
                break;
            case Keys.Escape when ((DataContext.AnchorEditor.CancelCommand is not null) && (DataContext.AnchorEditor.CancelCommand.CanExecute(null))):
                DataContext.AnchorEditor.CancelCommand.Execute(null);
                args.IsInputKey = true;
                break;
            default:
                args.IsInputKey = _anchorService.KeyDown(args);
                break;
        }

        base.OnPreviewKeyDown(args);
    }

    /// <summary>Function to render the background.</summary>
    /// <remarks>Developers can override this method to render a custom background.</remarks>
    protected override void OnRenderBackground()
    {
        base.OnRenderBackground();

        Vector3 spriteTopLeft = new(SpriteRegion.TopLeft.X, SpriteRegion.TopLeft.Y, 0);
        Vector3 spriteBottomRight = new(SpriteRegion.BottomRight.X, SpriteRegion.BottomRight.Y, 0);
        Camera.Unproject(spriteTopLeft, out Vector3 transformedTopLeft);
        Camera.Unproject(spriteBottomRight, out Vector3 transformedBottomRight);

        DX.RectangleF bounds = new()
        {
            Left = transformedTopLeft.X,
            Top = transformedTopLeft.Y,
            Right = transformedBottomRight.X,
            Bottom = transformedBottomRight.Y
        };

        Renderer.Begin();
        Renderer.DrawRectangle(bounds, new GorgonColor(GorgonColors.Black, 0.30f), 4);
        Renderer.End();
    }

    /// <summary>Function to draw the sprite.</summary>
    protected override void DrawSprite()
    {
        Vector2 halfSize = new(Sprite.Size.Width * 0.5f, Sprite.Size.Height * 0.5f);
        Sprite.Anchor = new Vector2((DataContext.AnchorEditor.Anchor.X + halfSize.X) / DataContext.Size.Width,
                                       (DataContext.AnchorEditor.Anchor.Y + halfSize.Y) / DataContext.Size.Height);
        Sprite.Position = DataContext.AnchorEditor.Anchor;

        base.DrawSprite();

        Renderer.Begin();
        _anchorService.Render();
        Renderer.End();

        _controller.Update();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        _anchorService.Camera = Camera;
        RenderRegion = SpriteRegion;
        Vector2 halfSize = new(Sprite.Size.Width * 0.5f, Sprite.Size.Height * 0.5f);
        Vector2[] vertices = ArrayPool<Vector2>.Shared.Rent(4);
        vertices[0] = new Vector2(Sprite.CornerOffsets.UpperLeft.X - halfSize.X, Sprite.CornerOffsets.UpperLeft.Y - halfSize.Y).Truncate();
        vertices[1] = new Vector2(Sprite.CornerOffsets.UpperRight.X + halfSize.X, Sprite.CornerOffsets.UpperRight.Y - halfSize.Y).Truncate();
        vertices[2] = new Vector2(Sprite.CornerOffsets.LowerRight.X + halfSize.X, Sprite.CornerOffsets.LowerRight.Y + halfSize.Y).Truncate();
        vertices[3] = new Vector2(Sprite.CornerOffsets.LowerLeft.X - halfSize.X, Sprite.CornerOffsets.LowerLeft.Y + halfSize.Y).Truncate();
        DataContext.AnchorEditor.SpriteBounds = vertices;
        ArrayPool<Vector2>.Shared.Return(vertices, true);

        DataContext.AnchorEditor.Anchor = _anchorService.AnchorPosition = new Vector2(DataContext.Size.Width * DataContext.Anchor.X - halfSize.X,
                                                                                         DataContext.Size.Height * DataContext.Anchor.Y - halfSize.Y);

        GorgonAnimationBuilder builder = new();
        _scaleRotateAnim = builder.EditVector2(GorgonSpriteAnimationController.ScaleTrack.TrackName)
              .SetInterpolationMode(TrackInterpolationMode.Spline)
              .Disabled()
              .SetKey(new GorgonKeyVector2(0, new Vector2(1, 1)))
              .SetKey(new GorgonKeyVector2(2.5f, new Vector2(0.25f, 0.25f)))
              .SetKey(new GorgonKeyVector2(5.0f, new Vector2(2, 2)))
              .SetKey(new GorgonKeyVector2(10.0f, new Vector2(1, 1)))
              .EndEdit()
              .EditSingle(GorgonSpriteAnimationController.AngleTrack.TrackName)
              .SetInterpolationMode(TrackInterpolationMode.Spline)
              .Disabled()
              .SetKey(new GorgonKeySingle(0, 0))
              .SetKey(new GorgonKeySingle(5.0f, 360.0f))
              .SetKey(new GorgonKeySingle(10.0f, 0))
              .EndEdit()
              .Build("ScaleAnimation");
        _scaleRotateAnim.IsLooped = true;
        _controller.Play(Sprite, _scaleRotateAnim);

        // Enable/disable tracks based on context state.
        _scaleRotateAnim.SingleTracks[GorgonSpriteAnimationController.AngleTrack.TrackName].IsEnabled = DataContext.AnchorEditor.PreviewRotation;
        _scaleRotateAnim.Vector2Tracks[GorgonSpriteAnimationController.ScaleTrack.TrackName].IsEnabled = DataContext.AnchorEditor.PreviewRotation;

        DataContext.AnchorEditor.PropertyChanged += AnchorEditor_PropertyChanged;
        _anchorService.AnchorChanged += AnchorService_AnchorChanged;
    }

    /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
    /// <remarks>
    /// Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.
    /// </remarks>
    protected override void OnUnload()
    {
        _anchorService.AnchorChanged -= AnchorService_AnchorChanged;
        DataContext.AnchorEditor.PropertyChanged -= AnchorEditor_PropertyChanged;
        base.OnUnload();
    }


}
