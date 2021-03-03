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
// Created: April 8, 2019 5:03:58 PM
// 
#endregion

using System;
using System.Buffers;
using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A renderer to use with the vertex offset editing tool.
    /// </summary>
    internal class VertexEditViewer
        : SingleSpriteViewer
    {
        #region Events.
        // Event triggered when toggling the manual input UI.
        private event EventHandler ToggleManualInputEvent;

        /// <summary>
        /// Event triggered when the manual input window should be toggled on or off.
        /// </summary>
        public event EventHandler ToggleManualInput
        {
            add
            {
                if (value is null)
                {
                    ToggleManualInputEvent = null;
                    return;
                }

                ToggleManualInputEvent += value;
            }
            remove
            {
                if (value is null)
                {
                    return;
                }

                ToggleManualInputEvent -= value;
            }
        }
        #endregion

        #region Variables.
        // The editor used to update the sprite vertices.
        private readonly SpriteVertexEditService _vertexEditor;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to set the vertices in the vertex editor service.
        /// </summary>
        private void SetEditorVertices()
        {            
            // Transform the vertex offsets into sprite local space.
            DX.Vector2[] vertices = ArrayPool<DX.Vector2>.Shared.Rent(Sprite.CornerOffsets.Count);
            vertices[0] = DataContext.SpriteVertexEditContext.Vertices[0];
            vertices[1] = new DX.Vector2(Sprite.Size.Width + DataContext.SpriteVertexEditContext.Vertices[1].X, DataContext.SpriteVertexEditContext.Vertices[1].Y);
            vertices[2] = new DX.Vector2(Sprite.Size.Width + DataContext.SpriteVertexEditContext.Vertices[2].X, Sprite.Size.Height + DataContext.SpriteVertexEditContext.Vertices[2].Y);
            vertices[3] = new DX.Vector2(DataContext.SpriteVertexEditContext.Vertices[3].X, Sprite.Size.Height + DataContext.SpriteVertexEditContext.Vertices[3].Y);
            _vertexEditor.Vertices = vertices;
            ArrayPool<DX.Vector2>.Shared.Return(vertices);
        }

        /// <summary>
        /// Function to update the sprite vertices.
        /// </summary>
        private void UpdateSpriteVertices()
        {
            for (int i = 0; i < DataContext.SpriteVertexEditContext.Vertices.Count; ++i)
            {
                Sprite.CornerOffsets[i] = (DX.Vector3)DataContext.SpriteVertexEditContext.Vertices[i];
            }
        }

        /// <summary>Handles the KeyboardIconClicked event of the VertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VertexEditor_KeyboardIconClicked(object sender, EventArgs e)
        {
            EventHandler handler = ToggleManualInputEvent;
            handler?.Invoke(this, e);
        }

        /// <summary>Handles the VerticesChanged event of the VertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VertexEditor_VerticesChanged(object sender, EventArgs e)
        {
            if ((DataContext.SpriteVertexEditContext.SelectedVertexIndex < 0)
                || (DataContext.SpriteVertexEditContext.SelectedVertexIndex > 3))
            {
                return;
            }

            DX.Vector2[] verts = ArrayPool<DX.Vector2>.Shared.Rent(_vertexEditor.Vertices.Count);

            verts[0] = _vertexEditor.Vertices[0];
            verts[1] = new DX.Vector2(_vertexEditor.Vertices[1].X - Sprite.Size.Width, _vertexEditor.Vertices[1].Y);
            verts[2] = new DX.Vector2(_vertexEditor.Vertices[2].X - Sprite.Size.Width, _vertexEditor.Vertices[2].Y - Sprite.Size.Height);
            verts[3] = new DX.Vector2(_vertexEditor.Vertices[3].X, _vertexEditor.Vertices[3].Y - Sprite.Size.Height);

            DataContext.SpriteVertexEditContext.Vertices = verts;
            DataContext.SpriteVertexEditContext.Offset = DataContext.SpriteVertexEditContext.Vertices[DataContext.SpriteVertexEditContext.SelectedVertexIndex];            

            ArrayPool<DX.Vector2>.Shared.Return(verts);
        }

        /// <summary>Handles the VertexSelected event of the VertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VertexEditor_VertexSelected(object sender, EventArgs e)
        {
            DataContext.SpriteVertexEditContext.SelectedVertexIndex = _vertexEditor.SelectedVertexIndex;

            if ((_vertexEditor.SelectedVertexIndex >= 0) && (_vertexEditor.SelectedVertexIndex < _vertexEditor.Vertices.Count))
            {
                DataContext.SpriteVertexEditContext.Offset = DataContext.SpriteVertexEditContext.Vertices[DataContext.SpriteVertexEditContext.SelectedVertexIndex];
            }
        }

        /// <summary>Handles the PropertyChanged event of the SpriteVertexEditContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SpriteVertexEditContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteVertexEditContext.SelectedVertexIndex):
                    _vertexEditor.SelectedVertexIndex = DataContext.SpriteVertexEditContext.SelectedVertexIndex;
                    break;
                case nameof(ISpriteVertexEditContext.Vertices):
                    SetEditorVertices();
                    UpdateSpriteVertices();
                    break;
            }
        }

        /// <summary>Function called when the camera is zoomed.</summary>
        /// <remarks>Developers can override this method to implement a custom action when the camera is zoomed in or out.</remarks>
        protected override void OnCameraZoomed() => _vertexEditor.Refresh();

        /// <summary>Function called when the camera is panned.</summary>
        /// <remarks>Developers can override this method to implement a custom action when the camera is panned around the view.</remarks>
        protected override void OnCameraMoved() => _vertexEditor.Refresh();

        /// <summary>Function called when the view has been resized.</summary>
        /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
        protected override void OnResizeEnd() => _vertexEditor.Refresh();

        /// <summary>Function to handle a mouse move event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
        protected override void OnMouseMove(MouseArgs args)
        {
            args.Handled = _vertexEditor.MouseMove(args);
            base.OnMouseMove(args);
        }

        /// <summary>Function to handle a mouse down event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
        protected override void OnMouseDown(MouseArgs args)
        {
            args.Handled = _vertexEditor.MouseDown(args);
            base.OnMouseDown(args);
        }

        /// <summary>Function to handle a mouse up event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
        protected override void OnMouseUp(MouseArgs args)
        {
            args.Handled = _vertexEditor.MouseUp(args);
            base.OnMouseUp(args);
        }

        /// <summary>Function to handle a key down event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a key down event in their own content view.</remarks>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
        {
            args.IsInputKey = _vertexEditor.KeyDown(args);
            base.OnPreviewKeyDown(args);
        }

        /// <summary>Function to render the background.</summary>
        /// <remarks>Developers can override this method to render a custom background.</remarks>
        protected override void OnRenderBackground()
        {
            base.OnRenderBackground();

            DX.RectangleF spriteRegion = Renderer.GetAABB(Sprite);

            DX.Vector3 topLeft = Camera.Unproject((DX.Vector3)spriteRegion.TopLeft);
            DX.Vector3 bottomRight = Camera.Unproject((DX.Vector3)spriteRegion.BottomRight);

            var position = new DX.Vector2(topLeft.X + ((bottomRight.X - topLeft.X) * 0.5f), topLeft.Y + ((bottomRight.Y - topLeft.Y) * 0.5f));
            Renderer.Begin();
            Renderer.DrawLine(position.X, 0, position.X, ClientSize.Height, GorgonColor.Black);
            Renderer.DrawLine(0, position.Y, ClientSize.Width, position.Y, GorgonColor.Black);
            Renderer.End();
        }

        /// <summary>Function to draw the sprite.</summary>
        protected override void DrawSprite()
        {
            base.DrawSprite();

            var spriteTopLeft = new DX.Vector3(Sprite.Bounds.TopLeft, 0);
            var spriteBottomRight = new DX.Vector3(Sprite.Bounds.BottomRight, 0);
            Camera.Unproject(ref spriteTopLeft, out DX.Vector3 transformedTopLeft);
            Camera.Unproject(ref spriteBottomRight, out DX.Vector3 transformedBottomRight);

            _vertexEditor.SpriteBounds = new DX.RectangleF
            {
                Left = transformedTopLeft.X,
                Top = transformedTopLeft.Y,
                Right = transformedBottomRight.X,
                Bottom = transformedBottomRight.Y
            };

            Renderer.Begin();
            _vertexEditor.Render();
            Renderer.End();            
        }

        /// <summary>Function called when the renderer needs to load any resource data.</summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            // We'll need to readjust the sprite for this specific scenario.
            Sprite.Color = GorgonColor.White;
            Sprite.TextureSampler = GorgonSamplerState.PointFiltering;
            SpriteRegion = Renderer.GetAABB(Sprite);

            _vertexEditor.SelectedVertexIndex = DataContext.SpriteVertexEditContext.SelectedVertexIndex;
            _vertexEditor.Camera = Camera;
            RenderRegion = SpriteRegion;

            SetEditorVertices();
            UpdateSpriteVertices();

            _vertexEditor.VertexSelected += VertexEditor_VertexSelected;
            _vertexEditor.VerticesChanged += VertexEditor_VerticesChanged;
            _vertexEditor.KeyboardIconClicked += VertexEditor_KeyboardIconClicked;
            DataContext.SpriteVertexEditContext.PropertyChanged += SpriteVertexEditContext_PropertyChanged;
        }

        /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
        /// <remarks>
        /// Developers should always override this method if they've overridden the <see cref="OnLoad"/> method. Failure to do so can cause memory leakage.
        /// </remarks>
        protected override void OnUnload()
        {
            DataContext.SpriteVertexEditContext.PropertyChanged -= SpriteVertexEditContext_PropertyChanged;
            _vertexEditor.VerticesChanged -= VertexEditor_VerticesChanged;
            _vertexEditor.VertexSelected -= VertexEditor_VertexSelected;
            _vertexEditor.KeyboardIconClicked -= VertexEditor_KeyboardIconClicked;

            base.OnUnload();
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ToggleManualInputEvent = null;                
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="VertexEditViewer"/> class.</summary>
        /// <param name="dataContext">The sprite view model.</param>        
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="vertexEditor">The editor used to modify the sprite vertices.</param>        
        public VertexEditViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext, SpriteVertexEditService vertexEditor)
            : base(SpriteVertexEditContext.ViewerName, renderer, swapChain, dataContext) => _vertexEditor = vertexEditor;
        #endregion
    }
}
