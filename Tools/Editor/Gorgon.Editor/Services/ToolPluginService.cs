﻿
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: October 29, 2018 1:19:30 PM
// 

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.PlugIns;

namespace Gorgon.Editor.Services;

/// <summary>
/// The service used for managing the tool plugins
/// </summary>
/// <remarks>Initializes a new instance of the ToolPlugInService class.</remarks>
/// <param name="settingsDirectory">The directory that will contain settings for the content plug-ins.</param>
/// <param name="hostServices">The host appplication services to pass to the plug-ins.</param>
internal class ToolPlugInService(string settingsDirectory, IHostContentServices hostServices)
        : IToolPlugInService, IDisposable
{

    // The plugin list.
    private readonly Dictionary<string, ToolPlugIn> _plugins = new(StringComparer.OrdinalIgnoreCase);
    // The list of disabled tool plug-ins.
    private readonly Dictionary<string, IDisabledPlugIn> _disabled = new(StringComparer.OrdinalIgnoreCase);
    // The list of ribbon buttons for all tools.
    private readonly Dictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> _ribbonButtons = new(StringComparer.CurrentCultureIgnoreCase);
    // The directory that contains the settings for the plug-ins.
    private readonly string _settingsDir = settingsDirectory;
    // The host application services to pass to the plug-ins.
    private readonly IHostContentServices _hostServices = hostServices;

    /// <summary>Property to return the list of tool plugins loaded in to the application.</summary>
    /// <value>The plugins.</value>
    public IReadOnlyDictionary<string, ToolPlugIn> PlugIns => _plugins;

    /// <summary>Property to return the list of disabled plug-ins.</summary>
    public IReadOnlyDictionary<string, IDisabledPlugIn> DisabledPlugIns => _disabled;

    /// <summary>
    /// Property to return the UI buttons for the tool plug-in.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<IToolPlugInRibbonButton>> RibbonButtons => _ribbonButtons;

    /// <summary>
    /// Function to return the file for the content plug-in settings.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <returns>The file containing the plug-in settings.</returns>
    private FileInfo GetContentPlugInSettingsPath(string name) =>
#if DEBUG
        new(Path.Combine(_settingsDir, name.FormatFileName()) + ".DEBUG.json");
#else
        new(Path.Combine(_settingsDir, Path.ChangeExtension(name.FormatFileName(), "json")));
#endif
    /// <summary>
    /// Function to clear the UI buttons for the plug-ins.
    /// </summary>
    private void ClearToolButtons()
    {
        foreach (KeyValuePair<string, IReadOnlyList<IToolPlugInRibbonButton>> buttonGroup in _ribbonButtons)
        {
            foreach (IDisposable button in buttonGroup.Value.OfType<IDisposable>())
            {
                button.Dispose();
            }
        }

        _ribbonButtons.Clear();
    }

    /// <summary>
    /// Function to rebuild the list of sorted ribbon buttons.
    /// </summary>
    private void GetToolButtons()
    {
        ClearToolButtons();

        foreach (KeyValuePair<string, ToolPlugIn> plugin in PlugIns)
        {
            IToolPlugInRibbonButton button = plugin.Value.GetToolButton();
            button.ValidateButton();

            List<IToolPlugInRibbonButton> buttons;
            if (_ribbonButtons.TryGetValue(button.GroupName, out IReadOnlyList<IToolPlugInRibbonButton> roButtons))
            {
                // This is safe because this is the implementation.
                buttons = (List<IToolPlugInRibbonButton>)roButtons;
            }
            else
            {
                _ribbonButtons[button.GroupName] = buttons = [];
            }

            buttons.Add(button);
        }
    }

    /// <summary>Function to add a tool plugin to the service.</summary>
    /// <param name="plugin">The plugin to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
    public void AddToolPlugIn(ToolPlugIn plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        if (_plugins.ContainsKey(plugin.Name))
        {
            return;
        }

        _plugins[plugin.Name] = plugin;
    }

    /// <summary>Function to clear all of the tool plugins.</summary>
    public void Clear()
    {
        foreach (KeyValuePair<string, ToolPlugIn> plugin in _plugins)
        {
            plugin.Value.Shutdown();
        }

        _plugins.Clear();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => Clear();

    /// <summary>
    /// Function to load all of the tool plug-ins into the service.
    /// </summary>
    /// <param name="pluginCache">The plug-in assembly cache.</param>
    /// <param name="pluginDir">The directory that contains the plug-ins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/>, or the <paramref name="pluginDir"/> parameter is <b>null</b>.</exception>
    public void LoadToolPlugIns(GorgonMefPlugInCache pluginCache, string pluginDir)
    {
        if (pluginCache is null)
        {
            throw new ArgumentNullException(nameof(pluginCache));
        }

        if (pluginDir is null)
        {
            throw new ArgumentNullException(nameof(pluginDir));
        }

        IReadOnlyList<PlugInAssemblyState> assemblies = pluginCache.ValidateAndLoadAssemblies(Directory.EnumerateFiles(pluginDir, "*.dll", SearchOption.TopDirectoryOnly), Program.Log);

        if (assemblies.Count > 0)
        {
            foreach (PlugInAssemblyState record in assemblies.Where(item => !item.IsAssemblyLoaded && item.IsManaged))
            {
                _disabled[Path.GetFileName(record.Path)] = new DisabledPlugIn(DisabledReasonCode.Error, Path.GetFileName(record.Path), record.LoadFailureReason, record.Path);
            }
        }

        IGorgonPlugInService plugins = new GorgonMefPlugInService(pluginCache);
        IReadOnlyList<ToolPlugIn> pluginList = plugins.GetPlugIns<ToolPlugIn>();

        foreach (ToolPlugIn plugin in pluginList)
        {
            try
            {
                Program.Log.Print($"Creating tool plug-in '{plugin.Name}'...", LoggingLevel.Simple);
                plugin.Initialize(_hostServices);

                // Check to see if this plug-in can continue.
                IReadOnlyList<string> validation = plugin.IsPlugInAvailable();

                if (validation.Count > 0)
                {
                    // Shut the plug-in down.
                    plugin.Shutdown();

                    Program.Log.PrintWarning($"The tool plug-in '{plugin.Name}' is disabled:", LoggingLevel.Simple);
                    foreach (string reason in validation)
                    {
                        Program.Log.PrintWarning($"{reason}", LoggingLevel.Verbose);
                    }

                    _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.ValidationError, plugin.Name, string.Join("\r\n", validation), plugin.PlugInPath);

                    // Remove this plug-in.
                    plugins.Unload(plugin.Name);
                    continue;
                }

                AddToolPlugIn(plugin);
            }
            catch (Exception ex)
            {
                // Attempt to gracefully shut the plug-in down if we error out.
                plugin.Shutdown();

                Program.Log.PrintError($"Cannot create tool plug-in '{plugin.Name}'.", LoggingLevel.Simple);
                Program.Log.LogException(ex);

                _disabled[plugin.Name] = new DisabledPlugIn(DisabledReasonCode.Error, plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_PLUGIN_EXCEPTION, ex.Message), plugin.PlugInPath);
            }
        }
    }

    /// <summary>Function to remove a tool plugin from the service.</summary>
    /// <param name="plugin">The plugin to remove.</param>
    public void RemoveToolPlugIn(ToolPlugIn plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        if (!_plugins.ContainsKey(plugin.Name))
        {
            return;
        }

        _plugins.Remove(plugin.Name);
        plugin.Shutdown();
    }

    /// <summary>Funcion to read the settings for a content plug-in from a JSON file.</summary>
    /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="converters">A list of JSON data converters.</param>
    /// <returns>The settings object for the plug-in, or <b>null</b> if no settings file was found for the plug-in.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="plugin" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>This will read in the settings for a content plug from the same location where the editor stores its application settings file.</remarks>
    public T ReadContentSettings<T>(string name, params JsonConverter[] converters)
        where T : class
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        FileInfo settingsFile = GetContentPlugInSettingsPath(name);

        if (!settingsFile.Exists)
        {
            return null;
        }

        using Stream stream = settingsFile.OpenRead();
        using StreamReader reader = new(stream, Encoding.UTF8);

        JsonSerializerOptions options = new();

        if (converters is not null)
        {
            foreach (JsonConverter converter in converters)
            {
                options.Converters.Add(converter);
            }
        }

        return JsonSerializer.Deserialize<T>(reader.ReadToEnd(), options);
    }

    /// <summary>Function to write out the settings for a content plug-in as a JSON file.</summary>
    /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="contentSettings">The content settings to persist as JSON file.</param>
    /// <param name="converters">A list of JSON converters.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="plugin" />, or the <paramref name="contentSettings" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>This will write out the settings for a content plug-in to the same location where the editor stores its application settings file.</remarks>
    public void WriteContentSettings<T>(string name, T contentSettings, params JsonConverter[] converters)
        where T : class
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentEmptyException(nameof(name));
        }

        if (contentSettings is null)
        {
            throw new ArgumentNullException(nameof(contentSettings));
        }

        FileInfo settingsFile = GetContentPlugInSettingsPath(name);
        using Stream stream = settingsFile.Open(FileMode.Create, FileAccess.Write, FileShare.None);
        using StreamWriter writer = new(stream, Encoding.UTF8, 80000, false);

        JsonSerializerOptions options = new();

        if (converters is not null)
        {
            foreach (JsonConverter converter in converters)
            {
                options.Converters.Add(converter);
            }
        }

        writer.Write(JsonSerializer.Serialize(contentSettings, options));
    }

    /// <summary>
    /// Function called when a project is loaded/created.
    /// </summary>
    /// <param name="fileManager">The content file manager for the project.</param>
    /// <param name="temporaryFileSystem">The file system used to hold temporary working data.</param>
    public void ProjectActivated(IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> temporaryFileSystem)
    {
        foreach (ToolPlugIn plugIn in _plugins.Values)
        {
            plugIn.ProjectOpened(fileManager, temporaryFileSystem);
        }

        GetToolButtons();
    }

    /// <summary>
    /// Function called when a project is unloaded.
    /// </summary>        
    public void ProjectDeactivated()
    {
        foreach (ToolPlugIn plugIn in _plugins.Values)
        {
            plugIn.ProjectClosed();
        }

        ClearToolButtons();
    }
}
