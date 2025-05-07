
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
// Created: March 28, 2020 7:50:38 PM
// 

using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.UI;

/// <summary>
/// A common view model for a plugins category
/// </summary>
/// <typeparam name="T">The type of parameters for the view model.</typeparam>
public abstract class PluginsCategory<T>
    : SettingsCategoryBase<T>
    where T : PluginsCategoryViewModelParameters
{
    /// <summary>
    /// Property to return the dialog used to open plugin assemblies.
    /// </summary>
    protected IFileDialogService OpenCodecDialog
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the file name that will hold the plugins.
    /// </summary>
    protected abstract string SettingsFileName
    {
        get;
    }

    /// <summary>
    /// Property to return the command for writing setting data.
    /// </summary>
    public IEditorCommand<object> WriteSettingsCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command for loading a plugin assembly.
    /// </summary>
    public IEditorCommand<object> LoadPluginAssemblyCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to unloading a plugin assembly.
    /// </summary>
    public IEditorCommand<object> UnloadPluginAssembliesCommand
    {
        get;
    }

    /// <summary>
    /// Function to write out the settings.
    /// </summary>
    private void DoWriteSettings()
    {
        try
        {
            object settingsData = OnGetSettings();

            if (settingsData is null)
            {
                return;
            }

            HostServices.ContentPluginService.WriteContentSettings(SettingsFileName, settingsData);
        }
        catch (Exception ex)
        {
            // We don't care if it crashes. The worst thing that'll happen is your settings won't persist.
            HostServices.Log.PrintException(ex);
        }
    }

    /// <summary>
    /// Function to unload the selected plugin assemblies.
    /// </summary>
    private void DoUnloadPluginAssemblies()
    {
        try
        {
            if (!OnUnloadPlugins())
            {
                return;
            }

            DoWriteSettings();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_UNABLE_TO_UNLOAD_pluginS);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to load in a plugin assembly.
    /// </summary>
    private void DoLoadPluginAssembly()
    {
        try
        {
            if (!OnLoadPlugins())
            {
                return;
            }

            // Store the settings now.
            DoWriteSettings();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_UNABLE_TO_LOAD_pluginS);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to retrieve the underlying object used to hold the settings.
    /// </summary>
    /// <returns>The object that holds the settings.</returns>
    protected abstract object OnGetSettings();

    /// <summary>
    /// Function to determine if the selected plugin assemblies can be unloaded.
    /// </summary>
    /// <returns><b>true</b> if the plugin assemblies can be removed, <b>false</b> if not.</returns>
    protected abstract bool CanUnloadPluginAssemblies();

    /// <summary>
    /// Function to unload previously loaded plugins.
    /// </summary>
    /// <returns><b>true</b> to indicate that the operation succeeded, or <b>false</b> if it was cancelled.</returns>
    protected abstract bool OnUnloadPlugins();

    /// <summary>
    /// Function to load Plugins from selected assemblies.
    /// </summary>
    /// <returns><b>true</b> to indicate that the operation succeeded, or <b>false</b> if it was cancelled.</returns>
    protected abstract bool OnLoadPlugins();

    /// <summary>Function to inject dependencies for the view model.</summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>
    /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
    /// </remarks>
    protected override void OnInitialize(T injectionParameters) => OpenCodecDialog = injectionParameters.OpenCodecDialog;

    /// <summary>Initializes a new instance of the <see cref="PluginsCategory{T}"/> class.</summary>
    protected PluginsCategory()
    {
        WriteSettingsCommand = new EditorCommand<object>(DoWriteSettings);
        LoadPluginAssemblyCommand = new EditorCommand<object>(DoLoadPluginAssembly);
        UnloadPluginAssembliesCommand = new EditorCommand<object>(DoUnloadPluginAssemblies, CanUnloadPluginAssemblies);
    }
}
