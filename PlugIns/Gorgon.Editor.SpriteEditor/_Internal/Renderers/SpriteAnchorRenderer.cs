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
// Created: April 3, 2019 12:35:44 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A renderer used to render the anchor editor.
    /// </summary>
    internal class SpriteAnchorRenderer
        : SingleSpriteRenderer
    {
        #region Variables.
        // The anchor editor.
        private readonly IAnchorEditService _anchorEdit;
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the AnchorEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void AnchorEditor_PropertyChanged(object sender, PropertyChangedEventArgs e) => _anchorEdit.AnchorPosition = new DX.Vector2(SpriteContent.AnchorEditor.AnchorPosition.X + _anchorEdit.Bounds.X,
                                                                                                                                            SpriteContent.AnchorEditor.AnchorPosition.Y + _anchorEdit.Bounds.Y);


        /// <summary>Handles the AnchorChanged event of the AnchorEdit control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AnchorEdit_AnchorChanged(object sender, EventArgs e) => SpriteContent.AnchorEditor.AnchorPosition = new DX.Vector2(_anchorEdit.AnchorPosition.X - _anchorEdit.Bounds.X,
                                                                                                                                        _anchorEdit.AnchorPosition.Y - _anchorEdit.Bounds.Y);

        /// <summary>Handles the BoundsChanged event of the AnchorEdit control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AnchorEdit_BoundsChanged(object sender, EventArgs e) => SpriteContent.AnchorEditor.Bounds = _anchorEdit.Bounds;

        /// <summary>Handles the PreviewKeyDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        private void Window_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if ((SpriteContent.AnchorEditor.OkCommand != null) && (SpriteContent.AnchorEditor.OkCommand.CanExecute(null)))
                    {
                        SpriteContent.AnchorEditor.OkCommand.Execute(null);
                    }
                    e.IsInputKey = true;
                    break;
                case Keys.Escape:
                    if ((SpriteContent.AnchorEditor.CancelCommand != null) && (SpriteContent.AnchorEditor.CancelCommand.CanExecute(null)))
                    {
                        SpriteContent.AnchorEditor.CancelCommand.Execute(null);
                    }
                    e.IsInputKey = true;
                    break;
                default:
                    e.IsInputKey = _anchorEdit.KeyDown(e.KeyCode, e.Modifiers);
                    break;
            }
        }
        /// <summary>Handles the MouseUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseUp(object sender, MouseEventArgs e)
        {
            _anchorEdit.MousePosition = new DX.Vector2(e.X, e.Y);
            _anchorEdit.MouseUp(e.Button);
        }

        /// <summary>Handles the MouseDown event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            _anchorEdit.MousePosition = new DX.Vector2(e.X, e.Y);
            _anchorEdit.MouseDown(e.Button);
        }

        /// <summary>Handles the MouseMove event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            _anchorEdit.MousePosition = new DX.Vector2(e.X, e.Y);
            _anchorEdit.MouseMove(e.Button);
        }

        /// <summary>
        /// Function to update the sprite anchor.
        /// </summary>
        private void UpdateSpriteAnchor()
        {
            if (SpriteContent.Texture == null)
            {
                _anchorEdit.Bounds = DX.RectangleF.Empty;
                _anchorEdit.AnchorPosition = DX.Vector2.Zero;
                _anchorEdit.Refresh();
                return;
            }

            _anchorEdit.Bounds = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates).ToRectangleF();
            _anchorEdit.AnchorPosition = (new DX.Vector2(SpriteContent.Anchor.X * _anchorEdit.Bounds.Size.Width + _anchorEdit.Bounds.X,
                                                SpriteContent.Anchor.Y * _anchorEdit.Bounds.Size.Height + _anchorEdit.Bounds.Y));
            _anchorEdit.Refresh();
        }

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ZoomScaleValue"/> property is changed.</summary>
        protected override void OnZoomScaleChanged()
        {
            base.OnZoomScaleChanged();

            _anchorEdit.Refresh();
        }

        /// <summary>Function called when the <see cref="P:Gorgon.Editor.SpriteEditor.SpriteContentRenderer.ScrollOffset"/> property is changed.</summary>
        protected override void OnScrollOffsetChanged()
        {
            base.OnScrollOffsetChanged();

            _anchorEdit.Refresh();
        }

        /// <summary>Function called when the sprite has a property change.</summary>
        /// <param name="e">The event parameters.</param>
        protected override void OnSpriteChanged(PropertyChangedEventArgs e)
        {
            base.OnSpriteChanged(e);

            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.SamplerState):
                    SpriteSampler = GorgonSamplerState.PointFiltering;
                    break;
                case nameof(ISpriteContent.Texture):
                case nameof(ISpriteContent.Anchor):
                    UpdateSpriteAnchor();
                    break;
            }
        }

        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            base.OnRender();

            Renderer.Begin();
            Renderer.DrawRectangle(ToClient(_anchorEdit.Bounds), new GorgonColor(GorgonColor.Black, 0.30f), 4);
            Renderer.End();

            _anchorEdit.Render();

            return _anchorEdit.IsDragging ? 0 : 1;
        }

        /// <summary>Function called to perform custom loading of resources.</summary>
        protected override void OnLoad()
        {
            UpdateSpriteAnchor();

            // Set the sprite sampling to point so we can see the pixels when zoomed in.
            SpriteSampler = GorgonSamplerState.PointFiltering;
            SpriteContent.AnchorEditor.PropertyChanged += AnchorEditor_PropertyChanged;

            SwapChain.Window.MouseMove += Window_MouseMove;
            SwapChain.Window.MouseDown += Window_MouseDown;
            SwapChain.Window.MouseUp += Window_MouseUp;
            SwapChain.Window.PreviewKeyDown += Window_PreviewKeyDown;
        }

        /// <summary>Function called to perform custom unloading of resources.</summary>
        protected override void OnUnload()
        {
            SpriteContent.AnchorEditor.PropertyChanged -= AnchorEditor_PropertyChanged;

            SwapChain.Window.MouseMove -= Window_MouseMove;
            SwapChain.Window.MouseDown -= Window_MouseDown;
            SwapChain.Window.MouseUp -= Window_MouseUp;
            SwapChain.Window.PreviewKeyDown -= Window_PreviewKeyDown;

            base.OnUnload();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void Dispose()
        {
            _anchorEdit.BoundsChanged -= AnchorEdit_BoundsChanged;
            _anchorEdit.AnchorChanged -= AnchorEdit_AnchorChanged;
            base.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="anchorEditor">The editor for the anchor position.</param>
        /// <param name="initialZoom">The initial zoom scale value.</param>
        public SpriteAnchorRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, IAnchorEditService anchorEditor, float initialZoom)
            : base(sprite, graphics, swapChain, renderer, initialZoom)
        {
            _anchorEdit = anchorEditor;

            _anchorEdit.PointToClient = r => ToClient(r);
            _anchorEdit.PointFromClient = p =>
            {
                DX.Vector2 pos = FromClient(p);
                return new DX.Vector2((int)pos.X, (int)pos.Y);
            };

            _anchorEdit.AnchorChanged += AnchorEdit_AnchorChanged;
            _anchorEdit.BoundsChanged += AnchorEdit_BoundsChanged;
        }
        #endregion
    }
}
