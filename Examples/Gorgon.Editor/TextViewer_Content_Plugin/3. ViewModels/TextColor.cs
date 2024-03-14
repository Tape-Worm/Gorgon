
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
// Created: April 4, 2019 8:59:29 PM
// 


using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Examples;

/// <summary>
/// The view model for the text color editor
/// </summary>
internal class TextColor
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, ITextColor
{

    // The original color for the sprite.
    private GorgonColor _originalColor = GorgonColor.Black;
    // The current color for the sprite.
    private GorgonColor _color = GorgonColor.Black;



    /// <summary>Property to set or return the currently selected color.</summary>
    public GorgonColor SelectedColor
    {
        get => _color;
        set
        {
            if (_color.Equals(in value))
            {
                return;
            }

            OnPropertyChanging();
            _color = value;
            OnPropertyChanged();
        }
    }

    public GorgonColor OriginalColor
    {
        get => _originalColor;
        set
        {
            if (_originalColor.Equals(in value))
            {
                return;
            }

            OnPropertyChanging();
            _originalColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => true;



    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
    {
        // Nothing to inject.
    }

}
