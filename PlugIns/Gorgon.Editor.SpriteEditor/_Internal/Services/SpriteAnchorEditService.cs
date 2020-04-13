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
// Created: May 19, 2020 12:48:27 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// A service used to edit an anchor point on a sprite.
    /// </summary>
    internal class SpriteAnchorEditService
    {
        #region Variables.
        // The icon for the anchor.
        private readonly GorgonSprite _anchorIcon;
        // The 2D renderer for the application.
        private readonly Gorgon2D _renderer;
        // The anchor position, in image coordinates.
        private DX.Vector2 _anchorPosition;
        // The start point of the drag operation.
        private DX.Vector2 _dragStart;
        // The current position of the icon when drag starts.
        private DX.Vector2 _dragStartPosition;
        // The animation controller used to update the opacity of the icon over time.
        private readonly GorgonSpriteAnimationController _animController = new GorgonSpriteAnimationController();
        // The icon opacity animation.
        private readonly IGorgonAnimation _animation;
        // The current mouse position.
        private DX.Vector2 _mousePosition;
        // The boundaries for the anchor point.
        private DX.Rectangle _bounds;
        #endregion

        #region Events.
        /// <summary>
        /// Event triggered when the anchor position is updated.
        /// </summary>
        public event EventHandler AnchorChanged;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the camera for the renderer.
        /// </summary>
        public IGorgon2DCamera Camera
        {
            get;
            set;
        }

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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to assign the anchor position.
        /// </summary>
        /// <param name="value">The value to assign to the anchor position.</param>
        private void SetAnchorPosition(DX.Vector2 value)
        {
            _anchorPosition = value.Truncate();

            _anchorPosition.X = _anchorPosition.X.Min(_bounds.Right).Max(_bounds.Left);
            _anchorPosition.Y = _anchorPosition.Y.Min(_bounds.Bottom).Max(_bounds.Top);

            EventHandler handler = AnchorChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Function called when the mouse button is pressed.</summary>
        /// <param name="args">The mouse event arguments.</param>
        /// <returns>
        ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseDown(MouseArgs args)
        {
            _mousePosition = args.ClientPosition;
            DX.RectangleF iconBounds = _renderer.MeasureSprite(_anchorIcon);

            if (!iconBounds.Contains(_mousePosition))
            {
                return false;
            }

            Cursor.Hide();
            _dragStartPosition = AnchorPosition;
            _dragStart = args.CameraSpacePosition;
            IsDragging = true;

            return true;
        }

        /// <summary>Function called when the mouse button is moved.</summary>
        /// <param name="args">The mouse event arguments.</param>
        /// <returns>
        ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseMove(MouseArgs args)
        {
            _mousePosition = args.ClientPosition;
            if (args.MouseButtons != MouseButtons.Left)
            {
                IsDragging = false;
            }

            DX.RectangleF iconBounds = _renderer.MeasureSprite(_anchorIcon);

            if (!IsDragging)
            {
                if (!iconBounds.Contains(_mousePosition))
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

            DX.Vector2 mouse = args.CameraSpacePosition;
            DX.Vector2.Subtract(ref mouse, ref _dragStart, out DX.Vector2 dragDelta);
            AnchorPosition = (new DX.Vector2(_dragStartPosition.X + dragDelta.X, _dragStartPosition.Y + dragDelta.Y)).Truncate();

            return true;
        }

        /// <summary>Function called when the mouse button is released.</summary>
        /// <param name="_">Not used.</param>
        /// <returns>
        ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
        public bool MouseUp(MouseArgs _)
        {
            if (IsDragging)
            {
                Cursor.Show();
                IsDragging = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Function to intercept keyboard key presses.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        /// <returns><b>true</b> if the event is handled, <b>false</b> if not.</returns>
        public bool KeyDown(PreviewKeyDownEventArgs e)
        {
            int offset = 1;

            if ((e.Modifiers & Keys.Shift) == Keys.Shift)
            {
                offset = 10;
            }

            if ((e.Modifiers & Keys.Control) == Keys.Control)
            {
                offset = 100;
            }

            switch (e.KeyCode)
            {
                case Keys.NumPad8:
                case Keys.Up:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X, AnchorPosition.Y - offset);
                    return true;
                case Keys.NumPad2:
                case Keys.Down:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X, AnchorPosition.Y + offset);
                    return true;
                case Keys.NumPad4:
                case Keys.Left:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X - offset, AnchorPosition.Y);
                    return true;
                case Keys.NumPad6:
                case Keys.Right:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X + offset, AnchorPosition.Y);
                    return true;
                case Keys.NumPad7:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X - offset, AnchorPosition.Y - offset);
                    return true;
                case Keys.NumPad9:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X + offset, AnchorPosition.Y - offset);
                    return true;
                case Keys.NumPad1:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X - offset, AnchorPosition.Y + offset);
                    return true;
                case Keys.NumPad3:
                    AnchorPosition = new DX.Vector2(AnchorPosition.X + offset, AnchorPosition.Y + offset);
                    return true;
            }

            return false;
        }

        /// <summary>Function to render the anchor region.</summary>
        public void Render()
        {
            if (_animController.State != AnimationState.Playing)
            {
                _animController.Play(_anchorIcon, _animation);
            }

            var position = (DX.Vector3)AnchorPosition;
            DX.Vector3 screenAnchor = DX.Vector3.Zero;
            Camera?.Unproject(ref position, out screenAnchor);            
            _anchorIcon.Position = ((DX.Vector2)screenAnchor).Truncate();

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
        /// <summary>Initializes a new instance of the <see cref="SpriteAnchorEditService"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="anchorSprite">The sprite representing the anchor icon.</param>
        /// <param name="bounds">The boundaries for the anchor point.</param>
        public SpriteAnchorEditService(Gorgon2D renderer, GorgonSprite anchorSprite, DX.Rectangle bounds)
        {
            _renderer = renderer;
            _anchorIcon = anchorSprite;
            _bounds = bounds;

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
