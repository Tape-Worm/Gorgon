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
// Created: July 11, 2020 9:51:23 PM
// 
#endregion

using System;
using System.Numerics;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using DX = SharpDX;

namespace Gorgon.Editor.Services;

/// <summary>
/// A service used to edit an anchor point on a sprite.
/// </summary>
public class AnchorEditService : IAnchorEditService
{
    #region Variables.
    // The icon for the anchor.
    private readonly GorgonSprite _anchorIcon;
    // The 2D renderer for the application.
    private readonly Gorgon2D _renderer;
    // The anchor position, in image coordinates.
    private Vector2 _anchorPosition;
    // The start point of the drag operation.
    private Vector2 _dragStart;
    // The current position of the icon when drag starts.
    private Vector2 _dragStartPosition;
    // The animation controller used to update the opacity of the icon over time.
    private readonly GorgonSpriteAnimationController _animController = new();
    // The icon opacity animation.
    private readonly IGorgonAnimation _animation;
    // The current mouse position.
    private Vector2 _mousePosition;
    // The boundaries for the anchor point.
    private DX.Rectangle _bounds;
    // Flag to indicate that the mouse cursor is hidden.
    private bool _cursorHidden;
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
    public GorgonOrthoCamera Camera
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
    public Vector2 AnchorPosition
    {
        get => _anchorPosition;
        set
        {
            if (_anchorPosition.Equals(value))
            {
                return;
            }

            SetAnchorPosition(value);
        }
    }

    /// <summary>Property to set or return the center position of the sprite.</summary>
    public Vector2 CenterPosition
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
    private void SetAnchorPosition(Vector2 value)
    {
        _anchorPosition = value.Truncate();

        _anchorPosition.X = _anchorPosition.X.Min(_bounds.Right).Max(_bounds.Left);
        _anchorPosition.Y = _anchorPosition.Y.Min(_bounds.Bottom).Max(_bounds.Top);

        EventHandler handler = AnchorChanged;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Function to update the mouse cursor when over the anchor icon.
    /// </summary>
    private void UpdateCursor()
    {
        if (IsDragging) 
        {
            return;
        }

        DX.RectangleF iconBounds = _renderer.MeasureSprite(_anchorIcon);
        Cursor cursor = Cursors.Default;

        if (iconBounds.Contains(_mousePosition.X, _mousePosition.Y))
        {
            cursor = Cursors.Hand;
        }

        if (Cursor.Current != cursor)
        {
            Cursor.Current = cursor;
        }
    }

    /// <summary>Function called when the mouse button is pressed.</summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <returns>
    ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
    public bool MouseDown(MouseArgs args)
    {
        // Just in case we lose the mouse cursor for some reason.
        if ((IsDragging) && (_cursorHidden))
        {
            Cursor.Show();
            _cursorHidden = false;
        }

        _mousePosition = args.ClientPosition.ToVector2();
        DX.RectangleF iconBounds = _renderer.MeasureSprite(_anchorIcon);

        if (!iconBounds.Contains(_mousePosition.X, _mousePosition.Y))
        {
            UpdateCursor();
            return false;
        }

        Cursor.Hide();
        _cursorHidden = true;
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
        _mousePosition = args.ClientPosition.ToVector2();
        if (args.MouseButtons != MouseButtons.Left)
        {
            if (_cursorHidden)
            {
                Cursor.Show();
                _cursorHidden = false;
            }
            IsDragging = false;
        }

        UpdateCursor();

        if (!IsDragging)
        {
            return false;
        }

        Vector2 mouse = args.CameraSpacePosition;
        var dragDelta = Vector2.Subtract(mouse, _dragStart);
        AnchorPosition = (new Vector2(_dragStartPosition.X + dragDelta.X, _dragStartPosition.Y + dragDelta.Y)).Truncate();

        return true;
    }

    /// <summary>Function called when the mouse button is released.</summary>
    /// <param name="_">Not used.</param>
    /// <returns>
    ///   <b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
    public bool MouseUp(MouseArgs _)
    {
        if (_cursorHidden)
        {
            Cursor.Show();
            _cursorHidden = false;
        }

        UpdateCursor();

        if (IsDragging)
        {                
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
                AnchorPosition = new Vector2(AnchorPosition.X, AnchorPosition.Y - offset);
                return true;
            case Keys.NumPad2:
            case Keys.Down:
                AnchorPosition = new Vector2(AnchorPosition.X, AnchorPosition.Y + offset);
                return true;
            case Keys.NumPad4:
            case Keys.Left:
                AnchorPosition = new Vector2(AnchorPosition.X - offset, AnchorPosition.Y);
                return true;
            case Keys.NumPad6:
            case Keys.Right:
                AnchorPosition = new Vector2(AnchorPosition.X + offset, AnchorPosition.Y);
                return true;
            case Keys.NumPad7:
                AnchorPosition = new Vector2(AnchorPosition.X - offset, AnchorPosition.Y - offset);
                return true;
            case Keys.NumPad9:
                AnchorPosition = new Vector2(AnchorPosition.X + offset, AnchorPosition.Y - offset);
                return true;
            case Keys.NumPad1:
                AnchorPosition = new Vector2(AnchorPosition.X - offset, AnchorPosition.Y + offset);
                return true;
            case Keys.NumPad3:
                AnchorPosition = new Vector2(AnchorPosition.X + offset, AnchorPosition.Y + offset);
                return true;
        }

        return false;
    }

    /// <summary>
    /// Function to reset the anchor value.
    /// </summary>
    public void Reset() => _anchorPosition = Vector2.Zero;

    /// <summary>Function to render the anchor UI.</summary>
    public void Render()
    {
        if (_animController.State != AnimationState.Playing)
        {
            _animController.Play(_anchorIcon, _animation);
        }

        UpdateCursor();

        Vector2 position = AnchorPosition - CenterPosition;
        Vector3 screenAnchor = default;
        Camera?.Unproject(new Vector3(position.X, position.Y, 0), out screenAnchor);
        _anchorIcon.Position = (new Vector2(screenAnchor.X, screenAnchor.Y)).Truncate();

        _renderer.DrawSprite(_anchorIcon);
        _renderer.DrawFilledRectangle(new DX.RectangleF(_anchorIcon.Position.X - 4, _anchorIcon.Position.Y - 1, 9, 3), GorgonColor.Black);
        _renderer.DrawFilledRectangle(new DX.RectangleF(_anchorIcon.Position.X - 1, _anchorIcon.Position.Y - 4, 3, 9), GorgonColor.Black);
        _renderer.DrawLine(_anchorIcon.Position.X - 3, _anchorIcon.Position.Y, _anchorIcon.Position.X + 4, _anchorIcon.Position.Y, GorgonColor.White);
        _renderer.DrawLine(_anchorIcon.Position.X, _anchorIcon.Position.Y - 3, _anchorIcon.Position.X, _anchorIcon.Position.Y + 4, GorgonColor.White);

        _animController.Update();
    }
    #endregion

    #region Constructor.
    /// <summary>Initializes a new instance of the <see cref="AnchorEditService"/> class.</summary>
    /// <param name="renderer">The 2D renderer for the application.</param>
    /// <param name="anchorSprite">The sprite representing the anchor icon.</param>
    /// <param name="bounds">The boundaries for the anchor point.</param>
    public AnchorEditService(Gorgon2D renderer, GorgonSprite anchorSprite, DX.Rectangle bounds)
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
