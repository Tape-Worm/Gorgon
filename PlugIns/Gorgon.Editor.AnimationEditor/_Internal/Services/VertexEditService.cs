﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 19, 2020 12:02:56 AM
// 

using System.Numerics;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The service used to edit sprite vertices for an animation
/// </summary>
internal class VertexEditService
{

    // The renderer used to draw the UI.
    private readonly Gorgon2D _renderer;
    // The list of vertices to update.
    private readonly Vector2[] _vertices = new Vector2[4];
    // The handles for grabbing.
    private readonly RectHandle[] _handles =
    [
        new(),
        new(),
        new(),
        new()
    ];
    // The vertex positions, in screen space.
    private readonly Vector2[] _screenVertices = new Vector2[4];
    // The currently selected corner.
    private TrackSpriteProperty _selectedCorner = TrackSpriteProperty.None;
    // The corner for mouse over.
    private TrackSpriteProperty _activeCorner = TrackSpriteProperty.None;
    // Starting drag position.
    private Vector2 _localStartDrag;
    // Mouse position (in client space).
    private Vector2 _mousePos;
    // The original position of the handle being dragged.
    private Vector2 _dragHandlePos;
    // The camera used to render the UI.
    private GorgonOrthoCamera _camera;

    // Event triggered when the vertex coordinates have been altered.        
    private event EventHandler<VertexChangedEventArgs> VerticesChangedEvent;

    /// <summary>
    /// Event triggered when the vertex coordinates have been altered.        
    /// </summary>
    public event EventHandler<VertexChangedEventArgs> VerticesChanged
    {
        add
        {
            if (value is null)
            {
                VerticesChangedEvent = null;
                return;
            }

            VerticesChangedEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            VerticesChangedEvent -= value;
        }
    }

    /// <summary>
    /// Property to set or return the camera being used.
    /// </summary>
    public GorgonOrthoCamera Camera
    {
        get => _camera;
        set
        {
            _camera = value;
            SetupHandles();
        }
    }

    /// <summary>
    /// Property to return whether we're in the middle of a drag operation or not.
    /// </summary>
    public bool IsDragging
    {
        get;
        private set;
    }

    /// <summary>Property to set or return the vertices for the sprite.</summary>
    public IReadOnlyList<Vector2> Vertices
    {
        get => _vertices;
        set
        {
            if (value is null)
            {
                Array.Clear(_vertices, 0, _vertices.Length);
                return;
            }

            if (_vertices.SequenceEqual(value))
            {
                return;
            }

            for (int i = 0; i < _vertices.Length; ++i)
            {
                _vertices[i] = i < value.Count ? value[i] : Vector2.Zero;
            }

            SetupHandles();
        }
    }

    /// <summary>
    /// Property to return the vertex that was selected.
    /// </summary>
    public Vector2 SelectedVertex
    {
        get
        {
            int vertexIndex = GetIndex();

            return vertexIndex == -1 ? Vector2.Zero : _vertices[vertexIndex];
        }
    }

    /// <summary>
    /// Property to set or return the selected sprite corner to update.
    /// </summary>
    public TrackSpriteProperty SpriteCorner
    {
        get => _selectedCorner;
        set
        {
            if (_selectedCorner == value)
            {
                return;
            }

            _selectedCorner = value;
            SetupHandles();
        }
    }

    /// <summary>
    /// Function to raise the <see cref="VerticesChangedEvent"/> event.
    /// </summary>
    /// <param name="vertexIndex">The index of the vertex that was changed.</param>
    /// <param name="oldPos">The old position of the vertex.</param>
    /// <param name="newPos">The new position of the vertex.</param>
    private void OnVerticesChanged(int vertexIndex, Vector2 oldPos, Vector2 newPos)
    {
        EventHandler<VertexChangedEventArgs> handler = VerticesChangedEvent;
        handler?.Invoke(this, new VertexChangedEventArgs(vertexIndex, oldPos, newPos));
    }

    /// <summary>
    /// Function to retrieve the vertex index from the selected sprite corner.
    /// </summary>
    /// <returns>The index of the sprite.</returns>
    private int GetIndex() => _selectedCorner switch
    {
        TrackSpriteProperty.UpperLeft => 0,
        TrackSpriteProperty.UpperRight => 1,
        TrackSpriteProperty.LowerLeft => 3,
        TrackSpriteProperty.LowerRight => 2,
        _ => -1,
    };

    /// <summary>
    /// Function to perform the dragging on the handles or the body of the selection.
    /// </summary>
    /// <param name="localMousePos">The position of the mouse, relative to the sprite.</param>
    private void DragHandles(Vector2 localMousePos)
    {
        int vertexIndex = GetIndex();
        if (vertexIndex == -1)
        {
            return;
        }

        Vector2 currentPosition = _vertices[vertexIndex];
        Vector2 dragDelta = Vector2.Subtract(localMousePos, _localStartDrag);
        _vertices[vertexIndex] = (new Vector2(_dragHandlePos.X + dragDelta.X, _dragHandlePos.Y + dragDelta.Y)).Truncate();
        SetupHandles();

        OnVerticesChanged(vertexIndex, currentPosition, _vertices[vertexIndex]);
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

        int vertexIndex = GetIndex();

        if (vertexIndex == -1)
        {
            Cursor.Current = Cursors.Default;
            return;
        }

        GorgonRectangleF handleBounds = _handles[vertexIndex].HandleBounds;

        if ((handleBounds.IsEmpty) || (!handleBounds.Contains(_mousePos.X, _mousePos.Y)))
        {
            _activeCorner = TrackSpriteProperty.None;
        }
        else
        {
            _activeCorner = _selectedCorner;
            mouseCursor = _handles[vertexIndex].HandleCursor;
        }

        if (vertexIndex == -1)
        {
            mouseCursor = Cursors.Default;
        }

        if (mouseCursor != Cursor.Current)
        {
            Cursor.Current = vertexIndex == -1 ? Cursors.Default : mouseCursor;
        }
    }

    /// <summary>
    /// Function to set up the handles for 
    /// </summary>
    private void SetupHandles()
    {
        // Convert to client space.
        if (Camera is not null)
        {
            for (int i = 0; i < _screenVertices.Length; ++i)
            {
                Vector3 unprojected = Camera.Unproject(new Vector3(_vertices[i].X, _vertices[i].Y, 0));
                _screenVertices[i] = new Vector2(unprojected.X, unprojected.Y);
            }
        }
        else
        {
            _screenVertices[0] = new Vector2(_vertices[0].X, _vertices[0].Y);
            _screenVertices[1] = new Vector2(_vertices[1].X, _vertices[1].Y);
            _screenVertices[2] = new Vector2(_vertices[2].X, _vertices[2].Y);
            _screenVertices[3] = new Vector2(_vertices[3].X, _vertices[3].Y);
        }

        _handles[0].HandleBounds = new GorgonRectangleF(_screenVertices[0].X - 8, _screenVertices[0].Y - 8, 8, 8);
        _handles[1].HandleBounds = new GorgonRectangleF(_screenVertices[1].X, _screenVertices[1].Y - 8, 8, 8);
        _handles[2].HandleBounds = new GorgonRectangleF(_screenVertices[2].X, _screenVertices[2].Y, 8, 8);
        _handles[3].HandleBounds = new GorgonRectangleF(_screenVertices[3].X - 8, _screenVertices[3].Y, 8, 8);

        GetActiveHandle();
    }

    /// <summary>
    /// Function called when a key is held down.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns><b>true</b> if the key was handled, <b>false</b> if it was not.</returns>
    public bool KeyDown(PreviewKeyDownEventArgs args)
    {
        int offset = 1;

        if ((args.Modifiers & Keys.Shift) == Keys.Shift)
        {
            offset = 10;
        }

        if ((args.Modifiers & Keys.Control) == Keys.Control)
        {
            offset = 100;
        }

        Vector2 localPos = Vector2.Zero;
        int vertexIndex = GetIndex();

        if (vertexIndex != -1)
        {
            _dragHandlePos = _vertices[vertexIndex];
            localPos = _localStartDrag = _vertices[vertexIndex];
        }

        switch (args.KeyCode)
        {
            case Keys.Up:
            case Keys.NumPad8:
                localPos = new Vector2(localPos.X, localPos.Y - offset);
                break;
            case Keys.Down:
            case Keys.NumPad2:
                localPos = new Vector2(localPos.X, localPos.Y + offset);
                break;
            case Keys.Right:
            case Keys.NumPad6:
                localPos = new Vector2(localPos.X + offset, localPos.Y);
                break;
            case Keys.Left:
            case Keys.NumPad4:
                localPos = new Vector2(localPos.X - offset, localPos.Y);
                break;
            case Keys.NumPad7:
                localPos = new Vector2(localPos.X - offset, localPos.Y - offset);
                break;
            case Keys.NumPad9:
                localPos = new Vector2(localPos.X + offset, localPos.Y - offset);
                break;
            case Keys.NumPad1:
                localPos = new Vector2(localPos.X - offset, localPos.Y + offset);
                break;
            case Keys.NumPad3:
                localPos = new Vector2(localPos.X + offset, localPos.Y + offset);
                break;
            default:
                return false;
        }

        if (vertexIndex == -1)
        {
            return false;
        }

        DragHandles(localPos);

        return true;
    }

    /// <summary>
    /// Function called when the mouse button is moved.
    /// </summary>
    /// <param name="args">Mouse movement event arguments.</param>
    /// <returns><b>true</b> if the event is handled, <b>false</b> if not.</returns>
    public bool MouseMove(MouseArgs args)
    {
        int vertexIndex = GetIndex();

        _mousePos = args.ClientPosition;
        GetActiveHandle();

        if (vertexIndex == -1)
        {
            return false;
        }

        if (args.MouseButtons != MouseButtons.Left)
        {
            IsDragging = false;
            return false;
        }

        if ((args.MouseButtons == MouseButtons.Left) && (vertexIndex != -1) && (!IsDragging))
        {
            Vector2 delta = new(args.CameraSpacePosition.X - _localStartDrag.X, args.CameraSpacePosition.Y - _localStartDrag.Y);
            ref Vector2 vertexPosition = ref _vertices[vertexIndex];
            _localStartDrag = args.CameraSpacePosition;
            _dragHandlePos = new Vector2(vertexPosition.X + delta.X, vertexPosition.Y + delta.Y);

            SetupHandles();
            GetActiveHandle();

            IsDragging = true;
        }

        if (IsDragging)
        {
            DragHandles(args.CameraSpacePosition);
        }

        return true;
    }

    /// <summary>
    /// Function called when the mouse button is pressed.
    /// </summary>
    /// <param name="args">The arguments for the mouse down event.</param>
    public bool MouseDown(MouseArgs args)
    {
        int vertexIndex = GetIndex();

        _mousePos = args.ClientPosition;
        GetActiveHandle();

        if (args.MouseButtons != MouseButtons.Left)
        {
            return false;
        }

        if (vertexIndex == -1)
        {
            return false;
        }

        _localStartDrag = args.CameraSpacePosition;
        return true;
    }

    /// <summary>
    /// Function called when the mouse button is released.
    /// </summary>
    /// <param name="args">The mouse up event arguments.</param>
    /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
    public bool MouseUp(MouseArgs args)
    {
        int vertexIndex = GetIndex();

        _mousePos = args.ClientPosition;
        GetActiveHandle();

        if ((args.MouseButtons != MouseButtons.Left) || (vertexIndex == -1))
        {
            return false;
        }

        if (args.MouseButtons == MouseButtons.Left)
        {
            _localStartDrag = Vector2.Zero;
            IsDragging = false;
        }

        return true;
    }

    /// <summary>Function to render the editor UI.</summary>
    public void Render()
    {
        // End any drawing that we were doing prior to this.
        _renderer.End();

        GetActiveHandle();

        _renderer.Begin(Gorgon2DBatchState.InvertedBlend);

        _renderer.DrawLine(_screenVertices[0].X, _screenVertices[0].Y, _screenVertices[1].X - 1, _screenVertices[1].Y, GorgonColors.White, 1.25f);
        _renderer.DrawLine(_screenVertices[1].X - 1, _screenVertices[1].Y, _screenVertices[2].X - 1, _screenVertices[2].Y - 1, GorgonColors.White, 1.25f);
        _renderer.DrawLine(_screenVertices[2].X, _screenVertices[2].Y - 1, _screenVertices[3].X + 1, _screenVertices[3].Y - 1, GorgonColors.White, 1.25f);
        _renderer.DrawLine(_screenVertices[3].X, _screenVertices[3].Y, _screenVertices[0].X, _screenVertices[0].Y + 1, GorgonColors.White, 1.25f);

        _renderer.End();

        // Restart drawing using the original state.
        _renderer.Begin();

        int vertexIndex = GetIndex();

        if (vertexIndex != -1)
        {
            GorgonRectangleF handleBounds = _handles[vertexIndex].HandleBounds;

            if (handleBounds.IsEmpty)
            {
                return;
            }

            // Hilight our active handle.
            if (_selectedCorner == _activeCorner)
            {
                _renderer.DrawFilledRectangle(handleBounds, new GorgonColor(GorgonColors.Red, 0.7f));
            }

            _renderer.DrawRectangle(handleBounds, GorgonColors.Black);
            _renderer.DrawRectangle(new GorgonRectangleF(handleBounds.X + 1, handleBounds.Y + 1, handleBounds.Width - 2, handleBounds.Height - 2), GorgonColors.White);
        }
    }

    /// <summary>
    /// Function to refresh the UI of the editor.
    /// </summary>
    public void Refresh() => SetupHandles();

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => VerticesChangedEvent = null;

    /// <summary>Initializes a new instance of the <see cref="VertexEditService"/> class.</summary>
    /// <param name="renderer">The 2D renderer for the application.</param>
    public VertexEditService(Gorgon2D renderer)
    {
        _renderer = renderer;

        _handles[2].HandleCursor = _handles[0].HandleCursor =
        _handles[3].HandleCursor = _handles[1].HandleCursor = Cursors.Cross;
    }
}
