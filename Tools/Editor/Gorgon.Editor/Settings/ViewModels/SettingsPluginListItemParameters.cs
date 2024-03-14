#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 20, 2019 11:17:43 AM
// 
#endregion

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.IO.Providers;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The parameters for the <see cref="ISettingsPlugInListItem"/> view model.
/// </summary>
internal class SettingsPlugInListItemParameters
    : ViewModelInjection<IHostServices>
{
    #region Properties.
    /// <summary>
    /// Property to return the description/name of the plug in.
    /// </summary>
    public string Name
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the type of plug in.
    /// </summary>
    public PlugInType Type
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the current state of the plug in.
    /// </summary>
    public string State
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the reason that the plug in was disabled.
    /// </summary>
    public string DisabledReason
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the path to the plug in assembly.
    /// </summary>
    public string Path
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the description of the plug in.
    /// </summary>
    public string Description
    {
        get;
        private set;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="SettingsPlugInListItemParameters"/> class.</summary>
    /// <param name="plugin">The plugin to evaluate.</param>
    /// <param name="hostServices">The host application services.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> is parameter <b>null</b>.</exception>
    public SettingsPlugInListItemParameters(EditorPlugIn plugin, IHostServices hostServices)
        : base(hostServices)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        Name = string.IsNullOrWhiteSpace(plugin.Description) ? plugin.Name : plugin.Description;
        Type = plugin.PlugInType;
        State = Resources.GOREDIT_PLUGIN_STATE_LOADED;
        DisabledReason = string.Empty;
        Path = plugin.PlugInPath;
        Description = plugin.Description;
    }

    /// <summary>Initializes a new instance of the <see cref="SettingsPlugInListItemParameters"/> class.</summary>
    /// <param name="plugin">The plugin to evaluate.</param>
    /// <param name="hostServices">The host application services.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> is parameter <b>null</b>.</exception>
    public SettingsPlugInListItemParameters(IDisabledPlugIn plugin, IHostServices hostServices)
        : base(hostServices)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        Name = plugin.PlugInName;
        Type = PlugInType.Unknown;
        State = plugin.ReasonCode.ToString();
        DisabledReason = plugin.Description;
        Path = plugin.Path;
        Description = string.Empty;
    }

    /// <summary>Initializes a new instance of the <see cref="SettingsPlugInListItemParameters"/> class.</summary>
    /// <param name="plugin">The plugin to evaluate.</param>
    /// <param name="hostServices">The host application services.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> is parameter <b>null</b>.</exception>
    public SettingsPlugInListItemParameters(IGorgonFileSystemProvider plugin, IHostServices hostServices)
        : base(hostServices)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        Name = string.IsNullOrWhiteSpace(plugin.Description) ? plugin.Name : plugin.Description;
        Type = PlugInType.Reader;
        State = Resources.GOREDIT_PLUGIN_STATE_LOADED;
        DisabledReason = string.Empty;
        Path = plugin.ProviderPath;
        Description = plugin.Description;
    }
    #endregion
}
