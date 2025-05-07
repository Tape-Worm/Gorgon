
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
// Created: October 29, 2018 1:19:30 PM
// 

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Properties;
using Gorgon.IO;
using Gorgon.IO.FileSystem;
using Gorgon.Plugins;

namespace Gorgon.Editor.Services;

/// <summary>
/// The service used for managing the content Plugins
/// </summary>
internal class ContentPluginService
    : IContentPluginService, IDisposable
{

    // The Plugin list.
    private readonly Dictionary<string, ContentPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);
    // The Plugin list.
    private readonly Dictionary<string, ContentImportPlugin> _importers = new(StringComparer.OrdinalIgnoreCase);
    // The list of disabled content plugins.
    private readonly Dictionary<string, IDisabledPlugin> _disabled = new(StringComparer.OrdinalIgnoreCase);
    // The directory that contains the settings for the plugins.
    private readonly string _settingsDir;
    // The services passed from the host to the content plugins.
    private readonly IHostContentServices _hostServices;

    /// <summary>Property to return the list of content Plugins loaded in to the application.</summary>
    /// <value>The Plugins.</value>
    public IReadOnlyDictionary<string, ContentPlugin> Plugins => _plugins;

    /// <summary>
    /// Property to return the list of content importer plugins loaded into the application.
    /// </summary>
    public IReadOnlyDictionary<string, ContentImportPlugin> Importers => _importers;

    /// <summary>Property to return the list of disabled plugins.</summary>
    public IReadOnlyDictionary<string, IDisabledPlugin> DisabledPlugins => _disabled;

    /// <summary>
    /// Property to set or return the currently active content file manager to pass to any plugins.
    /// </summary>
    public IContentFileManager ContentFileManager
    {
        get;
        set;
    }

    /// <summary>
    /// Function to retrieve the actual plugin based on the name associated with the project metadata item.
    /// </summary>
    /// <param name="metadata">The metadata item to evaluate.</param>
    /// <returns>The plugin, and the <see cref="MetadataPluginState"/> used to evaluate whether a deep inspection is required.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/> parameter is <b>null</b>.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public (ContentPlugin Plugin, MetadataPluginState state) GetContentPlugin(ProjectItemMetadata metadata)
    {
        if (metadata is null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        // If the name is null, then we've never assigned the content Plugin.  So look it up.
        if (metadata.PluginName is null)
        {
            return (null, MetadataPluginState.Unassigned);
        }

        if ((string.IsNullOrWhiteSpace(metadata.PluginName))
            || (!Plugins.TryGetValue(metadata.PluginName, out ContentPlugin Plugin)))
        {
            return (null, MetadataPluginState.NotFound);
        }

        return (Plugin, MetadataPluginState.Assigned);
    }

    /// <summary>
    /// Function to return the file for the content plugin settings.
    /// </summary>
    /// <param name="name">The name of the file.</param>
    /// <returns>The file containing the plugin settings.</returns>
    private string GetContentPluginSettingsPath(string name) =>
#if DEBUG
        Path.Combine(_settingsDir, name.FormatFileName()) + ".DEBUG.json";
#else
        Path.Combine(_settingsDir, Path.ChangeExtension(name.FormatFileName(), "json"));
#endif

    /// <summary>
    /// Function to load Plugins for content editors.
    /// </summary>
    /// <param name="plugins">The plugin service to use when loading the Plugins.</param>
    private void LoadContentEditors(IGorgonPluginService plugins)
    {
        IReadOnlyList<ContentPlugin> PluginList = plugins.GetPlugins<ContentPlugin>();

        foreach (ContentPlugin Plugin in PluginList)
        {
            try
            {
                _hostServices.Log.Print($"Creating content plugin '{Plugin.Name}'...", LoggingLevel.Simple);
                Plugin.Initialize(_hostServices);

                // Check to see if this plugin can continue.
                IReadOnlyList<string> validation = Plugin.IsPluginAvailable();

                if (validation.Count > 0)
                {
                    // Shut the plugin down.
                    Plugin.Shutdown();

                    _hostServices.Log.PrintWarning($"The content plugin '{Plugin.Name}' is disabled:", LoggingLevel.Simple);
                    foreach (string reason in validation)
                    {
                        _hostServices.Log.PrintWarning($"{reason}", LoggingLevel.Verbose);
                    }

                    _disabled[Plugin.Name] = new DisabledPlugin(DisabledReasonCode.ValidationError, Plugin.Name, string.Join("\r\n", validation), Plugin.PluginPath);

                    // Remove this plugin.
                    plugins.Unload(Plugin.Name);
                    continue;
                }

                AddContentPlugin(Plugin);
            }
            catch (Exception ex)
            {
                // Attempt to gracefully shut the plugin down if we error out.
                Plugin.Shutdown();

                _hostServices.Log.PrintError($"Cannot create content plugin '{Plugin.Name}'.", LoggingLevel.Simple);
                _hostServices.Log.PrintException(ex);

                _disabled[Plugin.Name] = new DisabledPlugin(DisabledReasonCode.Error, Plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_plugin_EXCEPTION, ex.Message), Plugin.PluginPath);
            }
        }
    }

    /// <summary>
    /// Function to load Plugins for content importers.
    /// </summary>
    /// <param name="plugins">The plugin service to use when loading the Plugins.</param>
    private void LoadImporters(IGorgonPluginService plugins)
    {
        // Before we load, pull in any importers so they'll be initialized and ready for content plugins (if they're needed).
        IReadOnlyList<ContentImportPlugin> importers = plugins.GetPlugins<ContentImportPlugin>();

        foreach (ContentImportPlugin Plugin in importers)
        {
            try
            {
                _hostServices.Log.Print($"Creating content importer plugin '{Plugin.Name}'...", LoggingLevel.Simple);
                Plugin.Initialize(_hostServices);

                // Check to see if this plugin can continue.
                IReadOnlyList<string> validation = Plugin.IsPluginAvailable();

                if (validation.Count > 0)
                {
                    // Shut the plugin down.
                    Plugin.Shutdown();

                    _hostServices.Log.PrintWarning($"The importer plugin '{Plugin.Name}' is disabled:", LoggingLevel.Simple);
                    foreach (string reason in validation)
                    {
                        _hostServices.Log.PrintWarning($"{reason}", LoggingLevel.Verbose);
                    }

                    _disabled[Plugin.Name] = new DisabledPlugin(DisabledReasonCode.ValidationError, Plugin.Name, string.Join("\r\n", validation), Plugin.PluginPath);

                    // Remove this plugin.
                    plugins.Unload(Plugin.Name);
                    continue;
                }

                AddContentImportPlugin(Plugin);
            }
            catch (Exception ex)
            {
                // Attempt to gracefully shut the plugin down if we error out.
                Plugin.Shutdown();

                _hostServices.Log.PrintError($"Cannot create importer plugin '{Plugin.Name}'.", LoggingLevel.Simple);
                _hostServices.Log.PrintException(ex);

                _disabled[Plugin.Name] = new DisabledPlugin(DisabledReasonCode.Error, Plugin.Name, string.Format(Resources.GOREDIT_DISABLE_CONTENT_plugin_EXCEPTION, ex.Message), Plugin.PluginPath);
            }
        }
    }

    /// <summary>Funcion to read the settings for a content plugin from a JSON file.</summary>
    /// <typeparam name="T">The type of settings to read. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="converters">A list of JSON data converters.</param>
    /// <returns>The settings object for the plugin, or <b>null</b> if no settings file was found for the plugin.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="Plugin" /> parameter is <b>null</b>.</exception>
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

        string settingsFile = GetContentPluginSettingsPath(name);

        if (!File.Exists(settingsFile))
        {
            return null;
        }

        using Stream stream = File.Open(settingsFile, FileMode.Open, FileAccess.Read, FileShare.Read);
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

    /// <summary>Function to write out the settings for a content plugin as a JSON file.</summary>
    /// <typeparam name="T">The type of settings to write. Must be a reference type.</typeparam>
    /// <param name="name">The name of the file.</param>
    /// <param name="contentSettings">The content settings to persist as JSON file.</param>
    /// <param name="converters">A list of JSON converters.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="Plugin" />, or the <paramref name="contentSettings" /> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
    /// <remarks>This will write out the settings for a content plugin to the same location where the editor stores its application settings file.</remarks>
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

        string settingsFile = GetContentPluginSettingsPath(name);
        using Stream stream = File.Open(settingsFile, FileMode.Create, FileAccess.Write, FileShare.None);
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

    /// <summary>Function to add a content import Plugin to the service.</summary>
    /// <param name="plugin">The Plugin to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
    public void AddContentImportPlugin(ContentImportPlugin plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        if (_importers.ContainsKey(plugin.Name))
        {
            return;
        }

        _importers[plugin.Name] = plugin;
    }

    /// <summary>Function to add a content Plugin to the service.</summary>
    /// <param name="plugin">The Plugin to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="plugin"/> parameter is <b>null</b>.</exception>
    public void AddContentPlugin(ContentPlugin plugin)
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

    /// <summary>Function to clear all of the content Plugins.</summary>
    public void Clear()
    {

        foreach (KeyValuePair<string, ContentPlugin> plugin in _plugins)
        {
            plugin.Value.Shutdown();
        }

        _plugins.Clear();

        foreach (KeyValuePair<string, ContentImportPlugin> plugin in _importers)
        {
            plugin.Value.Shutdown();
        }

        _importers.Clear();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => Clear();

    /// <summary>
    /// Function to load all of the content plugins into the service.
    /// </summary>
    /// <param name="pluginCache">The plugin assembly cache.</param>
    /// <param name="pluginDir">The directory that contains the plugins.</param>        
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginCache"/>, or the <paramref name="pluginDir"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="pluginDir"/> parameter is empty.</exception>
    public void LoadContentPlugins(GorgonMefPluginCache pluginCache, string pluginDir)
    {
        if (pluginCache is null)
        {
            throw new ArgumentNullException(nameof(pluginCache));
        }

        if (pluginDir is null)
        {
            throw new ArgumentNullException(nameof(pluginDir));
        }

        if (string.IsNullOrWhiteSpace(pluginDir))
        {
            throw new ArgumentEmptyException(nameof(pluginDir));
        }

        IReadOnlyList<string> files = Directory.GetFiles(pluginDir, "*.dll");
        IReadOnlyList<PluginAssemblyState> assemblies = pluginCache.ValidateAndLoadAssemblies(files, _hostServices.Log);

        if (assemblies.Count > 0)
        {
            foreach (PluginAssemblyState record in assemblies.Where(item => !item.IsAssemblyLoaded && item.IsManaged))
            {
                _disabled[Path.GetFileName(record.Path)] = new DisabledPlugin(DisabledReasonCode.Error, Path.GetFileName(record.Path), record.LoadFailureReason, record.Path);
            }
        }

        IGorgonPluginService Plugins = new GorgonMefPluginService(pluginCache);

        LoadImporters(Plugins);
        LoadContentEditors(Plugins);
    }

    /// <summary>
    /// Function to remove a content import Plugin from the service.
    /// </summary>
    /// <param name="plugin">The Plugin to remove.</param>
    public void RemoveContentImportPlugin(ContentImportPlugin plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        if (!_importers.ContainsKey(plugin.Name))
        {
            return;
        }

        _importers.Remove(plugin.Name);
        plugin.Shutdown();
    }

    /// <summary>Function to remove a content Plugin from the service.</summary>
    /// <param name="plugin">The Plugin to remove.</param>
    public void RemoveContentPlugin(ContentPlugin plugin)
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

    /// <summary>
    /// Function to retrieve the appropriate content importer for the file specified.
    /// </summary>
    /// <param name="filePath">The path to the file to evaluate.</param>
    /// <returns>A <see cref="IEditorContentImporter"/>, or <b>null</b> if none was found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// Since the content importers are meant for importing into the project virtual file system, the <paramref name="filePath"/> must point to a file on the physical file system. 
    /// </para>
    /// </remarks>
    public IEditorContentImporter GetContentImporter(string filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        ContentImportPlugin importPlugin = null;

        foreach (KeyValuePair<string, ContentImportPlugin> Plugin in _importers)
        {
            if (Plugin.Value.CanOpenContent(filePath))
            {
                importPlugin = Plugin.Value;
                break;
            }

            continue;
        }

        return importPlugin?.GetImporter();
    }

    /// <summary>
    /// Function called when a project is loaded/created.
    /// </summary>
    /// <param name="projectFileSystem">The read only file system used by the project.</param>
    /// <param name="fileManager">The content file manager for the project.</param>
    /// <param name="temporaryFileSystem">The file system used to hold temporary working data.</param>
    public void ProjectActivated(IGorgonFileSystem projectFileSystem, IContentFileManager fileManager, IGorgonFileSystem temporaryFileSystem)
    {
        foreach (ContentPlugin Plugin in _plugins.Values)
        {
            Plugin.ProjectOpened(fileManager, temporaryFileSystem);
        }

        foreach (ContentImportPlugin Plugin in _importers.Values)
        {
            Plugin.ProjectOpened(projectFileSystem, temporaryFileSystem);
        }
    }

    /// <summary>
    /// Function called when a project is unloaded.
    /// </summary>        
    public void ProjectDeactivated()
    {
        foreach (ContentImportPlugin Plugin in _importers.Values)
        {
            Plugin.ProjectClosed();
        }

        foreach (ContentPlugin Plugin in _plugins.Values)
        {
            Plugin.ProjectClosed();
        }
    }

    /// <summary>Initializes a new instance of the ContentPluginService class.</summary>
    /// <param name="settingsDirectory">The directory that will contain settings for the content plugins.</param>
    /// <param name="hostServices">The services to pass from the host application to the content plugins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="settingsDirectory"/>, or the <paramref name="hostServices"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="settingsDirectory"/> parameter is empty.</exception>
    /// <example
    public ContentPluginService(string settingsDirectory, IHostContentServices hostServices)
    {
        _settingsDir = settingsDirectory ?? throw new ArgumentNullException(nameof(settingsDirectory));

        if (string.IsNullOrWhiteSpace(settingsDirectory))
        {
            throw new ArgumentEmptyException(nameof(settingsDirectory));
        }

        _hostServices = hostServices ?? throw new ArgumentNullException(nameof(hostServices));
    }
}
