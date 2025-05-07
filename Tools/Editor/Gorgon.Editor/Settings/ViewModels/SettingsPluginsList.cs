
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
// Created: April 20, 2019 11:50:31 AM
// 

using System.Collections.ObjectModel;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The plugin list category for the settings
/// </summary>
internal class SettingsPluginsList
    : ViewModelBase<SettingsPluginsListParameters, IHostServices>, ISettingsPluginsList
{

    // The current plugin.
    private ISettingsPluginListItem _current;

    /// <summary>Property to return the plugins.</summary>
    public ObservableCollection<ISettingsPluginListItem> Plugins
    {
        get;
        private set;
    }

    /// <summary>Property to return the currently selected plugin.</summary>
    public ISettingsPluginListItem Current
    {
        get => _current;
        private set
        {
            if (_current == value)
            {
                return;
            }

            OnPropertyChanging();
            _current = value;
            OnPropertyChanged();
        }
    }

    public IEditorCommand<int> SelectPluginCommand
    {
        get;
    }

    /// <summary>Property to return the ID for the panel.</summary>
    public Guid ID => Guid.Empty;

    /// <summary>Property to return the name of this object.</summary>
    /// <remarks>For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this
    /// property.</remarks>
    public string Name => Resources.GOREDIT_SETTINGS_CATEGORY_pluginS;

    /// <summary>
    /// Function to select a plugin from the list.
    /// </summary>
    /// <param name="index">The index of the plugin.</param>
    private void DoSelectPlugin(int index)
    {
        try
        {
            if ((index < 0) || (index >= Plugins.Count))
            {
                Current = null;
                return;
            }

            Current = Plugins[index];
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_SELECTING_plugin);
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(SettingsPluginsListParameters injectionParameters) => Plugins = [.. injectionParameters.Plugins?.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>Initializes a new instance of the <see cref="SettingsPluginsList"/> class.</summary>
    public SettingsPluginsList() => SelectPluginCommand = new EditorCommand<int>(DoSelectPlugin);

}
