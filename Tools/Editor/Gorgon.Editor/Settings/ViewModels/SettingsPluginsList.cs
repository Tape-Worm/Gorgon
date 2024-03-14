
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
// Created: April 20, 2019 11:50:31 AM
// 


using System.Collections.ObjectModel;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The plug in list category for the settings
/// </summary>
internal class SettingsPlugInsList
    : ViewModelBase<SettingsPlugInsListParameters, IHostServices>, ISettingsPlugInsList
{

    // The current plug in.
    private ISettingsPlugInListItem _current;



    /// <summary>Property to return the plug ins.</summary>
    public ObservableCollection<ISettingsPlugInListItem> PlugIns
    {
        get;
        private set;
    }

    /// <summary>Property to return the currently selected plug in.</summary>
    public ISettingsPlugInListItem Current
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

    public IEditorCommand<int> SelectPlugInCommand
    {
        get;
    }

    /// <summary>Property to return the ID for the panel.</summary>
    public Guid ID => Guid.Empty;

    /// <summary>Property to return the name of this object.</summary>
    /// <remarks>For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this
    /// property.</remarks>
    public string Name => Resources.GOREDIT_SETTINGS_CATEGORY_PLUGINS;



    /// <summary>
    /// Function to select a plug in from the list.
    /// </summary>
    /// <param name="index">The index of the plug in.</param>
    private void DoSelectPlugIn(int index)
    {
        try
        {
            if ((index < 0) || (index >= PlugIns.Count))
            {
                Current = null;
                return;
            }

            Current = PlugIns[index];
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_SELECTING_PLUGIN);
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(SettingsPlugInsListParameters injectionParameters) => PlugIns = new ObservableCollection<ISettingsPlugInListItem>(injectionParameters.PlugIns?.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase));



    /// <summary>Initializes a new instance of the <see cref="SettingsPlugInsList"/> class.</summary>
    public SettingsPlugInsList() => SelectPlugInCommand = new EditorCommand<int>(DoSelectPlugIn);

}
