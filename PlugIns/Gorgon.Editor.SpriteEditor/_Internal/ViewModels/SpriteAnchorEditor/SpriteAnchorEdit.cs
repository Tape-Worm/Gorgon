
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
// Created: May 19, 2020 12:51:42 PM
// 


using System.Numerics;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model for the sprite anchor editor
/// </summary>
internal class SpriteAnchorEdit
    : HostedPanelViewModelBase<SpriteAnchorEditParameters>, ISpriteAnchorEdit
{

    // The anchor point.
    private Vector2 _anchor;
    // The boundaries for the anchor.
    private GorgonRectangle _bounds;
    // The boundaries of the sprite vertices.
    private readonly Vector2[] _spriteBounds = new Vector2[4];
    // The mid point of the sprite.
    private Vector2 _midPoint;
    // Flag to indicate that preview scaling is active.
    private bool _previewScaling;
    // Flag to indicate that preview rotation is active.
    private bool _previewRotation;



    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => true;

    /// <summary>
    /// Property to set or return whether to preview rotation with the current anchor setting.
    /// </summary>
    public bool PreviewRotation
    {
        get => _previewRotation;
        set
        {
            if (_previewRotation == value)
            {
                return;
            }

            OnPropertyChanging();
            _previewRotation = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether to preview scaling with the current anchor setting.
    /// </summary>
    public bool PreviewScale
    {
        get => _previewScaling;
        set
        {
            if (_previewScaling == value)
            {
                return;
            }

            OnPropertyChanging();
            _previewScaling = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the boundaries for the anchor point.</summary>
    public GorgonRectangle Bounds
    {
        get => _bounds;
        set
        {
            if (_bounds.Equals(value))
            {
                return;
            }

            OnPropertyChanging();
            _bounds = value;
            OnPropertyChanged();

            Anchor = new Vector2(_anchor.X.Min(_bounds.Right).Max(_bounds.Left), _anchor.Y.Min(_bounds.Bottom).Max(_bounds.Top));
        }
    }

    /// <summary>Property to set or return the anchor point.</summary>
    public Vector2 Anchor
    {
        get => _anchor;
        set
        {
            if (_anchor.Equals(value))
            {
                return;
            }

            OnPropertyChanging();
            _anchor = new Vector2(value.X.Min(_bounds.Right).Max(_bounds.Left), value.Y.Min(_bounds.Bottom).Max(_bounds.Top));
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the mid point of the sprite, based on its vertices.
    /// </summary>
    public Vector2 MidPoint
    {
        get => _midPoint;
        private set
        {
            if (_midPoint == value)
            {
                return;
            }

            OnPropertyChanging();
            _midPoint = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the boundaries of the sprite (vertices).</summary>
    public IReadOnlyList<Vector2> SpriteBounds
    {
        get => _spriteBounds;
        set
        {
            if (value is null)
            {
                OnPropertyChanging();
                Array.Clear(_spriteBounds, 0, _spriteBounds.Length);
                OnPropertyChanged();
                return;
            }

            OnPropertyChanging();
            GorgonRectangleF range = new()
            {
                Left = float.MaxValue,
                Top = float.MaxValue,
                Right = float.MinValue,
                Bottom = float.MinValue
            };
            for (int i = 0; i < _spriteBounds.Length.Min(value.Count); ++i)
            {
                range.Left = value[i].X.Min(range.Left);
                range.Top = value[i].Y.Min(range.Top);
                range.Right = value[i].X.Max(range.Right);
                range.Bottom = value[i].Y.Max(range.Bottom);
                _spriteBounds[i] = value[i].Truncate();
            }
            MidPoint = new Vector2(range.Left + range.Width * 0.5f, range.Top + range.Height * 0.5f).Truncate();
            OnPropertyChanged();
        }
    }



    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    protected override void OnInitialize(SpriteAnchorEditParameters injectionParameters) => _bounds = injectionParameters.Bounds;

}
