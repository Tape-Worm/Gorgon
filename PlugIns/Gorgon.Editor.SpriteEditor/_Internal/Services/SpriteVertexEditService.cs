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
// Created: April 8, 2019 9:27:07 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Drawing = System.Drawing;
using DX = SharpDX;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Math;
using System.Threading;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The service used to edit sprite vertices.
    /// </summary>
    internal class SpriteVertexEditService
        : ISpriteVertexEditService
    {
        #region Variables.
        // The renderer used to draw the UI.
        private readonly Gorgon2D _renderer;
        // The list of vertices to update.
        private readonly DX.Vector2[] _vertices = new DX.Vector2[4];
        // The handles for grabbing.
        private readonly RectHandle[] _handles =
        {
            new RectHandle(),
            new RectHandle(),
            new RectHandle(),
            new RectHandle(),
            new RectHandle()
        };
        // The sprite boundaries in screen space.
        private DX.RectangleF _screenSpriteBounds;
        // The sprite boundaries, in texture space (but in pixels).
        private DX.RectangleF _spriteBounds;
        // The state used to draw inverted items.
        private readonly Gorgon2DBatchState _invertedState;
        // The vertex positions, in screen space.
        private readonly DX.Vector2[] _screenVertices = new DX.Vector2[4];
        // The currently active handle (the mouse is over it).
        private int _activeHandleIndex = -1;
        // Starting drag position.
        private DX.Vector2 _startDrag;
        // Mouse down position (in client space).
        private DX.Vector2 _mouseDownPos;
        // The original position of the handle being dragged.
        private DX.Vector2 _dragHandlePos;
        // The icon used for keyboard manual input.
        private Lazy<GorgonTexture2DView> _keyboardIcon;
        // The currently selected vertex.
        private int _selectedVertexIndex = -1;
        #endregion

        #region Events.
        /// <summary>Event triggered when the keyboard icon is clicked.</summary>
        public event EventHandler KeyboardIconClicked;

        /// <summary>
        /// Event triggered when the vertex coordinates have been altered.
        /// </summary>
        public event EventHandler VerticesChanged;

        /// <summary>
        /// Event triggered when a vertex is selected or deselected.
        /// </summary>
        public event EventHandler VertexSelected;
        #endregion

        #region Properties.		
        /// <summary>
        /// Property to return whether we're in the middle of a drag operation or not.
        /// </summary>
        public bool IsDragging
        {
            get;
            private set;
        }

        /// <summary>Property to set or return the vertices for the sprite.</summary>
        public IReadOnlyList<DX.Vector2> Vertices
        {
            get => _vertices;
            set
            {
                if (value == null)
                {
                    Array.Clear(_vertices, 0, _vertices.Length);
                    OnVerticesChanged();
                    return;
                }

                if (_vertices.SequenceEqual(value))
                {
                    return;
                }

                for (int i = 0; i < _vertices.Length; ++i)
                {
                    _vertices[i] = i < value.Count ? value[i] : DX.Vector2.Zero;
                }

                SetupHandles();

                OnVerticesChanged();
            }
        }

        /// <summary>Property to set or return the rectangular boundaries for the sprite.</summary>
        public DX.RectangleF SpriteBounds
        {
            get => _spriteBounds;
            set
            {
                value = value.Truncate();

                if (_spriteBounds.Equals(ref value))
                {
                    return;
                }

                _spriteBounds = value;
                SetupHandles();
            }
        }

        /// <summary>
        /// Property to set or return the mouse position in the client area of the primary rendering window.
        /// </summary>
        public DX.Vector2 MousePosition
        {
            get;
            set;
        }

        /// <summary>Property to set or return the function used to transform a rectangle from local clip space to window client space.</summary>
        public Func<DX.RectangleF, DX.RectangleF> RectToClient
        {
            get;
            set;
        }

        /// <summary>Property to set or return the function used to transform a point from local clip space to window client space.</summary>
        public Func<DX.Vector2, DX.Vector2> PointToClient
        {
            get;
            set;
        }

        /// <summary>Property to set or return the function used to transform a point from window client space into local clip space.</summary>
        public Func<DX.Vector2, DX.Vector2> PointFromClient
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the selected vertex index.
        /// </summary>
        public int SelectedVertexIndex
        {
            get => _selectedVertexIndex;
            set
            {
                if (_selectedVertexIndex == value)
                {
                    return;
                }

                _selectedVertexIndex = value;

                EventHandler handler = VertexSelected;
                handler?.Invoke(this, EventArgs.Empty);

                SetupHandles();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to raise the <see cref="VerticesChanged"/> event.
        /// </summary>
        private void OnVerticesChanged()
        {
            EventHandler handler = VerticesChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to perform the dragging on the handles or the body of the selection.
        /// </summary>
        /// <param name="mouse">The current mouse coordinates, transformed into local image space.</param>
        private void DragHandles(DX.Vector2 mouse)
        {
            if ((SelectedVertexIndex == -1) || (_activeHandleIndex > 3))
            {
                return;
            }

            DX.Vector2.Subtract(ref mouse, ref _startDrag, out DX.Vector2 dragDelta);
            _vertices[SelectedVertexIndex] = new DX.Vector2(_dragHandlePos.X + dragDelta.X, _dragHandlePos.Y + dragDelta.Y);
            SetupHandles();

            OnVerticesChanged();
        }

        /// <summary>
        /// Function to find the active handle.
        /// </summary>
        private void GetActiveHandle()
        {
            if (IsDragging)
            {
                return;
            }

            Cursor mouseCursor = Cursor.Current;

            _activeHandleIndex = -1;

            for (int i = 0; i < _handles.Length; ++i)
            {
                DX.RectangleF handleBounds = _handles[i].HandleBounds;

                if (handleBounds.IsEmpty)
                {
                    continue;
                }

                if ((handleBounds.Contains(MousePosition)) && (!IsDragging))
                {
                    _activeHandleIndex = i;
                    mouseCursor = _handles[_activeHandleIndex].HandleCursor;
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

        /// <summary>
        /// Function to set up the handles for 
        /// </summary>
        private void SetupHandles()
        {
            if (_spriteBounds.IsEmpty)
            {
                for (int i = 0; i < _handles.Length; ++i)
                {
                    _handles[i].HandleBounds = DX.RectangleF.Empty;
                }
                Cursor.Current = Cursors.Default;
                return;
            }

            _screenVertices[0] = new DX.Vector2(_spriteBounds.Left + _vertices[0].X, _spriteBounds.Top + _vertices[0].Y);
            _screenVertices[1] = new DX.Vector2(_spriteBounds.Right + _vertices[1].X, _spriteBounds.Top + _vertices[1].Y);
            _screenVertices[2] = new DX.Vector2(_spriteBounds.Right + _vertices[2].X, _spriteBounds.Bottom + _vertices[2].Y);
            _screenVertices[3] = new DX.Vector2(_spriteBounds.Left + _vertices[3].X, _spriteBounds.Bottom + _vertices[3].Y);

            if (PointToClient != null)
            {
                _screenVertices[0] = PointToClient(_screenVertices[0]);
                _screenVertices[1] = PointToClient(_screenVertices[1]);
                _screenVertices[2] = PointToClient(_screenVertices[2]);
                _screenVertices[3] = PointToClient(_screenVertices[3]);
            }

            var aabb = new DX.RectangleF
            {
                Left = float.MaxValue,
                Top = float.MaxValue,
                Right = float.MinValue,
                Bottom = float.MinValue
            };

            for (int i = 0; i < _screenVertices.Length; ++i)
            {
                aabb = new DX.RectangleF
                {
                    Left = aabb.Left.Min(_screenVertices[i].X),
                    Top = aabb.Top.Min(_screenVertices[i].Y),
                    Right = aabb.Right.Max(_screenVertices[i].X),
                    Bottom = aabb.Bottom.Max(_screenVertices[i].Y)
                };
            }

            _handles[0].HandleBounds = new DX.RectangleF(_screenVertices[0].X - 8, _screenVertices[0].Y - 8, 8, 8);
            _handles[1].HandleBounds = new DX.RectangleF(_screenVertices[1].X, _screenVertices[1].Y - 8, 8, 8);
            _handles[2].HandleBounds = new DX.RectangleF(_screenVertices[2].X, _screenVertices[2].Y, 8, 8);
            _handles[3].HandleBounds = new DX.RectangleF(_screenVertices[3].X - 8, _screenVertices[3].Y, 8, 8);
            if ((aabb.Width >= _keyboardIcon.Value.Width * 2) && (aabb.Height >= _keyboardIcon.Value.Height * 2) && (SelectedVertexIndex != -1))
            {
                DX.Vector2 keyPos = new DX.Vector2(_screenVertices[SelectedVertexIndex].X + 8, _screenVertices[SelectedVertexIndex].Y + 8).Truncate();

                _handles[4].HandleBounds = new DX.RectangleF(keyPos.X, keyPos.Y, _keyboardIcon.Value.Width, _keyboardIcon.Value.Height);
                _handles[4].Texture = _keyboardIcon.Value;
            }
            else
            {
                _handles[4].HandleBounds = DX.RectangleF.Empty;
            }

            _screenSpriteBounds = RectToClient == null ? _spriteBounds.Truncate() : RectToClient(_spriteBounds).Truncate();

            GetActiveHandle();
        }


        /// <summary>
        /// Function called when a key is held down.
        /// </summary>
        /// <param name="key">The key that was held down.</param>
        /// <param name="modifiers">The modifier keys held down with the <paramref name="key"/>.</param>
        /// <returns><b>true</b> if the key was handled, <b>false</b> if it was not.</returns>
        public bool KeyDown(Keys key, Keys modifiers)
        {
            GetActiveHandle();

            if ((_activeHandleIndex == -1) || (_activeHandleIndex > 3))
            {
                return false;
            }

            int offset = 1;

            if ((modifiers & Keys.Shift) == Keys.Shift)
            {
                offset = 10;
            }

            if ((modifiers & Keys.Control) == Keys.Control)
            {
                offset = 100;
            }

            _dragHandlePos = _vertices[_activeHandleIndex];
            _startDrag = PointFromClient != null ? PointFromClient(MousePosition) : MousePosition;
            int scale = (int)(_screenSpriteBounds.Width / _spriteBounds.Width).Max(1);
            offset *= scale;

            switch (key)
            {
                case Keys.Up:
                case Keys.NumPad8:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X, Cursor.Position.Y - offset);
                    MousePosition = new DX.Vector2(MousePosition.X, MousePosition.Y - offset);
                    break;
                case Keys.Down:
                case Keys.NumPad2:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X, Cursor.Position.Y + offset);
                    MousePosition = new DX.Vector2(MousePosition.X, MousePosition.Y + offset);
                    break;
                case Keys.Right:
                case Keys.NumPad6:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X + offset, Cursor.Position.Y);
                    MousePosition = new DX.Vector2(MousePosition.X + offset, MousePosition.Y);
                    break;
                case Keys.Left:
                case Keys.NumPad4:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X - offset, Cursor.Position.Y);
                    MousePosition = new DX.Vector2(MousePosition.X - offset, MousePosition.Y);
                    break;
                case Keys.NumPad7:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X - offset, Cursor.Position.Y - offset);
                    MousePosition = new DX.Vector2(MousePosition.X - offset, MousePosition.Y - offset);
                    break;
                case Keys.NumPad9:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X + offset, Cursor.Position.Y - offset);
                    MousePosition = new DX.Vector2(MousePosition.X + offset, MousePosition.Y - offset);
                    break;
                case Keys.NumPad1:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X - offset, Cursor.Position.Y + offset);
                    MousePosition = new DX.Vector2(MousePosition.X - offset, MousePosition.Y + offset);
                    break;
                case Keys.NumPad3:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X + offset, Cursor.Position.Y + offset);
                    MousePosition = new DX.Vector2(MousePosition.X + offset, MousePosition.Y + offset);
                    break;
                default:
                    return false;
            }

            DragHandles(PointFromClient != null ? PointFromClient(MousePosition) : MousePosition);

            return true;
        }

        /// <summary>
        /// Function called when the mouse button is moved.
        /// </summary>
        /// <param name="button">The button that was held down while moving.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseMove(MouseButtons button)
        {
            GetActiveHandle();

            if (SelectedVertexIndex == -1)
            {
                return false;
            }

            if (button != MouseButtons.Left)
            {
                IsDragging = false;
            }
            else if (!IsDragging)
            {
                var delta = new DX.Vector2((_mouseDownPos.X - MousePosition.X), (_mouseDownPos.Y - MousePosition.Y));
                if ((delta.X.Abs() > 4) || (delta.Y.Abs() > 4))
                {
                    DX.Vector2 mouseDown = PointFromClient != null ? PointFromClient(_mouseDownPos) : _mouseDownPos;
                    _startDrag = PointFromClient != null ? PointFromClient(MousePosition) : MousePosition;
                    delta = new DX.Vector2(_startDrag.X - mouseDown.X, _startDrag.Y - mouseDown.Y);
                    ref DX.Vector2 vertexPosition = ref _vertices[SelectedVertexIndex];
                    vertexPosition = new DX.Vector2(vertexPosition.X + delta.X, vertexPosition.Y + delta.Y);
                    _dragHandlePos = vertexPosition;

                    SetupHandles();
                    GetActiveHandle();

                    IsDragging = true;

                    return true;
                }
            }

            if (IsDragging)
            {
                DragHandles(PointFromClient != null ? PointFromClient(MousePosition) : MousePosition);
            }

            return true;
        }

        /// <summary>
        /// Function called when the mouse button is pressed.
        /// </summary>
        /// <param name="button">The button that was pressed.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseDown(MouseButtons button)
        {
            _mouseDownPos = MousePosition;

            GetActiveHandle();

            if (_activeHandleIndex == 4)
            {
                return true;
            }

            if ((_activeHandleIndex == -1) || (_activeHandleIndex > 3))
            {
                SelectedVertexIndex = -1;
                return false;
            }

            // Only drag if we've moved a little bit while having the mouse button held down.
            SelectedVertexIndex = _activeHandleIndex;
            return true;
        }

        /// <summary>
        /// Function called when the mouse button is released.
        /// </summary>
        /// <param name="button">The mouse button that was released.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseUp(MouseButtons button)
        {
            GetActiveHandle();

            _mouseDownPos = DX.Vector2.Zero;

            if ((_activeHandleIndex == 4) && (!IsDragging))
            {
                EventHandler handler = KeyboardIconClicked;
                handler?.Invoke(this, EventArgs.Empty);
                return true;
            }

            if (SelectedVertexIndex == -1)
            {
                return false;
            }

            if (button == MouseButtons.Left)
            {
                _startDrag = DX.Vector2.Zero;
                IsDragging = false;
            }

            return true;
        }

        /// <summary>Function to render the editor UI.</summary>
        public void Render()
        {
            if (_spriteBounds.IsEmpty)
            {
                return;
            }

            // End any drawing that we were doing prior to this.
            _renderer.End();

            GetActiveHandle();

            _renderer.Begin(_invertedState);

            _renderer.DrawLine(_screenVertices[0].X, _screenVertices[0].Y, _screenVertices[1].X - 1, _screenVertices[1].Y, GorgonColor.White);
            _renderer.DrawLine(_screenVertices[1].X - 1, _screenVertices[1].Y, _screenVertices[2].X - 1, _screenVertices[2].Y - 1, GorgonColor.White);
            _renderer.DrawLine(_screenVertices[2].X, _screenVertices[2].Y - 1, _screenVertices[3].X + 1, _screenVertices[3].Y - 1, GorgonColor.White);
            _renderer.DrawLine(_screenVertices[3].X, _screenVertices[3].Y, _screenVertices[0].X, _screenVertices[0].Y + 1, GorgonColor.White);

            _renderer.End();

            // Restart drawing using the original state.
            _renderer.Begin();

            for (int i = 0; i < _handles.Length; ++i)
            {
                GorgonTexture2DView texture = _handles[i].Texture;
                DX.RectangleF handleBounds = _handles[i].HandleBounds;

                if (handleBounds.IsEmpty)
                {
                    continue;
                }

                // 4 is the keyboard icon index, and if we're dragging we shouldn't draw it.
                if ((i == 4) && (IsDragging))
                {
                    continue;
                }

                // Hilight our active handle.
                if (_activeHandleIndex == i)
                {
                    _renderer.DrawFilledRectangle(handleBounds, new GorgonColor(GorgonColor.RedPure, 0.7f));
                }

                if (texture == null)
                {
                    _renderer.DrawRectangle(handleBounds, GorgonColor.Black);
                    _renderer.DrawRectangle(new DX.RectangleF(handleBounds.X + 1, handleBounds.Y + 1, handleBounds.Width - 2, handleBounds.Height - 2), GorgonColor.White);
                }
                else
                {
                    _renderer.DrawFilledRectangle(handleBounds, GorgonColor.White, texture, new DX.RectangleF(0, 0, 1, 1));
                }


                if (SelectedVertexIndex == i)
                {
                    handleBounds.Inflate(4, 4);
                    _renderer.DrawEllipse(handleBounds, GorgonColor.Black, thickness: 2);
                    handleBounds.Inflate(-1, -1);
                    _renderer.DrawEllipse(handleBounds, GorgonColor.White);
                }
            }
        }

        /// <summary>
        /// Function to refresh the UI of the editor.
        /// </summary>
        public void Refresh() => SetupHandles();

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Lazy<GorgonTexture2DView> keyIcon = Interlocked.Exchange(ref _keyboardIcon, null);

            if (keyIcon?.IsValueCreated ?? false)
            {
                keyIcon.Value.Dispose();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpriteVertexEditService"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        public SpriteVertexEditService(Gorgon2D renderer)
        {
            _renderer = renderer;

            var builder = new Gorgon2DBatchStateBuilder();
            _invertedState = builder.BlendState(GorgonBlendState.Inverted)
                                    .Build();

            _handles[2].HandleCursor = _handles[0].HandleCursor =
            _handles[3].HandleCursor = _handles[1].HandleCursor = Cursors.Cross;
            _handles[4].HandleCursor = Cursors.Hand;

            _keyboardIcon = new Lazy<GorgonTexture2DView>(() => GorgonTexture2DView.CreateTexture(renderer.Graphics, new GorgonTexture2DInfo("VertexEditor_KeyboardIcon"), EditorCommonResources.KeyboardIcon), true);
        }
        #endregion

    }
}
