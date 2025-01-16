
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
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
// Created: September 3, 2021 5:42:42 PM
// 

using Gorgon.Editor.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// View model for the glyph padding interface
/// </summary>
internal class FontPadding
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFontPadding
{

    // The padding around each glyph on the texture.
    private int _padding = 1;

    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => true;

    /// <summary>Property to set or return the padding around each glyph, in pixels.</summary>
    public int Padding
    {
        get => _padding;
        set
        {
            if (_padding == value)
            {
                return;
            }

            OnPropertyChanging();
            _padding = value;
            OnPropertyChanged();
        }
    }

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
