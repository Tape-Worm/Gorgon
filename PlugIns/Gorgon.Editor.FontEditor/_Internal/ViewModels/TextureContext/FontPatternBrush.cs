
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
// Created: September 6, 2021 12:54:13 PM
// 


using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The view model for the <see cref="FontPatternBrushView"/>
/// </summary>
internal class FontPatternBrush
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFontPatternBrush, IFontBrush
{

    /// <summary>
    /// The default pattern brush.
    /// </summary>
    public static readonly GorgonGlyphHatchBrush DefaultBrush = new()
    {
        HatchStyle = GlyphBrushHatchStyle.BackwardDiagonal,
        BackgroundColor = GorgonColor.BlackTransparent,
        ForegroundColor = GorgonColor.Black
    };

    // The brush to edit.
    private GorgonGlyphHatchBrush _brush = DefaultBrush;
    // The original colors for the brush.
    private (GorgonColor Foreground, GorgonColor Background) _originalColors;



    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => true;

    /// <summary>Property to set or return the original foreground and background colors.</summary>
    public (GorgonColor Foreground, GorgonColor Background) OriginalColor
    {
        get => _originalColors;
        set
        {
            if (value.Equals(_originalColors))
            {
                return;
            }

            OnPropertyChanging();
            _originalColors = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the current brush.</summary>
    public GorgonGlyphHatchBrush Brush
    {
        get => _brush;
        set
        {
            value ??= DefaultBrush;

            if ((_brush == value) || (_brush.Equals(value)))
            {
                return;
            }

            OnPropertyChanging();
            _brush = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the generic glyph brush.
    /// </summary>
    GorgonGlyphBrush IFontBrush.Brush => Brush;



    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
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
