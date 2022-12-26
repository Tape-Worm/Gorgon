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
// Created: March 6, 2020 1:38:12 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The settings view model for the edge detect effect.
/// </summary>
internal class FxEdgeDetect
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFxEdgeDetect
{
    #region Variables.
    // The threshold for the edge detection.
    private int _threshold = 50;
    // The offset for the edge line width.
    private float _offset = 1.0f;
    // The color of the edge line.
    private GorgonColor _lineColor = GorgonColor.Black;
    // Flag to indicate that the edges should be overlaid on top of the original image or not.
    private bool _overlay = true;
    #endregion

    #region Properties.
    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => false;

    /// <summary>Property to set or return the threshold for detecting edges (as a percentage).</summary>
    public int Threshold
    {
        get => _threshold;
        set
        {
            if (_threshold == value)
            {
                return;
            }

            OnPropertyChanging();
            _threshold = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the amount to offset the edge line widths.</summary>
    public float Offset
    {
        get => _offset;
        set
        {
            if (_offset.EqualsEpsilon(value))
            {
                return;
            }

            OnPropertyChanging();
            _offset = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the color of the edge lines.</summary>
    public GorgonColor LineColor
    {
        get => _lineColor;
        set
        {
            if (GorgonColor.Equals(in _lineColor, in value))
            {
                return;
            }

            OnPropertyChanging();
            _lineColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether the edges should be overlaid on top of the original image or not.
    /// </summary>
    public bool Overlay
    {
        get => _overlay;
        set
        {
            if (_overlay == value)
            {
                return;
            }

            OnPropertyChanging();
            _overlay = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Methods.
    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    ///   <para>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </para>
    ///   <para>
    /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
    /// </para>
    /// </remarks>
    protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
    {
    }
    #endregion
}
