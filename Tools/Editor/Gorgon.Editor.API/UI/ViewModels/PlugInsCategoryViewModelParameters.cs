
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
// Created: March 27, 2020 10:53:09 AM
// 

using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Plugins;

namespace Gorgon.Editor.UI;

/// <summary>
/// Parameters for a <see cref="ISettingsCategory"/> specific to loading plugins for an editor plugin
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="PluginsCategoryViewModelParameters"/> class.</remarks>
/// <param name="openCodecDialog">The service used to locate plugin assemblies for loading.</param>
/// <param name="pluginCache">The cache for plugin assemblies.</param>
/// <param name="hostServices">Services passed down from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
public class PluginsCategoryViewModelParameters(IFileDialogService openCodecDialog, GorgonMefPluginCache pluginCache, IHostContentServices hostServices)
        : SettingsCategoryViewModelParameters(hostServices)
{
    /// <summary>
    /// Property to return the plugin cache.
    /// </summary>
    public GorgonMefPluginCache PluginCache
    {
        get;
    } = pluginCache ?? throw new ArgumentNullException(nameof(pluginCache));

    /// <summary>
    /// Property to return the service used to locate plugin assemblies for loading.
    /// </summary>
    public IFileDialogService OpenCodecDialog
    {
        get;
    } = openCodecDialog ?? throw new ArgumentNullException(nameof(openCodecDialog));

}
