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
// Created: March 19, 2019 2:09:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Drawing = System.Drawing;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// A service used to clip a rectangular area from an image.
    /// </summary>
    public class RectClipperService
        : IRectClipperService
    {
        #region Variables.
        // The clipped area.
        private DX.RectangleF _clipRect;
        // The marching ants rectangle.
        private IMarchingAnts _marchingAnts;
        // The 2D renderer.
        private Gorgon2D _renderer;
        // The handles for grabbing.
        private RectHandle[] _handles = new RectHandle[10];
        // The currently active handle (the mouse is over it).
        private int _activeHandleIndex = -1;
        // Starting drag position.
        private DX.Vector2 _startDrag;
        // The rectangle dimensions when dragging has begun.
        private DX.RectangleF _dragRect;
        // The region representing our rectangle to draw on the screen.
        private DX.RectangleF _screenRect;
        // The icon used for keyboard manual input.
        private Lazy<GorgonTexture2DView> _keyboardIcon;
        // Flag to indicate that the selection can be moved.
        private bool _allowMove = true;
        // Flag to indicate that the selection can be resized.
        private bool _allowResize = true;
        // Flag to indicate that manual input is allowed.
        private bool _showManualInput = true;
        #endregion

        #region Events.
        /// <summary>Event triggered when the keyboard icon is clicked.</summary>
        public event EventHandler KeyboardIconClicked;

        /// <summary>
        /// Event triggered when the rectangle coordinates have been altered.
        /// </summary>
        public event EventHandler RectChanged;
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

        /// <summary>
        /// Property to set or return the boundaries for the clipping rectangle.
        /// </summary>
        public DX.RectangleF Bounds
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the selection can be moved.
        /// </summary>
        public bool AllowMove
        {
            get => _allowMove;
            set
            {
                _allowMove = value;
                SetupHandles();
            }
        }
        /// <summary>
        /// Property to set or return whether resizing is allowed.
        /// </summary>
        public bool AllowResize
        {
            get => _allowResize;
            set
            {
                _allowResize = value;
                SetupHandles();
            }
        }
        /// <summary>
        /// Property to set or return whether to show the manual input icon.
        /// </summary>
        public bool ShowManualInput
        {
            get => _showManualInput;
            set
            {
                _showManualInput = value;
                SetupHandles();
            }
        }

        /// <summary>
        /// Property to return the rectangular region marked for clipping.
        /// </summary>
        public DX.RectangleF Rectangle
        {
            get => _clipRect;
            set
            {
                // Do nothing if the coordinates haven't changed.
                if (_clipRect.Equals(ref value))
                {
                    return;
                }

                // Flip the coordinates if needed.
                float temp = 0;
                if (value.Width < 0)
                {
                    temp = value.Right;
                    value.Right = value.Left;
                    value.Left = temp;
                }

                if (value.Height < 0)
                {
                    temp = value.Bottom;
                    value.Bottom = value.Top;
                    value.Top = temp;
                }

                if (!Bounds.IsEmpty)
                {
                    Bounds.Contains(ref value, out bool contains);

                    if (!contains)
                    {
                        value = DX.RectangleF.Intersect(Bounds, value);

                        if (value.Width < 1)
                        {
                            value.Width = 1;
                        }

                        if (value.Height < 1)
                        {
                            value.Height = 1;
                        }
                    }
                }

                _clipRect = value.Truncate();
                SetupHandles();
                OnRectChanged();
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

        /// <summary>
        /// Property to set or return the function used to transform the window client area mouse position to the image pixel space.
        /// </summary>
        public Func<DX.Vector2, DX.Vector2> TransformMouseToImage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the function used to transform a rectangle from image space to window client space.
        /// </summary>
        public Func<DX.RectangleF, DX.RectangleF> TransformImageAreaToClient
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the rectangle dimensions have been updated.
        /// </summary>
        private void OnRectChanged()
        {
            EventHandler handler = RectChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to initialize the handle locations.
        /// </summary>
        private void SetupHandles()
        {
            if (_clipRect.IsEmpty)
            {
                for (int i = 0; i < _handles.Length; ++i)
                {
                    _handles[i].HandleBounds = DX.RectangleF.Empty;
                }
                Cursor.Current = Cursors.Default;
                return;
            }

            _screenRect = TransformImageAreaToClient == null ? _clipRect : TransformImageAreaToClient(_clipRect);

            _handles[0].HandleBounds = AllowResize ? new DX.RectangleF(_screenRect.Left - 8, _screenRect.Top - 8, 8, 8) : DX.RectangleF.Empty;
            _handles[1].HandleBounds = AllowResize ? new DX.RectangleF(_screenRect.Right, _screenRect.Top - 8, 8, 8) : DX.RectangleF.Empty;
            _handles[2].HandleBounds = AllowResize ? new DX.RectangleF(_screenRect.Right, _screenRect.Bottom, 8, 8) : DX.RectangleF.Empty;
            _handles[3].HandleBounds = AllowResize ? new DX.RectangleF(_screenRect.Left - 8, _screenRect.Bottom, 8, 8) : DX.RectangleF.Empty;
            _handles[4].HandleBounds = (AllowResize) && (_screenRect.Width >= 16) ? new DX.RectangleF(_screenRect.Left + (_screenRect.Width / 2.0f) - 4, _screenRect.Top - 8, 8, 8) : DX.RectangleF.Empty;
            _handles[5].HandleBounds = (AllowResize) && (_screenRect.Width >= 16) ? new DX.RectangleF(_screenRect.Left + (_screenRect.Width / 2.0f) - 4, _screenRect.Bottom, 8, 8) : DX.RectangleF.Empty;
            _handles[6].HandleBounds = (AllowResize) && (_screenRect.Height >= 16) ? new DX.RectangleF(_screenRect.Right, _screenRect.Top + (_screenRect.Height / 2.0f) - 4, 8, 8) : DX.RectangleF.Empty;
            _handles[7].HandleBounds = (AllowResize) && (_screenRect.Height >= 16) ? new DX.RectangleF(_screenRect.Left - 8, _screenRect.Top + (_screenRect.Height / 2.0f) - 4, 8, 8) : DX.RectangleF.Empty;
            _handles[8].HandleBounds = (AllowMove) && (_screenRect.Width >= 32) && (_screenRect.Height >= 32) ?
                new DX.RectangleF((_screenRect.Width / 2.0f) + _screenRect.Left - 8, (_screenRect.Height / 2.0f) + _screenRect.Top - 8, 16, 16)
                : DX.RectangleF.Empty;
            _handles[9].Texture = _keyboardIcon.Value;

            var keyBounds = new DX.RectangleF(_screenRect.Left + 8, _screenRect.Top + 8, _keyboardIcon.Value.Width, _keyboardIcon.Value.Height);
            _handles[9].HandleBounds = (AllowResize)
                && (ShowManualInput)
                && ((_handles[8].HandleBounds.IsEmpty) || (!_handles[8].HandleBounds.Intersects(keyBounds)))
                && ((_screenRect.Width >= 32) && (_screenRect.Height >= 32)) ? keyBounds : DX.RectangleF.Empty;

            GetActiveHandle();
        }

        /// <summary>
        /// Function to perform the dragging on the handles or the body of the selection.
        /// </summary>
        /// <param name="mouse">The current mouse coordinates, transformed into local image space.</param>
        private void DragHandles(DX.Vector2 mouse)
        {
            DX.Vector2.Subtract(ref mouse, ref _startDrag, out DX.Vector2 dragDelta);

            switch (_activeHandleIndex)
            {
                // NW
                case 0:
                    Rectangle = new DX.RectangleF
                    {
                        Left = (_dragRect.Left + dragDelta.X).Min(_clipRect.Right - 1),
                        Top = (_dragRect.Top + dragDelta.Y).Min(_clipRect.Bottom - 1),
                        Right = _clipRect.Right,
                        Bottom = _clipRect.Bottom
                    };
                    break;
                // NE
                case 1:
                    Rectangle = new DX.RectangleF
                    {
                        Left = _clipRect.Left,
                        Top = (_dragRect.Top + dragDelta.Y).Min(_clipRect.Bottom - 1),
                        Right = (_dragRect.Right + dragDelta.X).Max(_clipRect.Left + 1),
                        Bottom = _clipRect.Bottom
                    };
                    break;
                // SE
                case 2:
                    Rectangle = new DX.RectangleF
                    {
                        Left = _clipRect.Left,
                        Top = _clipRect.Top,
                        Right = (_dragRect.Right + dragDelta.X).Max(_clipRect.Left + 1),
                        Bottom = (_dragRect.Bottom + dragDelta.Y).Max(_clipRect.Top + 1)
                    };
                    break;
                // SW
                case 3:
                    Rectangle = new DX.RectangleF
                    {
                        Left = (_dragRect.Left + dragDelta.X).Min(_clipRect.Right - 1),
                        Top = _clipRect.Top,
                        Right = _clipRect.Right,
                        Bottom = (_dragRect.Bottom + dragDelta.Y).Max(_clipRect.Top + 1)
                    };
                    break;
                // N
                case 4:
                    Rectangle = new DX.RectangleF
                    {
                        Left = _clipRect.Left,
                        Top = (_dragRect.Top + dragDelta.Y).Min(_clipRect.Bottom - 1),
                        Right = _clipRect.Right,
                        Bottom = _clipRect.Bottom
                    };
                    break;
                // S
                case 5:
                    Rectangle = new DX.RectangleF
                    {
                        Left = _clipRect.Left,
                        Top = _clipRect.Top,
                        Right = _clipRect.Right,
                        Bottom = (_dragRect.Bottom + dragDelta.Y).Max(_clipRect.Top + 1)
                    };
                    break;
                // E
                case 6:
                    Rectangle = new DX.RectangleF
                    {
                        Left = _clipRect.Left,
                        Top = _clipRect.Top,
                        Right = (_dragRect.Right + dragDelta.X).Max(_clipRect.Left + 1),
                        Bottom = _clipRect.Bottom
                    };
                    break;
                // W
                case 7:
                    Rectangle = new DX.RectangleF
                    {
                        Left = (_dragRect.Left + dragDelta.X).Min(_clipRect.Right - 1),
                        Top = _clipRect.Top,
                        Right = _clipRect.Right,
                        Bottom = _clipRect.Bottom
                    };
                    break;
                // Drag body.
                case 8:
                    Rectangle = new DX.RectangleF(_dragRect.Left + dragDelta.X, _dragRect.Top + dragDelta.Y, Rectangle.Width, Rectangle.Height);
                    break;
            }
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

            int prevActiveIndex = _activeHandleIndex;
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
        /// Function called when the mouse button is moved.
        /// </summary>
        /// <param name="button">The button that was held down while moving.</param>
        /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseMove(MouseButtons button)
        {
            GetActiveHandle();

            if ((_activeHandleIndex == -1) || (_activeHandleIndex > 8))
            {
                return false;
            }

            DX.Vector2 mouse = TransformMouseToImage == null ? MousePosition : TransformMouseToImage(MousePosition);

            if (button != MouseButtons.Left)
            {
                IsDragging = false;
            }

            if (IsDragging)
            {
                DragHandles(mouse);
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
            GetActiveHandle();

            if ((_activeHandleIndex == -1) || (_activeHandleIndex > 8))
            {
                return false;
            }

            // Only drag if we've moved a little bit while having the mouse button held down.
            IsDragging = true;
            _startDrag = TransformMouseToImage == null ? MousePosition : TransformMouseToImage(MousePosition);
            _dragRect = _clipRect;
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

            if (_activeHandleIndex == -1)
            {
                return false;
            }

            if ((_activeHandleIndex == 9) && (!IsDragging))
            {
                EventHandler handler = KeyboardIconClicked;
                handler?.Invoke(this, EventArgs.Empty);
            }

            if (button == MouseButtons.Left)
            {
                _startDrag = DX.Vector2.Zero;
                IsDragging = false;
            }

            return true;
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

            if ((_activeHandleIndex == -1) || (_activeHandleIndex > 8))
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

            _dragRect = _clipRect;
            _startDrag = TransformMouseToImage == null ? MousePosition : TransformMouseToImage(MousePosition);

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
                    MousePosition = new DX.Vector2(MousePosition.X + offset , MousePosition.Y);
                    break;
                case Keys.Left:
                case Keys.NumPad4:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X - offset, Cursor.Position.Y);
                    MousePosition = new DX.Vector2(MousePosition.X - offset, MousePosition.Y);
                    break;
                case Keys.NumPad7:
                    Cursor.Position = new Drawing.Point(Cursor.Position.X - offset, Cursor.Position.Y -offset);
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

            DragHandles(TransformMouseToImage == null ? MousePosition : TransformMouseToImage(MousePosition));

            return true;
        }

        /// <summary>
        /// Function to force a refresh of the service.
        /// </summary>
        public void Refresh() => SetupHandles();

        /// <summary>
        /// Function to render the clipping region.
        /// </summary>
        public void Render()
        {
            if (_screenRect.IsEmpty)
            {
                return;
            }

            GetActiveHandle();

            _marchingAnts.Draw(_screenRect);

            for (int i = 0; i < _handles.Length; ++i)
            {
                GorgonTexture2DView texture = _handles[i].Texture;
                DX.RectangleF handleBounds = _handles[i].HandleBounds;

                if (handleBounds.IsEmpty)
                {
                    continue;
                }

                // 9 is the keyboard icon index, and if we're dragging we shouldn't draw it.
                if ((i == 9) && (IsDragging))
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
            }
        }

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
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.Services.RectClipperService"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="marchingAnts">The marching ants.</param>
        public RectClipperService(Gorgon2D renderer, IMarchingAnts marchingAnts)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _marchingAnts = marchingAnts ?? throw new ArgumentNullException(nameof(marchingAnts));

            for (int i = 0; i < _handles.Length; ++i)
            {
                _handles[i] = new RectHandle();
            }

            _handles[2].HandleCursor = _handles[0].HandleCursor = Cursors.SizeNWSE;
            _handles[3].HandleCursor = _handles[1].HandleCursor = Cursors.SizeNESW;
            _handles[4].HandleCursor = _handles[5].HandleCursor = Cursors.SizeNS;
            _handles[6].HandleCursor = _handles[7].HandleCursor = Cursors.SizeWE;
            _handles[8].HandleCursor = Cursors.SizeAll;
            _handles[9].HandleCursor = Cursors.Hand;

            _keyboardIcon = new Lazy<GorgonTexture2DView>(() => GorgonTexture2DView.CreateTexture(renderer.Graphics, new GorgonTexture2DInfo("RectClipper_KeyboardIcon"), EditorCommonResources.KeyboardIcon), true);
        }
        #endregion
    }
}
