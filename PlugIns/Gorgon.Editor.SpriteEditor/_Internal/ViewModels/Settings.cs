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
// Created: March 25, 2019 9:58:48 AM
// 

using Gorgon.Core;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model used to manipulate the settings for the plug in
/// </summary>
internal class Settings
    : ViewModelBase<SettingsParameters, IHostContentServices>, ISettings
{
    // The plug in settings.
    private SpriteEditorSettings _settings;

    /// <summary>
    /// Property to set or return the type of masking to perform when picking a sprite using the <see cref="SpriteEditTool.SpritePick"/> tool.
    /// </summary>
    public ClipMask ClipMaskType
    {
        get => _settings.ClipMaskType;
        set
        {
            if (_settings.ClipMaskType == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.ClipMaskType = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the sprite picker mask color.
    /// </summary>
    public GorgonColor ClipMaskValue
    {
        get => GorgonColor.FromRGBA(_settings.ClipMaskValue);
        set
        {
            int intValue = GorgonColor.ToRGBA(value);

            if (intValue == _settings.ClipMaskValue)
            {
                return;
            }

            OnPropertyChanging();
            _settings.ClipMaskValue = intValue;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the position of the manual input window.
    /// </summary>
    public GorgonRectangle? ManualRectangleEditorBounds
    {
        get => _settings.ManualRectangleEditorBounds;
        set
        {
            if (_settings.ManualRectangleEditorBounds == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.ManualRectangleEditorBounds = value;
            OnPropertyChanged();
        }
    }

    /// <summary>property to set or return the position of the manual vertex editor window.</summary>
    public GorgonRectangle? ManualVertexEditorBounds
    {
        get => _settings.ManualVertexEditorBounds;
        set
        {
            if (_settings.ManualVertexEditorBounds == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.ManualVertexEditorBounds = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the flag used to show a warning when a large image is being used with the sprite picker tool.
    /// </summary>
    public bool ShowImageSizeWarning
    {
        get => _settings.ShowImageSizeWarning;
        set
        {
            if (_settings.ShowImageSizeWarning == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.ShowImageSizeWarning = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <exception cref="ArgumentMissingException">Thrown when required parameters are missing.</exception>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(SettingsParameters injectionParameters) => _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(SettingsParameters.Settings), nameof(injectionParameters));
}
