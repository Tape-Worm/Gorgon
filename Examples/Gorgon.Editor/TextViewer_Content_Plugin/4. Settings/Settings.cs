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
// Created: August 5, 2020 8:33:33 PM
// 
#endregion

using Gorgon.Editor.UI;

namespace Gorgon.Examples;

/// <summary>
/// The view model for the settings for the text content editor.
/// </summary>
internal class Settings
    : SettingsCategoryBase<SettingsParameters>, ISettings
{
    #region Variables.
    // The settings data for the plug in.
    private TextContentSettings _settings;
    #endregion

    #region Properties.
    /// <summary>Property to set or return the default font face.</summary>
    public FontFace DefaultFont
    {
        get => _settings.DefaultFont;
        set
        {
            if (_settings.DefaultFont == value)
            {
                return;
            }

            OnPropertyChanging();
            _settings.DefaultFont = value;
            OnPropertyChanged();
        }
    }        

    /// <summary>Property to return the name of this object.</summary>
    public override string Name => "Example Plug in - Text Content";
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
    protected override void OnInitialize(SettingsParameters injectionParameters) => _settings = injectionParameters.Settings;
    #endregion
}
