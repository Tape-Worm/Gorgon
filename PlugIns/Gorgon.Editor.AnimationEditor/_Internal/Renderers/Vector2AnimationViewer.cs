﻿
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
// Created: June 10, 2020 6:38:49 AM
// 

using System.Buffers;
using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Geometry;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The viewer for editing a vector 2 key value for an aniamtion
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="DefaultAnimationViewer"/> class.</remarks>
/// <param name="renderer">The main renderer for the content view.</param>
/// <param name="swapChain">The swap chain for the content view.</param>
/// <param name="dataContext">The view model to assign to the renderer.</param>        
/// <param name="clipper">The rectangle clipper interface.</param>
/// <param name="anchorEditor">The anchor editor interface.</param>
/// <param name="vertexEditor">The editor for sprite vertices.</param>
internal class Vector2AnimationViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IAnimationContent dataContext, IRectClipperService clipper, IAnchorEditService anchorEditor, VertexEditService vertexEditor)
        : AnimationViewer(ViewerName, renderer, swapChain, dataContext, clipper, true)
{
    /// <summary>
    /// The name of the viewer.
    /// </summary>
    public const string ViewerName = nameof(AnimationTrackKeyType.Vector2);

    // The anchor editor service.
    private readonly IAnchorEditService _anchorEdit = anchorEditor;
    // The editor used to modify sprite vertices.
    private readonly VertexEditService _vertexEditor = vertexEditor;
    // Flag to indicate whether the clipper/anchor events are assigned.
    private int _clipAnchorEvent;
    // Previous angle when modifying vertices for a sprite.
    private GorgonSprite _vertexEditSprite;

    /// <summary>
    /// Function to enable the events for the clipper/anchor editor.
    /// </summary>
    private void EnableEvents()
    {
        if (Interlocked.Exchange(ref _clipAnchorEvent, 1) == 1)
        {
            return;
        }

        Clipper.RectChanged += Clipper_RectChanged;
        _anchorEdit.AnchorChanged += AnchorEdit_AnchorChanged;
        _vertexEditor.VerticesChanged += Vertex_Changed;
    }

    /// <summary>
    /// Function to disable the events for the clipper/anchor editor.
    /// </summary>
    private void DisableEvents()
    {
        if (Interlocked.Exchange(ref _clipAnchorEvent, 0) == 0)
        {
            return;
        }

        Clipper.RectChanged -= Clipper_RectChanged;
        _anchorEdit.AnchorChanged -= AnchorEdit_AnchorChanged;
        _vertexEditor.VerticesChanged -= Vertex_Changed;
    }

    /// <summary>Handles the Changed event of the Vertex control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Vertex_Changed(object sender, VertexChangedEventArgs e)
    {
        if (_vertexEditor?.SelectedVertex is null)
        {
            return;
        }

        Vector2 vertexDelta = (e.NewPosition - e.PreviousPosition).Truncate();
        DataContext.KeyEditor.CurrentEditor.Value += new Vector4(vertexDelta.X / _vertexEditSprite.Scale.X, vertexDelta.Y / _vertexEditSprite.Scale.Y, 0, 0);

        UpdateVertexEditorSprite(true);
    }

    /// <summary>Handles the AnchorChanged event of the AnchorEdit control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void AnchorEdit_AnchorChanged(object sender, EventArgs e)
    {
        Vector2 offset = new((_anchorEdit.AnchorPosition.X + Sprite.ScaledSize.X * 0.5f) / Sprite.ScaledSize.X,
                                    (_anchorEdit.AnchorPosition.Y + Sprite.ScaledSize.Y * 0.5f) / Sprite.ScaledSize.Y);

        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.AnchorAbsolute:
                DataContext.KeyEditor.CurrentEditor.Value = new Vector4(offset.X * Sprite.Size.X * 0.5f,
                                                                           offset.Y * Sprite.Size.Y * 0.5f, 0, 0);
                break;
            case TrackSpriteProperty.Anchor:
                DataContext.KeyEditor.CurrentEditor.Value = new Vector4(offset, 0, 0);
                break;
        }
    }

    /// <summary>Handles the RectChanged event of the Clipper control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void Clipper_RectChanged(object sender, EventArgs e)
    {
        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.Position:
                Vector2 offset = new Vector2(Sprite.Anchor.X * Clipper.Rectangle.Width, Sprite.Anchor.Y * Clipper.Rectangle.Height).Truncate();
                DataContext.KeyEditor.CurrentEditor.Value = new Vector4(Clipper.Rectangle.X + offset.X, Clipper.Rectangle.Y + offset.Y, 0, 0);
                break;
            case TrackSpriteProperty.Size:
                DataContext.KeyEditor.CurrentEditor.Value = new Vector4(Clipper.Rectangle.Width, Clipper.Rectangle.Height, 0, 0);
                break;
        }
    }

    /// <summary>
    /// Function to update the clipping rectangle to the size of the working sprite.
    /// </summary>
    private void UpdateClippingRectangle()
    {
        GorgonRenderTargetView oldTarget;

        oldTarget = Graphics.RenderTargets[0];

        DisableEvents();

        try
        {
            if (oldTarget == MainRenderTarget)
            {
                Clipper.Rectangle = GorgonRectangleF.Truncate(Renderer.MeasureSprite(Sprite));
                return;
            }

            // The camera relies on the target size of our swap chain for its projection, so we have to switch to our main 
            // render target temporarily.
            Graphics.SetRenderTarget(MainRenderTarget);
            Clipper.Rectangle = GorgonRectangleF.Truncate(Renderer.MeasureSprite(Sprite));
            Graphics.SetRenderTarget(oldTarget);
        }
        finally
        {
            EnableEvents();
        }
    }

    /// <summary>
    /// Function to update the vertex editor.
    /// </summary>
    private void UpdateVertexEditor()
    {
        GorgonRenderTargetView oldTarget;

        Vector2[] vertices = null;

        oldTarget = Graphics.RenderTargets[0];

        DisableEvents();

        try
        {
            _vertexEditor.SpriteCorner = SelectedTrackID;

            GorgonRectangleF bounds = Renderer.MeasureSprite(_vertexEditSprite);
            ref readonly Gorgon2DVertex[] spriteVertices = ref Renderer.GetVertices(_vertexEditSprite);
            vertices = ArrayPool<Vector2>.Shared.Rent(4);

            if (oldTarget != MainRenderTarget)
            {
                Graphics.SetRenderTarget(MainRenderTarget);
            }

            Vector2 halfRegion = new(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);
            vertices[0] = new Vector2(spriteVertices[0].Position.X - halfRegion.X, spriteVertices[0].Position.Y - halfRegion.Y);
            vertices[1] = new Vector2(spriteVertices[1].Position.X - halfRegion.X, spriteVertices[1].Position.Y - halfRegion.Y);
            vertices[2] = new Vector2(spriteVertices[3].Position.X - halfRegion.X, spriteVertices[3].Position.Y - halfRegion.Y);
            vertices[3] = new Vector2(spriteVertices[2].Position.X - halfRegion.X, spriteVertices[2].Position.Y - halfRegion.Y);

            _vertexEditor.Vertices = vertices;

            if (oldTarget != MainRenderTarget)
            {
                Graphics.SetRenderTarget(oldTarget);
            }

            UpdateVertexEditorSprite(true);
        }
        finally
        {
            if (vertices is not null)
            {
                ArrayPool<Vector2>.Shared.Return(vertices);
            }
            EnableEvents();
        }
    }

    /// <summary>
    /// Function to update the editors.
    /// </summary>
    private void RefreshEditors()
    {
        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.Scale:
            case TrackSpriteProperty.ScaledSize:
            case TrackSpriteProperty.Size:
            case TrackSpriteProperty.Anchor:
            case TrackSpriteProperty.AnchorAbsolute:
            case TrackSpriteProperty.Angle:
            case TrackSpriteProperty.Position:
                Clipper.Refresh();
                break;
            case TrackSpriteProperty.UpperLeft:
            case TrackSpriteProperty.UpperRight:
            case TrackSpriteProperty.LowerLeft:
            case TrackSpriteProperty.LowerRight:
                _vertexEditor.Refresh();
                break;
        }
    }

    /// <summary>
    /// Function to update the guide sprite for vertex editing.
    /// </summary>
    /// <param name="cornersOnly"><b>true</b> to update corner values, <b>false</b> to update everything.</param>
    private void UpdateVertexEditorSprite(bool cornersOnly)
    {
        if (!cornersOnly)
        {
            Sprite.CopyTo(_vertexEditSprite);
            _vertexEditSprite.Angle = 0;
            _vertexEditSprite.Color = new GorgonColor(GorgonColors.Black, 0.85f);
            // Flip the scale to positive.
            _vertexEditSprite.Scale = new Vector2(Sprite.Scale.X.Abs(), Sprite.Scale.Y.Abs());
            return;
        }

        _vertexEditSprite.CornerOffsets.LowerLeft = Sprite.CornerOffsets.LowerLeft;
        _vertexEditSprite.CornerOffsets.LowerRight = Sprite.CornerOffsets.LowerRight;
        _vertexEditSprite.CornerOffsets.UpperLeft = Sprite.CornerOffsets.UpperLeft;
        _vertexEditSprite.CornerOffsets.UpperRight = Sprite.CornerOffsets.UpperRight;
    }

    /// <summary>Function called when a property is changed on the keyframe editor panel.</summary>
    /// <param name="propertyName">The name of the changed property.</param>
    protected override void OnKeyFramePanelPropertyChanged(string propertyName)
    {
        base.OnKeyFramePanelPropertyChanged(propertyName);

        void UpdateSprite()
        {
            UpdateClippingRectangle();

            DisableEvents();
            try
            {
                Vector2 spritePos = new(Sprite.Position.X - RenderRegion.Width * 0.5f, Sprite.Position.Y - RenderRegion.Height * 0.5f);
                Vector2 anchorPos = Vector2.Zero;

                switch (SelectedTrackID)
                {
                    case TrackSpriteProperty.Anchor:
                        anchorPos = new Vector2(DataContext.KeyEditor.CurrentEditor.Value.X * Sprite.Size.X - Sprite.Size.X * 0.5f,
                                                   DataContext.KeyEditor.CurrentEditor.Value.Y * Sprite.Size.Y - Sprite.Size.Y * 0.5f);
                        break;
                    case TrackSpriteProperty.AnchorAbsolute:
                        anchorPos = new Vector2(DataContext.KeyEditor.CurrentEditor.Value.X - Sprite.Size.X * 0.5f,
                                                   DataContext.KeyEditor.CurrentEditor.Value.Y - Sprite.Size.Y * 0.5f);
                        break;
                    case TrackSpriteProperty.UpperLeft:
                    case TrackSpriteProperty.UpperRight:
                    case TrackSpriteProperty.LowerLeft:
                    case TrackSpriteProperty.LowerRight:
                        if (!_vertexEditor.IsDragging)
                        {
                            UpdateVertexEditor();
                        }
                        return;
                    default:
                        return;
                }

                Vector2 center = Vector2.Subtract(anchorPos, spritePos);

                _anchorEdit.AnchorPosition = anchorPos;
                _anchorEdit.CenterPosition = center;
            }
            finally
            {
                EnableEvents();
            }
        }

        switch (propertyName)
        {
            case nameof(IKeyValueEditor.Value):
                UpdateSprite();
                break;
            case nameof(IKeyValueEditor.Track):
                UpdateSprite();
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
            case nameof(IAnimationContent.Selected):
                Clipper.AllowMove = SelectedTrackID == TrackSpriteProperty.Position;
                UpdateVertexEditorSprite(false);
                break;
            case nameof(IAnimationContent.BackgroundImage):
                Clipper.Bounds = RenderRegion;
                break;
        }
    }

    /// <summary>Function to handle a mouse down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
    protected override void OnMouseDown(MouseArgs args)
    {
        if (DataContext.KeyEditor?.CurrentEditor?.Track is null)
        {
            base.OnMouseDown(args);
            return;
        }

        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.Position:
                args.Handled = Clipper.MouseDown(args);
                break;
            case TrackSpriteProperty.AnchorAbsolute:
            case TrackSpriteProperty.Anchor:
                args.Handled = _anchorEdit.MouseDown(args);
                break;
            case TrackSpriteProperty.UpperLeft:
            case TrackSpriteProperty.UpperRight:
            case TrackSpriteProperty.LowerLeft:
            case TrackSpriteProperty.LowerRight:
                args.Handled = _vertexEditor.MouseDown(args);
                break;
        }

        base.OnMouseDown(args);
    }

    /// <summary>Function to handle a mouse move event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
    protected override void OnMouseMove(MouseArgs args)
    {
        if (DataContext.KeyEditor?.CurrentEditor?.Track is null)
        {
            base.OnMouseMove(args);
            return;
        }

        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.Position:
                args.Handled = Clipper.MouseMove(args);
                break;
            case TrackSpriteProperty.AnchorAbsolute:
            case TrackSpriteProperty.Anchor:
                args.Handled = _anchorEdit.MouseMove(args);
                break;
            case TrackSpriteProperty.UpperLeft:
            case TrackSpriteProperty.UpperRight:
            case TrackSpriteProperty.LowerLeft:
            case TrackSpriteProperty.LowerRight:
                args.Handled = _vertexEditor.MouseMove(args);
                break;
        }

        base.OnMouseMove(args);
    }

    /// <summary>Function to handle a preview key down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a preview key down event in their own content view.</remarks>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
        if (DataContext.KeyEditor?.CurrentEditor?.Track is null)
        {
            base.OnPreviewKeyDown(args);
            return;
        }

        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.Position:
                args.IsInputKey = Clipper.KeyDown(args.KeyCode, args.Modifiers);
                break;
            case TrackSpriteProperty.AnchorAbsolute:
            case TrackSpriteProperty.Anchor:
                args.IsInputKey = _anchorEdit.KeyDown(args);
                break;
        }

        base.OnPreviewKeyDown(args);
    }

    /// <summary>Function to handle a mouse up event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
    protected override void OnMouseUp(MouseArgs args)
    {
        if (DataContext.KeyEditor?.CurrentEditor?.Track is null)
        {
            base.OnMouseUp(args);
            return;
        }

        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.Position:
                args.Handled = Clipper.MouseUp(args);
                break;
            case TrackSpriteProperty.AnchorAbsolute:
            case TrackSpriteProperty.Anchor:
                args.Handled = _anchorEdit.MouseUp(args);
                break;
            case TrackSpriteProperty.UpperLeft:
            case TrackSpriteProperty.UpperRight:
            case TrackSpriteProperty.LowerLeft:
            case TrackSpriteProperty.LowerRight:
                args.Handled = _vertexEditor.MouseUp(args);
                break;
        }

        base.OnMouseUp(args);
    }

    /// <summary>Function called when the camera is zoomed.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is zoomed in or out.</remarks>
    protected override void OnCameraZoomed()
    {
        base.OnCameraZoomed();
        RefreshEditors();
    }

    /// <summary>Function called when the camera is panned.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is panned around the view.</remarks>
    protected override void OnCameraMoved()
    {
        base.OnCameraMoved();
        RefreshEditors();
    }

    /// <summary>Function called when the view has been resized.</summary>
    /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
    protected override void OnResizeEnd()
    {
        base.OnResizeEnd();
        RefreshEditors();
    }

    /// <summary>Function to draw any gizmos for UI components.</summary>
    protected override void DrawGizmos()
    {
        if (DataContext.KeyEditor.CurrentEditor?.Track is null)
        {
            return;
        }

        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.Scale:
            case TrackSpriteProperty.ScaledSize:
            case TrackSpriteProperty.Size:
            case TrackSpriteProperty.Position:
                Renderer.Begin();

                Clipper.Render();
                DrawAnchorPoint();

                Renderer.End();
                break;
            case TrackSpriteProperty.AnchorAbsolute:
            case TrackSpriteProperty.Anchor:
                Renderer.Begin();
                Clipper.Render();
                _anchorEdit.Render();
                Renderer.End();
                break;
            case TrackSpriteProperty.UpperLeft:
            case TrackSpriteProperty.UpperRight:
            case TrackSpriteProperty.LowerLeft:
            case TrackSpriteProperty.LowerRight:
                Renderer.Begin();
                _vertexEditor.Render();
                DrawAnchorPoint();
                Renderer.End();
                break;
        }
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

        switch (SelectedTrackID)
        {
            case TrackSpriteProperty.LowerLeft:
            case TrackSpriteProperty.LowerRight:
            case TrackSpriteProperty.UpperLeft:
            case TrackSpriteProperty.UpperRight:
                Silhouette.Begin();
                Renderer.DrawSprite(_vertexEditSprite);
                Silhouette.End();
                break;
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

        _vertexEditor.Camera = _anchorEdit.Camera = Clipper.Camera = Camera;
        Clipper.Bounds = RenderRegion;
        Clipper.ClipAgainstBoundaries = false;
        Clipper.AllowResize = false;
        Clipper.AllowManualInput = false;
        Clipper.AllowMove = SelectedTrackID == TrackSpriteProperty.Position;
        _vertexEditSprite = new GorgonSprite();
        UpdateVertexEditorSprite(false);

        EnableEvents();
    }

    /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
    /// <remarks>
    /// Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.
    /// </remarks>
    protected override void OnUnload()
    {
        DisableEvents();

        base.OnUnload();
    }

    /// <summary>Function to set the default zoom/offset for the viewer.</summary>
    public override void DefaultZoom()
    {
        if (Sprite is null)
        {
            return;
        }

        ZoomToSprite(Sprite);
    }
}
