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
// Created: June 10, 2020 6:38:49 AM
// 
#endregion

using System.Numerics;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The viewer for editing a single floating point key value for an aniamtion.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="DefaultAnimationViewer"/> class.</remarks>
/// <param name="renderer">The main renderer for the content view.</param>
/// <param name="swapChain">The swap chain for the content view.</param>
/// <param name="dataContext">The view model to assign to the renderer.</param>        
internal class SingleAnimationViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IAnimationContent dataContext)
        : AnimationViewer(ViewerName, renderer, swapChain, dataContext, null, true)
{
    #region Constants.
    /// <summary>
    /// The name of the viewer.
    /// </summary>
    public const string ViewerName = nameof(AnimationTrackKeyType.Single);
    #endregion

    #region Variables.
    // The starting angle for the sprite.
    private float _startAngle;
    // Flag to indicate we're dragging the angle.
    private bool _dragAngle;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to calculate an angle, in degrees, from the current mouse position.
    /// </summary>
    /// <param name="mousePos">The mouse client position.</param>
    /// <param name="currentAngle">The current angle of rotation to apply.</param>
    /// <returns>The angle of rotation, relative to the sprite.</returns>
    private float CalcAngleFromMouse(Vector2 mousePos, float currentAngle)
    {
        var pivotPos = new Vector2(Sprite.Position.X - RenderRegion.Width * Camera.Anchor.X, Sprite.Position.Y - RenderRegion.Height * Camera.Anchor.Y);
        pivotPos = ToClient(pivotPos, ClientSize);

        Vector2 dest = mousePos - pivotPos;
        dest = Vector2.Normalize(dest);

        return (dest.Y.ATan(dest.X).ToDegrees() - currentAngle).LimitAngle(-360);
    }

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(IAnimationContent.Selected):
                SupportsOnionSkinning = SelectedTrackID == TrackSpriteProperty.Angle;
                break;
        }
    }

    /// <summary>Function to handle a mouse down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
    protected override void OnMouseDown(MouseArgs args)
    {
        if ((args.MouseButtons != MouseButtons.Left) || (SelectedTrackID != TrackSpriteProperty.Angle))
        {
            base.OnMouseDown(args);
            return;
        }

        _startAngle = CalcAngleFromMouse(args.ClientPosition.ToVector2(), DataContext.KeyEditor.CurrentEditor.Value.X);
        _dragAngle = true;
        args.Handled = true;
        base.OnMouseDown(args);
    }

    /// <summary>Function to handle a mouse move event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
    protected override void OnMouseMove(MouseArgs args)
    {
        if ((args.MouseButtons != MouseButtons.Left) || (SelectedTrackID != TrackSpriteProperty.Angle))
        {
            base.OnMouseMove(args);
            return;
        }

        float angle = CalcAngleFromMouse(args.ClientPosition.ToVector2(), _startAngle);
        DataContext.KeyEditor.CurrentEditor.Value = new Vector4(angle, 0, 0, 0);

        args.Handled = true;
        base.OnMouseMove(args);
    }

    /// <summary>Function to handle a preview key down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a preview key down event in their own content view.</remarks>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
        if (SelectedTrackID != TrackSpriteProperty.Angle)
        {
            base.OnPreviewKeyDown(args);
            return;
        }

        float amount = 1;

        if (args.Control)
        {
            if (args.Shift)
            {
                amount = 180;
            }
            else
            {
                amount = 90;
            }
        }
        else if (args.Shift)
        {
            amount = 45;
        }            

        switch (args.KeyCode)
        {
            case Keys.Home:
                DataContext.KeyEditor.CurrentEditor.Value = Vector4.Zero;
                args.IsInputKey = true;
                return;
            case Keys.Left:

                DataContext.KeyEditor.CurrentEditor.Value = new Vector4((DataContext.KeyEditor.CurrentEditor.Value.X - amount).LimitAngle(-360), 0, 0, 0);
                args.IsInputKey = true;
                return;
            case Keys.Right:
                DataContext.KeyEditor.CurrentEditor.Value = new Vector4((DataContext.KeyEditor.CurrentEditor.Value.X + amount).LimitAngle(-360), 0, 0, 0);
                args.IsInputKey = true;
                return;
        }

        base.OnPreviewKeyDown(args);
    }

    /// <summary>Function to handle a mouse up event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
    protected override void OnMouseUp(MouseArgs args)
    {
        if ((args.MouseButtons != MouseButtons.Left) || (SelectedTrackID != TrackSpriteProperty.Angle))
        {
            base.OnMouseUp(args);
            return;
        }

        _dragAngle = false;
        args.Handled = true;
        base.OnMouseUp(args);
    }

    /// <summary>Function to draw any gizmos for UI components.</summary>
    protected override void DrawGizmos()
    {
        if (SelectedTrackID != TrackSpriteProperty.Angle)
        {
            return;
        }

        DX.RectangleF aabb = Renderer.MeasureSprite(Sprite);
        aabb.Offset(-RenderRegion.Width * Camera.Anchor.X, -RenderRegion.Height * Camera.Anchor.Y);
        aabb = ToClient(aabb);

        var aabbCircle = new DX.RectangleF(aabb.Left, aabb.Top, aabb.Width > aabb.Height ? aabb.Width : aabb.Height, aabb.Height > aabb.Width ? aabb.Height : aabb.Width);

        aabbCircle.X -= (aabbCircle.Width * 0.5f) - (aabb.Width * 0.5f);
        aabbCircle.Y -= (aabbCircle.Height * 0.5f) - (aabb.Height * 0.5f);            

        Renderer.Begin(Gorgon2DBatchState.InvertedBlend);
        Renderer.DrawEllipse(aabbCircle, GorgonColor.GreenPure, thickness: 4);
        Renderer.End();

        Renderer.Begin();
        DrawAnchorPoint();

        if (!_dragAngle)
        {
            Renderer.End();
            return;
        }

        Renderer.DrawFilledArc(aabbCircle, new GorgonColor(GorgonColor.SteelBlue, 0.3f), DataContext.KeyEditor.CurrentEditor.Value.X, 360.0f);
        Renderer.End();
    }

    /// <summary>Function to draw the animation.</summary>
    protected override void DrawAnimation()
    {
        if (DataContext.KeyEditor.CurrentEditor?.Track is null)
        {
            return;
        }

        Renderer.Begin();
        Renderer.DrawSprite(Sprite);
        Renderer.End();
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        SupportsOnionSkinning = SelectedTrackID == TrackSpriteProperty.Angle;
    }
    #endregion

    #region Methods.
    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom()
    {
        if (Sprite is null)
        {
            return;
        }

        ZoomToSprite(Sprite);
    }

    #endregion
}
