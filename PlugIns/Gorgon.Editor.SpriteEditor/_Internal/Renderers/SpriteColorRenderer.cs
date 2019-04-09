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
// Created: April 16, 2019 11:31:06 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor
{
	/// <summary>
    /// The renderer used when updating a sprites colors.
    /// </summary>
    internal class SpriteColorRenderer
		: SingleSpriteRenderer
    {
        #region Variables.
		// The handles for corner selection.
        private readonly DX.RectangleF[] _handles = new DX.RectangleF[5];
		// The handles that are selected.
        private readonly bool[] _selected = new bool[4];
		// The currently active handle.
        private int _activeHandleIndex = -1;
		// Working color set.
        private readonly GorgonColor[] _colors = new GorgonColor[4];
        #endregion

        #region Methods.
		/// <summary>
        /// Function to set the selected colors.
        /// </summary>
        private void SetSelectedColors()
        {
            for (int i = 0; i < _selected.Length; ++i)
            {
                if (_selected[i])
                {
                    _colors[i] = SpriteContent.ColorEditor.SpriteColor[i];
                }
            }

            SpriteColor = _colors;
        }

        /// <summary>
        /// Function to retrieve the currently active handle.
        /// </summary>
        /// <param name="clientMousePos">Client mouse position.</param>
        private void GetActiveHandle(DX.Vector2 clientMousePos)
        {
            _activeHandleIndex = -1;

            Cursor mouseCursor = Cursor.Current;

            for (int i = 0; i < _handles.Length; ++i)
            {
                DX.RectangleF handle = _handles[i];

                if (handle.IsEmpty)
                {
                    continue;
                }

                if (handle.Contains(clientMousePos))
                {
                    _activeHandleIndex = i;
                    mouseCursor = Cursors.Cross;
                    break;
                }
            }

            if (_activeHandleIndex == -1)
            {
                mouseCursor = Cursors.Default;
            }

            if (mouseCursor != Cursor.Current)
            {
                Cursor.Current = _activeHandleIndex == -1 ? Cursors.Default : mouseCursor;
            }
        }


        /// <summary>Handles the PropertyChanged event of the ColorEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ColorEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteColorEdit.SpriteColor):
                    SetSelectedColors();
                    break;
            }
        }

        /// <summary>Handles the DoubleClick event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_DoubleClick(object sender, EventArgs e)
        {
        }

        /// <summary>Handles the MouseUp event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseUp(object sender, MouseEventArgs e)
        {
            GetActiveHandle(new DX.Vector2(e.X, e.Y));

            for (int i = 0; i < _selected.Length; ++i)
            {
                if (((Control.ModifierKeys & Keys.Shift) != Keys.Shift) || (_activeHandleIndex == 4))
                {
                    _selected[i] = _activeHandleIndex == 4;
                }
            }

            if ((_activeHandleIndex == -1) || (_activeHandleIndex == 4))
            {
                SpriteContent.ColorEditor.SelectedVertices = _selected;
            }
            else
            {
                if (((Control.ModifierKeys & Keys.Shift) != Keys.Shift) || (e.Button == MouseButtons.Right))
                {
                    _selected[_activeHandleIndex] = true;
                }
                else
                {
                    _selected[_activeHandleIndex] = !_selected[_activeHandleIndex];
                }

                SpriteContent.ColorEditor.SelectedVertices = _selected;
            }

            if (e.Button != MouseButtons.Right)
            {
                return;
            }            

            for (int i = 0; i < _selected.Length; ++i)
            {
                if (_selected[i])
                {
                    _colors[i] = SpriteContent.ColorEditor.SelectedColor;
                }
            }

            SpriteContent.ColorEditor.SpriteColor = _colors;
        }

        /// <summary>Handles the MouseMove event of the Window control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void Window_MouseMove(object sender, MouseEventArgs e) => GetActiveHandle(new DX.Vector2(e.X, e.Y));

        /// <summary>
        /// Function to set up the handles for selection.
        /// </summary>
        private void SetHandles()
        {
            if (SpriteContent.Texture == null)
            {
                return;
            }

            DX.Rectangle spriteRect = SpriteContent.Texture.ToPixel(SpriteContent.TextureCoordinates);
            DX.RectangleF screenRect = ToClient(spriteRect.ToRectangleF());			

            _handles[0] = new DX.RectangleF(screenRect.Left - 8, screenRect.Top - 8, 8, 8);
            _handles[1] = new DX.RectangleF(screenRect.Right, screenRect.Top - 8, 8, 8);
            _handles[2] = new DX.RectangleF(screenRect.Right, screenRect.Bottom, 8, 8);
            _handles[3] = new DX.RectangleF(screenRect.Left - 8, screenRect.Bottom, 8, 8);

            if ((screenRect.Width >= 48) && (screenRect.Height >= 48))
            {
                _handles[4] = new DX.RectangleF((screenRect.Left - 16) + screenRect.Width * 0.5f, (screenRect.Top - 16) + screenRect.Height * 0.5f, 32, 32);
            }
            else
            {
                _handles[4] = DX.RectangleF.Empty;
            }
        }

        /// <summary>Function called when after the swap chain is resized.</summary>
        protected override void OnAfterSwapChainResized()
        {
            base.OnAfterSwapChainResized();

            SetHandles();
        }

        /// <summary>Function called when the <see cref="SpriteContentRenderer.ScrollOffset"/> property is changed.</summary>
        protected override void OnScrollOffsetChanged()
        {
            base.OnScrollOffsetChanged();
            SetHandles();
        }

        /// <summary>Function called when the <see cref="SpriteContentRenderer.ZoomScaleValue"/> property is changed.</summary>
        protected override void OnZoomScaleChanged()
        {
            base.OnZoomScaleChanged();
            SetHandles();
        }

        /// <summary>Function called when the sprite is changing a property.</summary>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected override void OnSpriteChanged(PropertyChangedEventArgs e)
        {
            base.OnSpriteChanged(e);

            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.SamplerState):
                    SpriteSampler = SpriteContent.SamplerState;
                    break;
                case nameof(ISpriteContent.VertexColors):
                    for (int i = 0; i < _colors.Length; ++i)
                    {
                        _colors[i] = SpriteContent.VertexColors[i];
                    }
                    SpriteContent.ColorEditor.SpriteColor = _colors;
                    SpriteContent.ColorEditor.OriginalSpriteColor = _colors;
                    break;
                case nameof(ISpriteContent.TextureCoordinates):
                case nameof(ISpriteContent.Texture):
                    SetHandles();
                    break;
            }
        }

        /// <summary>Function called to render the sprite data.</summary>
        /// <returns>The presentation interval to use when rendering.</returns>
        protected override int OnRender()
        {
            base.OnRender();

            Renderer.Begin();

            for (int i = 0; i < _handles.Length; ++i)
            {
                DX.RectangleF handleBounds = _handles[i];

                if (handleBounds.IsEmpty)
                {
                    continue;
                }

                // Hilight our active handle.
                if (_activeHandleIndex == i)
                {
                    Renderer.DrawFilledRectangle(handleBounds, new GorgonColor(GorgonColor.RedPure, 0.7f));
                }

                Renderer.DrawRectangle(handleBounds, GorgonColor.Black);
                var inner = new DX.RectangleF(handleBounds.Left + 1, handleBounds.Top + 1, handleBounds.Width - 2, handleBounds.Height - 2);
                Renderer.DrawRectangle(inner, GorgonColor.White);

                if ((i < 4) && (_selected[i]))
                {
                    inner = new DX.RectangleF(handleBounds.Left - 4, handleBounds.Top - 4, handleBounds.Width + 8, handleBounds.Height + 8);
                    Renderer.DrawEllipse(inner, GorgonColor.Black);
                    inner.Inflate(-1, -1);
                    Renderer.DrawEllipse(inner, GorgonColor.White);
                }
            }

            Renderer.End();

            return 1;
        }

        /// <summary>Function called to perform custom loading of resources.</summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            SpriteSampler = SpriteContent.SamplerState;
            SwapChain.Window.MouseMove += Window_MouseMove;
            SwapChain.Window.MouseUp += Window_MouseUp;
            SwapChain.Window.DoubleClick += Window_DoubleClick;
            SpriteContent.ColorEditor.PropertyChanged += ColorEditor_PropertyChanged;

            for (int i = 0; i < _selected.Length; ++i)
            {
                _selected[i] = SpriteContent.ColorEditor.SelectedVertices[i];
            }

            for (int i = 0; i < _colors.Length; ++i)
            {
                _colors[i] = SpriteContent.VertexColors[i];
            }
						
            SpriteContent.ColorEditor.OriginalSpriteColor = _colors;
            SpriteContent.ColorEditor.SpriteColor = _colors;
            SpriteContent.ColorEditor.SelectedVertices = _selected;

            SetHandles();
        }

        /// <summary>Function called to perform custom unloading of resources.</summary>
        protected override void OnUnload()
        {
            SpriteContent.ColorEditor.PropertyChanged -= ColorEditor_PropertyChanged;
            SwapChain.Window.DoubleClick -= Window_DoubleClick;
            SwapChain.Window.MouseMove -= Window_MouseMove;
            SwapChain.Window.MouseUp -= Window_MouseUp;

            base.OnUnload(); 
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.SpriteContentRenderer"/> class.</summary>
        /// <param name="sprite">The sprite view model.</param>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="swapChain">The swap chain for the render area.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="initialZoom">The initial zoom scale value.</param>
        public SpriteColorRenderer(ISpriteContent sprite, GorgonGraphics graphics, GorgonSwapChain swapChain, Gorgon2D renderer, float initialZoom)
            : base(sprite, graphics, swapChain, renderer, initialZoom)
        {
        }
		#endregion
    }
}
