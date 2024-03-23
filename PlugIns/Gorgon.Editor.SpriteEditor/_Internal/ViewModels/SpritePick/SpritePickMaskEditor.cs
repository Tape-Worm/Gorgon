
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
// Created: May 9, 2020 11:21:29 PM
// 

using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model used to manipulate the sprite picker mask clipping type and color
/// </summary>
internal class SpritePickMaskEditor
    : HostedPanelViewModelBase<SpritePickMaskEditorParameters>, ISpritePickMaskEditor
{

    // The settings for the plug in.
    private ISettings _pluginSettings;

    /// <summary>Property to set or return the type of masking to perform when picking a sprite using the sprite picker tool.</summary>
    public ClipMask ClipMaskType
    {
        get => _pluginSettings.ClipMaskType;
        set
        {
            if (_pluginSettings.ClipMaskType == value)
            {
                return;
            }

            OnPropertyChanging();
            _pluginSettings.ClipMaskType = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the sprite picker mask color.</summary>
    public GorgonColor ClipMaskValue
    {
        get => _pluginSettings.ClipMaskValue;
        set
        {
            if (_pluginSettings.ClipMaskValue == value)
            {
                return;
            }

            OnPropertyChanging();
            _pluginSettings.ClipMaskValue = value;
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
    protected override void OnInitialize(SpritePickMaskEditorParameters injectionParameters) => _pluginSettings = injectionParameters.PluginSettings;

}
