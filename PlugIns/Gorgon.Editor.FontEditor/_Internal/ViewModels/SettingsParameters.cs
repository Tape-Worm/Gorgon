
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
// Created: August 5, 2020 8:35:30 PM
// 


using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// The parameters to pass to the <see cref="ISettings"/> view model
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SettingsParameters"/> class.</remarks>
/// <param name="settings">The plug in settings.</param>
/// <param name="hostServices">Common application services.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="settings" /> parameter is <strong>null</strong>.</exception>
internal class SettingsParameters(FontEditorSettings settings, IHostContentServices hostServices)
        : SettingsCategoryViewModelParameters(hostServices)
{

    /// <summary>
    /// Property to return the settings for the plug in.
    /// </summary>
    public FontEditorSettings Settings
    {
        get;
    } = settings ?? throw new ArgumentNullException(nameof(settings));


}
