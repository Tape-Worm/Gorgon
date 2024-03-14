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

using System.ComponentModel;
using System.Numerics;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The renderer used when updating a sprites colors.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ColorEditViewer"/> class.</remarks>
/// <param name="renderer">The 2D renderer for the application.</param>
/// <param name="swapChain">The swap chain for the render area.</param>
/// <param name="dataContext">The sprite view model.</param>
internal class ColorEditViewer(Gorgon2D renderer, GorgonSwapChain swapChain, ISpriteContent dataContext)
        : SingleSpriteViewer(typeof(SpriteColorEdit).FullName, renderer, swapChain, dataContext)
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
                _colors[i] = Sprite.CornerColors[i] = DataContext.ColorEditor.SpriteColor[i];
            }
        }
    }

    /// <summary>
    /// Function to retrieve the currently active handle.
    /// </summary>
    /// <param name="clientMousePos">Client mouse position.</param>
    private void GetActiveHandle(Vector2 clientMousePos)
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

            if (handle.Contains(clientMousePos.X, clientMousePos.Y))
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

    /// <summary>Function to handle a preview key down event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a preview key down event in their own content view.</remarks>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
        base.OnPreviewKeyDown(args);

        switch (args.KeyCode)
        {
            case Keys.D1:
                if (args.Modifiers is not Keys.Control and not Keys.Shift)
                {
                    Array.Clear(_selected, 0, _selected.Length);
                }
                _selected[0] = !_selected[0];
                args.IsInputKey = true;
                break;
            case Keys.D2:
                if (args.Modifiers is not Keys.Control and not Keys.Shift)
                {
                    Array.Clear(_selected, 0, _selected.Length);
                }
                _selected[1] = !_selected[1];
                args.IsInputKey = true;
                break;
            case Keys.D3:
                if (args.Modifiers is not Keys.Control and not Keys.Shift)
                {
                    Array.Clear(_selected, 0, _selected.Length);
                }
                _selected[2] = !_selected[2];
                args.IsInputKey = true;
                break;
            case Keys.D4:
                if (args.Modifiers is not Keys.Control and not Keys.Shift)
                {
                    Array.Clear(_selected, 0, _selected.Length);
                }
                _selected[3] = !_selected[3];
                args.IsInputKey = true;
                break;
            case Keys.Back:
            case Keys.D0:
                Array.Clear(_selected, 0, _selected.Length);
                args.IsInputKey = true;
                break;
            case Keys.Space:
                bool hasChanges = false;

                for (int i = 0; i < _selected.Length; ++i)
                {
                    if (_selected[i])
                    {
                        _colors[i] = DataContext.ColorEditor.SelectedColor;
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    DataContext.ColorEditor.SpriteColor = _colors;
                }
                args.IsInputKey = true;
                break;
            case Keys.Enter when ((DataContext.ColorEditor.OkCommand is not null) && (DataContext.ColorEditor.OkCommand.CanExecute(null))):
                DataContext.ColorEditor.OkCommand.Execute(null);
                args.IsInputKey = true;
                break;
            case Keys.Escape when ((DataContext.ColorEditor.CancelCommand is not null) && (DataContext.ColorEditor.CancelCommand.CanExecute(null))):
                DataContext.ColorEditor.CancelCommand.Execute(null);
                args.IsInputKey = true;
                break;
        }
    }

    /// <summary>Function to handle a mouse up event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse up event in their own content view.</remarks>
    protected override void OnMouseUp(MouseArgs args)
    {
        base.OnMouseUp(args);

        GetActiveHandle(args.ClientPosition.ToVector2());

        for (int i = 0; i < _selected.Length; ++i)
        {
            if (((Control.ModifierKeys & Keys.Shift) != Keys.Shift) || (_activeHandleIndex == 4))
            {
                _selected[i] = _activeHandleIndex == 4;
            }
        }

        if (_activeHandleIndex is (-1) or 4)
        {
            DataContext.ColorEditor.SelectedVertices = _selected;
        }
        else
        {
            if (((Control.ModifierKeys & Keys.Shift) != Keys.Shift) || (args.MouseButtons == MouseButtons.Right))
            {
                _selected[_activeHandleIndex] = true;
            }
            else
            {
                _selected[_activeHandleIndex] = !_selected[_activeHandleIndex];
            }

            DataContext.ColorEditor.SelectedVertices = _selected;
        }

        if (args.MouseButtons != MouseButtons.Right)
        {
            return;
        }

        for (int i = 0; i < _selected.Length; ++i)
        {
            if (_selected[i])
            {
                _colors[i] = DataContext.ColorEditor.SelectedColor;
            }
        }

        DataContext.ColorEditor.SpriteColor = _colors;
    }

    /// <summary>Function to handle a mouse move event.</summary>
    /// <param name="args">The arguments for the event.</param>
    /// <remarks>Developers can override this method to handle a mouse move event in their own content view.</remarks>
    protected override void OnMouseMove(MouseArgs args)
    {
        base.OnMouseMove(args);
        GetActiveHandle(args.ClientPosition.ToVector2());
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

    /// <summary>
    /// Function to set up the handles for selection.
    /// </summary>
    private void SetHandles()
    {
        if (DataContext.Texture is null)
        {
            return;
        }

        var spriteTopLeft = new Vector3(SpriteRegion.Left, SpriteRegion.Top, 0);
        var spriteBottomRight = new Vector3(SpriteRegion.Right, SpriteRegion.Bottom, 0);
        Camera.Unproject(in spriteTopLeft, out Vector3 transformedTopLeft);
        Camera.Unproject(in spriteBottomRight, out Vector3 transformedBottomRight);

        var screenRect = new DX.RectangleF
        {
            Left = transformedTopLeft.X,
            Top = transformedTopLeft.Y,
            Right = transformedBottomRight.X,
            Bottom = transformedBottomRight.Y
        };

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

    /// <summary>Function called when the camera is panned.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is panned around the view.</remarks>
    protected override void OnCameraMoved()
    {
        base.OnCameraMoved();
        SetHandles();
    }

    /// <summary>Function called when the camera is zoomed.</summary>
    /// <remarks>Developers can override this method to implement a custom action when the camera is zoomed in or out.</remarks>
    protected override void OnCameraZoomed()
    {
        base.OnCameraZoomed();
        SetHandles();
    }

    /// <summary>Function called when the view has been resized.</summary>
    /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
    protected override void OnResizeEnd()
    {
        base.OnResizeEnd();
        SetHandles();
    }

    /// <summary>Function called to render the sprite data.</summary>
    protected override void DrawSprite()
    {
        base.DrawSprite();                       

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
    }

    /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
    /// <remarks>
    /// Developers should always override this method if they've overridden the <see cref="OnLoad"/> method. Failure to do so can cause memory leakage.
    /// </remarks>
    protected override void OnUnload()
    {
        DataContext.ColorEditor.PropertyChanged -= ColorEditor_PropertyChanged;
        base.OnUnload();
    }

    /// <summary>Function called to perform custom loading of resources.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        RenderRegion = SpriteRegion;

        for (int i = 0; i < _selected.Length; ++i)
        {
            _selected[i] = DataContext.ColorEditor.SelectedVertices[i];
        }

        for (int i = 0; i < _colors.Length; ++i)
        {
            _colors[i] = DataContext.VertexColors[i];
        }

        DataContext.ColorEditor.OriginalSpriteColor = _colors;
        DataContext.ColorEditor.SpriteColor = _colors;
        DataContext.ColorEditor.SelectedVertices = _selected;

        SetHandles();

        DataContext.ColorEditor.PropertyChanged += ColorEditor_PropertyChanged;
    }

    #endregion
}
