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
// Created: April 3, 2019 7:57:15 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Graphics;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A service used to edit an anchor point on a sprite.
    /// </summary>
    internal class AnchorEditService
        : IAnchorEditService
    {
        #region Variables.
        // The icon for the anchor.
        private readonly GorgonSprite _anchorIcon;
        // The 2D renderer for the application.
        private readonly Gorgon2D _renderer;
        // The anchor position, in screen coordinates.
        private DX.Vector2 _screenAnchor;
        // The anchor position, in image coordinates.
        private DX.Vector2 _anchorPosition;
        // The start point of the drag operation.
        private DX.Vector2 _dragStart;
        // The current position of the icon when drag starts.
        private DX.Vector2 _dragStartPosition;
        // The animation controller used to update the opacity of the icon over time.
        private readonly GorgonSpriteAnimationController _animController;
        // The icon opacity animation.
        private readonly IGorgonAnimation _animation;
        // The boundaries for the anchor.
        private DX.RectangleF _anchorBounds;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the anchor position is updated.
        /// </summary>
        public event EventHandler AnchorChanged;

        /// <summary>
        /// Event triggered when the anchor bounds are changed.
        /// </summary>
        public event EventHandler BoundsChanged;
        #endregion

        #region Properties.
        /// <summary>Property to return whether we're in the middle of a drag operation or not.</summary>
        public bool IsDragging
        {
            get;
            private set;
        }

        /// <summary>Property to set or return the position of the sprite anchor.</summary>
        public DX.Vector2 AnchorPosition
        {
            get => _anchorPosition;
            set
            {
                if (_anchorPosition.Equals(ref value))
                {
                    return;
                }

                SetAnchorPosition(value);
            }
        }

        /// <summary>Property to set or return the boundaries for the anchor position.</summary>
        public DX.RectangleF Bounds
        {
            get => _anchorBounds;
            set
            {
                if (_anchorBounds.Equals(ref value))
                {
                    return;
                }

                _anchorBounds = value;

                EventHandler handler = BoundsChanged;
                BoundsChanged?.Invoke(this, EventArgs.Empty);

                SetAnchorPosition(_anchorPosition);
            }
        }

        /// <summary>Property to set or return the mouse position in the client area of the primary rendering window.</summary>
        public DX.Vector2 MousePosition
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the function used to transform a point from local clip space to window client space.
        /// </summary>
        public Func<DX.Vector2, DX.Vector2> PointFromClient
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the function used to transform a point from local clip space into window client space.
        /// </summary>
        public Func<DX.Vector2, DX.Vector2> PointToClient
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign the anchor position.
        /// </summary>
        /// <param name="value">The value to assign to the anchor position.</param>
        private void SetAnchorPosition(DX.Vector2 value)
        {
            if (!Bounds.IsEmpty)
            {
                if (value.X < Bounds.X)
                {
                    value.X = Bounds.X;
                }

                if (value.Y < Bounds.Y)
                {
                    value.Y = Bounds.Y;
                }

                if (value.X > Bounds.Right)
                {
                    value.X = Bounds.Right;
                }

                if (value.Y > Bounds.Bottom)
                {
                    value.Y = Bounds.Bottom;
                }
            }

            _anchorPosition = value.Truncate();
            Refresh();

            EventHandler handler = AnchorChanged;
            AnchorChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Function called when a key is held down.</summary>
        /// <param name="key">The key that was held down.</param>
        /// <param name="modifiers">The modifier keys held down with the <paramref name="key" />.</param>
        /// <returns>
        ///   <b>true</b> if the key was handled, <b>false</b> if it was not.</returns>
        public bool KeyDown(Keys key, Keys modifiers) => false;

        /// <summary>Function called when the mouse button is pressed.</summary>
        /// <param name="button">The button that was pressed.</param>
        /// <returns>
        ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseDown(MouseButtons button)
        {
            DX.RectangleF iconBounds = _renderer.MeasureSprite(_anchorIcon);

            if (!iconBounds.Contains(MousePosition))
            {
                return false;
            }

            Cursor.Hide();
            _dragStartPosition = AnchorPosition;
            _dragStart = PointFromClient == null ? MousePosition : PointFromClient(MousePosition);
            IsDragging = true;

            return true;
        }

        /// <summary>Function called when the mouse button is moved.</summary>
        /// <param name="button">The button that was held down while moving.</param>
        /// <returns>
        ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseMove(MouseButtons button)
        {
            if (button != MouseButtons.Left)
            {
                IsDragging = false;
            }

            DX.RectangleF iconBounds = _renderer.MeasureSprite(_anchorIcon);

            if (!IsDragging)
            {
                if (!iconBounds.Contains(MousePosition))
                {
                    if (Cursor.Current != Cursors.Default)
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
                else
                {
                    if (Cursor.Current != Cursors.Hand)
                    {
                        Cursor.Current = Cursors.Hand;
                    }
                }

                return false;
            }

            DX.Vector2 mouse = PointFromClient == null ? MousePosition : PointFromClient(MousePosition);
            DX.Vector2.Subtract(ref mouse, ref _dragStart, out DX.Vector2 dragDelta);
            AnchorPosition = new DX.Vector2(_dragStartPosition.X + dragDelta.X, _dragStartPosition.Y + dragDelta.Y);

            return true;
        }

        /// <summary>Function called when the mouse button is released.</summary>
        /// <param name="button">The mouse button that was released.</param>
        /// <returns>
        ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseUp(MouseButtons button)
        {
            if (IsDragging)
            {
                Cursor.Show();
                IsDragging = false;
                return true;
            }

            return false;
        }

        /// <summary>Function to force a refresh of the service.</summary>
        public void Refresh() => _screenAnchor = PointToClient == null ? _anchorPosition : PointToClient(_anchorPosition);

        /// <summary>Function to render the anchor region.</summary>
        public void Render()
        {
            if (_animController.State != AnimationState.Playing)
            {
                _animController.Play(_anchorIcon, _animation);
            }

            _anchorIcon.Position = _screenAnchor.Truncate();

            _renderer.Begin();
            _renderer.DrawSprite(_anchorIcon);
            _renderer.DrawFilledRectangle(new DX.RectangleF(_anchorIcon.Position.X - 4, _anchorIcon.Position.Y - 1, 9, 3), GorgonColor.Black);
            _renderer.DrawFilledRectangle(new DX.RectangleF(_anchorIcon.Position.X - 1, _anchorIcon.Position.Y - 4, 3, 9), GorgonColor.Black);
            _renderer.DrawLine(_anchorIcon.Position.X - 3, _anchorIcon.Position.Y, _anchorIcon.Position.X + 4, _anchorIcon.Position.Y, GorgonColor.White);
            _renderer.DrawLine(_anchorIcon.Position.X, _anchorIcon.Position.Y - 3, _anchorIcon.Position.X, _anchorIcon.Position.Y + 4, GorgonColor.White);
            _renderer.End();

            _animController.Update();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.AnchorEditService"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="anchorSprite">The sprite representing the anchor icon.</param>
        public AnchorEditService(Gorgon2D renderer, GorgonSprite anchorSprite)
        {
            _renderer = renderer;
            _anchorIcon = anchorSprite;
            _animController = new GorgonSpriteAnimationController();

            var builder = new GorgonAnimationBuilder();

            _animation = builder.EditColor("Color")
                .SetKey(new GorgonKeyGorgonColor(0.0f, GorgonColor.White))
                .SetKey(new GorgonKeyGorgonColor(2.0f, new GorgonColor(GorgonColor.White, 0.3f)))
                .SetKey(new GorgonKeyGorgonColor(4.0f, new GorgonColor(GorgonColor.LightRed, 0.6f)))
                .SetKey(new GorgonKeyGorgonColor(6.0f, new GorgonColor(GorgonColor.LightRed, 0.3f)))
                .SetKey(new GorgonKeyGorgonColor(8.0f, new GorgonColor(GorgonColor.LightGreen, 0.6f)))
                .SetKey(new GorgonKeyGorgonColor(10.0f, new GorgonColor(GorgonColor.LightGreen, 0.3f)))
                .SetKey(new GorgonKeyGorgonColor(12.0f, new GorgonColor(GorgonColor.LightBlue, 0.6f)))
                .SetKey(new GorgonKeyGorgonColor(14.0f, new GorgonColor(GorgonColor.LightBlue, 0.3f)))
                .SetKey(new GorgonKeyGorgonColor(16.0f, GorgonColor.White))
                .EndEdit()
                .Build("Icon Opacity");
        }
        #endregion
    }
}
