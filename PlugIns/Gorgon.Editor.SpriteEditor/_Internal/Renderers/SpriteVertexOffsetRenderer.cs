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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A renderer to use with the vertex offset tool.
    /// </summary>
    internal class SpriteVertexOffsetRenderer
        : SpriteContentRenderer
    {
        #region Variables.
        // The actual sprite to render.
        private readonly GorgonSprite _workingSprite;
        // The editor used to update the sprite vertices.
        private readonly ISpriteVertexEditService _vertexEditor;
        // Perspective correct camera.
        private readonly IGorgon2DCamera _camera;
        #endregion

        #region Methods.
        /// <summary>Handles the KeyboardIconClicked event of the VertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VertexEditor_KeyboardIconClicked(object sender, EventArgs e)
        {
            if ((SpriteContent?.ToggleManualVertexEditCommand == null) || (!SpriteContent.ToggleManualVertexEditCommand.CanExecute(_vertexEditor.SelectedVertexIndex)))
            {
                return;
            }

            SpriteContent.ToggleManualVertexEditCommand.Execute(_vertexEditor.SelectedVertexIndex);
        }

        /// <summary>Handles the VerticesChanged event of the VertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VertexEditor_VerticesChanged(object sender, EventArgs e)
        {
            SpriteContent.ManualVertexEditor.Vertices = _vertexEditor.Vertices.ToArray();

            if ((SpriteContent?.ManualVertexEditor == null)
                || (!SpriteContent.ManualVertexEditor.IsActive)
                || (SpriteContent.ManualVertexEditor.SelectedVertexIndex < 0)
                || (SpriteContent.ManualVertexEditor.SelectedVertexIndex > 3))
            {
                return;
            }

            SpriteContent.ManualVertexEditor.Offset = _vertexEditor.Vertices[SpriteContent.ManualVertexEditor.SelectedVertexIndex];
        }

        /// <summary>Handles the VertexSelected event of the VertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void VertexEditor_VertexSelected(object sender, EventArgs e)
        {
            SpriteContent.ManualVertexEditor.SelectedVertexIndex = _vertexEditor.SelectedVertexIndex;

            if ((_vertexEditor.SelectedVertexIndex >= 0) && (_vertexEditor.SelectedVertexIndex < _vertexEditor.Vertices.Count))
            {
                SpriteContent.ManualVertexEditor.Offset = _vertexEditor.Vertices[_vertexEditor.SelectedVertexIndex];
            }
        }

        /// <summary>
        /// Function to update the working sprite data from the sprite content.
        /// </summary>
        private void UpdateWorkingSprite()
        {
            if (_workingSprite == null)
            {
                return;
            }

            _workingSprite.Texture = SpriteContent?.Texture;
            _workingSprite.TextureArrayIndex = TextureArrayIndex;

            if (SpriteContent?.Texture == null)
            {
                return;
            }

            var spriteRegion = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
            DX.Vector2 scaledSpritePosition = ToClient(spriteRegion.TopLeft).Truncate();

            _workingSprite.TextureRegion = SpriteContent.TextureCoordinates;
            _workingSprite.Size = spriteRegion.Size;
            _workingSprite.Position = scaledSpritePosition;
            _workingSprite.Scale = new DX.Vector2(ZoomScaleValue);
        }

        /// <summary>Handles the PreviewKeyDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    SubmitVertexOffsets();
                    if (SpriteContent.ManualVertexEditor.IsActive)
                    {
                        VertexEditor_KeyboardIconClicked(_vertexEditor, EventArgs.Empty);
                    }
                    e.IsInputKey = true;
                    break;
                case Keys.Escape:
                    if ((SpriteContent.ManualVertexEditor.CancelCommand != null) && (SpriteContent.ManualVertexEditor.CancelCommand.CanExecute(null)))
                    {
                        SpriteContent.ManualVertexEditor.CancelCommand.Execute(null);
                    }

                    if (SpriteContent.ManualVertexEditor.IsActive)
                    {
                        VertexEditor_KeyboardIconClicked(_vertexEditor, EventArgs.Empty);
                    }
                    e.IsInputKey = true;
                    break;
                default:
                    e.IsInputKey = _vertexEditor.KeyDown(e.KeyCode, e.Modifiers);
                    break;
            }
        }

        /// <summary>Handles the MouseUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseUp(object sender, MouseEventArgs e)
        {
            _vertexEditor.MousePosition = new DX.Vector2(e.X, e.Y);
            _vertexEditor.MouseUp(e.Button);
        }

        /// <summary>Handles the MouseDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            _vertexEditor.MousePosition = new DX.Vector2(e.X, e.Y);
            _vertexEditor.MouseDown(e.Button);
        }

        /// <summary>Handles the MouseMove event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            _vertexEditor.MousePosition = new DX.Vector2(e.X, e.Y);
            _vertexEditor.MouseMove(e.Button);
        }

        /// <summary>
        /// Function to submit the updated vertex offsets to the sprite.
        /// </summary>
        private void SubmitVertexOffsets()
        {
            if ((SpriteContent?.ManualVertexEditor?.ApplyCommand == null) || (!SpriteContent.ManualVertexEditor.ApplyCommand.CanExecute(null)))
            {
                return;
            }

            SpriteContent.ManualVertexEditor.ApplyCommand.Execute(null);
        }

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ZoomScaleValue"/> property is changed.</summary>
        protected override void OnZoomScaleChanged()
        {
            UpdateWorkingSprite();
            _vertexEditor.Refresh();
        }

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ScrollOffset"/> property is changed.</summary>
        protected override void OnScrollOffsetChanged()
        {
            UpdateWorkingSprite();
            _vertexEditor.Refresh();
        }


        /// <summary>Handles the PropertyChanged event of the ManualVertexEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ManualVertexEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IManualVertexEditor.Offset):
                    int index = SpriteContent.ManualVertexEditor.SelectedVertexIndex;
                    if ((index < 0) || (index >= _vertexEditor.Vertices.Count))
                    {
                        break;
                    }

                    if (_vertexEditor.Vertices[index].Equals(SpriteContent.ManualVertexEditor.Offset))
                    {
                        break;
                    }

                    _vertexEditor.Vertices = SpriteContent.ManualVertexEditor.Vertices;

                    for (int i = 0; i < _workingSprite.CornerOffsets.Count; ++i)
                    {
                        _workingSprite.CornerOffsets[i] = i < _vertexEditor.Vertices.Count ? new DX.Vector3(_vertexEditor.Vertices[i], 0) : DX.Vector3.Zero;
                    }
                    break;
                case nameof(IManualVertexEditor.SelectedVertexIndex):
                    _vertexEditor.SelectedVertexIndex = SpriteContent.ManualVertexEditor.SelectedVertexIndex;
                    break;
                case nameof(IManualVertexEditor.Vertices):
                    _vertexEditor.Vertices = SpriteContent.ManualVertexEditor.Vertices;
                    for (int i = 0; i < _workingSprite.CornerOffsets.Count; ++i)
                    {
                        _workingSprite.CornerOffsets[i] = i < _vertexEditor.Vertices.Count ? new DX.Vector3(_vertexEditor.Vertices[i], 0) : DX.Vector3.Zero;
                    }
                    break;
            }
        }

        /// <summary>Function called when the sprite is changing a property.</summary>
        /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
        protected override void OnSpriteChanging(PropertyChangingEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.CurrentTool):
                    if (SpriteContent.CurrentTool == SpriteEditTool.CornerResize)
                    {
                        SubmitVertexOffsets();
                    }
                    break;
            }
        }

        /// <summary>Function called when the sprite has a property change.</summary>
        /// <param name="e">The event parameters.</param>
        protected override void OnSpriteChanged(PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.SamplerState):
                    _workingSprite.TextureSampler = SpriteContent.SamplerState;
                    break;
                case nameof(ISpriteContent.Texture):
                case nameof(ISpriteContent.TextureCoordinates):
                    for (int i = 0; i < _workingSprite.CornerOffsets.Count; ++i)
                    {
                        _workingSprite.CornerOffsets[i] = i < SpriteContent.VertexOffsets.Count ? SpriteContent.VertexOffsets[i] : DX.Vector3.Zero;
                    }
                    SpriteContent.ManualVertexEditor.Vertices = _workingSprite.CornerOffsets.Select(item => (DX.Vector2)item).ToArray();

                    UpdateWorkingSprite();

                    if (SpriteContent.Texture != null)
                    {
                        _vertexEditor.SpriteBounds = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF().Truncate();
                    }
                    break;
                case nameof(ISpriteContent.VertexOffsets):
                    for (int i = 0; i < _workingSprite.CornerOffsets.Count; ++i)
                    {
                        _workingSprite.CornerOffsets[i] = SpriteContent.VertexOffsets[i];
                    }
                    SpriteContent.ManualVertexEditor.Vertices = _workingSprite.CornerOffsets.Select(item => (DX.Vector2)item).ToArray();
                    break;
            }
        }

        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            var spriteRegion = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
            var imageRegion = new DX.RectangleF(0, 0, SpriteContent.Texture.Width, SpriteContent.Texture.Height);

            RenderSpriteTextureWithoutSprite(imageRegion, spriteRegion);

            SwapChain.RenderTargetView.Clear(BackgroundColor);

            Renderer.Begin();

            // Draw the pattern layer.
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height),
                new GorgonColor(GorgonColor.White, (0.5f - TextureAlpha.Min(0.5f).Max(0)) * 2),
                BackgroundPattern,
                new DX.RectangleF(0, 0, SwapChain.Width / BackgroundPattern.Width, SwapChain.Height / BackgroundPattern.Height));

            if (IsAnimating)
            {
                RenderRect(imageRegion, new GorgonColor(GorgonColor.White, TextureAlpha * 2), BackgroundPattern, new DX.RectangleF(0, 0, (imageRegion.Width * ZoomScaleValue) / BackgroundPattern.Width, (imageRegion.Height * ZoomScaleValue) / BackgroundPattern.Height));
                Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, SwapChain.Width, SwapChain.Height), new GorgonColor(GorgonColor.White, TextureAlpha), ImageBufferTexture, new DX.RectangleF(0, 0, 1, 1));
            }

            if ((SpriteContent.ManualVertexEditor != null) && (SpriteContent.ManualVertexEditor.IsActive) && (SpriteContent.ManualVertexEditor.IsMoving))
            {
                DrawDockRect();
            }

            Renderer.End();

            // TODO: We have warping due to affine texture mapping.  We need a perspective correct camera to do this properly, but currently the only 
            //       perspective camera in Gorgon's 2D renderer is not really meant for this purpose.
            Renderer.Begin(camera: _camera);
            // Draw the sprite layer.                        
            Renderer.DrawSprite(_workingSprite);
            Renderer.End();

            Renderer.Begin();
            _vertexEditor.Render();
            Renderer.End();

            return _vertexEditor.IsDragging ? 0 : 1;
        }

        /// <summary>Function called to perform custom loading of resources.</summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            _workingSprite.TextureSampler = SpriteContent.SamplerState;

            if (SpriteContent.Texture != null)
            {
                _vertexEditor.SpriteBounds = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF().Truncate();
            }

            SpriteContent.ManualVertexEditor.SelectedVertexIndex = _vertexEditor.SelectedVertexIndex = -1;

            for (int i = 0; i < _workingSprite.CornerOffsets.Count; ++i)
            {
                _workingSprite.CornerOffsets[i] = i < SpriteContent.VertexOffsets.Count ? SpriteContent.VertexOffsets[i] : DX.Vector3.Zero;
            }
            _vertexEditor.Vertices = SpriteContent.ManualVertexEditor.Vertices = _workingSprite.CornerOffsets.Select(item => (DX.Vector2)item).ToArray();

            SwapChain.Window.MouseMove += Window_MouseMove;
            SwapChain.Window.MouseDown += Window_MouseDown;
            SwapChain.Window.MouseUp += Window_MouseUp;
            SwapChain.Window.PreviewKeyDown += Window_PreviewKeyDown;

            SpriteContent.ManualVertexEditor.PropertyChanged += ManualVertexEditor_PropertyChanged;
        }

        /// <summary>Function called to perform custom unloading of resources.</summary>
        protected override void OnUnload()
        {
            SpriteContent.ManualVertexEditor.PropertyChanged -= ManualVertexEditor_PropertyChanged;
            SwapChain.Window.MouseMove -= Window_MouseMove;
            SwapChain.Window.MouseDown -= Window_MouseDown;
            SwapChain.Window.MouseUp -= Window_MouseUp;
            SwapChain.Window.PreviewKeyDown -= Window_PreviewKeyDown;

            base.OnUnload();
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            _vertexEditor.VertexSelected -= VertexEditor_VertexSelected;
            _vertexEditor.KeyboardIconClicked -= VertexEditor_KeyboardIconClicked;
            _vertexEditor.VerticesChanged -= VertexEditor_VerticesChanged;
            _vertexEditor.RectToClient = null;
            _vertexEditor.PointToClient = null;
            _vertexEditor.PointFromClient = null;

            base.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteVertexOffsetRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="vertexEditor">The editor used to modify the sprite vertices.</param>
        /// <param name="initialZoom">The initial zoom scale value.</param>
        public SpriteVertexOffsetRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, ISpriteVertexEditService vertexEditor, float initialZoom)
            : base(sprite, graphics, swapChain, renderer, initialZoom)
        {
            InitialTextureAlpha = 0;
            _vertexEditor = vertexEditor;
            _vertexEditor.RectToClient = r => ToClient(r).Truncate();
            _vertexEditor.PointToClient = p => ToClient(p).Truncate();
            _vertexEditor.PointFromClient = p => FromClient(p).Truncate();

            _workingSprite = new GorgonSprite
            {
                Texture = sprite.Texture,
                TextureRegion = sprite.TextureCoordinates,
                TextureArrayIndex = TextureArrayIndex,
                Size = sprite.Size,
                Scale = DX.Vector2.One,
                Anchor = DX.Vector2.Zero,
                Color = GorgonColor.White,
                Depth = 0.1f
            };

            UpdateWorkingSprite();

            _vertexEditor.VerticesChanged += VertexEditor_VerticesChanged;
            _vertexEditor.KeyboardIconClicked += VertexEditor_KeyboardIconClicked;
            _vertexEditor.VertexSelected += VertexEditor_VertexSelected;

            _camera = new Gorgon2DPerspectiveCamera(renderer, new DX.Size2F(swapChain.Width, swapChain.Height));
        }
        #endregion
    }
}
