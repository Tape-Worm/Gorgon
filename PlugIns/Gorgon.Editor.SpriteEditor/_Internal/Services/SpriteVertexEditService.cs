
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 8, 2019 9:27:07 PM
// 


using System.Numerics;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The service used to edit sprite vertices
/// </summary>
internal class SpriteVertexEditService
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
        new(),
        new()
    ];
    // The sprite boundaries, in texture space (but in pixels).
    private DX.RectangleF _spriteBounds;
    // The state used to draw inverted items.
    private readonly Gorgon2DBatchState _invertedState;
    // The vertex positions, in screen space.
    private readonly Vector2[] _screenVertices = new Vector2[4];
    // The currently active handle (the mouse is over it).
    private int _activeHandleIndex = -1;
    // Starting drag position.
    private Vector2 _localStartDrag;
    // Mouse position (in client space).
    private Vector2 _mousePos;
    // The original position of the handle being dragged.
    private Vector2 _dragHandlePos;
    // The icon used for keyboard manual input.
    private Lazy<GorgonTexture2DView> _keyboardIcon;
    // The currently selected vertex.
    private int _selectedVertexIndex = -1;
    // The camera used to render the UI.
    private GorgonOrthoCamera _camera;



    // Event triggered when the keyboard icon is clicked.
    private event EventHandler KeyboardIconClickedEvent;

    // Event triggered when the vertex coordinates have been altered.        
    private event EventHandler VerticesChangedEvent;

    // Event triggered when a vertex is selected or deselected.
    private event EventHandler VertexSelectedEvent;

    /// <summary>
    /// Event triggered when the vertex coordinates have been altered.        
    /// </summary>
    public event EventHandler VerticesChanged
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
    /// Event triggered when a vertex is selected or deselected.
    /// </summary>
    public event EventHandler VertexSelected
    {
        add
        {
            if (value is null)
            {
                VertexSelectedEvent = null;
                return;
            }

            VertexSelectedEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            VertexSelectedEvent -= value;
        }
    }

    /// <summary>
    /// Event triggered when the keyboard icon is clicked.
    /// </summary>
    public event EventHandler KeyboardIconClicked
    {
        add
        {
            if (value is null)
            {
                KeyboardIconClickedEvent = null;
                return;
            }

            KeyboardIconClickedEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            KeyboardIconClickedEvent -= value;
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

            EventHandler handler = VertexSelectedEvent;
            handler?.Invoke(this, EventArgs.Empty);

            SetupHandles();
        }
    }



    /// <summary>
    /// Function to raise the <see cref="VerticesChangedEvent"/> event.
    /// </summary>
    private void OnVerticesChanged()
    {
        EventHandler handler = VerticesChangedEvent;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Function to perform the dragging on the handles or the body of the selection.
    /// </summary>
    /// <param name="localMousePos">The position of the mouse, relative to the sprite.</param>
    private void DragHandles(Vector2 localMousePos)
    {
        if (SelectedVertexIndex == -1)
        {
            return;
        }

        Vector2 dragDelta = Vector2.Subtract(localMousePos, _localStartDrag);
        Vector2 newPos = new(_dragHandlePos.X + dragDelta.X, _dragHandlePos.Y + dragDelta.Y);
        _vertices[SelectedVertexIndex] = newPos.Truncate();
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

            if ((handleBounds.Contains(_mousePos.X, _mousePos.Y)) && (!IsDragging))
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
        // Convert to client space.
        if (Camera is not null)
        {
            Vector3 half = new(SpriteBounds.Width * 0.5f, SpriteBounds.Height * 0.5f, 0);

            for (int i = 0; i < _screenVertices.Length; ++i)
            {
                Vector3 unprojected = Camera.Unproject(new Vector3(_vertices[i].X, _vertices[i].Y, 0)) - half;
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

        DX.RectangleF aabb = new()
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
            Vector2 keyPos = new Vector2(_screenVertices[SelectedVertexIndex].X + 8, _screenVertices[SelectedVertexIndex].Y + 8).Truncate();

            _handles[4].HandleBounds = new DX.RectangleF(keyPos.X, keyPos.Y, _keyboardIcon.Value.Width, _keyboardIcon.Value.Height);
            _handles[4].Texture = _keyboardIcon.Value;
        }
        else
        {
            _handles[4].HandleBounds = DX.RectangleF.Empty;
        }

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

        if (SelectedVertexIndex != -1)
        {
            _dragHandlePos = _vertices[SelectedVertexIndex];
            localPos = _localStartDrag = _vertices[SelectedVertexIndex];
        }

        int newVertexIndex = SelectedVertexIndex;

        switch (args.KeyCode)
        {
            case Keys.OemMinus when (args.Modifiers == Keys.Shift):
            case Keys.Subtract:
                --newVertexIndex;
                if (newVertexIndex < 0)
                {
                    newVertexIndex = 3;
                }
                SelectedVertexIndex = newVertexIndex;
                return true;
            case Keys.Oemplus when (args.Modifiers == Keys.Shift):
            case Keys.Add:
                ++newVertexIndex;
                if (newVertexIndex > 3)
                {
                    newVertexIndex = 0;
                }
                SelectedVertexIndex = newVertexIndex;
                return true;
            case Keys.Back:
            case Keys.D0:
                SelectedVertexIndex = -1;
                _dragHandlePos = Vector2.Zero;
                _localStartDrag = Vector2.Zero;
                IsDragging = false;
                return true;
            case Keys.D1:
                SelectedVertexIndex = 0;
                _dragHandlePos = Vector2.Zero;
                _localStartDrag = Vector2.Zero;
                IsDragging = false;
                return true;
            case Keys.D2:
                SelectedVertexIndex = 1;
                _dragHandlePos = Vector2.Zero;
                _localStartDrag = Vector2.Zero;
                IsDragging = false;
                return true;
            case Keys.D3:
                SelectedVertexIndex = 2;
                _dragHandlePos = Vector2.Zero;
                _localStartDrag = Vector2.Zero;
                IsDragging = false;
                return true;
            case Keys.D4:
                SelectedVertexIndex = 3;
                _dragHandlePos = Vector2.Zero;
                _localStartDrag = Vector2.Zero;
                IsDragging = false;
                return true;
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

        if (SelectedVertexIndex == -1)
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
        _mousePos = args.ClientPosition;
        GetActiveHandle();

        if (SelectedVertexIndex == -1)
        {
            return false;
        }

        if (args.MouseButtons != MouseButtons.Left)
        {
            IsDragging = false;
            return true;
        }

        if ((args.MouseButtons == MouseButtons.Left) && (_activeHandleIndex != -1) && (_activeHandleIndex != 4) && (!IsDragging))
        {
            Vector2 delta = new(_localStartDrag.X - args.CameraSpacePosition.X, _localStartDrag.Y - args.CameraSpacePosition.Y);
            ref Vector2 vertexPosition = ref _vertices[SelectedVertexIndex];
            vertexPosition = new Vector2(vertexPosition.X + delta.X, vertexPosition.Y + delta.Y);
            _localStartDrag = args.CameraSpacePosition;
            _dragHandlePos = vertexPosition;

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
        _mousePos = args.ClientPosition;
        GetActiveHandle();

        if (_activeHandleIndex == 4)
        {
            return true;
        }

        if (_activeHandleIndex is (-1) or > 3)
        {
            SelectedVertexIndex = -1;
            return false;
        }

        _localStartDrag = args.CameraSpacePosition;
        SelectedVertexIndex = _activeHandleIndex;
        return true;
    }

    /// <summary>
    /// Function called when the mouse button is released.
    /// </summary>
    /// <param name="args">The mouse up event arguments.</param>
    /// <returns><b>true</b> if the mouse event was handled, <b>false</b> if it was not.</returns>
    public bool MouseUp(MouseArgs args)
    {
        _mousePos = args.ClientPosition;
        GetActiveHandle();

        if ((_activeHandleIndex == 4) && (!IsDragging))
        {
            EventHandler handler = KeyboardIconClickedEvent;
            handler?.Invoke(this, EventArgs.Empty);
            return true;
        }

        if (SelectedVertexIndex == -1)
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
        if (_spriteBounds.IsEmpty)
        {
            return;
        }

        // End any drawing that we were doing prior to this.
        _renderer.End();

        GetActiveHandle();

        _renderer.Begin(_invertedState);

        _renderer.DrawLine(_screenVertices[0].X, _screenVertices[0].Y, _screenVertices[1].X - 1, _screenVertices[1].Y, GorgonColors.White);
        _renderer.DrawLine(_screenVertices[1].X - 1, _screenVertices[1].Y, _screenVertices[2].X - 1, _screenVertices[2].Y - 1, GorgonColors.White);
        _renderer.DrawLine(_screenVertices[2].X, _screenVertices[2].Y - 1, _screenVertices[3].X + 1, _screenVertices[3].Y - 1, GorgonColors.White);
        _renderer.DrawLine(_screenVertices[3].X, _screenVertices[3].Y, _screenVertices[0].X, _screenVertices[0].Y + 1, GorgonColors.White);

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
                _renderer.DrawFilledRectangle(handleBounds, new GorgonColor(GorgonColors.Red, 0.7f));
            }

            if (texture is null)
            {
                _renderer.DrawRectangle(handleBounds, GorgonColors.Black);
                _renderer.DrawRectangle(new DX.RectangleF(handleBounds.X + 1, handleBounds.Y + 1, handleBounds.Width - 2, handleBounds.Height - 2), GorgonColors.White);
            }
            else
            {
                _renderer.DrawFilledRectangle(handleBounds, GorgonColors.White, texture, new DX.RectangleF(0, 0, 1, 1));
            }


            if (SelectedVertexIndex == i)
            {
                handleBounds.Inflate(4, 4);
                _renderer.DrawEllipse(handleBounds, GorgonColors.Black, thickness: 2);
                handleBounds.Inflate(-1, -1);
                _renderer.DrawEllipse(handleBounds, GorgonColors.White);
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
        KeyboardIconClickedEvent = null;
        VertexSelectedEvent = null;
        VerticesChangedEvent = null;

        Lazy<GorgonTexture2DView> keyIcon = Interlocked.Exchange(ref _keyboardIcon, null);

        if (keyIcon?.IsValueCreated ?? false)
        {
            keyIcon.Value.Dispose();
        }
    }



    /// <summary>Initializes a new instance of the <see cref="SpriteVertexEditService"/> class.</summary>
    /// <param name="renderer">The 2D renderer for the application.</param>
    public SpriteVertexEditService(Gorgon2D renderer)
    {
        _renderer = renderer;

        Gorgon2DBatchStateBuilder builder = new();
        _invertedState = builder.BlendState(GorgonBlendState.Inverted)
                                .Build();

        _handles[2].HandleCursor = _handles[0].HandleCursor =
        _handles[3].HandleCursor = _handles[1].HandleCursor = Cursors.Cross;
        _handles[4].HandleCursor = Cursors.Hand;

        _keyboardIcon = new Lazy<GorgonTexture2DView>(() => GorgonTexture2DView.CreateTexture(renderer.Graphics, new GorgonTexture2DInfo(CommonEditorResources.KeyboardIcon.Width,
                                                                                                                                              CommonEditorResources.KeyboardIcon.Height,
                                                                                                                                              CommonEditorResources.KeyboardIcon.Format)
        {
            Name = "VertexEditor_KeyboardIcon"
        }, CommonEditorResources.KeyboardIcon), true);
    }

}
