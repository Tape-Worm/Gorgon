
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
// Created: March 26, 2020 9:05:50 PM
// 


using Gorgon.Editor.UI;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The settings view model for the one bit effect
/// </summary>
internal class FxOneBit
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFxOneBit
{

    // The min and max threshold for white pixels.
    private int _minThreshold = 127;
    private int _maxThreshold = 255;
    // The flag used to invert the black/white values.
    private bool _invert;



    /// <summary>
    /// Property to set or return the maximum threshold to convert to white (or black if inverted).
    /// </summary>
    public int MaxWhiteThreshold
    {
        get => _maxThreshold;
        set
        {
            if (_maxThreshold == value)
            {
                return;
            }

            OnPropertyChanging();
            _maxThreshold = value.Min(255).Max(0);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the minimum threshold to convert to white (or black if inverted).
    /// </summary>
    public int MinWhiteThreshold
    {
        get => _minThreshold;
        set
        {
            if (_minThreshold == value)
            {
                return;
            }

            OnPropertyChanging();
            _minThreshold = value.Min(255).Max(0);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the flag used to invert the black/white values.
    /// </summary>
    public bool Invert
    {
        get => _invert;
        set
        {
            if (_invert == value)
            {
                return;
            }

            OnPropertyChanging();
            _invert = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => false;



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

}
