
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
// Created: March 25, 2019 9:47:13 AM
// 

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.PlugIns;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The parameters to pass to the <see cref="Settings"/> view model
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ImportSettingsParameters"/> class.</remarks>
/// <param name="settings">The plug-in settings.</param>
/// <param name="codecs">The codec registry.</param>
/// <param name="openCodecDialog">The service used to locate plug-in assemblies for loading.</param>
/// <param name="plugInCache">The cache for plug-in assemblies.</param>
/// <param name="hostServices">Common application services.</param>
/// <exception cref="ArgumentNullException">Thrown when any parameter is <strong>null</strong>.</exception>
internal class ImportSettingsParameters(AnimationImportSettings settings, CodecRegistry codecs, IFileDialogService openCodecDialog, GorgonMefPlugInCache plugInCache, IHostContentServices hostServices)
        : PlugInsCategoryViewModelParameters(openCodecDialog, plugInCache, hostServices)
{
    /// <summary>
    /// Property to return the settings for the plug-in.
    /// </summary>
    public AnimationImportSettings Settings
    {
        get;
    } = settings ?? throw new ArgumentNullException(nameof(settings));

    /// <summary>
    /// Property to return the codec registry.
    /// </summary>
    public CodecRegistry Codecs
    {
        get;
    } = codecs ?? throw new ArgumentNullException(nameof(settings));

}
