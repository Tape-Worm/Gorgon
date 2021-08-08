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

using System;
using System.Buffers;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Geometry;
using DX = SharpDX;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// The viewer for editing a vector 2 key value for an aniamtion.
    /// </summary>
    internal class Vector2AnimationViewer
        : AnimationViewer
    {
        #region Constants.
        /// <summary>
        /// The name of the viewer.
        /// </summary>
        public const string ViewerName = nameof(AnimationTrackKeyType.Vector2);
        #endregion

        #region Variables.
        // The clipper service used to manipulate a sprite.
        private readonly IRectClipperService _clipper;
        // The anchor editor service.
        private readonly IAnchorEditService _anchorEdit;
        // The editor used to modify sprite vertices.
        private readonly VertexEditService _vertexEditor;
        // Flag to indicate whether the clipper/anchor events are assigned.
        private int _clipAnchorEvent;
        // Previous angle when modifying vertices for a sprite.
        private GorgonSprite _vertexEditSprite;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to enable the events for the clipper/anchor editor.
        /// </summary>
        private void EnableEvents()
        {
            if (Interlocked.Exchange(ref _clipAnchorEvent, 1) == 1)
            {
                return;
            }

            _clipper.RectChanged += Clipper_RectChanged;
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

            _clipper.RectChanged -= Clipper_RectChanged;
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
            var offset = new Vector2((_anchorEdit.AnchorPosition.X + Sprite.ScaledSize.Width * 0.5f) / Sprite.ScaledSize.Width,
                                        (_anchorEdit.AnchorPosition.Y + Sprite.ScaledSize.Height * 0.5f) / Sprite.ScaledSize.Height);

            switch (SelectedTrackID)
            {
                case TrackSpriteProperty.AnchorAbsolute:
                    DataContext.KeyEditor.CurrentEditor.Value = new Vector4(offset.X * Sprite.Size.Width * 0.5f,
                                                                               offset.Y * Sprite.Size.Height * 0.5f, 0, 0);
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
                    Vector2 offset = new Vector2(Sprite.Anchor.X * _clipper.Rectangle.Width, Sprite.Anchor.Y * _clipper.Rectangle.Height).Truncate();
                    DataContext.KeyEditor.CurrentEditor.Value = new Vector4(_clipper.Rectangle.X + offset.X, _clipper.Rectangle.Y + offset.Y, 0, 0);
                    break;
                case TrackSpriteProperty.Size:
                    DataContext.KeyEditor.CurrentEditor.Value = new Vector4(_clipper.Rectangle.Width, _clipper.Rectangle.Height, 0, 0);
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
                    _clipper.Rectangle = Renderer.MeasureSprite(Sprite).Truncate();
                    return;
                }

                // The camera relies on the target size of our swap chain for its projection, so we have to switch to our main 
                // render target temporarily.
                Graphics.SetRenderTarget(MainRenderTarget);
                _clipper.Rectangle = Renderer.MeasureSprite(Sprite).Truncate();
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

                DX.RectangleF bounds = Renderer.MeasureSprite(_vertexEditSprite);
                ref readonly Gorgon2DVertex[] spriteVertices = ref Renderer.GetVertices(_vertexEditSprite);
                vertices = ArrayPool<Vector2>.Shared.Rent(4);

                if (oldTarget != MainRenderTarget)
                {
                    Graphics.SetRenderTarget(MainRenderTarget);
                }

                var halfRegion = new Vector2(RenderRegion.Width * 0.5f, RenderRegion.Height * 0.5f);
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
                    _clipper.Refresh();
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
                _vertexEditSprite.Color = new GorgonColor(GorgonColor.Black, 0.85f);
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
                    var spritePos = new Vector2(Sprite.Position.X - RenderRegion.Width * 0.5f, Sprite.Position.Y - RenderRegion.Height * 0.5f);
                    Vector2 anchorPos = Vector2.Zero;

                    switch (SelectedTrackID)
                    {
                        case TrackSpriteProperty.Anchor:
                            anchorPos = new Vector2(DataContext.KeyEditor.CurrentEditor.Value.X * Sprite.Size.Width - Sprite.Size.Width * 0.5f,
                                                       DataContext.KeyEditor.CurrentEditor.Value.Y * Sprite.Size.Height - Sprite.Size.Height * 0.5f);
                            break;
                        case TrackSpriteProperty.AnchorAbsolute:
                            anchorPos = new Vector2(DataContext.KeyEditor.CurrentEditor.Value.X - Sprite.Size.Width * 0.5f,
                                                       DataContext.KeyEditor.CurrentEditor.Value.Y - Sprite.Size.Height * 0.5f);
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

                    var center = Vector2.Subtract(anchorPos, spritePos);

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
                    _clipper.AllowMove = SelectedTrackID == TrackSpriteProperty.Position;
                    UpdateVertexEditorSprite(false);
                    break;
                case nameof(IAnimationContent.BackgroundImage):
                    _clipper.Bounds = RenderRegion;
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
                    args.Handled = _clipper.MouseDown(args);
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
                    args.Handled = _clipper.MouseMove(args);
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
                    args.IsInputKey = _clipper.KeyDown(args.KeyCode, args.Modifiers);
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
                    args.Handled = _clipper.MouseUp(args);
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
                    
                    _clipper.Render();
                    DrawAnchorPoint();

                    Renderer.End();
                    break;
                case TrackSpriteProperty.AnchorAbsolute:
                case TrackSpriteProperty.Anchor:
                    Renderer.Begin();
                    _clipper.Render();
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

            _vertexEditor.Camera = _anchorEdit.Camera = _clipper.Camera = Camera;
            _clipper.Bounds = RenderRegion;
            _clipper.ClipAgainstBoundaries = false;
            _clipper.AllowResize = false;
            _clipper.AllowManualInput = false;
            _clipper.AllowMove = SelectedTrackID == TrackSpriteProperty.Position;
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

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="DefaultAnimationViewer"/> class.</summary>
        /// <param name="renderer">The main renderer for the content view.</param>
        /// <param name="swapChain">The swap chain for the content view.</param>
        /// <param name="dataContext">The view model to assign to the renderer.</param>        
        /// <param name="clipper">The rectangle clipper interface.</param>
        /// <param name="anchorEditor">The anchor editor interface.</param>
        /// <param name="vertexEditor">The editor for sprite vertices.</param>
        public Vector2AnimationViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IAnimationContent dataContext, IRectClipperService clipper, IAnchorEditService anchorEditor, VertexEditService vertexEditor)
            : base(ViewerName, renderer, swapChain, dataContext, true)
        {            
            _clipper = clipper;
            _anchorEdit = anchorEditor;
            _vertexEditor = vertexEditor;
        }
        #endregion
    }
}
