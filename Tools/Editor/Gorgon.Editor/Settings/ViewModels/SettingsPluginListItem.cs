
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
// Created: April 20, 2019 10:58:12 AM
// 

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// An item to display on the <see cref="ISettingsPlugInsList"/> view model
/// </summary>
internal class SettingsPlugInListItem
    : ViewModelBase<SettingsPlugInListItemParameters, IHostServices>, ISettingsPlugInListItem
{

    // The name of the plug-in.
    private string _name;
    // The plug-in type.
    private PlugInType _type;
    // The current state.
    private string _state;
    // The reason why a plug-in is disabled.
    private string _disableReason;
    // The path to the plug-in assembly.
    private string _path;

    /// <summary>Property to return the name/description for the plug-in.</summary>
    public string Name
    {
        get => _name;
        private set
        {
            if (string.Equals(_name, value, StringComparison.CurrentCulture))
            {
                return;
            }

            OnPropertyChanging();
            _name = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the path to the plug-in.
    /// </summary>
    public string Path
    {
        get => _path;
        private set
        {
            if (string.Equals(_path, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            OnPropertyChanging();
            _path = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the type for the plug-in.</summary>
    public PlugInType Type
    {
        get => _type;
        set
        {
            if (_type == value)
            {
                return;
            }

            OnPropertyChanging();
            _type = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the current state of the plug-in.</summary>
    public string State
    {
        get => _state;
        set
        {
            if (string.Equals(_state, value, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            OnPropertyChanging();
            _state = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return why the plug-in was disabled.</summary>
    public string DisabledReason
    {
        get => _disableReason;
        set
        {
            if (string.Equals(_disableReason, value, StringComparison.CurrentCulture))
            {
                return;
            }

            OnPropertyChanging();
            _disableReason = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <exception cref="ArgumentMissingException">Name - injectionParameters</exception>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(SettingsPlugInListItemParameters injectionParameters)
    {
        Name = injectionParameters.Name;
        Type = injectionParameters.Type;
        State = injectionParameters.State ?? string.Empty;
        DisabledReason = string.IsNullOrWhiteSpace(injectionParameters.DisabledReason) ? $"{injectionParameters.Description}\r\n\r\n{Resources.GOREDIT_TEXT_PLUGIN_LOADED_SUCCESSFULLY}"
                                                                                        : injectionParameters.DisabledReason;
        Path = injectionParameters.Path;
    }
}
