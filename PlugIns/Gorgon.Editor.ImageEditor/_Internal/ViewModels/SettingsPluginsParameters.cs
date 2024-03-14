
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
// Created: April 20, 2019 5:20:34 PM
// 


using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.PlugIns;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Parameters to pass to the <see cref="ISettingsPlugins"/> view model
/// </summary>
/// <remarks>Initializes a new instance of the ImageContentVmParameters class.</remarks>
/// <param name="settings">The settings for the image editor.</param>
/// <param name="codecs">The codecs loaded into the system.</param>
/// <param name="openCodecDialog">The file dialog used to locate codec assemblies.</param>
/// <param name="plugInCache">The cache for plug in assemblies.</param>
/// <param name="hostServices">Common application services.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class SettingsPluginsParameters(ImageEditorSettings settings, ICodecRegistry codecs, IFileDialogService openCodecDialog, GorgonMefPlugInCache plugInCache, IHostContentServices hostServices)
        : PlugInsCategoryViewModelParameters(openCodecDialog, plugInCache, hostServices)
{

    /// <summary>
    /// Property to return the settings for the image editor plugin.
    /// </summary>
    public ImageEditorSettings Settings
    {
        get;
    } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Property to return the codecs loaded into the system.
    /// </summary>
    public ICodecRegistry Codecs
    {
        get;
    } = codecs ?? throw new ArgumentNullException(nameof(codecs));


}
