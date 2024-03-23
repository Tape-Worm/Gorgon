
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
// Created: September 4, 2021 10:15:00 AM
// 


using Gorgon.Editor.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// View model for the font character selection view
/// </summary>
internal class FontCharacterSelection
    : HostedPanelViewModelBase<FontCharacterSelectionParameters>, IFontCharacterSelection
{

    // The characters to use as glyphs.
    private IEnumerable<char> _characters = FontService.DefaultCharacters;
    // The font generation service.
    private FontService _fontService;
    // The current font.
    private Font _currentFont;



    /// <summary>Property to set or return the characters to use as the font glyphs.</summary>
    public IEnumerable<char> Characters
    {
        get => _characters;
        set
        {
            value ??= [];

            if ((_characters == value) || (_characters.SequenceEqual(value)))
            {
                return;
            }

            OnPropertyChanging();
            _characters = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the current font.
    /// </summary>
    public Font CurrentFont
    {
        get => _currentFont;
        private set
        {
            if (_currentFont == value)
            {
                return;
            }

            OnPropertyChanging();
            _currentFont = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => true;



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
    protected override void OnInitialize(FontCharacterSelectionParameters injectionParameters)
        => _fontService = injectionParameters.FontService;

    /// <summary>Function called when the associated view is loaded.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        CurrentFont ??= new Font(_fontService.WorkerFont.FontFamilyName, 16.0f, GraphicsUnit.Pixel);
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
    protected override void OnUnload()
    {
        _currentFont?.Dispose();

        base.OnUnload();
    }

}
