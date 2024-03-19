
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
// Created: September 5, 2021 8:24:33 PM
// 


using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view model for the solid color brush
/// </summary>
internal class FontSolidBrush
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFontSolidBrush, IFontBrush
{

    /// <summary>
    /// The default brush.
    /// </summary>
    public static readonly GorgonGlyphSolidBrush DefaultBrush = new()
    {
        Color = GorgonColors.White
    };

    // The current brush for the font.
    private GorgonGlyphSolidBrush _brush = DefaultBrush;
    private GorgonColor _original = GorgonColors.White;



    /// <summary>
    /// Property to set or return the currently selected solid color brush.
    /// </summary>
    public GorgonGlyphSolidBrush Brush
    {
        get => _brush;
        set
        {
            value ??= DefaultBrush;

            if ((value == _brush) || (value.Color == _brush.Color))
            {
                return;
            }

            OnPropertyChanging();
            _brush = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the original color for the brush.
    /// </summary>
    public GorgonColor OriginalColor
    {
        get => _original;
        set
        {
            if (_original == value)
            {
                return;
            }

            OnPropertyChanging();
            _original = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return whether the panel is modal.
    /// </summary>
    public override bool IsModal => true;

    /// <summary>
    /// Property to return the generic glyph brush.
    /// </summary>
    GorgonGlyphBrush IFontBrush.Brush => Brush;



    /// <summary>
    /// Function to inject dependencies for the view model.
    /// </summary>
    protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
    {
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    protected override void OnUnload()
    {
        _brush = DefaultBrush;
        base.Unload();
    }

}
