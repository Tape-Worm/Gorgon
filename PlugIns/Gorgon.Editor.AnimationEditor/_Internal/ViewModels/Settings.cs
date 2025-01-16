
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

using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The view model used to manipulate the settings for the plug-in
/// </summary>
internal class Settings
    : SettingsCategoryBase<SettingsParameters>, ISettings
{
    // The plug-in settings.
    private AnimationEditorSettings _settings;

    /// <summary>
    /// Property to set or return whether a warning will be shown when an animation with unsupported tracks is loaded.
    /// </summary>
    public bool WarnUnsupportedTracks
    {
        get => _settings.WarnUnsupportedTracks;
        set
        {
            if (_settings.WarnUnsupportedTracks == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.WarnUnsupportedTracks = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the default screen resolution for the animation.
    /// </summary>
    public GorgonPoint DefaultResolution
    {
        get => _settings.DefaultResolution;
        set
        {
            if (_settings.DefaultResolution == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.DefaultResolution = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether to animate the background when no primary sprite is present.
    /// </summary>
    public bool AnimateNoPrimarySpriteBackground
    {
        get => _settings.AnimateBgNoPrimary;
        set
        {
            if (_settings.AnimateBgNoPrimary == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.AnimateBgNoPrimary = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether to use onion skinning for some editors.
    /// </summary>
    public bool UseOnionSkinning
    {
        get => _settings.UseOnionSkin;
        set
        {
            if (_settings.UseOnionSkin == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.UseOnionSkin = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the offset of the splitter between the main view and lower view.
    /// </summary>
    public int SplitterOffset
    {
        get => _settings.SplitOffset;
        set
        {
            if (_settings.SplitOffset == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.SplitOffset = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return whether to create a texture track in an empty animation on primary sprite assignment.
    /// </summary>
    public bool AddTextureTrackForPrimarySprite
    {
        get => _settings.AddTextureTrackForPrimarySprite;
        set
        {
            if (_settings.AddTextureTrackForPrimarySprite == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.AddTextureTrackForPrimarySprite = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the name of this object.</summary>
    public override string Name => Resources.GORANM_DESC;

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
    protected override void OnInitialize(SettingsParameters injectionParameters) => _settings = injectionParameters.Settings;

}
